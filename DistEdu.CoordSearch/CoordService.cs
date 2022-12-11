using System.Diagnostics;
using DistEdu.CoordinatedInvertedIndex;

namespace DistEdu.CoordSearch;

public sealed class CoordService
{
    private readonly DoubleIndex.DoubleIndex _doubleIndex;
    private readonly CoordinatedIndexes _coordinatedIndexes;

    public CoordService(DoubleIndex.DoubleIndex doubleIndex, CoordinatedIndexes coordinatedIndexes)
    {
        _doubleIndex = doubleIndex;
        _coordinatedIndexes = coordinatedIndexes;
    }

    public void QueryMode()
    {
        while (true)
        {
            Console.WriteLine("Please choose query mode. \nTo exit, press Ctrl + C\n");
            Console.WriteLine("1.Phrasal search\n2.Coordinate search.");
            Console.Write("> ");
            int.TryParse(Console.ReadLine(), out var number);

            switch (number)
            {
                case 1:
                {
                    Console.WriteLine("To exit, write \'break;\'");
                    DoubleIndexSearch(_doubleIndex);

                    break;
                }
                case 2:
                {
                    Console.WriteLine("To exit, write \'break;\'");
                    CoordInverseSearch(_coordinatedIndexes);
                   
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
    
    private static void DoubleIndexSearch(DoubleIndex.DoubleIndex doubleIndex)
    {
        var stopWatch = new Stopwatch();
        var searcher = new PhrasalSearchService(doubleIndex);

        while (true)
        {
            Console.WriteLine();
            Console.Write("Enter query: ");
            var query = Console.ReadLine();

            if (query is "break;")
            {
                return;
            }

            stopWatch.Reset();
            stopWatch.Start();

            try
            {
                var result = searcher.Search(query);

                stopWatch.Stop();

                PrintResults(result, stopWatch.ElapsedMilliseconds);
            }
            catch
            {
                Console.WriteLine();
                Console.WriteLine("Invalid query format");
                Console.WriteLine();
            }
        }
    }
    
    private static void CoordInverseSearch(CoordinatedIndexes coordinatedIndexes)
    {
        var stopWatch = new Stopwatch();
        var searcher = new CoordInverseIndexSearchService(coordinatedIndexes);

        while (true)
        {
            Console.WriteLine();
            Console.Write("Enter query: ");
            var query = Console.ReadLine();

            if (query is "break;")
            {
                return;
            }

            stopWatch.Reset();
            stopWatch.Start();

            try
            {
                var result = searcher.Search(query);

                stopWatch.Stop();

                PrintResults(result, stopWatch.ElapsedMilliseconds);
            }
            catch
            {
                Console.WriteLine();
                Console.WriteLine("Invalid query format");
                Console.WriteLine();
            }
        }
    }
    
    private static void PrintResults(
        string[] results,
        long stopWatchElapsedMilliseconds)
    {
        Console.WriteLine();

        if (results.Length is 0)
        {
            Console.WriteLine($"Total results: 0. Search time: {stopWatchElapsedMilliseconds} ms.");

            return;
        }

        var maxShow = Math.Min(results.Length, 10);

        Console.WriteLine($"First {maxShow} results: ");
        for (var i = 0; i < maxShow; i++)
        {
            Console.WriteLine($"{results[i]}.");
        }

        Console.WriteLine();

        Console.WriteLine($"Total results: {results.Length}. Search time: {stopWatchElapsedMilliseconds} ms.");
    }
}