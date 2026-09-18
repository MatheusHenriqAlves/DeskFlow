using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DeskFlow.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: false),
                Role = table.Column<int>(type: "integer", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Tickets",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                Priority = table.Column<int>(type: "integer", nullable: false),
                PriorityScore = table.Column<int>(type: "integer", nullable: false),
                PriorityReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                Category = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tickets", x => x.Id);
                table.ForeignKey("FK_Tickets_Users_AssignedToUserId", x => x.AssignedToUserId, "Users", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_Tickets_Users_CreatedByUserId", x => x.CreatedByUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TicketComments",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                TicketId = table.Column<int>(type: "integer", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TicketComments", x => x.Id);
                table.ForeignKey("FK_TicketComments_Tickets_TicketId", x => x.TicketId, "Tickets", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_TicketComments_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TicketHistories",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                TicketId = table.Column<int>(type: "integer", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: true),
                Action = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TicketHistories", x => x.Id);
                table.ForeignKey("FK_TicketHistories_Tickets_TicketId", x => x.TicketId, "Tickets", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_TicketHistories_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_Users_Email", "Users", "Email", unique: true);
        migrationBuilder.CreateIndex("IX_Tickets_AssignedToUserId", "Tickets", "AssignedToUserId");
        migrationBuilder.CreateIndex("IX_Tickets_Category", "Tickets", "Category");
        migrationBuilder.CreateIndex("IX_Tickets_CreatedAt", "Tickets", "CreatedAt");
        migrationBuilder.CreateIndex("IX_Tickets_CreatedByUserId", "Tickets", "CreatedByUserId");
        migrationBuilder.CreateIndex("IX_Tickets_Priority", "Tickets", "Priority");
        migrationBuilder.CreateIndex("IX_Tickets_Status", "Tickets", "Status");
        migrationBuilder.CreateIndex("IX_TicketComments_TicketId", "TicketComments", "TicketId");
        migrationBuilder.CreateIndex("IX_TicketComments_UserId", "TicketComments", "UserId");
        migrationBuilder.CreateIndex("IX_TicketHistories_TicketId", "TicketHistories", "TicketId");
        migrationBuilder.CreateIndex("IX_TicketHistories_UserId", "TicketHistories", "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("TicketComments");
        migrationBuilder.DropTable("TicketHistories");
        migrationBuilder.DropTable("Tickets");
        migrationBuilder.DropTable("Users");
    }
}
