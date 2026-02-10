using Microsoft.EntityFrameworkCore;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;

namespace RoomSplit.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomMember> RoomMembers => Set<RoomMember>();
    public DbSet<SharedExpense> SharedExpenses => Set<SharedExpense>();
    public DbSet<ExpenseSplit> ExpenseSplits => Set<ExpenseSplit>();
    public DbSet<Settlement> Settlements => Set<Settlement>();
    public DbSet<TaskTemplate> TaskTemplates => Set<TaskTemplate>();
    public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<TransactionImage> TransactionImages => Set<TransactionImage>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ====== USER ======
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.PasswordHash).IsRequired();
        });

        // ====== ROOM ======
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Description).HasMaxLength(500);
            entity.Property(r => r.InviteCode).IsRequired().HasMaxLength(8);
            entity.HasIndex(r => r.InviteCode).IsUnique();

            entity.HasOne(r => r.CreatedBy)
                .WithMany(u => u.CreatedRooms)
                .HasForeignKey(r => r.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ====== ROOM MEMBER ======
        modelBuilder.Entity<RoomMember>(entity =>
        {
            entity.HasKey(rm => rm.Id);
            entity.HasIndex(rm => new { rm.RoomId, rm.UserId }).IsUnique();
            entity.Property(rm => rm.NickName).HasMaxLength(50);
            entity.Property(rm => rm.Role).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(rm => rm.Room)
                .WithMany(r => r.Members)
                .HasForeignKey(rm => rm.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rm => rm.User)
                .WithMany(u => u.RoomMembers)
                .HasForeignKey(rm => rm.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ====== SHARED EXPENSE ======
        modelBuilder.Entity<SharedExpense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.SplitType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.ReceiptImageUrl).HasMaxLength(500);
            entity.Property(e => e.Note).HasMaxLength(500);

            entity.HasOne(e => e.Room)
                .WithMany(r => r.SharedExpenses)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PaidBy)
                .WithMany(u => u.PaidExpenses)
                .HasForeignKey(e => e.PaidByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ====== EXPENSE SPLIT ======
        modelBuilder.Entity<ExpenseSplit>(entity =>
        {
            entity.HasKey(es => es.Id);
            entity.Property(es => es.Amount).HasPrecision(18, 2);

            entity.HasOne(es => es.SharedExpense)
                .WithMany(e => e.Splits)
                .HasForeignKey(es => es.SharedExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(es => es.User)
                .WithMany(u => u.ExpenseSplits)
                .HasForeignKey(es => es.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ====== SETTLEMENT ======
        modelBuilder.Entity<Settlement>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Amount).HasPrecision(18, 2);
            entity.Property(s => s.Note).HasMaxLength(500);

            entity.HasOne(s => s.Room)
                .WithMany(r => r.Settlements)
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Payer)
                .WithMany(u => u.SentSettlements)
                .HasForeignKey(s => s.PayerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Payee)
                .WithMany(u => u.ReceivedSettlements)
                .HasForeignKey(s => s.PayeeUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ====== TASK TEMPLATE ======
        modelBuilder.Entity<TaskTemplate>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Description).HasMaxLength(500);
            entity.Property(t => t.Icon).HasMaxLength(50);
            entity.Property(t => t.FrequencyType).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.RotationOrder).HasColumnType("jsonb");

            entity.HasOne(t => t.Room)
                .WithMany(r => r.TaskTemplates)
                .HasForeignKey(t => t.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.CreatedBy)
                .WithMany(u => u.CreatedTaskTemplates)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ====== TASK ASSIGNMENT ======
        modelBuilder.Entity<TaskAssignment>(entity =>
        {
            entity.HasKey(ta => ta.Id);
            entity.Property(ta => ta.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(ta => ta.Note).HasMaxLength(200);
            entity.Property(ta => ta.ProofImageUrl).HasMaxLength(500);

            entity.HasOne(ta => ta.TaskTemplate)
                .WithMany(t => t.Assignments)
                .HasForeignKey(ta => ta.TaskTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ta => ta.Room)
                .WithMany(r => r.TaskAssignments)
                .HasForeignKey(ta => ta.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ta => ta.AssignedTo)
                .WithMany(u => u.TaskAssignments)
                .HasForeignKey(ta => ta.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ta => ta.CompletedBy)
                .WithMany(u => u.CompletedTaskAssignments)
                .HasForeignKey(ta => ta.CompletedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ====== TRANSACTION ======
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.Property(t => t.Description).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.IncomeCategory).HasConversion<string?>().HasMaxLength(30);
            entity.Property(t => t.ExpenseCategory).HasConversion<string?>().HasMaxLength(30);
            entity.Property(t => t.ImageUrl).HasMaxLength(500);
            entity.Property(t => t.Note).HasMaxLength(500);
            entity.Property(t => t.Tags).HasMaxLength(500);

            entity.HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ====== BUDGET ======
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.MonthlyLimit).HasPrecision(18, 2);
            entity.Property(b => b.ExpenseCategory).HasConversion<string>().HasMaxLength(30);

            entity.HasIndex(b => new { b.UserId, b.ExpenseCategory, b.Month, b.Year }).IsUnique();

            entity.HasOne(b => b.User)
                .WithMany(u => u.Budgets)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ====== TRANSACTION IMAGE ======
        modelBuilder.Entity<TransactionImage>(entity =>
        {
            entity.HasKey(ti => ti.Id);
            entity.Property(ti => ti.ImageUrl).IsRequired().HasMaxLength(500);
            entity.Property(ti => ti.ThumbnailUrl).HasMaxLength(500);
            entity.Property(ti => ti.OriginalFileName).IsRequired().HasMaxLength(200);

            entity.HasOne(ti => ti.Transaction)
                .WithMany(t => t.Images)
                .HasForeignKey(ti => ti.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ====== REFRESH TOKEN ======
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
            entity.Property(rt => rt.CreatedByIp).IsRequired().HasMaxLength(45);
            entity.Property(rt => rt.RevokedByIp).HasMaxLength(45);
            entity.Property(rt => rt.RevokedReason).HasMaxLength(200);
            entity.Property(rt => rt.ReplacedByToken).HasMaxLength(500);

            entity.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
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
