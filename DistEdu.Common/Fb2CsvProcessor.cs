using System.Collections.Concurrent;
using FB2Library;
using FB2Library.Elements;
using FB2Library.Elements.Poem;

namespace DistEdu.Common;

public sealed class Fb2CsvProcessor
{
    private readonly string _fileName;
    private readonly int _fileId;
    private readonly FB2File _fb2File;

    private readonly ConcurrentBag<FileToken> _tokens = new();

    public Fb2CsvProcessor(Fb2FileWrapper fb2FileWrapper)
    {
        _fileId = fb2FileWrapper.FileIndex;
        _fileName = fb2FileWrapper.FileName;
        _fb2File = fb2FileWrapper.File;
    }
    
    public ConcurrentBag<FileToken> ProcessFb2Async()
    {
        for (var bodyId = 0; bodyId < _fb2File.Bodies.Count; bodyId++)
        {
            var body = _fb2File.Bodies[bodyId];

            ProcessString(bodyId, default, default, body.Name);
            ProcessTitle(bodyId, default, default, body.Title);
            ProcessSections(bodyId, body.Sections);
        }

        return _tokens;
    }

    private void ProcessSections(int bodyId, List<SectionItem>? sectionItems)
    {
        if (sectionItems is null || sectionItems.Count is 0)
        {
            return;
        }

        for (var sectionId = 0; sectionId < sectionItems.Count; sectionId++)
        {
            var sectionItem = sectionItems[sectionId];
            
            ProcessString(bodyId, sectionId, default, sectionItem.ID); 
            ProcessTitle(bodyId, sectionId, default, sectionItem.Title);
            ProcessEpigraphs(bodyId, sectionId, default, sectionItem.Epigraphs);
            ProcessSectionItems(bodyId, sectionId, sectionItem.Content);
        }
    }
    
    private void ProcessSectionItems(int bodyId, int sectionId, List<IFb2TextItem>? sectionItems)
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
                    ProcessString(bodyId, sectionId, sectionItemId, castedSectionItem.ID);
                    ProcessTitle(bodyId, sectionId, sectionItemId, castedSectionItem.Title);
                    ProcessEpigraphs(bodyId, sectionId, sectionItemId, castedSectionItem.Epigraphs);
                    ProcessParagraphItem(bodyId, sectionId, sectionItemId, castedSectionItem.Content);
                    break;

                case ParagraphItem castedParagraphItem:
                    ProcessParagraphData(bodyId, sectionId, sectionItemId, castedParagraphItem.ParagraphData);
                    break;
            }
        }
    }

    private void ProcessParagraphItem(int bodyId, int sectionId, int sectionItemId, List<IFb2TextItem>? sectionItems)
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
            
            ProcessString(bodyId, sectionId, sectionItemId, castedParagraphItem.ID); 
            ProcessParagraphData(bodyId, sectionId, sectionItemId, castedParagraphItem.ParagraphData);
        }
    }
    
    private void ProcessTitle(int bodyId, int sectionId, int sectionItemId, TitleItem? bodyItem)
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
            
            ProcessString(bodyId, sectionId, sectionItemId, castedTitle.Lang); 
            ProcessString(bodyId, sectionId, sectionItemId,castedTitle.Style); 
            ProcessString(bodyId, sectionId, sectionItemId,castedTitle.ID);
            ProcessString(bodyId, sectionId, sectionItemId, castedTitle.ToString());
        }
    }

    private void ProcessParagraphData(int bodyId, int sectionId, int sectionItemId, IList<StyleType>? styleTypes)
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
            
            ProcessString(bodyId, sectionId, sectionItemId, castedSimpleText.Text);
        }
    }
    
    private void ProcessEpigraphs(int bodyId, int sectionId, int sectionItemId, IList<EpigraphItem>? epigraphItems)
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
                
                ProcessStanzaItems(bodyId, sectionId, sectionItemId, castedPoemItem.Content);
            }
            
            ProcessTextAuthorItems(bodyId, sectionId, sectionItemId, epigraphItem.TextAuthors);
        }
    }
    
    private void ProcessStanzaItems(int bodyId, int sectionId, int sectionItemId, IList<IFb2TextItem>? epigraphItems)
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
                ProcessParagraphData(bodyId, sectionId, sectionItemId, line.ParagraphData);
            }
        }
    }
    
    private void ProcessTextAuthorItems(int bodyId, int sectionId, int sectionItemId, IList<IFb2TextItem>? epigraphItems)
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
            
            ProcessParagraphData(bodyId, sectionId, sectionItemId, castedTextAuthorItem.ParagraphData);
        }
    }
    
    private void ProcessString(int bodyId, int sectionId, int sectionItemId, string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }

        var token = new FileToken(_fileName, _fileId, bodyId, sectionId, sectionItemId, str);

        _tokens.Add(token);
    }
}