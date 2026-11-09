namespace Prensadao.Seeder;

public static class SeederConfiguration
{
    public const string ConnectionStringEnvironmentVariable =
        "ConnectionStrings__DefaultConnection";

    public static string GetConnectionString(Func<string, string?> environmentVariableReader)
    {
        ArgumentNullException.ThrowIfNull(environmentVariableReader);

        var connectionString = environmentVariableReader(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Defina a variável de ambiente {ConnectionStringEnvironmentVariable} " +
                "com a conexão da base local antes de executar o seeder.");
        }

        return connectionString;
    }
}
