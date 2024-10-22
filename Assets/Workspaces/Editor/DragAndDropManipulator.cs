using UnityEditor;
using UnityEngine.UIElements;

namespace Howl.Workspaces
{
    using System;
    using UnityEngine;

    public class DragAndDropManipulator : PointerManipulator
    {
        private Action<Vector2, string[]> _onAddsAssetCallback;

        public DragAndDropManipulator(VisualElement root)
        {
            target = root.Q<VisualElement>("GraphRoot");
        }

        public void OnAddAssetCallback(Action<Vector2, string[]> onAddAssetCallback)
            => _onAddsAssetCallback = onAddAssetCallback;

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<DragEnterEvent>(OnDragEnter);
            target.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            target.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.RegisterCallback<DragPerformEvent>(OnDragPerform);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<DragEnterEvent>(OnDragEnter);
            target.UnregisterCallback<DragLeaveEvent>(OnDragLeave);
            target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.UnregisterCallback<DragPerformEvent>(OnDragPerform);
        }

        private void OnDragEnter(DragEnterEvent _)
        {
            target.AddToClassList("graph-area--dropping");
        }

        private void OnDragLeave(DragLeaveEvent _)
        {
            target.RemoveFromClassList("graph-area--dropping");
        }

        private void OnDragUpdate(DragUpdatedEvent _)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            target.RemoveFromClassList("graph-area--dropping");

            _onAddsAssetCallback?.Invoke(evt.localMousePosition, DragAndDrop.paths);
        }
    }
}