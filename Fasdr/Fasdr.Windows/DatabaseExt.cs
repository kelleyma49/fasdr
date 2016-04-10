using Fasdr.Backend;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fasdr.Windows
{
    public static class DatabaseExt
    {
        public static int AddFromJumplists(this Database database,string providerName)
        {
            int numAdded = 0;
            var dirsAdded = new Dictionary<string, string>();
	
            var jumpListPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),@"Microsoft\Windows\Recent\AutomaticDestinations\");
            foreach (var f in Directory.EnumerateFiles(jumpListPath, "*.automaticDestinations-ms"))
            {
                foreach (var entry in JumpList.JumpList.LoadAutoJumplist(f).DestListEntries)
                {
                    try
                    {
                        var path = Path.GetFullPath(entry.Path);
                        // get the directory of the file:
                        if (File.Exists(path))
                        {
                            path = Path.GetDirectoryName(path);
                            if (database.AddEntry(providerName, path, p => false))
                            {
                                numAdded++;
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            return numAdded;
        }

        public static int AddFromRecents(this Database database, string providerName)
        {
            int numAdded = 0;
            var dirsAdded = new Dictionary<string, string>();

            var jumpListPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Recent));
            foreach (var f in Directory.EnumerateFiles(jumpListPath, "*.lnk"))
            {
                try
                {
                    var targetPath = GetShortcutTargetFile(f);
                    if (File.Exists(targetPath))
                    {
                        targetPath = Path.GetDirectoryName(targetPath);
                    }
                    if (Directory.Exists(targetPath))
                    {
                        if (database.AddEntry(providerName, targetPath, p => false))
                        {
                            numAdded++;
                        }
                    }
                }
                catch(Exception)
                {

                }                
            }

            return numAdded;
        }

        private static string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = System.IO.Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = System.IO.Path.GetFileName(shortcutFilename);

            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder folder = shell.NameSpace(pathOnly);
            Shell32.FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link =
                (Shell32.ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }
            return ""; // not found
        }
    }
}
