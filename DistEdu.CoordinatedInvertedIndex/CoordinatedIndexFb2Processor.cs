using DistEdu.Common.Extensions;
using DistEdu.Common.Models;
using FB2Library.Elements;
using FB2Library.Elements.Poem;

namespace DistEdu.CoordinatedInvertedIndex;

public sealed class CoordinatedIndexFb2Processor
{
    private int _id;
    private int _processedLength;
    private readonly CoordinatedIndex _coordinatedIndex;
    private readonly Fb2FileWrapper _fb2FileWrapperWrapper;

    public CoordinatedIndexFb2Processor(Fb2FileWrapper fb2FileWrapperWrapper)
    {
        _fb2FileWrapperWrapper = fb2FileWrapperWrapper;
        _coordinatedIndex = new CoordinatedIndex();
    }

    public CoordinatedIndex GetIndex()
    {
        ProcessBodies(_fb2FileWrapperWrapper.File.Bodies);

        return _coordinatedIndex;
    }

    private void AddToIndex(
        string stringToProcess)
    {
        if (string.IsNullOrEmpty(stringToProcess))
        {
            return;
        }

        var termStart = 0;
        var i = 0;
        var stringSpan = stringToProcess.ToLower().AsSpan();
        var internalLength = 0;


        while (true)
        {
            if (char.IsLetterOrDigit(stringSpan[i]) || Common.SearchOption.ContainsAcceptableSymbols(stringSpan[i]))
            {
                termStart++;
            }
            else
            {
                if (termStart > 0)
                {
                    var slice = stringSpan.Slice(i - termStart, termStart);
                    Interlocked.Increment(ref internalLength);
                    var positionInFile = internalLength + _processedLength;
                    
                    _coordinatedIndex.Add(slice.Lower(), _id, _fb2FileWrapperWrapper.FileName, positionInFile);
                }
                Interlocked.Increment(ref _id);

                termStart = 0;
            }

            if (++i != stringToProcess.Length)
            {
                continue;
            }

            if (termStart > 0)
            {
                var slice = stringSpan.Slice(i - termStart, termStart);
                Interlocked.Increment(ref internalLength);
                var positionInFile = internalLength + _processedLength;

                _coordinatedIndex.Add(slice.Lower(), _id, _fb2FileWrapperWrapper.FileName, positionInFile);
                Interlocked.Increment(ref _id);
            }
            break;
        }

        _processedLength += internalLength;
    }
    
    private void ProcessBodies(ICollection<BodyItem> bodies)
    {
        for (var bodyId = 0; bodyId < bodies.Count; bodyId++)
        {
            var body = _fb2FileWrapperWrapper.File.Bodies[bodyId];

            AddToIndex(body.Name);
            ProcessTitle(body.Title);
            ProcessSections(body.Sections);
        }
    }

    private void ProcessSections(List<SectionItem>? sectionItems)
    {
        if (sectionItems is null || sectionItems.Count is 0) return;

        foreach (var sectionItem in sectionItems)
        {
            AddToIndex(sectionItem.ID);
            ProcessTitle(sectionItem.Title);
            ProcessEpigraphs(sectionItem.Epigraphs);
            ProcessSectionItems(sectionItem.Content);
        }
    }

    private void ProcessSectionItems(List<IFb2TextItem>? sectionItems)
    {
        if (sectionItems is null || sectionItems.Count is 0) return;

        foreach (var sectionItem in sectionItems)
            switch (sectionItem)
            {
                case SectionItem castedSectionItem:
                    AddToIndex(castedSectionItem.ID);
                    ProcessTitle(castedSectionItem.Title);
                    ProcessEpigraphs(castedSectionItem.Epigraphs);
                    ProcessParagraphItem(castedSectionItem.Content);
                    break;

                case ParagraphItem castedParagraphItem:
                    ProcessParagraphData(castedParagraphItem.ParagraphData);
                    break;
            }
    }

    private void ProcessParagraphItem(List<IFb2TextItem>? sectionItems)
    {
        if (sectionItems is null || sectionItems.Count is 0) return;

        foreach (var sectionItem in sectionItems)
        {
            if (sectionItem is not ParagraphItem castedParagraphItem) continue;

            AddToIndex(castedParagraphItem.ID);
            ProcessParagraphData(castedParagraphItem.ParagraphData);
        }
    }

    private void ProcessTitle(TitleItem? bodyItem)
    {
        if (bodyItem?.TitleData is null || bodyItem.TitleData?.Count is 0) return;

        foreach (var title in bodyItem.TitleData!)
        {
            if (title is not ParagraphItem castedTitle) continue;

            AddToIndex(castedTitle.Lang);
            AddToIndex(castedTitle.Style);
            AddToIndex(castedTitle.ID);
            AddToIndex(castedTitle.ToString());
        }
    }

    private void ProcessParagraphData(IList<StyleType>? styleTypes)
    {
        if (styleTypes is null || styleTypes.Count is 0) return;

        foreach (var simpleText in styleTypes)
        {
            if (simpleText is not SimpleText castedSimpleText) continue;

            AddToIndex(castedSimpleText.Text);
        }
    }

    private void ProcessEpigraphs(IList<EpigraphItem>? epigraphItems)
    {
        if (epigraphItems is null || epigraphItems.Count is 0) return;

        foreach (var epigraphItem in epigraphItems)
        {
            foreach (var poemItem in epigraphItem.EpigraphData)
            {
                if (poemItem is not PoemItem castedPoemItem) continue;

                ProcessStanzaItems(castedPoemItem.Content);
            }

            ProcessTextAuthorItems(epigraphItem.TextAuthors);
        }
    }

    private void ProcessStanzaItems(IList<IFb2TextItem>? epigraphItems)
    {
        if (epigraphItems is null || epigraphItems.Count is 0) return;

        foreach (var stanzaItem in epigraphItems)
        {
            if (stanzaItem is not StanzaItem castedStanzaItem) continue;

            foreach (var line in castedStanzaItem.Lines) ProcessParagraphData(line.ParagraphData);
        }
    }

    private void ProcessTextAuthorItems(IList<IFb2TextItem>? epigraphItems)
    {
        if (epigraphItems is null || epigraphItems.Count is 0) return;

        foreach (var fb2TextItem in epigraphItems)
        {
            if (fb2TextItem is not TextAuthorItem castedTextAuthorItem) continue;

            ProcessParagraphData(castedTextAuthorItem.ParagraphData);
        }
    }
}