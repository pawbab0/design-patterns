using PawBab.DesignPatterns.Command;
using UnityEngine;

namespace PawBab.DesignPatterns.Examples.Command
{
    public sealed class MoveTransformCommand : IAsyncCommand<Transform>
    {
        private const float Duration = 0.3f;

        public string Name => "Move Transform";

        private readonly Vector3 _delta;
        private Vector3 _previousPosition;

        public MoveTransformCommand(Vector3 delta) => _delta = delta;

        public async Awaitable ExecuteAsync(Transform context)
        {
            _previousPosition = context.position;
            var targetPosition = context.position + _delta;

            await MoveAsync(context, _previousPosition, targetPosition);
        }

        public async Awaitable UndoAsync(Transform context)
        {
            var targetPosition = _previousPosition;
            var startPosition = context.position;

            await MoveAsync(context, startPosition, targetPosition);
        }

        private static async Awaitable MoveAsync(Transform context, Vector3 from, Vector3 to)
        {
            var elapsed = 0f;

            while (elapsed < Duration)
            {
                await Awaitable.NextFrameAsync();
                elapsed += Time.deltaTime;
                var alpha = Mathf.Clamp01(elapsed / Duration);
                context.position = Vector3.Lerp(from, to, alpha);
            }

            context.position = to;
        }
    }
}