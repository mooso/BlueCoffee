using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SplitAndMerge
{
	public class Program
	{
		static void Main(string[] args)
		{
			Combine(args[0]);
		}

		public static void Split(string filePath, int parts = 2)
		{
			var fileLength = new FileInfo(filePath).Length;
			var basePartLength = fileLength / parts;
			var remainder = fileLength % parts;
			var buffer = new byte[1024 * 1024];
			using (var inputStream = File.OpenRead(filePath))
			{
				for (int i = 0; i < parts; i++)
				{
					using (var outputStream = File.Create(filePath + "." + i))
					{
						var wantedLength = (i < remainder) ? basePartLength + 1 : basePartLength;
						CopyStreamToStream(inputStream, outputStream, wantedLength, buffer);
					}
				}
			}
		}

		public static void Combine(string baseFilePath)
		{
			var parts = Directory.EnumerateFiles(Path.GetDirectoryName(baseFilePath), Path.GetFileName(baseFilePath) + "*")
				.Where(f => Regex.IsMatch(f, @".*\.[\d]+"));
			var buffer = new byte[1024 * 1024];
			using (var outputStream = File.Create(baseFilePath))
			{
				foreach (var part in parts)
				{
					using (var inputStream = File.OpenRead(part))
					{
						CopyStreamToStream(inputStream, outputStream, long.MaxValue, buffer);
					}
				}
			}
		}

		private static void CopyStreamToStream(Stream input, Stream output, long length, byte[] buffer)
		{
			for (long i = 0; i < length; i += buffer.LongLength)
			{
				var actualCount = input.Read(buffer, 0,
					(int)Math.Min(buffer.Length, length - i));
				if (actualCount <= 0)
				{
					return;
				}
				output.Write(buffer, 0, actualCount);
			}
		}
	}
}
