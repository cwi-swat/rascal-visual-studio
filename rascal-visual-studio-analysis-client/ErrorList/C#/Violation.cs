using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.Text;

namespace ArchitecturalErosionChecker
{
    class Violation
    {
        public readonly SnapshotSpan Span;
        public readonly string Problem;

        // This is used by ViolationSnapshot.TranslateTo() to map this error to the corresponding error in the next snapshot.
        public int NextIndex = -1;

        public Violation(SnapshotSpan span, string problem)
        {
            this.Span = span;
            this.Problem = problem;
        }

        public static Violation Clone(Violation error)
        {
            return new Violation(error.Span, error.Problem);
        }

        public static Violation CloneAndTranslateTo(Violation error, ITextSnapshot newSnapshot)
        {
            var newSpan = error.Span.TranslateTo(newSnapshot, SpanTrackingMode.EdgeExclusive);

            // We want to only translate the error if the length of the error span did not change (if it did change, it would imply that
            // there was some text edit inside the error and, therefore, that the error is no longer valid).
            return (newSpan.Length == error.Span.Length)
                   ? new Violation(newSpan, error.Problem)
                   : null;
        }
    }
}
