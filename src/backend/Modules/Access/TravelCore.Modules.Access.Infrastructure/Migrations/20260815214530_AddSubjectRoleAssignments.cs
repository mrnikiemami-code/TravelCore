using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Access.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectRoleAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subject_role_assignments",
                schema: "access",
                columns: table => new
                {
                    subject_kind = table.Column<short>(type: "smallint", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subject_role_assignments", x => new { x.subject_kind, x.subject_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_subject_role_assignments_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "access",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_subject_role_assignments_role_id",
                schema: "access",
                table: "subject_role_assignments",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_subject_role_assignments_subject",
                schema: "access",
                table: "subject_role_assignments",
                columns: new[] { "subject_kind", "subject_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subject_role_assignments",
                schema: "access");
        }
    }
}
