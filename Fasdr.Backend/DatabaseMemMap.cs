using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Abstractions;

namespace Fasdr.Backend
{
	public class DatabaseMemMap : Database
	{
		public DatabaseMemMap(IFileSystem fileSystem,string configDir=null) : base(fileSystem,configDir) 
		{
		}
		protected override IDisposable OpenFile(string textFile,out StreamReader sr) 
		{	
			MemMapFile = MemoryMappedFile.CreateFromFile (textFile, FileMode.Open);

			MemoryMappedViewStream fileMap = MemMapFile.CreateViewStream();
			sr = new StreamReader(fileMap);
			return fileMap;
		}

		private MemoryMappedFile MemMapFile { get; set; }
	}
}

