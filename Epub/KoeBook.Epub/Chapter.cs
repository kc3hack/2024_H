namespace KoeBook.Epub;

public class Chapter
{
    private readonly List<string> _sectionIds = [];
    public string? Title { get; set; }

    public Section CreateSection(string title)
    {
        var sec = SectionService.Instance.CreateSection(title);
        _sectionIds.Add(sec.Id);
        return sec;
    }

    public List<Section> GetAllSection()
    {
        List<Section> sections = [];
        foreach (var sectionId in _sectionIds)
        {
            if(SectionService.Instance.TryGetSection(sectionId, out var section))
            {
                sections.Add(section);
            }
        }
        return sections;
    }
}
