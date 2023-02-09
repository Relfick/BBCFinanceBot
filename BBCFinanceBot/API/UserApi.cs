using System.Net;
using System.Net.Http.Json;
using BBCFinanceBot.Models;
using Telegram.Bot.Types;
using User = BBCFinanceBot.Models.User;    // Telegram.Bot.Types contains the same Class

namespace BBCFinanceBot.API;

public class UserApi
{
    private readonly HttpClient _httpClient;

    public UserApi()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://localhost:7166/");
    } 
    
    public async Task<bool> UserExists(long tgUserId)
    {
        var httpResponseMessage = await _httpClient.GetAsync($"api/User/{tgUserId}");
        return httpResponseMessage.IsSuccessStatusCode;
    }
    
    public async Task<bool> PostUser(User newUser)
    {
        var httpResponseMessage = await _httpClient.PostAsJsonAsync("api/User", newUser);
        return httpResponseMessage.IsSuccessStatusCode;
    }

    public async Task<bool> SetWorkMode(long tgUserId, UserWorkMode workMode)
    {
        // TODO: log it
        string errorLog = "";

        var httpResponseMessage = await _httpClient.PutAsJsonAsync($"api/User/workmode/{tgUserId}", workMode);
        
        return httpResponseMessage.IsSuccessStatusCode;
    }
    
    public async Task<UserWorkMode?> GetWorkMode(long tgUserId)
    {
        // TODO: log it
        string errorLog = "";
        
        var httpResponseMessage = await _httpClient.GetAsync($"api/User/{tgUserId}");
        if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            return null;

        User? user = await httpResponseMessage.Content.ReadFromJsonAsync<User>();
        return user?.WorkMode;
    } 
}