using DistEdu.Common.Models;
using FB2Library.Elements;
using FB2Library.Elements.Poem;

namespace DistEdu.Common;

public abstract class Fb2Processor<TCollection>
{
    private readonly Action<string, Fb2FileWrapper, TCollection> _action;
    protected readonly Fb2FileWrapper Fb2FileWrapper;

    protected Fb2Processor(
        Fb2FileWrapper fb2FileWrapperWrapper, Action<string, Fb2FileWrapper, TCollection> action)
    {
        Fb2FileWrapper = fb2FileWrapperWrapper;
        _action = action;
    }

    protected virtual void ProcessBodies(ICollection<BodyItem> bodies, TCollection collection)
    {
        for (var bodyId = 0; bodyId < bodies.Count; bodyId++)
        {
            var body = Fb2FileWrapper.File.Bodies[bodyId];

            _action(body.Name, Fb2FileWrapper, collection);
            ProcessTitle(body.Title, collection);
            ProcessSections(body.Sections, collection);
        }
    }

    protected virtual void ProcessSections(List<SectionItem>? sectionItems, TCollection collection)
    {
        if (sectionItems is null || sectionItems.Count is 0) return;

        foreach (var sectionItem in sectionItems)
        {
            _action(sectionItem.ID, Fb2FileWrapper, collection);
            ProcessTitle(sectionItem.Title, collection);
            ProcessEpigraphs(sectionItem.Epigraphs, collection);
            ProcessSectionItems(sectionItem.Content, collection);
        }
    }

    protected virtual void ProcessSectionItems(List<IFb2TextItem>? sectionItems, TCollection collection)
    {
        if (sectionItems is null || sectionItems.Count is 0) return;

        foreach (var sectionItem in sectionItems)
            switch (sectionItem)
            {
                case SectionItem castedSectionItem:
                    _action(castedSectionItem.ID, Fb2FileWrapper, collection);
                    ProcessTitle(castedSectionItem.Title, collection);
                    ProcessEpigraphs(castedSectionItem.Epigraphs, collection);
                    ProcessParagraphItem(castedSectionItem.Content, collection);
                    break;

                case ParagraphItem castedParagraphItem:
                    ProcessParagraphData(castedParagraphItem.ParagraphData, collection);
                    break;
            }
    }

    protected virtual void ProcessParagraphItem(List<IFb2TextItem>? sectionItems, TCollection collection)
    {
        if (sectionItems is null || sectionItems.Count is 0) return;

        foreach (var sectionItem in sectionItems)
        {
            if (sectionItem is not ParagraphItem castedParagraphItem) continue;

            _action(castedParagraphItem.ID, Fb2FileWrapper, collection);
            ProcessParagraphData(castedParagraphItem.ParagraphData, collection);
        }
    }

    protected virtual void ProcessTitle(TitleItem? bodyItem, TCollection collection)
    {
        if (bodyItem?.TitleData is null || bodyItem.TitleData?.Count is 0) return;

        foreach (var title in bodyItem.TitleData!)
        {
            if (title is not ParagraphItem castedTitle) continue;

            _action(castedTitle.Lang, Fb2FileWrapper, collection);
            _action(castedTitle.Style, Fb2FileWrapper, collection);
            _action(castedTitle.ID, Fb2FileWrapper, collection);
            _action(castedTitle.ToString(), Fb2FileWrapper, collection);
        }
    }

    protected virtual void ProcessParagraphData(IList<StyleType>? styleTypes, TCollection collection)
    {
        if (styleTypes is null || styleTypes.Count is 0) return;

        foreach (var simpleText in styleTypes)
        {
            if (simpleText is not SimpleText castedSimpleText) continue;

            _action(castedSimpleText.Text, Fb2FileWrapper, collection);
        }
    }

    protected virtual void ProcessEpigraphs(IList<EpigraphItem>? epigraphItems, TCollection collection)
    {
        if (epigraphItems is null || epigraphItems.Count is 0) return;

        foreach (var epigraphItem in epigraphItems)
        {
            foreach (var poemItem in epigraphItem.EpigraphData)
            {
                if (poemItem is not PoemItem castedPoemItem) continue;

                ProcessStanzaItems(castedPoemItem.Content, collection);
            }

            ProcessTextAuthorItems(epigraphItem.TextAuthors, collection);
        }
    }

    protected virtual void ProcessStanzaItems(IList<IFb2TextItem>? epigraphItems, TCollection collection)
    {
        if (epigraphItems is null || epigraphItems.Count is 0) return;

        foreach (var stanzaItem in epigraphItems)
        {
            if (stanzaItem is not StanzaItem castedStanzaItem) continue;

            foreach (var line in castedStanzaItem.Lines) ProcessParagraphData(line.ParagraphData, collection);
        }
    }

    protected virtual void ProcessTextAuthorItems(IList<IFb2TextItem>? epigraphItems, TCollection collection)
    {
        if (epigraphItems is null || epigraphItems.Count is 0) return;

        foreach (var fb2TextItem in epigraphItems)
        {
            if (fb2TextItem is not TextAuthorItem castedTextAuthorItem) continue;

            ProcessParagraphData(castedTextAuthorItem.ParagraphData, collection);
        }
    }
}