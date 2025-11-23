using UnityEngine;

namespace PawBab.DesignPatterns.FSM
{
    /// <summary>
    /// Bazowa klasa pomocnicza dla komponentów Unity, które korzystają z maszyny stanów.
    /// <para>
    /// Klasa:
    /// <list type="bullet">
    ///   <item><description>Tworzy maszynę stanów w metodzie <c>Awake()</c>.</description></item>
    ///   <item><description>Wywołuje <c>Tick()</c> w <c>Update()</c> oraz <c>FixedTick()</c> w <c>FixedUpdate()</c>.</description></item>
    ///   <item><description>Wymusza implementację metod do rejestracji i ustawienia pierwszego stanu.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Dzięki temu w klasach dziedziczących skupiasz się tylko na definiowaniu stanów
    /// i logiki przełączania, a nie na klejeniu pętli aktualizacji.
    /// </para>
    /// </summary>
    /// <typeparam name="TOwner">
    /// Typ właściciela maszyny stanów. Najczęściej będzie to typ klasy dziedziczącej,
    /// np. <c>PlayerController</c>, <c>EnemyAI</c>, <c>Door</c>.
    /// </typeparam>
    public abstract class StateMachineBehaviour<TOwner> : MonoBehaviour
    {
        /// <summary>
        /// Maszyna stanów przypisana do tego komponentu.
        /// <para>
        /// Tworzona w metodzie <see cref="Awake"/> na podstawie wartości zwracanej przez
        /// właściwość <see cref="Owner"/>.
        /// </para>
        /// </summary>
        protected StateMachine<TOwner> StateMachine { get; private set; }

        /// <summary>
        /// Obiekt będący właścicielem maszyny stanów.
        /// <para>
        /// W typowym przypadku będzie to <c>this</c> rzutowane na konkretny typ,
        /// ale można też zwrócić inny obiekt, jeśli logika maszyny dotyczy czegoś zewnętrznego.
        /// </para>
        /// </summary>
        protected abstract TOwner Owner { get; }

        /// <summary>
        /// Metoda wywoływana w <see cref="Awake"/>, w której należy zarejestrować
        /// wszystkie stany maszyny, np. przy użyciu
        /// <see cref="StateMachineExtensions.AddStates{TOwner}"/>.
        /// </summary>
        protected abstract void SetStates();

        /// <summary>
        /// Metoda wywoływana w <see cref="Start"/>, w której należy ustawić stan początkowy,
        /// np. <c>StateMachine.ChangeState&lt;IdleState&gt;()</c>.
        /// </summary>
        protected abstract void SetFirstState();

        /// <summary>
        /// Tworzy maszynę stanów i rejestruje stany.
        /// <para>
        /// Wywoływana automatycznie przez Unity. W większości przypadków nie ma potrzeby
        /// jej nadpisywania – logikę konfiguracji przenieś do <see cref="SetStates"/>.
        /// </para>
        /// </summary>
        protected virtual void Awake()
        {
            StateMachine = new(Owner);
            SetStates();
        }

        /// <summary>
        /// Ustawia stan początkowy maszyny stanów.
        /// <para>
        /// Domyślna implementacja wywołuje tylko <see cref="SetFirstState"/>.
        /// </para>
        /// </summary>
        protected virtual void Start() => SetFirstState();

        /// <summary>
        /// Aktualizacja logiki maszyny stanów w każdej klatce gry.
        /// </summary>
        private void Update() => StateMachine.Tick(Time.deltaTime);

        /// <summary>
        /// Aktualizacja logiki maszyny stanów w każdym kroku fizyki.
        /// </summary>
        private void FixedUpdate() => StateMachine.FixedTick(Time.fixedDeltaTime);
    }
}