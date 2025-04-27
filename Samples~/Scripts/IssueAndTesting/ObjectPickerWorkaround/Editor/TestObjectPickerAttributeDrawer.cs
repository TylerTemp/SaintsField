#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Samples.Scripts.IssueAndTesting.ObjectPickerWorkaround.Editor
{


    public class SaintsObjectPickerUIToolkit : EditorWindow
    {
        public readonly UnityEvent<ObjectInfo> OnSelectedEvent = new UnityEvent<ObjectInfo>();
        private static float _scale;

        public readonly struct ObjectBaseInfo: IEquatable<ObjectBaseInfo>
        {
            public readonly Object Target;
            // public readonly Texture2D Icon;
            public readonly string Name;
            public readonly string TypeName;
            public readonly string Path;

            public ObjectBaseInfo(Object target, string name, string typeName, string path)
            {
                Target = target;
                // Icon = icon;
                Name = name;
                TypeName = typeName;
                Path = path;
            }

            public bool Equals(ObjectBaseInfo other)
            {
                // ReSharper disable once Unity.BurstAccessingManagedMethod
                return ReferenceEquals(
                    // ReSharper disable once Unity.BurstLoadingManagedType
                    Target,
                    // ReSharper disable once Unity.BurstLoadingManagedType
                    other.Target);
            }

            public override bool Equals(object obj)
            {
                // ReSharper disable once Unity.BurstLoadingManagedType
                return obj is ObjectBaseInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                // ReSharper disable once Unity.BurstLoadingManagedType
                return Target != null
                    // ReSharper disable once Unity.BurstLoadingManagedType
                    ? Target.GetHashCode()
                    : 0;
            }
        }

        public class ObjectInfo
        {
            public ObjectBaseInfo BaseInfo;
            public Texture2D Preview;
            public Texture2D Icon;
            public int PreviewLoadCount;
            public bool IconLoaded;

            public VisualElement ListItem;
            public VisualElement BlockItem;

            public Button BlockItemButton;
            public Image BlockItemPreview;
            public VisualElement BlockItemLabelContainer;
            public Image BlockItemIcon;

            public Button ListItemButton;
            public Image ListItemIcon;

            public bool Display;
        }

        private bool _currentOnAssets = true;

        private ToolbarToggle _assetsToggle;
        private ToolbarToggle _sceneToggle;

        private VisualElement _pickerBody;
        private ListView _listView;
        private VisualElement _listViewContent;
        private ScrollView _blockView;
        private VisualElement _blockViewContent;

        private Image _loadingImage;

        public static readonly ObjectBaseInfo NoneObjectInfo = new ObjectBaseInfo(null, "None", "", "");

        private readonly List<ObjectInfo> _sceneObjects = new List<ObjectInfo>();
        private readonly List<ObjectInfo> _assetsObjects = new List<ObjectInfo>();

        private VisualTreeAsset _listItemAsset;
        private VisualTreeAsset _blockItemAsset;

        private Slider _slider;
        private bool _isBlockView;

        private bool _ctrlDown;

        private ScrollView _listScrollView;
        private float _listScrollViewScrollSize;
        private ScrollView _blockScrollView;
        private float _blockScrollViewScrollSize;

        private Image _selectedPreviewImage;
        private Label _selectedPreviewName;
        private Label _selectedPreviewType;
        private Label _selectedPreviewPath;

        public void CreateGUI()
        {
            VisualTreeAsset visualTreeAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/ObjectPicker/ObjectPickerPanel.uxml");
            visualTreeAsset.CloneTree(rootVisualElement);

            ToolbarSearchField toolbarSearchField = rootVisualElement.Q<ToolbarSearchField>(name: "saints-field-object-picker-search");

            _assetsToggle = rootVisualElement.Q<ToolbarToggle>(name: "saints-field-object-picker-toggle-assets");
            _sceneToggle = rootVisualElement.Q<ToolbarToggle>(name: "saints-field-object-picker-toggle-scene");

            _slider = rootVisualElement.Q<Slider>("saints-field-object-picker-slider");
            _slider.RegisterValueChangedCallback(e =>
            {
                if (!_currentOnAssets)  // scene view only has list type
                {
                    return;
                }

                float scaleValue = e.newValue;
                if (scaleValue > 0.1f)
                {
                    if (!_isBlockView)
                    {
                        _isBlockView = true;
                        _listView.RemoveFromHierarchy();
                        _pickerBody.Add(_blockView);
                    }

                    UpdateBlockItemScale();
                }
                else
                {
                    if (_isBlockView)
                    {
                        _isBlockView = false;
                        _blockView.RemoveFromHierarchy();
                        _pickerBody.Add(_listView);
                    }
                }
            });

            // so key event can be captured
            rootVisualElement.focusable = true;

            _assetsToggle.RegisterValueChangedCallback(e =>
            {
                if (e.newValue)
                {
                    _sceneToggle.SetValueWithoutNotify(false);
                    SwitchToAssets(true);
                }
                else
                {
                    _assetsToggle.SetValueWithoutNotify(true);
                }
            });
            _sceneToggle.RegisterValueChangedCallback(e =>
            {
                if (e.newValue)
                {
                    // _currentOnAssets = false;
                    _assetsToggle.SetValueWithoutNotify(false);
                    SwitchToAssets(false);
                }
                else
                {
                    _sceneToggle.SetValueWithoutNotify(true);
                }
            });

            _loadingImage = rootVisualElement.Q<Image>(name: "saints-field-object-picker-loading");
            _loadingImage.image = Util.LoadResource<Texture2D>("refresh.png");
            UIToolkitUtils.KeepRotate(_loadingImage);
            _loadingImage.schedule.Execute(() => UIToolkitUtils.TriggerRotate(_loadingImage)).StartingIn(300);

            _pickerBody = rootVisualElement.Q<VisualElement>("saints-field-object-picker-body");

            _listItemAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/ObjectPicker/ObjectPickerListItem.uxml");
            Debug.Assert(_listItemAsset != null);
            _listView = _pickerBody.Q<ListView>(name: "saints-field-object-picker-list");
            Debug.Assert(_listView != null);
            _listViewContent = _listView.Q<VisualElement>(name: "unity-content-container");
            Debug.Assert(_listViewContent != null);
            _listScrollView = _listView.Q<ScrollView>();

// #if UNITY_2021_3 || UNITY_2022_3 || UNITY_6000_OR_NEWER
#if UNITY_2022_2_OR_NEWER
            _listScrollViewScrollSize = _listScrollView.mouseWheelScrollSize;
#endif

            rootVisualElement.RegisterCallback<WheelEvent>(WheelEvent);
            _listScrollView.RegisterCallback<WheelEvent>(WheelEvent);

            _blockItemAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/ObjectPicker/ObjectPickerBlockItem.uxml");
            Debug.Assert(_blockItemAsset != null);
            // StyleSheet blockItemStyle = Util.LoadResource<StyleSheet>("UIToolkit/ObjectPicker/ObjectPickerBlockItemStyle.uss");
            // _blockItemAsset.styleSheets.Add(blockItemStyle);
            _blockView = _pickerBody.Q<ScrollView>(name: "saints-field-object-picker-block");
            Debug.Assert(_blockView != null);
            _blockViewContent = _blockView.Q<VisualElement>(name: "unity-content-container");
            Debug.Assert(_blockViewContent != null);

            _blockViewContent.style.flexDirection = FlexDirection.Row;
            _blockViewContent.style.flexWrap = Wrap.Wrap;

            _blockView.RemoveFromHierarchy();

            _blockScrollView = _blockView.Q<ScrollView>();
            _blockScrollView.RegisterCallback<WheelEvent>(WheelEvent);

#if UNITY_2022_2_OR_NEWER
            _blockScrollViewScrollSize = _blockScrollView.mouseWheelScrollSize;
#endif

            rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
            {
                bool ctrl = evt.keyCode is KeyCode.LeftControl or KeyCode.RightControl or KeyCode.LeftCommand or KeyCode.RightCommand;
                // Debug.Log($"ctrl={ctrl}");
                // ReSharper disable once InvertIf
                if (ctrl)
                {
                    _ctrlDown = true;
#if UNITY_2022_2_OR_NEWER
                    _listScrollView.mouseWheelScrollSize = 0;
                    _blockScrollView.mouseWheelScrollSize = 0;
#endif
                }
            });
            rootVisualElement.RegisterCallback<KeyUpEvent>(evt =>
            {
                bool ctrl = evt.keyCode is KeyCode.LeftControl or KeyCode.RightControl or KeyCode.LeftCommand or KeyCode.RightCommand;
                // ReSharper disable once InvertIf
                if (ctrl)
                {
                    _ctrlDown = false;
#if UNITY_2022_2_OR_NEWER
                    _listScrollView.mouseWheelScrollSize = _listScrollViewScrollSize;
                    _blockScrollView.mouseWheelScrollSize = _blockScrollViewScrollSize;
#endif
                }
            });

            _selectedPreviewImage = rootVisualElement.Q<Image>(name: "saints-field-object-picker-preview-image");
            _selectedPreviewName = rootVisualElement.Q<Label>(name: "saints-field-object-picker-preview-label-name");
            _selectedPreviewType = rootVisualElement.Q<Label>(name: "saints-field-object-picker-preview-label-type");
            _selectedPreviewPath = rootVisualElement.Q<Label>(name: "saints-field-object-picker-preview-label-path");

            rootVisualElement.schedule.Execute(() =>
            {
                _slider.value = _scale;
                toolbarSearchField.Focus();
            });
            rootVisualElement.schedule.Execute(() =>
            {
                UpdateIcon();
                UpdatePreview();
            }).Every(500);
        }

        public void SetLoadingImage(bool on)
        {
            Visibility visibleType = on ? Visibility.Visible : Visibility.Hidden;
            if (_loadingImage.style.visibility != visibleType)
            {
                _loadingImage.style.visibility = visibleType;
            }
        }

        private void SetCtrl(bool down)
        {
            if (_ctrlDown == down)
            {
                return;
            }

            _ctrlDown = down;
#if UNITY_2022_2_OR_NEWER
            if (down)
            {
                _listScrollView.mouseWheelScrollSize = 0;
                _blockScrollView.mouseWheelScrollSize = 0;
            }
            else
            {
                _listScrollView.mouseWheelScrollSize = _listScrollViewScrollSize;
                _blockScrollView.mouseWheelScrollSize = _blockScrollViewScrollSize;
            }
#endif
        }

        private void WheelEvent(WheelEvent evt)
        {
            bool nowCtrlDown = evt.modifiers is EventModifiers.Control or EventModifiers.Command;

            SetCtrl(nowCtrlDown);

            if (!_currentOnAssets)
            {
                return;
            }

            if (!_ctrlDown)
            {
                return;
            }

            Vector3 delta = evt.delta;
            float deltaY = -delta.y / 30f;
            _scale = _slider.value = Mathf.Clamp(_slider.value + deltaY, 0, 1);
        }

        private void UpdateIcon()
        {
            // AssetPreview.GetMiniThumbnail(each);
            Rect viewBound = _listScrollView.contentViewport.worldBound;
            if (double.IsNaN(viewBound.width) || double.IsNaN(viewBound.height))
            {
                return;
            }

            foreach (ObjectInfo objectInfo in _sceneObjects.Concat(_assetsObjects).ToArray())
            {
                if (objectInfo.BaseInfo.Target is null)
                {
                    continue;
                }
                Rect listBound = objectInfo.ListItem.worldBound;
                // Debug.Log($"{viewBound}/{blockBound}");
                if (double.IsNaN(listBound.width) || double.IsNaN(listBound.height))
                {
                    continue;
                }

                // ReSharper disable once InvertIf
                if (viewBound.Overlaps(listBound))
                {
                    // Debug.Log(objectInfo.BaseInfo.Name);
                    UpdateObjectIcon(objectInfo);
                }

            }
        }

        private static void UpdateObjectIcon(ObjectInfo objectInfo)
        {
            if (objectInfo.IconLoaded)
            {
                return;
            }

            objectInfo.IconLoaded = true;
            Texture icon
                = objectInfo.ListItemIcon.image
                = objectInfo.BlockItemIcon.image
                = objectInfo.Icon
                = AssetPreview.GetMiniThumbnail(objectInfo.BaseInfo.Target);

            if(objectInfo.PreviewLoadCount < PreviewLoadMaxCount)
            {
                objectInfo.BlockItemPreview.image = icon;
            }
        }

        private const int PreviewLoadMaxCount = 10;

        private void UpdatePreview()
        {
            Rect viewBound = _blockScrollView.contentViewport.worldBound;

            if (double.IsNaN(viewBound.width) || double.IsNaN(viewBound.height))
            {
                return;
            }

            foreach (ObjectInfo objectInfo in _sceneObjects.Concat(_assetsObjects).ToArray())
            {
                if (objectInfo.BaseInfo.Target is null)
                {
                    continue;
                }

                Rect blockBound = objectInfo.BlockItem.worldBound;
                // Debug.Log($"{viewBound}/{blockBound}");
                if (double.IsNaN(blockBound.width) || double.IsNaN(blockBound.height))
                {
                    continue;
                }

                // ReSharper disable once InvertIf
                if (viewBound.Overlaps(blockBound))
                {
                    // Debug.Log(objectInfo.BaseInfo.Name);

                    UpdateObjectIcon(objectInfo);

                    if(objectInfo.PreviewLoadCount < PreviewLoadMaxCount)
                    {
                        Object target = objectInfo.BaseInfo.Target;

                        if (target is Component comp)
                        {
                            target = comp.gameObject;
                        }

                        Texture2D preview = AssetPreview.GetAssetPreview(target);
                        if (preview != null && preview.width > 1)
                        {
                            objectInfo.Preview = preview;
                            objectInfo.PreviewLoadCount = int.MaxValue;

                            objectInfo.BlockItemIcon.style.display = DisplayStyle.Flex;
                            objectInfo.BlockItemPreview.image = preview;
                        }
                        else
                        {
                            objectInfo.PreviewLoadCount++;
                        }
                    }
                }

            }
        }

        private void RefreshDisplay()
        {
            DisplayStyle sliderDisplay = _currentOnAssets
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            if (_slider.style.display != sliderDisplay)
            {
                _slider.style.display = sliderDisplay;
            }

            _currentOnAssets = _currentOnAssets;

            List<ObjectInfo> objInfoTargets = _currentOnAssets? _assetsObjects : _sceneObjects;
            // Debug.Log($"switch to assets {_currentOnAssets}: {objInfoTargets.Count}");
            _listViewContent.Clear();
            _blockViewContent.Clear();

            foreach (ObjectInfo objInfoTarget in objInfoTargets)
            {
                if(objInfoTarget.Display)
                {
                    _listViewContent.Add(objInfoTarget.ListItem);
                    _blockViewContent.Add(objInfoTarget.BlockItem);
                }
            }

            if (!_currentOnAssets && _isBlockView)
            {
                _isBlockView = false;
                _blockView.RemoveFromHierarchy();
                _pickerBody.Add(_listView);
            }
        }

        private void SwitchToAssets(bool on)
        {
            if (on == _currentOnAssets)
            {
                return;
            }

            _currentOnAssets = on;

            RefreshDisplay();
        }

        public void DisableAssets()
        {
            InternalDisableAssets(true);
        }

        public void DisableScene()
        {
            InternalDisableAssets(false);
        }

        private void InternalDisableAssets(bool disable)
        {
            _assetsToggle.SetEnabled(!disable);
            _sceneToggle.SetEnabled(disable);

            if (_currentOnAssets && disable)
            {
                // _assetsToggle.SetValueWithoutNotify(false);
                _sceneToggle.value = true;
                // _currentOnAssets = false;
            }
            else
            {
                // _sceneToggle.SetValueWithoutNotify(false);
                _assetsToggle.value = true;
                _currentOnAssets = true;
            }
        }

        public void EnqueueSceneObjects(IEnumerable<ObjectBaseInfo> objectBaseInfos)
        {
            EnqueueToAssetsObjects(false, objectBaseInfos);
        }

        public void EnqueueAssetsObjects(IEnumerable<ObjectBaseInfo> objectBaseInfos)
        {
            EnqueueToAssetsObjects(true, objectBaseInfos);
        }

        public void SetItemActive(ObjectBaseInfo objectBaseInfo)
        {
            foreach (ObjectInfo objectInfo in _sceneObjects.Concat(_assetsObjects).ToArray())
            {
                bool equal = objectInfo.BaseInfo.Equals(objectBaseInfo);
                SetObjectInfoActive(objectInfo, equal);

                if (equal)
                {
                    SetDetailPanel(objectInfo);
                }
            }
        }

        private void SetDetailPanel(ObjectInfo objectInfo)
        {
            _selectedPreviewImage.image = objectInfo.Preview ?? objectInfo.Icon;
            _selectedPreviewName.text = objectInfo.BaseInfo.Name;
            _selectedPreviewType.text = objectInfo.BaseInfo.TypeName;
            _selectedPreviewPath.text = objectInfo.BaseInfo.Path;
        }

        private static void SetObjectInfoActive(ObjectInfo objectInfo, bool active)
        {
            const string activeClass = "pressed";

            VisualElement blockItemLabelContainer = objectInfo.BlockItemLabelContainer;
            Button listItemButton = objectInfo.ListItemButton;

            if (active)
            {
                if (!blockItemLabelContainer.ClassListContains(activeClass))
                {
                    blockItemLabelContainer.AddToClassList(activeClass);
                }

                if (!listItemButton.ClassListContains(activeClass))
                {
                    listItemButton.AddToClassList(activeClass);
                }
            }
            else
            {
                if (blockItemLabelContainer.ClassListContains(activeClass))
                {
                    blockItemLabelContainer.RemoveFromClassList(activeClass);
                }
                if (listItemButton.ClassListContains(activeClass))
                {
                    listItemButton.RemoveFromClassList(activeClass);
                }
            }
        }

        private void EnqueueToAssetsObjects(bool isAssets, IEnumerable<ObjectBaseInfo> objectBaseInfos)
        {
            foreach (ObjectBaseInfo objectBaseInfo in objectBaseInfos)
            {
                VisualElement listItem = _listItemAsset.CloneTree();
                VisualElement blockItem = _blockItemAsset.CloneTree();
                ObjectInfo objectInfo = new ObjectInfo
                {
                    BaseInfo = objectBaseInfo,
                    Preview = null,
                    PreviewLoadCount = 0,

                    ListItem = listItem,
                    BlockItem = blockItem,
                    BlockItemButton = blockItem.Q<Button>(),
                    BlockItemIcon = blockItem.Q<Image>(name: "saints-field-object-picker-block-item-icon"),
                    BlockItemPreview = blockItem.Q<Image>(name: "saints-field-object-picker-block-item-preview"),
                    Display = true,
                };

                InitItem(_slider.value, objectInfo, listItem, blockItem);

                if (isAssets == _currentOnAssets)
                {
                    _listViewContent.Add(objectInfo.ListItem);
                    _blockViewContent.Add(objectInfo.BlockItem);
                }

                List<ObjectInfo> objInfoTargets = isAssets? _assetsObjects : _sceneObjects;
                objInfoTargets.Add(objectInfo);
            }
        }

        private void InitItem(float sliderValue, ObjectInfo objectInfo, VisualElement listItem, VisualElement blockItem)
        {
            // Debug.Log(objectInfo.BaseInfo.Icon);

            listItem.Q<Label>(name: "saints-field-object-picker-list-item-label").text = objectInfo.BaseInfo.Name;
            objectInfo.ListItemButton = listItem.Q<Button>(name: "saints-field-object-picker-list-item-button");
            objectInfo.ListItemIcon = listItem.Q<Image>(name: "saints-field-object-picker-list-item-image");

            objectInfo.BlockItemLabelContainer =
                blockItem.Q<VisualElement>(name: "saints-field-object-picker-block-item-label-container");
            blockItem.Q<Label>(name: "saints-field-object-picker-block-item-name").text = objectInfo.BaseInfo.Name;

            ApplyBlockItemScale(sliderValue, objectInfo.BlockItemButton);

            objectInfo.ListItemButton.clicked += OnClick;
            objectInfo.BlockItemButton.clicked += OnClick;
            return;

            void OnClick()
            {
                SetItemActive(objectInfo.BaseInfo);
                OnSelectedEvent.Invoke(objectInfo);
            }
        }

        private static void ApplyBlockItemScale(float sliderValue, Button button)
        {
            // float curValue = (_slider.value - 0.1f) / 0.9f;
            float curValue = Mathf.InverseLerp(0.1f, 0.9f, sliderValue);
            button.style.width = 60 * (1 + curValue);
            button.style.height = 80 * (1 + curValue);
        }

        private void UpdateBlockItemScale()
        {
            // float curValue = (_slider.value - 0.1f) / 0.9f;
            // float curValue = Mathf.InverseLerp(0.1f, 0.9f, _slider.value);
            // Debug.Log(curValue);
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (ObjectInfo objectInfo in _assetsObjects)
            {
                ApplyBlockItemScale(_slider.value, objectInfo.BlockItemButton);
                // Button button = objectInfo.BlockItem.Q<Button>();
                // button.style.width = 60 * (1 + curValue);
                // button.style.height = 80 * (1 + curValue);
            }
        }
    }

    [CustomPropertyDrawer(typeof(TestObjectPickerAttribute), true)]
    public class TestObjectPickerAttributeDrawer : SaintsPropertyDrawer
    {

        private void DebugOpenWindow()
        {
            SaintsObjectPickerUIToolkit pop = EditorWindow.GetWindow<SaintsObjectPickerUIToolkit>();
            pop.Close();
            pop = EditorWindow.GetWindow<SaintsObjectPickerUIToolkit>();
            pop.Show();

            pop.EnqueueSceneObjects(new[]{SaintsObjectPickerUIToolkit.NoneObjectInfo});

            Object[] resources = Resources.LoadAll("");
            // Debug.Log(resources.Length);

            pop.EnqueueAssetsObjects(new[]{SaintsObjectPickerUIToolkit.NoneObjectInfo});
            pop.EnqueueAssetsObjects(resources.Select(each => new SaintsObjectPickerUIToolkit.ObjectBaseInfo(
                each, each.name, each.GetType().Name, AssetDatabase.GetAssetPath(each)
            )));

            pop.SetItemActive(SaintsObjectPickerUIToolkit.NoneObjectInfo);

            // pop.SetLoadingImage(false);
        }


        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {

            Button button = new Button(() =>
            {
                // var visualTreeAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/ObjectPicker/ObjectPickerPanel.uxml");
                // visualTreeAsset.CloneTree(popContainer);

                // PopupWindow.Show(container.worldBound, new PopupContentExample());

                // PopupWindow.Show(container.worldBound, new PopupContentExample());
                DebugOpenWindow();

                // UnityEngine.UIElements.PopupWindow popup = new UnityEngine.UIElements.PopupWindow();
                // visualTreeAsset.CloneTree(popup);
                // popup.Show(container.worldBound);
                // EditorWindow.ShowPopup();
                // popup.worldBound = container.worldBound;
                // popup.
            })
            {
                text = "Test",
            };

            // var pop = ScriptableObject.CreateInstance<PopupContentExample>();
            DebugOpenWindow();

            return button;
        }
    }
}
#endif
