# Clippy Bot

This Telegram Bot simplifies file management tasks for users. It allows users to save, retrieve, delete, and manage files on Google Drive through simple commands.

## Requirements

- .NET Core SDK
- Telegram.Bot NuGet package
- Newtonsoft.Json NuGet package
- Google.Apis.Drive.v3 NuGet package

## Setup

1. Obtain a Telegram Bot token from the BotFather.
2. Replace `"YOUR_TOKEN"` with your Telegram Bot token in the `UpdateHandler` class.

## Usage

The Telegram Bot supports the following commands:

- `/start`: Starts the bot and checks if the user's folder exists on Google Drive.
- `/cancel`: Cancels the last operation.
- `/savefile`: Prompts the user to send a file to be saved.
- `/getfile`: Retrieves the user's files from Google Drive.
- `/deletefile`: Deletes a file from the user's Google Drive.
- `/eraseallfiles`: Erases all files from the user's Google Drive.
- `/getdownloadurl`: Retrieves the download URL of a file from the user's Google Drive.
- `/getviewurl`: Retrieves the view URL of a file from the user's Google Drive.
- `/deleteuser`: Deletes the user and all their data from Google Drive.

## Code Structure

The code is organized into the following classes:

- `UpdateHandler`: Handles the updates received from Telegram and dispatches them to the appropriate command handlers.
- `DriveHandler`: Provides methods for interacting with Google Drive, such as checking folder existence, saving files, retrieving files, deleting files, erasing all data, and retrieving file URLs.
- `UserStateChanger`: Manages the state of each user.

## Main Methods

### UpdateHandler class

- `HandleUpdateAsync`: Entry point for handling updates.
- `HandleStartCommand`: Handles the `/start` command.
- `HandleCancelCommand`: Handles the `/cancel` command.
- `HandleSaveFileCommand`: Handles the `/savefile` command.
- `HandleGetFileCommand`: Handles the `/getfile` command.
- `HandleDeleteFileCommand`: Handles the `/deletefile` command.
- `HandleEraseAllFilesCommand`: Handles the `/eraseallfiles` command.
- `HandleGetDownloadUrlCommand`: Handles the `/getdownloadurl` command.
- `HandleGetViewUrlCommand`: Handles the `/getviewurl` command.
- `HandleDeleteUserCommand`: Handles the `/deleteuser` command.
- `HandleUnknownCommand`: Handles unknown commands.
- `HandleAccountCreation`: Handles the account creation process.
- `HandleUserFileQuantityCheck`: Handles the case when the user doesn't have any files.

### DriveHandler class

- `CheckFolderExistence`: Checks if the user's folder exists on Google Drive.
- `CreateUserFolder`: Creates a folder for the user on Google Drive.
- `SaveFile`: Saves the file sent by the user to their Google Drive folder.
- `GetFiles`: Retrieves the list of files from the user's Google Drive folder.
- `DeleteFile`: Deletes a file from the user's Google Drive folder.
- `EraseAllFiles`: Erases all files from the user's Google Drive folder.
- `GetDownloadUrl`: Retrieves the download URL of a file from the user's Google Drive folder.
- `GetViewUrl`: Retrieves the view URL of a file from the user's Google Drive folder.
- `DeleteUser`: Deletes the user and all their data from Google Drive.

### UserStateChanger class

- `UpdateUserState`: Updates the user state for the specified chat ID.
- `GetUserState`: Retrieves the user state.
