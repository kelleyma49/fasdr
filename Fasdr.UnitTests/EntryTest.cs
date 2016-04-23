using System;
using NUnit.Framework;
using Fasdr.Backend;

namespace Fasdr.UnitTests
{
	[TestFixture]
	public class EntryTest
	{
		[Test]
		public void TestCanCreateFromString()
		{
			var now = DateTime.UtcNow;
			var s = string.Join ("" + Entry.Separator, @"c:\dir1\dir2", "1", now.ToFileTimeUtc(), false);
			Entry e = Entry.FromString (s);
			Assert.AreEqual (@"c:\dir1\dir2", e.FullPath);
			Assert.AreEqual (new string[]{"c:","dir1","dir2"}, e.SplitPath);
			Assert.AreEqual (1, e.Frequency);
			Assert.AreEqual (now.ToFileTimeUtc (), e.LastAccessTimeUtc);
			Assert.IsFalse (e.IsLeaf);
			Assert.IsTrue (e.IsValid);
		}

		[Test]
		public void TestFailedParseEmptyString()
		{
			Assert.Throws<Exception>(()=>Entry.FromString (""));
		}

		[Test]
		public void TestFailedParseFrequency()
		{
			var now = DateTime.UtcNow;
			var s = string.Join ("" + Entry.Separator, @"c:\dir1\", "NotANumber", now.ToFileTimeUtc(), false);
			Assert.Throws<Exception>(()=>Entry.FromString (s));
		}

		[Test]
		public void TestFailedParseLastAccessTime()
		{
			var now = DateTime.UtcNow;
			var s = string.Join ("" + Entry.Separator, @"c:\dir1\", "1", "NotANumber", false);
			Assert.Throws<Exception>(()=>Entry.FromString (s));
		}

		[Test]
		public void TestFailedParseIsLeaf()
		{
			var now = DateTime.UtcNow;
			var s = string.Join ("" + Entry.Separator, @"c:\dir1\", "1", now.ToFileTimeUtc(), "NotBoolean");
			Assert.Throws<Exception>(()=>Entry.FromString (s));
		}
	}
}

