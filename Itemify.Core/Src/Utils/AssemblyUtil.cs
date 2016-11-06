using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Itemify.Core.Utils
{
    internal class AssemblyUtil
    {
        public static string GetResourceFileContent(string resName, Encoding encoding)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var strResources = assembly.GetName().Name + ".g.resources";
            var rStream = assembly.GetManifestResourceStream(strResources);
            var resourceReader = new ResourceReader(rStream);
            var items = resourceReader.OfType<DictionaryEntry>();
            var stream = items.First(x => (x.Key as string) == resName.ToLower()).Value;

            using (var sr = new StreamReader((UnmanagedMemoryStream) stream, encoding))
                return sr.ReadToEnd();
        }
    }
}
