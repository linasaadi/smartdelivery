using System.Text;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SmartDelivery.Domaine.Commands;
using SmartDelivery.Domaine.Interface;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Queries;
using SmartDelivery.Donnees.Context;
using SmartDelivery.Donnees.Repositories;
using SmartDelivery.Infrastructure.Handlers.Camions;
using SmartDelivery.Infrastructure.Handlers.Generiques;
using SmartDelivery.Infrastructure.Handlers.Livraisons;
using SmartDelivery.Infrastructure.Handlers.Tracking;
using SmartDelivery.Infrastructure.Services;

namespace SmartDelivery.Infrastructure
{
    public static class DependancesInfrastructure
    {
        public static IServiceCollection AjouterServicesInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── Base de données SQL Server ───────────────────────────────────
            var chaineConnexion = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(chaineConnexion))
                throw new InvalidOperationException(
                    "La chaîne de connexion 'DefaultConnection' est manquante. " +
                    "Vérifiez appsettings.Development.json ou les variables d'environnement.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(chaineConnexion));

            // ── Identity ─────────────────────────────────────────────────────
            services.AddIdentity<UtilisateurSmartDelivery, IdentityRole>(options =>
            {
                options.Password.RequireDigit           = true;
                options.Password.RequiredLength         = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase       = true;
                options.Password.RequireLowercase       = true;
                options.User.RequireUniqueEmail         = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ── JWT ──────────────────────────────────────────────────────────
            var jwtKey = configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException(
                    "La clé JWT 'Jwt:Key' est manquante. " +
                    "Définissez-la dans appsettings.Development.json ou la variable d'environnement Jwt__Key.");
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = configuration["Jwt:Issuer"],
                    ValidAudience            = configuration["Jwt:Audience"],
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
                };
                // SignalR passe le token en query string
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var token = ctx.Request.Query["access_token"];
                        var path  = ctx.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/hubs"))
                            ctx.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();

            // ── SignalR ──────────────────────────────────────────────────────
            services.AddSignalR();

            // ── Repositories génériques ──────────────────────────────────────
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // ── Cache mémoire (KPI dashboard, listes de référence) ───────────
            services.AddMemoryCache();

            // ── AutoMapper — profils dans Infrastructure.Mappings ────────────
            services.AddAutoMapper(typeof(DependancesInfrastructure).Assembly);

            // ── FluentValidation — validators dans Infrastructure.Validators ─
            services.AddValidatorsFromAssembly(typeof(DependancesInfrastructure).Assembly);

            // ── Services métier ──────────────────────────────────────────────
            services.AddScoped<IOptimisationRouteService, OptimisationRouteService>();
            services.AddScoped<IDetectionAnomalieService, DetectionAnomalieService>();

            // ── Handlers génériques pour chaque entité ───────────────────────
            EnregistrerHandlers<Livraison>(services);
            EnregistrerHandlers<Camion>(services);
            EnregistrerHandlers<Chauffeur>(services);
            EnregistrerHandlers<Destination>(services);
            EnregistrerHandlers<Produit>(services);
            EnregistrerHandlers<PointTracking>(services);
            EnregistrerHandlers<Anomalie>(services);

            // ── Handlers spécifiques (tous dans Infrastructure.Handlers) ────
            services.AddScoped<IRequestHandler<ModifierStatutLivraisonCommand, bool>,
                ModifierStatutLivraisonHandler>();
            services.AddScoped<IRequestHandler<AjouterPointTrackingCommand, bool>,
                AjouterPointTrackingHandler>();
            services.AddScoped<IRequestHandler<GetLivraisonsEnRetardQuery, IEnumerable<Livraison>>,
                GetLivraisonsEnRetardHandler>();
            services.AddScoped<IRequestHandler<GetCamionsDisponiblesQuery, IEnumerable<Camion>>,
                GetCamionsDisponiblesHandler>();
            services.AddScoped<IRequestHandler<GetHistoriqueTrackingQuery, IEnumerable<PointTracking>>,
                GetHistoriqueTrackingHandler>();

            return services;
        }

        private static void EnregistrerHandlers<T>(IServiceCollection services) where T : class
        {
            // Handlers génériques consolidés dans Infrastructure.Handlers.Generiques
            services.AddScoped<IRequestHandler<AddGenericCommand<T>, T>,
                AddGenericHandler<T>>();
            services.AddScoped<IRequestHandler<PutGenericCommand<T>, T>,
                PutGenericHandler<T>>();
            services.AddScoped<IRequestHandler<DeleteGenericCommand<T>, bool>,
                DeleteGenericHandler<T>>();
            services.AddScoped<IRequestHandler<GetGenericQuery<T>, T?>,
                GetGenericHandler<T>>();
            services.AddScoped<IRequestHandler<GetListGenericQuery<T>, IEnumerable<T>>,
                GetListGenericHandler<T>>();
        }
    }
}
