namespace BBCFinanceBot.API;

public class BaseApi
{
    protected readonly HttpClient _httpClient;

    protected BaseApi()
    {
        _httpClient = new HttpClient() { BaseAddress = new Uri("https://localhost:7166/") };
    }

    public List<string> GetErrors(HttpResponseMessage msg)
    {
        msg.Headers.TryGetValues("FilterError", out var errors);
        return errors == null ? new List<string>() : errors.ToList();
    }
}