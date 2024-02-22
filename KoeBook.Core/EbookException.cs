using System.Runtime.Serialization;

namespace KoeBook.Core;

public class EbookException : Exception
{
    public override string? Message { get; }

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
