using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartDelivery.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactoringDispatcherProduitInline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LivraisonProduits");

            migrationBuilder.DropTable(
                name: "Produits");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionProduit",
                table: "Livraisons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DispatcheurId",
                table: "Livraisons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomProduit",
                table: "Livraisons",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PoidsKg",
                table: "Livraisons",
                type: "decimal(10,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrixUnitaire",
                table: "Livraisons",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Quantite",
                table: "Livraisons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "VolumeM3",
                table: "Livraisons",
                type: "decimal(10,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DispatcherId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Dispatchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatePriseEnCharge = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ZoneResponsabilite = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispatchers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Livraisons_DispatcheurId",
                table: "Livraisons",
                column: "DispatcheurId");

            migrationBuilder.AddForeignKey(
                name: "FK_Livraisons_Dispatchers_DispatcheurId",
                table: "Livraisons",
                column: "DispatcheurId",
                principalTable: "Dispatchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Livraisons_Dispatchers_DispatcheurId",
                table: "Livraisons");

            migrationBuilder.DropTable(
                name: "Dispatchers");

            migrationBuilder.DropIndex(
                name: "IX_Livraisons_DispatcheurId",
                table: "Livraisons");

            migrationBuilder.DropColumn(
                name: "DescriptionProduit",
                table: "Livraisons");

            migrationBuilder.DropColumn(
                name: "DispatcheurId",
                table: "Livraisons");

            migrationBuilder.DropColumn(
                name: "NomProduit",
                table: "Livraisons");

            migrationBuilder.DropColumn(
                name: "PoidsKg",
                table: "Livraisons");

            migrationBuilder.DropColumn(
                name: "PrixUnitaire",
                table: "Livraisons");

            migrationBuilder.DropColumn(
                name: "Quantite",
                table: "Livraisons");

            migrationBuilder.DropColumn(
                name: "VolumeM3",
                table: "Livraisons");

            migrationBuilder.DropColumn(
                name: "DispatcherId",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "Produits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nom = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PoidsKg = table.Column<double>(type: "float", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VolumeM3 = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LivraisonProduits",
                columns: table => new
                {
                    LivraisonId = table.Column<int>(type: "int", nullable: false),
                    ProduitId = table.Column<int>(type: "int", nullable: false),
                    PrixTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantite = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivraisonProduits", x => new { x.LivraisonId, x.ProduitId });
                    table.ForeignKey(
                        name: "FK_LivraisonProduits_Livraisons_LivraisonId",
                        column: x => x.LivraisonId,
                        principalTable: "Livraisons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LivraisonProduits_Produits_ProduitId",
                        column: x => x.ProduitId,
                        principalTable: "Produits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LivraisonProduits_ProduitId",
                table: "LivraisonProduits",
                column: "ProduitId");
        }
    }
}
