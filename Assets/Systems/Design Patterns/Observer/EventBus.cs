using System;
using System.Collections.Generic;

namespace PawBab.DesignPatterns.Observer
{
    /// <summary>
    /// Domyślna implementacja <see cref="IEventBus"/> oparta o słownik
    /// typów zdarzeń i listy zarejestrowanych obserwatorów.
    /// </summary>
    /// <remarks>
    /// Implementacja nie jest w pełni thread-safe (zakłada typowe użycie
    /// w głównym wątku Unity), ale zapewnia izolację iteracji:
    /// podczas publikacji zdarzenia tworzona jest migawka (snapshot)
    /// aktualnej listy listenerów, dzięki czemu modyfikacje listy
    /// (subskrypcje/wyrejestrowania) wykonane w trakcie obsługi
    /// zdarzenia nie wpływają na bieżącą iterację.
    /// </remarks>
    /// <example>
    /// Rejestracja listenera i publikacja zdarzenia:
    /// <code>
    /// var bus = new EventBus();
    ///
    /// var listener = new ScoreUIListener();
    /// bus.Subscribe(listener);
    ///
    /// bus.Publish(new ScoreChangedEvent(42));
    /// </code>
    /// </example>
    public sealed class EventBus : IEventBus
    {
        /// <summary>
        /// Przechowuje listy listenerów zgrupowane po typie zdarzenia.
        /// Klucz: typ zdarzenia, wartość: lista obiektów implementujących
        /// odpowiednie <see cref="IEventListener{TEvent}"/>.
        /// </summary>
        private readonly Dictionary<Type, List<object>> _listeners = new();

        public void Publish<TEvent>(TEvent evt) where TEvent : IEvent
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            if (!_listeners.TryGetValue(typeof(TEvent), out var list))
                return;

            var snapshot = list.ToArray();
            foreach (var obj in snapshot)
            {
                if (obj is IEventListener<TEvent> listener)
                    listener.OnEvent(evt);
            }
        }

        public void Subscribe<TEvent>(IEventListener<TEvent> listener) where TEvent : IEvent
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            var type = typeof(TEvent);
            if (!_listeners.TryGetValue(type, out var list))
            {
                list = new();
                _listeners.Add(type, list);
            }

            if (!list.Contains(listener))
                list.Add(listener);
        }

        public void Unsubscribe<TEvent>(IEventListener<TEvent> listener) where TEvent : IEvent
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            var type = typeof(TEvent);
            if (!_listeners.TryGetValue(type, out var list))
                return;

            list.Remove(listener);
            if (list.Count == 0)
                _listeners.Remove(type);
        }
    }
}