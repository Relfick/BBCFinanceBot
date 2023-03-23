namespace BBCFinanceBot.Models;

public class Expense
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public int ExpenseCategoryId { get; set; }
    public string Name { get; set; }
    public int Cost { get; set; }
    public DateTime Date { get; set; }
    
    public Expense(long userId, string name, int cost, int expenseCategoryId, DateTime date)
    {
        UserId = userId;
        Name = name;
        Cost = cost;
        ExpenseCategoryId = expenseCategoryId;
        Date = date;
    }
}