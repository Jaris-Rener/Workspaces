namespace Howl.Workspaces
{
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public enum WorkspaceItemViewType
    {
        Icon = 0,
        Expanded = 1,
        Thin = 2
    }

    [Serializable]
    public class WorkspaceItemData
    {
        public WorkspaceItemViewType ViewType = WorkspaceItemViewType.Icon;
        public Color Color = Workspace.DefaultItemColor;
        public bool LockPosition;
        public Vector2 Position;
        public string AssetPath;

        public WorkspaceItemData() {}
        public WorkspaceItemData(Vector2 pos, string assetPath)
        {
            Position = pos;
            AssetPath = assetPath;
        }
    }

    [Serializable]
    public abstract class WorkspaceItemData<T> : WorkspaceItemData where T : Object
    {
    }
}