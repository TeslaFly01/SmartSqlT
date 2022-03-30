using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.DocUtils.Dtos
{
    public class ChmHHP
    {
        public ChmHHP() { }

        public ChmHHP(string chmFile, string workTmpDir)
        {
            this.ChmFile = chmFile;
            this.WorkTmpDir = workTmpDir;

            if (!string.IsNullOrWhiteSpace(this.WorkTmpDir))
            {
                this.Files = Directory.GetFiles(this.WorkTmpDir, "*.html", SearchOption.AllDirectories).ToList();
            }
        }

        public string ChmFile { get; set; }

        public string WorkTmpDir { get; set; }

        public string DefaultFile { get; set; } = "数据库目录.html";

        public string Title
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.ChmFile))
                {
                    return string.Empty;
                }
                return Path.GetFileName(this.ChmFile);
            }
        }

        public List<string> Files { get; private set; }
    }
}
