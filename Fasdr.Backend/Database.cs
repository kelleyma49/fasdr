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

        public void Load()
        {
            var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            try
            {
                PathToWeight.Clear();

                foreach (var textFile in FileSystem.Directory.GetFiles(userDir, "fasdrConfig.txt", SearchOption.TopDirectoryOnly))
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

        public IFileSystem FileSystem { get; }
        public Dictionary<string, double> PathToWeight { get; } = new Dictionary<string, double>();
    }

}
