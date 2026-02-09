using AutoMapper;
using RoomSplit.API.DTOs.Auth;
using RoomSplit.API.DTOs.Budget;
using RoomSplit.API.DTOs.Expense;
using RoomSplit.API.DTOs.Room;
using RoomSplit.API.DTOs.Task;
using RoomSplit.API.DTOs.Transaction;
using RoomSplit.Core.Entities;

namespace RoomSplit.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User
        CreateMap<User, UserDto>();

        // Room
        CreateMap<Room, RoomDto>()
            .ForMember(d => d.MemberCount, opt => opt.MapFrom(s => s.Members.Count));
        CreateMap<Room, RoomDetailDto>();
        CreateMap<RoomMember, RoomMemberDto>()
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()));

        // SharedExpense
        CreateMap<SharedExpense, ExpenseDto>()
            .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.SplitType, opt => opt.MapFrom(s => s.SplitType.ToString()));
        CreateMap<ExpenseSplit, ExpenseSplitDto>();

        // TaskTemplate
        CreateMap<TaskTemplate, TaskDto>()
            .ForMember(d => d.FrequencyType, opt => opt.MapFrom(s => s.FrequencyType.ToString()));
        CreateMap<TaskAssignment, TaskAssignmentDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        // Transaction
        CreateMap<Transaction, TransactionDto>()
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.IncomeCategory, opt => opt.MapFrom(s => s.IncomeCategory != null ? s.IncomeCategory.ToString() : null))
            .ForMember(d => d.ExpenseCategory, opt => opt.MapFrom(s => s.ExpenseCategory != null ? s.ExpenseCategory.ToString() : null));

        // Budget
        CreateMap<Budget, BudgetDto>()
            .ForMember(d => d.ExpenseCategory, opt => opt.MapFrom(s => s.ExpenseCategory.ToString()))
            .ForMember(d => d.SpentAmount, opt => opt.Ignore());
    }
}
