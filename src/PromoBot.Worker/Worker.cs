using PromoBot.Application.Interfaces;
using PromoBot.Application.UseCases;
using PromoBot.Domain.Models;

namespace PromoBot.Worker;

public class Worker(ILogger<Worker> logger, ITelegramGateway telegramGateway, IServiceScopeFactory scopeFactory) : BackgroundService
{
   private readonly ILogger<Worker> _logger = logger;
   private readonly ITelegramGateway _telegramGateway = telegramGateway;
   private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      _logger.LogInformation("PromoBot Worker run...");
      
      await _telegramGateway.StartAsync(stoppingToken);
      
      _logger.LogInformation("PromoBot Worker started");

      try
      {
         await foreach (var msg in _telegramGateway.Messages.ReadAllAsync(stoppingToken))
         {
            await ProcessMessageAsync(msg, stoppingToken);
         }
      }
      catch (Exception e)
      {
         Console.WriteLine(e);
         throw;
      }
   }
   
   private async Task HandleMessageReceivedAsync(long chatId, int messageId, string text)
   {
      _logger.LogInformation("Mensagem recebida do Chat {ChatId} (Id: {MessageId})", chatId, messageId);
      
      try
      {
         using var scope = _scopeFactory.CreateScope();
         var useCase = scope.ServiceProvider.GetRequiredService<ProcessIncomingMessageUseCase>();

         await useCase.ExecuteAsync(chatId, messageId, text);
      }
      catch (Exception ex)
      {
         _logger.LogInformation(ex, "Erro ao processar mensagem {MessageId} do Chat {ChatId}", messageId, chatId);
      }
   }
   
   private async Task ProcessMessageAsync(IncomingMessage msg, CancellationToken ct)
   {
      _logger.LogInformation("Consumindo mensagem do Chat {ChatId} (Id: {MessageId})", msg.ChatId, msg.MessageId);
      try
      {
         using var scope = _scopeFactory.CreateScope();
         var useCase = scope.ServiceProvider.GetRequiredService<ProcessIncomingMessageUseCase>();
         await useCase.ExecuteAsync(msg.ChatId, msg.MessageId, msg.Text, ct);
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Erro ao processar mensagem {MessageId} do Chat {ChatId}", msg.MessageId, msg.ChatId);
      }
   }
}
