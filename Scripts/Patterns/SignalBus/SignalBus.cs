namespace GameFoundation.Scripts.Patterns.SignalBus
{
    using System;
    using System.Collections.Generic;

    public class SignalBus
    {
        private abstract class HandlerBase { }

        private sealed class Handler<T> : HandlerBase
        {
            public Action<T> Typed;
            public Action    NoParam;
        }

        private readonly Dictionary<Type, HandlerBase> _handlers = new();

        private Handler<T> GetOrCreate<T>()
        {
            if (!this._handlers.TryGetValue(typeof(T), out var h))
                this._handlers[typeof(T)] = h = new Handler<T>();
            return (Handler<T>)h;
        }

        public void Subscribe<T>(Action<T> callback)   => this.GetOrCreate<T>().Typed   += callback;
        public void Subscribe<T>(Action callback)      => this.GetOrCreate<T>().NoParam += callback;
        public void Unsubscribe<T>(Action<T> callback) => this.GetOrCreate<T>().Typed   -= callback;
        public void Unsubscribe<T>(Action callback)    => this.GetOrCreate<T>().NoParam -= callback;

        public void UnsubscribeAll<T>() => this._handlers.Remove(typeof(T));

        public void Fire<T>(T signal)
        {
            if (!this._handlers.TryGetValue(typeof(T), out var h)) return;
            var handler = (Handler<T>)h;
            handler.Typed?.Invoke(signal);
            handler.NoParam?.Invoke();
        }
    }
}