using Serilog;
using ViaEventAssociation.Core.Application.CommandDispatching.Commands;
using ViaEventAssociation.Core.Application.Features.Dispatcher;
using ViaEventAssociation.Core.Domain;

namespace ViaEventAssociation.Core.Application.CommandDispatching.Dispatcher;

public class TransactionCommandDispatcherDecorator(ICommandDispatcher decoratedDispatcher, IUnitOfWork unitOfWork)
    : ICommandDispatcher {
    static TransactionCommandDispatcherDecorator() {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/commands_log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }


    public async Task<Result> DispatchAsync(Command command) {
        var result = await decoratedDispatcher.DispatchAsync(command);

        if (result.IsSuccess) {
            await unitOfWork.SaveChangesAsync();
            Log.Information("Transaction committed successfully");
        }
        else {
            Log.Information("Transaction rolled back due to command failure");
        }

        return result;
    }
}