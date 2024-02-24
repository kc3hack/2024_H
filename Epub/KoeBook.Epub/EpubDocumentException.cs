using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoeBook.Epub
{
    public class EpubDocumentException : Exception
    {
        public EpubDocumentException(string? message) : base(message) {}
    }
}
