namespace KoeBook.Core.Models.StyleBertVits;

internal record ModelInfo(
    Dictionary<string, int> spk2id,
    Dictionary<string, string> id2spk,
    Dictionary<string, int> style2id)
{
    public string FirstSpk { get; } = id2spk.First().Value;

    public string[] Styles { get; } = [.. style2id.Keys];
}
