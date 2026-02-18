using RoomSplit.Core.Entities;
using RoomSplit.Core.Interfaces;

namespace RoomSplit.UnitTests.Helpers;

public static class MockUnitOfWorkFactory
{
    public static MockUnitOfWork Create()
    {
        var uow = new Mock<IUnitOfWork>();
        var users = new Mock<IRepository<User>>();
        var rooms = new Mock<IRepository<Room>>();
        var roomMembers = new Mock<IRepository<RoomMember>>();
        var refreshTokens = new Mock<IRepository<RefreshToken>>();
        var taskTemplates = new Mock<ITaskRepository>();
        var taskAssignments = new Mock<IRepository<TaskAssignment>>();
        var budgets = new Mock<IRepository<Budget>>();
        var transactions = new Mock<ITransactionRepository>();

        uow.Setup(u => u.Users).Returns(users.Object);
        uow.Setup(u => u.Rooms).Returns(rooms.Object);
        uow.Setup(u => u.RoomMembers).Returns(roomMembers.Object);
        uow.Setup(u => u.RefreshTokens).Returns(refreshTokens.Object);
        uow.Setup(u => u.TaskTemplates).Returns(taskTemplates.Object);
        uow.Setup(u => u.TaskAssignments).Returns(taskAssignments.Object);
        uow.Setup(u => u.Budgets).Returns(budgets.Object);
        uow.Setup(u => u.Transactions).Returns(transactions.Object);
        uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        return new MockUnitOfWork(uow, users, rooms, roomMembers, refreshTokens, taskTemplates, taskAssignments, budgets, transactions);
    }
}

public record MockUnitOfWork(
    Mock<IUnitOfWork> UnitOfWork,
    Mock<IRepository<User>> Users,
    Mock<IRepository<Room>> Rooms,
    Mock<IRepository<RoomMember>> RoomMembers,
    Mock<IRepository<RefreshToken>> RefreshTokens,
    Mock<ITaskRepository> TaskTemplates,
    Mock<IRepository<TaskAssignment>> TaskAssignments,
    Mock<IRepository<Budget>> Budgets,
    Mock<ITransactionRepository> Transactions
);
