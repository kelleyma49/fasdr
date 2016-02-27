using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO;

namespace Fasdr.Backend
{
	public class Provider
	{
		public static readonly char Separator = '|';

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
				var split = line.Split(new char[] {Separator}, StringSplitOptions.RemoveEmptyEntries);

				// should be three entries - path name, weight, and if it's a leaf?
				if (split==null || split.Length!=3)
				{
					throw new Exception("Failed to parse line '" + line + "'");
				}

				var path = split[0];

				double weight;
				if (!Double.TryParse(split[1], out weight))
				{
					throw new Exception ("Failed to parse weight");
				}

				bool isLeaf;
				if (!Boolean.TryParse(split[2], out isLeaf))
				{
					throw new Exception ("Failed to parse isLeaf flag");
				}

				Add (path, isLeaf, weight);
			}
		}

		public void Save(string filePath,IFileSystem fileSystem)
		{
			var fileName = System.IO.Path.Combine(Path.GetTempPath(),Path.GetRandomFileName());
			using (var s = fileSystem.File.CreateText(fileName))
			{
				foreach(var p in Entries)
				{
					string line = $"{p.Value.FullPath}{Separator}{p.Value.Weight}{Separator}{p.Value.IsLeaf}";
					s.WriteLine(line);
				}
			}
			fileSystem.File.Move(fileName, filePath);
		}

		public void Add(string fullPath,bool isLeaf,double weight = 1.0)
		{
			Entries.Add(CurrentId,new Entry(fullPath,weight,isLeaf));
			var pathSplit = fullPath.Split (new char[]{ '\\'});
			string lastElement = pathSplit [pathSplit.Length - 1].ToLower();
			IList<int> ids;
			if (!LastEntries.TryGetValue (lastElement, out ids)) 
			{
				LastEntries.Add(lastElement,ids = new List<int> ());
			}
			ids.Add (CurrentId);
			CurrentId = CurrentId + 1;
		}
			
		public string Name { get; }
		public Dictionary<int,Entry> Entries { get; } = new Dictionary<int,Entry>();
		public Dictionary<string,IList<int>> LastEntries { get; } = new Dictionary<string,IList<int>>();

		private int CurrentId { get; set; }
	}

	public struct Entry
	{
		public Entry(string fullPath,double weight,bool isLeaf)
		{
			FullPath = fullPath;
			Weight = weight;
			IsLeaf = isLeaf;
		}

		public string FullPath { get; }
		public double Weight { get; }
		public bool IsLeaf { get; }
	}
}

