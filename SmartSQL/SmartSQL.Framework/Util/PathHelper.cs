using System.IO;

namespace SmartSQL.Framework.Util
{
    public class PathHelper
    {
        public static void CreateCodeFileNamePath(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        public static string GetCodeFileNamePath(string outputPath, string languageAlias, string engineName, 
            string templateName, string packageName, string modelId)
        {
            return Path.Combine(outputPath, modelId, languageAlias, engineName, templateName, packageName);
        }

        public static string GetCodeFileName(string outputPath, string languageAlias, string engineName, 
            string templateName, string packageName, string modelId, string fileName)
        {
            return Path.Combine(GetCodeFileNamePath(outputPath, languageAlias, engineName, templateName, packageName, modelId), fileName);
        }
    }
}
