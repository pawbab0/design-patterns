using System;
using System.Collections.Generic;
using UnityEngine;

namespace PawBab.DesignPatterns.FSM
{
    /// <summary>
    /// Uniwersalna maszyna stanów obsługująca dowolny typ właściciela.
    /// <para>
    /// Maszyna przechowuje zarejestrowane stany, dba o wywoływanie cyklu życia
    /// (<c>OnEnter</c>, <c>OnExit</c>, <c>Tick</c>, <c>FixedTick</c>) oraz dostarcza
    /// metody do przełączania pomiędzy stanami.
    /// </para>
    /// <para>
    /// Dzięki typowi generycznemu <typeparamref name="TOwner"/> możesz reuse’ować
    /// ten sam mechanizm FSM w wielu systemach (postacie, AI, UI, logika scenariuszy,
    /// obiekty techniczne typu drzwi/taśmociąg itp.).
    /// </para>
    /// </summary>
    /// <typeparam name="TOwner">
    /// Typ obiektu, którego zachowanie jest kontrolowane przez maszynę stanów.
    /// </typeparam>
    public class StateMachine<TOwner>
    {
        /// <summary>
        /// Zdarzenie wywoływane przy każdej zmianie stanu.
        /// <para>
        /// Pierwszy parametr to stan poprzedni (może być <c>null</c> przy pierwszym ustawieniu),
        /// drugi parametr to stan nowy. Można wykorzystać to np. do logowania,
        /// debugowania lub aktualizacji UI diagnostycznego.
        /// </para>
        /// </summary>
        public event Action<State<TOwner>, State<TOwner>> StateChanged;

        private readonly TOwner _owner;
        private readonly Dictionary<Type, State<TOwner>> _states;
        private State<TOwner> _current;

        /// <summary>
        /// Typ aktualnego stanu.
        /// <para>
        /// Jest równy <c>null</c>, jeśli <see cref="CurrentState"/> jest <c>null</c>.
        /// </para>
        /// </summary>
        public Type CurrentStateType => _current?.GetType();

        // <summary>
        /// Tworzy nową maszynę stanów dla wskazanego właściciela.
        /// </summary>
        /// <param name="owner">
        /// Obiekt, którego zachowanie będzie kontrolowane przez maszyny stanów.
        /// Referencja ta jest przekazywana do wszystkich stanów przy rejestracji.
        /// </param>
        public StateMachine(TOwner owner)
        {
            _owner = owner;
            _states = new();
        }

        /// <summary>
        /// Zwraca zarejestrowany stan danego typu.
        /// <para>
        /// Jeśli stan o takim typie nie został wcześniej zarejestrowany metodą
        /// <see cref="AddState"/>, metoda zaloguje błąd i zwróci <c>null</c>.
        /// </para>
        /// </summary>
        /// <typeparam name="TState">Typ oczekiwanego stanu.</typeparam>
        /// <returns>
        /// Instancja stanu typu <typeparamref name="TState"/>, jeśli została zarejestrowana;
        /// w przeciwnym wypadku <c>null</c>.
        /// </returns>
        public TState GetState<TState>() where TState : State<TOwner>
        {
            try
            {
                return (TState)_states[typeof(TState)];
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError($"Unknown state ({nameof(TState)}) for: {_owner}");
                return null;
            }
        }

        public bool TryGetState<TState>(out TState state) where TState : State<TOwner>
        {
            if (_states.TryGetValue(typeof(TState), out var s))
            {
                state = (TState)s;
                return true;
            }

            state = null;
            return false;
        }

        /// <summary>
        /// Sprawdza, czy aktualny stan maszyny jest danego typu.
        /// <para>
        /// Użyteczne jako prosty guard np. przy zewnętrznych wywołaniach:
        /// pozwala ograniczyć akcje tylko do konkretnych stanów.
        /// </para>
        /// </summary>
        /// <typeparam name="TState">Oczekiwany typ stanu.</typeparam>
        /// <returns>
        /// <c>true</c>, jeśli obecny stan jest typu <typeparamref name="TState"/>,
        /// w przeciwnym razie <c>false</c>.
        /// </returns>
        public bool IsInState<TState>() where TState : State<TOwner>
            => _current is TState;

