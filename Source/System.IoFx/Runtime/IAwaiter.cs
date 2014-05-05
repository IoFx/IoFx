using System.Runtime.CompilerServices;

namespace System.IoFx.Runtime
{
    internal interface IAwaiter : INotifyCompletion
    {
        void GetResult();

        bool IsCompleted { get; }
    }

    internal interface IAwaiter<out TResult> : INotifyCompletion
    {
        TResult GetResult();

        bool IsCompleted { get; }
    }
}
