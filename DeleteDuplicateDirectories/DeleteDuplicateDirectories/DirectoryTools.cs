using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace DeleteDuplicateDirectories
{
	public class DirectoryTools
	{
		protected string Filepath { get; set; }

		public DirectoryTools (string filepath)
		{
			Filepath = filepath;
		}

		private string CalculateMD5ChecksumFromFile(string file) 
		{
			using (FileStream stream = File.OpenRead(file))
			{
				return CalculateMD5ChecksumFromStream (stream);
			}
		}

		private string CalculateMD5ChecksumFromStream(Stream stream) 
		{
			using (MD5 hashCalculator = MD5.Create())
				return BitConverter.ToString(hashCalculator.ComputeHash(stream)).Replace("-","").ToLower();
		}

		public string CalculateRecursiveMD5Checksum()
		{
			using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(CalculateIntermediateRecursiveMD5Checksum (Filepath))))
	        {
				return CalculateMD5ChecksumFromStream(stream);
			}
		}

		public string CalculateIntermediateRecursiveMD5Checksum(string rootDirectory) 
		{
			StringBuilder checksum = new StringBuilder ();

			foreach (string file in Directory.GetFiles(rootDirectory)) 
			{
				checksum.AppendLine (CalculateMD5ChecksumFromFile (file));
			}

			foreach (string directory in Directory.GetDirectories(rootDirectory, "*", SearchOption.AllDirectories)) 
			{
				checksum.AppendLine (CalculateIntermediateRecursiveMD5Checksum (directory));
			}

			return checksum.ToString ();
		}

		public static long CalculateSize(string rootDirectory)
		{
			return CalculateSize (new DirectoryInfo (rootDirectory));
		}

		public static long CalculateSize(DirectoryInfo rootDirectory)
		{
			long Size = 0;    

			foreach (FileInfo file in rootDirectory.GetFiles()) 
			{      
				Size += file.Length;    
			}

			foreach (DirectoryInfo directory in rootDirectory.GetDirectories()) 
			{
				Size += CalculateSize(directory);   
			}

			return Size; 
		}
	}
}
