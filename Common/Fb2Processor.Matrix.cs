using System.Collections.Concurrent;
using FB2Library.Elements;
using FB2Library.Elements.Poem;

namespace Common;

public sealed partial class Fb2Processor
{
    public ConcurrentDictionary<string, HashSet<string>> ProcessFb2WithMatrixAsync()
    {
        var lexemes = new ConcurrentDictionary<string, HashSet<string>>();
        
        for (var bodyId = 0; bodyId < _fb2File.Bodies.Count; bodyId++)
        {
            var body = _fb2File.Bodies[bodyId];

            ProcessMatrixString(body.Name, lexemes);
            ProcessTitle(body.Title, lexemes);
            ProcessSections(body.Sections, lexemes);
        }

        return lexemes;
    }

    private void ProcessSections(List<SectionItem>? sectionItems, ConcurrentDictionary<string, HashSet<string>> lexemes)
    {
        if (sectionItems is null || sectionItems.Count is 0)
        {
            return;
        }

        for (var sectionId = 0; sectionId < sectionItems.Count; sectionId++)
        {
            var sectionItem = sectionItems[sectionId];
            
            ProcessMatrixString(sectionItem.ID, lexemes); 
            ProcessTitle(sectionItem.Title, lexemes);
            ProcessEpigraphs(sectionItem.Epigraphs, lexemes);
            ProcessSectionItems(sectionItem.Content, lexemes);
        }
    }
    
    private void ProcessSectionItems(List<IFb2TextItem>? sectionItems, ConcurrentDictionary<string, HashSet<string>> lexemes)
    {
        if (sectionItems is null || sectionItems.Count is 0)
        {
            return;
        }

        for (var sectionItemId = 0; sectionItemId < sectionItems.Count; sectionItemId++)
        {
            var sectionItem = sectionItems[sectionItemId];
            
            switch (sectionItem)
            {
                case SectionItem castedSectionItem:
                    ProcessMatrixString(castedSectionItem.ID, lexemes);
                    ProcessTitle(castedSectionItem.Title, lexemes);
                    ProcessEpigraphs(castedSectionItem.Epigraphs, lexemes);
                    ProcessParagraphItem(castedSectionItem.Content, lexemes);
                    break;

                case ParagraphItem castedParagraphItem:
                    ProcessParagraphData(castedParagraphItem.ParagraphData, lexemes);
                    break;
            }
        }
    }

    private void ProcessParagraphItem(List<IFb2TextItem>? sectionItems, ConcurrentDictionary<string, HashSet<string>> lexemes)
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
            
            ProcessMatrixString(castedParagraphItem.ID, lexemes); 
            ProcessParagraphData(castedParagraphItem.ParagraphData, lexemes);
        }
    }
    
    private void ProcessTitle(TitleItem? bodyItem, ConcurrentDictionary<string, HashSet<string>> lexemes)
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
            
            ProcessMatrixString(castedTitle.Lang, lexemes); 
            ProcessMatrixString(castedTitle.Style, lexemes); 
            ProcessMatrixString(castedTitle.ID, lexemes);
            ProcessMatrixString(castedTitle.ToString(), lexemes);
        }
    }

    private void ProcessParagraphData(IList<StyleType>? styleTypes, ConcurrentDictionary<string, HashSet<string>> lexemes)
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
            
            ProcessMatrixString(castedSimpleText.Text, lexemes);
        }
    }
    
    private void ProcessEpigraphs(IList<EpigraphItem>? epigraphItems, ConcurrentDictionary<string, HashSet<string>> lexemes)
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
                
                ProcessStanzaItems(castedPoemItem.Content, lexemes);
            }
            
            ProcessTextAuthorItems(epigraphItem.TextAuthors, lexemes);
        }
    }
    
    private void ProcessStanzaItems(IList<IFb2TextItem>? epigraphItems, ConcurrentDictionary<string, HashSet<string>> lexemes)
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
                ProcessParagraphData(line.ParagraphData, lexemes);
            }
        }
    }
    
    private void ProcessTextAuthorItems(IList<IFb2TextItem>? epigraphItems, ConcurrentDictionary<string, HashSet<string>> lexemes)
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
            
            ProcessParagraphData(castedTextAuthorItem.ParagraphData, lexemes);
        }
    }
    
    private void ProcessMatrixString(string str, ConcurrentDictionary<string, HashSet<string>> lexemes) 
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        
        var subStr = Regex.Split(str);

        foreach (var splitStr in subStr.AsSpan())
        {
            if (string.IsNullOrWhiteSpace(splitStr))
            {
                continue;
            }
            
            var strToAdd = splitStr.ToLower();
            
            lexemes.AddOrUpdate(_fileName, _ => new HashSet<string> { strToAdd }, (_, hashSet) =>
            {
                hashSet.Add(strToAdd);
                return hashSet;
            });
        }
    }
}