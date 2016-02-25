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
				}

				bool isLeaf;
				if (!Boolean.TryParse(split[2], out isLeaf))
				{
				}

				Entries.Add(path, new Entry(weight,isLeaf));
			}
		}

		public void Save(string filePath,IFileSystem fileSystem)
		{
			var fileName = System.IO.Path.Combine(Path.GetTempPath(),Path.GetRandomFileName());
			using (var s = fileSystem.File.CreateText(fileName))
			{
				foreach(var p in Entries)
				{
					string line = $"{p.Key}{Separator}{p.Value.Weight}{Separator}{p.Value.IsLeaf}";
					s.WriteLine(line);
				}
			}
			fileSystem.File.Move(fileName, filePath);
		}

		public string Name { get; }
		public Dictionary<string, Entry> Entries { get; } = new Dictionary<string, Entry>();
	}

	public struct Entry
	{
		public Entry(double weight,bool isLeaf)
		{
			Weight = weight;
			IsLeaf = isLeaf;
		}

		public double Weight { get; }
		public bool IsLeaf { get; }
	}
}

