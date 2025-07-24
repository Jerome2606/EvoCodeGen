using System;
using System.Collections.Generic;
using System.IO;
using EvoCodeGen.Core.Models;

namespace EvoCodeGen.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0 || Array.Exists(args, a => a == "--help" || a == "-h"))
            {
                ShowUsage();
                return 1;
            }

            var templates = new List<string>();
            string projectFolder = null;
            string outputFolder = null;
            string modelName = null;
            string jsonPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-t":
                    case "--template":
                        if (i + 1 < args.Length)
                        {
                            templates.Add(args[++i]);
                        }
                        break;
                    case "-p":
                    case "--project":
                        if (i + 1 < args.Length)
                            projectFolder = args[++i];
                        break;
                    case "-o":
                    case "--output":
                        if (i + 1 < args.Length)
                            outputFolder = args[++i];
                        break;
                    case "-n":
                    case "--name":
                        if (i + 1 < args.Length)
                            modelName = args[++i];
                        break;
                    case "-j":
                    case "--json":
                        if (i + 1 < args.Length)
                            jsonPath = args[++i];
                        break;
                }
            }

            if (templates.Count == 0 || string.IsNullOrEmpty(projectFolder) || string.IsNullOrEmpty(outputFolder) || string.IsNullOrEmpty(modelName) || string.IsNullOrEmpty(jsonPath))
            {
                Console.Error.WriteLine("Missing required arguments.");
                ShowUsage();
                return 1;
            }

            if (!File.Exists(jsonPath))
            {
                Console.Error.WriteLine($"JSON file not found: {jsonPath}");
                return 1;
            }

            string jsonModel = File.ReadAllText(jsonPath);

            var generator = new EvoCodeGenerator(new EvoCodeGeneratorConfiguration());
            var model = new EvoCodeGeneratorModel
            {
                TemplateFiles = templates,
                FolderProject = projectFolder,
                FolderItem = outputFolder,
                Prefix = modelName,
                JsonModel = jsonModel
            };

            try
            {
                var result = generator.GenerateCode(model).GetAwaiter().GetResult();
                foreach (var file in result.Files)
                {
                    Console.WriteLine($"{file.FileStatus}: {file.FilePath}");
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        static void ShowUsage()
        {
            Console.WriteLine("Usage: EvoCodeGen.Cli -t <template> [-t <template> ...] -p <projectFolder> -o <outputFolder> -n <name> -j <jsonFile>");
        }
    }
}
