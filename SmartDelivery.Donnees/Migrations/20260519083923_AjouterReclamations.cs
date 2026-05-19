using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartDelivery.Data.Migrations
{
    /// <inheritdoc />
    public partial class AjouterReclamations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RaisonRefus",
                table: "Livraisons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Reclamations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValue: "EnAttente"),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateResolution = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReponseAdmin = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LivraisonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reclamations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reclamations_AspNetUsers_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reclamations_Livraisons_LivraisonId",
                        column: x => x.LivraisonId,
                        principalTable: "Livraisons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reclamations_LivraisonId",
                table: "Reclamations",
                column: "LivraisonId");

            migrationBuilder.CreateIndex(
                name: "IX_Reclamations_Statut",
                table: "Reclamations",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_Reclamations_UtilisateurId",
                table: "Reclamations",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reclamations");

            migrationBuilder.DropColumn(
                name: "RaisonRefus",
                table: "Livraisons");
        }
    }
}
