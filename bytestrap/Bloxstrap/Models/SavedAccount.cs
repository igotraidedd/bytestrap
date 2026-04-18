namespace Bloxstrap.Models
{
    public class SavedAccount
    {
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string EncryptedCookie { get; set; } = string.Empty;
        public DateTime LastUsed { get; set; } = DateTime.MinValue;
        public string Nickname { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
