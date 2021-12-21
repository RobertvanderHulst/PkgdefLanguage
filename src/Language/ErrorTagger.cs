using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace PkgdefLanguage
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType(Constants.LanguageName)]
    [Name(Constants.LanguageName)]
    public class RestErrorTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag =>
            buffer.Properties.GetOrCreateSingletonProperty(() => new ErrorTagger(buffer)) as ITagger<T>;
    }

    public class ErrorTagger : ITagger<IErrorTag>
    {
        private readonly ITextBuffer _buffer;
        private readonly PkgdefDocument _document;

        public ErrorTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _document = PkgdefDocument.FromTextbuffer(buffer);
        }

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (_document.IsParsing || _document.IsValid)
            {
                yield break;
            }

            ITextSnapshot snapshot = _buffer.CurrentSnapshot;

            foreach (SnapshotSpan span in spans.Where(s => !s.IsEmpty))
            {
                IEnumerable<ParseItem> tokens = _document.Items.Where(t => t.Start < span.End && t.End > span.Start);

                foreach (ParseItem item in _document.Items)
                {
                    if (!item.IsValid)
                    {
                        var tooltip = string.Join(Environment.NewLine, item.Errors);

                        var simpleSpan = new Span(item.Start, item.Length);
                        var snapShotSpan = new SnapshotSpan(snapshot, simpleSpan);
                        var errorTag = new ErrorTag(PredefinedErrorTypeNames.CompilerError, tooltip);

                        yield return new TagSpan<IErrorTag>(snapShotSpan, errorTag);
                    }

                    foreach (Reference reference in item.References)
                    {
                        if (!reference.Value.IsValid)
                        {
                            var tooltip = string.Join(Environment.NewLine, reference.Value.Errors);

                            var simpleSpan = new Span(reference.Value.Start, reference.Value.Length);
                            var snapShotSpan = new SnapshotSpan(snapshot, simpleSpan);
                            var errorTag = new ErrorTag(PredefinedErrorTypeNames.CompilerError, tooltip);

                            yield return new TagSpan<IErrorTag>(snapShotSpan, errorTag);
                        }
                    }
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }
    }
}
