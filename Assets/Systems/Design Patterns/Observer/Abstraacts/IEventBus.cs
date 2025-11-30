namespace PawBab.DesignPatterns.Observer
{
    /// <summary>
    /// Kontrakt szyny zdarzeń (Event Busa), odpowiedzialnej za
    /// publikowanie zdarzeń do zarejestrowanych obserwatorów.
    /// </summary>
    /// <remarks>
    /// Implementacja <see cref="IEventBus"/> pozwala odseparować logikę
    /// emitowania zdarzeń od logiki systemów, które na nie reagują.
    /// Dzięki temu kod jest luźno powiązany (loose coupling) i łatwiej go testować.
    /// </remarks>
    /// <example>
    /// Publikowanie prostego zdarzenia:
    /// <code>
    /// var bus = GlobalEventBus.Instance;
    /// bus.Publish(new PlayerDiedEvent(playerId: 1));
    /// </code>
    ///
    /// Subskrypcja (np. bezpośrednio, bez <see cref="UnityEngine.MonoBehaviour"/>):
    /// <code>
    /// public sealed class PlayerDiedListener : IEventListener&lt;PlayerDiedEvent&gt;
    /// {
    ///     public void OnEvent(PlayerDiedEvent evt)
    ///     {
    ///         UnityEngine.Debug.Log($"Gracz {evt.PlayerId} zginął.");
    ///     }
    /// }
    ///
    /// var listener = new PlayerDiedListener();
    /// var bus = GlobalEventBus.Instance;
    /// bus.Subscribe(listener);
    /// </code>
    /// </example>
    public interface IEventBus
    {
        
        /// <summary>
        /// Publikuje zdarzenie danego typu do wszystkich aktualnie
        /// zarejestrowanych obserwatorów tego typu.
        /// </summary>
        /// <typeparam name="TEvent">Typ publikowanego zdarzenia.</typeparam>
        /// <param name="evt">Instancja zdarzenia do rozesłania.</param>
        void Publish<TEvent>(TEvent evt) where TEvent : IEvent;
        
        /// <summary>
        /// Rejestruje listenera danego typu zdarzenia w szynie zdarzeń.
        /// </summary>
        /// <typeparam name="TEvent">Typ obsługiwanego zdarzenia.</typeparam>
        /// <param name="listener">
        /// Obiekt implementujący <see cref="IEventListener{TEvent}"/>, który ma być powiadamiany.
        /// </param>
        void Subscribe<TEvent>(IEventListener<TEvent> listener) where TEvent : IEvent;
        
        /// <summary>
        /// Wyrejestrowuje listenera danego typu zdarzenia z szyny zdarzeń.
        /// </summary>
        /// <typeparam name="TEvent">Typ zdarzenia, którego listener już nie ma obsługiwać.</typeparam>
        /// <param name="listener">Listener do usunięcia z listy subskrybentów.</param>
        void Unsubscribe<TEvent>(IEventListener<TEvent> listener) where TEvent : IEvent;
    }
}