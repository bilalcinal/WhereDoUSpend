using FinanceTracker.Domain;

namespace FinanceTracker.Application.Accounts;

public record AccountCreateDto(string Name, AccountType Type, string Currency, decimal OpeningBalance);
public record AccountVm(int Id, string Name, AccountType Type, string Currency, decimal OpeningBalance, bool IsArchived); 