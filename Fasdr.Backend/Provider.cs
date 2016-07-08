using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO;
using System.Linq;

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

        public void RemoveStaleEntries(Predicate<string> checkPath)
        {
            if (checkPath==null)
                return;

            var toRemove = new List<string>();
            foreach (var e in GetAllEntries())
            {
                if (!checkPath(e))
                {
                    toRemove.Add(e);
                }
            }

            foreach (var e in toRemove)
            {
                Remove(e);
            }
        }

        public void Save(string filePath,IFileSystem fileSystem,int maxEntries)
		{
			var fileName = Path.Combine(Path.GetTempPath(),Path.GetRandomFileName());
			using (var s = fileSystem.File.CreateText(fileName))
			{
                var sortedEntries = (from pair in Entries
                                           orderby pair.Value.CalculateFrecency()
                                           select pair).Take(maxEntries);
                foreach (var p in sortedEntries)
				{
					if (p.Value.IsValid)
						s.WriteLine(p.Value);
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

					// check to see if we've removed an entry, unless we recently 
					// updated the entry:
					long maxFreq = Math.Max (theirs.Frequency, ours.Frequency);
					if (!theirs.IsValid && theirs.LastAccessTime >= ours.LastAccessTime) {
						maxFreq = Entry.cInvalid;
					}
					else if (!ours.IsValid && ours.LastAccessTime >= theirs.LastAccessTime) {
						maxFreq = Entry.cInvalid;
					}

					Entries [index] = new Entry (ours.FullPath, maxFreq, Math.Max(theirs.LastAccessTimeUtc,ours.LastAccessTimeUtc), ours.IsLeaf);
				}
			}
		}

		public static string GetLastElement(string path)
		{
			var pathSplit = path.Split (new char[]{ '\\'},StringSplitOptions.RemoveEmptyEntries);
			return pathSplit[pathSplit.Length - 1];
		}

		public void Add(Entry e)
		{
			FullPathToEntry.Add (e.FullPath.ToLower (), CurrentId);
			Entries.Add(CurrentId,e);
			string lastElement = GetLastElement(e.FullPath);
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

		public bool Remove(string fullPath)
		{
			var fullPathLower = fullPath.ToLower ().TrimEnd(new char[] { '\\', '/' });
			int id;
			if (!FullPathToEntry.TryGetValue(fullPathLower,out id))
			{
				return false;
			}

			return UpdateEntry (id, remove: true);
		}

        public bool UpdateEntry(string fullPath,Predicate<string> checkIsLeaf = null)
		{
			int id;
            fullPath = fullPath.TrimEnd(new char[] { '\\', '/' });
            string fullPathLower = fullPath.ToLower();
			if (!FullPathToEntry.TryGetValue (fullPathLower, out id))
            {
                if (checkIsLeaf==null) 
				    return false;

                Add(new Entry(fullPath, 1, DateTime.Now.ToFileTimeUtc(), checkIsLeaf(fullPath)));
                return true;
            }

            return UpdateEntry (id);
		}

		public bool UpdateEntry(int id,bool remove=false)
		{
			Entry e;
			if (!Entries.TryGetValue (id, out e) || !e.IsValid)
				return false;

			long newFrequency = remove ? -1 : e.Frequency + 1;
			Entries [id] = new Entry (e.FullPath, newFrequency, DateTime.Now.ToFileTimeUtc (), e.IsLeaf);
			return true;
		}

        public IEnumerable<string> GetAllEntries()
		{
			return from pair in Entries
				orderby pair.Value.CalculateFrecency()
                   where pair.Value.IsValid
					select pair.Value.FullPath;
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

