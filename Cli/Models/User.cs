namespace Cli.Models
{
    public static class User
    {
        public static string Username { get; set; } = string.Empty;
        public static string Email { get; set; } = string.Empty;
        public static string GoogleId { get; set; } = string.Empty;
        public static string Token { get; set; } = "No token";


        public static void SetUserDetails(string username, string email, string id, string token)
        {

            Username = username;
            Email = email;
            GoogleId = id;
            Token = token;
        }

        public static void Clear()
        {
            Username = string.Empty;
            Email = string.Empty;
            GoogleId = string.Empty;
        }
    }
}