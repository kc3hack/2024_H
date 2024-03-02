using KoeBook.Epub.Models;
using PrivateProxy;

namespace KoeBook.Test.Proxies;

[GeneratePrivateProxy(typeof(EpubDocument))]
partial struct EpubDocumentProxy;
