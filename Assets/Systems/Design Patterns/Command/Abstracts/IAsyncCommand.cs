using UnityEngine;

namespace PawBab.DesignPatterns.Command
{
    /// <summary>
    /// Interfejs dla asynchronicznych komend z możliwością cofania (Undo).
    /// <para>
    /// Każda komenda powinna:
    /// <list type="bullet">
    /// <item><description>mieć czytelną nazwę (<see cref="Name"/>),</description></item>
    /// <item><description>implementować logikę wykonania (<see cref="ExecuteAsync"/>),</description></item>
    /// <item><description>implementować logikę cofania (<see cref="UndoAsync"/>).</description></item>
    /// </list>
    /// Interfejs jest generyczny – można go użyć z różnymi typami kontekstu, np. <c>GameplayContext</c>,
    /// <c>InventoryContext</c>, a gdy kontekst nie jest potrzebny – z <see cref="NoContext"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TContext">
    /// Typ kontekstu przekazywany do metody <see cref="ExecuteAsync"/> i <see cref="UndoAsync"/>.
    /// </typeparam>
    public interface IAsyncCommand<TContext>
    {

        /// <summary>
        /// Nazwa komendy.
        /// <para>
        /// Może służyć do debugowania, logowania, a także do wyświetlania użytkownikowi
        /// listy wykonanych operacji (np. w historii edytora lub w menu „Cofnij / Ponów”).
        /// </para>
        /// </summary>
        string Name { get; }
        
                /// <summary>
        /// Asynchronicznie wykonuje komendę w podanym kontekście.
        /// <para>
        /// Wewnątrz tej metody możesz korzystać z:
        /// <list type="bullet">
        /// <item><description><c>await Awaitable.NextFrameAsync()</c> do animacji w czasie,</description></item>
        /// <item><description>operacji IO,</description></item>
        /// <item><description>dowolnych innych asynchronicznych akcji.</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="context">
        /// Kontekst, na którym operuje komenda – może zawierać np. referencje do obiektów sceny, systemów itp.
        /// </param>
        /// <returns>
        /// Awaitable reprezentujący asynchroniczne wykonanie komendy. Musi być możliwy do użycia z <c>await</c>.
        /// </returns>
        /// <example>
        /// Przykład prostej implementacji komendy:
        /// <code>
        /// public sealed class MovePlayerCommand : IAsyncCommand&lt;GameplayContext&gt;
        /// {
        ///     public string Name =&gt; "MovePlayer";
        ///
        ///     private readonly Vector3 _delta;
        ///     private Vector3 _previousPosition;
        ///
        ///     public MovePlayerCommand(Vector3 delta)
        ///     {
        ///         _delta = delta;
        ///     }
        ///
        ///     public async Awaitable ExecuteAsync(GameplayContext context)
        ///     {
        ///         var t = context.PlayerTransform;
        ///         _previousPosition = t.position;
        ///
        ///         t.position += _delta;
        ///         await Awaitable.NextFrameAsync();
        ///     }
        ///
        ///     public async Awaitable UndoAsync(GameplayContext context)
        ///     {
        ///         context.PlayerTransform.position = _previousPosition;
        ///         await Awaitable.NextFrameAsync();
        ///     }
        /// }
        /// </code>
        /// </example>
        Awaitable ExecuteAsync(TContext context);
                
        /// <summary>
        /// Asynchronicznie cofa efekty działania komendy.
        /// <para>
        /// W typowej implementacji metoda ta powinna przywracać stan sprzed wywołania
        /// <see cref="ExecuteAsync"/> (np. pozycję obiektu, wartości w systemie ekwipunku, ustawienia itp.).
        /// </para>
        /// </summary>
        /// <param name="context">
        /// Ten sam typ kontekstu, który był używany przy wykonaniu komendy.
        /// </param>
        /// <returns>Awaitable reprezentujący operację cofania.</returns>
        Awaitable UndoAsync(TContext context);
    }
}