using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;

namespace BudgetTracker.Data.Repositories;

public class IncomeRepository : Repository<Income>, IIncomeRepository
{
    public IncomeRepository(BudgetDbContext context) : base(context) { }
}