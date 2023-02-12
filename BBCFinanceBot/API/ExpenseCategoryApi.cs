using System.Net;
using System.Net.Http.Json;
using BBCFinanceBot.Models;

namespace BBCFinanceBot.API;

public class ExpenseCategoryApi: BaseApi
{
    public ExpenseCategoryApi()
    {
        _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress!, "UserExpenseCategory/");
    }
    public async Task<List<string>> GetUserCategories(long tgUserId)
    {
        // TODO: Log it
        string errorLog = "";
        var httpResponseMessage = await _httpClient.GetAsync($"{tgUserId}");
        if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            throw new Exception("Error in api method");     // TODO: refactor

        var categories = await httpResponseMessage.Content.ReadFromJsonAsync<List<string>>()
                   ?? throw new InvalidOperationException("Conversion from Content to List<string> failed!");
        
        return categories;
    }

    public async Task<bool> PostCategory(ExpenseCategory newCategory)
    {
        var httpResponseMessage = await _httpClient.PostAsJsonAsync("", newCategory);
        return httpResponseMessage.IsSuccessStatusCode;
    }

    public async Task<bool> PutCategory(long tgUserId, Dictionary<string,string> categories)
    {
        // TODO: Log it
        string errorLog = "";
        
        var httpResponseMessage = await _httpClient.PutAsJsonAsync($"{tgUserId}", categories);
        if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            errorLog = $"У вас нет категории {categories["oldCategory"]}";

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            errorLog ="Не удалось изменить категорию...";
        }

        return httpResponseMessage.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(long tgUserId, string categoryToRemove)
    {
        // TODO: Log it
        string errorLog = "";
        var httpResponseMessage = await _httpClient.DeleteAsync(
            $"{tgUserId}/{categoryToRemove}");
        
        return httpResponseMessage.IsSuccessStatusCode;
    }
}