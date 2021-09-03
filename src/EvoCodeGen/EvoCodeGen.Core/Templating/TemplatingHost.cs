using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EvoCodeGen.Core.Templating
{
    class TemplatingHost : Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost
    {
        private readonly string _templateFileName;
        private readonly object _dataObject;
        private CompilerErrorCollection _errors;
        private string _fileExtension;
        private Encoding _fileEncoding;

        public TemplatingHost(string templateFileName, object dataObject)
        {
            _templateFileName = templateFileName;
            _dataObject = dataObject;
        }
        public IList<string> StandardAssemblyReferences => new string[] 
                { 
                    Assembly.GetExecutingAssembly().Location,
                    typeof(Uri).Assembly.Location,
                    typeof(Enumerable).Assembly.Location
                };

        public IList<string> StandardImports => new string[]
                {
                    "System",
                    typeof(TemplatingHost).Namespace
                };

        public string TemplateFile => _templateFileName;

        public object GetHostOption(string optionName)
        {
            object hostObject;

            switch (optionName)
            {
                case "Data":
                    hostObject = _dataObject;
                    break;
                default:
                    hostObject = null;
                    break;
            }
            return hostObject;
        }

        public bool LoadIncludeText(string requestFileName, out string content, out string location)
        {
            content = string.Empty;
            location = string.Empty;
            if (File.Exists(requestFileName))
            {
                content = File.ReadAllText(requestFileName);
                return true;
            }
            else
            {
                string searchDir = Path.GetDirectoryName(TemplateFile);
                string searchFile = Path.Combine(searchDir, requestFileName);
                if (File.Exists(searchFile))
                {
                    content = File.ReadAllText(searchFile);
                    return true;
                }
            }
            return false;
        }

        public void LogErrors(CompilerErrorCollection errors)
        {
            _errors = errors;
        }

        public AppDomain ProvideTemplatingAppDomain(string content)
        {
            return AppDomain.CreateDomain("New Domain");
        }

        public string ResolveAssemblyReference(string assemblyReference)
        {
            //If the argument is the fully qualified path of an existing file,
            //then we are done. (This does not do any work.)
            //----------------------------------------------------------------
            if (File.Exists(assemblyReference))
            {
                return assemblyReference;
            }

            //Maybe the assembly is in the same folder as the text template that 
            //called the directive.
            //----------------------------------------------------------------
            string candidate = Path.Combine(Path.GetDirectoryName(this.TemplateFile), assemblyReference);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            //This can be customized to search specific paths for the file
            //or to search the GAC.
            //----------------------------------------------------------------

            //This can be customized to accept paths to search as command line
            //arguments.
            //----------------------------------------------------------------

            //If we cannot do better, return the original file name.
            return "";
        }

        public Type ResolveDirectiveProcessor(string processorName)
        {
            throw new Exception(string.Concat("Directive Processor ", processorName, " not found"));
        }

        public string ResolveParameterValue(string directiveId, string processorName, string parameterName)
        {
            if (directiveId != null)
            {
                if (processorName != null)
                {
                    if (parameterName != null)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        throw new ArgumentNullException("parameterName");
                    }
                }
                else
                {
                    throw new ArgumentNullException("processorName");
                }
            }
            else
            {
                throw new ArgumentNullException("directiveId");
            }
        }

        public string ResolvePath(string path)
        {
            if (path != null)
            {
                if (!File.Exists(path))
                {
                    string str = Path.Combine(Path.GetDirectoryName(this.TemplateFile), path);
                    if (!File.Exists(str))
                    {
                        return path;
                    }
                    else
                    {
                        return str;
                    }
                }
                else
                {
                    return path;
                }
            }
            else
            {
                throw new ArgumentNullException("path");
            }
        }

        public void SetFileExtension(string extension)
        {
            _fileExtension = extension;
        }

        public void SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
        {
            _fileEncoding = encoding;
        }
    }
}
