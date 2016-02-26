using System;
using System.Text;
using Fasdr.Backend;

namespace Fasdr.UnitTests
{
	public static class TestData
	{
		public static string GetTwoDirFilesystem()
		{
			var sb = new StringBuilder ();
			sb.AppendLine(string.Join ("" + Provider.Separator, @"c:\dir1\", "101.0", "true"));
			sb.AppendLine(string.Join("" + Provider.Separator,@"c:\dir1\file2", "10.0", "false"));
			return sb.ToString ();
		}

		public static string GetMatchFilesystem()
		{
			var sb = new StringBuilder ();
			sb.AppendLine (string.Join ("" + Provider.Separator, @"c:\dir1\testStr", "101.0","true"));			
			sb.AppendLine (string.Join ("" + Provider.Separator, @"c:\testStr", "10.0", "false"));
			return sb.ToString ();
		}
	}
}

