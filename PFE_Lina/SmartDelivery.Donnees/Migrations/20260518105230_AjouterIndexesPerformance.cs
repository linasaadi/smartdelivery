using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartDelivery.Data.Migrations
{
    /// <inheritdoc />
    public partial class AjouterIndexesPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PointsTracking_LivraisonId",
                table: "PointsTracking");

            migrationBuilder.DropIndex(
                name: "IX_Anomalies_LivraisonId",
                table: "Anomalies");

            migrationBuilder.AlterColumn<string>(
                name: "Statut",
                table: "Livraisons",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "EnAttente",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "EnAttente");

            migrationBuilder.AlterColumn<string>(
                name: "Statut",
                table: "Camions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "Disponible",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "Disponible");

            migrationBuilder.CreateIndex(
                name: "IX_PointsTracking_LivraisonId_Horodatage",
                table: "PointsTracking",
                columns: new[] { "LivraisonId", "Horodatage" });

            migrationBuilder.CreateIndex(
                name: "IX_Livraisons_DateCreation",
                table: "Livraisons",
                column: "DateCreation");

            migrationBuilder.CreateIndex(
                name: "IX_Livraisons_Statut",
                table: "Livraisons",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_Livraisons_Statut_DatePrevue",
                table: "Livraisons",
                columns: new[] { "Statut", "DateLivraisonPrevue" });

            migrationBuilder.CreateIndex(
                name: "IX_Camions_Statut",
                table: "Camions",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_Anomalies_LivraisonId_EstResolue",
                table: "Anomalies",
                columns: new[] { "LivraisonId", "EstResolue" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PointsTracking_LivraisonId_Horodatage",
                table: "PointsTracking");

            migrationBuilder.DropIndex(
                name: "IX_Livraisons_DateCreation",
                table: "Livraisons");

            migrationBuilder.DropIndex(
                name: "IX_Livraisons_Statut",
                table: "Livraisons");

            migrationBuilder.DropIndex(
                name: "IX_Livraisons_Statut_DatePrevue",
                table: "Livraisons");

            migrationBuilder.DropIndex(
                name: "IX_Camions_Statut",
                table: "Camions");

            migrationBuilder.DropIndex(
                name: "IX_Anomalies_LivraisonId_EstResolue",
                table: "Anomalies");

            migrationBuilder.AlterColumn<string>(
                name: "Statut",
                table: "Livraisons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "EnAttente",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldDefaultValue: "EnAttente");

            migrationBuilder.AlterColumn<string>(
                name: "Statut",
                table: "Camions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Disponible",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldDefaultValue: "Disponible");

            migrationBuilder.CreateIndex(
                name: "IX_PointsTracking_LivraisonId",
                table: "PointsTracking",
                column: "LivraisonId");

            migrationBuilder.CreateIndex(
                name: "IX_Anomalies_LivraisonId",
                table: "Anomalies",
                column: "LivraisonId");
        }
    }
}
