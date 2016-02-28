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
			var s = string.Join ("" + Entry.Separator, @"c:\dir1\", "1", now.ToFileTimeUtc(), false);
			Entry e = Entry.FromString (s);
			Assert.AreEqual (@"c:\dir1\", e.FullPath);
			Assert.AreEqual (1, e.Frequency);
			Assert.AreEqual (now.ToFileTimeUtc (), e.LastAccessTimeUtc);
			Assert.AreEqual (false, e.IsLeaf);
		}

		[Test]
		[ExpectedException(typeof(Exception))]
		public void TestFailedParseEmptyString()
		{
			Entry.FromString ("");
		}

		[Test]
		[ExpectedException(typeof(Exception))]
		public void TestFailedParseFrequency()
		{
			var now = DateTime.UtcNow;
			var s = string.Join ("" + Entry.Separator, @"c:\dir1\", "NotANumber", now.ToFileTimeUtc(), false);
			Entry.FromString (s);
		}

		[Test]
		[ExpectedException(typeof(Exception))]
		public void TestFailedParseLastAccessTime()
		{
			var now = DateTime.UtcNow;
			var s = string.Join ("" + Entry.Separator, @"c:\dir1\", "1", "NotANumber", false);
			Entry.FromString (s);
		}

		[Test]
		[ExpectedException(typeof(Exception))]
		public void TestFailedParseIsLeaf()
		{
			var now = DateTime.UtcNow;
			var s = string.Join ("" + Entry.Separator, @"c:\dir1\", "1", now.ToFileTimeUtc(), "NotBoolean");
			Entry.FromString (s);
		}
	}
}

