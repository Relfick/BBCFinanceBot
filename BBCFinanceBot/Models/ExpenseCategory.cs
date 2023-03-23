namespace BBCFinanceBot.Models;

public class ExpenseCategory
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; }
    
    public ExpenseCategory(long userId, string name)
    {
        UserId = userId;
        Name = name;
    }
}