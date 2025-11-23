using System;
using UnityEngine;

namespace PawBab.DesignPatterns.Command
{
    /// <summary>
    /// Uniwersalny wykonawca komend asynchronicznych z obsługą historii cofania (Undo).
    /// <para>
    /// Odpowiada za:
    /// <list type="bullet">
    /// <item><description>kolejkowanie pojedynczych komend,</description></item>
    /// <item><description>zapisywanie ich do historii (z limitem kroków),</description></item>
    /// <item><description>cofanie ostatniej komendy lub wszystkich komend,</description></item>
    /// <item><description>zapewnienie, że w danym momencie wykonywana jest tylko jedna komenda (flaga <see cref="IsBusy"/>).</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Typ kontekstu <typeparamref name="TContext"/> pozwala dopasować ten sam mechanizm do różnych systemów,
    /// np. dla ruchu gracza, ekwipunku, ustawień itp.
    /// </para>
    /// </summary>
    /// <typeparam name="TContext">
    /// Typ kontekstu przekazywany do komend. Może to być np. <c>GameplayContext</c>, <c>InventoryContext</c>,
    /// albo <see cref="NoContext"/> gdy komenda nie wymaga dodatkowych danych.
    /// </typeparam>
    public sealed class CommandExecutor<TContext>
    {
        private readonly CommandHistory<TContext> _history;

        /// <summary>
        /// Informuje, czy aktualnie wykonywana jest jakakolwiek komenda.
        /// <para>
        /// Wartość <see langword="true"/> oznacza, że:
        /// <list type="bullet">
        /// <item><description>trwa wykonywanie komendy (<see cref="ExecuteAsync"/>), lub</description></item>
        /// <item><description>trwa cofanie komendy (<see cref="UndoAsync"/> / <see cref="UndoAll"/>).</description></item>
        /// </list>
        /// Dzięki temu możesz łatwo zablokować UI (np. przyciski) na czas operacji.
        /// </para>
        /// </summary>
        public bool IsBusy { get; private set; }

        /// <summary>
        /// Maksymalna liczba komend przechowywana w historii Undo.
        /// <para>
        /// Po przekroczeniu limitu najstarsze wpisy są usuwane, tak aby zachować tylko nowsze komendy.
        /// Zmiana tej wartości działa dynamicznie – historia jest przycinana do nowego limitu.
        /// </para>
        /// <example>
        /// Przykład ustawienia limitu historii na 10 ostatnich komend:
        /// <code>
        /// var executor = new CommandExecutor&lt;GameplayContext&gt;(maxUndoSteps: 20);
        /// executor.UndoLimit = 10; // teraz trzymamy tylko 10 ostatnich komend
        /// </code>
        /// </example>
        /// </summary>
        public int UndoLimit
        {
            get => _history.MaxSteps;
            set => _history.MaxSteps = value;
        }

        /// <summary>
        /// Informuje, czy istnieje przynajmniej jedna komenda w historii, którą można cofnąć.
        /// <para>
        /// Zwraca <see langword="true"/>, jeśli lista historii nie jest pusta. Przydatne np. do
        /// aktywowania/dezaktywowania przycisku "Cofnij" w UI.
        /// </para>
        /// </summary>
        public bool CanUndo => !_history.IsEmpty;

        /// <summary>
        /// Tworzy nowy wykonawca komend z ustawionym limitem kroków Undo.
        /// </summary>
        /// <param name="maxUndoSteps">
        /// Maksymalna liczba komend do przechowywania w historii.
        /// Wartości mniejsze od 0 są traktowane jako 0 (Undo zostaje de facto wyłączone).
        /// </param>
        /// <example>
        /// <code>
        /// // Executor dla kontekstu gameplay ze śledzeniem 20 ostatnich komend:
        /// var executor = new CommandExecutor&lt;GameplayContext&gt;(maxUndoSteps: 20);
        /// </code>
        /// </example>
        public CommandExecutor(int maxUndoSteps)
        {
            _history = new(maxUndoSteps);
        }

