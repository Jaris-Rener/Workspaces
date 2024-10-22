namespace Howl.Workspaces
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    [UxmlElement]
    public abstract partial class WorkspaceElement : VisualElement
    {
        protected virtual bool SupportsViewType(WorkspaceItemViewType viewType)
        {
            return viewType switch
            {
                WorkspaceItemViewType.Icon => true,
                WorkspaceItemViewType.Expanded => false,
                WorkspaceItemViewType.Thin => true,
                _ => throw new ArgumentOutOfRangeException(nameof(viewType), viewType, null)
            };
        }

        protected WorkspaceWindow Window;

        protected readonly Image Icon;
        protected readonly Label Label;
        protected readonly VisualElement LockIcon;

        protected WorkspaceItemViewType ViewType { get; set; }

        public string AssetPath;

        public Color Color { get; private set; } = Workspace.DefaultItemColor;

        public bool Locked
        {
            get => _locked;
            protected set
            {
                _locked = value;
                LockIcon.visible = value;

                if (!_locked)
                    RemoveFromClassList("graph-item--locked");
                else
                    AddToClassList("graph-item--locked");
            }
        }

        protected Vector2 AbsolutePosition;
        protected Vector2 LocalPosition => AbsolutePosition + Window.Offset;

        private bool _locked;

        public WorkspaceElement()
        {
            var asset = Resources.Load<VisualTreeAsset>("WorkspaceElement");
            asset.CloneTree(this);

            Icon = this.Q<Image>("MainImage");
            Label = this.Q<Label>("Label");
            LockIcon = this.Q<VisualElement>("LockIcon");

            this.WithManipulator(new WorkspaceItemManipulator(OnClick, OnHoverStart, OnHoverStop));
            this.WithManipulator(new ContextualMenuManipulator(BuildContextMenu));
        }

        protected virtual void BuildContextMenu(ContextualMenuPopulateEvent evt)
        {
            var lockStatus = Locked ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
            evt.menu.AppendAction("Open", Open);
            evt.menu.AppendAction("Change Color", ChangeColor);
            evt.menu.AppendAction("Lock Position", LockPosition, lockStatus);
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Delete Item", Delete);

            foreach (WorkspaceItemViewType viewType in Enum.GetValues(typeof(WorkspaceItemViewType)))
            {
                var status = ViewType == viewType
                    ? DropdownMenuAction.Status.Checked
                    : DropdownMenuAction.Status.Normal;

                if (SupportsViewType(viewType))
                    evt.menu.AppendAction($"View/{viewType}", _ => SetView(viewType), status);
            }
        }

        protected void SetView(WorkspaceItemViewType viewType)
        {
            Label.RemoveFromClassList("graph-item-label--icon");
            Label.RemoveFromClassList("graph-item-label--thin");
            Label.RemoveFromClassList("graph-item-label--expanded");

            switch (viewType)
            {
                case WorkspaceItemViewType.Icon:
                    SetIconView();
                    break;
                case WorkspaceItemViewType.Expanded:
                    SetExpandedView();
                    break;
                case WorkspaceItemViewType.Thin:
                    SetThinView();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(viewType), viewType, null);
            }
        }

        protected virtual void SetThinView()
        {
            ViewType = WorkspaceItemViewType.Thin;

            Label.AddClass("graph-item-label--thin");
            style.maxWidth = StyleKeyword.None;
            style.flexDirection = FlexDirection.Row;
        }

        protected virtual void SetIconView()
        {
            ViewType = WorkspaceItemViewType.Icon;

            Label.AddClass("graph-item-label--icon");
            style.flexDirection = FlexDirection.Column;
            style.width = StyleKeyword.Null;
            style.height = StyleKeyword.Null;

            Icon.image = GetIcon();
        }

        protected virtual void SetExpandedView()
        {
            style.flexDirection = FlexDirection.Column;
            Label.AddClass("graph-item-label--expanded");
        }

        private void ChangeColor(DropdownMenuAction obj)
        {
            ReflectionAccessor.ShowColorPicker(SetColor, Color);
            return;
        }

        protected void SetColor(Color color)
        {
            Color = color;
            style.backgroundColor = Color;
        }

        private void LockPosition(DropdownMenuAction obj)
        {
            Locked = !Locked;
        }

        private void Delete(DropdownMenuAction obj)
        {
            Window.RemoveItem(this);
        }

        private void Open(DropdownMenuAction obj)
        {
            EditorUtility.OpenWithDefaultApp(AssetPath);
        }

        protected virtual void OnClick(int button) { }

        protected virtual void OnHoverStart()
        {
            Window.HoverStart(this);
        }

        protected virtual void OnHoverStop()
        {
            Window.HoverStop(this);
        }

        public abstract void Setup(WorkspaceWindow window, WorkspaceItemData data);

        protected virtual string GetLabel() => Path.GetFileNameWithoutExtension(AssetPath);
        protected virtual Texture GetIcon() => AssetDatabase.GetCachedIcon(AssetPath);

        public void SetOffset(Vector2 offset)
        {
            transform.position = LocalPosition;
        }

        public void InitPosition(Vector2 pos)
        {
            AbsolutePosition = pos;
            transform.position = LocalPosition;
        }

        public abstract WorkspaceItemData GetData();

        public void MovePosition(Vector2 delta)
        {
            if (Locked)
                return;

            AbsolutePosition += delta;
            transform.position = LocalPosition;
        }

        public void SnapToGrid()
        {
            var snappedPos = AbsolutePosition.SnapToGrid(64);
            if (!Locked)
                InitPosition(snappedPos);
        }
    }

    public abstract class WorkspaceElement<TAsset, TData> : WorkspaceElement
        where TAsset : Object
        where TData : WorkspaceItemData, new()
    {
        public TAsset Asset => _asset != null ? _asset : _asset = GetAsset();
        private TAsset _asset;

        private TAsset GetAsset() => AssetDatabase.LoadAssetAtPath<TAsset>(AssetPath);

        protected WorkspaceElement() { }

        protected WorkspaceElement(string styleClass) => this.AddClass(styleClass);

        ~WorkspaceElement()
        {
            Window.OnGraphOffsetChanged -= SetOffset;
        }

        public sealed override void Setup(WorkspaceWindow window, WorkspaceItemData itemData)
        {
            Window = window;
            AssetPath = itemData.AssetPath;
            Label.text = GetLabel();
            Icon.image = GetIcon();

            InitPosition(itemData.Position);
            SetColor(itemData.Color);
            SetView(itemData.ViewType);
            Locked = itemData.LockPosition;

            Window.OnGraphOffsetChanged += SetOffset;

            if (Asset == null)
                this.AddClass("graph-item--missing");

            if (itemData is TData typedData)
                Setup(typedData);
        }

        protected virtual void Setup(TData data) {}

        protected override void OnClick(int button)
        {
            if (button == 0)
                PingAsset();
        }

        protected void PingAsset()
        {
            Select();
            EditorGUIUtility.PingObject(Asset);
        }

        protected override void BuildContextMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextMenu(evt);
            evt.menu.AppendAction("Select", Select);
        }


        private void Select(DropdownMenuAction obj) => Select();

        private void Select()
        {
            Selection.activeObject = Asset;
        }

        public override WorkspaceItemData GetData()
        {
            return new TData
            {
                Position = transform.position,
                AssetPath = AssetPath,
                Color = Color,
                LockPosition = Locked,
                ViewType = ViewType
            };
        }
    }
}