using PawBab.DesignPatterns.Observer;
using UnityEngine;

namespace PawBab.Architecture.Initiator
{
    /// <summary>
    /// Interfejs opisujący pojedynczy inicjator systemowy w aplikacji.
    /// Inicjator:
    /// - ma przypisany identyfikator typu <see cref="InitiatorTag"/>,
    /// - zgłasza się do <see cref="InitiatorManager"/>,
    /// - przechodzi dwie fazy cyklu życia: inicjalizacja (<see cref="InitAsync"/>) oraz uruchomienie (<see cref="RunAsync"/>).
    ///
    /// Typowe zastosowanie:
    /// - osobne inicjatory dla konfiguracji, sieci, audio, UI, gameplayu itp.,
    /// - <see cref="InitiatorManager"/> dba o kolejność i wywołanie wszystkich faz.
    /// </summary>
    public abstract class Initiator : MonoBehaviour, IInitiator
    {
        /// <summary>
        /// Tag identyfikujący inicjator.
        /// Ten sam typ tagu może być użyty w wielu scenach, ale w obrębie jednej sceny
        /// powinien występować tylko jeden inicjator dla danego tagu.
        /// </summary>
        [SerializeField]
        private InitiatorTag _tag;

        public InitiatorTag Tag => _tag;
        protected virtual IEventBus EventBus => GlobalEventBus.Instance;

        protected virtual void OnEnable()
        {
            if (_tag == null) Debug.LogError($"[Initiator] {name} nie ma ustawionego Tagu.");

            EventBus.Publish(new InitiatorRegisterEvent(this));
        }

        protected virtual void OnDisable()
        {
            EventBus.Publish(new InitiatorUnregisterEvent(this));
        }

        /// <summary>
        /// Asynchroniczna faza przygotowania inicjatora.
        /// W tej metodzie należy wykonywać:
        /// - wczytywanie danych,
        /// - przygotowanie zasobów,
        /// - tworzenie i konfigurację zależności.
        ///
        /// Jest wywoływana przez <see cref="InitiatorManager"/> zgodnie z kolejnością
        /// zadeklarowaną w jego konfiguracji.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Awaitable"/> reprezentujący asynchroniczne zakończenie procesu inicjalizacji.
        /// </returns>
        public abstract Awaitable InitAsync();


        /// <summary>
        /// Asynchroniczna faza uruchomienia właściwej logiki inicjatora.
        /// Jest wywoływana dopiero po tym, jak wszystkie wymagane inicjatory zakończą
        /// poprawnie swoją fazę <see cref="InitAsync"/>.
        ///
        /// Przykładowe zastosowania:
        /// - start pętli logiki gry,
        /// - włączenie UI po pełnym wczytaniu danych,
        /// - uruchomienie systemów zależnych od innych inicjatorów (np. audio po konfiguracji).
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Awaitable"/> reprezentujący asynchroniczne zakończenie fazy uruchomienia.
        /// </returns>
        public abstract Awaitable RunAsync();
    }
}