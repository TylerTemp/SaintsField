## 2.2.2

1.  `Rate`: no longer frozen the first star if the range starts from 1.
2.  UIToolkit `MinMaxSlider`: incorrect update min/max value when there is an error.
3.  Fix `EnumFlags` IMGUI incorrect height result because Unity will give width=1 during rendering IMGUI for the first time...
    Fix `EnumFlags` incorrect field type checking and did not work inside `Serializable`.
4.  Fix `Rate`, `PropRange`(IMGUI) do not immediately update the value when it's inside a `Serializable`.

## 2.2.1

1.  Add `RequireType`. This attribute allow you to specify the required component or **interface** for a field.
2.  `FieldType` now has a custom object picker window. By default Unity disallow to see an asset with a specific type because of "performance consideration". Ironically, if you directly use a component type instead of a `GameObject`, Unity **WILL** show a correct asset picker. As this can not be "hacked" or "tricked" to let Unity show the correct picker, the custom object picker will allow you to pick a object (e.g. a prefab) with the component you want.

## 2.2.0

1.  Fix [Issue 8](https://github.com/TylerTemp/SaintsField/issues/8) that most attributes in `SaintsField` did NOT looking for the inherited parent target... This includes `PropRange`, `Min`, `Max`, `Dropdown`, `AdvancedDropdown`, `SpriteToggle`, `MaterialToggle`, `ColorToggle`, `RichLabel`, `Above/BelowRichLabel`, `InfoBox`, Buttons etc.
2.  Fix `Dropdown` and `AdvancedDropdown` incorrect parent finding which may lead to incorrect dropdown items.
3.  Most callbacks now can receive the value of the target field. Allowing a suitable required/optional parameter in the callback to make it work.

This update does not break existing APIs, but greatly changed internal logic of property/method finding, so it's a minor version bump.

Special thanks to [ZeroUltra](https://github.com/ZeroUltra) to find this important issue.

## 2.1.13

1.  Fix [issue 7](https://github.com/TylerTemp/SaintsField/issues/7): when Unity uses `NativeProperty` to inject property with native code
    and serialized property, `serializedObject` will still give correct serialized fields, but reflection will not work, making `SaintsEditor`
    failed to display some fields.
2.  Remove default `SaintsEditor` for example scene, so people who imports it (most likely when using `unitypackage`) won't accidentally
    get `SaintsEditor` enabled for the whole project.
3.  Fix a bug that possibly break `rate` in some situation.
4.  IMGUI: fix `SaintsEditor` display an empty `MonoScript` when the target is neither `MonoBehavior` nor `ScriptableObject`.

## 2.1.12

1.  IMGUI: Fix `SaintsRow` incorrect renderer cache when inside a list.
2.  Fix `ReadOnly` will disable the field when there is an error in your callbacks.
3.  Add `EMode` for `ReadOnly`, `EnableIf`, `DisableIf`, `ShowIf`, `HideIf` to specific if editor is playing or not.
4.  **Breaking Changes**:`ReadOnly`, `EnableIf`, `DisableIf`, `ShowIf`, `HideIf` no longer support directValue and groupBy parameters.
5.  `SaintsEditor`: Add `PlayaDisableIf`, `PlayaEnableIf`, `PlayaShowIf`, `PlayaHideIf`

## 2.1.11

1.  Fix [Issue 3](https://github.com/TylerTemp/SaintsField/issues/3) Texture2D can not be destroyed on a GC calling.
2.  UI Toolkit: Fix [Issue 2](https://github.com/TylerTemp/SaintsField/issues/2) incorrect readonly.
3.  Fix `HideIf` inconsistent logic of being opposite of `ShowIf`.
4.  Add `EnableIf`, `DisableIf`

## 2.1.10

1.  Add `SaintsRow` for `Serializable` class/struct to draw Button/Layout/DOTweenPlay for the target field.
2.  Fix `AboveImage`/`BelowImage` gives error instead of display nothing when the SpriteRenderer/Image/RawImage does not have a sprite.

## 2.1.9

1.  `RichLabel`, `AboveRichLabel`, `BelowRichLabel`, `OverlayRichLabel`, `PostFieldRichLabel` now can receive the value and/or the index (if it's in a list/array) in the callback function.
2.  IMGUI: fix incorrect height on first time rendering.
3.  `OnChanged` now can receive the changed value in the callback.
4.  Fix string value incorrect truly check for `ValudateInput` and `InfoBox`
5.  `InfoBox` now will disappear if the callback function returns null as content.
6.  Fix `InfoBox` gives error instead of display nothing when the content is null.
7.  Fix `AboveImage`/`BelowImage` gives error instead of display nothing when the SpriteRenderer/Image/RawImage does not have

## 2.1.8 ##

1.  IMGUI: Fix PropertyField not with `includeChildren: true` and lead to broken `ReferencePicker`
2.  `ValidateInput` now can receive the value and/or the index (if it's in a list/array) in the callback function.

## 2.1.7 ##

1.  Fix `MinMaxSlider` incorrect step.
2.  Fix `ProgressBar` set value is not in real-time in struct type.
3.  UI Toolkit: Fix incorrect image align for `AsssetPreview`, `AboveImage`, `BelowImage`
4.  UI Toolkit: Fix `LeftToggle` not apply the value.

## 2.1.6 ##

1.  UI Toolkit: Fix `MinMaxSlider` incorrect clamp.
2.  UI Toolkit: Fix `MinMaxSlider` difficult to manually input a value because of auto-correction.

## 2.1.5 ##

1.  A better parser for `AdvancedDropdown`. Now you can use `/` to define a sub item.
2.  UI Toolkit: dropdown icon for `AdvancedDropdown` looks better now.
3.  Fix readme about `FindComponent`
4.  `ValidateInput` now support validation callback with parameter of the target field value.
5.  `SaintsEditor` fix a button rendered twice if it's override (or use `new`) in a derived class.
6.  `SaintsEditor`, `ShowInInspector` now will change appearance when value changed for auto property.
7.  `SaintsEditor`, `ShowInInspector` now support to show `null` value with a yellow background.

## 2.1.4 ##

1.  Add `NavMeshAreaMask` to select NavMesh area bitmask for Unity's AI Navigation.
2.  Add `NavMeshArea` to select NavMesh area as name, value or bitmask.
3.  Fix a weird issue that `SaintsEditor` might not find the correct `MonoScript` in Unity 2021

## 2.1.3 ##

1.  Add `GetComponentInParent` / `GetComponentInParents`
2.  `ValidateInput` now also support for `bool` result.
3.  `ValidateInput` now will continuously validate the input rather than check on value changed.
4.  Fix `<label/>` not work in rich text when working with NaughtyAttributes.

## 2.1.2 ##

Add `GetComponentByPath`. Now you can auto sign a component with hierarchy by path, with index filter support.

## 2.1.1 ##

1.  `SaintsEditor` now supports Layout (Foldout, Tab, GropBox, TitledBox etc) to group several fields together
2.  Fix incorrect width condition check for UI Toolkit when trying to fix Unity's labelWidth issue.

## 2.1.0 ##

1.  **Breaking Changes**: rename `UnsaintlyEditor` to `SaintsEditor`
2.  `SatinsEditor` now supports `DOTweenPlay` to preview DOTween without entering play mode
3.  Add `Windows/Saints` menu for quick function toggle
4.  **Breaking Changes**: rename `InfoBox`'s `contentIsCallback` to `isCallback`
5.  **Breaking Changes**: General Buttons rename parameter `buttonLabelIsCallback` to `isCallback`
6.  General Buttons now will use function name as label when you omit the `buttonLabel` parameter

## 2.0.12 ##

1.  Fix `Addressable` broken on last code refactor
2.  UI Toolkit: Fix `Addressable` picker out of view when the item is long

## 2.0.10 ##

1.  IMGUI: Fix `DecoratorDrawer` got drawn more than once because Unity changes the behavior of `PropertyField` in 2022.1+
2.  IMGUI: Fix `Texture2D` resize for Unity 2021.1

## 2.0.9 ##

1.  IMGUI: Fix `Expandable` foldout overlap with label, [Issue#1](https://github.com/TylerTemp/SaintsField/issues/1)
2.  IMGUI: Fix `Expandable` may fail when expand a Unity component
3.  IMGUI: Fix `ReadOnly` did not have `EditorGUI.BeginDisabledGroup`  and `EditorGUI.EndDisabledGroup` in pair

## 2.0.8 ##

1.  Fix `Required` not work on nested case. Remove the limitation on int/float
2.  Fix many attributes didn't find correct parent object and failed on some cases
3.  IMGUI: Fix incorrect height for `InfoBox`
4.  Fix incorrect height when you manually disable UI Toolkit
5.  IMGUI: Fix `Expandable` not fold/expand correctly on array/list
6.  Fix incorrect texture scale function
7.  Fix `AssetPreview` won't scale up when give a bigger width/height value
8.  Change `AssetPreview`'s parameters: `maxWidth`->`width`, `maxHeight`->`height`
9.  UI Toolkit: Fix `AboveImage`/`BelowImage` has empty frame space when scale down

## 2.0.7 ##

Add `ReferencePicker` for Unity's [`SerializeReference`](https://docs.unity3d.com/ScriptReference/SerializeReference.html)

## 2.0.6 ##

1.  Fix `Dropdown` has not sub item for `/` in UI Toolkit
2.  `Dropdown` Allow to disable `/` as a sub item

## 2.0.5 ##

1.  Add `AdvancedDropdown` for UI Toolkit
2.  Fix a issue that when select a value from dropdown, the value is changed internally, but will get actually applied only when you finish inspect current object (like, to inspect another object, close Unity, etc)
3.  Change the logic of finding resources

## 2.0.4 ##

1.  Add `ProgressBar`
2.  Fix `RichLable(null)` not work in IMGUI after the UI Toolkit refactor
3.  Fix `IMGUI` `PropertyScope` not disposed issue
4.  Add color `charcoalGray`, `oceanicSlate`

## 2.0.3 ##

1.  Fix scene out of boundary when you remove a scene from build list
2.  (UI Toolkit) Expandable use `InspectorElement` so it can use a custom editor if you have one
3.  (UI Toolkit) Change all `Toggle` drawer to use `RadioButton` component
4.  (UI Toolkit) Fix `CurveRange` won't allow to set `m_CurveColor` by script
5.  No longer disable IMGUI functions when UI Toolkit is active, to allow Unity (or your custom inspector) to use UI Toolkit or IMGUI when you have both available.

## 2.0.2 ##

1.  When using UI Toolkit mixed with default field with no drawer, now you can add `[UIToolkit]` so `SaintsField` will try to align the label width the same way as UI Toolkit does.
2.  `UnsaintlyEditor` will try to fix the label width issue when using UI Toolkit. If you're using `UnsaintlyEditor`, you do not need the `[UIToolkit]` attribute.

UI Toolkit supports are experimental, you can disable it by adding a custom marco `SAINTSFIELD_UI_TOOLKIT_DISABLE`

## 2.0.1 ##

Fix UI Toolkit breaks old Unity versions.

## 2.0.0 ##

1.  Fix `GameObjectActive` won't save after change active state.
2.  Experimental: support for UI Toolkit (Unity 2022.2+ uses UI Toolkit by default)
3.  Fix `UnsaintlyEditor` incorrect fields order.

## 1.2.3 ##

1.  Add `FindComponent`
2.  Add `ButtonAddOnClick`
3.  Add `UnsaintlyEditor`

## 1.2.2 ##

1.  No longer need `read/write` enabled when using a picture as icon
2.  `AboveImage`/`BelowImage` now will try to get the image from the field when no `name` is given
3.  Change how the scale of `AssetPreview` handled

## 1.2.1 ##

1.  Add `CurveRange`
2.  Add `AdvancedDropdown`
3.  Fix `EColor.Green` incorrect color present
4.  Allow `SAINTSFIELD_ADDRESSABLE_DISABLE` macro to disable `Addressable` related attributes

## 1.2.0 ##

*   Now we supports Unity `2019.1+`!
*   Add `AddressableLabel`. This only works if you have `Addressable` installed in your project.
*   Add `AddressableAddress`. This only works if you have `Addressable` installed in your project.
*   Add `GetScriptableObject`
*   `Expandable` now can be used on anything that is serializable as long as the target can use used by `SerializedObject`, no longer limited to `ScriptableObject`.
*   Fix `Expandable` background covers the fields on Unity 2019.
*   `<color>` label now use the same color as [Unity Rich Label](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html#ColorNames) plus some extra colors.
*   Fix `clear` color in `RichLabel` actually got white color.
*   Colors name is now case insensitive in `RichLabel`.

## 1.1.3 ##

*   `AnimatorParam` no longer offers `null` value
*   `AnimatorParam` and `AnimatorState` now will try to find the `animator` on current object if the name of `animator` is not set
*   Use standard field picker for `layer` and `tag`
*   Add `LeftToggle`
*   Fix a issue that when using `Scene` with a string without default value, it would display the first item but the actually value is null or empty string.
    Now it will sign the first value on it.
*   Fix a issue that `Scene` will display empty when your scene name starts with an underscore.

## 1.1.2 ##

*   Fix indent for `Expandable`
*   Add `GetComponentInChildren`
*   Add `GetComponent`
*   Add `GetComponentInScene`
*   Add `GetPrefabWithComponent`
*   Add `AddComponent`

## 1.1.1 ##

*   Fix `AboveImage` and `BelowImage` won't scale if the target image is not readable/writeable
*   Fix `AboveImage` and `BelowImage` scale logic
*   Rename `Range` to `PropRange`
*   Fix a bug that the space below the field is calculated incorrectly

## 1.1.0 ##

*   Allow for list/array field
*   `RichLabel` no longer draw a background
*   Massive fix for callbacks that did not use the parent container as the target
*   Fix image cached logic for `AssetPreview` and `AboveImage`/`BelowImage`
*   Fix `Dropdown` when you put it on a field of a `struct`
*   Now you can use `RichLabel` to override child field of an array/list

The core of how `Attribute`s are discovered is now changed. This version should be compatible with old ones, but in case I pumped the minor version number to `1`.

## 1.0.9 ##

*   Add `EnumFlags`
*   Fix a issue that `OnValueChanged` will get called when no change happens at all.

## 1.0.8 ##

*   Add `AboveImage`, `BelowImage`
*   Add `EAlign` for `AssetPreview`
*   Fix scale for `AssetPreview`
*   Add `Range`
*   Fix an issue that editor scripts will get build and lead to a build error

## 1.0.7 ##

*   Add `OverlayRichLabel`
*   Add `PostFieldRichLabel`

## 1.0.6 ##

Add `InputAxisAttribute`

## 1.0.5 ##

1.  Add `RateAttribute` for rating stars.
2.  Fix `ValidateInputAttribute` won't call the validator the first time it renders.
3.  Fix an indent issue
