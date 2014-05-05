using Microsoft.Diagnostics.Tracing;

namespace System.IoFx.Tracing
{
    sealed class Trace : EventSource
    {
        public class Keywords
        {
            public const EventKeywords Page = (EventKeywords)1;
            public const EventKeywords DataBase = (EventKeywords)2;
            public const EventKeywords Diagnostic = (EventKeywords)4;
            public const EventKeywords Perf = (EventKeywords)8;
        }

        [Event(1, Message = "Application Failure: {0}", Level = EventLevel.Error, Keywords = Keywords.Diagnostic)]
        public void Failure(string message) { WriteEvent(1, message); }


        [Event(2, Message = "Starting up.", Keywords = Keywords.Perf, Level = EventLevel.Informational)]
        public void SocketStartup() { WriteEvent(2); }

    }
}
