namespace Jourlity.Data.Entities;

public class Entry : BaseEntity
{
    public EntryTypeEnum EntryType { get; set; }
}

/// <summary>
/// EntryTypeEnums values are used in Database, do not change them.
/// </summary>
public enum EntryTypeEnum
{
    Text = 0,
    Image = 1,
    Document = 2
}