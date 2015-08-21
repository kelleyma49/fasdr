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
            try
            {
                PathToWeight.Clear();

                foreach (var textFile in FileSystem.Directory.GetFiles(@"c:\", "fasdrConfig.txt", SearchOption.TopDirectoryOnly))
                {
                    foreach (var line in FileSystem.File.ReadLines(textFile))
                    {
                        int indexSplit = line.IndexOf(' ');
                        if (indexSplit<=0)
                        {
                            throw new Exception("Failed to parse line '" + line + "'");
                        }

                        double weight;
                        if (!Double.TryParse(line.Substring(0, indexSplit), out weight))
                        {
                            throw new Exception("Failed to parse line '" + line + "'");
                        }

                        string path = line.Substring(indexSplit);

                        PathToWeight.Add(path, weight);
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
