namespace PawBab.DesignPatterns.FSM
{
    /// <summary>
    /// Interfejs dla stanów, które wymagają przekazania danych wejściowych (payloadu)
    /// w momencie przełączania się do tego stanu.
    /// <para>
    /// Umożliwia budowanie stanów, które reagują na kontekst – np. stan ładowania sceny
    /// z nazwą sceny, stan ruchu z pozycją docelową, stan dialogu z treścią kwestii itd.
    /// </para>
    /// <para>
    /// Typ <typeparamref name="TPayload"/> może być zarówno prostym typem (np. <c>string</c>),
    /// jak i złożoną strukturą/rekordem lub krotką (ValueTuple), jeśli chcesz przekazać
    /// wiele wartości naraz.
    /// </para>
    /// </summary>
    /// <typeparam name="TPayload">
    /// Typ danych przekazywanych do stanu. Powinien jednoznacznie opisywać wszystkie
    /// informacje potrzebne temu stanowi do poprawnego działania.
    /// </typeparam>
    public interface IPayloadState<IPayload>
    {
        /// <summary>
        /// Ustawia dane wejściowe (payload) dla stanu przed jego aktywacją.
        /// <para>
        /// Metoda jest wywoływana automatycznie przez rozszerzenie
        /// <see cref="StateMachineExtensions.ChangeState{TOwner,TState,TPayload}"/>
        /// tuż przed wywołaniem <c>OnEnter()</c> na danym stanie.
        /// </para>
        /// </summary>
        /// <param name="payload">
        /// Dane, które stan powinien zapamiętać i wykorzystać w swojej logice
        /// (np. w <c>OnEnter</c>, <c>Tick</c> lub <c>OnExit</c>).
        /// </param>
        void SetPayload(IPayload payload);
    }
}