using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;
using RoomSplit.Core.Interfaces;
using RoomSplit.Infrastructure.Services;
using RoomSplit.UnitTests.Helpers;

namespace RoomSplit.UnitTests.Services;

public class TaskServiceTests
{
    private readonly TaskService _sut;
    private readonly MockUnitOfWork _mocks;
    private readonly Mock<IFileService> _fileService;

    public TaskServiceTests()
    {
        _mocks = MockUnitOfWorkFactory.Create();
        _fileService = new Mock<IFileService>();
        _sut = new TaskService(_mocks.UnitOfWork.Object, _fileService.Object);
    }

    private static TaskTemplate CreateTemplate(
        List<Guid> rotationOrder,
        TaskFrequency frequency = TaskFrequency.Daily,
        int frequencyValue = 1,
        int currentIndex = 0,
        bool isActive = true)
    {
        return new TaskTemplate
        {
            Id = Guid.NewGuid(),
            RoomId = Guid.NewGuid(),
            Title = "Test Task",
            FrequencyType = frequency,
            FrequencyValue = frequencyValue,
            RotationOrder = JsonSerializer.Serialize(rotationOrder),
            CurrentAssigneeIndex = currentIndex,
            StartDate = DateTime.UtcNow.Date,
            IsActive = isActive,
            CreatedByUserId = rotationOrder.FirstOrDefault()
        };
    }

    #region GenerateAssignmentsForTemplateAsync

