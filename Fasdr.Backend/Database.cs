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
        public static readonly char Separator = '|';

        public void Load()
        {
            try
            {
                Entries.Clear();

                foreach (var textFile in FileSystem.Directory.GetFiles(ConfigDir, ConfigFileName, SearchOption.TopDirectoryOnly))
                {
                    using (var s = FileSystem.File.OpenText(textFile))
                    {
                        while (!s.EndOfStream)
                        {
                            var line = s.ReadLine();
                            var split = line.Split(new char[] {Separator}, StringSplitOptions.RemoveEmptyEntries);

                            if (split==null || split.Length!=4)
                            {
                                throw new Exception("Failed to parse line '" + line + "'");
                            }

                            var path = split[0];

                            double weight;
                            if (!Double.TryParse(split[1], out weight))
                            {
                            }

                            string provider = split[2];


                            bool isLeaf;
                            if (!Boolean.TryParse(split[3], out isLeaf))
                            {
                            }

                            Entries.Add(path, new Entry(weight,provider, isLeaf));
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
                foreach(var p in Entries)
                {
                    string line = $"{p.Key}{Separator}{p.Value.Weight}{Separator}{p.Value.Provider}{Separator}{p.Value.IsLeaf}";
                    s.WriteLine(line);
                }
            }

            FileSystem.File.Move(fileName, ConfigPath);
        }

        public IFileSystem FileSystem { get; }
        public Dictionary<string, Entry> Entries { get; } = new Dictionary<string, Entry>();
    }

    public struct Entry
    {
        public Entry(double weight,string provider,bool isLeaf)
        {
            Weight = weight;
            Provider = provider;
            IsLeaf = isLeaf;
        }

        public double Weight { get; }
        public string Provider { get; }
        public bool IsLeaf { get; }
    }
}
