using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.Enums;

namespace SmartDelivery.Donnees.Context
{
    public class ApplicationDbContext : IdentityDbContext<UtilisateurSmartDelivery>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Livraison>   Livraisons   { get; set; }
        public DbSet<Camion>      Camions      { get; set; }
        public DbSet<Chauffeur>   Chauffeurs   { get; set; }
        public DbSet<Dispatcher>  Dispatchers  { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<PointTracking> PointsTracking { get; set; }
        public DbSet<Anomalie>    Anomalies    { get; set; }
        public DbSet<Reclamation> Reclamations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Livraison ────────────────────────────────────────────────────
            modelBuilder.Entity<Livraison>(entite =>
            {
                entite.HasKey(l => l.Id);
                entite.Property(l => l.Reference).IsRequired().HasMaxLength(50);
                entite.Property(l => l.Cout).HasColumnType("decimal(18,2)");
                entite.Property(l => l.Statut)
                      .HasConversion<string>()
                      .HasDefaultValue(StatutLivraison.EnAttente);

                // Produit inline
                entite.Property(l => l.NomProduit).IsRequired().HasMaxLength(150);
                entite.Property(l => l.DescriptionProduit).HasMaxLength(500);
                entite.Property(l => l.PoidsKg).HasColumnType("decimal(10,3)");
                entite.Property(l => l.VolumeM3).HasColumnType("decimal(10,3)");
                entite.Property(l => l.PrixUnitaire).HasColumnType("decimal(18,2)");

                entite.HasOne(l => l.Camion)
                      .WithMany(c => c.Livraisons)
                      .HasForeignKey(l => l.CamionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entite.HasOne(l => l.Destination)
                      .WithMany(d => d.Livraisons)
                      .HasForeignKey(l => l.DestinationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entite.HasOne(l => l.Dispatcher)
                      .WithMany(d => d.Livraisons)
                      .HasForeignKey(l => l.DispatcheurId)
                      .OnDelete(DeleteBehavior.SetNull);

                entite.Navigation(l => l.Camion).AutoInclude();
                entite.Navigation(l => l.Destination).AutoInclude();

                entite.HasIndex(l => l.Statut).HasDatabaseName("IX_Livraisons_Statut");
                entite.HasIndex(l => l.DateCreation).HasDatabaseName("IX_Livraisons_DateCreation");
                entite.HasIndex(l => new { l.Statut, l.DateLivraisonPrevue })
                      .HasDatabaseName("IX_Livraisons_Statut_DatePrevue");
                entite.HasIndex(l => l.CamionId).HasDatabaseName("IX_Livraisons_CamionId");
                entite.HasIndex(l => l.DispatcheurId).HasDatabaseName("IX_Livraisons_DispatcheurId");
            });

            // ── Dispatcher ───────────────────────────────────────────────────
            modelBuilder.Entity<Dispatcher>(entite =>
            {
                entite.HasKey(d => d.Id);
                entite.Property(d => d.Nom).IsRequired().HasMaxLength(100);
                entite.Property(d => d.Prenom).IsRequired().HasMaxLength(100);
                entite.Property(d => d.Telephone).HasMaxLength(20);
                entite.Property(d => d.ZoneResponsabilite).HasMaxLength(200);
            });

            // ── Camion ───────────────────────────────────────────────────────
            modelBuilder.Entity<Camion>(entite =>
            {
                entite.HasKey(c => c.Id);
                entite.Property(c => c.Immatriculation).IsRequired().HasMaxLength(20);
                entite.HasIndex(c => c.Immatriculation).IsUnique();
                entite.Property(c => c.Statut)
                      .HasConversion<string>()
                      .HasDefaultValue(StatutCamion.Disponible);

                entite.HasOne(c => c.Chauffeur)
                      .WithMany(ch => ch.Camions)
                      .HasForeignKey(c => c.ChauffeurId)
                      .OnDelete(DeleteBehavior.SetNull);

                entite.Navigation(c => c.Chauffeur).AutoInclude();

                entite.HasIndex(c => c.Statut).HasDatabaseName("IX_Camions_Statut");
                entite.HasIndex(c => c.ChauffeurId).HasDatabaseName("IX_Camions_ChauffeurId");
            });

            // ── Chauffeur ────────────────────────────────────────────────────
            modelBuilder.Entity<Chauffeur>(entite =>
            {
                entite.HasKey(c => c.Id);
                entite.Property(c => c.Nom).IsRequired().HasMaxLength(100);
                entite.Property(c => c.Prenom).IsRequired().HasMaxLength(100);
                entite.Property(c => c.NumeroPermis).IsRequired().HasMaxLength(50);
                entite.HasIndex(c => c.NumeroPermis).IsUnique();
                entite.Property(c => c.Telephone).HasMaxLength(20);
            });

            // ── Destination ──────────────────────────────────────────────────
            modelBuilder.Entity<Destination>(entite =>
            {
                entite.HasKey(d => d.Id);
                entite.Property(d => d.Adresse).IsRequired().HasMaxLength(200);
                entite.Property(d => d.Ville).IsRequired().HasMaxLength(100);
                entite.Property(d => d.CodePostal).HasMaxLength(10);
            });

            // ── PointTracking ────────────────────────────────────────────────
            modelBuilder.Entity<PointTracking>(entite =>
            {
                entite.HasKey(pt => pt.Id);
                entite.HasOne(pt => pt.Livraison)
                      .WithMany(l => l.PointsTracking)
                      .HasForeignKey(pt => pt.LivraisonId)
                      .OnDelete(DeleteBehavior.Cascade);

                entite.HasIndex(pt => new { pt.LivraisonId, pt.Horodatage })
                      .HasDatabaseName("IX_PointsTracking_LivraisonId_Horodatage");
            });

            // ── Anomalie ─────────────────────────────────────────────────────
            modelBuilder.Entity<Anomalie>(entite =>
            {
                entite.HasKey(a => a.Id);
                entite.Property(a => a.Type).HasConversion<string>();
                entite.Property(a => a.Description).IsRequired().HasMaxLength(500);

                entite.HasOne(a => a.Livraison)
                      .WithMany(l => l.Anomalies)
                      .HasForeignKey(a => a.LivraisonId)
                      .OnDelete(DeleteBehavior.Cascade);

                entite.HasIndex(a => new { a.LivraisonId, a.EstResolue })
                      .HasDatabaseName("IX_Anomalies_LivraisonId_EstResolue");
            });

            // ── Reclamation ───────────────────────────────────────────────────
            modelBuilder.Entity<Reclamation>(entite =>
            {
                entite.HasKey(r => r.Id);
                entite.Property(r => r.Titre).IsRequired().HasMaxLength(200);
                entite.Property(r => r.Description).IsRequired().HasMaxLength(2000);
                entite.Property(r => r.ReponseAdmin).HasMaxLength(2000);
                entite.Property(r => r.Statut)
                      .HasConversion<string>()
                      .HasDefaultValue(SmartDelivery.Domaine.Models.Enums.StatutReclamation.EnAttente);

                entite.HasOne(r => r.Utilisateur)
                      .WithMany()
                      .HasForeignKey(r => r.UtilisateurId)
                      .OnDelete(DeleteBehavior.Restrict);

                entite.HasOne(r => r.Livraison)
                      .WithMany()
                      .HasForeignKey(r => r.LivraisonId)
                      .OnDelete(DeleteBehavior.SetNull);

                entite.HasIndex(r => r.UtilisateurId).HasDatabaseName("IX_Reclamations_UtilisateurId");
                entite.HasIndex(r => r.Statut).HasDatabaseName("IX_Reclamations_Statut");
            });
        }
    }
}
