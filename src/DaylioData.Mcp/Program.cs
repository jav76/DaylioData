using System.Globalization;
using System.Text;

namespace DaylioData.Mcp;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        string? filePath = null;

        for (int i = 0; i < args.Length; i++)
        {
            if ((args[i].Equals("--file", StringComparison.OrdinalIgnoreCase) ||
                 args[i].Equals("-f", StringComparison.OrdinalIgnoreCase)) &&
                i + 1 < args.Length)
            {
                filePath = args[i + 1];
                i++;
            }
            else if (args[i].Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                     args[i].Equals("-h", StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine("Daylio MCP Server (daylio-mcp)");
                Console.Error.WriteLine("Usage: daylio-mcp [--file <path-to-daylio-export.csv>]");
                Console.Error.WriteLine();
                Console.Error.WriteLine("Options:");
                Console.Error.WriteLine("  -f, --file <path>   Initial Daylio CSV export to load at startup.");
                Console.Error.WriteLine("  -h, --help          Show help information.");
                Console.Error.WriteLine();
                Console.Error.WriteLine("Environment Variables:");
                Console.Error.WriteLine("  DAYLIO_CSV_PATH     Default path to Daylio CSV export file.");
                return 0;
            }
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            filePath = Environment.GetEnvironmentVariable("DAYLIO_CSV_PATH");
        }

        DaylioData? initialData = null;
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            if (File.Exists(filePath))
            {
                try
                {
                    initialData = new DaylioData(filePath);
                    int totalEntries = initialData.DataSummary?.TotalEntries ?? 0;
                    Console.Error.WriteLine(
                        $"[daylio-mcp] Pre-loaded {totalEntries} entries from '{filePath}'.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(
                        $"[daylio-mcp] Warning: Failed to pre-load CSV from '{filePath}': {ex.Message}");
                }
            }
            else
            {
                Console.Error.WriteLine(
                    $"[daylio-mcp] Warning: Specified file path does not exist: '{filePath}'.");
            }
        }

        McpServer server = new(initialData, filePath);
        await server.RunAsync(Console.In, Console.Out);
        return 0;
    }
}
