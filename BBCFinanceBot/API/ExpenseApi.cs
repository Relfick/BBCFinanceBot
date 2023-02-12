using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BBCFinanceBot.Models;

namespace BBCFinanceBot.API;

public class ExpenseApi : BaseApi
{
    public ExpenseApi()
    {
        _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress!, "expense/");
    }
    public async Task<List<Expense>> GetExpenses(long tgUserId)
    {
        // TODO: Log it
        string errorLog = "";

        var httpResponseMessage = await _httpClient.GetAsync($"{tgUserId}");
        if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            errorLog = "Error with null _db.Expenses";
        else if (!httpResponseMessage.IsSuccessStatusCode)
            errorLog = "Ошибка получения трат";

        return await httpResponseMessage.Content.ReadFromJsonAsync<List<Expense>>()
               ?? throw new InvalidOperationException("Conversion from Content to List<Expense> failed!");
    }

    public async Task<List<Expense>> GetExpensesWithCategory(long tgUserId, string category)
    {
        // TODO: Log it
        string errorLog = "";

        var httpResponseMessage = await _httpClient.GetAsync($"{tgUserId}/{category}");
        if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            errorLog = "Error with null _db.Expenses";
        else if (!httpResponseMessage.IsSuccessStatusCode)
            errorLog = "Error while getting expenses";

        return await httpResponseMessage.Content.ReadFromJsonAsync<List<Expense>>()
               ?? throw new InvalidOperationException("Convertation from Content to List<Expense> failed!");
    }

    public async Task<HttpResponseMessage> PostExpense(Expense expense)
    {
        var expenseJson = JsonSerializer.Serialize(expense);
        var content = new StringContent(expenseJson, Encoding.Default, "application/json");

        var httpResponseMessage = await _httpClient.PostAsync("", content);

        return httpResponseMessage;
    }
}