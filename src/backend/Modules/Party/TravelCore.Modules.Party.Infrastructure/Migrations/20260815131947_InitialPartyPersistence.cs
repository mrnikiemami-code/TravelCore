using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Party.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPartyPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "party");

            migrationBuilder.CreateTable(
                name: "parties",
                schema: "party",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<short>(type: "smallint", nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    primary_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    primary_phone = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parties", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "party_agencies",
                schema: "party",
                columns: table => new
                {
                    party_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trading_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    license_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_party_agencies", x => x.party_id);
                    table.ForeignKey(
                        name: "FK_party_agencies_parties_party_id",
                        column: x => x.party_id,
                        principalSchema: "party",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "party_organizations",
                schema: "party",
                columns: table => new
                {
                    party_id = table.Column<Guid>(type: "uuid", nullable: false),
                    legal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    trade_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_party_organizations", x => x.party_id);
                    table.ForeignKey(
                        name: "FK_party_organizations_parties_party_id",
                        column: x => x.party_id,
                        principalSchema: "party",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "party_persons",
                schema: "party",
                columns: table => new
                {
                    party_id = table.Column<Guid>(type: "uuid", nullable: false),
                    given_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    family_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_party_persons", x => x.party_id);
                    table.ForeignKey(
                        name: "FK_party_persons_parties_party_id",
                        column: x => x.party_id,
                        principalSchema: "party",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_parties_display_name",
                schema: "party",
                table: "parties",
                column: "display_name");

            migrationBuilder.CreateIndex(
                name: "ix_parties_kind",
                schema: "party",
                table: "parties",
                column: "kind");

            migrationBuilder.CreateIndex(
                name: "ix_parties_status",
                schema: "party",
                table: "parties",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "party_agencies",
                schema: "party");

            migrationBuilder.DropTable(
                name: "party_organizations",
                schema: "party");

            migrationBuilder.DropTable(
                name: "party_persons",
                schema: "party");

            migrationBuilder.DropTable(
                name: "parties",
                schema: "party");
        }
    }
}
