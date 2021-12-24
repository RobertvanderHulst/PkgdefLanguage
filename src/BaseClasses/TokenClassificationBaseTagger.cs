﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace BaseClasses
{
    public abstract class TokenClassificationBaseTagger : ITaggerProvider
    {
        [Import] internal IClassificationTypeRegistryService _classificationRegistry = null;
        [Import] internal IBufferTagAggregatorFactoryService _bufferTagAggregator = null;

        public abstract Dictionary<object, string> ClassificationMap { get; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITagAggregator<TokenTag> tags = _bufferTagAggregator.CreateTagAggregator<TokenTag>(buffer);
            return buffer.Properties.GetOrCreateSingletonProperty(() => new TokenClassifier(_classificationRegistry, tags, ClassificationMap)) as ITagger<T>;
        }
    }

    internal class TokenClassifier : TokenTaggerConsumerBase<IClassificationTag>
    {
        private static Dictionary<object, ClassificationTag> _classificationMap;

        internal TokenClassifier(IClassificationTypeRegistryService registry, ITagAggregator<TokenTag> tags, Dictionary<object, string> map) : base(tags)
        {
            _classificationMap = new();

            foreach (var key in map.Keys)
            {
                _classificationMap[key] = new ClassificationTag(registry.GetClassificationType(map[key]));
            }

        }

        public override IEnumerable<ITagSpan<IClassificationTag>> GetTags(IMappingTagSpan<TokenTag> span)
        {
            if (_classificationMap.TryGetValue(span.Tag.TokenType, out ClassificationTag classificationTag))
            {
                NormalizedSnapshotSpanCollection tagSpans = span.Span.GetSpans(span.Span.AnchorBuffer.CurrentSnapshot);

                foreach (SnapshotSpan tagSpan in tagSpans)
                {
                    yield return new TagSpan<ClassificationTag>(tagSpan, classificationTag);
                }
            }
        }
    }
}
