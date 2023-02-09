using BBCFinanceBot.BotHandlers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

namespace BBCFinanceBot;

class Program
{
    private static string _token = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build()
        ["BotToken"]!;

    private static readonly ITelegramBotClient Bot = new TelegramBotClient(_token);

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var handler = update.Type switch
        {
            UpdateType.Message            => new BotMessageHandler(update.Message, botClient).BotOnMessageReceived(),
            UpdateType.CallbackQuery      => new BotCallbackQueryHandler(botClient, update.CallbackQuery!).OnCallbackQueryReceived(),
            _                             => UnknownUpdateHandlerAsync(update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandlePollingErrorAsync(botClient, exception, cancellationToken);
        }
    }

    private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
    
    public static Task UnknownUpdateHandlerAsync(Update update)
    {
        Console.WriteLine($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }

    static async Task Main(string[] args)
    {
        var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>(), // receive all update types
            ThrowPendingUpdates = true,
        };
        try
        {
            await Bot.SetMyCommandsAsync(new List<BotCommand>
            {
                new() { Command = "/help", Description = "Справка" },
                new() { Command = "/categories", Description = "Редактирование категорий трат" },
                new() { Command = "/expenses", Description = "Получение списка трат" },
            }, cancellationToken: cts.Token);

            Bot.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );
            var me = await Bot.GetMeAsync(cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
            
            await Bot.SendTextMessageAsync(chatId: 321276694, text: "Здарова-здарова");
            Console.ReadLine();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            // cts.Cancel();
        }
    }
}