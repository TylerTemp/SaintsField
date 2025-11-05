using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaintsField.Editor.Linq;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.GuidDrawer
{
    public class GuidStringElement: BindableElement, INotifyValueChanged<string>
    {
        private static VisualTreeAsset _containerTree;

        private readonly IReadOnlyList<HexInputLengthElement> _hexInputs;

        private static Texture2D _warningIcon;
        private readonly StyleBackground _dropdownIcon;
        private readonly Button _dropdownButton;

        private VisualElement _dropdownBoundElement;

        public GuidStringElement()
        {
            _containerTree ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/Guid.uxml");
            TemplateContainer clone = _containerTree.CloneTree();

            _hexInputs = clone.Query<HexInputLengthElement>().ToList();
            _hexInputs[0].length = 8;
            _hexInputs[4].length = 12;

            foreach (HexInputLengthElement hexInputLengthElement in _hexInputs)
            {
                hexInputLengthElement.RegisterValueChangedCallback(WatchEachInput);
            }

            _dropdownButton = clone.Q<Button>();
            _dropdownIcon = _dropdownButton.style.backgroundImage;
            _dropdownButton.clicked += DropdownClicked;

            Add(clone);

            _warningIcon ??= (Texture2D)EditorGUIUtility.IconContent("console.warnicon").image;
        }

        public void BindDropdownElement(VisualElement target) => _dropdownBoundElement = target;

        private readonly List<(string, Guid, bool, bool)> _extraOptions = new List<(string, Guid, bool, bool)>();

        private static (bool, Guid) ParseUnityGuid(string cleaned)
        {
            if (cleaned.Length != 32)
            {
                return (false, Guid.Empty);
            }
            string hyphenated = $"{cleaned[..8]}-{cleaned.Substring(8,4)}-{cleaned.Substring(12,4)}-{cleaned.Substring(16,4)}-{cleaned.Substring(20,12)}";

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (Guid.TryParse(hyphenated, out Guid guid))
            {
                return (true, guid);
            }

            return (false, Guid.Empty);
            // return Guid.Parse(hyphenated);
        }

        public void BindProp(SerializedProperty property)
        {
            Object serTarget = property.serializedObject.targetObject;

            switch (serTarget)
            {
                case ScriptableObject so:
                {
                    TryAddGuidFromObject(so);
                }
                    break;
                case Component comp:
                {
                    TryAddPrefabGameObject(comp.gameObject);
                }
                    break;
                case GameObject go:
                {
                    TryAddPrefabGameObject(go);
                }
                    break;
            }

            MonoScript monoScript = SaintsEditor.GetMonoScript(serTarget);
            if (monoScript != null)
            {
                TryAddGuidFromObject(monoScript);
            }
        }

        private void TryAddPrefabGameObject(GameObject gameObject)
        {
            foreach (string assetPath in GetAllPrefabAssetPaths(gameObject))
            {
                TryAddByAssetPath(assetPath);
            }
        }

        private static List<string> GetAllPrefabAssetPaths(GameObject go)
        {
            List<string> paths = new List<string>();
            if (go == null) return paths;

            void AddUnique(string p)
            {
                if (!string.IsNullOrEmpty(p) && !paths.Contains(p))
                    paths.Add(p);
            }

            // Prefab stage (prefab mode / isolated view)
            PrefabStage stage = PrefabStageUtility.GetPrefabStage(go);
            if (stage != null)
                AddUnique(stage.assetPath);

            // Nearest instance root asset path (works for scene instances)
            AddUnique(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go));

            // Try the nearest instance root explicitly
            GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(go);
            if (root != null)
                AddUnique(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root));

            // If object itself is part of a prefab asset (project view)
            AddUnique(AssetDatabase.GetAssetPath(go));

            // Walk corresponding source objects to collect nested prefab asset links
            Object current = PrefabUtility.GetCorrespondingObjectFromSource(go);
            int safety = 0;
            while (current != null && safety++ < 64)
            {
                AddUnique(AssetDatabase.GetAssetPath(current));
                current = PrefabUtility.GetCorrespondingObjectFromSource(current);
            }

            return paths;
        }

        private void TryAddGuidFromObject(Object uObject)
        {
            string uPath = AssetDatabase.GetAssetPath(uObject);
            if(!string.IsNullOrEmpty(uPath))
            {
                TryAddByAssetPath(uPath);
            }
        }

        private void TryAddByAssetPath(string uPath)
        {
            string uGuid = AssetDatabase.AssetPathToGUID(uPath);
            (bool guidOk, Guid guidValue) = ParseUnityGuid(uGuid);
            if (guidOk)
            {
                string fileName = Path.GetFileName(uPath);
                _extraOptions.Add((fileName, guidValue, false, false));
            }
        }

        private void DropdownClicked()
        {
            List<(string, Guid, bool, bool)> options = new List<(string, Guid, bool, bool)>
            {
                ("New", Guid.NewGuid(), false, false),
                ("Empty", Guid.Empty, false, false),
            };
            if (_extraOptions.Count > 0)
            {
                options.Add(("", Guid.Empty, false, true));
                options.AddRange(_extraOptions);
            }

            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
            // int selectedIndex = 0;
            // Debug.Log($"metaInfo.SelectedIndex={metaInfo.SelectedIndex}");
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int index = 0; index < options.Count; index++)
            {
                // int curIndex = index;
                (string curName, Guid curItem, bool disabled, bool isSeparator) =
                    options[index];
                bool isChecked = value == curItem.ToString();

                if (isSeparator)
                {
                    genericDropdownMenu.AddSeparator("");
                }
                else if (disabled)
                {
                    genericDropdownMenu.AddDisabledItem(curName, isChecked);
                }
                else
                {
                    genericDropdownMenu.AddItem(curName, isChecked, () => value = curItem.ToString());
                }
            }

            genericDropdownMenu.DropDown((_dropdownBoundElement ?? this).worldBound, _dropdownButton, true);
        }

        private void WatchEachInput(ChangeEvent<string> _)
        {
            string merged = string.Join("-", _hexInputs.Select(each => each.value));
            if (Guid.TryParse(merged, out Guid _))
            {
                value = merged;
            }
        }

        private string _cachedValue;

        public void SetValueWithoutNotify(string newValue)
        {
            _cachedValue = newValue;
            foreach ((string part, int index) in newValue.Split('-').Take(5).WithIndex())
            {
                HexInputLengthElement input = _hexInputs[index];
                if (input.value != part)
                {
                    input.SetValueWithoutNotify(part);
                }
            }

            bool isValidGuid = Guid.TryParse(newValue, out Guid _);
            _dropdownButton.style.backgroundImage = isValidGuid ? _dropdownIcon : _warningIcon;
            _dropdownButton.tooltip = isValidGuid ? "" : $"Invalid GUID {newValue}";
        }

        public string value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                string previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<string> evt = ChangeEvent<string>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }

    public class GuidStringField : BaseField<string>
    {
        private readonly GuidStringElement _guidStringElement;

        public GuidStringField(string label, GuidStringElement visualInput) : base(label, visualInput)
        {
            _guidStringElement = visualInput;
            visualInput.BindDropdownElement(this);
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            _guidStringElement.SetValueWithoutNotify(newValue);
        }

        public override string value
        {
            get => _guidStringElement.value;
            set => _guidStringElement.value = value;
        }
    }
}
