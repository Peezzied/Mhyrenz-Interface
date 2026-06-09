using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Services;

namespace Mhyrenz_Interface.Database.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _botToken;
        private readonly string _chatId;

        public TelegramBotService(string botToken, string chatId)
        {
            _botToken = botToken;
            _chatId = chatId;
        }

        public async Task SendMessage(string message)
        {
            var url =
                $"https://api.telegram.org/bot{_botToken}/sendMessage";

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("chat_id", _chatId),
                new KeyValuePair<string, string>("text", message)
            });

            var response = await _httpClient.PostAsync(url, content);

            response.EnsureSuccessStatusCode();
        }
    }
}
