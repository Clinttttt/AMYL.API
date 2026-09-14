using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMYL.Api.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Brings the schema up to date with four sets of un-migrated model changes:
    ///
    ///   1. AuditableEntity gained CreatedAt/CreatedBy/UpdatedAt/UpdatedBy/
    ///      DeletedAt/DeletedBy/IsDeleted, and CreateAt was renamed to CreatedAt.
    ///   2. MemoryConfiguration declares an explicit FK on Memory.UserId, replacing
    ///      the shadow "UsersId" FK column EF had generated.
    ///   3. UserConfiguration declares unique indexes on Email and UserName.
    ///   4. MemoryConfiguration declares a composite index on (UserId, CreatedAt).
    ///
    /// HAND-WRITTEN, do not regenerate. The scaffolded version renamed
    /// "CreateAt" to "UpdatedAt" and added an empty "CreatedAt", which would have
    /// moved every existing creation timestamp into UpdatedAt and zeroed
    /// CreatedAt. This version renames "CreateAt" to "CreatedAt" so the original
    /// timestamps survive, then seeds UpdatedAt from CreatedAt.
    ///
    /// Pre-flight verified against the target database before authoring:
    /// 0 orphan Memories rows, 0 rows with "UsersId" populated, 0 duplicate
    /// emails, 0 duplicate usernames. The FK and unique indexes therefore apply
    /// without data cleanup.
    /// </summary>
    public partial class AuditFields_And_ExplicitUserFk : Migration
    {
        private const string EpochSql = "'0001-01-01 00:00:00+00'::timestamptz";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---- 1. Drop the shadow FK that EF had generated for the Users nav ----
            // Verified 0 rows have "UsersId" populated, so no relationship data is lost.
            migrationBuilder.DropForeignKey(
                name: "FK_Memories_Users_UsersId",
                table: "Memories");

            migrationBuilder.DropIndex(
                name: "IX_Memories_UsersId",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "Memories");

            // ---- 2. Preserve creation timestamps ----
            // This is the whole reason the migration is hand-written.
            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "Memories",
                newName: "CreatedAt");

            // ---- 3. Add the remaining audit columns ----
            // defaultValueSql avoids Npgsql rejecting a non-UTC DateTime literal
            // for a "timestamp with time zone" column.
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Memories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: EpochSql);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Memories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: EpochSql);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Memories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Memories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Memories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Memories",
                type: "text",
                nullable: true);

            // ---- 4. Existing rows have never been updated, so UpdatedAt = CreatedAt ----
            migrationBuilder.Sql(
                @"UPDATE ""Memories"" SET ""UpdatedAt"" = ""CreatedAt"";");

            // ---- 5. Drop the column defaults so the schema matches the model ----
            // The entity declares no default; the defaults above existed only to
            // backfill the NOT NULL columns on existing rows.
            migrationBuilder.Sql(
                @"ALTER TABLE ""Memories"" ALTER COLUMN ""UpdatedAt"" DROP DEFAULT;");
            migrationBuilder.Sql(
                @"ALTER TABLE ""Memories"" ALTER COLUMN ""DeletedAt"" DROP DEFAULT;");
            migrationBuilder.Sql(
                @"ALTER TABLE ""Memories"" ALTER COLUMN ""IsDeleted"" DROP DEFAULT;");

            // ---- 6. Explicit FK on UserId + supporting composite index ----
            migrationBuilder.CreateIndex(
                name: "IX_Memories_UserId_CreatedAt",
                table: "Memories",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Memories_Users_UserId",
                table: "Memories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // ---- 7. Unique identity constraints ----
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <remarks>
        /// Down is a genuine rollback of the schema, but the audit values held in
        /// the dropped columns cannot be recovered. CreatedAt is renamed back to
        /// CreateAt so the original timestamps still survive the round trip.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserName",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Memories_Users_UserId",
                table: "Memories");

            migrationBuilder.DropIndex(
                name: "IX_Memories_UserId_CreatedAt",
                table: "Memories");

            migrationBuilder.DropColumn(name: "UpdatedAt", table: "Memories");
            migrationBuilder.DropColumn(name: "UpdatedBy", table: "Memories");
            migrationBuilder.DropColumn(name: "DeletedAt", table: "Memories");
            migrationBuilder.DropColumn(name: "DeletedBy", table: "Memories");
            migrationBuilder.DropColumn(name: "IsDeleted", table: "Memories");
            migrationBuilder.DropColumn(name: "CreatedBy", table: "Memories");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Memories",
                newName: "CreateAt");

            migrationBuilder.AddColumn<Guid>(
                name: "UsersId",
                table: "Memories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Memories_UsersId",
                table: "Memories",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Memories_Users_UsersId",
                table: "Memories",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
