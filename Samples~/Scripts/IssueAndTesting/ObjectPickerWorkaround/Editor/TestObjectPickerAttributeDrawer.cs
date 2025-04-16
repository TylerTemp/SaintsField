#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Samples.Scripts.IssueAndTesting.ObjectPickerWorkaround.Editor
{


    public class PopupContentExample : EditorWindow
    {
        // public override void OnOpen()
        // {
        //     Debug.Log("Popup opened: " + this);
        //     VisualTreeAsset visualTreeAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/ObjectPicker/ObjectPickerPanel.uxml");
        //     visualTreeAsset.CloneTree(editorWindow.rootVisualElement);
        //
        // }
        //
        // // public override VisualElement CreateGUI()
        // // {
        // // }
        //
        // public override void OnGUI(Rect rect)
        // {
        //     // Intentionally left empty
        // }
        //
        // public override void OnClose()
        // {
        //     Debug.Log("Popup closed: " + this);
        // }
        public void CreateGUI()
        {
            // Get a list of all sprites in the project
            // var allObjectGuids = AssetDatabase.FindAssets("t:Sprite");
            // var allObjects = new List<Sprite>();
            // foreach (var guid in allObjectGuids)
            // {
            //     allObjects.Add(AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)));
            // }
            VisualTreeAsset visualTreeAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/ObjectPicker/ObjectPickerPanel.uxml");
            visualTreeAsset.CloneTree(rootVisualElement);
            // rootVisualElement.Q<ToolbarSearchField>().style.width = StyleKeyword.Initial;
        }
    }

    // public class PopupExample : EditorWindow
    // {
    //     //Add menu item
    //     [MenuItem("Examples/Popup Example")]
    //     static void Init()
    //     {
    //         EditorWindow window = EditorWindow.CreateInstance<PopupExample>();
    //         window.Show();
    //     }
    //
    //     private void CreateGUI()
    //     {
    //         var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ui-toolkit-manual-code-examples/create-a-popup-window/PopupExample.uxml");
    //         visualTreeAsset.CloneTree(rootVisualElement);
    //
    //         var button = rootVisualElement.Q<Button>();
    //         button.clicked += () => PopupWindow.Show(button.worldBound, new PopupContentExample());
    //     }
    // }

    // public class PopupContentExample : PopupWindowContent
    // {
    //     public override void OnGUI(Rect rect)
    //     {
    //         throw new System.NotImplementedException();
    //     }
    //
    //     public override void OnOpen()
    //     {
    //         Debug.Log("Popup opened: " + this);
    //     }
    //
    //     public override VisualElement CreateGUI()
    //     {
    //         var visualTreeAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/ObjectPicker/ObjectPickerPanel.uxml");
    //         return visualTreeAsset.CloneTree();
    //     }
    //
    //     public override void OnClose()
    //     {
    //         Debug.Log("Popup closed: " + this);
    //     }
    // }

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
                var pop = EditorWindow.GetWindow<PopupContentExample>();
                pop.Close();
                pop = EditorWindow.GetWindow<PopupContentExample>();
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
            var pop = EditorWindow.GetWindow<PopupContentExample>();
            pop.Close();
            pop = EditorWindow.GetWindow<PopupContentExample>();
            pop.Show();

            VisualElement root = new VisualElement();
            root.Add(popContainer);
            root.Add(button);
            return root;
        }
    }
}
#endif
