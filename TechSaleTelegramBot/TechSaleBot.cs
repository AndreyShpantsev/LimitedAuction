using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace TechSaleTelegramBot
{
    public class TechSaleBot : IBot
    {
        public IBotHost Host { get; set; }

        private readonly TelegramBotClient telegramBot;

        public TechSaleBot(string token)
        {
            telegramBot = new TelegramBotClient(token);
            telegramBot.OnMessage += Bot_OnMessage;
            telegramBot.StartReceiving();
        }

        public async Task SendMessage(string msg, string chatId)
        {
            await telegramBot.SendTextMessageAsync(chatId, msg);
        }

        private async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text.Contains(@"/start"))
            {
                if (Host != null)
                {
                    await Host.RespondToMessage(e.Message.Chat.Id.ToString(), e.Message.From.Username);
                }
            }
        }
    }
}
