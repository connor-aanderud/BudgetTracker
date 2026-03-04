using AutoMapper;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Domain.Entities;

namespace BudgetTracker.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Statement, StatementDto>()
            .ForMember(d => d.TransactionCount, opt => opt.MapFrom(s => s.Transactions.Count));

        CreateMap<Transaction, TransactionDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.MerchantName, opt => opt.MapFrom(s => s.Merchant != null ? s.Merchant.NormalizedName : null))
            .ForMember(d => d.StatementFileName, opt => opt.MapFrom(s => s.Statement.FileName));

        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.TransactionCount, opt => opt.MapFrom(s => s.Transactions.Count))
            .ForMember(d => d.MerchantCount, opt => opt.MapFrom(s => s.Merchants.Count));

        CreateMap<Merchant, MerchantDto>()
            .ForMember(d => d.MatchType, opt => opt.MapFrom(s => s.MatchType.ToString()))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name));

        CreateMap<Income, IncomeDto>()
            .ForMember(d => d.Frequency, opt => opt.MapFrom(s => s.Frequency != null ? s.Frequency.ToString() : null));

        CreateMap<Budget, BudgetDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
            .ForMember(d => d.CategoryColor, opt => opt.MapFrom(s => s.Category.Color));
    }
}