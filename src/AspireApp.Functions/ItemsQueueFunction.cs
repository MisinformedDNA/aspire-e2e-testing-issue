using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AspireApp.Functions;

public class ItemsQueueFunction
{
    private readonly ILogger<ItemsQueueFunction> _logger;

    public ItemsQueueFunction(ILogger<ItemsQueueFunction> logger)
    {
        _logger = logger;
    }

    [Function("ItemsQueueFunction")]
    public void Run([QueueTrigger("items")] string message)
    {
        _logger.LogInformation("Processing queue message: {Message}", message);
    }
}
