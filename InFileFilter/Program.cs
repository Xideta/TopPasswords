using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InFileFilter
{
	class Program
	{

		private static readonly Regex _regex = new Regex( @"^(?<email>(?:[a-zA-Z0-9!#$%&'*+\/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+\/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")\.?@[^:;@]+\.dk)(?<sep>[:;\ ])(?<password>.+)$", RegexOptions.IgnoreCase);
		private static string[] _files;



		private static void Main(string[] args)
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
				var filtered = File.ReadLines(file).Where(line => _regex.IsMatch(line));
				filtered = filtered.Select(line => _regex.Replace(line, "${email}:${password}"));
				File.WriteAllLines(file+".filt", filtered);
			}

			
		}




	}
}
