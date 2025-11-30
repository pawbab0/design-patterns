using PawBab.DesignPatterns.Observer;

namespace PawBab.Architecture.Initiator
{
    /// <summary>
    /// Zdarzenie informujące system o zarejestrowaniu nowego inicjatora.
    ///
    /// Publikowane zwykle przez klasę bazową <see cref="Initiator"/> w metodzie OnEnable,
    /// a obsługiwane przez <see cref="InitiatorManager"/>. Na podstawie tego zdarzenia
    /// manager buduje mapę inicjatorów przypisanych do tagów.
    /// </summary>
    public readonly struct InitiatorRegisterEvent : IEvent
    {
        public IInitiator Initiator { get; }

        public InitiatorRegisterEvent(IInitiator initiator)
        {
            Initiator = initiator;
        }
    }
}