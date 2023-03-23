using BBCFinanceBot.API;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BBCFinanceBot.BotHandlers;

public abstract class BaseHandler
{
    protected readonly ITelegramBotClient _bot;
    protected readonly Message _message;
    protected readonly long _tgUserId;
    protected readonly UserApi _userApi;
    
    protected BaseHandler(ITelegramBotClient bot, Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        
        _bot = bot;
        _message = message;
        _tgUserId = message.Chat.Id;
        
        _userApi = new UserApi(message.Chat.Id);
    }
    
    protected async Task<Message> Send(string msg, IReplyMarkup? replyMarkup = null)
    {
        return await _bot.SendTextMessageAsync(chatId: _tgUserId,
            text: msg, replyMarkup: replyMarkup);
    }
    
    protected async Task<Message> Edit(string msg, int msgId, InlineKeyboardMarkup? replyMarkup = null)
    {
        return await _bot.EditMessageTextAsync(chatId: _tgUserId, messageId: msgId,
            text: msg, replyMarkup: replyMarkup);
    }
    
    protected async Task Delete(int msgId)
    {
        await _bot.DeleteMessageAsync(chatId: _tgUserId, messageId: msgId);
    } 
}