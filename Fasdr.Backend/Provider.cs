using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO;

namespace Fasdr.Backend
{
	public class Provider
	{
		public Provider(string name)
		{
			Name = name;
		}

		public void Load(StreamReader stream)
		{
			Entries.Clear ();

			CurrentId = 0;
			while (!stream.EndOfStream)
			{
				var line = stream.ReadLine();

				Entry e = Entry.FromString (line);
				Add (e);
			}
		}

		public void Save(string filePath,IFileSystem fileSystem)
		{
			var fileName = System.IO.Path.Combine(Path.GetTempPath(),Path.GetRandomFileName());
			using (var s = fileSystem.File.CreateText(fileName))
			{
				foreach(var p in Entries)
				{
					s.WriteLine(p.Value.ToString());
				}
			}
            if (fileSystem.File.Exists(filePath))
                fileSystem.File.Delete(filePath);
            fileSystem.File.Move(fileName, filePath);
		}

		public void Add(Entry e)
		{
			FullPathToEntry.Add (e.FullPath.ToLower (), CurrentId);
			Entries.Add(CurrentId,e);
			var pathSplit = e.FullPath.Split (new char[]{ '\\'});
			string lastElement = pathSplit [pathSplit.Length - 1].ToLower();
			IList<int> ids;
			if (!LastEntries.TryGetValue (lastElement, out ids)) 
			{
				LastEntries.Add(lastElement,ids = new List<int> ());
			}
			ids.Add (CurrentId);
			CurrentId = CurrentId + 1;
		}

        public bool UpdateEntry(string fullPath,Predicate<string> checkIsLeaf = null)
		{
			int id;
            string fullPathLower = fullPath.ToLower();
			if (!FullPathToEntry.TryGetValue (fullPathLower, out id))
            {
                if (checkIsLeaf==null) 
				    return false;

                Add(new Entry(fullPathLower, 1, DateTime.Now.ToFileTimeUtc(), checkIsLeaf(fullPath)));
                return true;
            }

            return UpdateEntry (id);
		}

		public bool UpdateEntry(int id)
		{
			Entry e;
			if (!Entries.TryGetValue (id, out e))
				return false;

			Entries [id] = new Entry (e.FullPath, e.Frequency + 1, DateTime.Now.ToFileTimeUtc (), e.IsLeaf);
			return true;
		}
			
		public string Name { get; }
		public Dictionary<int,Entry> Entries { get; } = new Dictionary<int,Entry>();
		public Dictionary<string,int> FullPathToEntry { get; } = new Dictionary<string,int>();
		public Dictionary<string,IList<int>> LastEntries { get; } = new Dictionary<string,IList<int>>();

		private int CurrentId { get; set; }
	}
}

