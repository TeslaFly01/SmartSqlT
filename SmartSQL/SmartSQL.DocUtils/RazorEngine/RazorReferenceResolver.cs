using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;

namespace SmartSQL.DocUtils
{
    public class RazorReferenceResolver: IReferenceResolver
    {
        public string FindLoaded(IEnumerable<string> refs, string find)
        {
            return refs.First(r => r.EndsWith(System.IO.Path.DirectorySeparatorChar + find));
        }
        public IEnumerable<CompilerReference> GetReferences(TypeContext context, IEnumerable<CompilerReference> includeAssemblies)
        {
            // TypeContext gives you some context for the compilation (which templates, which namespaces and types)

            // You must make sure to include all libraries that are required!
            // Mono compiler does add more standard references than csc! 
            // If you want mono compatibility include ALL references here, including mscorlib!
            // If you include mscorlib here the compiler is called with /nostdlib.
            IEnumerable<string> loadedAssemblies = (new UseCurrentAssembliesReferenceResolver())
                .GetReferences(context, includeAssemblies)
                .Select(r => r.GetFile())
                .ToArray();

            yield return CompilerReference.From(FindLoaded(loadedAssemblies, "mscorlib.dll"));
            yield return CompilerReference.From(FindLoaded(loadedAssemblies, "System.dll"));
            yield return CompilerReference.From(FindLoaded(loadedAssemblies, "System.Core.dll"));
            yield return CompilerReference.From(FindLoaded(loadedAssemblies, "RazorEngine.dll"));
            yield return CompilerReference.From(typeof(RazorReferenceResolver).Assembly); // Assembly

            // There are several ways to load an assembly:
            //yield return CompilerReference.From("Path-to-my-custom-assembly"); // file path (string)
            //byte[] assemblyInByteArray = --- Load your assembly ---;
            //yield return CompilerReference.From(assemblyInByteArray); // byte array (roslyn only)
            //string assemblyFile = --- Get the path to the assembly ---;
            //yield return CompilerReference.From(File.OpenRead(assemblyFile)); // stream (roslyn only)
        }
    }
}
