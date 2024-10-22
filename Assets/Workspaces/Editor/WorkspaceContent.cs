namespace Howl.Workspaces
{
    using UnityEditor;
    using UnityEngine;

    public static class WorkspaceContent
    {
        public static Texture2D WindowIcon => EditorGUIUtility.FindTexture("d_CustomTool");
    }
}