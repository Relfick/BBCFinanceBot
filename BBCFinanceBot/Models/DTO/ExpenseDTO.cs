namespace BBCFinanceBot.Models.DTO;

public record ExpenseDTO
{
    public string ExpenseCategoryName { get; init; }
    public string Name { get; init; }
    public int Cost { get; init; }
    public DateTime Date { get; init; }
}