namespace KoeBook.Core.Models;

/// <summary>
/// 本ごとの設定
/// </summary>
/// <param name="characterMapping">キャラクターとモデルの紐づけ</param>
public class BookOptions(Dictionary<string, string> characterMapping)
{
    public BookOptions() : this([]) { }

    /// <summary>
    /// キャラクターとモデルの紐づけ。Key: キャラクター, Value: モデル
    /// </summary>
    public Dictionary<string, string> CharacterMapping { get; } = characterMapping;
}
