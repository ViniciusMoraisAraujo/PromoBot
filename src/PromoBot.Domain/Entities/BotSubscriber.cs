namespace PromoBot.Domain.Entities;

public class BotSubscriber
{
    public int Id { get; private set; }
    public long ChatId { get; private set; }
    public string? UserName { get; private set; }
    public DateTime SubscribedAt { get; private set; } 
    public bool IsActive { get; private set; }
    
    protected BotSubscriber() { }
    
    public BotSubscriber(long chatId,   string? userName)
    {
        ChatId = chatId;
        UserName = userName;
        SubscribedAt = DateTime.Now;
        IsActive = true;
    }

    public void Reactivate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}