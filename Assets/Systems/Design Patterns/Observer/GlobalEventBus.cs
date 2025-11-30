using System;

namespace PawBab.DesignPatterns.Observer
{
    /// <summary>
    /// Globalny punkt dostępu do domyślnej instancji <see cref="IEventBus"/>.
    /// </summary>
    /// <remarks>
    /// Klasa udostępnia statyczną właściwość <see cref="Instance"/>, która
    /// domyślnie jest ustawiona na nową instancję <see cref="EventBus"/>.
    ///
    /// W testach lub specyficznych konfiguracjach (np. per scena) można
    /// wstrzyknąć własną implementację <see cref="IEventBus"/> poprzez
    /// ustawienie właściwości <see cref="Instance"/>.
    /// </remarks>
    /// <example>
    /// Publikowanie zdarzenia przez globalny EventBus:
    /// <code>
    /// GlobalEventBus.Instance.Publish(new PlayerDiedEvent(playerId: 1));
    /// </code>
    ///
    /// Podmiana EventBusa w testach:
    /// <code>
    /// [SetUp]
    /// public void SetUp()
    /// {
    ///     GlobalEventBus.Instance = new EventBus();
    /// }
    /// </code>
    /// </example>
    public static class GlobalEventBus
    {
        /// <summary>
        /// Aktualna globalna instancja <see cref="IEventBus"/>.
        /// Domyślnie ustawiona na <see cref="EventBus"/>.
        /// </summary>
        private static IEventBus _instance = new EventBus();

        /// <summary>
        /// Pobiera lub ustawia globalną instancję <see cref="IEventBus"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Rzucane, gdy próbuje się ustawić wartość <c>null</c>.
        /// </exception>
        public static IEventBus Instance
        {
            get => _instance;
            set => _instance = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}