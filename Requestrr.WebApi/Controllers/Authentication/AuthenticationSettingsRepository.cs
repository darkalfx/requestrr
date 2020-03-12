namespace Requestrr.WebApi.Controllers
{
    public static class AuthenticationSettingsRepository
    {
        public static void UpdateAdminAccount(string username, string password)
        {
            SettingsFile.Write(settings =>
            {
                settings.Authentication.Username = username;
                settings.Authentication.Password = password;
            });
        }
    }
}