using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Types;
using static Clippy.UserStateChanger;

namespace Clippy
{
    internal static class UpdateHandler
    {
        private static ITelegramBotClient bot = new TelegramBotClient("YOUR_TOKEN");
        private static DriveHandler driveHandler = new DriveHandler();
        private static int messageId = 0;

        public static ITelegramBotClient Bot
        {
            get
            {
                return bot;
            }
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            var message = update.Message;

            if (update.Message == null)
            {
                message = update.CallbackQuery?.Message;
            }

            if (!string.IsNullOrEmpty(message.Text) && string.Equals(message.Text, "/start", StringComparison.OrdinalIgnoreCase))
            {
                await HandleStartCommand(botClient, message);
            }
            else if (!string.IsNullOrEmpty(message.Text) && string.Equals(message.Text, "/cancel", StringComparison.OrdinalIgnoreCase) && UserStateChanger.GetUserState(message.Chat.Id) != UserStateChanger.UserState.None)
            {
                await HandleCancelCommand(botClient, message);
            }
            else if (!string.IsNullOrEmpty(message.Text) && string.Equals(message.Text, "/savefile", StringComparison.OrdinalIgnoreCase))
            {
                await HandleSaveFileCommand(botClient, message);
            }
            else if (UserStateChanger.GetUserState(message.Chat.Id) == UserStateChanger.UserState.SaveFile)
            {
                await ExecuteSaveFileCommand(botClient, message);
            }
            else if (!string.IsNullOrEmpty(message.Text) && string.Equals(message.Text, "/getfile", StringComparison.OrdinalIgnoreCase))
            {
                await HandleGetFileCommand(botClient, message);
            }
            else if (UserStateChanger.GetUserState(message.Chat.Id) == UserStateChanger.UserState.DownloadFile && update.CallbackQuery != null)
            {
                await ExecuteGetFileCommand(botClient, message, update);
            }
            else if (!string.IsNullOrEmpty(message.Text) && string.Equals(message.Text, "/deletefile", StringComparison.OrdinalIgnoreCase))
            {
                await HandleDeleteFileCommand(botClient, message);
            }
            else if (UserStateChanger.GetUserState(message.Chat.Id) == UserStateChanger.UserState.DeleteFile && update.CallbackQuery != null)
            {
                await ExecuteDeleteFileCommand(botClient, message, update);
            }
            else if (!string.IsNullOrEmpty(message.Text) && string.Equals(message.Text, "/eraseallfiles", StringComparison.OrdinalIgnoreCase))
            {
                await HandleEraseAllFilesCommand(botClient, message);
            }
            else if (UserStateChanger.GetUserState(message.Chat.Id) == UserStateChanger.UserState.EraseAllData && update.CallbackQuery != null)
            {
                await ExecuteEraseAllFilesCommand(botClient, message, update);
            }
            else if (!string.IsNullOrEmpty(message.Text) && string.Equals(message.Text, "/getdownloadurl", StringComparison.OrdinalIgnoreCase))
            {
                await HandleGetDownloadUrlCommand(botClient, message);
            }
            else if (UserStateChanger.GetUserState(message.Chat.Id) == UserStateChanger.UserState.GetDownloadUrl && update.CallbackQuery != null)
            {
                await ExecuteGetDownloadUrlCommand(botClient, message, update);
            }
            else if (!string.IsNullOrEmpty(message.Text) && string.Equals(message.Text, "/getviewurl", StringComparison.OrdinalIgnoreCase))
            {
                await HandleGetViewUrlCommand(botClient, message);
            }
            else if (UserStateChanger.GetUserState(message.Chat.Id) == UserStateChanger.UserState.GetViewUrl && update.CallbackQuery != null)
            {
                await ExecuteGetViewUrlCommand(botClient, message, update);
            }
            else if (!string.IsNullOrEmpty(message.Text) && string.Equals(message.Text, "/deleteuser", StringComparison.OrdinalIgnoreCase))
            {
                await HandleDeleteUserCommand(botClient, message);
            }
            else if (UserStateChanger.GetUserState(message.Chat.Id) == UserStateChanger.UserState.DeleteUser && update.CallbackQuery != null)
            {
                await ExecuteDeleteUserCommand(botClient, message, update);
            }
            else
            {
                await HandleUnknownCommand(botClient, message);
            }
        }

        private static async Task HandleStartCommand(ITelegramBotClient botClient, Message message)
        {
            await CheckIfInlineButtonsWereSentBefore(botClient, message);

            bool check = await driveHandler.CheckIfUserFolderExistsOnGoogleDrive(botClient, message);

            if (check)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Welcome back, {message.From.FirstName}!");
                return;
            }

            await botClient.SendTextMessageAsync(message.Chat.Id, $"Hi, {message.From.FirstName}, my name is Clippy!\nI was developed to simplify your job with files");
        }

