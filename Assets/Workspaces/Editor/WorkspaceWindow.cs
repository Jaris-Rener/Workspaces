namespace Howl.Workspaces
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    public class WorkspaceWindow : EditorWindow
    {
        private static readonly JsonSerializerSettings _jsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        };

        private static string _workspacePath => Path.Combine(Application.persistentDataPath, "workspace.json");

        public Vector2 Offset => _graphOffset;
        private Vector2 _graphOffset;
        private float _graphZoom;

        public event Action<Vector2> OnGraphOffsetChanged;
        public event Action<float> OnGraphZoomChanged;

        [SerializeField] private VisualTreeAsset _root;
        [SerializeField] private VisualTreeAsset _graphItem;

        private VisualElement _graphRoot;
        private Toolbar _toolbar;
        private Button _saveButton;

        private DragAndDropManipulator _dragAndDropManipulator;
        private GraphManipulator _graphManipulator;

        private Workspace _activeWorkspace = new();
        private Label _debugText;
        private Label _pathText;

        [MenuItem("Tools/Workspaces")]
        public static void ShowWindow()
        {
            var window = GetWindow<WorkspaceWindow>("Workspace", true, typeof(SceneView));
            window.titleContent = new GUIContent("Workspace", WorkspaceContent.WindowIcon);
            window.Show();
        }

        private void OnDisable()
        {
            _dragAndDropManipulator.target.RemoveManipulator(_dragAndDropManipulator);
            _graphManipulator.target.RemoveManipulator(_graphManipulator);
            _saveButton.clicked -= OnSaveClicked;
        }

        private void CreateGUI()
        {
            CacheElementTypes();

            _root.CloneTree(rootVisualElement);

            _toolbar = rootVisualElement.Q<Toolbar>("Toolbar");
            _debugText = _toolbar.Q<Label>("DebugText");
            _pathText = _toolbar.Q<Label>("PathText");
            _saveButton = _toolbar.Q<Button>("SaveButton");
            _saveButton.clicked += OnSaveClicked;

            _dragAndDropManipulator = new(rootVisualElement);
            _dragAndDropManipulator.OnAddAssetCallback(AddAssets);

            _graphManipulator = new(rootVisualElement);
            _graphManipulator.OnDrag(UpdateGraphOffset);
            _graphManipulator.OnScroll(UpdateGraphZoom);

            _graphRoot = rootVisualElement.Q<VisualElement>("GraphRoot");
            _graphRoot.WithManipulator(new ContextualMenuManipulator(BuildGraphContextMenu));

            UpdateGraphOffsetLabel();
            LoadWorkspace();
            foreach (var item in _activeWorkspace.Items)
                AddAsset(item);
        }

        private void BuildGraphContextMenu(ContextualMenuPopulateEvent obj)
        {
            obj.menu.AppendAction("Open Workspace File...", OpenWorkspaceFile);
            obj.menu.AppendAction("Snap Items To Grid", SnapAllItems);
        }

        private void SnapAllItems(DropdownMenuAction obj)
        {
            foreach (var element in _workspaceElements)
            {
                element.SnapToGrid();
            }
        }

        private void OpenWorkspaceFile(DropdownMenuAction obj)
        {
            Process.Start(_workspacePath);
        }

        private void UpdateGraphZoom(float delta)
        {
            _graphZoom += -delta*0.025f;
            _graphZoom = Math.Clamp(_graphZoom, 0, 1);
            OnGraphZoomChanged?.Invoke(_graphZoom);
            _graphRoot.style.scale = new StyleScale((1 + _graphZoom)*Vector2.one);
        }

        private void UpdateGraphOffset(Vector2 delta)
        {
            _graphOffset += delta;
            OnGraphOffsetChanged?.Invoke(_graphOffset);
            UpdateGraphOffsetLabel();

            _graphRoot.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Top, _graphOffset.x);
            _graphRoot.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Top, _graphOffset.y);
        }

        private void UpdateGraphOffsetLabel()
        {
            _debugText.text = _graphOffset.ToString();
        }

        private void OnSaveClicked()
        {
            SaveWorkspace();
        }

        private void LoadWorkspace()
        {
            if (!File.Exists(_workspacePath))
                return;

            var json = File.ReadAllText(_workspacePath);
            if (string.IsNullOrEmpty(json))
                return;

            _workspaceElements.Clear();
            _activeWorkspace = JsonConvert.DeserializeObject<Workspace>(json, _jsonSettings);
        }

        private readonly List<WorkspaceElement> _workspaceElements = new();

        private void SaveWorkspace()
        {
            _activeWorkspace.Items = new List<WorkspaceItemData>();
            foreach (var element in _workspaceElements)
            {
                var data = element.GetData();
                _activeWorkspace.Items.Add(data);
            }

            var json = JsonConvert.SerializeObject(_activeWorkspace, _jsonSettings);
            File.WriteAllText(_workspacePath, json);
        }

        private void AddAsset(WorkspaceItemData item)
        {
            var assetType = AssetDatabase.LoadAssetAtPath<Object>(item.AssetPath);
            if (assetType == null)
                return;

            var element = CreateWorkspaceElement(assetType);
            element.Setup(this, item);

            _graphRoot.Add(element);
            _workspaceElements.Add(element);
        }

        private void AddAssets(Vector2 pos, params string[] assetPaths)
        {
            Vector2 iterationOffset = new Vector2(12, 8);

            foreach (var assetPath in assetPaths)
            {
                AddAsset(new WorkspaceItemData(pos, assetPath));
                pos += iterationOffset;
            }
        }

        private readonly Dictionary<Type, Type> _elementTypeCache = new();

        private void CacheElementTypes()
        {
            _elementTypeCache.Clear();

            var elementTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(WorkspaceElement).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .ToArray();

            foreach (var type in elementTypes)
            {
                var assetType = type.BaseType?
                    .GetGenericArguments()
                    .First();

                if (assetType == null)
                    continue;

                _elementTypeCache[assetType] = type;
            }
        }

        private WorkspaceElement CreateWorkspaceElement<T>(T asset)
        {
            if (!_elementTypeCache.TryGetValue(asset.GetType(), out var elementType))
                elementType = typeof(DefaultWorkspaceElement);

            var element = (WorkspaceElement)Activator.CreateInstance(elementType);
            element.AddClass("graph-item").AddTo(_graphRoot);
            return element;
        }

        public void RemoveItem(WorkspaceElement element)
        {
            _workspaceElements.Remove(element);
            _activeWorkspace.Items.RemoveAll(x => x.AssetPath == element.AssetPath);
            element.RemoveFromHierarchy();
        }

        private WorkspaceElement _hovered;

        public void HoverStart(WorkspaceElement element)
        {
            _pathText.text = element.AssetPath;
            _hovered = element;
        }

        public void HoverStop(WorkspaceElement element)
        {
            if (_hovered != element)
                return;

            _pathText.text = string.Empty;
        }
    }
}