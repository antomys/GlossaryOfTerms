namespace DistEdu.Common;

public sealed class FileToken
{
    public int Id { get; set; }
    
    public string FileName { get; }
    
    public int FileId { get; }
    
    public int BodyId { get; }
    
    public int SectionId { get; }
    
    public int SectionItemId { get; }
    
    public string Token { get; }
    
    public static string GetHeaders() 
        => $"{nameof(Id)};{nameof(FileName)};{nameof(FileId)};{nameof(BodyId)};{nameof(SectionId)};{nameof(SectionItemId)};{nameof(Token)}";


    public override string ToString()
    {
        return $"{Id};{FileName};{FileId};{BodyId};{SectionId};{SectionItemId};{Token}";
    }

    public FileToken(string fileName, int fileId, int bodyId, int sectionId, int sectionItemId, string strToken)
    {
        FileName = fileName;
        FileId = fileId;
        BodyId = bodyId;
        SectionId = sectionId;
        SectionItemId = sectionItemId;
        Token = strToken;
    }
}