        public bool HasState<TState>() where TState : State<TOwner>
            => _states.ContainsKey(typeof(TState));

        /// <summary>
        /// Dodaje stan do maszyny stanów.
        /// <para>
        /// Metoda:
        /// <list type="number">
        ///   <item><description>Inicjalizuje stan właścicielem i maszyną.</description></item>
        ///   <item><description>Rejestruje stan w słowniku pod jego konkretnym typem.</description></item>
        /// </list>
        /// Jeśli stan danego typu był już zarejestrowany, zostanie nadpisany.
        /// </para>
        /// </summary>
        /// <param name="state">Instancja stanu do zarejestrowania.</param>
        public void AddState(State<TOwner> state)
        {
            state.Initialize(_owner, this);
            _states[state.GetType()] = state;
        }

        /// <summary>
        /// Usuwa stan danego typu z maszyny stanów.
        /// <para>
        /// Jeśli usuwany stan jest aktualnie aktywnym stanem:
        /// <list type="bullet">
        ///   <item><description>najpierw wywoływane jest <c>OnExit()</c> na tym stanie,</description></item>
        ///   <item><description>następnie aktualny stan jest ustawiany na <c>null</c>.</description></item>
        /// </list>
        /// Metoda nie przełącza automatycznie na inny stan – decyzja o tym, jaki stan
        /// powinien być ustawiony dalej, należy do kodu wywołującego.
        /// </para>
        /// </summary>
        /// <typeparam name="TState">Typ stanu do usunięcia.</typeparam>
        /// <returns>
        /// <c>true</c>, jeśli stan danego typu został znaleziony i usunięty;
        /// w przeciwnym razie <c>false</c>.
        /// </returns>
        public bool RemoveState<TState>() where TState : State<TOwner>
        {
            var type = typeof(TState);

            if (!TryGetState<TState>(out var state))
                return false;

            if (_current == state)
            {
                _current.OnExit();
                _current = null;
            }

            return _states.Remove(type);
        }

        /// <summary>
        /// Przełącza maszynę stanów na wskazany typ stanu.
        /// <para>
        /// Jeżeli:
        /// <list type="bullet">
        ///   <item><description>stan docelowy jest tym samym, co obecny – nic się nie dzieje,</description></item>
        ///   <item><description>stan docelowy nie został znaleziony – przełączenie jest ignorowane.</description></item>
        /// </list>
        /// W przypadku poprawnego przełączenia:
        /// <list type="number">
        ///   <item><description>wywoływane jest <c>OnExit()</c> na obecnym stanie (jeśli istnieje),</description></item>
        ///   <item><description>ustawiany jest nowy stan,</description></item>
        ///   <item><description>wywoływane jest <c>OnEnter()</c> na nowym stanie,</description></item>
        ///   <item><description>wywoływane jest zdarzenie <see cref="StateChanged"/>.</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <typeparam name="TState">Typ stanu, na który ma zostać przełączona maszyna.</typeparam>
        public void ChangeState<TState>() where TState : State<TOwner>
        {
            var newState = GetState<TState>();
            if (_current == newState || newState == null)
                return;

            var previous = _current;

            _current?.OnExit();
            _current = newState;
            _current.OnEnter();

            StateChanged?.Invoke(previous, _current);
        }

        /// <summary>
        /// Wywołuje logikę aktualnego stanu powiązaną z aktualizacją w każdej klatce.
        /// <para>
        /// Zwykle wywoływana z metody <c>Update()</c> MonoBehaviour albo
        /// z innej pętli głównej Twojego systemu.
        /// </para>
        /// </summary>
        /// <param name="deltaTime">Czas, jaki upłynął od poprzedniej klatki.</param>
        public void Tick(float deltaTime)
            => _current?.Tick(deltaTime);


        /// <summary>
        /// Wywołuje logikę aktualnego stanu powiązaną z aktualizacją fizyki.
        /// <para>
        /// Zwykle wywoływana z metody <c>FixedUpdate()</c> MonoBehaviour.
        /// </para>
        /// </summary>
        /// <param name="fixedDeltaTime">Czas kroku fizyki.</param>
        public void FixedTick(float fixedDeltaTime)
            => _current?.FixedTick(fixedDeltaTime);
    }
}