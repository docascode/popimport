using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Linq;
using System.Diagnostics;

namespace popimport
{
    class Program
    {
        private const string FRAMEWORKS_FILE = "frameworks.xml";

        static void Main(string[] args)
        {
            ParserResult<CommandLineOptions> options = Parser.Default.ParseArguments<CommandLineOptions>(args);

            if (options != null)
            {
                options.WithParsed(t => ProcessData(t));
            }
        }

        static void ProcessData(CommandLineOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.FrameworksPath))
            {
                string text = Path.Combine(options.FrameworksPath, "frameworks.xml");
                if (File.Exists(text))
                {
                    List<Framework> frameworks = new List<Framework>();
                    XDocument xDocument = XDocument.Load(text);
                    foreach (XElement current in xDocument.Descendants("Frameworks").Descendants("Framework"))
                    {
                        Framework framework = new Framework()
                        {
                            Name = current.Attribute("Name").Value,
                            SourcePath = current.Attribute("Source").Value,
                            XMLElement = current,
                            AssemblyVersionMapping = new Dictionary<string, string>()
                        };
                        Console.WriteLine(string.Format("Operating on {0}", framework.Name));
                        string sourcePath = Path.Combine(options.FrameworksPath, framework.SourcePath);
                        foreach (var xmlPath in Directory.GetFiles(sourcePath, "*.xml"))
                        {
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(xmlPath);
                            string dllPath = Path.Combine(sourcePath, fileNameWithoutExtension + ".dll");
                            if (File.Exists(dllPath))
                            {
                                var version = FileVersionInfo.GetVersionInfo(dllPath).FileVersion;
                                if (!string.IsNullOrEmpty(version))
                                {
                                    framework.AssemblyVersionMapping.Add(Path.GetFileName(xmlPath), version);
                                }
                            }
                            
                        }
                        frameworks.Add(framework);
                    }
                    var maxVersions = frameworks.SelectMany(f => f.AssemblyVersionMapping)
                        .GroupBy(m => m.Key)
                        .ToDictionary(g => g.Key, g => g.Max(m => m.Value));

                    foreach(var framework in frameworks)
                    {
                        foreach(var assembly in framework.AssemblyVersionMapping)
                        {
                            if (maxVersions.ContainsKey(assembly.Key) && assembly.Value == maxVersions[assembly.Key])
                            {
                                framework.XMLElement.Add(new XElement("import", string.Format("{0}\\{1}", framework.SourcePath, assembly.Key)));
                            }
                        }
                    }
                    xDocument.Save(text);
                    return;
                }
                Console.WriteLine("There was no frameworks.xml file found.");
            }
        }
    }

    class Framework
    {
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public XElement XMLElement { get; set; }
        public Dictionary<string, string> AssemblyVersionMapping { get; set; }
    }
}
