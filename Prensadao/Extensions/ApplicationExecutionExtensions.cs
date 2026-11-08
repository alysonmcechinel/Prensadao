using Prensadao.Infra.Persistence;

namespace Prensadao.API.Extensions;

public static class ApplicationExecutionExtensions
{
    private const string MigrateDatabaseArgument = "--migrate-database";

    public static async Task RunConfiguredModeAsync(
        this WebApplication application,
        string[] arguments)
    {
        if (arguments.Contains(MigrateDatabaseArgument, StringComparer.OrdinalIgnoreCase))
        {
            await application.Services.ApplyDatabaseMigrationsAsync();
            return;
        }

        await application.RunAsync();
    }
}
