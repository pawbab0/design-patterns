using System;
using UnityEngine;

namespace PawBab.DesignPatterns.Command
{
    /// <summary>
    /// Komenda złożona (Composite) grupująca wiele komend w jedną operację.
    /// <para>
    /// Umożliwia:
    /// <list type="bullet">
    /// <item><description>wykonanie wielu komend sekwencyjnie jako jednej „akcji użytkownika”,</description></item>
    /// <item><description>cofnięcie wszystkich wykonanych komend jednym wywołaniem <see cref="UndoAsync"/>,</description></item>
    /// <item><description>opcjonalny rollback w razie błędu w trakcie wykonania (jeśli <c>rollbackOnFailure</c> jest ustawiony).</description></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <typeparam name="TContext">Typ kontekstu przekazywany do wszystkich komend wewnątrz kompozytu.</typeparam>
    public sealed class CompositeCommand<TContext> : IAsyncCommand<TContext>
    {
        /// <summary>
        /// Czytelna nazwa komendy złożonej.
        /// <para>
        /// Przydaje się do logowania, debugowania oraz wyświetlania w UI (np. „Złożona operacja budowy ściany”).
        /// </para>
        /// </summary>
        public string Name { get; }

        private readonly IAsyncCommand<TContext>[] _commands;
        private readonly bool _rollbackOnFailure;
        private int _executedCount;

        /// <summary>
        /// Tworzy nową komendę złożoną z podanego zestawu komend.
        /// </summary>
        /// <param name="name">
        /// Nazwa komendy – jeśli jest pusta lub zawiera wyłącznie białe znaki, użyta zostanie wartość domyślna
        /// <c>"CompositeCommand"</c>.
        /// </param>
        /// <param name="rollbackOnFailure">
        /// Określa, czy w razie wyjątku podczas wykonywania jednej z komend, powinien być wykonany rollback –
        /// czyli próba wywołania <c>UndoAsync</c> na wszystkich komendach, które zdążyły się już wykonać.
        /// </param>
        /// <param name="commands">Tablica komend, które mają zostać wykonane w określonej kolejności.</param>
        /// <example>
        /// <code>
        /// var moveForward = new MovePlayerCommand(Vector3.forward);
        /// var moveRight   = new MovePlayerCommand(Vector3.right);
        ///
        /// var composite = new CompositeCommand&lt;GameplayContext&gt;(
        ///     name: "MoveForwardRight",
        ///     rollbackOnFailure: true,
        ///     commands: new[] { moveForward, moveRight });
        /// </code>
        /// </example>
        public CompositeCommand(string name, bool rollbackOnFailure, params IAsyncCommand<TContext>[] commands)
        {
            Name = string.IsNullOrWhiteSpace(name) ? Consts.Command.DefaultCompositeCommandName : name;
            _rollbackOnFailure = rollbackOnFailure;
            _commands = commands ?? Array.Empty<IAsyncCommand<TContext>>();
        }


        /// <summary>
        /// Asynchronicznie wykonuje wszystkie komendy wchodzące w skład kompozytu.
        /// <para>
        /// Komendy wykonywane są sekwencyjnie, w kolejności ich podania w konstruktorze.
        /// W przypadku błędu:
        /// <list type="bullet">
        /// <item><description>
        /// jeśli <c>_rollbackOnFailure == false</c> – wyjątek jest propagowany bez cofania,
        /// </description></item>
        /// <item><description>
        /// jeśli <c>_rollbackOnFailure == true</c> – zostanie wykonana próba cofnięcia
        /// już wykonanych komend (w odwrotnej kolejności), a następnie wyjątek zostanie ponownie rzucony.
        /// </description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="context">
        /// Kontekst przekazywany do każdej komendy z wewnętrznej tablicy.
        /// </param>
        /// <returns>Awaitable reprezentujący wykonanie całej operacji złożonej.</returns>
        /// <example>
        /// <code>
        /// var composite = new CompositeCommand&lt;GameplayContext&gt;(
        ///     "ComboAttack",
        ///     rollbackOnFailure: true,
        ///     new AttackCommand(...),
        ///     new PlayAnimationCommand(...),
        ///     new SpawnEffectCommand(...));
        ///
        /// await executor.ExecuteAsync(composite, gameplayContext);
        /// </code>
        /// </example>
        public async Awaitable ExecuteAsync(TContext context)
        {
            _executedCount = 0;

            try
            {
                foreach (var cmd in _commands)
                {
                    if (cmd == null)
                        continue;

                    await cmd.ExecuteAsync(context);
                    _executedCount++;
                }
            }
            catch
            {
                if (!_rollbackOnFailure || _executedCount <= 0)
                    throw;

                for (var i = _executedCount - 1; i >= 0; i--)
                {
                    var cmd = _commands[i];
                    if (cmd == null)
                        continue;

                    try
                    {
                        await cmd.UndoAsync(context);
                    }
                    catch
                    {
                        //ignore
                    }
                }

                throw;
            }
        }

        /// <summary>
        /// Asynchronicznie cofa wszystkie komendy wchodzące w skład tej komendy złożonej.
        /// <para>
        /// Cofane są tylko te komendy, które zostały faktycznie wykonane podczas ostatniego <see cref="ExecuteAsync"/>.
        /// Dzięki temu jeżeli wykonanie zostało przerwane w połowie (np. przez wyjątek),
        /// <see cref="UndoAsync"/> nie będzie próbowało cofnąć niewykonanych komend.
        /// </para>
        /// </summary>
        /// <param name="context">
        /// Kontekst przekazywany do każdej komendy podczas cofania.
        /// </param>
        /// <returns>Awaitable reprezentujący proces cofania wszystkich wykonanych komend.</returns>
        /// <example>
        /// <code>
        /// // Załóżmy, że "composite" zostało wcześniej zapisane w historii:
        /// await composite.UndoAsync(gameplayContext);
        /// </code>
        /// </example>
        public async Awaitable UndoAsync(TContext context)
        {
            var count = Math.Min(_executedCount, _commands.Length);

            for (var i = count - 1; i >= 0; i--)
            {
                var cmd = _commands[i];
                if (cmd == null)
                    continue;

                await cmd.UndoAsync(context);
            }
        }
    }
}