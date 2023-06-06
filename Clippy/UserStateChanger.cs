namespace Clippy
{
    internal class UserStateChanger
    {
        private static Dictionary<long, UserState> userStates = new Dictionary<long, UserState>();

        public enum UserState
        {
            None,
            SaveFile,
            DownloadFile,
            DeleteFile,
            EraseAllData,
            GetDownloadUrl,
            GetViewUrl,
            DeleteUser
        }

        public static void UpdateUserState(long chatId, UserState state)
        {
            userStates[chatId] = state;
        }

        public static UserState GetUserState(long chatId)
        {
            if (userStates.TryGetValue(chatId, out UserState state))
            {
                return state;
            }
            else
            {
                return UserState.None;
            }
        }
    }
}
