using System.Text;
using System.Text.RegularExpressions;
using BBCFinanceBot.API;
using BBCFinanceBot.Models;
using BBCFinanceBot.Models.DTO;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BBCFinanceBot.BotHandlers;

public class ExpenseHandler : BaseHandler
{
    public ExpenseHandler(ITelegramBotClient bot, Message message) : base(bot, message) { }

    public async Task<Message> AddExpenseHandler()
    {
        var r = new Regex(@"(?<name>(\w+\s)+)(?<cost>\d+)\s(?<category>(\w+\s*)+)", RegexOptions.Compiled);
        var m = r.Match(_message.Text!);
        if (!m.Success)
            return await Send("Используйте формат {покупка} {цена} {категория}", new ReplyKeyboardRemove());

        var expenseName = m.Result("${name}").Trim().ToLower();
        
        // If parsing failed, expenseCost = 0, so validation will fail
        int.TryParse(m.Result("${cost}"), out var expenseCost);
        
        string expenseCategoryName = m.Result("${category}").Trim().ToLower();

        List<ExpenseCategory> userCategories = await _userApi.GetExpenseCategories();
        var expenseCategory = userCategories.FirstOrDefault(uc => uc.Name == expenseCategoryName);
            
        if (expenseCategory == null)
            return await Send($"У вас нет категории {expenseCategoryName}");

        var expenseDate = DateTime.Now;

        var expense = new Expense(_tgUserId, expenseName, expenseCost, expenseCategory.Id, expenseDate);
        
        var httpResponseMessage = await _userApi.PostExpense(expense);
        string responseMessageText = "Добавили!";

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            responseMessageText = _userApi.GetErrors(httpResponseMessage);
        }

        return await Send(responseMessageText);
    }

    public async Task<Message> ExpensesCommandHandler()
    {
        List<ExpenseDTO> expenses = await _userApi.GetExpenses();
        
        if (expenses.Count == 0)
            return await Send("На данный момент у вас нет трат");
        
        // var lastDate = DateTime.Now.AddDays(-3);
        // var nowDate = DateTime.Now;
        expenses = expenses.OrderByDescending(e => e.Date).ToList();
        
        var sb = new StringBuilder();
        // sb.Append($"Ваши траты c {lastDate.ToString("dd.MM")} по {nowDate.ToString("dd.MM")}:\n\n");
        sb.Append("Ваши траты: n\n");
        foreach (var expense in expenses)
        {
            sb.Append($"{expense.Date.ToString("dd.MM")}:  ");
            sb.Append($"{expense.Cost} - ");
            sb.Append($"{expense.Name}  ");
            sb.Append($"[ {expense.ExpenseCategoryName} ]  \n");
        }

        return await Send(sb.ToString());
    }
}