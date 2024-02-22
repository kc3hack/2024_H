using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;

namespace KoeBook.Core.Helpers;

public static class IDisplayStateChangeEx
{
    public static DisplayStateChanging ResetProgress(
        this IDisplayStateChangeService service,
        BookProperties bookProperties,
        GenerationState state,
        int maximum)
    {
        service.UpdateProgress(bookProperties, 0, maximum);
        service.UpdateState(bookProperties, state);
        return new(service, bookProperties, maximum);
    }

    public class DisplayStateChanging(IDisplayStateChangeService displayStateChangeService, BookProperties bookProperties, int maximum)
    {
        private readonly IDisplayStateChangeService _displayStateChangeService = displayStateChangeService;

        private readonly BookProperties _bookProperties = bookProperties;

        private readonly int _maximum = maximum;

        public void UpdateProgress(int progress)
        {
            _displayStateChangeService.UpdateProgress(_bookProperties, progress, _maximum);
        }
    }
}
