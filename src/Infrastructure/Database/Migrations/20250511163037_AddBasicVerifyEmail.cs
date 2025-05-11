using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBasicVerifyEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_carts_users_id",
                schema: "public",
                table: "carts");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "carts",
                newName: "owner_id");

            migrationBuilder.AddColumn<bool>(
                name: "is_email_verified",
                schema: "public",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "email_verification_tokens",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_verification_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_email_verification_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_email_verification_tokens_user_id",
                schema: "public",
                table: "email_verification_tokens",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_carts_users_owner_id",
                schema: "public",
                table: "carts",
                column: "owner_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_carts_users_owner_id",
                schema: "public",
                table: "carts");

            migrationBuilder.DropTable(
                name: "email_verification_tokens",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "is_email_verified",
                schema: "public",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                schema: "public",
                table: "carts",
                newName: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_carts_users_id",
                schema: "public",
                table: "carts",
                column: "id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
