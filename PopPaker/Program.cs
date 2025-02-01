using PopLib.Pak;
using System.Diagnostics;

namespace PopPaker;

internal static class Program
{
	private static void Main(string[] args)
	{
		if (args.Length != 3 || args[0] is not "extract" and not "pack")
		{
			Console.WriteLine("Usage:\n" +
							"    poppaker extract inputFilePath outputDirPath\n" +
							"    poppaker pack inputDirPath outputFilePath");

			return;
		}

		switch (args[0])
		{
			case "extract":
				Extract(args[1], args[2]);
				break;
			case "pack":
				Pack(args[1], args[2]);
				break;
		}
	}

	private static void Extract(string inputFilePath, string outputDirPath)
	{
		if (!File.Exists(inputFilePath))
		{
			Console.WriteLine("Input file not found.");
			return;
		}

		using var pak = PakArchive.OpenRead(inputFilePath);

		var maxNumWidth = pak.Entries.Count.ToString().Length;

		var terminalWidth = Console.BufferWidth;
		var progressWidth = (maxNumWidth * 2) + 3;

		var maxFileNameWidth = terminalWidth - progressWidth - 1;

		var (x, y) = Console.GetCursorPosition();

		var startTime = Stopwatch.GetTimestamp();
		
		for (var i = 0; i < pak.Entries.Count; i++)
		{
			var file = pak.Entries[i];
			var filePath = Path.Combine(outputDirPath, file.Name);
			
			var strLeft = $"Extracting {filePath}";
			if (strLeft.Length > maxFileNameWidth)
				strLeft = strLeft[..(maxFileNameWidth - 3)] + "...";
			
			Console.SetCursorPosition(x, y);
			Console.Write($"{strLeft.PadRight(maxFileNameWidth)} [{i + 1}/{pak.Entries.Count}]");
			file.ExtractToFile(filePath);
		}
		Console.WriteLine();
		
		var endTime = Stopwatch.GetTimestamp();
		var duration = endTime - startTime;
		var durationMs = duration * 1000.0 / Stopwatch.Frequency;
		Console.WriteLine($"Done. ({durationMs:0.000}ms)");
	}

	private static void Pack(string inputDirPath, string outputFilePath)
	{
		using var writer = new PakWriter(outputFilePath);
		var files = Directory.GetFiles(inputDirPath, "*", SearchOption.AllDirectories);

		var startTime = Stopwatch.GetTimestamp();

		foreach (var file in files)
		{
			var name = Path.GetRelativePath(inputDirPath, file).Replace('/', '\\');
			writer.SubmitFile(file, name);
		}

		writer.Write();
		
		var endTime = Stopwatch.GetTimestamp();
		var duration = endTime - startTime;
		var durationMs = duration * 1000.0 / Stopwatch.Frequency;
		Console.WriteLine($"Done. ({durationMs:0.000}ms)");
	}
}
