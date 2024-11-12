using System.Text;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;


namespace bot
{

    public class GrammarCheckResponse
    {
        public bool status { get; set; }
        public GrammarResponse response { get; set; }
    }

    public class GrammarResponse
    {
        public string corrected { get; set; }
    }

    public class TextGearsClient
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string apiKey;

        public TextGearsClient(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public async Task<GrammarCheckResponse> CheckGrammarAsync(string text)
        {
            string url = "https://api.textgears.com/correct";

            var requestBody = new
            {
                text = text,
                key = apiKey
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            

            var responseContent = await response.Content.ReadAsStringAsync();
                
            return JsonConvert.DeserializeObject<GrammarCheckResponse>(responseContent); 
        }
    }
    class Program
    {
        private static TelegramBotClient botClient;
        private static TextGearsClient textGearsClient;

        static async Task Main(string[] args)
        {
            string telegramToken = "7617929535:AAFHoWUOkPcjY3n0b_c0hecJqWcS6IY8q1o";
            string apiKey = "LFhjSZJ7YPYqXF9E";

            botClient = new TelegramBotClient(telegramToken);
            textGearsClient = new TextGearsClient(apiKey);

            botClient.OnMessage += BotClient_OnMessage;
            botClient.StartReceiving();

            Console.WriteLine("Bot is running...");
            Console.ReadLine();
            botClient.StopReceiving();
        }

        private static async void BotClient_OnMessage(object sender, MessageEventArgs e)
        {

            if (e.Message.Text != null)
            {
                string replyMessage;
                if (e.Message.Text == "/start" || e.Message.Text == "/help")
                {
                    replyMessage = "This bot is designed to correct errors in English text.\nWrite the text and the bot will send the corrected text in response.";
                }
                else
                {
                    var response = await textGearsClient.CheckGrammarAsync(e.Message.Text);

                    if (response.status)
                    {
                        replyMessage = "Grammar check successful!\n" + response.response.corrected;
                    }
                    else
                    {
                        replyMessage = "\"Grammar check failed: \"\n";
                    }
                }
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, replyMessage, replyMarkup: new ReplyKeyboardMarkup(new KeyboardButton("/help")));
            }
        }
    }
}