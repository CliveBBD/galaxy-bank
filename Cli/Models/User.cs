namespace Cli.Models
{
    public static class User
    {
        public static string Username { get; set; } = string.Empty;
        public static string Email { get; set; } = string.Empty;
        public static string GoogleId { get; set; } = string.Empty;
        public static string Token { get; set; } = "No token";
        public static string Role { get; set; } = string.Empty;
        public static string SessionId { get; set; } = string.Empty;


        public static void SetUserDetails(string username, string email, string id, string token, string role, string sessionId)
        {

            Username = username;
            Email = email;
            GoogleId = id;
            Token = token;
            Role = role;
            SessionId = sessionId;
        }

        public static void Clear()
        {
            Username = string.Empty;
            Email = string.Empty;
            GoogleId = string.Empty;
            Token = "No token";
            Role = string.Empty;
            SessionId = string.Empty;
        }
    }
}