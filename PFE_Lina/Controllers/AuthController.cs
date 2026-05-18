using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.DTOs;

namespace SmartDelivery.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UtilisateurSmartDelivery>  _userManager;
        private readonly RoleManager<IdentityRole>              _roleManager;
        private readonly IConfiguration                         _config;

        public AuthController(
            UserManager<UtilisateurSmartDelivery> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config      = config;
        }

        // POST /api/auth/register  (Admin uniquement — crée n'importe quel rôle)
        [HttpPost("register")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!Roles.Tous.Contains(dto.Role))
                return BadRequest(new { message = $"Rôle invalide. Valeurs acceptées : {string.Join(", ", Roles.Tous)}" });

            var user = new UtilisateurSmartDelivery
            {
                UserName    = dto.Email,
                Email       = dto.Email,
                Nom         = dto.Nom,
                Prenom      = dto.Prenom,
                ChauffeurId = dto.ChauffeurId
            };

            var result = await _userManager.CreateAsync(user, dto.MotDePasse);
            if (!result.Succeeded)
                return BadRequest(ResultatOperation<object>.EchecValidation(
                    result.Errors.Select(e => e.Description).ToList()));

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));

            await _userManager.AddToRoleAsync(user, dto.Role);

            return Ok(ResultatOperation<object>.Vide($"Utilisateur {dto.Email} créé avec le rôle {dto.Role}."));
        }

        // POST /api/auth/register-public  (Auto-inscription — Dispatcher ou Chauffeur uniquement)
        [HttpPost("register-public")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterPublic([FromBody] RegisterDto dto)
        {
            var rolesAutorises = new[] { Roles.Dispatcher, Roles.Chauffeur };
            if (!rolesAutorises.Contains(dto.Role))
                return BadRequest(new { message = "L'auto-inscription est réservée aux rôles Dispatcher et Chauffeur." });

            var user = new UtilisateurSmartDelivery
            {
                UserName    = dto.Email,
                Email       = dto.Email,
                Nom         = dto.Nom,
                Prenom      = dto.Prenom,
                ChauffeurId = dto.ChauffeurId
            };

            var result = await _userManager.CreateAsync(user, dto.MotDePasse);
            if (!result.Succeeded)
                return BadRequest(ResultatOperation<object>.EchecValidation(
                    result.Errors.Select(e => e.Description).ToList()));

            await _userManager.AddToRoleAsync(user, dto.Role);

            return Ok(ResultatOperation<object>.Vide("Compte créé avec succès. Vous pouvez maintenant vous connecter."));
        }

        // POST /api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.MotDePasse))
                return Unauthorized(ResultatOperation<TokenResponseDto>.Echec("Email ou mot de passe incorrect."));

            var roles = await _userManager.GetRolesAsync(user);
            var role  = roles.FirstOrDefault() ?? Roles.Dispatcher;

            var token      = GenererToken(user, role);
            var expiration = DateTime.UtcNow.AddHours(int.Parse(_config["Jwt:ExpirationHours"]!));

            var tokenDto = new TokenResponseDto(
                Token:      new JwtSecurityTokenHandler().WriteToken(token),
                Email:      user.Email!,
                Nom:        user.Nom,
                Prenom:     user.Prenom,
                Role:       role,
                Expiration: expiration);

            return Ok(ResultatOperation<TokenResponseDto>.Ok(tokenDto, "Connexion réussie."));
        }

        // GET /api/auth/me
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var user = await _userManager.FindByEmailAsync(User.FindFirstValue(ClaimTypes.Email)!);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new
            {
                user.Email,
                user.Nom,
                user.Prenom,
                Role        = roles.FirstOrDefault(),
                user.ChauffeurId
            });
        }

        // ── Utilitaire ──────────────────────────────────────────────────────
        private JwtSecurityToken GenererToken(UtilisateurSmartDelivery user, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email,          user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name,           $"{user.Prenom} {user.Nom}"),
                new Claim(ClaimTypes.Role,           role),
                new Claim("ChauffeurId",             user.ChauffeurId?.ToString() ?? "")
            };

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer:   _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims:   claims,
                expires:  DateTime.UtcNow.AddHours(int.Parse(_config["Jwt:ExpirationHours"]!)),
                signingCredentials: creds);
        }
    }
}
