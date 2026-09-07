namespace PromoBot.Domain.Entities;

public class Promotion
{
    public int Id { get; private set; }
    public int MessageId { get; private set; }
    public long ChatId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? Url { get; private set; } = string.Empty;
    public DateTime PromotionDate { get; private set; } = DateTime.Now;
    public decimal? Value { get; private set; }
    public bool IsNotify { get; private set; }

    protected Promotion()
    {
        //required for ef
    }
    
    public Promotion(int messageId, long chatId,string description, string url, DateTime promotionDate, decimal? value)
    {
        MessageId = messageId;
        ChatId = chatId;
        Description = description;
        Url = url;
        PromotionDate = promotionDate;
        Value = value;
        IsNotify = false;
    }

    public void MarkAsNotify()
    {
        IsNotify = true;
    }
}