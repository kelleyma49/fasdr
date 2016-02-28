using System;
using NUnit.Framework;
using Fasdr.Backend;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fasdr.UnitTests
{
	[TestFixture]
	public class ProviderTest
	{
		[Test]
		public void TestCanConstruct()
		{
			var p = new Provider("FileSystem");
			Assert.AreEqual ("FileSystem", p.Name);
		}

		[Test]
		public void TestFiles()
		{
			var p = new Provider ("FileSystem");
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(TestData.GetTwoDirFilesystem()))) 
			{
				p.Load(new StreamReader(ms));	
			}

			Assert.AreEqual (2, p.Entries.Count);
			/*
			Assert.IsTrue(p.Entries.ContainsKey(@"c:\dir1\"));
			Assert.AreEqual(101.0f, p.Entries[@"c:\dir1\"].Weight);
			Assert.AreEqual(true, p.Entries[@"c:\dir1\"].IsLeaf);

			Assert.IsTrue(p.Entries.ContainsKey(@"c:\dir1\file2"));
			Assert.AreEqual(10.0f, p.Entries[@"c:\dir1\file2"].Weight);
			Assert.AreEqual(false, p.Entries[@"c:\dir1\file2"].IsLeaf);
			*/
		}

		[Test]
		public void TestUpdateNonExistantEntry()
		{
			var p = new Provider ("FileSystem");
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(TestData.GetTwoDirFilesystem()))) 
			{
				p.Load(new StreamReader(ms));	
			}

			Assert.AreEqual(false,p.UpdateEntry (-1));
		}

		[Test]
		public void TestUpdateEntry() 
		{
			var p = new Provider ("FileSystem");
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(TestData.GetTwoDirFilesystem()))) 
			{
				p.Load(new StreamReader(ms));	
			}
				
			var f = p.Entries [0].Frequency;
			var now = DateTime.UtcNow;
			Assert.IsTrue(p.UpdateEntry (TestData.Dir1.FullPath));
			Assert.AreEqual (f + 1, p.Entries [0].Frequency);
			Assert.Greater (p.Entries [0].LastAccessTime, now);
		}
	}
}