        private static async Task HandleCancelCommand(ITelegramBotClient botClient, Message message)
        {
            await CheckIfInlineButtonsWereSentBefore(botClient, message);

            if (messageId != 0)
            {
                await botClient.DeleteMessageAsync(message.Chat.Id, messageId);
            }

            await botClient.SendTextMessageAsync(message.Chat.Id, "Okay, the last operation is canceled now!");
            UpdateUserState(message.From.Id, UserState.None);
        }

        private static async Task HandleSaveFileCommand(ITelegramBotClient botClient, Message message)
        {
            await CheckIfInlineButtonsWereSentBefore(botClient, message);

            bool check = await driveHandler.CheckIfUserFolderExistsOnGoogleDrive(botClient, message);

            if (!check)
            {
                await HandleAccountCreation(botClient, message);
                return;
            }

            await botClient.SendTextMessageAsync(message.Chat.Id, "Okay, send me your file!\nPromise, I will keep it in a safe place.");
            UpdateUserState(message.From.Id, UserState.SaveFile);
        }

        private static async Task HandleGetFileCommand(ITelegramBotClient botClient, Message message)
        {
            await CheckIfInlineButtonsWereSentBefore(botClient, message);

            bool check = await driveHandler.CheckIfUserFolderExistsOnGoogleDrive(botClient, message);

            if (!check)
            {
                await HandleAccountCreation(botClient, message);
                return;
            }

            var files = await driveHandler.GetFileNames(botClient, message);

            if (files.Count == 0)
            {
                await HandleUserFileQuantityCheck(botClient, message);
                return;
            }

            messageId = await driveHandler.SendUserFilesAsInlineButtons(botClient, message, files);
            UserStateChanger.UpdateUserState(message.From.Id, UserStateChanger.UserState.DownloadFile);
        }

        private static async Task HandleDeleteFileCommand(ITelegramBotClient botClient, Message message)
        {
            await CheckIfInlineButtonsWereSentBefore(botClient, message);

            bool check = await driveHandler.CheckIfUserFolderExistsOnGoogleDrive(botClient, message);

            if (!check)
            {
                await HandleAccountCreation(botClient, message);
                return;
            }

            var files = await driveHandler.GetFileNames(botClient, message);

            if (files.Count == 0)
            {
                await HandleUserFileQuantityCheck(botClient, message);
                return;
            }

            messageId = await driveHandler.SendUserFilesAsInlineButtons(botClient, message, files);
            UserStateChanger.UpdateUserState(message.From.Id, UserStateChanger.UserState.DeleteFile);
        }

        private static async Task HandleUnknownCommand(ITelegramBotClient botClient, Message message)
        {
            await CheckIfInlineButtonsWereSentBefore(botClient, message);
            await botClient.SendTextMessageAsync(message.Chat.Id, "Error occurred!");
            UpdateUserState(message.From.Id, UserState.None);
        }

        private static async Task HandleEraseAllFilesCommand(ITelegramBotClient botClient, Message message)
        {
            await CheckIfInlineButtonsWereSentBefore(botClient, message);

            bool check = await driveHandler.CheckIfUserFolderExistsOnGoogleDrive(botClient, message);

            if (!check)
            {
                await HandleAccountCreation(botClient, message);
                return;
            }

            var files = await driveHandler.GetFileNames(botClient, message);

            if (files.Count == 0)
            {
                await HandleUserFileQuantityCheck(botClient, message);
                return;
            }

            messageId = await driveHandler.SendUserConfirmationInlineButtons(botClient, message);
            UserStateChanger.UpdateUserState(message.From.Id, UserStateChanger.UserState.EraseAllData);
        }

        private static async Task HandleGetDownloadUrlCommand(ITelegramBotClient botClient, Message message)
        {
            await CheckIfInlineButtonsWereSentBefore(botClient, message);

            bool check = await driveHandler.CheckIfUserFolderExistsOnGoogleDrive(botClient, message);

            if (!check)
            {
                await HandleAccountCreation(botClient, message);
                return;
            }

            var files = await driveHandler.GetFileNames(botClient, message);

            if (files.Count == 0)
            {
                await HandleUserFileQuantityCheck(botClient, message);
                return;
            }

            messageId = await driveHandler.SendUserFilesAsInlineButtons(botClient, message, files);
            UserStateChanger.UpdateUserState(message.From.Id, UserStateChanger.UserState.GetDownloadUrl);
        }

