using PromoBot.Application.Interfaces;
using PromoBot.Application.UseCases;
using PromoBot.Domain.Models;

namespace PromoBot.Worker;

public class Worker(
    ILogger<Worker> logger, 
    ITelegramGateway telegramGateway, 
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly ITelegramGateway _telegramGateway = telegramGateway;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    // Define quantas mensagens podem ser processadas simultaneamente
    private const int MaxConcurrentMessages = 4;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("started connection with Telegram Gateway...");
        await _telegramGateway.StartAsync(stoppingToken);

        _logger.LogInformation(
            "PromoBot run! Consuming with up to {Concurrency} messages in parallel...", 
            MaxConcurrentMessages);

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = MaxConcurrentMessages, 
            CancellationToken = stoppingToken
        };

        try
        {
            await Parallel.ForEachAsync(
                _telegramGateway.Messages.ReadAllAsync(stoppingToken),
                parallelOptions,
                async (msg, ct) =>
                {
                    await ProcessMessageAsync(msg, ct);
                });
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker successfully completed.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in the parallel processing loop.");
            throw;
        }
    }

    private async Task ProcessMessageAsync(IncomingMessage msg, CancellationToken ct)
    {
        _logger.LogDebug("Starting processing of message {MessageId} from chat {ChatId}", msg.MessageId, msg.ChatId);

        try
        {
            // ⚠️ REGRA DE OURO: Cada tarefa paralela DEVE ter seu próprio escopo!
            // O DbContext do EF Core NÃO é thread-safe. Criar o escopo aqui garante
            // que cada mensagem tenha sua própria instância isolada de DbContext.
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