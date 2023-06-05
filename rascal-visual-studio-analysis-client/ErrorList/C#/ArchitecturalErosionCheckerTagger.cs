using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;

namespace ArchitecturalErosionChecker
{
    class ArchitecturalErosionCheckerTagger : ITagger<IErrorTag>, IDisposable
    {
        private readonly ArchitecturalErosionChecker _architecturalErosionChecker;
        private ViolationSnapshot _violation;

        internal ArchitecturalErosionCheckerTagger(ArchitecturalErosionChecker architecturalErosionChecker)
        {
            _architecturalErosionChecker = architecturalErosionChecker;
            _violation = architecturalErosionChecker.LastViolation;

            architecturalErosionChecker.AddTagger(this);
        }

        internal void UpdateErrors(ITextSnapshot currentSnapshot, ViolationSnapshot violation)
        {
            var oldViolations = _violation;
            _violation = violation;

            var h = this.TagsChanged;
            if (h != null)
            {
                // Raise a single tags changed event over the span that could have been affected by the change in the errors.
                int start = int.MaxValue;
                int end = int.MinValue;

                if ((oldViolations != null) && (oldViolations.Errors.Count > 0))
                {
                    start = oldViolations.Errors[0].Span.Start.TranslateTo(currentSnapshot, PointTrackingMode.Negative);
                    end = oldViolations.Errors[oldViolations.Errors.Count - 1].Span.End.TranslateTo(currentSnapshot, PointTrackingMode.Positive);
                }

                if (violation.Count > 0)
                {
                    start = Math.Min(start, violation.Errors[0].Span.Start.Position);
                    end = Math.Max(end, violation.Errors[violation.Errors.Count - 1].Span.End.Position);
                }

                if (start < end)
                {
                    h(this, new SnapshotSpanEventArgs(new SnapshotSpan(currentSnapshot, Span.FromBounds(start, end))));
                }
            }
        }

        public void Dispose()
        {
            // Called when the tagger is no longer needed (generally when the ITextView is closed).
            _architecturalErosionChecker.RemoveTagger(this);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (_violation != null)
            {
                foreach (var error in _violation.Errors)
                {
                    if (spans.IntersectsWith(error.Span))
                    {
                        yield return new TagSpan<IErrorTag>(error.Span, new ErrorTag(PredefinedErrorTypeNames.Warning));
                    }
                }
            }
        }
    }
}
