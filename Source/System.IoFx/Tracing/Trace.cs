using System.ServiceModel;
using Microsoft.Diagnostics.Tracing;

namespace System.IoFx.Tracing
{
    sealed class Events : EventSource
    {

        public static Events Trace = new Events();

        public class Keywords
        {
            public const EventKeywords Page = (EventKeywords)1;
            public const EventKeywords DataBase = (EventKeywords)2;
            public const EventKeywords Diagnostic = (EventKeywords)4;
            public const EventKeywords Perf = (EventKeywords)8;
        }

        [Event(1, Message = "Application Failure: {0}", Level = EventLevel.Error, Keywords = Keywords.Diagnostic)]
        public void Failure(string message) { WriteEvent(1, message); }


        [Event(2, Message = "Starting up.", Keywords = Keywords.Diagnostic, Level = EventLevel.Informational)]
        public void CreateSocketListener(string address) { WriteEvent(2, address); }
    }
}
