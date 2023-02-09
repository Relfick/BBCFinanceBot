namespace BBCFinanceBot.Models;

public class ExpenseCategory
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public string Category { get; set; }
    
    public ExpenseCategory(long userId, string category)
    {
        UserId = userId;
        Category = category;
    }
}