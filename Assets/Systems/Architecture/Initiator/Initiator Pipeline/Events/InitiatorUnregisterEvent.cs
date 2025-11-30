using PawBab.DesignPatterns.Observer;

namespace PawBab.Architecture.Initiator
{
    /// <summary>
    /// Zdarzenie informujące system o wyrejestrowaniu inicjatora.
    ///
    /// Publikowane zwykle przez klasę bazową <see cref="Initiator"/> w metodzie OnDisable,
    /// a obsługiwane przez <see cref="InitiatorManager"/>. Dzięki temu manager wie,
    /// że dany inicjator nie jest już dostępny i usuwa go z wewnętrznej kolekcji.
    /// </summary>
    public readonly struct InitiatorUnregisterEvent : IEvent
    {
        public IInitiator Initiator { get; }

        public InitiatorUnregisterEvent(IInitiator initiator)
        {
            Initiator = initiator;
        }
    }
}