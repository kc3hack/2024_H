using System.Runtime.Serialization;

namespace KoeBook.Models;

public enum SourceType
{
    [EnumMember(Value = "外部サイト")]
    Url,

    [EnumMember(Value = "ローカルファイル")]
    FilePath,
}
