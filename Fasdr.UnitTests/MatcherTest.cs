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
        static readonly string FileSystemConfigPath = System.IO.Path.Combine(Database.DefaultConfigDir,$"{Database.ConfigFilePrefix}.FileSystem.txt");

		private IDatabase SetupEmptyDatabase ()
		{
			var mfs = new MockFileSystem(new Dictionary<string, MockFileData> {});
			return new Database(mfs);
		}

     	private IDatabase SetupMatchSimple ()
		{
			var mfs = new MockFileSystem(new Dictionary<string, MockFileData> {
				{  
					FileSystemConfigPath,new MockFileData( TestData.GetMatchFilesystem())
				}
			});
			var db = new Database(mfs);
			db.Load();
			return db;
		}
		[Test]
		public void TestEmptyDatabase()
		{
			var db = SetupEmptyDatabase ();

			var matches = Matcher.Matches(db, "FileSystem", "testStr");
			CollectionAssert.AreEqual(new List<string>{},matches);
		}


        [Test]
        public void TestSingleElementMatch()
        {
			var db = SetupMatchSimple ();

            var matches = Matcher.Matches(db, "FileSystem", "testStr");
            CollectionAssert.AreEqual(new List<string>{ @"c:\dir1\dir2\testStr", @"c:\dir1\testStr", @"c:\testStr"},matches);
        }

        [Test]
        public void TestDoubleElementMatch()
        {
            var db = SetupMatchSimple();

            var matches = Matcher.Matches(db, "FileSystem", "dir1", "testStr");
            CollectionAssert.AreEqual(new List<string> { @"c:\dir1\dir2\testStr", @"c:\dir1\testStr" }, matches);
        }

        [Test]
        public void TestDoubleElementNoMatch()
        {
            var db = SetupMatchSimple();

            var matches = Matcher.Matches(db, "FileSystem", "NotThere", "testStr");
            CollectionAssert.AreEqual(new List<string> {}, matches);
        }

		[Test]
		public void TestSingleElementNoMatch()
		{
			var db = SetupMatchSimple();

			var matches = Matcher.Matches(db, "FileSystem", "ShouldNotBeThere");
			CollectionAssert.AreEqual(new List<string> {}, matches);
		}

		[Test]
		public void TestSingleElementMatchUpdated()
		{
			var db = SetupMatchSimple ();

			var matches = Matcher.Matches(db, "FileSystem", "testStr");
			CollectionAssert.AreEqual(new List<string>{ @"c:\dir1\dir2\testStr", @"c:\dir1\testStr", @"c:\testStr"},matches);
			Assert.IsTrue(db.Providers["FileSystem"].UpdateEntry(@"c:\dir1\testStr"));
			matches = Matcher.Matches(db, "FileSystem", "testStr");
			CollectionAssert.AreEqual(new List<string>{ @"c:\dir1\testStr", @"c:\dir1\dir2\testStr", @"c:\testStr"},matches);
		}
    }
}
