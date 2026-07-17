namespace ConsoleBackstop.Tool;

// A standalone console exe with NO tool packaging, NO parser package, and NO reference to Lib —
// it blocks the Library archetype (not auxiliary) so the repo reads App with zero entries.
internal static class Program
{
    private static void Main()
    {
        System.Console.WriteLine("backstop fixture");
    }
}
