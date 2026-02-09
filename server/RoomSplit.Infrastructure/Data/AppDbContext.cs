using Microsoft.EntityFrameworkCore;
using RoomSplit.Core.Entities;

namespace RoomSplit.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomMember> RoomMembers => Set<RoomMember>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseSplit> ExpenseSplits => Set<ExpenseSplit>();
    public DbSet<RoomTask> RoomTasks => Set<RoomTask>();
    public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget> Budgets => Set<Budget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FullName).HasMaxLength(100);
            entity.Property(u => u.Email).HasMaxLength(255);
        });

        // Room
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasIndex(r => r.InviteCode).IsUnique();
            entity.Property(r => r.Name).HasMaxLength(100);
            entity.Property(r => r.InviteCode).HasMaxLength(10);
            entity.HasOne(r => r.CreatedBy)
                .WithMany()
                .HasForeignKey(r => r.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RoomMember
        modelBuilder.Entity<RoomMember>(entity =>
        {
            entity.HasIndex(rm => new { rm.RoomId, rm.UserId }).IsUnique();
            entity.HasOne(rm => rm.Room)
                .WithMany(r => r.Members)
                .HasForeignKey(rm => rm.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rm => rm.User)
                .WithMany(u => u.RoomMembers)
                .HasForeignKey(rm => rm.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Expense
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasOne(e => e.Room)
                .WithMany(r => r.Expenses)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PaidBy)
                .WithMany(u => u.CreatedExpenses)
                .HasForeignKey(e => e.PaidByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ExpenseSplit
        modelBuilder.Entity<ExpenseSplit>(entity =>
        {
            entity.Property(es => es.Amount).HasPrecision(18, 2);
            entity.Property(es => es.Percentage).HasPrecision(5, 2);
            entity.HasOne(es => es.Expense)
                .WithMany(e => e.Splits)
                .HasForeignKey(es => es.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(es => es.User)
                .WithMany(u => u.ExpenseSplits)
                .HasForeignKey(es => es.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RoomTask
        modelBuilder.Entity<RoomTask>(entity =>
        {
            entity.Property(t => t.Title).HasMaxLength(200);
            entity.HasOne(t => t.Room)
                .WithMany(r => r.Tasks)
                .HasForeignKey(t => t.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TaskAssignment
        modelBuilder.Entity<TaskAssignment>(entity =>
        {
            entity.HasOne(ta => ta.RoomTask)
                .WithMany(t => t.Assignments)
                .HasForeignKey(ta => ta.RoomTaskId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ta => ta.AssignedTo)
                .WithMany(u => u.TaskAssignments)
                .HasForeignKey(ta => ta.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.Property(t => t.Description).HasMaxLength(500);
            entity.HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Budget
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.Property(b => b.LimitAmount).HasPrecision(18, 2);
            entity.HasIndex(b => new { b.UserId, b.Category, b.Month, b.Year }).IsUnique();
            entity.HasOne(b => b.User)
                .WithMany(u => u.Budgets)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
