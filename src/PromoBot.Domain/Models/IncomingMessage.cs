namespace PromoBot.Domain.Models;

public readonly record struct IncomingMessage(long ChatId, int MessageId, string Text);