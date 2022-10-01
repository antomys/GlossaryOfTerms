namespace DistEdu.BooleanSearch.Models;

public struct Book
{
    public string Id { get; }

    public string FileName { get; }

    public string FileId { get; }

    public string BodyId { get; }

    public string SectionId { get; }

    public string SectionItemId { get; }

    public string Token { get; }

    public Book(string id, string fileName, string fileId, string bodyId, string sectionId, string sectionItemId,
        string strToken)
    {
        Id = id;
        FileName = fileName;
        FileId = fileId;
        BodyId = bodyId;
        SectionId = sectionId;
        SectionItemId = sectionItemId;
        Token = strToken;
    }

    public override string ToString()
    {
        return
            $"{nameof(FileName)}: {FileName}. {nameof(FileId)} : {FileId}. {nameof(BodyId)} : {BodyId}. {nameof(SectionId)} : {SectionId}. {nameof(SectionItemId)} : {SectionItemId}. {nameof(Token)} : {Token}.";
    }
}