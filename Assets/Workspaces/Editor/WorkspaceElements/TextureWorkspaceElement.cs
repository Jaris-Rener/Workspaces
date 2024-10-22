namespace Howl.Workspaces
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class TextureWorkspaceElement : WorkspaceElement<Texture2D, WorkspaceItemData>
    {
        public TextureWorkspaceElement() : base("graph-item--texture") {}

        protected override bool SupportsViewType(WorkspaceItemViewType viewType)
        {
            return viewType switch
            {
                WorkspaceItemViewType.Icon => true,
                WorkspaceItemViewType.Expanded => true,
                WorkspaceItemViewType.Thin => true,
                _ => throw new ArgumentOutOfRangeException(nameof(viewType), viewType, null)
            };
        }

        protected override void SetExpandedView()
        {
            base.SetExpandedView();

            ViewType = WorkspaceItemViewType.Expanded;

            var aspectRatio = (float)Asset.width/Asset.height;
            if (aspectRatio > 1)
            {
                Icon.style.maxHeight = new StyleLength(Mathf.Min(Asset.height, 128));
                Icon.style.maxWidth = new StyleLength(Icon.style.maxHeight.value.value*aspectRatio);
            }
            else
            {
                Icon.style.maxWidth = new StyleLength(Mathf.Min(Asset.width, 128));
                Icon.style.maxHeight = new StyleLength(Icon.style.maxWidth.value.value/aspectRatio);
            }

            Icon.image = Asset;
        }

        protected override Texture GetIcon() => AssetPreview.GetMiniTypeThumbnail(typeof(Texture2D));
    }
}