namespace PawBab.DesignPatterns.FSM
{
    /// <summary>
    /// Zestaw metod rozszerzających ułatwiających pracę z maszyną stanów.
    /// <para>
    /// Zawiera m.in. wsparcie dla stanów z payloadem oraz wygodne dodawanie wielu stanów
    /// naraz.
    /// </para>
    /// </summary>
    public static class StateMachineExtensions
    {
        /// <summary>
        /// Przełącza maszynę stanów na wskazany stan, przekazując mu dane wejściowe (payload).
        /// <para>
        /// Metoda:
        /// <list type="number">
        ///   <item><description>Pobiera instancję stanu za pomocą <c>GetState&lt;TState&gt;()</c>.</description></item>
        ///   <item><description>Wywołuje na niej <c>SetPayload(payload)</c>.</description></item>
        ///   <item><description>Przełącza maszynę na ten stan poprzez <c>ChangeState&lt;TState&gt;()</c>.</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Dzięki temu możesz w jednym wywołaniu przekazać do stanu wszystkie potrzebne dane,
        /// np. nazwę sceny, cel ruchu, parametry animacji, konfigurację zadania itp.
        /// </para>
        /// </summary>
        /// <typeparam name="TOwner">
        /// Typ właściciela maszyny stanów.
        /// </typeparam>
        /// <typeparam name="TState">
        /// Typ stanu, na który ma zostać przełączona maszyna.
        /// Musi implementować <see cref="State{TOwner}"/> oraz <see cref="IPayloadState{TPayload}"/>.
        /// </typeparam>
        /// <typeparam name="TPayload">
        /// Typ danych przekazywanych do stanu. Może być prostym typem lub obiektem złożonym
        /// (rekord, struktura, krotka).
        /// </typeparam>
        /// <param name="machine">Maszyna stanów, która ma zostać przełączona.</param>
        /// <param name="payload">
        /// Dane wejściowe przekazywane do stanu przed wywołaniem <c>OnEnter()</c>.
        /// </param>
        public static void ChangeState<TOwner, TState, TPayload>(this StateMachine<TOwner> machine, TPayload payload)
            where TState : State<TOwner>, IPayloadState<TPayload>
        {
            var state = machine.GetState<TState>();
            state.SetPayload(payload);
            machine.ChangeState<TState>();
        }
        
        /// <summary>
        /// Dodaje wiele stanów do maszyny stanów za jednym wywołaniem.
        /// <para>
        /// Jest to wygodny sposób na rejestrację wszystkich stanów w metodzie inicjalizującej,
        /// np.:
        /// <code>
        /// StateMachine.AddStates(
        ///     new IdleState(),
        ///     new MoveState(),
        ///     new AttackState()
        /// );
        /// </code>
        /// </para>
        /// </summary>
        /// <typeparam name="TOwner">Typ właściciela maszyny stanów.</typeparam>
        /// <param name="machine">Maszyna stanów, do której dodawane są stany.</param>
        /// <param name="states">Zbiór instancji stanów do zarejestrowania.</param>

        public static void AddStates<TOwner>(this StateMachine<TOwner> machine, params State<TOwner>[] states)
        {
            foreach (var state in states)
                machine.AddState(state);
        }
    }
}