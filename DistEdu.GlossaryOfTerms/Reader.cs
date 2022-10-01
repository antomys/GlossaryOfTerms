using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DistEdu.Common;
using FB2Library;
using FB2Library.Elements;
using FB2Library.Elements.Poem;

namespace DistEdu.GlossaryOfTerms;

public sealed class Reader
{
    private static readonly string OutputDirectory = $"{Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName}/Output";
    private static readonly Regex Regex = new(@"[^a-zа-яA-ZА-Я0-9]", RegexOptions.Compiled);

    private static readonly ConcurrentDictionary<string, int> GlossaryOfTerms = new();

    static Reader()
    {
        if (Directory.Exists(OutputDirectory) is false)
        {
            Directory.CreateDirectory(OutputDirectory);
        }
    }

    public static async Task ProcessValues(string folderName)
    {
        var filesInFolder = IoExtensions.GetFileNames(folderName, "fb2");

        var fb2Files = filesInFolder.Select(file => IoExtensions.ReadFb2FilesV2Async(file)
            .ContinueWith(task => ProcessFb2(task.Result), TaskContinuationOptions.AttachedToParent));
        
        await Task.WhenAll(fb2Files);
    }

    public static async Task WriteCustomFileAsync()
    {
        const string fileNameDir = "Custom.bin";
        
        var file = Path.Combine(OutputDirectory, fileNameDir);

        Console.WriteLine("Async Write File has started.");

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        await using(var outputFile = new StreamWriter(file))
        {
            foreach (var item in GlossaryOfTerms)
            {
                await outputFile.WriteLineAsync(item.ToString());
            }
        }
        Console.WriteLine("Async Write File has completed.");
    }

    public static Task WriteJsonFileAsync() =>
        GlossaryOfTerms.WriteJsonFileAsync(OutputDirectory, "GlossaryJson");
    
    public static Task WriteMsgPackFileAsync() =>
        GlossaryOfTerms.WriteMsgPackFileAsync(OutputDirectory, "GlossaryMsgPack");

    private static Task ProcessFb2(FB2File fb2File)
    {
        foreach (var body in fb2File.Bodies)
        {
            ProcessString(body.Name);
            ProcessTitle(body.Title);
            ProcessSections(body.Sections);
        }
        
        return Task.CompletedTask;
    }

    private static void ProcessSections(List<SectionItem>? sectionItems)
    {
        if (sectionItems is null || sectionItems.Count is 0)
        {
            return;
        }
        
        foreach (var sectionItem in sectionItems)
        {
            ProcessString(sectionItem.ID);
            ProcessTitle(sectionItem.Title);
            ProcessEpigraphs(sectionItem.Epigraphs);
            ProcessSectionItems(sectionItem.Content);
        }
    }
    
    private static void ProcessSectionItems(List<IFb2TextItem>? sectionItems)
    {
        if (sectionItems is null || sectionItems.Count is 0)
        {
            return;
        }
        
        foreach (var sectionItem in sectionItems)
        {
            switch (sectionItem)
            {
                case SectionItem castedSectionItem:
                    ProcessString(castedSectionItem.ID);
                    ProcessTitle(castedSectionItem.Title);
                    ProcessEpigraphs(castedSectionItem.Epigraphs);
                    ProcessParagraphItem(castedSectionItem.Content);
                    break;

                case ParagraphItem castedParagraphItem:
                    ProcessParagraphData(castedParagraphItem.ParagraphData);
                    break;
            }
        }
    }

    private static void ProcessParagraphItem(List<IFb2TextItem>? sectionItems)
    {
        if (sectionItems is null || sectionItems.Count is 0)
        {
            return;
        }
        
        foreach (var sectionItem in sectionItems)
        {
            if (sectionItem is not ParagraphItem castedParagraphItem)
            {
                continue;
            }
            
            ProcessString(castedParagraphItem.ID);
            ProcessParagraphData(castedParagraphItem.ParagraphData);
        }
    }
    
    private static void ProcessTitle(TitleItem? bodyItem)
    {
        if (bodyItem?.TitleData is null || bodyItem.TitleData?.Count is 0)
        {
            return;
        }
        
        foreach (var title in bodyItem.TitleData!)
        {
            if (title is not ParagraphItem castedTitle)
            {
                continue;
            }
            
            ProcessString(castedTitle.Lang);
            ProcessString(castedTitle.Style);
            ProcessString(castedTitle.ID);
            ProcessString(castedTitle.ToString());
        }
    }

    private static void ProcessParagraphData(IList<StyleType>? styleTypes)
    {
        if (styleTypes is null || styleTypes.Count is 0)
        {
            return;
        }
        
        foreach (var simpleText in styleTypes)
        {
            if (simpleText is not SimpleText castedSimpleText)
            {
                continue;
            }
            
            ProcessString(castedSimpleText.Text);
        }
    }
    
    private static void ProcessEpigraphs(IList<EpigraphItem>? epigraphItems)
    {
        if (epigraphItems is null || epigraphItems.Count is 0)
        {
            return;
        }
        
        foreach (var epigraphItem in epigraphItems)
        {
            foreach (var poemItem in epigraphItem.EpigraphData)
            {
                if (poemItem is not PoemItem castedPoemItem)
                {
                    continue;
                }
                
                ProcessStanzaItems(castedPoemItem.Content);
            }
            ProcessTextAuthorItems(epigraphItem.TextAuthors);
        }
    }
    
    private static void ProcessStanzaItems(IList<IFb2TextItem>? epigraphItems)
    {
        if (epigraphItems is null || epigraphItems.Count is 0)
        {
            return;
        }
        
        foreach (var stanzaItem in epigraphItems)
        {
            if (stanzaItem is not StanzaItem castedStanzaItem)
            {
                continue;
            }
            
            foreach (var line in castedStanzaItem.Lines)
            {
                ProcessParagraphData(line.ParagraphData);
            }
        }
    }
    
    private static void ProcessTextAuthorItems(IList<IFb2TextItem>? epigraphItems)
    {
        if (epigraphItems is null || epigraphItems.Count is 0)
        {
            return;
        }
        
        foreach (var fb2TextItem in epigraphItems)
        {
            if (fb2TextItem is not TextAuthorItem castedTextAuthorItem)
            {
                continue;
            }
            
            ProcessParagraphData(castedTextAuthorItem.ParagraphData);
        }
    }

    private static void ProcessString(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        
        var subStr = Regex.Split(str);

        foreach (ref var splitStr in subStr.AsSpan())
        {
            splitStr = splitStr.ToLower();
            GlossaryOfTerms.AddOrUpdate(splitStr, _ => 1, (_, i) => Interlocked.Increment(ref i));
        }
    }
}