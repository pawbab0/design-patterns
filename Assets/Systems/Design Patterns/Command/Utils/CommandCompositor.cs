using System.Collections.Generic;

namespace PawBab.DesignPatterns.Command
{
    /// <summary>
    /// Pomocnicza klasa do składania wielu komend w jedną komendę złożoną (<see cref="CompositeCommand{TContext}"/>).
    /// <para>
    /// Umożliwia:
    /// <list type="bullet">
    /// <item><description>dodawanie komend pojedynczo lub całymi kolekcjami,</description></item>
    /// <item><description>nadanie nazwy całemu „batchowi” operacji,</description></item>
    /// <item><description>skonfigurowanie zachowania przy błędzie (rollback już wykonanych komend).</description></item>
    /// </list>
    /// Gotowy obiekt można przekazać do <see cref="Build"/> lub skorzystać
    /// z niejawnej konwersji na <see cref="CompositeCommand{TContext}"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TContext">Typ kontekstu przekazywany do komend.</typeparam>
    public sealed class CommandCompositor<TContext>
    {
        private readonly List<IAsyncCommand<TContext>> _commands = new();

        /// <summary>
        /// Czytelna nazwa złożonej komendy.
        /// <para>
        /// Zostanie przypisana do wewnętrznego <see cref="CompositeCommand{TContext}"/> i może być później używana
        /// np. w logach, debugowaniu czy wyświetlaniu w UI (np. „Wykonaj 20 kroków do przodu”).
        /// </para>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Określa, czy w razie błędu podczas wykonywania złożonej komendy
        /// powinna zostać podjęta próba cofnięcia już wykonanych komend.
        /// <para>
        /// Gdy wartość to <see langword="true"/>, w momencie wyjątku CompositeCommand kolejno wywoła
        /// <c>UndoAsync</c> dla wszystkich komend, które zdążyły się wykonać.
        /// </para>
        /// </summary>
        public bool RollbackOnFailure { get; }


        /// <summary>
        /// Tworzy nowy obiekt pomagający złożyć wiele komend w jedną złożoną.
        /// </summary>
        /// <param name="name">
        /// Nazwa złożonej komendy. Jeśli nie zostanie podana, użyta zostanie wartość domyślna <c>"CompositeCommand"</c>.
        /// </param>
        /// <param name="rollbackOnFailure">
        /// Czy w razie błędu podczas wykonania którąś z komend, należy spróbować cofnąć
        /// wszystkie wcześniej wykonane komendy (tzw. „miękka transakcja”).
        /// </param>
        /// <example>
        /// <code>
        /// var compositor = new CommandCompositor&lt;GameplayContext&gt;(
        ///     name: "ComplexMove",
        ///     rollbackOnFailure: true);
        /// </code>
        /// </example>
        public CommandCompositor(string name = "CompositeCommand", bool rollbackOnFailure = true)
        {
            Name = name;
            RollbackOnFailure = rollbackOnFailure;
        }

        /// <summary>
        /// Dodaje pojedynczą komendę do zestawu komend złożonych.
        /// </summary>
        /// <param name="command">Komenda do dodania. Jeśli jest <see langword="null"/>, zostanie pominięta.</param>
        /// <returns>
        /// Ten sam obiekt <see cref="CommandCompositor{TContext}"/>, co pozwala łańcuchować wywołania
        /// (metoda w stylu "buildera").
        /// </returns>
        /// <example>
        /// <code>
        /// var compositor = new CommandCompositor&lt;GameplayContext&gt;("MultiMove");
        ///
        /// compositor
        ///     .Add(new MovePlayerCommand(Vector3.forward))
        ///     .Add(new MovePlayerCommand(Vector3.right));
        /// </code>
        /// </example>
        public CommandCompositor<TContext> Add(IAsyncCommand<TContext> command)
        {
            if (command != null)
                _commands.Add(command);

            return this;
        }


        /// <summary>
        /// Dodaje wiele komend jednocześnie.
        /// </summary>
        /// <param name="commands">
        /// Kolekcja komend do dodania. Jeśli jest <see langword="null"/>, metoda nie robi nic.
        /// </param>
        /// <returns>
        /// Ten sam obiekt <see cref="CommandCompositor{TContext}"/>, umożliwiając dalsze łańcuchowanie.
        /// </returns>
        /// <example>
        /// <code>
        /// var commands = new List&lt;IAsyncCommand&lt;GameplayContext&gt;&gt;
        /// {
        ///     new MovePlayerCommand(Vector3.forward),
        ///     new MovePlayerCommand(Vector3.left),
        ///     new MovePlayerCommand(Vector3.back)
        /// };
        ///
        /// var compositor = new CommandCompositor&lt;GameplayContext&gt;("PathMove");
        /// compositor.AddRange(commands);
        /// </code>
        /// </example>
        public CommandCompositor<TContext> AddRange(IEnumerable<IAsyncCommand<TContext>> commands)
        {
            if (commands == null)
                return this;

            _commands.AddRange(commands);
            return this;
        }

        /// <summary>
        /// Buduje finalną komendę złożoną (<see cref="CompositeCommand{TContext}"/>)
        /// z aktualnie zgromadzonego zestawu komend.
        /// </summary>
        /// <returns>
        /// Nowa instancja <see cref="CompositeCommand{TContext}"/>, gotowa do przekazania
        /// do <see cref="CommandExecutor{TContext}.ExecuteAsync"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// var compositor = new CommandCompositor&lt;GameplayContext&gt;("DashCombo");
        /// compositor
        ///     .Add(new MovePlayerCommand(Vector3.forward))
        ///     .Add(new MovePlayerCommand(Vector3.forward * 2));
        ///
        /// CompositeCommand&lt;GameplayContext&gt; composite = compositor.Build();
        /// await executor.ExecuteAsync(composite, context, addToHistory: true);
        /// </code>
        /// </example>
        public CompositeCommand<TContext> Build() => new(Name, RollbackOnFailure, _commands.ToArray());

        /// <summary>
        /// Niejawna konwersja z <see cref="CommandCompositor{TContext}"/> na
        /// <see cref="CompositeCommand{TContext}"/>.
        /// <para>
        /// Pozwala bezpośrednio przekazać obiekt <see cref="CommandCompositor{TContext}"/>
        /// tam, gdzie oczekiwany jest <see cref="CompositeCommand{TContext}"/>.
        /// </para>
        /// </summary>
        /// <param name="compositor">Obiekt kompozytora, z którego zostanie utworzona komenda złożona.</param>
        /// <returns>Nowa instancja <see cref="CompositeCommand{TContext}"/>.</returns>
        /// <example>
        /// <code>
        /// var compositor = new CommandCompositor&lt;GameplayContext&gt;("BatchMove");
        /// compositor.Add(new MovePlayerCommand(Vector3.forward));
        ///
        /// // Dzięki operatorowi implicit można przekazać "compositor" jak zwykłą komendę:
        /// await executor.ExecuteAsync(compositor, context);
        /// </code>
        /// </example>
        public static implicit operator CompositeCommand<TContext>(CommandCompositor<TContext> compositor) => compositor.Build();
    }
}