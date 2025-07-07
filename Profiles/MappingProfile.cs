using AutoMapper;
using FinDepen_Backend.DTOs;
using FinDepen_Backend.Entities;
using Microsoft.AspNetCore.Identity;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ✅ Map RegisterModel to ApplicationUser
        CreateMap<RegisterModel, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Email.ToUpper()))
            .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => true)) // Example: Automatically confirm email
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // UserManager will handle password hashing

        // ✅ Transaction Entity to TransactionModel DTO
        CreateMap<Transaction, TransactionModel>()
            .ForMember(dest => dest.FormattedAmount, opt => opt.MapFrom(src => Math.Round(src.Amount, 2)))
            .ForMember(dest => dest.FormattedDate, opt => opt.MapFrom(src => src.Date.ToString("MMM dd, yyyy")))
            .ForMember(dest => dest.BalanceImpact, opt => opt.MapFrom(src => src.Type == "Income" ? "+" : "-"))
            .ForMember(dest => dest.FormattedAmountWithSign, opt => opt.MapFrom(src => 
                (src.Type == "Income" ? "+" : "-") + Math.Round(src.Amount, 2).ToString("C")))
            .ForMember(dest => dest.IsIncome, opt => opt.MapFrom(src => src.Type == "Income"))
            .ForMember(dest => dest.IsExpense, opt => opt.MapFrom(src => src.Type == "Expense"));

        // ✅ CreateTransactionModel to Transaction Entity
        CreateMap<CreateTransactionModel, Transaction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Will be set by service
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Will be set by service
            .ForMember(dest => dest.User, opt => opt.Ignore()) // Don't map navigation property
            .ForMember(dest => dest.BudgetId, opt => opt.Ignore()) // Will be set by service
            .ForMember(dest => dest.Budget, opt => opt.Ignore()); // Don't map navigation property

        // ✅ UpdateTransactionModel to Transaction Entity
        CreateMap<UpdateTransactionModel, Transaction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Don't map ID
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Will be set by service
            .ForMember(dest => dest.User, opt => opt.Ignore()) // Don't map navigation property
            .ForMember(dest => dest.BudgetId, opt => opt.Ignore()) // Will be set by service
            .ForMember(dest => dest.Budget, opt => opt.Ignore()); // Don't map navigation property

        // ✅ Budget Entity to BudgetModel DTO
        CreateMap<Budget, BudgetModel>()
            .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.PlannedAmount - src.SpentAmount))
            .ForMember(dest => dest.ProgressPercentage, opt => opt.MapFrom(src => 
                src.PlannedAmount > 0 ? (src.SpentAmount / src.PlannedAmount) * 100 : 0))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 
                src.SpentAmount >= src.PlannedAmount ? "Exceeded" : 
                src.SpentAmount >= src.PlannedAmount * 0.8 ? "Warning" : "On Track"));

        // ✅ BudgetModel DTO to Budget Entity
        CreateMap<BudgetModel, Budget>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Don't map ID from DTO to entity
            .ForMember(dest => dest.User, opt => opt.Ignore()); // Don't map navigation property

        // ✅ CreateBudgetModel to Budget Entity
        CreateMap<CreateBudgetModel, Budget>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Will be set by service
            .ForMember(dest => dest.SpentAmount, opt => opt.MapFrom(src => 0.0)) // Initialize to 0
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.RenewalFrequency, opt => opt.MapFrom(src => src.RenewalFrequency))
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Will be set by controller
            .ForMember(dest => dest.User, opt => opt.Ignore()); // Don't map navigation property

        // ✅ UpdateBudgetModel to Budget Entity
        CreateMap<UpdateBudgetModel, Budget>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Don't map ID
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.RenewalFrequency, opt => opt.MapFrom(src => src.RenewalFrequency))
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Will be set by controller
            .ForMember(dest => dest.User, opt => opt.Ignore()); // Don't map navigation property

        // ✅ Budget Entity to CreateBudgetModel (for potential reverse mapping)
        CreateMap<Budget, CreateBudgetModel>()
            .ForMember(dest => dest.Reminder, opt => opt.MapFrom(src => src.Reminder));

        // ✅ Budget Entity to UpdateBudgetModel (for potential reverse mapping)
        CreateMap<Budget, UpdateBudgetModel>();

        // ✅ Goal Entity to GoalModel DTO
        CreateMap<Goal, GoalModel>()
            .ForMember(dest => dest.ProgressPercentage, opt => opt.MapFrom(src => 
                src.TargetAmount > 0 ? Math.Min((src.CurrentAmount / src.TargetAmount) * 100, 100) : 0))
            .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => 
                Math.Max(src.TargetAmount - src.CurrentAmount, 0)))
            .ForMember(dest => dest.DaysRemaining, opt => opt.MapFrom(src => 
                Math.Max((src.TargetDate - DateTime.UtcNow).Days, 0)))
            .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => 
                DateTime.UtcNow > src.TargetDate && src.CurrentAmount < src.TargetAmount))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => 
                src.CurrentAmount >= src.TargetAmount))
            .ForMember(dest => dest.ProgressStatus, opt => opt.MapFrom(src => 
                src.CurrentAmount >= src.TargetAmount ? "Completed" :
                DateTime.UtcNow > src.TargetDate && src.CurrentAmount < src.TargetAmount ? "Overdue" :
                (src.CurrentAmount / src.TargetAmount) * 100 >= 80 ? "Near Completion" :
                (src.CurrentAmount / src.TargetAmount) * 100 >= 50 ? "Good Progress" :
                (src.CurrentAmount / src.TargetAmount) * 100 >= 25 ? "In Progress" : "Just Started"));

        // ✅ GoalModel DTO to Goal Entity
        CreateMap<GoalModel, Goal>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Don't map ID from DTO to entity
            .ForMember(dest => dest.User, opt => opt.Ignore()); // Don't map navigation property

        // ✅ CreateGoalModel to Goal Entity
        CreateMap<CreateGoalModel, Goal>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Will be set by service
            .ForMember(dest => dest.CurrentAmount, opt => opt.MapFrom(src => 0.0)) // Initialize to 0
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => FinDepen_Backend.Constants.GoalStatus.Active))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Will be set by controller
            .ForMember(dest => dest.User, opt => opt.Ignore()); // Don't map navigation property

        // ✅ UpdateGoalModel to Goal Entity
        CreateMap<UpdateGoalModel, Goal>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Don't map ID
            .ForMember(dest => dest.LastUpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Will be set by controller
            .ForMember(dest => dest.User, opt => opt.Ignore()); // Don't map navigation property

        // ✅ Goal Entity to CreateGoalModel (for potential reverse mapping)
        CreateMap<Goal, CreateGoalModel>();

        // ✅ Goal Entity to UpdateGoalModel (for potential reverse mapping)
        CreateMap<Goal, UpdateGoalModel>();
    }
}

