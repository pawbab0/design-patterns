using PawBab.DesignPatterns.Command;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PawBab.DesignPatterns.Examples.Command
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private int undoLimit = 20;

        private CommandExecutor<Transform> _executor;

        private void Awake()
        {
            _executor = new(undoLimit);
        }

        private async Awaitable Update()
        {
            if (_executor.IsBusy)
                return;

            if (Keyboard.current.wKey.isPressed)
                await ExecuteMove(Vector3.forward);
            else if (Keyboard.current.aKey.isPressed)
                await ExecuteMove(Vector3.left);
            else if (Keyboard.current.sKey.isPressed)
                await ExecuteMove(Vector3.back);
            else if (Keyboard.current.dKey.isPressed)
                await ExecuteMove(Vector3.right);
            else if (Keyboard.current.spaceKey.wasPressedThisFrame)
                await ExecuteCompositeCommand();
            else if (Keyboard.current.escapeKey.isPressed && _executor.CanUndo)
                await _executor.UndoAsync(transform);
        }

        private Awaitable ExecuteCompositeCommand()
        {
            var cmd = new CommandCompositor<Transform>()
                .Add(new MoveTransformCommand(Vector3.left))
                .Add(new MoveTransformCommand(Vector3.back))
                .Add(new MoveTransformCommand(Vector3.right))
                .Add(new MoveTransformCommand(Vector3.forward));

            return _executor.ExecuteAsync(cmd.Build(), transform);
        }

        private Awaitable ExecuteMove(Vector3 delta)
        {
            var cmd = new MoveTransformCommand(delta);
            return _executor.ExecuteAsync(cmd, transform);
        }
    }
}