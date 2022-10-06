using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using Common;
using DistEdu.BooleanSearch.DataSource;
using DistEdu.BooleanSearch.Indexing;
using DistEdu.BooleanSearch.Searching;

namespace DistEdu.BooleanSearch;

public sealed class Program
{
    private static readonly string OutputDirectory = $"{Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName}/Output";

    public static async Task Main()
    {
        const string filename = "Input/Custom_csv.csv";

        var storage = new BookFileDataSource(filename);

        var index = BuildIndex(storage);

        Console.WriteLine("Save? y?");
        Console.Write("> ");
       
        if (Console.ReadKey().Key is ConsoleKey.Y)
        {
            Console.WriteLine();
            await SaveIndex((InvertedIndex)index);   
        }
        else
        {
            Console.WriteLine("Skipping saving");
        }

        QueryMode(index, storage);

        Console.ReadKey();
    }

    private static void QueryMode(IIndex index, IBookDataSource bookDataSource)
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
                    SimpleUserQueryMode(index, bookDataSource);
                    
                    break;
                }
                case 2:
                {
                    Console.WriteLine("To exit, write \'break;\'");
                    EtUserQueryMode(index, bookDataSource);
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

    private static IIndex BuildIndex(IBookDataSource storage)
    {
        Console.WriteLine("Start indexing");

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var indexBuilder = new InvertedIndexBuilder();
        var index = indexBuilder.BuildIndex(storage);

        stopWatch.Stop();
        Console.WriteLine($"Indexing is finished in {stopWatch.ElapsedMilliseconds} ms");

        Console.WriteLine();
        Console.WriteLine($"Books lines: {storage.GetAllBooks().Count}.");
        Console.WriteLine($"Inverted index terms size: {index.Size()}.");
        Console.WriteLine($"Inverted index memory size: {GetObjectSize(index)}.");
        Console.WriteLine();

        return index;
    }

    private static Task SaveIndex(IIndex index)
    {
        if (index is not InvertedIndex invertedIndex)
        {
            return Task.CompletedTask;
        }
        
        if (Directory.Exists(OutputDirectory) is false)
        {
            Directory.CreateDirectory(OutputDirectory);
        }
        
        return Task.WhenAll(invertedIndex.WriteJsonFileAsync(OutputDirectory, nameof(InvertedIndex)), invertedIndex.WriteMsgPackFileAsync(OutputDirectory, nameof(InvertedIndex)));
    }
    
    private static void EtUserQueryMode(IIndex index, IBookDataSource storage)
    {
        Console.CancelKeyPress += CancelHandler;
        var stopWatch = new Stopwatch();
        
        var searcher = new EtSearcher(index, storage);

        while (true)
        {
            Console.WriteLine();
            Console.Write("Enter the query: ");
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
    
    private static void SimpleUserQueryMode(IIndex index, IBookDataSource storage)
    {
        Console.CancelKeyPress += CancelHandler;
        var stopWatch = new Stopwatch();

        var searcher = new SimpleSearcher(index, storage);

        while (true)
        {
            Console.WriteLine();
            Console.Write("Enter the query: ");
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

    [Obsolete("Obsolete")]
    private static int GetObjectSize(object testObject)
    {
        var bf = new BinaryFormatter();
        var ms = new MemoryStream();
        bf.Serialize(ms, testObject);
        var array = ms.ToArray();
        return array.Length;
    }

    private static void CancelHandler(object sender, ConsoleCancelEventArgs args)
    {
        Console.WriteLine("Exit application");

        Environment.Exit(0);
    }
}