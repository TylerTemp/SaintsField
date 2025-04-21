#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Samples.Scripts.IssueAndTesting.ObjectPickerWorkaround.Editor
{


    public class SaintsObjectPickerUIToolkit : EditorWindow
    {
        public readonly struct ObjectBaseInfo
        {
            public readonly Object Target;
            public readonly Texture2D Icon;
            public readonly string Name;
            public readonly string TypeName;
            public readonly string Path;

            public ObjectBaseInfo(Object target, Texture2D icon, string name, string typeName, string path)
            {
                Target = target;
                Icon = icon;
                Name = name;
                TypeName = typeName;
                Path = path;
            }
        }

        public class ObjectInfo
        {
            public ObjectBaseInfo BaseInfo;
            public Texture2D Preview;
            public int PreviewLoadCount;

            public VisualElement ListItem;
            public VisualElement BlockItem;
            public Image BlockItemIcon;
            public Image BlockItemPreview;
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

        public void CreateGUI()
        {
            VisualTreeAsset visualTreeAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/ObjectPicker/ObjectPickerPanel.uxml");
            visualTreeAsset.CloneTree(rootVisualElement);

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

            rootVisualElement.schedule.Execute(() => _slider.value = _slider.value);
            rootVisualElement.schedule.Execute(UpdatePreview).Every(500);
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
            bool nowCtrlDown = evt.modifiers == EventModifiers.Control || evt.modifiers == EventModifiers.Command;

            SetCtrl(nowCtrlDown);

            if (!_ctrlDown)
            {
                return;
            }

            Vector3 delta = evt.delta;
            float deltaY = -delta.y / 30f;
            _slider.value = Mathf.Clamp(_slider.value + deltaY, 0, 1);
        }

        private void UpdatePreview()
        {
            foreach (ObjectInfo objectInfo in _sceneObjects.Concat(_assetsObjects).Where(each => each.PreviewLoadCount < 10).ToArray())
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

                    objectInfo.BlockItemIcon.RemoveFromHierarchy();
                    objectInfo.BlockItemPreview.image = preview;
                }
                else
                {
                    objectInfo.PreviewLoadCount++;
                }
            }
        }

        private void SwitchToAssets(bool on)
        {
            if (on == _currentOnAssets)
            {
                return;
            }

            _currentOnAssets = on;

            List<ObjectInfo> objInfoTargets = _currentOnAssets? _assetsObjects : _sceneObjects;
            // Debug.Log($"switch to assets {_currentOnAssets}: {objInfoTargets.Count}");
            _listViewContent.Clear();
            _blockViewContent.Clear();

            foreach (ObjectInfo objInfoTarget in objInfoTargets)
            {
                _listViewContent.Add(objInfoTarget.ListItem);
                _blockViewContent.Add(objInfoTarget.BlockItem);
            }
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
                    BlockItemIcon = blockItem.Q<Image>(name: "saints-field-object-picker-block-item-icon"),
                    BlockItemPreview = blockItem.Q<Image>(name: "saints-field-object-picker-block-item-preview"),
                };

                InitItem(objectInfo, listItem, blockItem);

                if (isAssets == _currentOnAssets)
                {
                    _listViewContent.Add(objectInfo.ListItem);
                    _blockViewContent.Add(objectInfo.BlockItem);
                }

                List<ObjectInfo> objInfoTargets = _currentOnAssets? _assetsObjects : _sceneObjects;
                objInfoTargets.Add(objectInfo);
            }
        }

        private static void InitItem(ObjectInfo objectInfo, VisualElement listItem, VisualElement blockItem)
        {
            // Debug.Log(objectInfo.BaseInfo.Icon);

            listItem.Q<Label>(name: "saints-field-object-picker-list-item-label").text = objectInfo.BaseInfo.Name;
            if(objectInfo.BaseInfo.Icon != null)
            {
                listItem.Q<Image>(name: "saints-field-object-picker-list-item-image").image = objectInfo.BaseInfo.Icon;
            }

            blockItem.Q<Label>(name: "saints-field-object-picker-block-item-name").text = objectInfo.BaseInfo.Name;
            if (objectInfo.BaseInfo.Icon != null)
            {
                objectInfo.BlockItemIcon.image = objectInfo.BaseInfo.Icon;
                objectInfo.BlockItemPreview.image = objectInfo.BaseInfo.Icon;
            }
        }

        private void UpdateBlockItemScale()
        {
            // float curValue = (_slider.value - 0.1f) / 0.9f;
            float curValue = Mathf.InverseLerp(0.1f, 0.9f, _slider.value);
            // Debug.Log(curValue);
            foreach (ObjectInfo objectInfo in _assetsObjects)
            {
                Button button = objectInfo.BlockItem.Q<Button>();
                button.style.width = 60 * (1 + curValue);
                button.style.height = 80 * (1 + curValue);

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

            Object[] resources = Resources.LoadAll("");
            // Debug.Log(resources.Length);
            pop.EnqueueAssetsObjects(resources.Select(each => new SaintsObjectPickerUIToolkit.ObjectBaseInfo(
                each, AssetPreview.GetMiniThumbnail(each), each.name, each.GetType().Name, AssetDatabase.GetAssetPath(each)
            )));
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
