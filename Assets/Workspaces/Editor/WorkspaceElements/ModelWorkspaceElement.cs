namespace Howl.Workspaces
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class ModelWorkspaceElement : WorkspaceElement<GameObject, WorkspaceItemData>
    {
        private Editor _editor;

        public ModelWorkspaceElement() : base("graph-item--mesh")
        {
            Icon.RemoveFromHierarchy();
            var modelView = new IMGUIContainer(DrawMeshOnGUI)
                .AddClass("graph-item--mesh-preview")
                .AddTo(this);

            modelView.SendToBack();
        }

        ~ModelWorkspaceElement()
        {
            Object.DestroyImmediate(_editor);
        }

        private void DrawMeshOnGUI()
        {
            if (Asset == null)
                return;

            if (_editor == null)
                _editor = Editor.CreateEditor(Asset);

            var r = GUILayoutUtility.GetRect(64, 256, 64, 256);
            _editor.OnPreviewGUI(r, GUI.skin.box);
        }
    }
}