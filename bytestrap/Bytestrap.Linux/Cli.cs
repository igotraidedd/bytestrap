namespace Bytestrap.Linux;

/// <summary>Minimal dependency-free CLI parser + pretty output helpers.</summary>
public sealed class ParsedArgs
{
    public string Command { get; init; } = string.Empty;
    public string Subcommand { get; init; } = string.Empty;
    public List<string> Positionals { get; init; } = new();
    public Dictionary<string, string> Options { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public static class Cli
{
    public static ParsedArgs Parse(string[] argv)
    {
        var parsed = new ParsedArgs();
        var positionals = new List<string>();

        for (int i = 0; i < argv.Length; i++)
        {
            string arg = argv[i];

            if (arg.StartsWith("--", StringComparison.Ordinal) && arg.Length > 2)
            {
                string key = arg[2..];
                string value = "true";

                int eq = key.IndexOf('=');
                if (eq >= 0)
                {
                    value = key[(eq + 1)..];
                    key = key[..eq];
                }
                else if (i + 1 < argv.Length && !argv[i + 1].StartsWith('-'))
                {
                    value = argv[++i];
                }

                parsed.Options[key] = value;
            }
            else if (arg.StartsWith('-') && arg.Length == 2)
            {
                string key = arg[1..];
                string value = "true";

                if (i + 1 < argv.Length && !argv[i + 1].StartsWith('-'))
                    value = argv[++i];

                parsed.Options[key] = value;
            }
            else
            {
                positionals.Add(arg);
            }
        }

        if (positionals.Count > 0)
            parsed.Command = positionals[0].ToLowerInvariant();
        if (positionals.Count > 1)
            parsed.Subcommand = positionals[1].ToLowerInvariant();
        if (positionals.Count > 2)
            parsed.Positionals.AddRange(positionals.Skip(2));

        return parsed;
    }

    public static bool Has(this ParsedArgs args, params string[] names) =>
        names.Any(n => args.Options.ContainsKey(n));

    public static string? Get(this ParsedArgs args, params string[] names)
    {
        foreach (string name in names)
        {
            if (args.Options.TryGetValue(name, out string? value))
                return value;
        }

        return null;
    }

    public static void WriteLine(string message = "") => Console.WriteLine(message);

    public static void Ok(string message) => ColorLine(message, ConsoleColor.Green);
    public static void Warn(string message) => ColorLine(message, ConsoleColor.Yellow);
    public static void Err(string message) => ColorLine(message, ConsoleColor.Red);
    public static void Dim(string message) => ColorLine(message, ConsoleColor.DarkGray);
    public static void Head(string message) => ColorLine(message, ConsoleColor.Cyan);

    private static void ColorLine(string message, ConsoleColor color)
    {
        if (!Console.IsOutputRedirected)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = prev;
        }
        else
        {
            Console.WriteLine(message);
        }
    }
}
