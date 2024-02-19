using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace KoeBook.Models;

public enum ProcessingState
{
    [EnumMember(Value = "待機中")]
    Waiting,

    [EnumMember(Value = "ダウンロード中")]
    Downloading,

    [EnumMember(Value = "解析中")]
    Analyzing,

    [EnumMember(Value = "音声生成中")]
    SoundProducing,

    [EnumMember(Value = "生成完了")]
    Completed,

    [EnumMember(Value = "失敗")]
    Failed,
}
