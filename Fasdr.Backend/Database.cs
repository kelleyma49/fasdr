using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Fasdr.UnitTests")]

namespace Fasdr.Backend
{
    public class Database
    {
        public Database(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        static Database()
        {
            ConfigDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            ConfigPath = System.IO.Path.Combine(ConfigDir, ConfigFileName);
        }

        public static readonly string ConfigDir;
        public static readonly string ConfigFileName = "fasdrConfig.txt";
        public static readonly string ConfigPath;

        public void Load()
        {
            try
            {
                PathToWeight.Clear();

                foreach (var textFile in FileSystem.Directory.GetFiles(ConfigDir, ConfigFileName, SearchOption.TopDirectoryOnly))
                {
                    using (var s = FileSystem.File.OpenText(textFile))
                    {
                        while (!s.EndOfStream)
                        {
                            var line = s.ReadLine();
                            var split = line.Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries);

                            if (split==null || split.Length!=2)
                            {
                                throw new Exception("Failed to parse line '" + line + "'");
                            }

                            double weight;
                            if (!Double.TryParse(split[1], out weight))
                            {
                            }

                            var path = split[0];

                            PathToWeight.Add(path, weight);
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                
            }
        }

        public void Save()
        {
            var fileName = System.IO.Path.Combine(ConfigDir,Path.GetRandomFileName());
            using (var s = FileSystem.File.CreateText(fileName))
            {
                foreach(var p in PathToWeight)
                {
                    s.WriteLine(string.Join("|", p.Key, p.Value));
                }
            }

            FileSystem.File.Move(fileName, ConfigPath);
        }

        public IFileSystem FileSystem { get; }
        public Dictionary<string, double> PathToWeight { get; } = new Dictionary<string, double>();
    }

}
