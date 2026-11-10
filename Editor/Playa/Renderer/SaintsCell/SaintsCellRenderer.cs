using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.SaintsCell
{
    public class SaintsCellRenderer: AbsRenderer, IMakeRenderer
    {
        private readonly SerializedObject _serializedObject;

        public SaintsCellRenderer(
            SerializedObject serializedObject,
            SaintsFieldWithInfo fieldWithInfo)
            : base(serializedObject, fieldWithInfo)
        {
            _serializedObject = serializedObject;
        }

        public IEnumerable<IReadOnlyList<SaintsFieldWithRenderer>> MakeRenderer(SerializedObject serializedObject,
            SaintsFieldWithInfo fieldWithInfo)
        {
            yield return SaintsEditor.WrapAroundSaintsRenderer(new AbsRenderer[] { this }, serializedObject,
                fieldWithInfo, fieldWithInfo.PlayaAttributes.Cast<Attribute>().ToArray());
        }

        protected override bool AllowGuiColor => true;

        public override void OnSearchField(string searchString)
        {
        }

        public override void OnDestroyIMGUI()
        {
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            return 0;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
        }

#if UNITY_2021_3_OR_NEWER
        public override void OnDestroyUIToolkit()
        {
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot, VisualElement container)
        {
            return (CreateFieldElement(_resultElement), false);
        }

        private VisualElement _resultElement;
        private VisualElement _container;

        private bool _arraySizeCondition;
        private bool _richLabelCondition;

        public VisualElement GetElementAndInit(VisualElement resultElement)
        {
            _resultElement = resultElement;
            VisualElement root = new VisualElement();
            IReadOnlyList<SaintsEditor.RendererGroupInfo> groups = SaintsEditor.ChainSaintsFieldWithInfo(
                new[] { FieldWithInfo }, _serializedObject, this);
            foreach (SaintsEditor.RendererGroupInfo group in groups)
            {
                ISaintsRenderer renderer = SaintsEditor.MakeRendererForGroupIfNeed(group);
                renderer.InAnyHorizontalLayout = InAnyHorizontalLayout;
                VisualElement element = renderer.CreateVisualElement(root);
                if (element != null)
                {
                    root.Add(element);
                }
            }
            return root;
        }

        private VisualElement CreateFieldElement(VisualElement resultElement)
        {
            // Debug.Assert(_resultElement != null);

            _arraySizeCondition = FieldWithInfo.PlayaAttributes.Any(each => each is IPlayaArraySizeAttribute);
            _richLabelCondition = FieldWithInfo.PlayaAttributes.Any(each => each is LabelTextAttribute);

            // int index = SerializedUtils.PropertyPathIndex(FieldWithInfo.SerializedProperty.propertyPath);
            // SaintsArrayDrawer.ElementField wrapper = new SaintsArrayDrawer.ElementField($"Element {index}", resultElement)
            // {
            //     userData = new UserDataPayload(),
            // };
            // wrapper.labelElement.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);

            VisualElement wrapper = new VisualElement
            {
                userData = new UserDataPayload(),
            };
            wrapper.Add(resultElement);

            _container = wrapper;
            UIToolkitUtils.OnAttachToPanelOnceWithEnsure(_container, () =>
            {
                _container.schedule.Execute(() => OnUpdateUIToolKit(_container));
                _container.schedule.Execute(() => OnUpdateUIToolKit(_container)).Every(100);
            });
            return _container;
        }

        private class UserDataPayload
        {
            public bool Init;
            public string XML;
            // public Label Label;
            // public string FriendlyName;
            public RichTextDrawer RichTextDrawer;

            public bool TableHasSize;
        }

#if UNITY_2021_3_OR_NEWER
        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult preCheckResult = base.OnUpdateUIToolKit(root);
            if (_container == null)
            {
                return preCheckResult;
            }

            if(_arraySizeCondition)
            {

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
                Debug.Log(
                    $"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; preCheckResult.ArraySize={preCheckResult.ArraySize}, curSize={FieldWithInfo.SerializedProperty.arraySize}");
#endif
                // Debug.Log(preCheckResult.ArraySize);
                if (preCheckResult.ArraySize.min != -1 || preCheckResult.ArraySize.max != -1)
                {
                    int sizeMin = preCheckResult.ArraySize.min;
                    int sizeMax = preCheckResult.ArraySize.max;

                    bool changed = false;
                    // Debug.Log($"sizeMin={sizeMin}, sizeMax={sizeMax}, arraySize={FieldWithInfo.SerializedProperty.arraySize}");
                    if (sizeMin >= 0 && FieldWithInfo.SerializedProperty.arraySize < sizeMin)
                    {
                        FieldWithInfo.SerializedProperty.arraySize = sizeMin;
                        changed = true;
                    }
                    if(sizeMax >= 0 && FieldWithInfo.SerializedProperty.arraySize > sizeMax)
                    {
                        FieldWithInfo.SerializedProperty.arraySize = sizeMax;
                        // Debug.Log($"size to {sizeMax} for min");
                        changed = true;
                    }

                    if (changed)
                    {
                        FieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            if (_richLabelCondition)
            {
                string xml = preCheckResult.RichLabelXml;
                // Debug.Log($"xml={xml}");
                UserDataPayload userDataPayload = (UserDataPayload) _container.userData;
                if (!userDataPayload.Init || xml != userDataPayload.XML || (xml ?? "").Contains("<field"))
                {
                    userDataPayload.Init = true;

                    // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                    if (userDataPayload.RichTextDrawer == null)
                    {
                        userDataPayload.RichTextDrawer = new RichTextDrawer();
                    }
                    userDataPayload.XML = xml;
                    VisualElement saintsFieldContainer =
                        _container.Q<VisualElement>(className: SaintsPropertyDrawer.ClassLabelFieldUIToolkit)
                        ?? _container;

                    // saintsFieldContainer.style.backgroundColor = Color.red;
                    // Debug.Log($"TARGET: {saintsFieldContainer}/{_container}");
                    IEnumerable<RichTextDrawer.RichTextChunk> chunks = string.IsNullOrEmpty(xml)? null: RichTextDrawer.ParseRichXmlWithProvider(xml, this);
                    UIToolkitUtils.ChangeLabelLoop(saintsFieldContainer, chunks, userDataPayload.RichTextDrawer);
                }
            }

            return preCheckResult;
        }
#endif

#endif
    }
}
