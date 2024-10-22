namespace Howl.Workspaces
{
    using System;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class WorkspaceItemManipulator : PointerManipulator
    {
        private Vector3 _startPos;

        private bool _isDragging;
        private Vector2 _dragOffset;
        private readonly Action<int> _onClick;
        private readonly Action _onHoverStart;
        private readonly Action _onHoverStop;

        public WorkspaceItemManipulator(Action<int> onClick, Action onHoverStart, Action onHoverStop)
        {
            _onClick = onClick;
            _onHoverStart = onHoverStart;
            _onHoverStop = onHoverStop;

            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse
            });
            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.RightMouse
            });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!CanStartManipulation(evt) || _isDragging)
                return;

            if (evt.button != 0)
            {
                evt.StopPropagation();
                return;
            }

            _startPos = evt.position;
            _dragOffset = evt.localPosition;
            _isDragging = true;

            target.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging || !target.HasPointerCapture(evt.pointerId))
                return;

            var delta = evt.localPosition - (Vector3)_dragOffset;
            var element = (WorkspaceElement)target;
            element.MovePosition(delta);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!CanStopManipulation(evt) || !_isDragging)
                return;

            _isDragging = false;
            target.ReleasePointer(evt.pointerId);
            evt.StopPropagation();

            if (_startPos == evt.position)
                _onClick?.Invoke(evt.button);
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            _onHoverStart?.Invoke();
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            _onHoverStop?.Invoke();
        }
    }
}