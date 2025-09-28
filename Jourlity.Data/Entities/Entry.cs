namespace Jourlity.Data.Entities;

public class Entry : BaseEntity
{
    public EntryTypeEnum EntryType { get; set; }
}

public enum EntryTypeEnum
{
    Text,
    Image,
    Document
}