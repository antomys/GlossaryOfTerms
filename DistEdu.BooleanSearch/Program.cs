﻿using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using DistEdu.BooleanSearch.DataSource;
using DistEdu.BooleanSearch.Indexing;
using DistEdu.BooleanSearch.Searching;

namespace DistEdu.BooleanSearch;

/// <summary>
///     TODO:
///     * Save index on disk to restore it on next start. Check index file date and source file modify date and rebuild
///     index if needed.
///     * Write unit tests
///     * Clean up the code.
///     It's just a example, so I don't want to implement jokers or checking typo in terms or do search query plan
///     optimizing. It's a path of samurai, endless path.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        // const string Filename = "notebooks_210000.csv";
        const string Filename = "Custom_csv.csv";
        /*var arguments = new List<string>() { "--generate", "210000" };
        args = arguments.ToArray();*/
        
        var storage = new BookFileDataSource(Filename);

        var index = BuildIndex(storage);
        
        UserQueryMode(index, storage);

        Console.ReadKey();
    }

    private static IIndex BuildIndex(BookFileDataSource storage)
    {
        Console.WriteLine("Start indexing");

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var indexBuilder = new InvertedIndexBuilder();
        var index = indexBuilder.BuildIndex(storage);

        stopWatch.Stop();
        Console.WriteLine($"Indexing is finished in {stopWatch.ElapsedMilliseconds} ms");

        Console.WriteLine();
        Console.WriteLine($"Notebooks: {storage.GetAllBooks().Count}.");
        Console.WriteLine($"Inverted index terms size: {index.Size()}.");
        Console.WriteLine($"Inverted index memory size: {GetObjectSize(index)}.");
        Console.WriteLine();

        return index;
    }

    private static void UserQueryMode(IIndex index, BookFileDataSource storage)
    {
        Console.CancelKeyPress += CancelHandler;
        var stopWatch = new Stopwatch();

        var searcher = new EtSearcher(index, storage);

        while (true)
        {
            Console.WriteLine();
            Console.Write("Enter the query: ");
            var query = Console.ReadLine();
            stopWatch.Reset();
            stopWatch.Start();

            try
            {
                var result = searcher.Search(query);

                stopWatch.Stop();

                PrintResults(result, stopWatch.ElapsedMilliseconds, storage);
            }
            catch
            {
                Console.WriteLine();
                Console.WriteLine("Invalid query format");
                Console.WriteLine();
            }
        }
    }

    private static void PrintResults(IReadOnlyList<int> result, long timeMeasure, IBookDataSource storage)
    {
        Console.WriteLine();
        var maxShow = Math.Min(result.Count, 10);

        if (maxShow > 0)
        {
            Console.WriteLine($"First {maxShow} results: ");
            for (var r = 0; r < maxShow; r++)
            {
                var book = storage.GetAllBooks()[result[r]];
                Console.WriteLine($"Id {result[r]}. Book: {book.ToString()}");
            }

            Console.WriteLine();
        }

        Console.WriteLine($"Total results: {result.Count}. Search time: {timeMeasure} ms.");
    }

    private static int GetObjectSize(object TestObject)
    {
        var bf = new BinaryFormatter();
        var ms = new MemoryStream();
        byte[] Array;
        bf.Serialize(ms, TestObject);
        Array = ms.ToArray();
        return Array.Length;
    }

    private static void CancelHandler(object sender, ConsoleCancelEventArgs args)
    {
        Console.WriteLine("Exit application");

        Environment.Exit(0);
    }
}