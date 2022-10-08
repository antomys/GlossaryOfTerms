using System.Diagnostics;
using DistEdu.BooleanSearch.Searching;
using DistEdu.Common;
using DistEdu.Index.Interfaces;

namespace DistEdu.BooleanSearch;

public sealed class BooleanSearchService : ServiceBase
{
    private readonly string[] _fileNames;
    private readonly IIndex _index;

    private BooleanSearchService(IIndex index, string folderName, string filesExtension, string serviceName)
        : base(folderName, filesExtension, serviceName)
    {
        _index = index;
        _fileNames = GetFiles()
            .Select(file => file[(file.LastIndexOf(Path.DirectorySeparatorChar) + 1)..])
            .ToArray();
    }

    public static BooleanSearchService CreateInstance(IIndex index, string folderName, string filesExtension, string serviceName)
    {
        return new BooleanSearchService(index, folderName, filesExtension, serviceName);
    }

    public void QueryMode()
    {
        while (true)
        {
            Console.WriteLine("Please choose query mode. \nTo exit, press Ctrl + C\n");
            Console.WriteLine("1.Simple query mode\n2.Expression Tree mode.");
            Console.Write("> ");
            int.TryParse(Console.ReadLine(), out var number);

            switch (number)
            {
                case 1:
                {
                    Console.WriteLine("To exit, write \'break;\'");
                    SimpleUserQueryMode(_index, _fileNames);

                    break;
                }
                case 2:
                {
                    Console.WriteLine("To exit, write \'break;\'");
                    EtUserQueryMode(_index, _fileNames);
                   
                    break;
                }
                default:
                {
                    Console.WriteLine("Invalid input.Exiting.\nPress any key...");

                    break;
                }
            }
        }
    }

    private static void EtUserQueryMode(IIndex index, string[] fileNames)
    {
        var stopWatch = new Stopwatch();

        var searcher = new EtSearcher(index, fileNames);

        while (true)
        {
            Console.WriteLine();
            Console.Write("Enter the query: ");
            var query = Console.ReadLine();

            if (query is "break;") return;

            stopWatch.Reset();
            stopWatch.Start();

            try
            {
                // var result = searcher.Search(query);
                var result = searcher.SearchV2(query);

                stopWatch.Stop();

                // PrintResultsV1(result.ToList(), stopWatch.ElapsedMilliseconds, fileNames);
                PrintResultsV2(result, stopWatch.ElapsedMilliseconds, fileNames);
            }
            catch
            {
                Console.WriteLine();
                Console.WriteLine("Invalid query format");
                Console.WriteLine();
            }
        }
    }

    private static void SimpleUserQueryMode(IIndex index, string[] fileNames)
    {
        var stopWatch = new Stopwatch();

        var searcher = new SimpleSearcher(index, fileNames);

        while (true)
        {
            Console.WriteLine();
            Console.Write("Enter query: ");
            var query = Console.ReadLine();

            if (query is "break;") return;

            stopWatch.Reset();
            stopWatch.Start();

            try
            {
                var result = searcher.Search(query);

                stopWatch.Stop();

                PrintResultsV1(result, stopWatch.ElapsedMilliseconds, fileNames);
            }
            catch
            {
                Console.WriteLine();
                Console.WriteLine("Invalid query format");
                Console.WriteLine();
            }
        }
    }

    private static void PrintResultsV1(IReadOnlyList<string> result, long timeMeasure, IReadOnlyList<string> fileNames)
    {
        Console.WriteLine();
        var maxShow = Math.Min(result.Count, 10);

        if (maxShow > 0)
        {
            Console.WriteLine($"First {maxShow} results: ");
            for (var r = 0; r < maxShow; r++)
            {
                var span = result[r].AsSpan();

                var index = SafeCastToInt(span[..span.IndexOf(':')]);
                var book = fileNames[index];
                Console.WriteLine($"{r}. Id: {span}; Book: {book};");
            }

            Console.WriteLine();
        }

        Console.WriteLine($"Total results: {result.Count}. Search time: {timeMeasure} ms.");
    }

    private static void PrintResultsV2(KeyValuePair<string, HashSet<string>>[]? result,
        long stopWatchElapsedMilliseconds, string[] fileNames)
    {
        Console.WriteLine();

        if (result is null)
        {
            Console.WriteLine($"Total results: 0. Search time: {stopWatchElapsedMilliseconds} ms.");

            return;
        }

        var maxShow = Math.Min(result.Length, 10);

        if (maxShow > 0)
        {
            Console.WriteLine($"First {maxShow} results: ");
            for (var r = 0; r < maxShow; r++)
            {
                var kvp = result[r];
                var value = kvp.Key.AsSpan();

                foreach (var id in kvp.Value)
                {
                    var index = SafeCastToInt(id[..id.IndexOf(':')]);
                    var valueId = id[(id.LastIndexOf(':') + 1)..];
                    var book = fileNames[index];

                    Console.WriteLine($"{r}. Id: {valueId}; Book: {book}; Value: {value}");
                }
            }

            Console.WriteLine();
        }

        Console.WriteLine($"Total results: {result.Length}. Search time: {stopWatchElapsedMilliseconds} ms.");
    }

    private static int SafeCastToInt(ReadOnlySpan<char> span)
    {
        return int.Parse(span);
    }
}