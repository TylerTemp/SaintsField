#if UNITY_EDITOR
using UnityEditor;
#if UNITY_2021_3_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue23
{
    [CustomPropertyDrawer(typeof(ImGuiFallback.Container<,>), true)]
    public sealed class ContainerDrawer : PropertyDrawer
    {
#if UNITY_2021_3_OR_NEWER
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var entries = property.FindPropertyRelative("Entries");
            var listView = new ListView
            {
                headerTitle = property.displayName,
                showBorder = true,
                showFoldoutHeader = true,
                showAddRemoveFooter = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showBoundCollectionSize = true,
                makeItem = MakeItem,
                bindItem = BindItem
            };
            listView.BindProperty(entries);

            return listView;

            VisualElement MakeItem()
            {
                var element = new VisualElement();

                var name = new TextField("Name") { name = "Name" };
                var obj = new PropertyField { label = "Object", name = "Object" };

                name.AddToClassList(BaseField<object>.alignedFieldUssClassName);

                element.Add(name);
                element.Add(obj);

                return element;
            }

            void BindItem(VisualElement element, int index)
            {
                var name = element.Q<TextField>("Name");
                var obj = element.Q<PropertyField>("Object");

                var item = entries.GetArrayElementAtIndex(index);
                name.BindProperty(item.FindPropertyRelative("Name"));
                obj.BindProperty(item.FindPropertyRelative("Object"));
            }
        }
#endif
    }
}
#endif
