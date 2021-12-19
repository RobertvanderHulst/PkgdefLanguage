using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace PkgdefLanguage
{
    public class DefaultColorizer : Colorizer
    {
        public DefaultColorizer(LanguageService svc, IVsTextLines buffer, IScanner scanner) :
            base(svc, buffer, scanner)
        { }
    }
}
