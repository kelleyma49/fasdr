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
    public class Database : IDatabase
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
		public static readonly string ConfigFilePrefix = "fasdrConfig";
		public static readonly string ConfigFileName = $"{ConfigFilePrefix}.*.txt";
        public static readonly string ConfigPath;
   
		public void Load()
        {
            try
            {
				Providers.Clear();

				// find provider files:
                foreach (var textFile in FileSystem.Directory.GetFiles(ConfigDir, ConfigFileName, SearchOption.TopDirectoryOnly))
                {
					string fileNameOnly = System.IO.Path.GetFileName(textFile); 
					string[] fileSplit = fileNameOnly.Split(new char[]{'.'});
					if (fileSplit==null || fileSplit.Length!=3)
					{
						throw new Exception("Failed to parse config file name '" + fileNameOnly + "'");
					}

					var provider = new Provider(fileSplit[1]);
					Providers.Add(provider.Name,provider);
					using (var s = FileSystem.File.OpenText(textFile))
					{
						provider.Load(s);
					}
                }
            }
            catch (FileNotFoundException)
            {
                
            }
            catch (DirectoryNotFoundException)
            {

            }
        }

        public void Save()
        {
			foreach (var p in Providers) 
			{
				var fileName = System.IO.Path.Combine (ConfigDir, ConfigFileName.Replace ("*", p.Key));
				p.Value.Save (fileName,FileSystem);
			}
        }

		public Dictionary<string,Provider> Providers { get; } = new Dictionary<string,Provider>();
        public IFileSystem FileSystem { get; }
       
    }

    
}
