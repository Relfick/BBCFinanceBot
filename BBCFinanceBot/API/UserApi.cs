using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BBCFinanceBot.db;
using BBCFinanceBot.Models;
using BBCFinanceBot.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using User = BBCFinanceBot.Models.User;    // Telegram.Bot.Types contains the same Class

namespace BBCFinanceBot.API;

public class UserApi: BaseApi
{
    public UserApi(long tgUserId): base(tgUserId)
    {
        _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress!, "user/");
    }
    
    public bool UserTokenExists()
    {
        return _authSuccess;
    }
    
    private async Task<bool> SaveToken(HttpResponseMessage httpResponseMessage, long tgUserId)
    {
        await using var db = new ApplicationContext();

        string token = await httpResponseMessage.Content.ReadAsStringAsync();
        var userToken = new UserToken(tgUserId, token);
        db.UserTokens.Add(userToken);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            Console.WriteLine(e.Message);
            return false;
        }

        return true;
    } 
    
    public async Task<bool> PostUser(User newUser)
    {
        var httpResponseMessage = await _httpClient.PostAsJsonAsync("", newUser);
        if (httpResponseMessage.StatusCode == HttpStatusCode.Conflict)
            return false;
        
        var tgUserId = newUser.Id;
        var successSaveToken = await SaveToken(httpResponseMessage, tgUserId);
        return httpResponseMessage.IsSuccessStatusCode && successSaveToken;
    }

    public async Task<List<ExpenseCategory>> GetExpenseCategories()
    {
        var httpResponseMessage = await _httpClient.GetAsync("expenseCategories");
        if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            throw new Exception("Unauthorized");
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            throw new Exception("Error in api method");     // TODO: clarify
        }
        
        var categories = await httpResponseMessage.Content.ReadFromJsonAsync<List<ExpenseCategory>>()
                         ?? throw new InvalidOperationException("Conversion from Content to List<ExpenseCategory> failed!");

        // var strCategories = categories.Select(ec => ec.Name).ToList();
        
        return categories;
    }
    
    public async Task<HttpResponseMessage> PostExpenseCategory(ExpenseCategory newCategory)
    {
        var httpResponseMessage = await _httpClient.PostAsJsonAsync("ExpenseCategory", newCategory);
        return httpResponseMessage;
    }
    
    public async Task<HttpResponseMessage> PutCategory(string oldCategory, string newCategory)
    {
        var httpResponseMessage = await _httpClient.GetAsync($"ExpenseCategory/{oldCategory}");
        if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
        {
            Console.WriteLine($"Category {oldCategory} not found");
            return httpResponseMessage;
        }
        else if (!httpResponseMessage.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error in GetAsync(ExpenseCategory{oldCategory})");
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
        
        httpResponseMessage = await _httpClient.PutAsJsonAsync($"ExpenseCategory/{existingCategory.Id}", existingCategory);
        return httpResponseMessage;
    }
    
    public async Task<bool> DeleteExpenseCategory(string categoryToRemove)
    {
        var httpResponseMessage = await _httpClient.DeleteAsync($"ExpenseCategory/{categoryToRemove}");
        
        return httpResponseMessage.IsSuccessStatusCode;
    }
    
    public async Task<List<ExpenseDTO>> GetExpensesWithCategory(string categoryName)
    {
        string errorLog = "";
        string uri = $"Expenses?categoryName={categoryName}";
        
        var httpResponseMessage = await _httpClient.GetAsync(uri);
        if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            errorLog = "Error with null _db.Expenses";
        else if (!httpResponseMessage.IsSuccessStatusCode)
            errorLog = "Error while getting expenses";

        var expenses = await httpResponseMessage.Content.ReadFromJsonAsync<List<ExpenseDTO>>();
        return expenses!;
    }

    public async Task<HttpResponseMessage> PostExpense(Expense expense)
    {
        var expenseJson = JsonSerializer.Serialize(expense);
        var content = new StringContent(expenseJson, Encoding.Default, "application/json");

        var httpResponseMessage = await _httpClient.PostAsync("Expense", content);
        return httpResponseMessage;
    }
    
    public async Task<List<ExpenseDTO>> GetExpenses()
    {
        string errorLog = "";

        var httpResponseMessage = await _httpClient.GetAsync("Expenses");
        if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            throw new Exception("Unauthorized");
        if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            errorLog = "Error with null _db.Expenses";
        else if (!httpResponseMessage.IsSuccessStatusCode)
            errorLog = "Ошибка получения трат";

        return await httpResponseMessage.Content.ReadFromJsonAsync<List<ExpenseDTO>>()
               ?? throw new InvalidOperationException("Conversion from Content to List<ExpenseDTO> failed!");
    }
    
    public async Task<bool> SetWorkMode(UserWorkMode workMode)
    {
        var httpResponseMessage = await _httpClient.PutAsJsonAsync("workmode", workMode);
        
        return httpResponseMessage.IsSuccessStatusCode;
    }
    
    public async Task<UserWorkMode?> GetWorkMode()
    {
        var httpResponseMessage = await _httpClient.GetAsync($"workmode");
        if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            return null;

        var workMode= await httpResponseMessage.Content.ReadFromJsonAsync<UserWorkMode>();
        return workMode;
    } 
}