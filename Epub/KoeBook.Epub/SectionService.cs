using System.Diagnostics.CodeAnalysis;

namespace KoeBook.Epub;

internal sealed class SectionService
{
    private readonly List<Section> _sections = [];
    private SectionService() { }
    private static SectionService? _sectionService = null;
    internal static SectionService Instance
    {
        get
        {
            _sectionService ??= new SectionService();
            return _sectionService;
        }
    }

    internal Section CreateSection(string title)
    {
        var uuid = Guid.NewGuid().ToString();
        var section = new Section(title, uuid);
        _sections.Add(section);
        return section;
    }

    internal bool Contain(string id)
    {
        return _sections.Where(s => s.Id == id).Any();
    }

    
    internal bool TryGetSection(string id, [NotNullWhen(true)] out  Section? section)
    {
        var sec =_sections.Where(s => s.Id == id).First();
        if (sec != null)
        {
            section = sec;
            return true;
        }
        else
        {
            section = null;
            return false;
        }
    }
}
