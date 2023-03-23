using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BBCFinanceBot.Models;

namespace BBCFinanceBot.API;

public class ExpenseApi : BaseApi
{
    private readonly Dictionary<string, string> _propertyErrors = new()
    {
        { "Name", "" },
    };
    
    public ExpenseApi(long tgUserId): base(tgUserId)
    {
        _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress!, "expense/");
    }
}