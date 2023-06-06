using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Clippy
{
    internal class DriveHandler
    {
        public async Task<bool> CheckIfUserFolderExistsOnGoogleDrive(ITelegramBotClient botClient, Message message)
        {
            string credentialsFile = "PATH_TO_CREDENTIALS_FILE";

            GoogleCredential credential;
            using (var stream = new FileStream(credentialsFile, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.ScopeConstants.Drive);
            }

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "APP_NAME"
            });

            long userId = message.From.Id;
            string storageFolderId = "FOLDER_ID";
            var query = $"name='{userId}' and mimeType='application/vnd.google-apps.folder' and '{storageFolderId}' in parents";
            var listRequest = service.Files.List();
            listRequest.Q = query;
            var result = await listRequest.ExecuteAsync();

            if (result.Files.Count > 0)
            {
                string folderId = result.Files[0].Id;
                return true;
            }

            var folderMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = userId.ToString(),
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { storageFolderId }
            };

            var request = service.Files.Create(folderMetadata);
            request.Fields = "id";
            var folder = await request.ExecuteAsync();
            string newFolderId = folder.Id;

            return false;
        }

        public async Task SaveFileToUserFolder(ITelegramBotClient botClient, Message message)
        {
            string credentialsFile = "PATH_TO_CREDENTIALS_FILE";

            GoogleCredential credential;
            using (var stream = new FileStream(credentialsFile, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.ScopeConstants.Drive);
            }

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "APP_NAME"
            });

            long userId = message.From.Id;
            string storageFolderId = "FOLDER_ID";
            var query = $"name='{userId}' and mimeType='application/vnd.google-apps.folder' and '{storageFolderId}' in parents";
            var listRequest = service.Files.List();
            listRequest.Q = query;
            var result = await listRequest.ExecuteAsync();

            if (result.Files.Count > 0)
            {
                var userFolderId = result.Files[0].Id;
                var file = await botClient.GetFileAsync(message.Document.FileId);
                string originalFileName = message.Document.FileName;

                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = originalFileName,
                    Parents = new List<string> { userFolderId }
                };

                using (var fileStream = new MemoryStream())
                {
                    await botClient.DownloadFileAsync(file.FilePath, fileStream);
                    fileStream.Position = 0;

                    var request = service.Files.Create(fileMetadata, fileStream, message.Document.MimeType);
                    request.Fields = "id";
                    var uploadedFile = await request.UploadAsync();
                }
            }
            else
            {
                await Console.Out.WriteLineAsync($"User folder for ID {userId} does not exist in the 'Storage' folder.");
            }
        }

        public async Task<List<string>> GetFileNames(ITelegramBotClient botClient, Message message)
        {
            string credentialsFile = "PATH_TO_CREDENTIALS_FILE";

            GoogleCredential credential;
            using (var stream = new FileStream(credentialsFile, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.ScopeConstants.Drive);
            }

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "APP_NAME"
            });

            long userId = message.From.Id;
            string storageFolderId = "FOLDER_ID";
            var query = $"name='{userId}' and mimeType='application/vnd.google-apps.folder' and '{storageFolderId}' in parents";
            var listRequest = service.Files.List();
            listRequest.Q = query;
            var result = await listRequest.ExecuteAsync();

            if (result.Files.Count > 0)
            {
                var userFolderId = result.Files[0].Id;
                var getAllFilesQuery = $"'{userFolderId}' in parents";
                var secondListRequest = service.Files.List();
                secondListRequest.Q = getAllFilesQuery;
                var queryResult = await secondListRequest.ExecuteAsync();

                if (queryResult.Files.Count > 0)
                {
                    List<string> fileList = new List<string>();

                    foreach (var item in queryResult.Files)
                    {
                        fileList.Add(item.Name);
                    }

                    return fileList;
                }
            }

            return new List<string>();
        }

        public async Task<int> SendUserFilesAsInlineButtons(ITelegramBotClient botClient, Message message, List<string> files)
        {
            var inlineKeyboard = new List<List<InlineKeyboardButton>>();

            foreach (var fileName in files)
            {
                var button = InlineKeyboardButton.WithCallbackData(fileName, fileName);
                var row = new List<InlineKeyboardButton> { button };
                inlineKeyboard.Add(row);
            }

            var inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboard);

            var sentMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "Names of files:", replyMarkup: inlineKeyboardMarkup);
            return sentMessage.MessageId;
        }

        public async Task<int> SendUserConfirmationInlineButtons(ITelegramBotClient botClient, Message message)
        {
            var inlineKeyboard = new List<List<InlineKeyboardButton>>();

            var yesButton = InlineKeyboardButton.WithCallbackData("Yes, I'm totally sure.", "Erase");
            var noButton = InlineKeyboardButton.WithCallbackData("No, thank you.", "Cancel");

            var row = new List<InlineKeyboardButton> { yesButton, noButton };
            inlineKeyboard.Add(row);

            var inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboard);

            var sentMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "Are you sure?", replyMarkup: inlineKeyboardMarkup);
            return sentMessage.MessageId;
        }

        public async Task GetFileFromUserFolder(ITelegramBotClient botClient, Update update)
        {
            string credentialsFile = "PATH_TO_CREDENTIALS_FILE";

            GoogleCredential credential;
            using (var stream = new FileStream(credentialsFile, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.ScopeConstants.Drive);
            }

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "APP_NAME"
            });

            long userId = update.CallbackQuery.From.Id;
            var userFolderName = userId.ToString();
            string storageFolderId = "FOLDER_ID";
            var fileName = update.CallbackQuery.Data;

            var listRequest = service.Files.List();
            listRequest.Q = $"name='{userFolderName}' and '{storageFolderId}' in parents";
            var folderResult = await listRequest.ExecuteAsync();

            if (folderResult.Files.Count > 0)
            {
                var userFolderId = folderResult.Files[0].Id;

                listRequest.Q = $"name='{fileName}' and '{userFolderId}' in parents";
                var fileResult = await listRequest.ExecuteAsync();

                if (fileResult.Files.Count > 0)
                {
                    var fileId = fileResult.Files[0].Id;

                    var fileRequest = service.Files.Get(fileId);
                    using (var memoryStream = new MemoryStream())
                    {
                        await fileRequest.DownloadAsync(memoryStream);
                        memoryStream.Position = 0;

                        await botClient.SendDocumentAsync(update.CallbackQuery.From.Id, new InputFileStream(memoryStream, fileName));
                    }
                }
                else
                {
                    Console.WriteLine($"File with name '{fileName}' not found in the user's folder.");
                }
            }
            else
            {
                Console.WriteLine($"User folder with name '{userFolderName}' not found in the storage folder.");
            }
        }

        public async Task DeleteFileFromUserFolder(ITelegramBotClient botClient, Update update)
        {
            string credentialsFile = "PATH_TO_CREDENTIALS_FILE";

            GoogleCredential credential;
            using (var stream = new FileStream(credentialsFile, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.ScopeConstants.Drive);
            }

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "APP_NAME"
            });

            long userId = update.CallbackQuery.From.Id;
            var userFolderName = userId.ToString();
            string storageFolderId = "FOLDER_ID";
            var fileName = update.CallbackQuery.Data;

            var listRequest = service.Files.List();
            listRequest.Q = $"name='{userFolderName}' and '{storageFolderId}' in parents";
            var folderResult = await listRequest.ExecuteAsync();

            if (folderResult.Files.Count > 0)
            {
                var userFolderId = folderResult.Files[0].Id;

                listRequest.Q = $"name='{fileName}' and '{userFolderId}' in parents";
                var fileResult = await listRequest.ExecuteAsync();

                if (fileResult.Files.Count > 0)
                {
                    var fileId = fileResult.Files[0].Id;
                    await service.Files.Delete(fileId).ExecuteAsync();
                }
                else
                {
                    Console.WriteLine($"File with name '{fileName}' not found in the user's folder.");
                }
            }
            else
            {
                Console.WriteLine($"User folder with name '{userFolderName}' not found in the storage folder.");
            }
        }

        public async Task EraseAllDataFromUserFolder(ITelegramBotClient botClient, Update update)
        {
            if (update.CallbackQuery.Data == "Erase")
            {
                string credentialsFile = "PATH_TO_CREDENTIALS_FILE";

                GoogleCredential credential;
                using (var stream = new FileStream(credentialsFile, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(DriveService.ScopeConstants.Drive);
                }

                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "APP_NAME"
                });

                long userId = update.CallbackQuery.From.Id;
                var userFolderName = userId.ToString();
                string storageFolderId = "FOLDER_ID";

                var listRequest = service.Files.List();
                listRequest.Q = $"name='{userFolderName}' and '{storageFolderId}' in parents";
                var folderResult = await listRequest.ExecuteAsync();

                if (folderResult.Files.Count > 0)
                {
                    var userFolderId = folderResult.Files[0].Id;

                    listRequest.Q = $"'{userFolderId}' in parents";
                    var fileResult = await listRequest.ExecuteAsync();

                    if (fileResult.Files.Count > 0)
                    {
                        foreach (var item in fileResult.Files)
                        {
                            await service.Files.Delete(item.Id).ExecuteAsync();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"There are no files in the user's folder.");
                    }
                }
                else
                {
                    Console.WriteLine($"User folder with name '{userFolderName}' not found in the storage folder.");
                }
            }
            else
            {
                return;
            }
        }

        public async Task SendUserDownloadFileUrl(ITelegramBotClient botClient, Update update)
        {
            string credentialsFile = "PATH_TO_CREDENTIALS_FILE";

            GoogleCredential credential;
            using (var stream = new FileStream(credentialsFile, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.ScopeConstants.Drive);
            }

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "APP_NAME"
            });

            long userId = update.CallbackQuery.From.Id;
            var userFolderName = userId.ToString();
            string storageFolderId = "FOLDER_ID";
            var fileName = update.CallbackQuery.Data;

            var listRequest = service.Files.List();
            listRequest.Q = $"name='{userFolderName}' and '{storageFolderId}' in parents";
            var folderResult = await listRequest.ExecuteAsync();

            if (folderResult.Files.Count > 0)
            {
                var userFolderId = folderResult.Files[0].Id;

                listRequest.Q = $"name='{fileName}' and '{userFolderId}' in parents";
                var fileResult = await listRequest.ExecuteAsync();

                if (fileResult.Files.Count > 0)
                {
                    var fileId = fileResult.Files[0].Id;

                    var fileUrl = await GetDownloadFileUrlFromGoogleDrive(service, fileId);

                    var chatId = update.CallbackQuery.Message.Chat.Id;
                    await botClient.SendTextMessageAsync(chatId, fileUrl);
                }
                else
                {
                    Console.WriteLine($"File with name '{fileName}' not found in the user's folder.");
                }
            }
            else
            {
                Console.WriteLine($"User folder with name '{userFolderName}' not found in the storage folder.");
            }
        }

        private async Task<string> GetDownloadFileUrlFromGoogleDrive(DriveService service, string fileId)
        {
            var fileRequest = service.Files.Get(fileId);
            fileRequest.Fields = "webContentLink";
            var file = await fileRequest.ExecuteAsync();

            return file.WebContentLink;
        }

        public async Task SendUserViewFileUrl(ITelegramBotClient botClient, Update update)
        {
            string credentialsFile = "PATH_TO_CREDENTIALS_FILE";

            GoogleCredential credential;
            using (var stream = new FileStream(credentialsFile, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.ScopeConstants.Drive);
            }

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "APP_NAME"
            });

            long userId = update.CallbackQuery.From.Id;
            var userFolderName = userId.ToString();
            string storageFolderId = "FOLDER_ID";
            var fileName = update.CallbackQuery.Data;

            var listRequest = service.Files.List();
            listRequest.Q = $"name='{userFolderName}' and '{storageFolderId}' in parents";
            var folderResult = await listRequest.ExecuteAsync();

            if (folderResult.Files.Count > 0)
            {
                var userFolderId = folderResult.Files[0].Id;

                listRequest.Q = $"name='{fileName}' and '{userFolderId}' in parents";
                var fileResult = await listRequest.ExecuteAsync();

                if (fileResult.Files.Count > 0)
                {
                    var fileId = fileResult.Files[0].Id;
                    var fileUrl = $"https://drive.google.com/file/d/{fileId}/view?usp=drivesdk";
                    var chatId = update.CallbackQuery.Message.Chat.Id;
                    await botClient.SendTextMessageAsync(chatId, fileUrl);
                }
            }
        }

        public async Task DeleteUserAndAllHisDataFromDrive(ITelegramBotClient telegramBotClient, Update update)
        {
            if (update.CallbackQuery.Data == "Erase")
            {
                string credentialsFile = "PATH_TO_CREDENTIALS_FILE";

                GoogleCredential credential;
                using (var stream = new FileStream(credentialsFile, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(DriveService.ScopeConstants.Drive);
                }

                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "APP_NAME"
                });

                long userId = update.CallbackQuery.From.Id;
                var userFolderName = userId.ToString();
                string storageFolderId = "FOLDER_ID";

                var listRequest = service.Files.List();
                listRequest.Q = $"name='{userFolderName}' and '{storageFolderId}' in parents";
                var folderResult = await listRequest.ExecuteAsync();

                if (folderResult.Files.Count > 0)
                {
                    var userFolderId = folderResult.Files[0].Id;
                    await DeleteSubfoldersAndSubfiles(service, userFolderId);
                }
                else
                {
                    Console.WriteLine($"User folder with name '{userFolderName}' not found in the storage folder.");
                }
            }
            else
            {
                return;
            }
        }

        private async Task DeleteSubfoldersAndSubfiles(DriveService service, string folderId)
        {
            var listRequest = service.Files.List();
            listRequest.Q = $"'{folderId}' in parents";
            var fileList = await listRequest.ExecuteAsync();

            foreach (var file in fileList.Files)
            {
                if (file.MimeType == "application/vnd.google-apps.folder")
                {
                    await DeleteSubfoldersAndSubfiles(service, file.Id);
                }
                else
                {
                    await service.Files.Delete(file.Id).ExecuteAsync();
                }
            }

            await service.Files.Delete(folderId).ExecuteAsync();
        }
    }
}
