using UnityEngine;

namespace PawBab.Architecture.Initiator
{
    /// <summary>
    /// ScriptableObject reprezentujący tag inicjatora.
    ///
    /// Tag pełni rolę unikalnego identyfikatora typu inicjatora:
    /// - <see cref="InitiatorManager"/> używa listy tagów do określenia kolejności Init/Run,
    /// - konkretne klasy inicjatorów przypisują sobie dany tag w Inspectorze,
    /// - pozwala to utrzymywać kolejność i zależności bez twardych referencji między klasami.
    /// </summary>
    [CreateAssetMenu(fileName = "InitiatorTag", menuName = "Initiator/Tag")]
    public class InitiatorTag : ScriptableObject
    {
    }
}