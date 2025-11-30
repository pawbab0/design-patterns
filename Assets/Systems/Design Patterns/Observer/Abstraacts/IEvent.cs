namespace PawBab.DesignPatterns.Observer
{
    /// <summary>
    /// Marker interface dla wszystkich typów zdarzeń obsługiwanych przez
    /// system obserwatora (<see cref="IEventBus"/>).
    /// </summary>
    /// <remarks>
    /// Każdy typ zdarzenia publikowanego w <see cref="IEventBus"/> powinien
    /// implementować ten interfejs. Zwykle są to lekkie, niemutowalne struktury
    /// (<c>readonly struct</c>) lub klasy, zawierające tylko dane potrzebne
    /// do obsługi logiki biznesowej.
    /// </remarks>
    /// <example>
    /// Przykładowe zdarzenie informujące o śmierci gracza:
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
    /// </code>
    /// </example>
    public interface IEvent
    {
    }
}