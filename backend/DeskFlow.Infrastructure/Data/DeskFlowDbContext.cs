using DeskFlow.Application.Abstractions;
using DeskFlow.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Infrastructure.Data;

public class DeskFlowDbContext(DbContextOptions<DeskFlowDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketHistory> TicketHistories => Set<TicketHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(180);
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(120);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(2000);
            entity.Property(x => x.PriorityReason).IsRequired().HasMaxLength(1000);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.Priority);
            entity.HasIndex(x => x.Category);
            entity.HasIndex(x => x.CreatedAt);

            entity.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedTickets)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssignedToUser)
                .WithMany(x => x.AssignedTickets)
                .HasForeignKey(x => x.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Content).IsRequired().HasMaxLength(2000);
            entity.HasOne(x => x.Ticket)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.User)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TicketHistory>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).IsRequired().HasMaxLength(80);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(1000);
            entity.HasOne(x => x.Ticket)
                .WithMany(x => x.History)
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
