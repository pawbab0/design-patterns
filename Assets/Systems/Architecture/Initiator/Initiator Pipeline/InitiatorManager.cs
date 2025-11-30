using System;
using System.Collections.Generic;
using PawBab.DesignPatterns.FSM;
using PawBab.DesignPatterns.Observer;
using UnityEngine;

namespace PawBab.Architecture.Initiator
{
    /// <summary>
    /// Centralny manager odpowiedzialny za proces inicjalizacji wszystkich inicjatorów w scenie.
    ///
    /// Zadania managera:
    /// - przechowuje listę wymaganych tagów inicjatorów (<c>_requiredInitiatorsOrder</c>),
    /// - nasłuchuje zdarzeń rejestracji i wyrejestrowania inicjatorów,
    /// - w momencie gdy wszystkie wymagane tagi są obsadzone:
    ///     1. wywołuje sekwencyjnie <see cref="IInitiator.InitAsync"/> dla wszystkich,
    ///     2. następnie wywołuje sekwencyjnie <see cref="IInitiator.RunAsync"/> dla wszystkich,
    /// - komunikuje stan procesu za pomocą wewnętrznej maszyny stanów.
    ///
    /// Dzięki temu modułowi możesz trzymać inicjalizację wielu systemów rozproszonych po scenie
    /// w jednym, jasno zdefiniowanym przepływie.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("Architecture/Initiator Manager")]
    public sealed class InitiatorManager :
        MonoBehaviour,
        IEventListener<InitiatorRegisterEvent>,
        IEventListener<InitiatorUnregisterEvent>
    {
        /// <summary>
        /// Lista tagów inicjatorów, które są wymagane w tej scenie.
        /// Kolejność elementów w tej tablicy określa kolejność wywołań InitAsync/RunAsync.
        ///
        /// Jeśli lista jest pusta, manager nie będzie wymuszał obecności żadnych inicjatorów.
        /// </summary>
        [Tooltip("Lista tagów, które muszą się zarejestrować. Kolejność definiuje kolejność wywołań Init/Run.")]
        [SerializeField]
        private InitiatorTag[] _requiredInitiatorsOrder;

        private readonly Dictionary<InitiatorTag, IInitiator> _registeredByTag = new();
        private StateMachine<InitiatorManager> _fsm;

        private IEventBus EventBus => GlobalEventBus.Instance;

        private void OnEnable()
        {
            InitStateMachine();

            EventBus.Subscribe<InitiatorRegisterEvent>(this);
            EventBus.Subscribe<InitiatorUnregisterEvent>(this);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InitiatorRegisterEvent>(this);
            EventBus.Unsubscribe<InitiatorUnregisterEvent>(this);
        }

        public void OnEvent(InitiatorRegisterEvent evt)
        {
            var initiator = evt.Initiator;

            if (initiator == null)
            {
                Debug.LogWarning("[InitiatorManager] Otrzymano InitiatorRegisteredEvent z null.");
                return;
            }

            var tag = initiator.Tag;
            if (tag == null)
            {
                Debug.LogWarning($"[InitiatorManager] Inicjator {initiator} nie ma ustawionego Tagu.");
                return;
            }

            var isExpected = IsTagRequired(tag);

            if (!isExpected)
            {
                Debug.LogWarning($"[InitiatorManager] Wykryto inicjator z tagiem '{tag.name}', którego nie ma na liście wymaganych.");
                return;
            }

            if (_registeredByTag.TryGetValue(tag, out var existing))
            {
                Debug.LogWarning($"[InitiatorManager] Tag '{tag.name}' jest już zajęty przez inny inicjator. Pomijam.");
                return;
            }

            _registeredByTag[tag] = initiator;

            if (_fsm.HasState<InitiatorNonInitialized>())
                _fsm.ChangeState<InitiatorManager, InitiatorInitializing, Action>(TryStartInitializationIfReady);
        }

        public void OnEvent(InitiatorUnregisterEvent evt)
        {
            var initiator = evt.Initiator;
            if (initiator == null)
                return;

            var initiatorTag = initiator.Tag;
            if (initiatorTag == null)
                return;

            if (_registeredByTag.TryGetValue(initiatorTag, out var existing) && ReferenceEquals(existing, initiator))
                _registeredByTag.Remove(initiatorTag);
        }

        private void InitStateMachine()
        {
            if (_fsm != null)
                return;

            _fsm = new(this);
            _fsm.AddStates(new InitiatorNonInitialized(), new InitiatorInitializing(), new InitiatorInitialized());
            _fsm.ChangeState<InitiatorNonInitialized>();
        }

        private bool IsTagRequired(InitiatorTag tag)
        {
            if (_requiredInitiatorsOrder == null)
                return false;

            foreach (var initiatorTag in _requiredInitiatorsOrder)
            {
                if (initiatorTag == tag)
                    return true;
            }

            return false;
        }

        private void TryStartInitializationIfReady()
        {
            if (!AreAllRequiredRegistered())
                throw new NotAllRequiredInitiatorsRegisteredException();

            _ = HandleInitiators();
        }

        private bool AreAllRequiredRegistered()
        {
            if (_requiredInitiatorsOrder == null || _requiredInitiatorsOrder.Length == 0)
                return true;

            foreach (var initiatorTag in _requiredInitiatorsOrder)
            {
                if (initiatorTag == null)
                {
                    Debug.LogWarning("[InitiatorManager] Na liście wymaganych jest null tag.");
                    continue;
                }

                if (!_registeredByTag.ContainsKey(initiatorTag))
                    return false;
            }

            return true;
        }

        private async Awaitable HandleInitiators()
        {
            OrderInitiators(out var ordered);
            await InitAsync(ordered);
            await RunAsync(ordered);
            Debug.Log("[InitiatorManager] Inicjatory zostały zainicjalizowane i uruchomione.");
        }

        private void OrderInitiators(out List<IInitiator> ordered)
        {
            ordered = new(_requiredInitiatorsOrder.Length);

            foreach (var initiatorTag in _requiredInitiatorsOrder)
            {
                if (initiatorTag == null)
                    continue;
                if (_registeredByTag.TryGetValue(initiatorTag, out var initiator))
                    ordered.Add(initiator);
                else
                    Debug.LogError($"[InitiatorManager] Brakuje inicjatora dla tagu '{initiatorTag.name}' podczas startu.");
            }
        }

        private static async Awaitable InitAsync(List<IInitiator> ordered)
        {
            foreach (var initiator in ordered)
            {
                try
                {
                    await initiator.InitAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[InitiatorManager] Błąd w InitAsync ({initiator}): {ex}");
                }
            }
        }

        private static async Awaitable RunAsync(List<IInitiator> ordered)
        {
            foreach (var initiator in ordered)
            {
                try
                {
                    await initiator.RunAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[InitiatorManager] Błąd w RunAsync ({initiator}): {ex}");
                }
            }
        }
    }

    internal class InitiatorNonInitialized : State<InitiatorManager>
    {
    }

    internal class InitiatorInitializing : State<InitiatorManager>, IPayloadState<Action>
    {
        private Action initAction;

        public override void OnEnter()
        {
            try
            {
                initAction.Invoke();
                Machine.ChangeState<InitiatorInitialized>();
            }
            catch (NotAllRequiredInitiatorsRegisteredException e)
            {
                Machine.ChangeState<InitiatorNonInitialized>();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Machine.ChangeState<InitiatorNonInitialized>();
            }
        }

        public void SetPayload(Action payload)
        {
            initAction = payload;
        }
    }

    internal class InitiatorInitialized : State<InitiatorManager>
    {
    }
}