using BBCFinanceBot.API;
using BBCFinanceBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = BBCFinanceBot.Models.User;    // Telegram.Bot.Types contains the same Class

namespace BBCFinanceBot.BotHandlers;

public class BotMessageHandler: BaseHandler
{
    public BotMessageHandler(Message? message, ITelegramBotClient bot): base(bot, message) { }

    public async Task BotOnMessageReceived()
    {
        LogMessage(_message);

        var action = _message.Text!.Split(' ')[0] switch
        {
            "/start" => StartCommandHandler(),
            "/categories" => new CategoryHandler(_bot, _message).CategoriesCommandHandler(),
            "/expenses" => new ExpenseHandler(_bot, _message).ExpensesCommandHandler(),
            "/help" => HelpCommandHandler(),
            _ => TextMessageHandler(),
        };
        await action;
    }

    private async Task<Message> HelpCommandHandler()
    {
        const string usage = "Введи траты в формате \n{Название} {Стоимость} {Категория}\n\n" +
                             "Для добавления категорий используйте команду /categories";

        return await Send(usage);
    }

    private async Task<Message> StartCommandHandler()
    {
        if (_userApi.UserTokenExists())
            return await Send("Ты уже зарегистрирован.");

        string userFirstName = _message.Chat.FirstName ?? "";
        string userUserName = _message.Chat.Username ?? "";
        var newUser = new User(_tgUserId, userFirstName, userUserName);

        bool success = await _userApi.PostUser(newUser);

        string responseMessageText = success
            ? "Привет! Ты успешно зарегистрирован!\n\n"
              // "Доступные пока команды: \n\n" +
              // "/help"
            : "Ошибка регистрации...";

        return await Send(responseMessageText);
    }

    private async Task<Message> TextMessageHandler()
    {
        var userWorkMode = await _userApi.GetWorkMode();
        if (userWorkMode == null)
            throw new NullReferenceException("Null in user.WorkMode");

        var action = userWorkMode switch
        {
            UserWorkMode.AddCategory => new CategoryHandler(_bot, _message).AddCategoryHandler(),
            UserWorkMode.EditCategory => new CategoryHandler(_bot, _message).EditCategoryHandler(),
            UserWorkMode.RemoveCategory => new CategoryHandler(_bot, _message).RemoveCategoryHandler(),
            _ => new ExpenseHandler(_bot, _message).AddExpenseHandler()
        };

        return await action;
    }

    private void LogMessage(Message message)
    {
        Console.WriteLine($"Receive message type: {message.Type}");
        if (message.Type != MessageType.Text)
            return;
        Console.WriteLine($"MessageId: {message.MessageId}");
        Console.WriteLine($"UserId: {message.From!.Id}");
        Console.WriteLine($"Message: {message.Text}");
    }
}
