namespace GameFoundation.Scripts.Patterns.SignalBus
{
    using System;
    using System.Collections.Generic;

    public class SignalBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public void Subscribe<T>(Action<T> callback)
        {
            var type = typeof(T);
            if (!this._subscribers.ContainsKey(type))
            {
                this._subscribers[type] = new List<Delegate>();
            }
            this._subscribers[type].Add(callback);
        }

        public void Unsubscribe<T>(Action<T> callback)
        {
            var type = typeof(T);
            if (this._subscribers.TryGetValue(type, out var subscriber))
            {
                subscriber.Remove(callback);
            }
        }

        public void UnsubscribeAll<T>()
        {
            var type = typeof(T);
            if (this._subscribers.ContainsKey(type))
            {
                this._subscribers.Remove(type);
            }
        }

        public void Fire<T>(T signal)
        {
            var type = typeof(T);
            if (!this._subscribers.TryGetValue(type, out var subscriber)) return;
            var subscribersCopy = new List<Delegate>(subscriber);
            foreach (var callback in subscribersCopy)
            {
                ((Action<T>)callback)?.Invoke(signal);
            }
        }
    }
}