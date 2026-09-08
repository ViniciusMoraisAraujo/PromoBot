using Microsoft.EntityFrameworkCore;
using PromoBot.Application.Interfaces;
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

builder.Services.AddDbContext<PromoBotDataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<INotifier, SavedMessagesNotifier>();

builder.Services.AddSingleton<ITelegramGateway, TelegramGateway>();

builder.Services.AddScoped<ProcessIncomingMessageUseCase>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();