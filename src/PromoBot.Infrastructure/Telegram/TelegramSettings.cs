namespace PromoBot.Infrastructure.Telegram;

public class TelegramSettings
{
    public const string SectionName = "Telegram";
    
    public int ApiId { get; set; }
    public string ApiHash { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<long> TargetChatIds { get; set; } = [];
}