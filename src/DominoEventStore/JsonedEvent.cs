using System;

namespace DominoEventStore
{
    public class JsonedEvent
    {
        /// <summary>
        /// Type name including namespace
        /// </summary>
        public string Type { get; set; }
        public string EventData { get; set; }
        public DateTimeOffset CommitDate { get; set; }

    }
}