# Clippy
Clippy is a Telegram bot developed to simplify file management tasks. It provides various commands to save, retrieve, delete, and manage files on Google Drive.

## Features
1) Save File: Allows users to save files to their personal Google Drive folders.
2) Retrieve File: Enables users to retrieve files from their Google Drive folders.
3) Delete File: Allows users to delete files from their Google Drive folders.
4) Erase All Files: Provides the ability to delete all files from the user's Google Drive folder.
5) Get Download URL: Retrieves the download URL for a specific file in the user's Google Drive folder.
6) Get View URL: Retrieves the view URL for a specific file in the user's Google Drive folder.
7) Delete User: Allows users to delete their account along with all associated data from Google Drive.

## Getting Started
### To use Clippy bot, follow these steps:
1) Create a new bot on Telegram by talking to BotFather.
2) Obtain the API token for your bot.
3) Clone this repository to your local machine.
4) Open the UpdateHandler.cs file and replace "YOUR_TOKEN" with your actual Telegram bot API token.
5) Build and run the application.

## Usage
1) /start: Initializes the bot and provides a welcome message.
2) /cancel: Cancels the last operation and returns to the default state.
3) /savefile: Initiates the process of saving a file to the user's Google Drive folder.
4) /getfile: Retrieves a list of files available in the user's Google Drive folder and allows selection for download.
5) /deletefile: Retrieves a list of files available in the user's Google Drive folder and allows selection for deletion.
6) /eraseallfiles: Initiates the process of deleting all files from the user's Google Drive folder.
7) /getdownloadurl: Retrieves a list of files available in the user's Google Drive folder and allows selection to get the download URL.
8) /getviewurl: Retrieves a list of files available in the user's Google Drive folder and allows selection to get the view URL.
9) /deleteuser: Deletes the user's account and all associated data from Google Drive.

## Contributing
Contributions to Clippy are welcome! If you have any bug reports, feature requests, or suggestions, please open an issue on the GitHub repository.

## License
This project is licensed under the MIT License.

## Acknowledgements
### Clippy was developed using the following technologies:

Telegram.Bot: https://github.com/TelegramBots/Telegram.Bot
Google.Apis.Drive: https://developers.google.com/drive
Special thanks to the contributors who have helped improve this project.
