namespace Howl.Workspaces
{
    using UnityEngine;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class AudioClipWorkspaceElement : WorkspaceElement<AudioClip, WorkspaceItemData>
    {
        public AudioClipWorkspaceElement() : base("graph-item--audioclip")
        {
        }
    }
}