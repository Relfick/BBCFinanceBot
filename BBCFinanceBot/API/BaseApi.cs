namespace BBCFinanceBot.API;

public class BaseApi
{
    protected readonly HttpClient _httpClient;

    protected BaseApi()
    {
        _httpClient = new HttpClient() { BaseAddress = new Uri("http://185.231.206.160/") };
    }

    public List<string> GetErrors(HttpResponseMessage msg)
    {
        msg.Headers.TryGetValues("FilterError", out var errors);
        return errors == null ? new List<string>() : errors.ToList();
    }
}