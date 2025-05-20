#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Utils.SaintsObjectPickerWindow
{
    public class SaintsObjectPickerWindowUIToolkit : EditorWindow
    {
        public readonly UnityEvent<ObjectInfo> OnSelectedEvent = new UnityEvent<ObjectInfo>();
        public readonly UnityEvent OnDestroyEvent = new UnityEvent();

        public abstract class ObjectBasePayload
        {
        }

        private static float Scale
        {
            get => EditorPrefs.GetFloat("saintsField:objectPicker:scale", 0);
            set => EditorPrefs.SetFloat("saintsField:objectPicker:scale", value);
        }

        public readonly struct ObjectBaseInfo: IEquatable<ObjectBaseInfo>
        {
            public readonly Object Target;
            // public readonly Texture2D Icon;
            public readonly string Name;
            public readonly string TypeName;
            public readonly string Path;

            public readonly ObjectBasePayload Payload;

            public ObjectBaseInfo(Object target, string name, string typeName, string path, ObjectBasePayload payload = null)
            {
                Target = target;
                // Icon = icon;
                Name = name;
                TypeName = typeName;
                Path = path;
                Payload = payload;
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

        private ToolbarSearchField _toolbarSearchField;

        private ToolbarToggle _assetsToggle;
        private ToolbarToggle _sceneToggle;

        private VisualElement _pickerBody;
        private ListView _listView;
        private VisualElement _listViewContent;
        private ScrollView _blockView;
        private VisualElement _blockViewContent;

        private Image _loadingImage;

        public static readonly ObjectBaseInfo NoneObjectInfo = new ObjectBaseInfo(null, "None", "", "");

        public List<ObjectInfo> SceneObjects = new List<ObjectInfo>();
        public List<ObjectInfo> AssetsObjects = new List<ObjectInfo>();

        private VisualTreeAsset _listItemAsset;
        private VisualTreeAsset _blockItemAsset;

        private Slider _slider;
        private bool _isBlockView;

        private bool _ctrlDown;

        private ScrollView _listScrollView;
        private float _listScrollViewScrollSize;
        // private ScrollView _blockScrollView;
        private float _blockScrollViewScrollSize;

        private Image _selectedPreviewImage;
        private Label _selectedPreviewName;
        private Label _selectedPreviewType;
        private Label _selectedPreviewPath;

        public void CreateGUI()
        {
            VisualTreeAsset visualTreeAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/ObjectPicker/ObjectPickerPanel.uxml");
            visualTreeAsset.CloneTree(rootVisualElement);

#if UNITY_2023_2_OR_NEWER
            // don't lose focus
            rootVisualElement.RegisterCallback<PointerDownEvent>(evt =>
            {
                if(!_isBlockView)
                {
                    rootVisualElement.focusController.IgnoreEvent(evt);
                }

            }, TrickleDown.TrickleDown);
#endif
            _toolbarSearchField = rootVisualElement.Q<ToolbarSearchField>(name: "saints-field-object-picker-search");

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
                if (scaleValue >= 0.1f)
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
                        _toolbarSearchField.Focus();
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
            // _listView.RegisterCallback<KeyDownEvent>(OnKeyDownListView);
            // _listView.RegisterCallback<NavigationMoveEvent>(OnNavMoveListView);
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
            // _blockViewContent = _blockView.Q<VisualElement>(name: "unity-content-container");
            _blockViewContent = _blockView.contentContainer;
            Debug.Assert(_blockViewContent != null);

            _blockViewContent.style.flexDirection = FlexDirection.Row;
            _blockViewContent.style.flexWrap = Wrap.Wrap;
            _blockViewContent.style.overflow = Overflow.Visible;

            _blockView.RemoveFromHierarchy();

            _blockView.RegisterCallback<WheelEvent>(WheelEvent);

            _blockView.RegisterCallback<NavigationMoveEvent>(OnNavBlockView, TrickleDown.TrickleDown);
            // _blockViewContent.RegisterCallback<NavigationMoveEvent>(OnNavBlockView, TrickleDown.TrickleDown);
            _blockView.focusable = true;
            // _blockViewContent.focusable = true;

#if UNITY_2022_2_OR_NEWER
            _blockScrollViewScrollSize = _blockView.mouseWheelScrollSize;
#endif

            rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
            {
                // Debug.Log(evt.keyCode);
                if (evt.keyCode == KeyCode.Return)
                {
                    evt.StopPropagation();
                    DoClose();
                }

                bool ctrl = evt.keyCode is KeyCode.LeftControl or KeyCode.RightControl or KeyCode.LeftCommand or KeyCode.RightCommand;
                // Debug.Log($"ctrl={ctrl}");
                // ReSharper disable once InvertIf
                if (ctrl)
                {
                    _ctrlDown = true;
#if UNITY_2022_2_OR_NEWER
                    _listScrollView.mouseWheelScrollSize = 0;
                    _blockView.mouseWheelScrollSize = 0;
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
                    _blockView.mouseWheelScrollSize = _blockScrollViewScrollSize;
#endif
                }
            });

            _toolbarSearchField.Q<TextField>().RegisterCallback<NavigationMoveEvent>(evt =>
            {
                bool isUp = evt.direction == NavigationMoveEvent.Direction.Up;
                bool isDown = evt.direction == NavigationMoveEvent.Direction.Down;
                if (isUp || isDown)
                {
                    evt.StopPropagation();
                    if (_isBlockView)
                    {
                        if(isDown)
                        {
                            _blockView.Focus();
                        }
                    }
                    else
                    {
                        OnUpListView(isUp);
                    }
                }
            }, TrickleDown.TrickleDown);

            _toolbarSearchField.Q<TextField>().RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return)
                {
                    evt.StopPropagation();
                    DoClose();
                }
            }, TrickleDown.TrickleDown);

            _selectedPreviewImage = rootVisualElement.Q<Image>(name: "saints-field-object-picker-preview-image");
            _selectedPreviewName = rootVisualElement.Q<Label>(name: "saints-field-object-picker-preview-label-name");
            _selectedPreviewType = rootVisualElement.Q<Label>(name: "saints-field-object-picker-preview-label-type");
            _selectedPreviewPath = rootVisualElement.Q<Label>(name: "saints-field-object-picker-preview-label-path");

            _toolbarSearchField.RegisterValueChangedCallback(evt =>
            {
                string searchTarget = evt.newValue;
                string[] searchFragments = searchTarget.Split(' ');

                bool changed = false;

                foreach (ObjectInfo objectInfo in SceneObjects.Concat(AssetsObjects).ToArray())
                {
                    bool match = string.IsNullOrEmpty(searchTarget) || searchFragments.All(fragment =>
                        objectInfo.BaseInfo.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase));
                    if (objectInfo.Display != match)
                    {
                        objectInfo.Display = match;
                        changed = true;
                    }
                    // SetObjectInfoActive(objectInfo, match);
                }

                if (changed)
                {
                    RefreshDisplay();
                }
            });

            rootVisualElement.schedule.Execute(() =>
            {
                _slider.value = Scale;
                _toolbarSearchField.Focus();
            });
            rootVisualElement.schedule.Execute(() =>
            {
                UpdateIcon();
                UpdatePreview();
                UpdateDetailPreview();
            }).Every(500);
        }

        private void OnUpListView(bool isUp)
        {
            List<ObjectInfo> targetInfos = _currentOnAssets ? AssetsObjects : SceneObjects;
            List<int> accIndexes = new List<int>();
            int foundIndex = -1;

            for (int index = 0; index < targetInfos.Count; index++)
            {
                // int totalCount = targetInfos.Count;
                ObjectInfo curInfo = targetInfos[index];
                if (!curInfo.Display)
                {
                    continue;
                }

                accIndexes.Add(index);
                // ReSharper disable once InvertIf
                if (curInfo.BaseInfo.Equals(_currentActive?.BaseInfo))
                {
                    foundIndex = index;
                    break;
                }
            }

            if (foundIndex == -1)
            {
                if (accIndexes.Count != 0)
                {
                    // ReSharper disable once UseIndexFromEndExpression
                    int useIndex = isUp ? accIndexes[accIndexes.Count - 1] : accIndexes[0];
                    ObjectInfo useInfo = targetInfos[useIndex];
                    SetItemActive(useInfo.BaseInfo);
                    _listScrollView.ScrollTo(useInfo.ListItem);
                    OnSelectedEvent.Invoke(useInfo);
                }
                return;
            }

            int nextIndex = -1;
            if (isUp)
            {
                if (accIndexes.Count == 1)
                {
                    _toolbarSearchField.Focus();
                    return;
                }

                // ReSharper disable once UseIndexFromEndExpression
                nextIndex = accIndexes[accIndexes.Count - 2];
            }
            else
            {
                for (int index = foundIndex + 1; index < targetInfos.Count; index++)
                {
                    // int totalCount = targetInfos.Count;
                    ObjectInfo curInfo = targetInfos[index];
                    if (curInfo.Display)
                    {
                        nextIndex = index;
                        break;
                    }
                }
            }

            if(nextIndex < 0)
            {
                _toolbarSearchField.Focus();
                return;
            }

            if (nextIndex >= targetInfos.Count)
            {
                return;
            }

            ObjectInfo nextInfo = targetInfos[nextIndex];
            SetItemActive(nextInfo.BaseInfo);
            _listScrollView.ScrollTo(nextInfo.ListItem);
            OnSelectedEvent.Invoke(nextInfo);
        }

        private void OnNavBlockView(NavigationMoveEvent evt)
        {
            // OnMoveBlockView(evt.direction);
            NavigationMoveEvent.Direction direction = evt.direction;

            List<ObjectInfo> checkingTargets = _currentOnAssets ? AssetsObjects : SceneObjects;

            if (_currentActive is { Display: false })
            {
                // Debug.Log($"current is not display, try pick the first displayed item");
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (ObjectInfo objectInfo in checkingTargets)
                {
                    if (!objectInfo.Display)
                    {
                        continue;
                    }
                    SetItemActive(objectInfo.BaseInfo);
                    _blockView.ScrollTo(objectInfo.BlockItem);
                    OnSelectedEvent.Invoke(objectInfo);
                    return;
                }
                return;
            }

            // Debug.Log(direction);
            VisualElement currentBlockItem = _currentActive?.BlockItem;
            if (currentBlockItem == null)
            {
                return;
            }

            Rect currentBound = currentBlockItem.worldBound;
            if (double.IsNaN(currentBound.width) || double.IsNaN(currentBound.height))
            {
                return;
            }

            // lets shrink it to make an overlap box
            Rect shrinkBound = new Rect(currentBound);
            Vector2 offset;
            switch (direction)
            {
                case NavigationMoveEvent.Direction.Left:
                    offset = new Vector2(-currentBound.width, 0);
                    break;
                case NavigationMoveEvent.Direction.Up:
                    offset = new Vector2(0, -currentBound.height);
                    break;
                case NavigationMoveEvent.Direction.Right:
                    offset = new Vector2(currentBound.width, 0);
                    break;
                case NavigationMoveEvent.Direction.Down:
                    offset = new Vector2(0, currentBound.height);
                    break;
                default:
                    return;
            }

            shrinkBound.center += offset;
            shrinkBound.x += currentBound.width * 0.2f;
            shrinkBound.width *= 0.6f;
            shrinkBound.y += currentBound.height * 0.2f;
            shrinkBound.height *= 0.6f;

            // Debug.Log($"{currentBound} -> {shrinkBound}");


            foreach (ObjectInfo objectInfo in checkingTargets)
            {
                if (!objectInfo.Display)
                {
                    continue;
                }

                VisualElement checkingBlockItem = objectInfo.BlockItem;
                if (checkingBlockItem.panel == null)
                {
                    continue;
                }
                Rect checkingWorldBound = checkingBlockItem.worldBound;
                if (double.IsNaN(checkingWorldBound.height) || double.IsNaN(checkingWorldBound.width))
                {
                    continue;
                }

                // ReSharper disable once InvertIf
                if (shrinkBound.Overlaps(checkingWorldBound))
                {
                    SetItemActive(objectInfo.BaseInfo);
                    _blockView.ScrollTo(checkingBlockItem);
                    OnSelectedEvent.Invoke(objectInfo);
                    return;
                }
            }

            // no match and it's up
            if (direction == NavigationMoveEvent.Direction.Up)
            {
                _toolbarSearchField.Focus();
            }
            else
            {
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (ObjectInfo objectInfo in checkingTargets)
                {
                    if (!objectInfo.Display)
                    {
                        continue;
                    }
                    SetItemActive(objectInfo.BaseInfo);
                    _blockView.ScrollTo(objectInfo.BlockItem);
                    OnSelectedEvent.Invoke(objectInfo);
                    break;
                }
            }
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
                _blockView.mouseWheelScrollSize = 0;
            }
            else
            {
                _listScrollView.mouseWheelScrollSize = _listScrollViewScrollSize;
                _blockView.mouseWheelScrollSize = _blockScrollViewScrollSize;
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
            float deltaY = -delta.y / 60f;
            Scale = _slider.value = Mathf.Clamp(_slider.value + deltaY, 0, 1);
            // Scale = _slider.value = 1f;
        }

        private void UpdateIcon()
        {
            // AssetPreview.GetMiniThumbnail(each);
            Rect viewBound = _listScrollView.contentViewport.worldBound;
            if (double.IsNaN(viewBound.width) || double.IsNaN(viewBound.height))
            {
                return;
            }

            foreach (ObjectInfo objectInfo in SceneObjects.Concat(AssetsObjects).ToArray())
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
                    UpdateObjectIconOne(objectInfo);
                }

            }
        }

        private static void UpdateObjectIconOne(ObjectInfo objectInfo)
        {
            if (objectInfo.IconLoaded)
            {
                return;
            }

            objectInfo.IconLoaded = true;
            // Texture icon
            objectInfo.ListItemIcon.image
                = objectInfo.BlockItemIcon.image
                = objectInfo.Icon
                = AssetPreview.GetMiniThumbnail(objectInfo.BaseInfo.Target);

            // if(objectInfo.PreviewLoadCount < PreviewLoadMaxCount)
            // {
            //     objectInfo.BlockItemPreview.image = icon;
            // }
        }

        private const int PreviewLoadMaxCount = 10;

        private void UpdatePreview()
        {
            Rect viewBound = _blockView.contentViewport.worldBound;

            if (double.IsNaN(viewBound.width) || double.IsNaN(viewBound.height))
            {
                return;
            }

            foreach (ObjectInfo objectInfo in SceneObjects.Concat(AssetsObjects).ToArray())
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
                    UpdatePreviewOne(objectInfo);
                }
            }
        }

        private static Texture2D _prefabIcon;
        private static Texture2D _gameObjectIcon;

        private static void UpdatePreviewOne(ObjectInfo objectInfo)
        {
            if (objectInfo.BaseInfo.Target is null)
            {
                return;
            }

            UpdateObjectIconOne(objectInfo);

            if(objectInfo.PreviewLoadCount < PreviewLoadMaxCount)
            {
                Object target = objectInfo.BaseInfo.Target;

                bool isComp = false;
                GameObject targetGo = null;

                if (target is Component comp)
                {
                    targetGo = comp.gameObject;
                    isComp = true;
                }

                Texture2D preview = GetPreview(target);
                if (preview is null)
                {
                    objectInfo.PreviewLoadCount++;
                    // ReSharper disable once InvertIf
                    if (objectInfo.BlockItemPreview.image == null && isComp)
                    {
                        Texture2D previewIcon = AssetPreview.GetMiniThumbnail(targetGo);
                        objectInfo.BlockItemPreview.image = previewIcon;
                    }
                }
                else
                {
                    // Debug.Log($"{objectInfo.BaseInfo.Name}: {preview}");
                    objectInfo.Preview = preview;
                    objectInfo.PreviewLoadCount = int.MaxValue;

                    objectInfo.BlockItemIcon.style.display = DisplayStyle.Flex;
                    objectInfo.BlockItemPreview.image = preview;
                }
            }
        }

        private static Texture2D GetPreview(Object target)
        {
            Object previewTarget = target;
            if (target is Component comp)
            {
                previewTarget = comp.gameObject;
            }
            Texture2D preview = AssetPreview.GetAssetPreview(previewTarget);
            if (preview != null && preview.width > 1)
            {
                return preview;
            }

            return null;
        }

        public void RefreshDisplay()
        {
            DisplayStyle sliderDisplay = _currentOnAssets
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            if (_slider.style.display != sliderDisplay)
            {
                _slider.style.display = sliderDisplay;
            }

            List<ObjectInfo> objInfoTargets = _currentOnAssets? AssetsObjects : SceneObjects;
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
            else if (_currentOnAssets && !_isBlockView && _slider.value >= 0.1f)
            {
                _isBlockView = true;
                _listView.RemoveFromHierarchy();
                _pickerBody.Add(_blockView);
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

        private ObjectInfo _currentActive;

        public void SetItemActive(ObjectBaseInfo objectBaseInfo)
        {
            foreach (ObjectInfo objectInfo in SceneObjects.Concat(AssetsObjects).ToArray())
            {
                bool equal = objectInfo.BaseInfo.Equals(objectBaseInfo);
                SetObjectInfoActive(objectInfo, equal);

                // ReSharper disable once InvertIf
                if (equal)
                {
                    _currentActive = objectInfo;
                    // Debug.Log($"_currentActive={objectInfo.BaseInfo.Name}");
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

        public void SetInitDetailPanel(ObjectBaseInfo baseInfo)
        {
            _selectedPreviewName.text = baseInfo.Name;
            _selectedPreviewType.text = baseInfo.TypeName;
            _selectedPreviewPath.text = baseInfo.Path;

            // ReSharper disable once InvertIf
            if(!RuntimeUtil.IsNull(baseInfo.Target))
            {
                Texture2D image = GetPreview(baseInfo.Target);
                if (image is null)
                {
                    if (baseInfo.Target is Component comp)
                    {
                        _selectedPreviewImage.image = AssetPreview.GetMiniThumbnail(comp.gameObject);
                    }
                    else
                    {
                        _selectedPreviewImage.image = AssetPreview.GetMiniThumbnail(baseInfo.Target);
                    }
                }
                else
                {
                    _selectedPreviewImage.image = image;
                }
            }
        }

        private void UpdateDetailPreview()
        {
            if (_currentActive == null)
            {
                return;
            }

            if((_currentActive.Preview == null && _currentActive.PreviewLoadCount < PreviewLoadMaxCount) || !_currentActive.IconLoaded)
            {
                UpdatePreviewOne(_currentActive);
            }

            // Debug.Log($"{_currentActive.BaseInfo.Name}:{_currentActive.Preview}/{_currentActive.PreviewLoadCount}");

            if (_currentActive.Preview != null)
            {
                if(_selectedPreviewImage.image != _currentActive.Preview)
                {
                    _selectedPreviewImage.image = _currentActive.Preview;
                }
            }
            else if(_currentActive.IconLoaded)
            {
                if(_selectedPreviewImage.image != _currentActive.Icon)
                {
                    _selectedPreviewImage.image = _currentActive.Icon;
                }
            }
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
            string searchTarget = _toolbarSearchField.value;
            string[] searchFragments = searchTarget.Split(' ');

            foreach (ObjectBaseInfo objectBaseInfo in objectBaseInfos)
            {
                VisualElement listItem = _listItemAsset.CloneTree();
                VisualElement blockItem = _blockItemAsset.CloneTree();

                bool display = string.IsNullOrEmpty(searchTarget) || searchFragments.All(fragment =>
                    objectBaseInfo.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase));

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
                    Display = display,
                };

                InitItem(_slider.value, objectInfo, listItem, blockItem);

                if (display && isAssets == _currentOnAssets)
                {
                    _listViewContent.Add(objectInfo.ListItem);
                    _blockViewContent.Add(objectInfo.BlockItem);
                }

                List<ObjectInfo> objInfoTargets = isAssets? AssetsObjects : SceneObjects;
                objInfoTargets.Add(objectInfo);
            }
        }

        private double _lastClickTime = double.MaxValue;

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

            objectInfo.ListItemButton.clicked += () =>
            {
                if (ShouldCloseOnDoubleClick(objectInfo))
                {
                    DoClose();
                    return;
                }

#if !UNITY_2023_2_OR_NEWER
                _toolbarSearchField.Focus();
#endif
                // Debug.Log($"pick {objectInfo.BaseInfo.Name}");
                SetItemActive(objectInfo.BaseInfo);
                OnSelectedEvent.Invoke(objectInfo);
            };
            objectInfo.BlockItemButton.clicked += () =>
            {
                if (ShouldCloseOnDoubleClick(objectInfo))
                {
                    DoClose();
                    return;
                }
                SetItemActive(objectInfo.BaseInfo);
                OnSelectedEvent.Invoke(objectInfo);
            };
        }

        public readonly UnityEvent PleaseCloseMeEvent = new UnityEvent();

        // private bool _closed;
        private void DoClose()
        {
            // if (_closed)
            // {
            //     // Debug.Log("already closed");
            //     return;
            // }

            // Debug.Log("close!");
            // _closed = true;
            // Debug.Log("close this window");
            // UI Toolkit has this weird issue that can not be close, but can be close from outside...
            // Close();
            PleaseCloseMeEvent.Invoke();
        }

        // public void ResetClose()
        // {
        //     // _closed = false;
        // }

        private bool ShouldCloseOnDoubleClick(ObjectInfo objectInfo)
        {
            double curTime = EditorApplication.timeSinceStartup;

            // Debug.Log($"{curTime - _lastClickTime}/{_currentActive?.BaseInfo.Equals(objectInfo.BaseInfo)}");

            if (_lastClickTime > 0 && curTime - _lastClickTime < 0.2d && (_currentActive?.BaseInfo.Equals(objectInfo.BaseInfo) ?? false))
            {
                // double click to close
                // _lastClickTime = double.MinValue;
                return true;
            }

            _lastClickTime = curTime;
            return false;
        }

        private static void ApplyBlockItemScale(float sliderValue, Button button)
        {
            // float curValue = (_slider.value - 0.1f) / 0.9f;
            float scale = Mathf.Lerp(0.55f, 1.55f, Mathf.InverseLerp(0.1f, 1f, sliderValue));
            // float scale = Mathf.Lerp(0.55f, 1.55f, Mathf.InverseLerp(0.1f, 1f, 1f));
            button.style.width = 60 * scale;
            button.style.height = 80 * scale;
        }

        private void UpdateBlockItemScale()
        {
            // float curValue = (_slider.value - 0.1f) / 0.9f;
            // float curValue = Mathf.InverseLerp(0.1f, 0.9f, _slider.value);
            // Debug.Log(curValue);
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (ObjectInfo objectInfo in AssetsObjects)
            {
                ApplyBlockItemScale(_slider.value, objectInfo.BlockItemButton);
                // Button button = objectInfo.BlockItem.Q<Button>();
                // button.style.width = 60 * (1 + curValue);
                // button.style.height = 80 * (1 + curValue);
            }
        }

        public void OnDestroy()
        {
            OnDestroyEvent.Invoke();
        }

        public void OnLostFocus()
        {
            DoClose();
        }
    }

}
#endif
