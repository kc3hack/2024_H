using System.Runtime.Serialization;

namespace KoeBook.Core.Models;

public enum GenerationState
{
    // 0~5 はStateProgressBarが依存しているので変更しないこと
    [EnumMember(Value = "待機中")]
    Waiting = 0,

    [EnumMember(Value = "ダウンロード中")]
    Downloading,

    [EnumMember(Value = "解析中")]
    Analyzing,

    [EnumMember(Value = "音声生成中")]
    SoundProducing,

    [EnumMember(Value = "出力中")]
    Publishing,

    [EnumMember(Value = "生成完了")]
    Completed,

    [EnumMember(Value = "失敗")]
    Failed,
}
