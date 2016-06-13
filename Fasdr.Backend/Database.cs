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
        public Database(IFileSystem fileSystem,string configDir=null)
        {
            FileSystem = fileSystem;
            if (String.IsNullOrEmpty(configDir))
                ConfigDir = DefaultConfigDir;
            else
                ConfigDir = configDir;
            ConfigPath = System.IO.Path.Combine(ConfigDir, ConfigFileName);
            if (!Directory.Exists(ConfigDir))
            {
                Directory.CreateDirectory(ConfigDir);
            }
        }

        static Database()
        {
            DefaultConfigDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".fasdr");
        }

        public readonly string ConfigDir;
        public readonly string ConfigPath;

        public static readonly string DefaultConfigDir;
        public static readonly string ConfigFilePrefix = "db";
		public static readonly string ConfigFileName = $"{ConfigFilePrefix}.*.txt";
        
        public string GetProviderDatabaseLocation(string providerName)
        {
            return System.IO.Path.Combine(ConfigDir, ConfigFileName.Replace("*", providerName));
        }

		public void Load()
        {
            try
            {
				Providers.Clear();

				// find provider files:
                foreach (var textFile in FileSystem.Directory.GetFiles(ConfigDir, ConfigFileName, SearchOption.TopDirectoryOnly))
                {
					var provider = LoadProvider(textFile);
					Providers.Add(provider.Name,provider);
                }
            }
            catch (FileNotFoundException)
            {
                
            }
            catch (DirectoryNotFoundException)
            {

            }
        }

		private Provider LoadProvider(string textFile)
		{
			string fileNameOnly = System.IO.Path.GetFileName(textFile); 
			string[] fileSplit = fileNameOnly.Split(new char[]{'.'});
			if (fileSplit==null || fileSplit.Length!=3)
			{
				throw new Exception("Failed to parse config file name '" + fileNameOnly + "'");
			}

			var provider = new Provider(fileSplit[1]);
			using (var s = FileSystem.File.OpenText(textFile))
			{
				provider.Load(s);
			}
			return provider;
		}

        public bool RemoveEntry(string providerName, string fullPath)
        {
            Provider provider;
            if (!Providers.TryGetValue(providerName, out provider))
                return false;

            return provider.Remove(fullPath);
        }

        public bool AddEntry(string providerName,string fullPath, Predicate<string> checkIsLeaf)
        {
            // create provider if it doesn't exist:
            Provider provider;
            if (!Providers.TryGetValue(providerName,out provider))
            {
                provider = new Provider(providerName);
                Providers[providerName] = provider;
            }

            return provider.UpdateEntry(fullPath, checkIsLeaf);
        }
        
        public bool GetEntries(string providerName,out Entry[] entries)
        {
            Provider provider;
            bool result = Providers.TryGetValue(providerName, out provider);

            if (provider != null)
            {
                entries = provider.Entries.
                    OrderByDescending(p => p.Value.CalculateFrecency()).
                    Select(p => p.Value).
                    ToArray();
            }
            else
                entries = null;
            return result;
        }  

        public void Save(int maxEntries)
        {
			using (var sgi = new SingleGlobalInstance (5000)) 
			{
				foreach (var p in Providers) {
					var fileName = System.IO.Path.Combine (ConfigDir, ConfigFileName.Replace ("*", p.Key));
					if (FileSystem.File.Exists (fileName)) {
						// merge with currently saved file in case another shell instance
						// saved out after our initial load.
						var currProvider = LoadProvider (fileName);
						p.Value.Merge (currProvider);
					}

					p.Value.Save (fileName, FileSystem, maxEntries);
				}
			}
        }

		public Dictionary<string,Provider> Providers { get; } = new Dictionary<string,Provider>();
        public IFileSystem FileSystem { get; }
       
    }

    
}
