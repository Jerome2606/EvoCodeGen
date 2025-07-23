using EvoCodeGen.Core.Helpers;
using EvoCodeGen.Core.Templating;
using Microsoft.VisualStudio.TextTemplating;
using Newtonsoft.Json;
using Scriban;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EvoCodeGen.Core.Models
{
    public class EvoCodeGenerator
    {
        private readonly EvoCodeGeneratorConfiguration _configuration;

        public EvoCodeGenerator(EvoCodeGeneratorConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GenerationResult> GenerateCode(EvoCodeGeneratorModel model)
        {
            var result = new GenerationResult();
            result.Files = new List<GenerationResultFile>();

            var templateFiles = model.TemplateFiles;
            foreach (string inputItem in templateFiles)
            {
                var input = inputItem;
                // Remove template extension ".sbn"
                input = input.Replace(Path.GetExtension(input), "");
                input = PathHelper.GetRelativePath(model.FolderProject, input);

                // Ignore the container folder
                input = string.Join("\\", input.Split('\\').Skip(1));

                input = input.Replace("{prefix}", model.Prefix);

                if (input.EndsWith("\\", StringComparison.Ordinal))
                {
                    input = input + "__dummy__";
                }

                var file = new FileInfo(Path.Combine(model.FolderItem, input));
                string path = file.DirectoryName;

                Directory.CreateDirectory(path);

                if (!file.Exists)
                {
                    try
                    {
                        int position = await WriteFileAsync(file.FullName, model, inputItem);


                        result.Files.Add(new GenerationResultFile
                        {
                            FilePath = file.FullName,
                            Position = position,
                            FileStatus = GenerationResultFile.Status.Created
                        });
                    } 
                    catch(Exception ex)
                    {
                        result.Files.Add(new GenerationResultFile
                        {
                            FilePath = file.FullName,
                            Exception = ex,
                            FileStatus = GenerationResultFile.Status.CreationFailed
                        });
                    }
                }
                else
                {
                    // TODO: inform user file is already existing

                    result.Files.Add(new GenerationResultFile
                    {
                        FilePath = file.FullName,
                        FileStatus = GenerationResultFile.Status.AlreadyExisting
                    });
                    //System.Windows.Forms.MessageBox.Show("The file '" + file + "' already exist.");
                }
            }

            return result;
        }


        private static async Task<int> WriteFileAsync(string file, EvoCodeGeneratorModel baseModel = null, string templatename = "")
        {
            string name = Path.GetFileName(file);
            string safeName = name.StartsWith(".") ? name : Path.GetFileNameWithoutExtension(file);
            string relative = PathHelper.GetRelativePath(baseModel.FolderProject, Path.GetDirectoryName(file));
            string template = null;
            try
            {
                string templateReplaced = await ReplaceTokensAsync(safeName, relative, templatename, baseModel.JsonModel);
                if (baseModel.StaticReplacer != null)
                foreach(var replacer in baseModel.StaticReplacer)
                {
                    templateReplaced = templateReplaced.Replace(replacer.Key, replacer.Value);
                }
                template = NormalizeLineEndings(templateReplaced);
            } catch(Exception ex)
            {
                var debug = ex;
            }

            if (!string.IsNullOrEmpty(template))
            {
                int index = template.IndexOf('$');

                if (index > -1)
                {
                    template = template.Remove(index, 1);
                }

                await WriteToDiskAsync(file, template);
                return index;
            }

            await WriteToDiskAsync(file, string.Empty);

            return 0;
        }

        private static string NormalizeLineEndings(string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            return Regex.Replace(content, @"\r\n|\n\r|\n|\r", "\r\n");
        }

        private static async System.Threading.Tasks.Task WriteToDiskAsync(string file, string content)
        {
            using (var writer = new StreamWriter(file, false, GetFileEncoding(file)))
            {
                await writer.WriteAsync(content);
            }
        }

        private static Encoding GetFileEncoding(string file)
        {
            string[] noBom = { ".cmd", ".bat", ".json" };
            string ext = Path.GetExtension(file).ToLowerInvariant();

            if (noBom.Contains(ext))
                return new UTF8Encoding(false);

            return new UTF8Encoding(true);
        }

        public static async Task<string> ReplaceTokensAsync(string name, string relative, string templateFile, string jsonModel = null)
        {
            if (string.IsNullOrEmpty(templateFile))
                return templateFile;

            using (var reader = new StreamReader(templateFile))
            {
                string content = await reader.ReadToEndAsync();

                if (Path.GetExtension(templateFile) == ".mbl")
                {
                    throw new NotImplementedException("mbl files are no more supported, please migrate to Scriban files (.sbn)");
                    //return new Engine().ProcessTemplate(content, new TemplatingHost(templateFile, new { TableName = name }));

                }
                else if (Path.GetExtension(templateFile) == ".sbn")
                {
                    var model = new { model = JsonConvert.DeserializeObject(jsonModel) };
                    return await Template.Parse(content).RenderAsync(model);

                }
                else
                {
                    return content.Replace("{itemname}", name);
                }
            }
        }

    }
}
