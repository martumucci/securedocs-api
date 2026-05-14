using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecureDocs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKdfFieldsToEncryptedPayloads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KdfAlgorithm",
                schema: "securedocs",
                table: "EncryptedPayloads",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KdfParameters",
                schema: "securedocs",
                table: "EncryptedPayloads",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "Salt",
                schema: "securedocs",
                table: "EncryptedPayloads",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KdfAlgorithm",
                schema: "securedocs",
                table: "EncryptedPayloads");

            migrationBuilder.DropColumn(
                name: "KdfParameters",
                schema: "securedocs",
                table: "EncryptedPayloads");

            migrationBuilder.DropColumn(
                name: "Salt",
                schema: "securedocs",
                table: "EncryptedPayloads");
        }
    }
}
