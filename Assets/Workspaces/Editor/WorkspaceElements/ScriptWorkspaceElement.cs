namespace Howl.Workspaces
{
    using UnityEditor;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class ScriptWorkspaceElement : WorkspaceElement<MonoScript, WorkspaceItemData>
    {
        public ScriptWorkspaceElement() : base("graph-item--script")
        {
        }
    }
}