using System;

namespace DominoEventStore
{
    public struct ProcessedCommitsCount
    {
        public ProcessedCommitsCount(int value)
        {
            value.Must(d=>d>=0);
            Value = value;
        }

        public int Value { get; }

        public static implicit operator int(ProcessedCommitsCount d) => d.Value;
        public static implicit operator ProcessedCommitsCount(int d) => new ProcessedCommitsCount(d);
    }
}