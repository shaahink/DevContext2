using System.Reflection;

namespace DevContext.Desktop.Tests;

public class AssemblyResolutionTests
{
    [Fact]
    public void Desktop_assembly_references_all_resolve()
    {
        var asm = Assembly.Load("DevContext.Desktop");
        var refs = asm.GetReferencedAssemblies();
        var failures = new List<string>();

        foreach (var r in refs)
        {
            try
            {
                Assembly.Load(r);
            }
            catch (FileNotFoundException ex)
            {
                failures.Add($"{r.Name}: {ex.Message}");
            }
            catch (Exception) { /* other errors like BadImageFormat are OK in test context */ }
        }

        Assert.Empty(failures);
    }
}
