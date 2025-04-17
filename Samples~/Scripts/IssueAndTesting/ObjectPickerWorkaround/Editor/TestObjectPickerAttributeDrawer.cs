#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
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
        }

        private bool _currentOnAssets = true;

        private ToolbarToggle _assetsToggle;
        private ToolbarToggle _sceneToggle;

        private VisualElement _pickerBody;
        private ListView _listView;
        private ScrollView _blockView;

        private Image _loadingImage;

        // private List<ObjectInfo> _scene

        public void CreateGUI()
        {
            VisualTreeAsset visualTreeAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/ObjectPicker/ObjectPickerPanel.uxml");
            visualTreeAsset.CloneTree(rootVisualElement);

            _assetsToggle = rootVisualElement.Q<ToolbarToggle>(name: "saints-field-object-picker-toggle-assets");
            _sceneToggle = rootVisualElement.Q<ToolbarToggle>(name: "saints-field-object-picker-toggle-scene");

            _assetsToggle.RegisterValueChangedCallback(e =>
            {
                if (e.newValue)
                {
                    _currentOnAssets = true;
                    _sceneToggle.SetValueWithoutNotify(false);
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
                    _currentOnAssets = false;
                    _assetsToggle.SetValueWithoutNotify(false);
                }
                else
                {
                    _sceneToggle.SetValueWithoutNotify(true);
                }
            });

            _loadingImage = rootVisualElement.Q<Image>(name: "saints-field-object-picker-loading");
            _loadingImage.image = Util.LoadResource<Texture2D>("refresh.png");
            Debug.Log(_loadingImage.image);
            UIToolkitUtils.KeepRotate(_loadingImage);
            // _loadingImage.RegisterCallback<AttachToPanelEvent>(_ =>  UIToolkitUtils.TriggerRotate(_loadingImage));
            _loadingImage.schedule.Execute(() => UIToolkitUtils.TriggerRotate(_loadingImage)).StartingIn(300);
            // UIToolkitUtils.TriggerRotate(_loadingImage);
            // buttonRotator.schedule.Execute(() => UIToolkitUtils.TriggerRotate(buttonRotator));

            _pickerBody = rootVisualElement.Q<VisualElement>("saints-field-object-picker-body");

            VisualTreeAsset listItemAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/ObjectPicker/ObjectPickerListItem.uxml");
            Debug.Assert(listItemAsset != null);
            _listView = _pickerBody.Q<ListView>(name: "saints-field-object-picker-list");
            Debug.Assert(_listView != null);
            VisualElement listViewContainer = _listView.Q<VisualElement>(name: "unity-content-container");
            Debug.Assert(listViewContainer != null);

            for (int i = 0; i < 15; i++)
            {
                VisualElement listItem = listItemAsset.CloneTree();
                listViewContainer.Add(listItem);
                // listItem.style.width = new StyleLength(Length.Percent(50));
                listItem.Q<Label>(name: "saints-field-object-picker-list-item-label").text = $"Item {i}";
            }

            // _listView.RemoveFromHierarchy();

            VisualTreeAsset blockItemAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/ObjectPicker/ObjectPickerBlockItem.uxml");
            Debug.Assert(blockItemAsset != null);
            _blockView = _pickerBody.Q<ScrollView>(name: "saints-field-object-picker-block");
            Debug.Assert(_blockView != null);
            VisualElement blockViewContainer = _blockView.Q<VisualElement>(name: "unity-content-container");
            Debug.Assert(blockViewContainer != null);

            blockViewContainer.style.flexDirection = FlexDirection.Row;
            blockViewContainer.style.flexWrap = Wrap.Wrap;

            _blockView.RemoveFromHierarchy();

            for (int i = 0; i < 15; i++)
            {
                VisualElement blockItem = blockItemAsset.CloneTree();
                // blockItem.style.width = 60;
                // blockItem.style.height = 60;
                blockViewContainer.Add(blockItem);
                blockItem.Q<Label>(name: "saints-field-object-picker-block-item-name").text = $"Item {i}";
            }

            // rootVisualElement.schedule.Execute(() =>
            // {
            //     DisableAssets();
            // }).StartingIn(1000);
            //
            // rootVisualElement.schedule.Execute(() =>
            // {
            //     DisableScene();
            // }).StartingIn(2000);
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
                _assetsToggle.SetValueWithoutNotify(false);
                _sceneToggle.SetValueWithoutNotify(true);
                _currentOnAssets = false;
            }
            else
            {
                _assetsToggle.SetValueWithoutNotify(true);
                _sceneToggle.SetValueWithoutNotify(false);
                _currentOnAssets = true;
            }
        }
    }

    [CustomPropertyDrawer(typeof(TestObjectPickerAttribute), true)]
    public class TestObjectPickerAttributeDrawer : SaintsPropertyDrawer
    {


        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement popContainer = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                },
            };

            Button button = new Button(() =>
            {
                // var visualTreeAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/ObjectPicker/ObjectPickerPanel.uxml");
                // visualTreeAsset.CloneTree(popContainer);

                // PopupWindow.Show(container.worldBound, new PopupContentExample());

                // PopupWindow.Show(container.worldBound, new PopupContentExample());
                var pop = EditorWindow.GetWindow<SaintsObjectPickerUIToolkit>();
                pop.Close();
                pop = EditorWindow.GetWindow<SaintsObjectPickerUIToolkit>();
                pop.Show();

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
            var pop = EditorWindow.GetWindow<SaintsObjectPickerUIToolkit>();
            pop.Close();
            pop = EditorWindow.GetWindow<SaintsObjectPickerUIToolkit>();
            pop.Show();

            VisualElement root = new VisualElement();
            root.Add(popContainer);
            root.Add(button);
            return root;
        }
    }
}
#endif
