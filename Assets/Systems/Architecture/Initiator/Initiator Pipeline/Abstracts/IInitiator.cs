using UnityEngine;

namespace PawBab.Architecture.Initiator
{
    /// <summary>
    /// Abstrakcyjna klasa bazowa dla wszystkich inicjatorów sceny.
    ///
    /// Zapewnia:
    /// - pole i właściwość tagu inicjatora (<see cref="_tag"/> / <see cref="Tag"/>),
    /// - automatyczną rejestrację w <see cref="InitiatorManager"/> poprzez publikację eventów:
    ///     - <see cref="InitiatorRegisterEvent"/> w metodzie <see cref="OnEnable"/>,
    ///     - <see cref="InitiatorUnregisterEvent"/> w metodzie <see cref="OnDisable"/>,
    /// - punkt rozszerzenia dla implementacji <see cref="InitAsync"/> i <see cref="RunAsync"/>.
    ///
    /// Typowy scenariusz:
    /// - dziedziczysz po <see cref="Initiator"/>,
    /// - ustawiasz <see cref="_tag"/> w Inspectorze,
    /// - w <see cref="InitAsync"/> przygotowujesz system,
    /// - w <see cref="RunAsync"/> uruchamiasz jego właściwą logikę.
    /// </summary>
    public interface IInitiator
    {
        InitiatorTag Tag { get; }

        Awaitable InitAsync();
        Awaitable RunAsync();
    }
}