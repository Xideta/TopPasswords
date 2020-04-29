using System;
using System.IO;
using System.Linq;

namespace Duplicate_Remover
{
	class Program
	{
		private static string[] _files;

		static void Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			if (args.Length != 1)
			{
				Console.WriteLine("Amount of arguments must be 1");
				Environment.Exit(1);
			}
			_files = Directory.GetFiles(args[0]);
			foreach (var file in _files)
			{
				Console.WriteLine(file);
				var filtered = File.ReadLines(file).ToHashSet();
				File.WriteAllLines(file+".fil2", filtered);
			}


		}
	}
}