        /// <summary>
        /// Asynchronicznie wykonuje podaną komendę w zadanym kontekście.
        /// <para>
        /// W zależności od parametru <paramref name="addToHistory"/> komenda może zostać dodana do historii
        /// Undo lub wykonana jednorazowo bez możliwości cofnięcia.
        /// </para>
        /// </summary>
        /// <param name="command">Komenda do wykonania.</param>
        /// <param name="context">Kontekst przekazywany do komendy (np. referencje do obiektów sceny).</param>
        /// <param name="addToHistory">
        /// Czy komenda powinna zostać dodana do historii cofania. Domyślnie <see langword="true"/>.
        /// </param>
        /// <returns>
        /// Awaitable reprezentujący asynchroniczne wykonanie komendy. Można na nim użyć <c>await</c>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Rzucany, jeśli równolegle próbowano wykonać inną komendę i <see cref="IsBusy"/> było w tym momencie
        /// ustawione na <see langword="true"/>.
        /// </exception>
        /// <example>
        /// <code>
        /// var executor = new CommandExecutor&lt;GameplayContext&gt;(10);
        /// var context = new GameplayContext(playerTransform);
        /// var command = new MovePlayerCommand(Vector3.forward);
        ///
        /// await executor.ExecuteAsync(command, context, addToHistory: true);
        /// </code>
        /// </example>
        public async Awaitable ExecuteAsync(IAsyncCommand<TContext> command, TContext context, bool addToHistory = true)
        {
            if (IsBusy)
                throw new InvalidOperationException("CommandExecutor jest już zajęty.");

            IsBusy = true;

            try
            {
                await command.ExecuteAsync(context);

                if (addToHistory)
                    _history.Push(command);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Asynchronicznie cofa ostatnią komendę z historii.
        /// <para>
        /// Jeśli historia jest pusta, metoda zwraca <see langword="false"/> i nie wykonuje żadnej operacji.
        /// W przeciwnym wypadku ostatnia komenda jest usuwana z historii i wywoływany jest jej <c>UndoAsync</c>.
        /// </para>
        /// </summary>
        /// <param name="context">
        /// Kontekst przekazywany do komendy podczas cofania. Powinien być spójny z tym użytym przy <c>ExecuteAsync</c>.
        /// </param>
        /// <returns>
        /// Awaitable, które po zakończeniu zwraca:
        /// <list type="bullet">
        /// <item><description><see langword="true"/> – gdy cofnięto jakąś komendę,</description></item>
        /// <item><description><see langword="false"/> – gdy nie było czego cofnąć.</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Rzucany, jeśli w momencie wywołania <see cref="IsBusy"/> jest równe <see langword="true"/>.
        /// </exception>
        /// <example>
        /// <code>
        /// if (executor.CanUndo)
        /// {
        ///     bool undone = await executor.UndoAsync(context);
        ///     if (!undone)
        ///     {
        ///         Debug.Log("Brak komend do cofnięcia.");
        ///     }
        /// }
        /// </code>
        /// </example>
        public async Awaitable<bool> UndoAsync(TContext context)
        {
            if (IsBusy)
                throw new InvalidOperationException("CommandExecutor jest już zajęty.");

            var cmd = _history.Pop();
            if (cmd == null)
                return false;

            IsBusy = true;

            try
            {
                await cmd.UndoAsync(context);
                return true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Asynchronicznie cofa wszystkie komendy zapisane w historii Undo.
        /// <para>
        /// Komendy są cofane w odwrotnej kolejności do ich wykonania (LIFO).
        /// Po zakończeniu działania historia jest pusta.
        /// </para>
        /// </summary>
        /// <param name="context">
        /// Kontekst przekazywany do każdej komendy podczas cofania.
        /// </param>
        /// <returns>Awaitable reprezentujący proces cofania wszystkich komend.</returns>
        /// <exception cref="InvalidOperationException">
        /// Rzucany, jeśli w momencie wywołania inna komenda jest wykonywana lub cofana
        /// i <see cref="IsBusy"/> jest ustawione na <see langword="true"/>.
        /// </exception>
        /// <example>
        /// <code>
        /// // Cofnij wszystkie operacje użytkownika (np. przy wyjściu z edytora):
        /// await executor.UndoAll(levelContext);
        /// </code>
        /// </example>
        public async Awaitable UndoAll(TContext context)
        {
            if (IsBusy)
                throw new InvalidOperationException("CommandExecutor jest już zajęty.");

            IsBusy = true;

            try
            {
                while (_history.Pop() is { } cmd)
                    await cmd.UndoAsync(context);
            }
            finally
            {
                IsBusy = false;
            }
        }


        /// <summary>
        /// Czyści historię komend bez wykonywania cofania.
        /// <para>
        /// Przydatne np. po zaakceptowaniu zmian przez użytkownika, gdy dalsze Undo nie ma już sensu
        /// (np. zapis stanu projektu, wejście w nowy rozdział gry itp.).
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Rzucany, gdy w momencie czyszczenia trwa wykonywanie jakiejś komendy (<see cref="IsBusy"/> jest <see langword="true"/>).
        /// </exception>
        /// <example>
        /// <code>
        /// // Po zapisaniu stanu aplikacji:
        /// executor.ClearHistory();
        /// </code>
        /// </example>
        public void ClearHistory()
        {
            if (IsBusy)
                throw new InvalidOperationException("CommandExecutor jest już zajęty.");

            _history.Clear();
        }
    }
}