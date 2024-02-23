using System.Runtime.Serialization;

namespace KoeBook.Core;

public class EbookException : Exception
{
#pragma warning disable CS8764 // 戻り値の型の NULL 値の許容が、オーバーライドされたメンバーと一致しません。おそらく、NULL 値の許容の属性が原因です。
    public override string? Message { get; }
#pragma warning restore CS8764 // 戻り値の型の NULL 値の許容が、オーバーライドされたメンバーと一致しません。おそらく、NULL 値の許容の属性が原因です。

    public ExceptionType ExceptionType { get; }

    public EbookException(ExceptionType exceptionType, string? message = null, Exception? innerException = null) : base(null, innerException)
    {
        Message = message;
        ExceptionType = exceptionType;
    }
}

public enum ExceptionType
{
    [EnumMember(Value = "Epubの生成に失敗しました")]
    EpubCreateError
}
