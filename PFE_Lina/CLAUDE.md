# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Projet

**SmartDelivery** — Système de gestion de livraisons en temps réel.  
Toute la codebase est **en français** : noms de classes, méthodes, variables, messages d'erreur, commentaires.

---

## Commandes essentielles

### Backend (.NET 10)

```bash
# Depuis la racine PFE_Lina/
dotnet build                                      # Compiler la solution complète
dotnet run --project PFE_Lina                     # Démarrer l'API (http://localhost:5XXX + Swagger à /)

# Migrations EF Core (toujours depuis PFE_Lina/)
dotnet ef migrations add NomMigration --project SmartDelivery.Donnees --startup-project PFE_Lina
dotnet ef database update --project SmartDelivery.Donnees --startup-project PFE_Lina
dotnet ef migrations remove --project SmartDelivery.Donnees --startup-project PFE_Lina
```

### Frontend (Angular 21)

```bash
# Depuis PFE_Lina_Frontend/
npm install
ng serve          # http://localhost:4200
ng build          # Build production
```

---

## Configuration requise au démarrage

L'API **refuse de démarrer** si ces clés sont absentes — elles ne doivent PAS être dans `appsettings.json` (vide par design) :

**`appsettings.Development.json`** (ignoré par git via `.gitignore`) :
```json
{
  "ConnectionStrings": { "DefaultConnection": "Server=localhost,1433;Database=LinaDB;..." },
  "Jwt": { "Key": "clé-minimum-32-caractères" },
  "AdminSeed": { "Email": "...", "MotDePasse": "..." }
}
```

Pour Docker/production : variables d'environnement `ConnectionStrings__DefaultConnection`, `Jwt__Key`, `AdminSeed__MotDePasse`.

---

## Architecture — 4 projets .NET

```
SmartDelivery.Domaine/       ← Entités, Enums, DTOs, Interfaces, Commands, Queries
SmartDelivery.Donnees/       ← ApplicationDbContext, GenericRepository, Migrations
SmartDelivery.Infrastructure/← Handlers CQRS, Services métier, Hubs SignalR, AutoMapper, Validators
PFE_Lina/ (API)              ← Controllers, Middleware, Program.cs
```

**Règle de dépendance :** `API → Infrastructure → Donnees → Domaine`. Le Domaine n'importe rien des autres projets.

---

## Flux CQRS (MediatR)

Toute opération métier passe par MediatR, jamais directement via repository depuis un controller.

```
Controller → IMediator.Send(Command/Query) → Handler (dans Infrastructure/Handlers/)
```

- **Commands** : `SmartDelivery.Domaine/Commands/` — modifient l'état
- **Queries** : `SmartDelivery.Domaine/Queries/` — lecture seule
- **Handlers** : `SmartDelivery.Infrastructure/Handlers/{Livraisons,Camions,Tracking,Dashboard,Generiques}/`

Les handlers génériques (`AddGenericHandler<T>`, `GetListGenericHandler<T>`, etc.) couvrent le CRUD basique. Pour toute logique métier non triviale, créer un handler spécifique dans le sous-dossier approprié.

---

## Enregistrement MediatR

MediatR scanne deux assemblies au démarrage (`Program.cs`) :
- `SmartDelivery.Domaine` — pour découvrir les Commands/Queries
- `SmartDelivery.Infrastructure` — pour découvrir tous les Handlers

Les handlers sont **aussi** enregistrés manuellement dans `DependancesInfrastructure.cs` pour les types génériques ouverts (`AddGenericHandler<T>`). Ne pas déplacer de handlers vers Domaine ou Donnees — cela casse la découverte.

---

## Format de réponse API

Tous les endpoints retournent `ResultatOperation<T>` (`SmartDelivery.Domaine/Models/ResultatOperation.cs`) :
```json
{ "succes": true, "donnees": {...}, "message": "...", "erreurs": [] }
```
Utiliser les factory methods : `.Ok(donnees)`, `.Echec("message")`, `.NonTrouve()`, `.EchecValidation(liste)`.

---

## Validation

`FiltreValidationAutomatique` (enregistré globalement dans `AddControllers`) intercepte automatiquement tous les DTOs entrants et appelle le validator FluentValidation correspondant s'il existe. Les validators sont dans `Infrastructure/Validators/` et enregistrés via `AddValidatorsFromAssembly`.

