using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeleteDuplicateDirectories
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			if (args.Length != 2 || !new[] { "delete", "analyze" }.Contains (args [1].ToLower ()) || 
				!Directory.Exists (args [0])) {
				Console.Error.WriteLine ("Usage: DeleteDuplicateDirectories <root filepath> <delete|analyze>");
			} 
			else 
			{
				bool performDeletion = args [1].ToLower () == "delete";
				string rootDir = args [0];

				IDictionary<string, IList<string>> directories = new Dictionary<string, IList<string>> ();

				foreach (string subdirectory in Directory.GetDirectories(rootDir, "*", 
				                                                         SearchOption.TopDirectoryOnly)) {

					string checksum = new DirectoryTools (subdirectory).CalculateRecursiveMD5Checksum ();

					if (directories.ContainsKey (checksum)) {
						directories [checksum].Add (subdirectory);
					} 
					else 
					{
						directories.Add (checksum, new List<string> () { subdirectory });
					}

				}

				IEnumerable<KeyValuePair<string, IList<string>>> duplicates = directories.Where (x => x.Value.Skip (1).Any ());

				if (duplicates.Any ()) 
				{
					Console.WriteLine ("The following subdirectories are duplicates and can safely be removed:");
					long totalSizeInBytes = 0;
					foreach (KeyValuePair<string, IList<string>> duplicate in duplicates) 
					{
						foreach (string directory in duplicate.Value.Skip(1)) 
						{
							long size = DirectoryTools.CalculateSize (directory);
							totalSizeInBytes += size;
							Console.WriteLine (string.Format ("{0} [{1}]", directory, GetNiceSizeString(size)));
						}

					}

					Console.WriteLine ("Total space consumed by duplicates: " + GetNiceSizeString (totalSizeInBytes));

					if (performDeletion) 
					{
						Console.Write ("Are you sure you want to delete these directories? (y/n): ");
						if (new string (new [] { Console.ReadKey().KeyChar }).ToLower () == "y") 
						{
							Parallel.ForEach (duplicates.SelectMany (x => x.Value.Skip (1)),
							                 directory => Directory.Delete (directory, true));
						}

						Console.WriteLine (Environment.NewLine + "Finished.");
					}
				}

			}


		}

		public static string GetNiceSizeString(long bytes)
		{
			string[] sizes = new[]{ "B", "KB", "MB", "GB", "TB" };

			int order = 0;
			while (bytes >= 1024 && order + 1 < sizes.Length) {
				order++;
				bytes = bytes / 1024;
			}

			return String.Format("{0:0.##} {1}", bytes, sizes[order]);
		}
	}
}