        private static async Task HandleGetViewUrlCommand(ITelegramBotClient botClient, Message message)
        {
            await CheckIfInlineButtonsWereSentBefore(botClient, message);

            bool check = await driveHandler.CheckIfUserFolderExistsOnGoogleDrive(botClient, message);

            if (!check)
            {
                await HandleAccountCreation(botClient, message);
                return;
            }

            var files = await driveHandler.GetFileNames(botClient, message);

            if (files.Count == 0)
            {
                await HandleUserFileQuantityCheck(botClient, message);
                return;
            }

            messageId = await driveHandler.SendUserFilesAsInlineButtons(botClient, message, files);
            UserStateChanger.UpdateUserState(message.From.Id, UserStateChanger.UserState.GetViewUrl);
        }

        private static async Task HandleDeleteUserCommand(ITelegramBotClient botClient, Message message)
        {
            await CheckIfInlineButtonsWereSentBefore(botClient, message);

            bool check = await driveHandler.CheckIfUserFolderExistsOnGoogleDrive(botClient, message);

            if (!check)
            {
                await HandleAccountCreation(botClient, message);
                return;
            }

            messageId = await driveHandler.SendUserConfirmationInlineButtons(botClient, message);
            UserStateChanger.UpdateUserState(message.From.Id, UserStateChanger.UserState.DeleteUser);
        }

        private static async Task HandleAccountCreation(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Looks like you have not created an account yet!" +
                $"\nNo worries, I created it for you.");

            UpdateUserState(message.From.Id, UserState.None);
        }

        private static async Task HandleUserFileQuantityCheck(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(message.From.Id, "You don't have any files");
            UserStateChanger.UpdateUserState(message.From.Id, UserStateChanger.UserState.None);
        }

        private static async Task ExecuteSaveFileCommand(ITelegramBotClient botClient, Message message)
        {
            if (message.Document != null)
            {
                await driveHandler.SaveFileToUserFolder(botClient, message);
                await botClient.SendTextMessageAsync(message.Chat, "File saved successfully!");
                UserStateChanger.UpdateUserState(message.From.Id, UserStateChanger.UserState.None);
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Send me a file please!");
            }
        }

        private static async Task ExecuteGetFileCommand(ITelegramBotClient botClient, Message message, Update update)
        {
            await driveHandler.GetFileFromUserFolder(botClient, update);
            await botClient.DeleteMessageAsync(message.Chat.Id, messageId);
            messageId = 0;
            await botClient.SendTextMessageAsync(message.Chat.Id, "Here it is!");
            UserStateChanger.UpdateUserState(message.From.Id, UserStateChanger.UserState.None);
        }

        private static async Task ExecuteDeleteFileCommand(ITelegramBotClient botClient, Message message, Update update)
        {
            await driveHandler.DeleteFileFromUserFolder(botClient, update);
            await botClient.DeleteMessageAsync(message.Chat.Id, messageId);
            messageId = 0;
            await botClient.SendTextMessageAsync(message.Chat.Id, "Great, file is erased now!");
            UserStateChanger.UpdateUserState(message.From.Id, UserStateChanger.UserState.None);
        }

        private static async Task ExecuteEraseAllFilesCommand(ITelegramBotClient botClient, Message message, Update update)
        {
            await driveHandler.EraseAllDataFromUserFolder(botClient, update);
            await botClient.DeleteMessageAsync(message.Chat.Id, messageId);
            messageId = 0;
            await botClient.SendTextMessageAsync(message.Chat.Id, "Great, all data is erased now!");
            UserStateChanger.UpdateUserState(message.From.Id, UserStateChanger.UserState.None);
        }

        private static async Task ExecuteGetDownloadUrlCommand(ITelegramBotClient botClient, Message message, Update update)
        {
            await driveHandler.SendUserDownloadFileUrl(botClient, update);
            await botClient.DeleteMessageAsync(message.Chat.Id, messageId);
            messageId = 0;
            UserStateChanger.UpdateUserState(message.From.Id, UserStateChanger.UserState.None);
        }

        private static async Task ExecuteGetViewUrlCommand(ITelegramBotClient botClient, Message message, Update update)
        {
            await driveHandler.SendUserViewFileUrl(botClient, update);
            await botClient.DeleteMessageAsync(message.Chat.Id, messageId);
            messageId = 0;
            UserStateChanger.UpdateUserState(message.From.Id, UserStateChanger.UserState.None);
        }

        private static async Task ExecuteDeleteUserCommand(ITelegramBotClient botClient, Message message, Update update)
        {
            await driveHandler.DeleteUserAndAllHisDataFromDrive(botClient, update);
            await botClient.DeleteMessageAsync(message.Chat.Id, messageId);
            messageId = 0;
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Goodbye, {message.Chat.FirstName}! See you soon, maybe...");
            UserStateChanger.UpdateUserState(message.From.Id, UserStateChanger.UserState.None);
        }

        private static async Task CheckIfInlineButtonsWereSentBefore(ITelegramBotClient botClient, Message message)
        {
            if(messageId != 0)
            {
                await botClient.DeleteMessageAsync(message.Chat.Id, messageId);
                messageId = 0;
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
    }
}
