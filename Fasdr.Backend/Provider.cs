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

		public void Merge(Provider other)
		{
			foreach (var kv in other.FullPathToEntry) {	
				var theirs = other.Entries [kv.Value];
				int index;
				if (!FullPathToEntry.TryGetValue (kv.Key, out index)) {
					FullPathToEntry.Add (kv.Key, CurrentId);
					Entries.Add (CurrentId, theirs);
					CurrentId = CurrentId + 1;
				} else {
					var ours = Entries [index];
					long maxFreq = Math.Max (theirs.Frequency, ours.Frequency);
					other.Entries [index] = new Entry (ours.FullPath, maxFreq, Math.Max(theirs.LastAccessTimeUtc,ours.LastAccessTimeUtc), ours.IsLeaf);
				}
			}
		}

		public void Add(Entry e)
		{
			FullPathToEntry.Add (e.FullPath.ToLower (), CurrentId);
			Entries.Add(CurrentId,e);
			var pathSplit = e.FullPath.Split (new char[]{ '\\'},StringSplitOptions.RemoveEmptyEntries);
            string lastElement = pathSplit[pathSplit.Length - 1];
            string lastElementLower = lastElement.ToLower();
			LastEntry lastEntry;
			if (!LastEntries.TryGetValue (lastElementLower, out lastEntry)) 
			{
                lastEntry = new LastEntry { Name = lastElement, Ids = new List<int>() };
                LastEntries.Add(lastElementLower, lastEntry);
			}
            lastEntry.Ids.Add(CurrentId);
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
		public Dictionary<string, LastEntry> LastEntries { get; } = new Dictionary<string, LastEntry>();

        public struct LastEntry
        {
            public string Name;
            public IList<int> Ids;
        }

		private int CurrentId { get; set; }
	}
}

