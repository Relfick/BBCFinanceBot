using System.Net;
using System.Text;
using BBCFinanceBot.API;
using BBCFinanceBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BBCFinanceBot.BotHandlers;

public class CategoryHandler: BaseHandler
{
    private string _messageText;
    
    public CategoryHandler(ITelegramBotClient bot, Message message) : base(bot, message)
    {
        _messageText = _message.Text ?? throw new Exception("Message.Text = Null in CategoryHandler");
    }
    
    public async Task<Message> CategoriesCommandHandler()
    {
        var categories = await _userApi.GetExpenseCategories();
        var categoryNames = categories.Select(c => c.Name).ToList();
        return await ShowCategoriesMessage(categoryNames);
    }

    private async Task<Message> ShowCategoriesMessage(List<string> userCategories)
    {
        var responseMessage = new StringBuilder();
        bool hasCategories = userCategories.Count > 0;
        
        InlineKeyboardMarkup inlineKeyboard;
        if (hasCategories)
        {
            responseMessage.Append("Текущие категории:\n\n");
            foreach (string category in userCategories)
            {
                responseMessage.Append($"    {category}\n");
            }
            
            inlineKeyboard = new InlineKeyboardMarkup(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Добавить", "category add"),
                        InlineKeyboardButton.WithCallbackData("Удалить", "category remove"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Изменить", "category edit"),
                        InlineKeyboardButton.WithCallbackData("Назад", "category back"),
                    },
                });
        }
        else
        {
            responseMessage.Append("Вы еще не добавили категории.\n\nВыберите действие:");
            
            inlineKeyboard = new InlineKeyboardMarkup(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Добавить", "category add"),
                        InlineKeyboardButton.WithCallbackData("Назад", "category back"),
                    },
                });
        }
        
        return await _bot.SendTextMessageAsync(chatId: _tgUserId, replyMarkup: inlineKeyboard,
            allowSendingWithoutReply: false, text: responseMessage.ToString());
    }

    public async Task<Message> BaseActionCategoryShowInfo(UserWorkMode workMode)
    {
        string suggest = workMode switch
        {
            UserWorkMode.AddCategory    => "добавить",
            UserWorkMode.EditCategory   => "изменить, и новое название через пробел",
            UserWorkMode.RemoveCategory => "удалить",
            _                       => throw new Exception($"UserWorkMode {workMode.ToString()} does not relate to Category")
        };
        
        bool success = await _userApi.SetWorkMode(workMode);
        Console.WriteLine(success
            ? $"поменяли режим на {workMode.ToString()}"
            : $"не поменяли режим на {workMode.ToString()} (((");
        
        return await _bot.SendTextMessageAsync(chatId: _tgUserId,
            text: $"Введите название категории, которую хотите {suggest}"); 
    }

    public async Task<Message> AddCategoryHandler()
    {
        var newCategoryText = _messageText.Trim().ToLower();

        var infoMessageId = (await Send("Пытаемся добавить...")).MessageId;
        
        var categories = await _userApi.GetExpenseCategories();
        var categoryNames = categories.Select(c => c.Name).ToList();
        if (categoryNames.Contains(newCategoryText))
            return await Edit($"У вас уже есть категория {newCategoryText}", infoMessageId);
        
        var newCategory = new ExpenseCategory(_tgUserId, newCategoryText);
        
        var httpResponseMessage = await _userApi.PostExpenseCategory(newCategory);
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            var responseText = _userApi.GetErrors(httpResponseMessage);
            return await Edit(responseText, infoMessageId);
        }

        var success = await _userApi.SetWorkMode(UserWorkMode.Default);
        if (!success)
        {
            await _userApi.DeleteExpenseCategory(newCategoryText);
            return await Edit("Не удалось добавить категорию", infoMessageId);
        }

        await Edit("Добавили категорию!", infoMessageId);

        // Show updated categories
        return await CategoriesCommandHandler();
    } 
    
    public async Task<Message> EditCategoryHandler()
    {
        _messageText = _messageText.Trim().ToLower();
        
        var splits = _messageText.Split(' ', StringSplitOptions.TrimEntries);
        if (splits.Length != 2)
            return await Send("Используйте формат {старая категория} {новая категория}");
        
        var oldCategory = splits[0];
        var newCategory = splits[1];

        if (oldCategory == newCategory)
            return await Send("Вы ввели одинаковые названия");
        
        var infoMessageId = (await Send("Пытаемся изменить...")).MessageId;
        
        var httpResponseMessage = await _userApi.PutCategory(oldCategory, newCategory);
        
        if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            return await Edit($"У вас нет категории {oldCategory}", infoMessageId);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            var responseText = _userApi.GetErrors(httpResponseMessage);
            if (responseText == string.Empty) 
                responseText = "Не удалось изменить категорию...";
            
            return await Edit(responseText, infoMessageId);
        }

        var success = await _userApi.SetWorkMode(UserWorkMode.Default);
        if (!success)
        {
            await _userApi.PutCategory(newCategory, oldCategory);
            return await Edit("Не удалось изменить категорию", infoMessageId);
        }

        await Edit("Изменили категорию!", infoMessageId);
        
        // Show updated categories
        return await CategoriesCommandHandler();
    }
    
    public async Task<Message> RemoveCategoryHandler()
    {
        var infoMessage = await Send("Пытаемся удалить...");
        string editText;
        
        var categoryToRemove = _messageText.Trim().ToLower();

        List<ExpenseCategory> categories = await _userApi.GetExpenseCategories();
        var categoryNames = categories.Select(c => c.Name).ToList();
        if (!categoryNames.Contains(categoryToRemove))
        {
            editText =
                $"У вас нет категории {categoryToRemove}. Введите название категории, которую хотите удалить:";
            return await Edit(editText, infoMessage.MessageId);
        }

        // Get existing expenses with this category
        var expensesWithCategory = await _userApi.GetExpensesWithCategory(categoryToRemove);
        var expensesCount = expensesWithCategory.Count;

        if (expensesCount == 0) 
            return await ContinueRemoveCategory(categoryToRemove, true, infoMessage.MessageId);
        
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("Да", $"continueRemoveCategory confirm {categoryToRemove}"),
            InlineKeyboardButton.WithCallbackData("Нет", $"continueRemoveCategory cancel {categoryToRemove}"),
        });
            
        editText = $"У вас есть {expensesCount} трат c категорией {categoryToRemove}. " +
                       "При удалении категории они также удалятся. Желаете продолжить?";
            
        return await Edit(editText, infoMessage.MessageId, replyMarkup: inlineKeyboard);
    }

    public async Task<Message> ContinueRemoveCategory(string categoryToRemove, bool confirm, int messageId)
    {
        await Delete(messageId);

        if (!confirm)
        {
            await _userApi.SetWorkMode(UserWorkMode.Default);
            return await Send("Не удаляем.");
        }

        var infoMessage = await Send("Удаляем..."); 
        
        bool success = await _userApi.DeleteExpenseCategory(categoryToRemove);
        if (!success)
            return await Edit("Не удалось удалить категорию...", msgId: infoMessage.MessageId);
        
        success = await _userApi.SetWorkMode(UserWorkMode.Default);
        if (!success)
            return await Edit("Категорию удалили, а воркмод вернуть не удалось...", msgId: infoMessage.MessageId);
        
        await Edit("Удалили категорию!", infoMessage.MessageId);
        
        // Show updated categories
        return await CategoriesCommandHandler();
    }
    
    public async Task<Message> BackCategoryHandler()
    {
        bool success = await _userApi.SetWorkMode(UserWorkMode.Default);
        
        if (!success) 
            return await Edit(msg: "Не удалось вернуться в стандартный режим!", msgId: _message.MessageId);
        
        await Delete(_message.MessageId);
        return await Send(msg: "Стандартный режим", replyMarkup: new ReplyKeyboardRemove());
    }
    
    public async Task<Message> UnknownCategoryHandler()
    {
        return await Send("Выбраное действие: Неизвестно");
    }
}