    [Fact]
    public async Task GenerateAssignments_WithNullTemplate_DoesNothing()
    {
        _mocks.TaskTemplates.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TaskTemplate?)null);

        await _sut.GenerateAssignmentsForTemplateAsync(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(7));

        _mocks.TaskAssignments.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<TaskAssignment>>()), Times.Never);
    }

    [Fact]
    public async Task GenerateAssignments_WithInactiveTemplate_DoesNothing()
    {
        var template = CreateTemplate(new List<Guid> { Guid.NewGuid() }, isActive: false);
        _mocks.TaskTemplates.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        await _sut.GenerateAssignmentsForTemplateAsync(template.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));

        _mocks.TaskAssignments.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<TaskAssignment>>()), Times.Never);
    }

    [Fact]
    public async Task GenerateAssignments_WithEmptyRotation_DoesNothing()
    {
        var template = CreateTemplate(new List<Guid>());
        _mocks.TaskTemplates.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        await _sut.GenerateAssignmentsForTemplateAsync(template.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));

        _mocks.TaskAssignments.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<TaskAssignment>>()), Times.Never);
    }

    [Fact]
    public async Task GenerateAssignments_DailyFrequency_CreatesCorrectCount()
    {
        var userId = Guid.NewGuid();
        var template = CreateTemplate(new List<Guid> { userId }, TaskFrequency.Daily);
        _mocks.TaskTemplates.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 7);

        IEnumerable<TaskAssignment>? savedAssignments = null;
        _mocks.TaskAssignments
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<TaskAssignment>>()))
            .Callback<IEnumerable<TaskAssignment>>(a => savedAssignments = a.ToList());

        await _sut.GenerateAssignmentsForTemplateAsync(template.Id, startDate, endDate);

        savedAssignments.Should().NotBeNull();
        savedAssignments!.Count().Should().Be(7); // 7 days
        savedAssignments.All(a => a.AssignedToUserId == userId).Should().BeTrue();
        savedAssignments.All(a => a.Status == TaskCompletionStatus.Pending).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAssignments_WeeklyFrequency_CreatesWeeklyAssignments()
    {
        var userId = Guid.NewGuid();
        var template = CreateTemplate(new List<Guid> { userId }, TaskFrequency.Weekly, (int)DayOfWeek.Monday);
        _mocks.TaskTemplates.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        // Monday Jan 6, 2025 to Monday Feb 3, 2025 (4 weeks)
        var startDate = new DateTime(2025, 1, 6); // Monday
        var endDate = new DateTime(2025, 2, 3);

        IEnumerable<TaskAssignment>? savedAssignments = null;
        _mocks.TaskAssignments
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<TaskAssignment>>()))
            .Callback<IEnumerable<TaskAssignment>>(a => savedAssignments = a.ToList());

        await _sut.GenerateAssignmentsForTemplateAsync(template.Id, startDate, endDate);

        savedAssignments.Should().NotBeNull();
        savedAssignments!.Count().Should().BeGreaterThanOrEqualTo(1);
        // All assignments should be on Mondays
        savedAssignments.All(a => a.DueDate.DayOfWeek == DayOfWeek.Monday || a.DueDate == startDate).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAssignments_DeletesFutureAssignmentsFirst()
    {
        var template = CreateTemplate(new List<Guid> { Guid.NewGuid() });
        _mocks.TaskTemplates.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        var startDate = DateTime.UtcNow.Date;
        await _sut.GenerateAssignmentsForTemplateAsync(template.Id, startDate, startDate.AddDays(7));

        _mocks.TaskTemplates.Verify(r => r.DeleteFutureAssignmentsAsync(template.Id, startDate), Times.Once);
    }

    #endregion

    #region GetNextAssigneeUserId

    [Fact]
    public void GetNextAssigneeUserId_ReturnsCorrectUser()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var user3 = Guid.NewGuid();
        var template = CreateTemplate(new List<Guid> { user1, user2, user3 }, currentIndex: 1);

        var result = _sut.GetNextAssigneeUserId(template);

        result.Should().Be(user2);
    }

    [Fact]
    public void GetNextAssigneeUserId_WithIndexBeyondCount_WrapsAround()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var template = CreateTemplate(new List<Guid> { user1, user2 }, currentIndex: 5);

        var result = _sut.GetNextAssigneeUserId(template);

        result.Should().Be(user2); // 5 % 2 == 1
    }

    [Fact]
    public void GetNextAssigneeUserId_WithEmptyRotation_Throws()
    {
        var template = CreateTemplate(new List<Guid>());

        _sut.Invoking(s => s.GetNextAssigneeUserId(template))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*empty*");
    }

    #endregion

    #region CompleteAssignmentAsync

    [Fact]
    public async Task CompleteAssignment_NotFound_ThrowsKeyNotFound()
    {
        _mocks.TaskAssignments.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TaskAssignment?)null);

        await _sut.Invoking(s => s.CompleteAssignmentAsync(Guid.NewGuid(), Guid.NewGuid(), null, null))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CompleteAssignment_WrongUser_ThrowsUnauthorized()
    {
        var assignment = new TaskAssignment
        {
            Id = Guid.NewGuid(),
            AssignedToUserId = Guid.NewGuid(),
            Status = TaskCompletionStatus.Pending
        };
        _mocks.TaskAssignments.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);

        await _sut.Invoking(s => s.CompleteAssignmentAsync(assignment.Id, Guid.NewGuid(), null, null))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task CompleteAssignment_AlreadyCompleted_ThrowsInvalidOperation()
    {
        var userId = Guid.NewGuid();
        var assignment = new TaskAssignment
        {
            Id = Guid.NewGuid(),
            AssignedToUserId = userId,
            Status = TaskCompletionStatus.Completed
        };
        _mocks.TaskAssignments.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);

        await _sut.Invoking(s => s.CompleteAssignmentAsync(assignment.Id, userId, null, null))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CompleteAssignment_Valid_SetsFieldsAndAdvancesRotation()
    {
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var template = CreateTemplate(new List<Guid> { userId, Guid.NewGuid() });
        template.Id = templateId;

        var assignment = new TaskAssignment
        {
            Id = Guid.NewGuid(),
            TaskTemplateId = templateId,
            AssignedToUserId = userId,
            Status = TaskCompletionStatus.Pending
        };

        _mocks.TaskAssignments.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);
        _mocks.TaskTemplates.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(template);

        var result = await _sut.CompleteAssignmentAsync(assignment.Id, userId, "Done!", null);

        result.Status.Should().Be(TaskCompletionStatus.Completed);
        result.CompletedAt.Should().NotBeNull();
        result.CompletedByUserId.Should().Be(userId);
        result.Note.Should().Be("Done!");
    }

    [Fact]
    public async Task CompleteAssignment_WithProofImage_UploadsFile()
    {
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var template = CreateTemplate(new List<Guid> { userId });
        template.Id = templateId;

        var assignment = new TaskAssignment
        {
            Id = Guid.NewGuid(),
            TaskTemplateId = templateId,
            AssignedToUserId = userId,
            Status = TaskCompletionStatus.Pending
        };

        _mocks.TaskAssignments.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);
        _mocks.TaskTemplates.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(template);

        var mockFile = new Mock<IFormFile>();
        _fileService.Setup(f => f.UploadFileAsync(mockFile.Object, "tasks")).ReturnsAsync("/tasks/proof.jpg");

        var result = await _sut.CompleteAssignmentAsync(assignment.Id, userId, null, mockFile.Object);

        result.ProofImageUrl.Should().Be("/tasks/proof.jpg");
        _fileService.Verify(f => f.UploadFileAsync(mockFile.Object, "tasks"), Times.Once);
    }

    #endregion

    #region SkipAssignmentAsync

    [Fact]
    public async Task SkipAssignment_Valid_SetsSkippedStatusAndAdvances()
    {
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var template = CreateTemplate(new List<Guid> { userId, Guid.NewGuid() });
        template.Id = templateId;

        var assignment = new TaskAssignment
        {
            Id = Guid.NewGuid(),
            TaskTemplateId = templateId,
            AssignedToUserId = userId,
            Status = TaskCompletionStatus.Pending
        };

        _mocks.TaskAssignments.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);
        _mocks.TaskTemplates.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(template);

        var result = await _sut.SkipAssignmentAsync(assignment.Id, userId, "Busy today");

        result.Status.Should().Be(TaskCompletionStatus.Skipped);
        result.Note.Should().Be("Busy today");
    }

    [Fact]
    public async Task SkipAssignment_WrongUser_ThrowsUnauthorized()
    {
        var assignment = new TaskAssignment
        {
            Id = Guid.NewGuid(),
            AssignedToUserId = Guid.NewGuid(),
            Status = TaskCompletionStatus.Pending
        };
        _mocks.TaskAssignments.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);

        await _sut.Invoking(s => s.SkipAssignmentAsync(assignment.Id, Guid.NewGuid(), null))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion

    #region SwapAssignmentAsync

    [Fact]
    public async Task SwapAssignment_Valid_ChangesAssignee()
    {
        var fromUser = Guid.NewGuid();
        var toUser = Guid.NewGuid();
        var assignment = new TaskAssignment
        {
            Id = Guid.NewGuid(),
            AssignedToUserId = fromUser,
            Status = TaskCompletionStatus.Pending
        };

        _mocks.TaskAssignments.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);

        var result = await _sut.SwapAssignmentAsync(assignment.Id, fromUser, toUser);

        result.AssignedToUserId.Should().Be(toUser);
    }

    [Fact]
    public async Task SwapAssignment_WrongUser_ThrowsUnauthorized()
    {
        var assignment = new TaskAssignment
        {
            Id = Guid.NewGuid(),
            AssignedToUserId = Guid.NewGuid(),
            Status = TaskCompletionStatus.Pending
        };
        _mocks.TaskAssignments.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);

        await _sut.Invoking(s => s.SwapAssignmentAsync(assignment.Id, Guid.NewGuid(), Guid.NewGuid()))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task SwapAssignment_NotPending_ThrowsInvalidOperation()
    {
        var userId = Guid.NewGuid();
        var assignment = new TaskAssignment
        {
            Id = Guid.NewGuid(),
            AssignedToUserId = userId,
            Status = TaskCompletionStatus.Completed
        };
        _mocks.TaskAssignments.Setup(r => r.GetByIdAsync(assignment.Id)).ReturnsAsync(assignment);

        await _sut.Invoking(s => s.SwapAssignmentAsync(assignment.Id, userId, Guid.NewGuid()))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region ValidateRotationOrderAsync

    [Fact]
    public async Task ValidateRotationOrder_AllValidMembers_ReturnsTrue()
    {
        var roomId = Guid.NewGuid();
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        _mocks.RoomMembers
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<RoomMember, bool>>>()))
            .ReturnsAsync(new List<RoomMember> { new RoomMember { UserId = user1, RoomId = roomId, IsActive = true } });

        var result = await _sut.ValidateRotationOrderAsync(roomId, new List<Guid> { user1, user2 });

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateRotationOrder_InvalidMember_ReturnsFalse()
    {
        var roomId = Guid.NewGuid();
        var validUser = Guid.NewGuid();
        var invalidUser = Guid.NewGuid();

        var callCount = 0;
        _mocks.RoomMembers
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<RoomMember, bool>>>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1
                    ? new List<RoomMember> { new RoomMember { UserId = validUser, RoomId = roomId, IsActive = true } }
                    : new List<RoomMember>();
            });

        var result = await _sut.ValidateRotationOrderAsync(roomId, new List<Guid> { validUser, invalidUser });

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateRotationOrder_EmptyList_ReturnsFalse()
    {
        var result = await _sut.ValidateRotationOrderAsync(Guid.NewGuid(), new List<Guid>());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateRotationOrder_NullList_ReturnsFalse()
    {
        var result = await _sut.ValidateRotationOrderAsync(Guid.NewGuid(), null!);

        result.Should().BeFalse();
    }

    #endregion

    #region MarkOverdueAssignmentsAsync

    [Fact]
    public async Task MarkOverdueAssignments_UpdatesOverdueAssignments()
    {
        var assignments = new List<TaskAssignment>
        {
            new() { Id = Guid.NewGuid(), Status = TaskCompletionStatus.Pending, DueDate = DateTime.UtcNow.AddDays(-1) },
            new() { Id = Guid.NewGuid(), Status = TaskCompletionStatus.Pending, DueDate = DateTime.UtcNow.AddDays(-2) },
        };

        _mocks.TaskTemplates
            .Setup(r => r.GetPendingOverdueAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(assignments);

        await _sut.MarkOverdueAssignmentsAsync();

        assignments.All(a => a.Status == TaskCompletionStatus.Overdue).Should().BeTrue();
        _mocks.TaskAssignments.Verify(r => r.UpdateAsync(It.IsAny<TaskAssignment>()), Times.Exactly(2));
        _mocks.UnitOfWork.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task MarkOverdueAssignments_NoOverdue_DoesNotSave()
    {
        _mocks.TaskTemplates
            .Setup(r => r.GetPendingOverdueAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TaskAssignment>());

        await _sut.MarkOverdueAssignmentsAsync();

        _mocks.UnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region AdvanceRotationAsync

    [Fact]
    public async Task AdvanceRotation_IncrementsIndexAndWraps()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var template = CreateTemplate(new List<Guid> { user1, user2 }, currentIndex: 0);

        _mocks.TaskTemplates.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        await _sut.AdvanceRotationAsync(template.Id);

        template.CurrentAssigneeIndex.Should().Be(1);
        _mocks.TaskTemplates.Verify(r => r.UpdateAsync(template), Times.Once);
    }

    [Fact]
    public async Task AdvanceRotation_AtLastIndex_WrapsToZero()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var template = CreateTemplate(new List<Guid> { user1, user2 }, currentIndex: 1);

        _mocks.TaskTemplates.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        await _sut.AdvanceRotationAsync(template.Id);

        template.CurrentAssigneeIndex.Should().Be(0);
    }

    #endregion
}
