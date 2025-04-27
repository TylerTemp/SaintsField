#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Samples.Scripts.IssueAndTesting.ObjectPickerWorkaround.Editor
{

    [CustomPropertyDrawer(typeof(TestObjectPickerAttribute), true)]
    public class TestObjectPickerAttributeDrawer : SaintsPropertyDrawer
    {

        private void DebugOpenWindow()
        {
            // SaintsObjectPickerWindowUIToolkit pop = EditorWindow.GetWindow<SaintsObjectPickerWindowUIToolkit>();
            // pop.Close();
            // pop = EditorWindow.GetWindow<SaintsObjectPickerWindowUIToolkit>();
            // pop.Show();

            SaintsObjectPickerWindowUIToolkit pop = ScriptableObject.CreateInstance<SaintsObjectPickerWindowUIToolkit>();
            pop.ShowAuxWindow();

            pop.EnqueueSceneObjects(new[]{SaintsObjectPickerWindowUIToolkit.NoneObjectInfo});
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                pop.EnqueueSceneObjects(new[]
                {
                    new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                        root, root.name, "GameObject", ""
                    ),
                });
            }

            Object[] resources = Resources.LoadAll("");
            // Debug.Log(resources.Length);

            pop.EnqueueAssetsObjects(new[]{SaintsObjectPickerWindowUIToolkit.NoneObjectInfo});
            pop.EnqueueAssetsObjects(resources.Select(each => new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                each, each.name, each.GetType().Name, AssetDatabase.GetAssetPath(each)
            )));

            pop.SetItemActive(SaintsObjectPickerWindowUIToolkit.NoneObjectInfo);

            pop.OnSelectedEvent.AddListener(result => Debug.Log($"Selected: {result.BaseInfo.Name}"));

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
            // DebugOpenWindow();

            return button;
        }
    }
}
#endif
