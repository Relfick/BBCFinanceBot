﻿namespace BBCFinanceBot.Models;

public class User
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string Username { get; set; }
    public UserWorkMode WorkMode { get; set; }
    
    public User(long id, string firstName, string username, UserWorkMode workMode = UserWorkMode.Default)
    {
        Id = id;
        FirstName = firstName;
        Username = username;
        WorkMode = workMode;
    }
}

public enum UserWorkMode
{
    EditCategory,
    AddCategory,
    RemoveCategory,
    Default
}