using System;

namespace PawBab.Architecture.Initiator
{
    /// <summary>
    /// Wyjątek wewnętrzny sygnalizujący, że nie wszystkie wymagane inicjatory
    /// zostały zarejestrowane w momencie próby rozpoczęcia inicjalizacji.
    ///
    /// Jest używany tylko przez <see cref="InitiatorManager"/> do kontrolowania
    /// przepływu w maszynie stanów. Nie powinien być obsługiwany poza modułem inicjatora.
    /// </summary>
    internal class NotAllRequiredInitiatorsRegisteredException : Exception
    {
    }
}