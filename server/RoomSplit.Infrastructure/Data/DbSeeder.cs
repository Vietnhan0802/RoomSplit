using Microsoft.EntityFrameworkCore;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;

namespace RoomSplit.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        // ====== USERS ======
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var user3Id = Guid.NewGuid();

        var users = new List<User>
        {
            new()
            {
                Id = user1Id,
                FullName = "Nguyễn Văn An",
                Email = "an@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                AvatarUrl = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = user2Id,
                FullName = "Trần Thị Bình",
                Email = "binh@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                AvatarUrl = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = user3Id,
                FullName = "Lê Minh Cường",
                Email = "cuong@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                AvatarUrl = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        await context.Users.AddRangeAsync(users);

        // ====== ROOM ======
        var roomId = Guid.NewGuid();
        var room = new Room
        {
            Id = roomId,
            Name = "Phòng 302 - Nguyễn Trãi",
            Description = "Phòng trọ 3 người ở quận 5",
            CreatedByUserId = user1Id,
            InviteCode = "RM302ABC",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await context.Rooms.AddAsync(room);

        // ====== ROOM MEMBERS ======
        var members = new List<RoomMember>
        {
            new()
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                UserId = user1Id,
                Role = RoomRole.Owner,
                NickName = "An",
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                UserId = user2Id,
                Role = RoomRole.Member,
                NickName = "Bình",
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                UserId = user3Id,
                Role = RoomRole.Member,
                NickName = "Cường",
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            }
        };
        await context.RoomMembers.AddRangeAsync(members);

        // ====== SHARED EXPENSES ======
        var expense1Id = Guid.NewGuid();
        var expense2Id = Guid.NewGuid();
        var expense3Id = Guid.NewGuid();

        var expenses = new List<SharedExpense>
        {
            new()
            {
                Id = expense1Id,
                RoomId = roomId,
                PaidByUserId = user1Id,
                Amount = 1_500_000m,
                Description = "Tiền điện tháng 1",
                Category = SharedExpenseCategory.Electricity,
                Date = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                SplitType = SplitType.Equal,
                Note = "Tháng này dùng nhiều điều hòa",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = expense2Id,
                RoomId = roomId,
                PaidByUserId = user2Id,
                Amount = 300_000m,
                Description = "Tiền nước tháng 1",
                Category = SharedExpenseCategory.Water,
                Date = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                SplitType = SplitType.Equal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = expense3Id,
                RoomId = roomId,
                PaidByUserId = user1Id,
                Amount = 250_000m,
                Description = "Tiền wifi tháng 1",
                Category = SharedExpenseCategory.Internet,
                Date = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                SplitType = SplitType.Equal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        await context.SharedExpenses.AddRangeAsync(expenses);

        // ====== EXPENSE SPLITS ======
        var splits = new List<ExpenseSplit>
        {
            // Tiền điện: 1,500,000 / 3 = 500,000 mỗi người
            new() { Id = Guid.NewGuid(), SharedExpenseId = expense1Id, UserId = user1Id, Amount = 500_000m, IsSettled = true, SettledAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), SharedExpenseId = expense1Id, UserId = user2Id, Amount = 500_000m, IsSettled = false },
            new() { Id = Guid.NewGuid(), SharedExpenseId = expense1Id, UserId = user3Id, Amount = 500_000m, IsSettled = false },
            // Tiền nước: 300,000 / 3 = 100,000
            new() { Id = Guid.NewGuid(), SharedExpenseId = expense2Id, UserId = user1Id, Amount = 100_000m, IsSettled = false },
            new() { Id = Guid.NewGuid(), SharedExpenseId = expense2Id, UserId = user2Id, Amount = 100_000m, IsSettled = true, SettledAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), SharedExpenseId = expense2Id, UserId = user3Id, Amount = 100_000m, IsSettled = false },
            // Tiền wifi: 250,000 / 3 ≈ 83,333
            new() { Id = Guid.NewGuid(), SharedExpenseId = expense3Id, UserId = user1Id, Amount = 83_334m, IsSettled = true, SettledAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), SharedExpenseId = expense3Id, UserId = user2Id, Amount = 83_333m, IsSettled = false },
            new() { Id = Guid.NewGuid(), SharedExpenseId = expense3Id, UserId = user3Id, Amount = 83_333m, IsSettled = false },
        };
        await context.ExpenseSplits.AddRangeAsync(splits);

        // ====== SETTLEMENT ======
        var settlements = new List<Settlement>
        {
            new()
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                PayerUserId = user3Id,
                PayeeUserId = user1Id,
                Amount = 200_000m,
                Note = "Trả bớt tiền điện + wifi",
                CreatedAt = DateTime.UtcNow
            }
        };
        await context.Settlements.AddRangeAsync(settlements);

        // ====== TASK TEMPLATES ======
        var task1Id = Guid.NewGuid();
        var task2Id = Guid.NewGuid();

        var taskTemplates = new List<TaskTemplate>
        {
            new()
            {
                Id = task1Id,
                RoomId = roomId,
                Title = "Đổ rác",
                Description = "Đổ rác ở hành lang tầng 3",
                Icon = "trash-2",
                FrequencyType = TaskFrequency.Daily,
                FrequencyValue = 1,
                RotationOrder = $"[\"{user1Id}\",\"{user2Id}\",\"{user3Id}\"]",
                CurrentAssigneeIndex = 0,
                StartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                CreatedByUserId = user1Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = task2Id,
                RoomId = roomId,
                Title = "Lau nhà",
                Description = "Lau toàn bộ phòng và nhà vệ sinh",
                Icon = "spray-can",
                FrequencyType = TaskFrequency.Weekly,
                FrequencyValue = 1,
                RotationOrder = $"[\"{user2Id}\",\"{user3Id}\",\"{user1Id}\"]",
                CurrentAssigneeIndex = 0,
                StartDate = new DateTime(2025, 1, 6, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                CreatedByUserId = user1Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        await context.TaskTemplates.AddRangeAsync(taskTemplates);

        // ====== TASK ASSIGNMENTS ======
        var assignments = new List<TaskAssignment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TaskTemplateId = task1Id,
                RoomId = roomId,
                AssignedToUserId = user1Id,
                DueDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Status = TaskCompletionStatus.Completed,
                CompletedAt = new DateTime(2025, 1, 1, 18, 0, 0, DateTimeKind.Utc),
                CompletedByUserId = user1Id,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TaskTemplateId = task1Id,
                RoomId = roomId,
                AssignedToUserId = user2Id,
                DueDate = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                Status = TaskCompletionStatus.Completed,
                CompletedAt = new DateTime(2025, 1, 2, 20, 0, 0, DateTimeKind.Utc),
                CompletedByUserId = user2Id,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TaskTemplateId = task1Id,
                RoomId = roomId,
                AssignedToUserId = user3Id,
                DueDate = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                Status = TaskCompletionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TaskTemplateId = task2Id,
                RoomId = roomId,
                AssignedToUserId = user2Id,
                DueDate = new DateTime(2025, 1, 6, 0, 0, 0, DateTimeKind.Utc),
                Status = TaskCompletionStatus.Completed,
                CompletedAt = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc),
                CompletedByUserId = user2Id,
                Note = "Đã lau sạch sẽ",
                CreatedAt = DateTime.UtcNow
            }
        };
        await context.TaskAssignments.AddRangeAsync(assignments);

        // ====== TRANSACTIONS (personal finance) ======
        var transactions = new List<Transaction>
        {
            // User 1 - Income
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user1Id,
                Type = TransactionType.Income,
                Amount = 15_000_000m,
                Description = "Lương tháng 1",
                IncomeCategory = IncomeCategory.Salary,
                Date = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                Tags = "lương,công ty",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user1Id,
                Type = TransactionType.Income,
                Amount = 3_000_000m,
                Description = "Freelance thiết kế logo",
                IncomeCategory = IncomeCategory.Freelance,
                Date = new DateTime(2025, 1, 12, 0, 0, 0, DateTimeKind.Utc),
                Tags = "freelance,design",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            // User 1 - Expenses
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user1Id,
                Type = TransactionType.Expense,
                Amount = 50_000m,
                Description = "Cà phê Highlands",
                ExpenseCategory = ExpenseCategory.Coffee,
                Date = new DateTime(2025, 1, 6, 0, 0, 0, DateTimeKind.Utc),
                Tags = "cà phê",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user1Id,
                Type = TransactionType.Expense,
                Amount = 200_000m,
                Description = "Đi grab đi làm",
                ExpenseCategory = ExpenseCategory.Transport,
                Date = new DateTime(2025, 1, 7, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user1Id,
                Type = TransactionType.Expense,
                Amount = 1_500_000m,
                Description = "Mua quần áo",
                ExpenseCategory = ExpenseCategory.Shopping,
                Date = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Tags = "quần áo,thời trang",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            // User 2 - Income
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user2Id,
                Type = TransactionType.Income,
                Amount = 12_000_000m,
                Description = "Lương tháng 1",
                IncomeCategory = IncomeCategory.Salary,
                Date = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            // User 2 - Expenses
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user2Id,
                Type = TransactionType.Expense,
                Amount = 80_000m,
                Description = "Ăn trưa cơm văn phòng",
                ExpenseCategory = ExpenseCategory.Food,
                Date = new DateTime(2025, 1, 6, 0, 0, 0, DateTimeKind.Utc),
                Tags = "ăn trưa",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user2Id,
                Type = TransactionType.Expense,
                Amount = 500_000m,
                Description = "Khám bệnh",
                ExpenseCategory = ExpenseCategory.Health,
                Date = new DateTime(2025, 1, 8, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        await context.Transactions.AddRangeAsync(transactions);

        // ====== BUDGETS ======
        var budgets = new List<Budget>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user1Id,
                ExpenseCategory = ExpenseCategory.Food,
                MonthlyLimit = 3_000_000m,
                Month = 1,
                Year = 2025,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user1Id,
                ExpenseCategory = ExpenseCategory.Coffee,
                MonthlyLimit = 500_000m,
                Month = 1,
                Year = 2025,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user1Id,
                ExpenseCategory = ExpenseCategory.Transport,
                MonthlyLimit = 1_000_000m,
                Month = 1,
                Year = 2025,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user2Id,
                ExpenseCategory = ExpenseCategory.Food,
                MonthlyLimit = 2_500_000m,
                Month = 1,
                Year = 2025,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user2Id,
                ExpenseCategory = ExpenseCategory.Entertainment,
                MonthlyLimit = 1_000_000m,
                Month = 1,
                Year = 2025,
                CreatedAt = DateTime.UtcNow
            }
        };
        await context.Budgets.AddRangeAsync(budgets);

        await context.SaveChangesAsync();
    }
}
