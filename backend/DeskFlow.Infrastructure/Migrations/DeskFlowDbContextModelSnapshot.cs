using System;
using DeskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DeskFlow.Infrastructure.Migrations;

[DbContext(typeof(DeskFlowDbContext))]
partial class DeskFlowDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.11");
        modelBuilder.Entity("DeskFlow.Domain.Models.User", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
            b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("Email").IsRequired().HasMaxLength(180).HasColumnType("character varying(180)");
            b.Property<bool>("IsActive").HasColumnType("boolean");
            b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<string>("PasswordHash").IsRequired().HasColumnType("text");
            b.Property<int>("Role").HasColumnType("integer");
            b.HasKey("Id"); b.HasIndex("Email").IsUnique(); b.ToTable("Users");
        });
        modelBuilder.Entity("DeskFlow.Domain.Models.Ticket", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            b.Property<Guid?>("AssignedToUserId").HasColumnType("uuid"); b.Property<int>("Category").HasColumnType("integer");
            b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone"); b.Property<Guid>("CreatedByUserId").HasColumnType("uuid");
            b.Property<string>("Description").IsRequired().HasMaxLength(2000).HasColumnType("character varying(2000)");
            b.Property<int>("Priority").HasColumnType("integer"); b.Property<string>("PriorityReason").IsRequired().HasMaxLength(1000).HasColumnType("character varying(1000)");
            b.Property<int>("PriorityScore").HasColumnType("integer"); b.Property<DateTime?>("ResolvedAt").HasColumnType("timestamp with time zone");
            b.Property<int>("Status").HasColumnType("integer"); b.Property<string>("Title").IsRequired().HasMaxLength(120).HasColumnType("character varying(120)");
            b.Property<DateTime>("UpdatedAt").HasColumnType("timestamp with time zone"); b.HasKey("Id");
            b.HasIndex("AssignedToUserId"); b.HasIndex("Category"); b.HasIndex("CreatedAt"); b.HasIndex("CreatedByUserId"); b.HasIndex("Priority"); b.HasIndex("Status"); b.ToTable("Tickets");
        });
        modelBuilder.Entity("DeskFlow.Domain.Models.TicketComment", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            b.Property<string>("Content").IsRequired().HasMaxLength(2000).HasColumnType("character varying(2000)"); b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<int>("TicketId").HasColumnType("integer"); b.Property<Guid>("UserId").HasColumnType("uuid"); b.HasKey("Id"); b.HasIndex("TicketId"); b.HasIndex("UserId"); b.ToTable("TicketComments");
        });
        modelBuilder.Entity("DeskFlow.Domain.Models.TicketHistory", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            b.Property<string>("Action").IsRequired().HasMaxLength(80).HasColumnType("character varying(80)"); b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("Description").IsRequired().HasMaxLength(1000).HasColumnType("character varying(1000)"); b.Property<int>("TicketId").HasColumnType("integer"); b.Property<Guid?>("UserId").HasColumnType("uuid");
            b.HasKey("Id"); b.HasIndex("TicketId"); b.HasIndex("UserId"); b.ToTable("TicketHistories");
        });
        modelBuilder.Entity("DeskFlow.Domain.Models.Ticket", b =>
        {
            b.HasOne("DeskFlow.Domain.Models.User", "AssignedToUser").WithMany("AssignedTickets").HasForeignKey("AssignedToUserId").OnDelete(DeleteBehavior.SetNull);
            b.HasOne("DeskFlow.Domain.Models.User", "CreatedByUser").WithMany("CreatedTickets").HasForeignKey("CreatedByUserId").OnDelete(DeleteBehavior.Restrict).IsRequired();
        });
        modelBuilder.Entity("DeskFlow.Domain.Models.TicketComment", b =>
        {
            b.HasOne("DeskFlow.Domain.Models.Ticket", "Ticket").WithMany("Comments").HasForeignKey("TicketId").OnDelete(DeleteBehavior.Cascade).IsRequired();
            b.HasOne("DeskFlow.Domain.Models.User", "User").WithMany("Comments").HasForeignKey("UserId").OnDelete(DeleteBehavior.Restrict).IsRequired();
        });
        modelBuilder.Entity("DeskFlow.Domain.Models.TicketHistory", b =>
        {
            b.HasOne("DeskFlow.Domain.Models.Ticket", "Ticket").WithMany("History").HasForeignKey("TicketId").OnDelete(DeleteBehavior.Cascade).IsRequired();
            b.HasOne("DeskFlow.Domain.Models.User", "User").WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.SetNull);
        });
    }
}
