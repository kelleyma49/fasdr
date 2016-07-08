using System;
using NUnit.Framework;
using Fasdr.Backend;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;

namespace Fasdr.UnitTests
{
	[TestFixture]
	public class ProviderTest
	{

		private Provider SetupMatchSimple(string contents)
		{
			var p = new Provider ("FileSystem");
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(contents))) 
			{
				p.Load(new StreamReader(ms));	
			}
			return p;
		}

		[Test]
		public void TestCanConstruct()
		{
			var p = new Provider("FileSystem");
			Assert.AreEqual ("FileSystem", p.Name);
		}

		[Test]
		public void TestFiles()
		{
			var p = SetupMatchSimple(TestData.GetTwoDirFilesystem ());

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
        public void TestUpdateNonExistentEntryShouldFail()
        {
            var p = new Provider("FileSystem");
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(TestData.GetTwoDirFilesystem())))
            {
                p.Load(new StreamReader(ms));
            }

            Assert.AreEqual(false, p.UpdateEntry(-1));
        }

        [Test]
        public void TestUpdateNonExistentEntryShouldSucceed()
        {
            var p = new Provider("FileSystem");
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(TestData.GetTwoDirFilesystem())))
            {
                p.Load(new StreamReader(ms));
            }

            var now = DateTime.UtcNow;
            var newStr = "c:\tree\newEntry";
			Assert.IsTrue(p.UpdateEntry(newStr,s => s.EndsWith("newentry",StringComparison.Ordinal)));
            var entry = p.Entries.Values.OfType<Entry>().First(e => e.FullPath == newStr);
            StringAssert.AreEqualIgnoringCase(newStr, entry.FullPath);
            Assert.AreEqual(1, entry.Frequency);
            Assert.GreaterOrEqual(entry.LastAccessTime, now);
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
			Assert.GreaterOrEqual(p.Entries [0].LastAccessTime, now);
		}

        [Test]
        public void TestUpdateEntryHandlesPathWithSlash()
        {
            var p = new Provider("FileSystem");
            Assert.AreEqual(0, p.Entries.Count);
            Assert.IsTrue(p.UpdateEntry(@"c:\dir1", s => false));
            Assert.AreEqual(1, p.Entries.Count);
            Assert.IsTrue(p.UpdateEntry(@"c:\dir1\", s => false));
            Assert.AreEqual(1, p.Entries.Count);
        }

        [Test, TestCaseSource(typeof(MyFactoryClass),"TestRemoveEntryCases")]
		public void TestRemoveEntry(string configFileContents, string fullPathToRemove, bool expected)
		{
			var p = SetupMatchSimple (configFileContents);
			int id = -1;
			if (expected) {
				id = p.FullPathToEntry [fullPathToRemove.ToLower().TrimEnd(new char[] { '\\', '/' })];
			}
			Assert.AreEqual(expected,p.Remove (fullPathToRemove));
			if (expected) {
				Assert.IsFalse(p.Entries[id].IsValid);
			}
		}


		[Test, TestCaseSource(typeof(MyFactoryClass),"TestCanGetAllEntriesCases")]
		public void TestCanGetAllEntries(string configFileContents, 
			string[] expected)
		{
			var p = SetupMatchSimple (configFileContents);
			CollectionAssert.AreEqual (expected, p.GetAllEntries ());
		}

        [Test, TestCaseSource(typeof(MyFactoryClass), "TestCanRemoveStaleEntriesCases")]
        public void TestCanRemoveStaleEntries(string configFileContents,
            Predicate<string> pathExists,
            string[] expected)
        {
            var p = SetupMatchSimple(configFileContents);
            p.RemoveStaleEntries(pathExists);
            CollectionAssert.AreEqual(expected, p.GetAllEntries());
        }


        public static class MyFactoryClass
		{
			public static IEnumerable TestRemoveEntryCases 
			{
				get 
				{
					yield return new TestCaseData ("", @"c:\treeNotThere", false)
						.SetName("TestFailToRemoveFromEmptyProvider");

					yield return new TestCaseData (string.Join(Environment.NewLine,
						new Entry(@"c:\tools\", 101, DateTime.Now, false),
						new Entry(@"c:\tree\", 102, DateTime.Now, false)), 
						@"c\treeNotThere", false)
						.SetName("TestFailToRemoveFromPopulatedProvider");

					yield return new TestCaseData (string.Join(Environment.NewLine,
						new Entry(@"c:\tools\", 101, DateTime.Now, false),
						new Entry(@"c:\tree\", 102, DateTime.Now, false)), 
						@"c:\tree", true)
							.SetName("TestRemoveFromProvider");
				}
			}

			public static IEnumerable TestCanGetAllEntriesCases
			{
				get 
				{
					yield return new TestCaseData ("", new string[]{})
						.SetName("TestGetAllEntriesEmptyDatabase");

					yield return new TestCaseData (
						string.Join(Environment.NewLine,
							new Entry(@"c:\tree\", 102, DateTime.Now, false)),
						new string[] {@"c:\tree"})
							.SetName("TestGetAllEntriesOneItem");
					
					yield return new TestCaseData (
						string.Join(Environment.NewLine,
							new Entry(@"c:\tools\", 101, DateTime.Now, false),
							new Entry(@"c:\tree\", 102, DateTime.Now, false)),
						new string[] {@"c:\tools",@"c:\tree"})
							.SetName("TestGetAllEntriesTwoItems");
				}
			}

            public static IEnumerable TestCanRemoveStaleEntriesCases
            {
                get
                {
                    yield return new TestCaseData("", new Predicate<string>(e => false), new string[] { })
                        .SetName("TestCanRemoveStaleEntriesEmptyDatabase");

                    yield return new TestCaseData(
                        string.Join(Environment.NewLine,
                            new Entry(@"c:\tree\", 102, DateTime.Now, false)),
                        new Predicate<string>(e => true),
                        new string[] { @"c:\tree" })
                            .SetName("TestCanRemoveStaleEntriesOneItemExists");

                    yield return new TestCaseData(
                        string.Join(Environment.NewLine,
                            new Entry(@"c:\tree\", 102, DateTime.Now, false)),
                        new Predicate<string>(e => false),
                        new string[] { })
                            .SetName("TestCanRemoveStaleEntriesOneItemDoesNotExist");

                    yield return new TestCaseData(
                        string.Join(Environment.NewLine,
                            new Entry(@"c:\tools\", 101, DateTime.Now, false),
                            new Entry(@"c:\tree\", 102, DateTime.Now, false)),
                        new Predicate<string>(e => e == @"c:\tools"),
                        new string[] { @"c:\tools" })
                            .SetName("TestCanRemoveStaleEntriesTwoItems");
                }
            }
        }
	}
}

