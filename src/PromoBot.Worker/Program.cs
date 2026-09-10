using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PromoBot.Application.Interfaces;
using PromoBot.Application.Services;
using PromoBot.Application.UseCases;
using PromoBot.Domain.Entities;
using PromoBot.Infrastructure.Persistence;
using PromoBot.Infrastructure.Telegram;
using PromoBot.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<TelegramSettings>(
    builder.Configuration.GetSection(TelegramSettings.SectionName));

builder.Services.Configure<List<FilterRule>>(
    builder.Configuration.GetSection("FilterRules"));

builder.Services.AddDbContextPool<PromoBotDataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

WTelegram.Helpers.Log = (_, message) =>
{
    var localMessage = Regex.Replace(message, @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}Z", match =>
    {
        if (DateTime.TryParse(match.Value, out var utcDate))
        {
            return utcDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }
        return match.Value;
    });

    Console.WriteLine(localMessage);
};

builder.Services.AddScoped<IBotSubscriberRepository, BotSubscribeRepository>();
builder.Services.AddScoped<IBotSubscriptionService, BotSubscriptionService>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<ProcessIncomingMessageUseCase>();

builder.Services.AddSingleton<ITelegramGateway, TelegramGateway>();

builder.Services.AddHttpClient<INotifier, TelegramBotNotifier>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<TelegramBotListenerWorker>();

var host = builder.Build();

host.Run();