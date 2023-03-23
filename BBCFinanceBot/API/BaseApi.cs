using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BBCFinanceBot.db;

namespace BBCFinanceBot.API;

public abstract class BaseApi
{
    protected readonly HttpClient _httpClient;
    protected readonly bool _authSuccess;
    
    protected BaseApi(long tgUserId)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:5000/api/") };
        _authSuccess = SetBearer(tgUserId);
    }

    private bool SetBearer(long tgUserId)
    {
        using var db = new ApplicationContext();
        
        
        var userToken = db.UserTokens.Find(tgUserId);
            
        if (userToken == null) return false;
            
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", userToken.Token);
            
        return true;
    }

    public string GetErrors(HttpResponseMessage msg)
    {
        // msg.Headers.TryGetValues("FilterError", out var errors);
        // return errors == null ? new List<string>() : errors.ToList();
        
        if (msg.StatusCode == HttpStatusCode.BadRequest)
        {
            var badProps = JsonObject.Parse(msg.Content.ReadAsStringAsync().Result)!["errors"]
                .Deserialize<Dictionary<string, List<string>>>();
            
            var errorsSb = new StringBuilder();
            foreach (var item in badProps)
            {
                var propErrors = string.Join(Environment.NewLine, item.Value.ToArray());
                errorsSb.Append(propErrors + Environment.NewLine);
            }
            
            return errorsSb.ToString();
        }

        return string.Empty;
    }
}