---

## Rôles et autorisations

Trois rôles : `Admin`, `Dispatcher`, `Chauffeur` (constantes dans `Roles` — `UtilisateurSmartDelivery.cs`).

| Rôle | Périmètre principal |
|------|---------------------|
| Admin | Dashboard KPI (`/api/dashboard/admin`), suppression, gestion utilisateurs |
| Dispatcher | Créer livraisons, assigner camions/chauffeurs, dashboard opérationnel |
| Chauffeur | Envoyer positions GPS (SignalR), accepter/rejeter livraisons, signaler anomalies |

`UtilisateurSmartDelivery.ChauffeurId` lie un compte Identity à l'entité `Chauffeur` (nullable — seuls les comptes Chauffeur l'ont).

---

## SignalR

Deux hubs, endpoint `/hubs/*`, token JWT passé en query string `?access_token=` :

- **`TrackingHub`** (`/hubs/tracking`) : méthode `EnvoyerPosition(livraisonId, lat, lng, vitesse)` — réservée `[Authorize(Roles="Chauffeur")]`, persiste en base via MediatR ET diffuse au groupe `livraison-{id}` + groupe `Dispatcher`.
- **`NotificationHub`** (`/hubs/notifications`) : `OnConnectedAsync` ajoute l'utilisateur dans son groupe de rôle + groupe `user-{userId}` + groupe `tous`. Méthode `SignalerAnomalie` pour les chauffeurs.

---

## Entités et relations clés

- `Livraison` → `Camion` (FK `CamionId`, DeleteBehavior.Restrict) — un camion peut avoir plusieurs livraisons
- `Camion` → `Chauffeur` (FK `ChauffeurId` nullable, DeleteBehavior.SetNull) — un chauffeur peut avoir plusieurs camions
- `Livraison` → `Destination` (FK, Restrict)
- `Livraison` ↔ `Produit` via `LivraisonProduit` (clé composite, Cascade sur Livraison)
- `Livraison` → `PointTracking[]` (Cascade) — **ne jamais charger avec AutoInclude**, charger séparément
- `Livraison` → `Anomalie[]` (Cascade)

`Camion` et `Destination` sont configurés avec `.AutoInclude()` dans le DbContext — ils se chargent automatiquement dans toutes les requêtes `Livraison`.

---

## Cache

`IMemoryCache` est injecté dans les handlers de dashboard :
- `GetKpiAdminHandler` : cache `"kpi_admin"` pendant 5 min (sliding 2 min)
- `GetKpiDispatcherHandler` : cache `"kpi_dispatcher"` pendant 2 min

Invalider manuellement si besoin : `_cache.Remove("kpi_admin")`.

---

## AutoMapper

Profils dans `Infrastructure/Mappings/` : `ProfilLivraison`, `ProfilCamion`, `ProfilChauffeur`, `ProfilDivers`.  
Enregistrés via `services.AddAutoMapper(typeof(DependancesInfrastructure).Assembly)` — pas besoin d'enregistrement manuel des profils individuels.

---

## Services métier

- **`OptimisationRouteService`** : Haversine pour distance, algorithme du plus proche voisin (Greedy TSP) pour multi-arrêts, facteur trafic heuristique par heure. Pas de vraie carte routière — calcul à vol d'oiseau.
- **`DetectionAnomalieService`** : détecte retard (> 30 min), ETA dépassée, vitesse anormale (> 120 km/h ou < 5 km/h). Zone urbaine Tunisie estimée par coordonnées.

---

## Conventions de nommage

- **Méthodes** : verbe français (`ObtenirTous`, `Creer`, `Modifier`, `Supprimer`)
- **Handlers** : suffixe `Handler`, préfixe action+entité (`GetLivraisonsEnRetardHandler`)
- **Commands/Queries** : suffixe `Command`/`Query` (`ModifierStatutLivraisonCommand`)
- **DTOs** : suffixe `Dto` ou `FormDto` pour la saisie, `ResumeDto` pour les listes
- **Variables** : `_contexte`, `_mediateur`, `_mapper` (accents tolérés dans les identifiants C#)
