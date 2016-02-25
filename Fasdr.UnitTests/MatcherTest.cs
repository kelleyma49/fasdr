using System;
using NUnit.Framework;
using Fasdr.Backend;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
using System.Linq;

namespace Fasdr.UnitTests
{
    [TestFixture]
    public class MatcherTest
	{
		static readonly string FileSystemConfigPath = System.IO.Path.Combine(Database.ConfigDir,$"{Database.ConfigFilePrefix}.FileSystem.txt");

     	private IDatabase SetupMatchSimple ()
		{
			Console.WriteLine ("test1");
			var mfs = new MockFileSystem(new Dictionary<string, MockFileData> {
				{  FileSystemConfigPath,new MockFileData( TestData.GetMatchFilesystem())
				}
			});
			var db = new Database(mfs);
			db.Load();
			Console.WriteLine("test2");
			return db;
		}

        [Test]
        public void TestMatch()
        {
			var db = SetupMatchSimple ();

            var matches = Matcher.Matches(db, "testStr", "FileSystem");
            Assert.AreEqual(2, matches.Count());
            CollectionAssert.AreEqual(new List<string>{@"c:\dir1\testStr", @"c:\testStr"},matches);
        }
    }
}
