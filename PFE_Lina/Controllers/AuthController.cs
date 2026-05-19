using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.DTOs;
using SmartDelivery.Donnees.Context;

namespace SmartDelivery.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UtilisateurSmartDelivery>  _userManager;
        private readonly RoleManager<IdentityRole>              _roleManager;
        private readonly IConfiguration                         _config;
        private readonly ApplicationDbContext                   _contexte;

        public AuthController(
            UserManager<UtilisateurSmartDelivery> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config,
            ApplicationDbContext contexte)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config      = config;
            _contexte    = contexte;
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
                UserName     = dto.Email,
                Email        = dto.Email,
                Nom          = dto.Nom,
                Prenom       = dto.Prenom,
                ChauffeurId  = dto.ChauffeurId,
                DispatcherId = dto.DispatcherId
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

        // POST /api/auth/register-public
        // Auto-inscription : crée automatiquement l'entité Chauffeur ou Dispatcher liée au compte
        [HttpPost("register-public")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterPublic([FromBody] RegisterDto dto)
        {
            var rolesAutorises = new[] { Roles.Dispatcher, Roles.Chauffeur };
            if (!rolesAutorises.Contains(dto.Role))
                return BadRequest(new { message = "L'auto-inscription est réservée aux rôles Dispatcher et Chauffeur." });

            int? chauffeurId  = null;
            int? dispatcherId = null;

            // ── Créer automatiquement l'entité métier correspondante ────────────
            if (dto.Role == Roles.Chauffeur)
            {
                var chauffeur = new Chauffeur
                {
                    Nom           = dto.Nom,
                    Prenom        = dto.Prenom,
                    Email         = dto.Email,
                    Telephone     = "",
                    NumeroPermis  = $"PERM-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    DateEmbauche  = DateTime.UtcNow,
                    EstDisponible = true
                };
                _contexte.Chauffeurs.Add(chauffeur);
                await _contexte.SaveChangesAsync();
                chauffeurId = chauffeur.Id;
            }
            else if (dto.Role == Roles.Dispatcher)
            {
                var dispatcher = new Dispatcher
                {
                    Nom                  = dto.Nom,
                    Prenom               = dto.Prenom,
                    Email                = dto.Email,
                    DatePriseEnCharge    = DateTime.UtcNow
                };
                _contexte.Dispatchers.Add(dispatcher);
                await _contexte.SaveChangesAsync();
                dispatcherId = dispatcher.Id;
            }

            var user = new UtilisateurSmartDelivery
            {
                UserName     = dto.Email,
                Email        = dto.Email,
                Nom          = dto.Nom,
                Prenom       = dto.Prenom,
                ChauffeurId  = chauffeurId,
                DispatcherId = dispatcherId
            };

            var result = await _userManager.CreateAsync(user, dto.MotDePasse);
            if (!result.Succeeded)
            {
                // Nettoyer l'entité créée si le compte Identity échoue
                if (chauffeurId.HasValue)
                {
                    var ch = await _contexte.Chauffeurs.FindAsync(chauffeurId.Value);
                    if (ch != null) _contexte.Chauffeurs.Remove(ch);
                    await _contexte.SaveChangesAsync();
                }
                if (dispatcherId.HasValue)
                {
                    var disp = await _contexte.Dispatchers.FindAsync(dispatcherId.Value);
                    if (disp != null) _contexte.Dispatchers.Remove(disp);
                    await _contexte.SaveChangesAsync();
                }
                return BadRequest(ResultatOperation<object>.EchecValidation(
                    result.Errors.Select(e => e.Description).ToList()));
            }

            await _userManager.AddToRoleAsync(user, dto.Role);

            // Retourner un token directement pour redirection immédiate
            var roles      = await _userManager.GetRolesAsync(user);
            var role       = roles.FirstOrDefault() ?? dto.Role;
            var token      = GenererToken(user, role);
            var expiration = DateTime.UtcNow.AddHours(int.Parse(_config["Jwt:ExpirationHours"]!));

            var tokenDto = new TokenResponseDto(
                Token:        new JwtSecurityTokenHandler().WriteToken(token),
                Email:        user.Email!,
                Nom:          user.Nom,
                Prenom:       user.Prenom,
                Role:         role,
                Expiration:   expiration,
                ChauffeurId:  chauffeurId,
                DispatcherId: dispatcherId);

            return Ok(ResultatOperation<TokenResponseDto>.Ok(tokenDto,
                $"Compte créé avec succès. Bienvenue {dto.Prenom} !"));
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
                Token:        new JwtSecurityTokenHandler().WriteToken(token),
                Email:        user.Email!,
                Nom:          user.Nom,
                Prenom:       user.Prenom,
                Role:         role,
                Expiration:   expiration,
                ChauffeurId:  user.ChauffeurId,
                DispatcherId: user.DispatcherId);

            return Ok(ResultatOperation<TokenResponseDto>.Ok(tokenDto, "Connexion réussie."));
        }

        // GET /api/auth/utilisateurs — Admin : liste tous les utilisateurs avec leurs rôles
        [HttpGet("utilisateurs")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ObtenirUtilisateurs()
        {
            var users  = await _userManager.Users.AsNoTracking().ToListAsync();
            var result = new List<UtilisateurDto>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UtilisateurDto(
                    Id:          u.Id,
                    Email:       u.Email ?? "",
                    Nom:         u.Nom,
                    Prenom:      u.Prenom,
                    Role:        roles.FirstOrDefault() ?? "",
                    ChauffeurId:  u.ChauffeurId,
                    DispatcherId: u.DispatcherId));
            }

            return Ok(ResultatOperation<IEnumerable<UtilisateurDto>>.Ok(result));
        }

        // DELETE /api/auth/utilisateurs/{id} — Admin : supprime un utilisateur
        [HttpDelete("utilisateurs/{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> SupprimerUtilisateur(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(ResultatOperation<object>.NonTrouve("Utilisateur introuvable."));

            var currentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == currentId)
                return BadRequest(ResultatOperation<object>.Echec("Vous ne pouvez pas supprimer votre propre compte."));

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(ResultatOperation<object>.EchecValidation(
                    result.Errors.Select(e => e.Description).ToList()));

            return Ok(ResultatOperation<object>.Vide("Utilisateur supprimé."));
        }

        // PUT /api/auth/utilisateurs/{userId}/lier-chauffeur/{chauffeurId}
        [HttpPut("utilisateurs/{userId}/lier-chauffeur/{chauffeurId:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> LierChauffeur(string userId, int chauffeurId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ResultatOperation<object>.NonTrouve("Utilisateur introuvable."));

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(Roles.Chauffeur))
                return BadRequest(ResultatOperation<object>.Echec(
                    "Seuls les comptes de rôle Chauffeur peuvent être liés à une entité chauffeur."));

            user.ChauffeurId = chauffeurId;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(ResultatOperation<object>.EchecValidation(
                    result.Errors.Select(e => e.Description).ToList()));

            return Ok(ResultatOperation<object>.Vide($"Compte lié au chauffeur #{chauffeurId}."));
        }

        // PUT /api/auth/utilisateurs/{id}/role — Admin : change le rôle d'un utilisateur
        [HttpPut("utilisateurs/{id}/role")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ChangerRole(string id, [FromBody] ChangerRoleDto dto)
        {
            if (!Roles.Tous.Contains(dto.NouveauRole))
                return BadRequest(ResultatOperation<object>.Echec(
                    $"Rôle invalide. Valeurs acceptées : {string.Join(", ", Roles.Tous)}"));

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(ResultatOperation<object>.NonTrouve("Utilisateur introuvable."));

            var rolesActuels = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, rolesActuels);
            await _userManager.AddToRoleAsync(user, dto.NouveauRole);

            return Ok(ResultatOperation<object>.Vide($"Rôle mis à jour : {dto.NouveauRole}."));
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
                Role         = roles.FirstOrDefault(),
                user.ChauffeurId,
                user.DispatcherId
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
                new Claim("ChauffeurId",             user.ChauffeurId?.ToString()  ?? ""),
                new Claim("DispatcherId",            user.DispatcherId?.ToString() ?? "")
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
