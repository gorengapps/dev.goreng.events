using System;

namespace Framework.Events
{
    internal class CombineLatestEventListener<T1, T2> : IDisposableEventListener<(T1, T2)>
    {
        private readonly IDisposableEventProducer<(T1, T2)> _producer = new BaseEventProducer<(T1, T2)>();

        private T1 _latest1;
        private T2 _latest2;
        private bool _hasValue1;
        private bool _hasValue2;

        private readonly IDisposable _subscription1;
        private readonly IDisposable _subscription2;

        public CombineLatestEventListener(IEventListener<T1> source1, IEventListener<T2> source2)
        {
            _subscription1 = source1.Subscribe((sender, data) =>
            {
                _latest1 = data;
                _hasValue1 = true;
                PublishCombined(sender);
            });

            _subscription2 = source2.Subscribe((sender, data) =>
            {
                _latest2 = data;
                _hasValue2 = true;
                PublishCombined(sender);
            });
        }

        private void PublishCombined(object sender)
        {
            if (_hasValue1 && _hasValue2)
            {
                _producer.Publish(sender, (_latest1, _latest2));
            }
        }

        public IDisposable Subscribe(EventHandler<(T1, T2)> handler)
        {
            return _producer.listener.Subscribe(handler);
        }

        void IEventListener<(T1, T2)>.Unsubscribe(EventHandler<(T1, T2)> handler)
        {
            Unsubscribe(handler);
        }

        public void Unsubscribe(EventHandler<(T1, T2)> handler)
        {
            _producer.listener.Unsubscribe(handler);
        }

        public void Dispose()
        {
            _producer.Dispose();
            _subscription1?.Dispose();
            _subscription2?.Dispose();
        }
    }
}