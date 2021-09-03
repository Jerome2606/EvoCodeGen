using System.Collections;
using System.Collections.Generic;

namespace EvoCodeGen.Core.Models
{
    public class EvoCodeGeneratorModel
    {
        public IEnumerable<string> TemplateFiles { get; set; }
        public string FolderProject { get; set; }
        public string Prefix { get; set; }
        public string FolderItem { get; set; }
        public string JsonModel { get; set; }
        public IEnumerable<KeyValuePair<string, string>> StaticReplacer { get; set; }
    }
}