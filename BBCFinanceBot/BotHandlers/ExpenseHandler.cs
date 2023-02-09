using System.Text;
using System.Text.RegularExpressions;
using BBCFinanceBot.API;
using BBCFinanceBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BBCFinanceBot.BotHandlers;

public class ExpenseHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly Message _message;
    private readonly long _tgUserId;
    private readonly ExpenseApi _expenseApi;
    private readonly ExpenseCategoryApi _categoryApi;

    public ExpenseHandler(ITelegramBotClient bot, Message message)
    {
        _bot = bot;
        _message = message;
        _tgUserId = message.Chat.Id;
        _expenseApi = new ExpenseApi();
        _categoryApi = new ExpenseCategoryApi();
    }

    public async Task<Message> AddExpenseHandler()
    {
        var r = new Regex(@"(?<name>(\w+\s)+)(?<cost>\d+)\s(?<category>(\w+\s*)+)", RegexOptions.Compiled);
        var m = r.Match(_message.Text!);
        if (!m.Success)
            return await _bot.SendTextMessageAsync(chatId: _tgUserId, replyMarkup: new ReplyKeyboardRemove(),
                text: "Используйте формат {покупка} {цена} {категория}");

        var expenseName = m.Result("${name}").Trim().ToLower();
        var expenseCost = int.Parse(m.Result("${cost}"));
        string expenseCategory = m.Result("${category}").Trim().ToLower();

        var userCategories = await _categoryApi.GetUserCategories(_tgUserId);
        if (!userCategories.Contains(expenseCategory))
            return await _bot.SendTextMessageAsync(chatId: _tgUserId,
                text: $"У вас нет категории {expenseCategory}");

        var expenseDate = DateTime.Now;

        var expense = new Expense(_tgUserId, expenseName, expenseCost, expenseCategory, expenseDate);
        
        var httpResponseMessage = await _expenseApi.PostExpense(expense);
        string responseMessageText = "Добавили!";

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            var errors = _expenseApi.GetErrors(httpResponseMessage);
            var errorsSb = new StringBuilder();
            foreach (var errorText in errors)
            {
                errorsSb.Append(errorText + Environment.NewLine);
            }

            responseMessageText = errorsSb.ToString();
        }

        return await _bot.SendTextMessageAsync(chatId: _tgUserId,
            text: responseMessageText);
    }

    public async Task<Message> ExpenseCommandHandler()
    {
        List<Expense> expenses = await _expenseApi.GetExpenses(_tgUserId);
        
        if (expenses.Count == 0)
            return await _bot.SendTextMessageAsync(chatId: _tgUserId,
                text: "На данный момент у вас нет трат");
        
        // var lastDate = DateTime.Now.AddDays(-3);
        // var nowDate = DateTime.Now;
        var expensesLinq = 
            from expense in expenses
            // where lastDate < expense.Date && expense.Date < nowDate
            orderby expense.Date descending
            select expense;

        expenses = expensesLinq.ToList();
        
        var sb = new StringBuilder();
        // sb.Append($"Ваши траты c {lastDate.ToString("dd.MM")} по {nowDate.ToString("dd.MM")}:\n\n");
        sb.Append("Ваши траты: n\n");
        foreach (var expense in expenses)
        {
            sb.Append($"{expense.Date.ToString("dd.MM")}:  ");
            sb.Append($"{expense.Cost} - ");
            sb.Append($"{expense.Name}  ");
            sb.Append($"[ {expense.ExpenseCategory} ]  \n");
        }

        return await _bot.SendTextMessageAsync(chatId: _tgUserId,
            text: sb.ToString());
    }
}