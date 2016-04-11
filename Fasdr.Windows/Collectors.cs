using Fasdr.Backend;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fasdr.Windows
{
    public static class Collectors
    {
        //this Database database,, string providerName
        public static IDictionary<string,string> CollectPaths(IDictionary<string, string> paths,IEnumerable<string> pathsEnumerator)
        {
            foreach (var p in pathsEnumerator)
            {
                paths[p.ToLower()] = p;
            }
            return paths;
        }

        public static IEnumerable<string> EnumerateJumpLists()
        {
            var jumpListPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Recent\AutomaticDestinations\");
            foreach (var f in Directory.EnumerateFiles(jumpListPath, "*.automaticDestinations-ms"))
            {
                foreach (var entry in JumpList.JumpList.LoadAutoJumplist(f).DestListEntries)
                {
                    string fullPath = null;
                    try
                    {
                        fullPath = Path.GetFullPath(entry.Path);
                    }
                    catch (Exception)
                    {

                    }

                    if (fullPath != null)
                        yield return fullPath;
               }
            }
        }

        public static IEnumerable<string> EnumerateRecents()
        {
            var recentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Recent);
            foreach (var f in Directory.EnumerateFiles(recentsPath, "*.lnk"))
            {
                string path = null;
                try
                {
                    path = GetShortcutTargetFile(f);
                }
                catch (Exception)
                {

                }

                if (path != null)
                    yield return path;
            }

        }

        public static IEnumerable<string> EnumerateSpecialFolders()
        {
            yield return System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            yield return System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonPictures);
            yield return System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonProgramFiles);
            yield return System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonProgramFilesX86);
            yield return System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonPrograms);
            yield return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            yield return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Favorites);
            yield return System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            yield return System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            yield return System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic);
            yield return System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            yield return System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyVideos);
            yield return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
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
