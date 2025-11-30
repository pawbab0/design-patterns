using UnityEngine;

namespace PawBab.DesignPatterns.Observer
{
    /// <summary>
    /// Bazowa klasa pomocnicza dla komponentów Unity, które chcą nasłuchiwać
    /// zdarzeń z systemu obserwatora za pośrednictwem globalnego <see cref="IEventBus"/>.
    /// </summary>
    /// <typeparam name="TEvent">
    /// Typ obsługiwanego zdarzenia. Musi implementować <see cref="IEvent"/>.
    /// </typeparam>
    /// <remarks>
    /// Klasa automatycznie rejestruje i wyrejestrowuje komponent jako
    /// <see cref="IEventListener{TEvent}"/> podczas cyklu życia
    /// <see cref="MonoBehaviour"/> (<see cref="OnEnable"/> / <see cref="OnDisable"/>).
    ///
    /// Dzięki temu logika nasłuchu zdarzeń jest odseparowana od reszty kodu
    /// i łatwa do ponownego użycia.
    /// </remarks>
    /// <example>
    /// Przykład prostego listenera reagującego na zdarzenie <c>PlayerDiedEvent</c>:
    /// <code>
    /// public readonly struct PlayerDiedEvent : IEvent
    /// {
    ///     public int PlayerId { get; }
    ///
    ///     public PlayerDiedEvent(int playerId)
    ///     {
    ///         PlayerId = playerId;
    ///     }
    /// }
    ///
    /// public sealed class PlayerDeathUiListener
    ///     : EventListenerBehaviour&lt;PlayerDiedEvent&gt;
    /// {
    ///     [SerializeField] private GameObject gameOverPanel;
    ///
    ///     public override void OnEvent(PlayerDiedEvent evt)
    ///     {
    ///         Debug.Log($"Gracz {evt.PlayerId} zginął.");
    ///         gameOverPanel.SetActive(true);
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class EventListenerBehaviour<TEvent> : MonoBehaviour, IEventListener<TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Źródło zdarzeń używane przez ten komponent.
        /// Domyślnie jest to <see cref="GlobalEventBus.Instance"/>, ale
        /// można nadpisać tę właściwość w klasie pochodnej, aby użyć
        /// innej instancji <see cref="IEventBus"/> (np. scenowej, testowej).
        /// </summary>
        protected virtual IEventBus EventBus => GlobalEventBus.Instance;

        /// <summary>
        /// Rejestruje komponent jako listenera zdarzeń typu <typeparamref name="TEvent"/>
        /// w aktualnym <see cref="EventBus"/>.
        /// </summary>
        private void OnEnable() => EventBus.Subscribe(this);

        /// <summary>
        /// Wyrejestrowuje komponent z nasłuchiwania zdarzeń typu <typeparamref name="TEvent"/>.
        /// </summary>
        private void OnDisable() => EventBus.Unsubscribe(this);

        /// <summary>
        /// Metoda wywoływana przez <see cref="IEventBus"/> za każdym razem, gdy
        /// opublikowane zostanie zdarzenie typu <typeparamref name="TEvent"/>.
        /// </summary>
        /// <param name="evt">Instancja odebranego zdarzenia.</param>
        public abstract void OnEvent(TEvent evt);
    }
}