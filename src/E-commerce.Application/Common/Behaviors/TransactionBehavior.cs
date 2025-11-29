using E_commerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace E_commerce.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that wraps command execution in a database transaction
/// Only applies to commands (not queries)
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IApplicationDbContext context,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        var requestName = typeof(TRequest).Name;

        // Skip transaction for queries (only use for commands that modify data)
        if (requestName.EndsWith("Query"))
        {
            return await next();
        }

        _logger.LogInformation("Begin transaction for {RequestName}", requestName);

        // Get the DbContext from IApplicationDbContext (needs cast)
        if (_context is not DbContext dbContext)
        {
            return await next();
        }

        // Use transaction for commands
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();
            
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Committed transaction for {RequestName}", requestName);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rollback transaction for {RequestName}", requestName);
            
            await transaction.RollbackAsync(cancellationToken);
            
            throw;
        }
    }
}
