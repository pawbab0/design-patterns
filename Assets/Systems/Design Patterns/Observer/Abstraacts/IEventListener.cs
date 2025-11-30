namespace PawBab.DesignPatterns.Observer
{
    /// <summary>
    /// Kontrakt obserwatora zdarzeń dla systemu opartego o <see cref="IEventBus"/>.
    /// </summary>
    /// <typeparam name="TEvent">
    /// Typ zdarzenia obsługiwanego przez listenera. Musi implementować <see cref="IEvent"/>.
    /// </typeparam>
    /// <remarks>
    /// Implementacje tego interfejsu są wywoływane przez <see cref="IEventBus"/>
    /// w momencie publikacji zdarzenia danego typu.
    /// </remarks>
    /// <example>
    /// <code>
    /// public readonly struct ScoreChangedEvent : IEvent
    /// {
    ///     public int NewScore { get; }
    ///
    ///     public ScoreChangedEvent(int newScore)
    ///     {
    ///         NewScore = newScore;
    ///     }
    /// }
    ///
    /// public sealed class ScoreUIListener : IEventListener&lt;ScoreChangedEvent&gt;
    /// {
    ///     public void OnEvent(ScoreChangedEvent evt)
    ///     {
    ///         UnityEngine.Debug.Log($"Nowy wynik: {evt.NewScore}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IEventListener<in TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Metoda wywoływana przez <see cref="IEventBus"/> za każdym razem,
        /// gdy opublikowane zostanie zdarzenie typu <typeparamref name="TEvent"/>.
        /// </summary>
        /// <param name="evt">Instancja zdarzenia, na które listener ma zareagować.</param>
        void OnEvent(TEvent evt);
    }
}