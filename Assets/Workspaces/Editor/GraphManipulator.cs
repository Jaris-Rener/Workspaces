namespace Howl.Workspaces
{
    using System;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class GraphManipulator : PointerManipulator
    {
        private bool _isDragging = false;
        private Action<Vector2> _onDrag;
        private Action<float> _onScroll;

        public GraphManipulator(VisualElement root)
        {
            target = root.Q<VisualElement>("GraphRoot");
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<WheelEvent>(OnScroll);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<WheelEvent>(OnScroll);
        }

        private void OnScroll(WheelEvent evt)
        {
            _onScroll?.Invoke(evt.delta.y);
            evt.StopPropagation();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 1)
                return;

            if (_isDragging)
                return;

            _isDragging = true;
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging)
                return;

            _onDrag?.Invoke(evt.deltaPosition);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isDragging)
                return;

            _isDragging = false;
            evt.StopImmediatePropagation();
        }

        public void OnDrag(Action<Vector2> onDrag)
        {
            _onDrag = onDrag;
        }

        public void OnScroll(Action<float> onScroll)
        {
            _onScroll = onScroll;
        }
    }
}