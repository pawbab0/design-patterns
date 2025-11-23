namespace PawBab.DesignPatterns.Command
{
    /// <summary>
    /// Specjalny typ oznaczający, że dana komenda nie wymaga żadnego kontekstu.
    /// <para>
    /// Przydatny, gdy chcesz użyć systemu komend w miejscach, gdzie nie potrzebujesz przekazywać
    /// dodatkowych danych (np. prosta zmiana ustawień globalnych, zapisywanie do PlayerPrefs itp.).
    /// </para>
    /// </summary>
    /// <remarks>
    /// Zamiast tworzyć własny pusty typ kontekstu, możesz użyć gotowego <see cref="NoContext"/> i
    /// za każdym razem przekazywać <see cref="Default"/>.
    /// </remarks>
    public readonly struct NoContext
    {
        /// <summary>
        /// Domyślna instancja pustego kontekstu.
        /// <para>
        /// Możesz ją przekazywać do komend jako parametr <c>context</c> wszędzie tam,
        /// gdzie nie jest wymagany żaden realny obiekt kontekstu.
        /// </para>
        /// <example>
        /// <code>
        /// var executor = new CommandExecutor&lt;NoContext&gt;(10);
        /// var cmd = new SetMasterVolumeCommand(0.5f);
        ///
        /// await executor.ExecuteAsync(cmd, NoContext.Default);
        /// </code>
        /// </example>
        /// </summary>
        public static readonly NoContext Default = new NoContext();
    }
}