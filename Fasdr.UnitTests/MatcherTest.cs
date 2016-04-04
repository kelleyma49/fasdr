using System;
using NUnit.Framework;
using Fasdr.Backend;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Fasdr.UnitTests
{
    [TestFixture]
    public class MatcherTest
    {
        static readonly string FileSystemConfigPath = System.IO.Path.Combine(Database.DefaultConfigDir, $"{Database.ConfigFilePrefix}.FileSystem.txt");

        private IDatabase SetupEmptyDatabase()
        {
            var mfs = new MockFileSystem(new Dictionary<string, MockFileData> { });
            return new Database(mfs);
        }

        private IDatabase SetupMatchSimple(string contents)
        {
            var mfs = new MockFileSystem(new Dictionary<string, MockFileData> {
                {
                    FileSystemConfigPath,new MockFileData(contents)
                }
            });
            var db = new Database(mfs);
            db.Load();
            return db;
        }

        private IDatabase SetupMatchSimple()
        {
            return SetupMatchSimple(TestData.GetMatchFilesystem());
        }


        [Test]
        public void TestEmptyDatabase()
        {
            var db = SetupEmptyDatabase();

            var matches = Matcher.Matches(db, "FileSystem", "testStr");
            CollectionAssert.AreEqual(new List<string> { }, matches);
        }


        [Test, TestCaseSource(typeof(MyFactoryClass),"TestSingleElementMatchCases")]
        public void TestSingleElementMatch(string configFileContents, string[] patterns, string[] expected)
        {
            var db = SetupMatchSimple(configFileContents);

            var actual = Matcher.Matches(db, "FileSystem", patterns);
            CollectionAssert.AreEqual(expected, actual);
        }


        public class MyFactoryClass
        {
            public static IEnumerable TestSingleElementMatchCases
            {
                get
                {
                    yield return new TestCaseData(TestData.GetMatchFilesystem(),
                        new string[] { "testStr" },
                        new string[] { @"c:\dir1\dir2\testStr", @"c:\dir1\testStr", @"c:\testStr"  })
                        .SetName("TestSingleElementMatchTestStr");

                    yield return new TestCaseData(TestData.GetMatchFilesystem(),
                        new string[] { "dir1", "testStr" },
                        new string[] { @"c:\dir1\dir2\testStr", @"c:\dir1\testStr", @"c:\testStr" })
                        .SetName("TestSingleElementMatchDir1TestStr");
                }
            }

            public static IEnumerable TestSingleElementNoMatchCases
            {
                get
                {
                    yield return new TestCaseData(TestData.GetMatchFilesystem(),new string[] { "NotThere" })
                        .SetName("TestElementNoMatchSingle");

                    yield return new TestCaseData(TestData.GetMatchFilesystem(), new string[] { "NotThere1" , "NotThere2" })
                        .SetName("TestElementNoMatchDouble");
                }
            }
        }

        [Test, TestCaseSource(typeof(MyFactoryClass), "TestSingleElementNoMatchCases")]
        public void TestElementNoMatch(string configFileContents, string[] patterns)
        {
            var db = SetupMatchSimple(configFileContents);

            var actual = Matcher.Matches(db, "FileSystem", patterns);
            CollectionAssert.AreEqual(new List<string> {}, actual);
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
