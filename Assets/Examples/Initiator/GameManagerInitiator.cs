using UnityEngine;

namespace PawBab.Architecture.Examples.Initiator
{
    public class GameManagerInitiator : Architecture.Initiator.Initiator
    {
        [SerializeField]
        private float _fakeInitDelaySeconds = 1f;

        [SerializeField]
        private float _fakeRunDelaySeconds = 0.5f;

        public override async Awaitable InitAsync()
        {
            Debug.Log("[GameManagerInitiator] InitAsync start");

            if (_fakeInitDelaySeconds > 0f)
                await Awaitable.WaitForSecondsAsync(_fakeInitDelaySeconds);

            Debug.Log("[GameManagerInitiator] InitAsync done");
        }

        public override async Awaitable RunAsync()
        {
            Debug.Log("[GameManagerInitiator] RunAsync start");

            if (_fakeRunDelaySeconds > 0f)
                await Awaitable.WaitForSecondsAsync(_fakeRunDelaySeconds);

            Debug.Log("[GameManagerInitiator] RunAsync done");
        }
    }
}