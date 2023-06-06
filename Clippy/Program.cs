using Telegram.Bot;
using Telegram.Bot.Polling;

namespace Clippy
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(UpdateHandler.Bot.GetMeAsync().Result.FirstName + " is active!");

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };

            UpdateHandler.Bot.StartReceiving(
                UpdateHandler.HandleUpdateAsync,
                UpdateHandler.HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );

            Console.ReadLine();
        }
    }
}
