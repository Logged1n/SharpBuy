using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RelationFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DomainUsers_CreatedAt",
                schema: "public",
                table: "DomainUsers");

            migrationBuilder.DropColumn(
                name: "PhotoPaths",
                schema: "public",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "public",
                table: "DomainUsers",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(9)",
                oldMaxLength: 9);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                schema: "public",
                table: "Orders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DomainUsers_UserId",
                schema: "public",
                table: "Orders",
                column: "UserId",
                principalSchema: "public",
                principalTable: "DomainUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DomainUsers_UserId",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId",
                schema: "public",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "PhotoPaths",
                schema: "public",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "public",
                table: "DomainUsers",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(12)",
                oldMaxLength: 12);

            migrationBuilder.CreateIndex(
                name: "IX_DomainUsers_CreatedAt",
                schema: "public",
                table: "DomainUsers",
                column: "CreatedAt");
        }
    }
}
