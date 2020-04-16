using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TopPasswords
{
	internal class Program
	{

		private static Regex _regex = new Regex( @"^(?<email>(?:[a-zA-Z0-9!#$%&'*+\/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+\/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")\.?@[^:;@]+\.dk)(?<sep>[:;\ ])(?<password>.+)$", RegexOptions.IgnoreCase);
		private static string[] _dataFiles;
		private static string _root;

		private static void Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			if (args.Length != 1)
			{
				Console.WriteLine("Amount of arguments must be 1, which is the directory to look for all files in");
				Environment.Exit(1);
			}

			_root = args[0];
			_dataFiles = Directory.GetFiles(_root+"/DanishCombo");
			Console.WriteLine("Please select an option:");
			Console.WriteLine();
			Console.WriteLine("1. Top 100 Raw");						// Top 100 (or less if there aren't 100 entries) without altering data
			Console.WriteLine("2. Top 100 Cleaned");					// Top 100 (or less) with only one occurrence of same user/pass form a single file
			Console.WriteLine("3. Top 100 Cleaned and Blacklisted");	// -||- cleaned and applied a blacklist of all .txt files in /Blacklist
			Console.WriteLine("4. Domain Specific Info");				// TODO Make this
			Console.WriteLine("5. Count Unilogin (with top)");			// TODO Make this

			var command = Console.ReadLine();
			switch (command)
			{
				case "1":
					PrintTopToTSV(TopRaw());
					break;
				case "2":
					PrintTopToTSV(TopClean());
					break;
				case "3":
					PrintTopToTSV(BlackList(TopClean()));
					break;
				case "4":
					PrintDomainsToTSV(Domains());
					break;
				case "5":
					PrintTopToTSV(UniLogin());
					break;
				default:
					Console.WriteLine("Input not understood. Terminating.");
					Console.WriteLine("Press any key to exit");
					Console.ReadLine();
					Environment.Exit(1);
					break;
			}

		}

		private static IDictionary<string,long> TopRaw()
		{
			var dict = new Dictionary<string, long>();
			foreach (var file in _dataFiles)
			{
				if(!file.EndsWith(".filt")) continue;
				foreach(var line in File.ReadLines(file))
				{
					var matches = _regex.Match(line);
					var pass = matches.Groups["password"].Value;
					dict.TryGetValue(pass, out var count);
					dict[pass] = count + 1;
				}
			}

			return dict;
		}

		private static IDictionary<string,long> TopClean()
		{
			var dict = new Dictionary<string, long>();
			// Count passwords
			foreach (var file in _dataFiles)
			{
				if(!file.EndsWith(".fil2")) continue;
				foreach(var line in File.ReadLines(file))
				{
					var matches = _regex.Match(line);
					var pass = matches.Groups["password"].Value;
					dict.TryGetValue(pass, out var count);
					dict[pass] = count + 1;
				}
			}

			return dict;
		}

		private static IDictionary<string, long> BlackList(IDictionary<string, long> dictionary)
		{
			var blacklist = new HashSet<string>();
			foreach (var file in Directory.GetFiles(_root+"/Blacklist"))
			{
				blacklist.UnionWith(File.ReadLines(file));
			}

			foreach (var entry in blacklist)
			{
				dictionary.Remove(entry);
			}

			return dictionary;
		}

		private static IDictionary<string, long> UniLogin()
		{
			var uniloginRegex = new Regex(@"^[a-zA-Z]{3}[0-9]{2}[a-zA-Z]{3}$");
			var dict = new Dictionary<string, long>();
			// Count passwords
			foreach (var file in _dataFiles)
			{
				if(!file.EndsWith(".fil2")) continue;
				foreach(var line in File.ReadLines(file))
				{
					var matches = _regex.Match(line);
					var pass = matches.Groups["password"].Value;
					if(!uniloginRegex.IsMatch(pass)) continue;
					pass = pass.ToLower();
					dict.TryGetValue(pass, out var count);
					dict[pass] = count + 1;
				}
			}
			Console.WriteLine(dict.Count);
			return dict;
		}

		private static void PrintTopToTSV(IDictionary<string, long> topRaw)
		{
			var inverse = new Dictionary<long, HashSet<string>>();
			foreach (KeyValuePair<string, long> item in topRaw)
			{
				
				if(!inverse.TryGetValue(item.Value, out var pass)) pass = new HashSet<string>();
				pass.Add(item.Key);
				inverse[item.Value] = pass;
			}

			var sorted = inverse.ToList().OrderByDescending(i => i.Key).ToList();


			var amount = 100 < sorted.Count ? 100 : sorted.Count;
			for (var i = 0; i < amount; i++)
			{
				var entry = sorted[i];
				Console.Write(entry.Key);
				foreach (var password in entry.Value)
				{
					Console.Write("\t"+ password);
				}
				Console.WriteLine();
			}

		}

		private static IDictionary<string, IDictionary<string,IList<string>>> Domains()
		{
			var domains = File.ReadLines(_root + "/DanishDomains.txt").ToList().Where(s => !s.StartsWith("#")).ToList();

			var dict = new Dictionary<string, IDictionary<string,IList<string>>>();
			// Count passwords
			foreach (var file in _dataFiles)
			{
				if(!file.EndsWith(".fil2")) continue;
				foreach(var line in File.ReadLines(file))
				{
					var matches = _regex.Match(line);
					var email = matches.Groups["email"].Value;
					var domain = email.Split('@')[1];
					if (!domains.Contains(domain)) continue;
					var pass = matches.Groups["password"].Value;

					dict.TryGetValue(domain, out var domainDict);
					if(domainDict==null) domainDict = new Dictionary<string, IList<string>>();
					domainDict.TryGetValue(email, out var passList);
					if(passList == null) passList = new List<string>();
					passList.Add(pass);
					domainDict[email] = passList;
					dict[domain] = domainDict;
				}
			}

			return dict;
		}

		private static void PrintDomainsToTSV(IDictionary<string, IDictionary<string, IList<string>>> dictionary)
		{
			foreach (var domain in dictionary)
			{
				Console.WriteLine(domain.Key + "\n");
				foreach (var combo in domain.Value)
				{
					Console.Write(combo.Key);
					foreach (var pass in combo.Value)
					{
						Console.Write("\t" + pass);
					}
					Console.WriteLine();
				}
				Console.WriteLine();
			}
		}
	}
}
