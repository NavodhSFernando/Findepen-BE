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
    }
}

