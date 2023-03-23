using System.ComponentModel.DataAnnotations;

namespace BBCFinanceBot.db;

public class UserToken
{
    [Required]
    public long Id { get; set; }
    [Required]
    public string Token { get; set; }

    public UserToken(long id, string token)
    {
        Id = id;
        Token = token;
    }
}