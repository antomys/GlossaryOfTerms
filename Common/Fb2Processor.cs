using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using FB2Library;
using FB2Library.Elements;
using FB2Library.Elements.Poem;

namespace Common;

public sealed partial class Fb2Processor
{
    private static readonly Regex Regex = new(@"[^a-zа-яA-ZА-Я0-9]", RegexOptions.Compiled);
    
    public static Task ProcessFb2(FB2File fb2File, ConcurrentDictionary<string, int> glossaryOfTerms) 
    {
        foreach (var body in fb2File.Bodies)
        {
            ProcessString(body.Name, glossaryOfTerms);
            ProcessTitle(body.Title, glossaryOfTerms);
            ProcessSections(body.Sections, glossaryOfTerms);
        }
        
        return Task.CompletedTask;
    }

    private static void ProcessSections(List<SectionItem>? sectionItems, ConcurrentDictionary<string, int> glossaryOfTerms)
    {
        if (sectionItems is null || sectionItems.Count is 0)
        {
            return;
        }
        
        foreach (var sectionItem in sectionItems)
        {
            ProcessString(sectionItem.ID, glossaryOfTerms);
            ProcessTitle(sectionItem.Title, glossaryOfTerms);
            ProcessEpigraphs(sectionItem.Epigraphs, glossaryOfTerms);
            ProcessSectionItems(sectionItem.Content, glossaryOfTerms);
        }
    }
    
    private static void ProcessSectionItems(List<IFb2TextItem>? sectionItems, ConcurrentDictionary<string, int> glossaryOfTerms)
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
                    ProcessString(castedSectionItem.ID, glossaryOfTerms);
                    ProcessTitle(castedSectionItem.Title, glossaryOfTerms);
                    ProcessEpigraphs(castedSectionItem.Epigraphs, glossaryOfTerms);
                    ProcessParagraphItem(castedSectionItem.Content, glossaryOfTerms);
                    break;

                case ParagraphItem castedParagraphItem:
                    ProcessParagraphData(castedParagraphItem.ParagraphData, glossaryOfTerms);
                    break;
            }
        }
    }

    private static void ProcessParagraphItem(List<IFb2TextItem>? sectionItems, ConcurrentDictionary<string, int> glossaryOfTerms)
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
            
            ProcessString(castedParagraphItem.ID, glossaryOfTerms);
            ProcessParagraphData(castedParagraphItem.ParagraphData, glossaryOfTerms);
        }
    }
    
    private static void ProcessTitle(TitleItem? bodyItem, ConcurrentDictionary<string, int> glossaryOfTerms)
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
            
            ProcessString(castedTitle.Lang, glossaryOfTerms);
            ProcessString(castedTitle.Style, glossaryOfTerms);
            ProcessString(castedTitle.ID, glossaryOfTerms);
            ProcessString(castedTitle.ToString(), glossaryOfTerms);
        }
    }

    private static void ProcessParagraphData(IList<StyleType>? styleTypes, ConcurrentDictionary<string, int> glossaryOfTerms)
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
            
            ProcessString(castedSimpleText.Text, glossaryOfTerms);
        }
    }
    
    private static void ProcessEpigraphs(IList<EpigraphItem>? epigraphItems, ConcurrentDictionary<string, int> glossaryOfTerms)
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
                
                ProcessStanzaItems(castedPoemItem.Content, glossaryOfTerms);
            }
            ProcessTextAuthorItems(epigraphItem.TextAuthors, glossaryOfTerms);
        }
    }
    
    private static void ProcessStanzaItems(IList<IFb2TextItem>? epigraphItems, ConcurrentDictionary<string, int> glossaryOfTerms)
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
                ProcessParagraphData(line.ParagraphData, glossaryOfTerms);
            }
        }
    }
    
    private static void ProcessTextAuthorItems(IList<IFb2TextItem>? epigraphItems, ConcurrentDictionary<string, int> glossaryOfTerms)
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
            
            ProcessParagraphData(castedTextAuthorItem.ParagraphData, glossaryOfTerms);
        }
    }

    private static void ProcessString(string str, ConcurrentDictionary<string, int> glossaryOfTerms)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return;
        }
        
        var subStr = Regex.Split(str);

        foreach (ref var splitStr in subStr.AsSpan())
        {
            if (string.IsNullOrWhiteSpace(splitStr))
            {
                continue;
            }
            
            splitStr = splitStr.ToLower();
            glossaryOfTerms.AddOrUpdate(splitStr, _ => 1, (_, i) => Interlocked.Increment(ref i));
        }
    }
}