namespace Howl.Workspaces
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class Workspace
    {
        public static readonly Color DefaultItemColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        public List<WorkspaceItemData> Items = new();
    }
}