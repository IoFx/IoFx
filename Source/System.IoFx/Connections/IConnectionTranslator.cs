using System.IoFx.Framing;

namespace System.IoFx.Connections
{
    interface IConnectionTranslator<T1, T2> : IConnection<T2>
    {
        IConnection<T2> Translate(IConnection<T1> input);
    }


    class ConnectionTranslator<T1, T2> : IConnectionTranslator<T1, T2>
    {
        private IConnection<T1> _input;
        private ITranslator<T2, T1> _txOutput;
        private ITranslator<T1, T2> _txInput;

        public ConnectionTranslator(IConnection<T1> input, ITranslator<T1, T2> inputTranslator, ITranslator<T2, T1> outputTranslator)
        {
            _input = input;
            _txInput = inputTranslator;
            _txOutput = outputTranslator;
        }

        public IConnection<T2> Translate(IConnection<T1> input)
        {
            return this;
        }

        public IDisposable Subscribe(IObserver<T2> observer)
        {
            var consumer = new ConsumerObserver<T2>(observer);
            var translator = _txInput;
            return _input.Subscribe(item =>
            {
                translator.OnNext(item, consumer);
            },
            observer.OnError,
            observer.OnCompleted);
        }

        public void Publish(T2 value)
        {
            _txOutput.OnNext(value, _input);
        }

        struct ConsumerObserver<T> : IConsumer<T>
        {
            private IObserver<T> _observer;
            public ConsumerObserver(IObserver<T> observer)
            {
                _observer = observer;
            }

            public void Publish(T value)
            {
                _observer.OnNext(value);
            }
        }
    }
}
