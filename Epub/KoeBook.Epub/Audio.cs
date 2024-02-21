using System;
using NAudio.Wave;

namespace KoeBook.Epub;

public sealed class Audio(byte[] mp3Data)
{
    private TimeSpan? _totalTime = null;
    private byte[] _mp3Data = mp3Data;

    public TimeSpan TotalTime
    {
        get {
            if (_totalTime.HasValue)
            {
               return _totalTime.Value;
            }
            else
            {
                using var ms = new MemoryStream();
                ms.Write(_mp3Data.AsSpan());
                ms.Flush();
                ms.Position = 0;
                using var reader = new Mp3FileReader(ms);
                _totalTime = reader.TotalTime;
                return _totalTime.Value;
            }
        }
    }
}
