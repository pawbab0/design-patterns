using System.Collections.Generic;

namespace PawBab.DesignPatterns.Command
{
    /// <summary>
    /// Prosta implementacja historii wykonanych komend z limitem liczby kroków.
    /// <para>
    /// Przechowuje komendy w strukturze LIFO (ostatnia dodana – pierwsza cofana),
    /// co idealnie pasuje do mechanizmu Undo/Redo.
    /// </para>
    /// </summary>
    /// <typeparam name="TContext">
    /// Typ kontekstu używany przez komendy. Ten sam, co w <see cref="IAsyncCommand{TContext}"/>.
    /// </typeparam>
    public sealed class CommandHistory<TContext>
    {
        private readonly LinkedList<IAsyncCommand<TContext>> _commands = new();
        private int _maxSteps;

        /// <summary>
        /// Maksymalna liczba komend przechowywana w historii.
        /// <para>
        /// W przypadku przekroczenia limitu najstarsze komendy są usuwane automatycznie
        /// po każdym dodaniu nowej komendy.
        /// </para>
        /// </summary>
        public int MaxSteps
        {
            get => _maxSteps;
            set
            {
                _maxSteps = value < 0 ? 0 : value;
                TrimToLimit();
            }
        }

        /// <summary>
        /// Informuje, czy historia jest pusta.
        /// <para>
        /// Zwraca <see langword="true"/>, gdy nie ma żadnych komend w historii.
        /// </para>
        /// </summary>
        public bool IsEmpty => _commands.Count == 0;


        /// <summary>
        /// Tworzy historię komend z zadanym limitem maksymalnej liczby kroków.
        /// </summary>
        /// <param name="maxSteps">
        /// Maksymalna liczba komend, które mogą być zapisane w historii.
        /// Wartość mniejsza od 0 jest traktowana jako 0.
        /// </param>
        public CommandHistory(int maxSteps)
        {
            _maxSteps = maxSteps < 0 ? 0 : maxSteps;
        }

        /// <summary>
        /// Dodaje komendę do historii.
        /// <para>
        /// Jeśli <see cref="MaxSteps"/> wynosi 0, metoda nie wykonuje żadnej operacji (Undo jest wyłączone).
        /// W przypadku przekroczenia limitu najstarsza komenda jest usuwana.
        /// </para>
        /// </summary>
        /// <param name="command">Komenda do zapisania w historii.</param>
        public void Push(IAsyncCommand<TContext> command)
        {
            if (_maxSteps == 0)
                return;

            _commands.AddLast(command);
            TrimToLimit();
        }

        /// <summary>
        /// Usuwa i zwraca ostatnią komendę z historii.
        /// </summary>
        /// <returns>
        /// Ostatnia komenda z historii lub <see langword="null"/>, jeśli historia jest pusta.
        /// </returns>
        public IAsyncCommand<TContext> Pop()
        {
            if (_commands.Last == null)
                return null;

            var last = _commands.Last.Value;
            _commands.RemoveLast();
            return last;
        }

        /// <summary>
        /// Czyści całą historię komend.
        /// <para>
        /// Po wywołaniu tej metody nie można już wykonać Undo, dopóki nie pojawią się nowe komendy.
        /// </para>
        /// </summary>
        public void Clear() => _commands.Clear();


        /// <summary>
        /// Pomocnicza metoda przycinająca historię do aktualnego limitu <see cref="MaxSteps"/>.
        /// <para>
        /// Usuwa najstarsze komendy tak długo, aż liczba elementów będzie mniejsza niż limit.
        /// </para>
        /// </summary>
        private void TrimToLimit()
        {
            while (_commands.Count >= _maxSteps)
                _commands.RemoveFirst();
        }
    }
}