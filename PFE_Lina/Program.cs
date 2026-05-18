using Microsoft.AspNetCore.Identity;
using SmartDelivery.API.Middleware;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Donnees.Context;
using SmartDelivery.Infrastructure;
using SmartDelivery.Infrastructure.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    // Validation automatique FluentValidation sur tous les endpoints
    options.Filters.Add<SmartDelivery.API.Middleware.FiltreValidationAutomatique>();
})
.AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SmartDelivery API", Version = "v1",
        Description = "POST /api/auth/login → JWT token. Utiliser 'Bearer {token}' dans Authorization." });
});

// MediatR — scan des Commands/Queries (Domaine) + Handlers (Infrastructure)
builder.Services.AddMediatR(cfg =>
{
    // Commands et Queries restent dans Domaine
    cfg.RegisterServicesFromAssembly(typeof(SmartDelivery.Domaine.Interface.IGenericRepository<>).Assembly);
    // Tous les handlers sont désormais dans Infrastructure
    cfg.RegisterServicesFromAssembly(typeof(SmartDelivery.Infrastructure.DependancesInfrastructure).Assembly);
});

// Infrastructure (DbContext + Identity + JWT + SignalR + Repositories + Services)
builder.Services.AjouterServicesInfrastructure(builder.Configuration);

// ── Authorization Policies par rôle ───────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminSeulement",
        policy => policy.RequireRole("Admin"));

    options.AddPolicy("DispatcherOuAdmin",
        policy => policy.RequireRole("Admin", "Dispatcher"));

    options.AddPolicy("TousLesRoles",
        policy => policy.RequireRole("Admin", "Dispatcher", "Chauffeur"));

    options.AddPolicy("GestionLivraisons",
        policy => policy.RequireRole("Admin", "Dispatcher"));

    options.AddPolicy("AccesTableauDeBord",
        policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddCors(options =>
    options.AddPolicy("ToutAutoriser", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials())); // AllowCredentials requis pour SignalR WebSocket

// ── Pipeline HTTP ──────────────────────────────────────────────────────────────
var app = builder.Build();

// Seed des rôles et de l'admin par défaut
await SeedAsync(app);

// Doit être le premier middleware — capture toutes les exceptions du pipeline
app.UseMiddleware<GestionnaireExceptionsMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartDelivery API v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseCors("ToutAutoriser");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Points d'entrée SignalR
app.MapHub<TrackingHub>("/hubs/tracking");
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();

// ── Seed admin par défaut ─────────────────────────────────────────────────────
static async Task SeedAsync(WebApplication app)
{
    using var scope       = app.Services.CreateScope();
    var roleManager       = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager       = scope.ServiceProvider.GetRequiredService<UserManager<UtilisateurSmartDelivery>>();
    var config            = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    foreach (var role in Roles.Tous)
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    var adminEmail    = config["AdminSeed:Email"]
        ?? throw new InvalidOperationException("AdminSeed:Email non configuré.");
    var adminPassword = config["AdminSeed:MotDePasse"]
        ?? throw new InvalidOperationException("AdminSeed:MotDePasse non configuré.");
    var adminNom      = config["AdminSeed:Nom"]    ?? "Saadi";
    var adminPrenom   = config["AdminSeed:Prenom"] ?? "Admin";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new UtilisateurSmartDelivery
        {
            UserName       = adminEmail,
            Email          = adminEmail,
            EmailConfirmed = true,
            Nom            = adminNom,
            Prenom         = adminPrenom
        };
        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        else
            Console.WriteLine("[SEED ERREUR] " + string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
