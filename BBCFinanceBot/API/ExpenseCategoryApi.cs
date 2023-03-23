using System.Net;
using System.Net.Http.Json;
using BBCFinanceBot.db;
using BBCFinanceBot.Models;

namespace BBCFinanceBot.API;

public class ExpenseCategoryApi: BaseApi
{
    public ExpenseCategoryApi(long tgUserId): base(tgUserId)
    {
        _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress!, "ExpenseCategory");
    }
    public async Task<List<string>> GetUserCategories()
    {
        // TODO: Log it
        string errorLog = "";
        
        var httpResponseMessage = await _httpClient.GetAsync("");
        if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            throw new Exception("Unauthorized");
        if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            throw new Exception("Error in api method");     // TODO: refactor

        var categories = await httpResponseMessage.Content.ReadFromJsonAsync<List<string>>()
                   ?? throw new InvalidOperationException("Conversion from Content to List<string> failed!");
        
        return categories;
    }

    public async Task<HttpResponseMessage> PostCategory(ExpenseCategory newCategory)
    {
        var httpResponseMessage = await _httpClient.PostAsJsonAsync("", newCategory);
        return httpResponseMessage;
    }

    public async Task<HttpResponseMessage> PutCategory(long tgUserId, string oldCategory, string newCategory)
    {
        var httpResponseMessage = await _httpClient.GetAsync($"{tgUserId}/{oldCategory}");
        if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
        {
            Console.WriteLine($"Category {oldCategory} not found");
            return httpResponseMessage;
        }
        else if (!httpResponseMessage.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error in GetAsync({tgUserId}/{oldCategory})");
            return httpResponseMessage;
        }
        
        var existingCategory = await httpResponseMessage.Content.ReadFromJsonAsync<ExpenseCategory>();
        if (existingCategory == null)
        {
            Console.WriteLine($"Cannot deserialize existing expense category");
            httpResponseMessage.StatusCode = HttpStatusCode.NotAcceptable;
            return httpResponseMessage;
        }

        existingCategory.Name = newCategory;
        
        httpResponseMessage = await _httpClient.PutAsJsonAsync($"{existingCategory.Id}", existingCategory);
        return httpResponseMessage;
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