namespace Prensadao.Seeder;

public static class SeederArguments
{
    private static readonly IReadOnlyDictionary<string, Func<LocalDatabaseSeedOptions, int, LocalDatabaseSeedOptions>>
        OptionSetters = new Dictionary<string, Func<LocalDatabaseSeedOptions, int, LocalDatabaseSeedOptions>>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["--seed-customers"] = (options, value) => options with { CustomerCount = value },
            ["--seed-products"] = (options, value) => options with { ProductCount = value },
            ["--seed-orders"] = (options, value) => options with { OrderCount = value },
            ["--seed-days"] = (options, value) => options with { HistoryDays = value },
            ["--seed-random"] = (options, value) => options with { RandomSeed = value },
            ["--seed-batch-size"] = (options, value) => options with { BatchSize = value }
        };

    public static LocalDatabaseSeedOptions Parse(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        var options = new LocalDatabaseSeedOptions();

        foreach (var argument in arguments)
        {
            var separatorIndex = argument.IndexOf('=');
            if (separatorIndex <= 0 || separatorIndex == argument.Length - 1)
            {
                throw new ArgumentException(
                    $"Argumento de seed inválido: '{argument}'. Use o formato --nome=valor.");
            }

            var name = argument[..separatorIndex];
            var rawValue = argument[(separatorIndex + 1)..];

            if (!OptionSetters.TryGetValue(name, out var setter))
                throw new ArgumentException($"Opção de seed desconhecida: '{name}'.");

            if (!int.TryParse(rawValue, out var value))
                throw new ArgumentException($"A opção '{name}' exige um número inteiro.");

            options = setter(options, value);
        }

        options.Validate();
        return options;
    }
}
