using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class main
{
	private static Regex _regex;
	private static string[] _subDirectories;
	private static string _outputFolder;


	/*
	 *
	 * Program will take a folder, go into every subfolder of it, go through all TXT files in it,
	 * and output lines matching the regex. Made for a research project that had a lot of txt files
	 * in a lot of folders...
	 *
	 * Main takes 2 to 3 arguments
	 * First argument is the regex to search for.
	 * Second argument is a folder name for it to look in.
	 * Third argument is an output folder.
	 */
	private static void Main(string[] args)
	{
		Console.OutputEncoding = System.Text.Encoding.UTF8;
		if (args.Length < 2 || args.Length > 3)
		{
			Console.WriteLine("Amount of arguments must be between 2 or 3");
			Environment.Exit(1);
		}

		_regex = new Regex(args[0]);
		_subDirectories = Directory.GetDirectories(args[1]);
		_outputFolder =  (args.Length == 3) ? args[2] : args[1];

		Console.WriteLine("Output Folder: " + _outputFolder);

		foreach (var dir in _subDirectories)
		{
			WriteFile(ReadAndFilterFiles(dir), dir);
		}

	}

	/*
	 * Given a folder path, reads all .txt files 
	 *
	 * Inspiration taken from https://stackoverflow.com/questions/20928705/read-and-process-files-in-parallel-c-sharp
	 */
	private static BlockingCollection<string> ReadAndFilterFiles(string folder)
	{
		var matchesCollection = new BlockingCollection<string>();

		var files = Directory.GetFiles(folder);
		
		Console.WriteLine("Reading Folder: " + folder);

		var readTask = Task.Run(() =>
		{
			try
			{
				foreach (var file in files)
				{
					try{
						if (!file.EndsWith(".txt"))
						{
							Console.WriteLine("Skipping: " + file);
							continue;
						}

						using (var reader = new StreamReader(file))
						{
							string line;

							while ((line = reader.ReadLine()) != null)
							{
								if (_regex.IsMatch(line)) matchesCollection.Add(line);
							}
						}
					}
					catch (DirectoryNotFoundException e)
					{
						Console.WriteLine(e.StackTrace);
					}
				}
			}
			
			finally
			{
				matchesCollection.CompleteAdding();
			}
		});

		Task.WaitAll(readTask);

		return matchesCollection;

	}

	private static void WriteFile(BlockingCollection<string> stringList, string directoryName)
	{
		if (stringList.Count == 0) return;
		directoryName = Path.GetFileName(directoryName);
		var outputFileName = Path.Combine(_outputFolder, directoryName) + ".txt";
		Console.WriteLine("Writing to: " + outputFileName);

		File.WriteAllLines(outputFileName, stringList);


	}

}
