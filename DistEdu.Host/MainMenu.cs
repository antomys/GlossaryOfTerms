using DistEdu.BooleanSearch;
using DistEdu.CoordinatedInvertedIndex;
using DistEdu.CoordSearch;
using DistEdu.Dictionary;
using DistEdu.DoubleIndex;
using DistEdu.Index;
using DistEdu.Matrix;

namespace DistEdu.Host;

public sealed class MainMenu
{
    private static InvertedIndex? _index;
    private static DoubleIndex.DoubleIndex? _doubleIndex;
    private static CoordinatedIndexes? _coordinatedIndexes;

    private readonly DictionaryService _dictionaryService;
    private readonly MatrixService _matrixService;
    private readonly InvertedIndexService _invertedIndexService;
    private readonly DoubleIndexService _doubleIndexService;
    private readonly CoordinatedIndexService _coordinatedIndexService;

    public MainMenu(string folderName, string filesExtension = "fb2")
    {
        _coordinatedIndexService = new CoordinatedIndexService(folderName, filesExtension);
        _dictionaryService = new DictionaryService(folderName, filesExtension);
        _matrixService = new MatrixService(folderName, filesExtension);
        _invertedIndexService = new InvertedIndexService(folderName, filesExtension);
        _doubleIndexService = new DoubleIndexService(folderName, filesExtension);
    }

    public async Task PrintAsync(string folderName, string filesExtension = "fb2")
    {
        do
        {
            Console.Write("Please choose job:\n" +
                          "1. Simple process(task1)\n" +
                          "2. Build adjacency matrix\n" +
                          "3. Build inverted index\n" +
                          "4. Boolean search\n" +
                          "5. Build double index\n" +
                          "6. Build coordinated inverted index\n" +
                          "7. Phrasal search\n" +
                          "0. Exit\n");

            Console.Write("\n> ");
            int.TryParse(Console.ReadLine(), out var inputNumber);

            switch (inputNumber)
            {
                case 1:
                {
                    await ProcessDictionaryAsync();

                    break;
                }
                case 2:
                {
                    await ProcessMatrixAsync();

                    break;
                }
                case 3:
                {
                    await ProcessIndexAsync();

                    break;
                }
                case 4:
                {
                    await ProcessBooleanSearchAsync(folderName, filesExtension);

                    break;
                }
                case 5:
                {
                    await ProcessDoubleIndexAsync();

                    break;
                }
                case 6:
                {
                    await ProcessCoordinatedIndexAsync();

                    break;
                }
                
                case 7:
                {
                    await ProcessPhrasalSearchAsync();

                    break;
                }
                case 0:
                {
                    return;
                }
                default:
                {
                    Console.Write("Invalid input. Try again");

                    break;
                }
            }
        } while (true);
    }

    private async Task ProcessDictionaryAsync()
    {
        await _dictionaryService.ProcessAsync();

        await Task.WhenAll(_dictionaryService.WriteFileAsync(), _dictionaryService.WriteJsonFileAsync(),
            _dictionaryService.WriteMsgPackFileAsync());

        _dictionaryService.GetStatistics();
    }

    private async Task ProcessMatrixAsync()
    {
        await _matrixService.GetAdjacencyMatrix();

        await Task.WhenAll(_matrixService.WriteAdjacencyMatrixAsync(),
            _matrixService.WritePrettyAdjacencyMatrixAsync());
    }

    private async Task ProcessIndexAsync()
    {
        await GetOrCreateIndexAsync();
        await _invertedIndexService.SaveIndexAsync();

        _invertedIndexService.GetStatistics();
    }
    
    private async Task ProcessDoubleIndexAsync()
    {
        await GetOrCreateDoubleIndexAsync();
        await _doubleIndexService.SaveIndexAsync();

        _doubleIndexService.GetStatistics();
    }
    
    private async Task ProcessCoordinatedIndexAsync()
    {
        await GetOrCreateCoordinatedIndexAsync();
        await _coordinatedIndexService.SaveIndexAsync();

        // _coordinatedIndexService.GetStatistics();
    }

    private async Task<InvertedIndex> GetOrCreateIndexAsync()
    {
        if (_index is not null) return _index;
        
        Console.WriteLine("Index is null, Trying to get from files or build.");

        _index = await _invertedIndexService.GetOrBuildAsync();

        return _index!;
    }
    
    private async Task<DoubleIndex.DoubleIndex?> GetOrCreateDoubleIndexAsync()
    {
        if (_doubleIndex is not null)
        {
            return _doubleIndex;
        }
        
        Console.WriteLine("Index is null, Trying to get from files or build.");

        _doubleIndex = await _doubleIndexService.GetOrBuildAsync();

        return _doubleIndex!;
    }
    
    private async Task<CoordinatedIndexes?> GetOrCreateCoordinatedIndexAsync()
    {
        if (_coordinatedIndexes is not null)
        {
            return _coordinatedIndexes;
        }
        
        Console.WriteLine("Index is null, Trying to get from files or build.");

        _coordinatedIndexes = await _coordinatedIndexService.GetOrBuildAsync();

        return _coordinatedIndexes;
    }

    private async Task ProcessPhrasalSearchAsync()
    {
        var doubleIndex = await GetOrCreateDoubleIndexAsync();
        var coordIndexes = await GetOrCreateCoordinatedIndexAsync();

        var booleanSearchService = new CoordService(doubleIndex!, coordIndexes!);
        
        booleanSearchService.QueryMode();
    }

    private async Task ProcessBooleanSearchAsync(string folderName, string filesExtension = "fb2")
    {
        var index = await GetOrCreateIndexAsync();
        
        var booleanSearchService =
            BooleanSearchService.CreateInstance(index, folderName, filesExtension, nameof(BooleanSearchService));

        booleanSearchService.QueryMode();
    }
}