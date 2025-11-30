using UnityEngine;

namespace PawBab.Architecture.Examples.Initiator
{
    public class PlayerManagerInitiator : Architecture.Initiator.Initiator
    {
        [SerializeField]
        private float _fakeInitDelaySeconds = 1f;

        [SerializeField]
        private float _fakeRunDelaySeconds = 0.5f;

        public override async Awaitable InitAsync()
        {
            Debug.Log("[PlayerManagerInitiator] InitAsync start");

            if (_fakeInitDelaySeconds > 0f)
                await Awaitable.WaitForSecondsAsync(_fakeInitDelaySeconds);

            Debug.Log("[PlayerManagerInitiator] InitAsync done");
        }

        public override async Awaitable RunAsync()
        {
            Debug.Log("[PlayerManagerInitiator] RunAsync start");

            if (_fakeRunDelaySeconds > 0f)
                await Awaitable.WaitForSecondsAsync(_fakeRunDelaySeconds);

            Debug.Log("[PlayerManagerInitiator] RunAsync done");
        }
    }
}