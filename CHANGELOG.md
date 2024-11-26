# Changelog

## 3.5.2 ##

1.  Fix multiple auto getters on a same field will cause partly filled values.
2.  Add `bool delayedSearch = false` for `ListDrawerSettings` to delay the search until you hit enter or blur the search field

## 3.5.1 ##

1.  Performance improvement, mainly for UI Toolkit, and partly for IMGUI, [#98](https://github.com/TylerTemp/SaintsField/issues/98)
2.  `SaintsFieldConfig` add `delay` and `update interval` for auto getters so you can have better control about it.

    It's recommended to set `delay` to 100 and `update interval` 0 (means disabled), because usually you'll not need to frequently check the resources. Everytime clicking on the target will do an update, which is enough for most cases.

## 3.5.0 ##

1.  UI Toolkit: Fix an issue with `MinMaxSlider(free: true)` that the high/low is jump back to code value when you input an out-ranged value, then slide back to in-range value
2.  Fix `Button` won't work if there are two methods with the same name (but different arguments overload) in the same class, [#104](https://github.com/TylerTemp/SaintsField/issues/104)
3.  UI Toolkit: Fix `OnValueChanged` won't get triggered when a `SerializeReference` field is changed, [#97](https://github.com/TylerTemp/SaintsField/issues/97)

    **Known Issue**:

    Unity changed how the `TrackPropertyValue` and `RegisterValueChangeCallback` works. Using on a `SerializeReference`, you can still get the correct callback, but the callback will happen multiple times for one change.

    Using `OnValueChanged` on an array/list of `SerializeReference` can cause some problem when you add/remove an element: the `Console` will give error, and the inspector view will display incorrect data. Selecting out then selecting back will fix this issue.
    However, you can just switch back to the old way if you do not care about the field change in the reference field, (Because Unity, still, does not fix related issues about property tracking...) by clicking `Window` - `Saints` - `Create or Edit SaintsField Config` and change the config here.

    These two issues can not be fixed unless Unity fixes it.

    See: [1](https://issuetracker.unity3d.com/issues/visualelements-that-use-trackpropertyvalue-keep-tracking-properties-when-they-are-removed), [2](https://issuetracker.unity3d.com/issues/visualelement-dot-trackpropertyvalue-doesnt-invoke-the-callback-when-the-property-is-under-serializereference-and-serializefield-attributes)

4.  `SaintsEditor`: Add `OnArraySizeChanged` to watch the array size change, [#97](https://github.com/TylerTemp/SaintsField/issues/97)

## 3.4.12 ##

Fix not work with Unity < 2021.3

## 3.4.11 ##

1.  `SaintsRow` now support managed reference type. [#80](https://github.com/TylerTemp/SaintsField/issues/80)
2.  Add `Window/Saints/Create or Edit SaintsField Config` config tweak so you can change the default behavior of auto getters. [#72](https://github.com/TylerTemp/SaintsField/issues/72#issuecomment-2453595293)
3.  UI Toolkit: fix auto-indent for foldout in nested layout can incorrectly indent some fields.

## 3.4.10 ##

Fix a bug that low/high input in `MinMaxSlider` with `free: false` won't work and get reset to min/max value, [#94](https://github.com/TylerTemp/SaintsField/issues/94)

## 3.4.9 ##

1.  Add `EUnique.Remove`, `EUnique.Disable` for `Dropdown` & `AdvancedDropdown`. When using on a list/array, a duplicated option can be removed or disabled.
2.  IMGUI: `Expandable` fix repeatedly creating `SerializedObject` and lead to un-editable fields. Possibly related to [#78](https://github.com/TylerTemp/SaintsField/issues/78)
3.  IMGUI: many IMGUI only parameters are removed from `AdvancedDropdown`.

## 3.4.8 ##

1.  Using `Dropdown`/`AdvancedDropdown` directly on an enum field (without specifying the callback) will allow you to pick
    up one enum, despise whether the enum is `[Flags]` or not. This is useful when you want to pick up one enum value.  [#81](https://github.com/TylerTemp/SaintsField/issues/81)
2.  Using `RichLabel` on an enum will allow `Dropdown`/`AdvancedDropdown` to change the name displayed for each enum item.
    The `EnumFlags` will also change the button's name accordingly. Please note: only standard Unity's RichText label is supported yet.
    Extended tag like `<icon>`, `<label>` and extended color name will not be supported at the point.

## 3.4.7 ##

1.  Fix `OnEvent` and `OnButtonClick` saving target incorrect and gives error.
2.  Fix `GetByXPath` failed to find the correct target.

## 3.4.6 ##

1.  Fix `OnEvent` and `OnButtonClick` wouldn't save the result.
2.  Fix `Dropdown` and `AdvancedDropdown` gave error when working with `long` type, [#92](https://github.com/TylerTemp/SaintsField/issues/92)
3.  Re-paint icons used in this project

## 3.4.5 ##

UI Toolkit: fix `MinMaxSlider` with `free` can display wrong slider when manually input.

## 3.4.4 ##

Allow free input value in `MinMaxSlider` if you manually input int the field, [#48](https://github.com/TylerTemp/SaintsField/issues/48)

## 3.4.3 ##

UI Toolkit: Fix `Get*` attributes won't save the value

## 3.4.2 ##

1.  Fix `GetComponentInScene` won't search a disabled object
2.  Fix auto-getter attributes support for `SaintsInterface`
3.  IMGUI: fix `OnValueChanged` did not work

## 3.4.1 ##

1.  UIToolkit: Fix `ShowInInspector` won't disable the updated value
2.  Fix `LayoutEnd` will make the following fields visible like `ShowInInspector`
3.  Add `DOTweenStart` as an alias of `DOTweenGroup`

## 3.4.0 ##

This upgrade contains **Breaking Changes**! Though it will not break your code, but some behavior is adjusted. Please read before upgrade.

**Bug Fix**:

1.  UI Toolkit: Fix `FieldType` won't update the value if the value is changed externally.
2.  Fix value sign error if the field is in a list/array in some cases.

**Breaking Changes**:

Since this version, `GetComponent*`, `FindComponent`, `GetPrefabWithComponent`, `GetScriptableObject` is inherent under `GetByXPath` (which will be documented separately). The behavior for these attributes is changed as:

*   If target is mis-matched, it'll be auto-resigned to the correct value. (Except `GetComponentByPath`, which will give you a reload button)
*   If the target is not found, it'll be auto-resigned to null. (Except `GetComponentByPath`, which will give you a remove button)
*   All attributes optionally receives `EXP` as the first argument, which has:

    *   `NoInitSign`: do not sign the value if the value is null on firsts rendering.
    *   `NoAutoResignToValue`: do not sign the value to the correct value on the following renderings.
    *   `NoAutoResignToNull`: do not sign the value to null value if the target disappears on the following renderings.
    *   `NoResignButton`: when `NoAutoResign` is on, by default there will be a `reload` button when value is mismatched. Turn this on to hide the `reload` button.
    *   `NoMessage`: when `NoAutoResign` and `NOResignButton` is on, by default there will be an error box when value is mismatched. Turn this on to hide the error message.
    *   `NoPicker`: this will remove the custom picker. This is on by default (if you do not pass `EXP` as first argument) to keep the consistency.
    *   `KeepOriginalPicker`: UI Toolkit only. By default, when a custom picker is shown, Unity's default picker will hide. This will keep Unity's picker together.
    *   `Silent` = `NoAutoResign | NoMessage`. Useful if you want to allow you to manually sign a different value with no buttons and error box.
    *   `JustPicker` = `NoInitSign | NoAutoResignToValue | NoAutoResignToNull | NoResignButton | NoMessage`. Do nothing but just give you a picker with matched targets.
    *   `Message` = `NoAutoResignToValue | NoAutoResignToNull | NoResignButton`. Just give an error message if target is mismatched.

All these attributes except `GetComponentByPath` uses `EXP.NoPicker | EXP.NoAutoResignToNull` as default.

Upgrading from previous version, you may notice:

1.  As these attributes complain less when missing the target, you may want to add a `[Required]` together.
2.  If you have same component added multiple times on the same target, it might only find the first one of them.

I'll update the document about `GetByXPath` soon.

## 3.3.9 ##

**Experimental** IMGUI for `GetByXPath`

## 3.3.8 ##

1.  `ArraySize` allow to set range, implements [#77](https://github.com/TylerTemp/SaintsField/issues/77)
2.  If you have `ListDrawerSettings` enabled, the `Add` and `Remove` buttons will be disabled/enabled accordingly if you also have `ArraySize`
3.  Improve logic and fix some bugs for `ListDrawerSettings` for IMGUI
4.  **Experimental** `GetByXPath` fix predicates parsing

## 3.3.7 ##

1.  Use `AdvancedDropdown` for `ReferencePicker`, implement [#87](https://github.com/TylerTemp/SaintsField/issues/87)
2.  UI Toolkit: Fix `AdvancedDropdown` long text align, fix no auto-focus in search field

**Experimental Feature**

Since this version, a new attribute called `GetByXPath` is added (only support UI Toolkit yet). This attribute is designed to be the super class of `GetComponent*`, `GetPrefab` etc.

I'm still working on it, but most used features are already there. The API is not documented yet. The syntax only has [some notes in Chinese](https://github.com/TylerTemp/SaintsField/blob/master/Runtime/SaintsXPathParser/README.md) if you're interested.

I'm actively working on this feature, hopefully to make it available ASAP. In the meantime, I now start to processing all the pending issues.

## 3.3.6 ##

Fix broken addressable support due to last version's refactor.

## 3.3.5 ##

1.  Fix inconsistent log of `ShowIf` and `HideIf`(also `PlayaShowIf`, `PlayaHideIf`):

    1.  As `[ShowIf]` will show the field, now `[HideIf]` will hide the field.
    2.  The `Or` logic is not completely correct for `HideIf`, especially with `EMode` config.

2.  Now you can use `[ShowIf(false)]`, `[HideIf(true)]` to directly show or hide the target field.
3.  UI Toolkit: fix array/list/struct foldout out of space when using `Layout`

## 3.3.4 ##

1.  Fix `PlayaShowIf`/`PlayaHideIf` could not be used more than once on the same target.
2.  IMGUI: Fix missing decorators. If you see duplicated decorators in your project, go: `Window` - `Saints` - `Enable IMGUI duplicated decorator fix`.
3.  IMGUI: Fix inconsistent height update for `InfoBox` and `ResiziableTextArea` in Unity 2022.3.46, [#85](https://github.com/TylerTemp/SaintsField/issues/85).
4.  IMGUI: Fix `PostFieldButton`, `AboveButton`, `BelowButton` use shared error message when in a list/array.
5.  Change `null` value color for `ShowInInspector`.
6.  Fix `ShowInInspector` can not detect a dictionary when the target is `IReadOnlyDictionary<,>`

## 3.3.3 ##

1.  UI Toolkit now can fall back to an IMGUI custom drawer if the target field is specified to be drawn by an IMGUI drawer (note: `<icon>` in `RichLabel` will not work and will get removed).
2.  Change the order of static/readonly field of `ShowInInspector` so it can stay at the position where it declared. Change the order of `property` to be above `method`.
3.  Fix compile error on old Unity version.
4.  `ColorToggle` supports `Graphic` (`text`, `TMP_Text`, `Image` etc). [#83](https://github.com/TylerTemp/SaintsField/issues/83)

## 3.3.2 ##

1.  Fix `Required` not work on every element of list/array.
2.  Fix `RichLabel` etc. failed to find a correct fallback drawer like `UnityEvent`

## 3.3.1 ##

Fix `Dropdown` & `AdvancedDropdown` not work on list/array.

## 3.3.0 ##

This upgrade **CONTAINS BREAKING CHANGES**, read before you upgrade.

1.  **Breaking Changes**: `InfoBox` now default at top. parameter `above` has been renamed to `below`. If you use `[InfoBox(..., above: true)]`, you will need to remove the `above`, or change it to `below`
2.  Add `BelowInfoBox` to show at below.
3.  Fix `Attribute`-s finding error when there is an `abstruct class` in inherent, related to [#79](https://github.com/TylerTemp/SaintsField/issues/79)
4.  Add `LayoutStart` as an alias of `LayoutGroup`. `LayoutGroup` is now deprecated (not removed).
5.  Add `PlayaInfoBox` for any property/field/method with rich text supports, implements [#71](https://github.com/TylerTemp/SaintsField/issues/71)
6.  IMGUI: fix incorrect display for `Separator` when `EAlign` is `End`.
7.  `$` prefix to set parameter as a callback/property

## 3.2.6 ##

1.  Improved `LayoutGroup` which supports `./GroupName` to add nested subgroup. [#67](https://github.com/TylerTemp/SaintsField/issues/67)
2.  IMGUI: better visual display for `TitledBox`, `FoldoutBox` and foldout tabs. [#64](https://github.com/TylerTemp/SaintsField/issues/64)

## 3.2.5 ##

`GetResourcePath` etc. support some build-in object types like `AudioClip`. [#69](https://github.com/TylerTemp/SaintsField/pull/69)

## 3.2.4 ##

1.  IMGUI: fix `Expandable` won't save the changed value.
2.  Fix `Required` won't work in parent of an inherited class.
3.  Fix `FieldType` won't work in array/list.
4.  Allow `FieldType` with `compType=null` to by-pass the issue that Unity won't show all the prefabs with expected component in the picker.

## 3.2.3 ##

1.  IMGUI: fix disposed access.
2.  IMGUI: fix `ProgressBar` dragging changes all instance inside array.

## 3.2.2 ##

1.  Fix possible disposed attribute access, fixes [#62](https://github.com/TylerTemp/SaintsField/issues/62)
2.  Pause `GetComponent*`, `GetScriptableObject`, `OnButtonClick` etc. when entering play mode

## 3.2.1 ##

1.  Rich label now supports `<container.Type />` to display the class/struct name of the container of the field, and `<container.Type.BaseType />`.
2.  `Separator` to draw text, separator, spaces for field on above / below with rich text & dynamic text support.
3.  `Layout`, `LayoutGroup` now supports `marginTop`, `marginBottom`, fixes [#52](https://github.com/TylerTemp/SaintsField/issues/52)
4.  IMGUI: fixes `Layout` broken inside `SaintsRow`

## 3.2.0 ##

1.  **Breaking Changes**: `GetComponentInChildren`, `GetComponentInParent`, `GetComponentInParents` will search the target itself, which means it's now the same behavior as Unity's build-in functions. No more surprises. Fixes [#56](https://github.com/TylerTemp/SaintsField/issues/56)
2.  **Breaking Changes**: `GetComponentInParents` now have `bool includeInactive = false` as the first argument to align with Unity's build-in function. Be aware this might break your existing code
3.  `GetComponentInChildren`, `GetComponentInParent`, `GetComponentInParents` now have `bool excludeSelf = false` argument to exclude the target itself from searching.
4.  `OnEvent` name now support dot to fetch property
5.  **Breaking Changes**: Since this version, for `Enable`/`Disable`/`Show`/`Hide`-`If`, the `EMode` argument now has the same operation logic with other arguments (in previous version it has a higher priority to shadow other arguments). This might break your existing code.

    Thanks, [@Lx34r](https://github.com/Lx34r), for [PR55](https://github.com/TylerTemp/SaintsField/pull/55)
6.  `ShowInInspector` now can show a dictionary (or any object implements `IDictionary<,>`), fixes [#57](https://github.com/TylerTemp/SaintsField/issues/57)
7.  `ShowInInspector` now can show public fields & properties of any type

## 3.1.5 ##

1.  Add `ELayout` shortcut:

    *   `TitleBox` = `Background | Title | TitleOut`
    *   `FoldoutBox` = `Background | Title | TitleOut | Foldout`
    *   `CollapseBox` = `Background | Title | TitleOut | Collapse`

    And improve the appearance for UI Toolkit with foldout/collapse + title.

2.  UI Toolkit: `Layout` now have some space with fields
3.  Fix drawer fallback not work for array/list
4.  Fix possible property disposed access
5.  UI Toolkit: Improved `AdvancedDropdown` position
6.  Fix `ListDrawerSettings` error when searching a enum field
7.  `ListDrawerSettings` now can search the child field when the target is a `ScriptableObject`

## 3.1.4

1.  Fix `EnableIf` not work with Value Comparison
2.  Fix [#42](https://github.com/TylerTemp/SaintsField/issues/42) GetComponent etc need better error handling and report
3.  Fix callback won't fill null parameter value when the value of the target is null
4.  Deprecated `PlayaArraySize`. Just use `ArraySize` instead.
5.  Fix `SaintsInterface` drawer did not work inside array.
6.  [#45](https://github.com/TylerTemp/SaintsField/issues/45): Now `GetComponent*` works with array/list, and will be auto filled if you have `SaintsEditor` enabled.
7.  UI Toolkit: Fix `AdvancedDropdown` out of screen.

## 3.1.3

Value Comparison for Show/Hide/Enable/Disable-If

Now the string can have `!` prefix to negate the comparison.

And `==`, `!=`, `>` etc. suffix for more comparison you want.

## 3.1.2

1.  `AssetPreview` now will load the preview of the `gameObject` if the target is a `Component`. Useful when you use a script type to reference a prefab.
2.  Fix `SaintsInterface` won't work when T is a super class of ScriptableObject.
3.  **Breaking Changes**: `ISaintsArray` has been removed, use `IWrapProp` instead.
4.  Now most decorators can be used on `SaintsArray`, `SaintsList` and `SaintsInterface`.
5.  Now `Dropdown` and `AdvancedDropdown` supports struct value too.
6.  Add document for `SaintsArray`, `SaintsRow` about custom drawer.

## 3.1.1

1.  Fixed: `AssetPreview` should not destroy the texture and lead to empty image.
2.  UI Toolkit: Better `ELayout.TitleOut` with foldout mode for `[Layout]`.
3.  Add `ListDrawerSettings`  to search and paging a large list/array.

## 3.1.0

1.  Layout system now have `LayoutGroup`, `LayoutEnd` to quickly group many fields together.
2.  DOTweenPlay tools now have `DOTweenPlayGroup`, `DOTweenPlayEnd` to quickly group many DOTweenPlay methods together.
3.  Layout now use the last config for the target group, this is useful when you want to inherent but want to change config of parent's Layout group.

Note: This version re-worked the layout system and might break the existing functions. Report a bug if you face any issues.

## 3.0.14

UI Toolkit: Fix [#35](https://github.com/TylerTemp/SaintsField/issues/35) float `[MinMaxSlider]` read min value to max value when using input rather than slider.

## 3.0.13

1.   (by [@Lx34r](https://github.com/Lx34r)) Fix foldout display error with tab in Layout Attribute [#33](https://github.com/TylerTemp/SaintsField/pull/33)
2.   Fix [#34](https://github.com/TylerTemp/SaintsField/issues/34) that Unity will dispose the serialized property first before destroy the drawer, leading to errors when drawer is in an array and the target element gets removed.

## 3.0.12

1.  `Button` now allows to have parameters and you can change the value in inspector.
2.  If you have `DOTween` installed without ASMDEF enabled, there will be a popup window to ask you either enable it, or disable SaintsField's DOTween ability.

## 3.0.11

`SaintsInterface` for serializing interface of a `UnityEngine.Object` type (or sub type).

Special thanks for [@dbc](https://stackoverflow.com/users/3744182/dbc)'s [answer in stackoverflow](https://stackoverflow.com/questions/78513347/getgenericarguments-recursively-on-inherited-class-type-in-c?noredirect=1#comment138415538_78513347)

## 3.0.10

Fix in some Unity version (tested on 2022.3.20) a cached struct object can not correctly report the value inside it after changing [#29](https://github.com/TylerTemp/SaintsField/issues/29)

## 3.0.9

1.  Fix `ResourcePath` not work on `ScriptableObject`
2.  UI Toolkit: fix `Layer` won't save the changed value [#28](https://github.com/TylerTemp/SaintsField/issues/28)

## 3.0.8

1.  `SaintsEditor` now have `OnEvent` to bind a method to your custom `UnityEvent`
2.  Fix fallback system broken on generic types [#27](https://github.com/TylerTemp/SaintsField/issues/27)

## 3.0.7

`SaintsEditor` now have `OnButtonClick` to bind a method to a button click event.

## 3.0.6

Fix can not check if a type is a subclass of another generic type, and failed to use the custom drawer. [#23](https://github.com/TylerTemp/SaintsField/issues/23)

## 3.0.5

1.  IMGUI now can fallback to CustomPropertyDrawer of data type (previously only PropertyAttribute drawer)
2.  Add `DOTween` detect using `DOTWEEN` marco added by `DOTween` when setup
3.  UI Toolkit: Fix `DOTweenPlay`, `ParticlePlay` button on Unity before 2022.2 which uses `unityBackgroundScaleMode`

## 3.0.4

1.  `ShowInInspector` now support to show array/list
2.  Fix `ShowInInspector` and some other SaintsEditor tool might not find the correct target in some case (especially inside `SaintsRow`)

## 3.0.3

1.  UI Toolkit: fix `DecoratorDrawer` get drawn more than once.
2.  Add `Enum` support for `ShowIf`/`HideIf`/`EnableIf`/`DisableIf`/`PlayaShowIf`/`PlayaHideIf`/`PlayaEnableIf`/`PlayaDisableIf`
3.  Improve `SaintsArray` so it works more like an actual array.

## 3.0.2

1.  UI Toolkit: fix `AddressableAdress` broken since 3.0.0
2.  UI Toolkit: fix dropdown button incorrect layout (`Dropdown`, `AdvancedDropdown`, `AddressableAdress` etc)
3.  UI Toolkit: allow buttons to wrap if `EnumFlags` has both the `AutoExpand` and `DefaultExpand` as `false`
4.  UI Toolkit: fix `NavMeshAreaMask` incorrect label align.
5.  Addressable tools now can open addressable groups edit window.
6.  AnimatorParams now can open the animator edit window.
7.  Fix `AnimatorParams` for integer type.

## 3.0.1

1.  UI Toolkit: Fix `SaintsEditor` "Script" field not aligned
2.  Fix [Issue 20](https://github.com/TylerTemp/SaintsField/issues/20) that `EnumFlags` check contains for mixed bits, and flip bits when toggling a mixed bit.
3.  UI Toolkit: fix `EnumFlags` layout issue.

## 3.0.0

1.  Completely rewrite UI Toolkit components with `"unity-base-field__aligned"` class. Since this version, UI Toolkit will no longer have the weird label width issue.

    The `UI Toolkit Label Width Fix` function is now disabled and removed.

2.  `TryFixUIToolkit` in `SaintsEditor` is now deprecated.

3.  `[UIToolkit]` (used for label width fixing) attribute is now deprecated

4.  `ShowAboveImage`/`ShowBelowImage` now use `FieldStart` as default align.

5.  IMGUI: fix `PlayaRichLabel` for `SaintsRow`

6.  UI Toolkit: fix `ShowInInspector` won't update values for non-serialized fields

7.  `UnsaintlyEditor` is removed

## 2.4.2

1.  Since this version, UI Toolkit now can property fallback to `CustomPropertyDrawer` of a custom type (previously it only supports to fallback to custom `PropertyAttribute` drawer).

    Note: this feature is only for UI Toolkit. IMGUI does not support this feature.

    Note: combining with `RichLabel`, UI Toolkit will find the first `unity-label` class label which in some case might not be correct. This feature can not be turned off yet. Please report issue if you face any problem.

2.  `SaintsList`/`SaintsArray` fix working with `RichLabel`, fix deep nesting rendering issue.

## 2.3.9

1.  Add `PlayaRichLabel` for array label modification.
2.  Fix callback for `OnChanged`, `RichLabel` etc. sometimes can not get a correct callback parameter filled.

## 2.3.8

*   Add `SaintsArray`, `SaintsList` for nested array/list serialization.
*   IMGUI: Change the logic of how rich text is rendered when the text is long.
*   Fix `AnimatorStateChanged` not excluded Editor fields.

## 2.3.7

*   Add `ParticlePlay`

## 2.3.6

*   Remove a forgotten log in `SaintsEditor` UI Toolkit.
*   Add `PlayaArraySize` which can deal with 0 length array/list.

## 2.3.5

*   Add `ArraySize`
*   UI Toolkit: Fix `ShowInInspector` for property that the equal operation is incorrect and repeatedly destroy and create elements. Fix a UI Toolkit weird bug that can not update values.
*   UI Toolkit: Fix `EnumFlags` icons not display.

## 2.3.4

*   IMGUI: Fixes [#9](https://github.com/TylerTemp/SaintsField/issues/9) a typo that breaks the `AnimatorState`.

## 2.3.3

*   Fixes [#9](https://github.com/TylerTemp/SaintsField/issues/9) `AnimatorState` won't work for sub-state machines.
*   `AnimatorState` now allow class/struct with satisfied fields.
*   Fix `AnimatorState` can not property display an error when the target is not found, or the target has no controller.
*   `AnimatorState` now has a `Edit {yourAnimatorController}...` option to directly open the animator controller.

## 2.3.2

*   Fix `AssetPreview` incorrect time to destroy the preview texture.
*   UI Toolkit: Fix `Dropdown` giving error when working with `RichLabel`.
*   Allow `EnumFlags` to be expanded with `Disabled` state.
*   IMGUI: Fix `EnumFlags` sometimes need click more than once to expand
*   IMGUI: `ReadOnly` (`DisabledIf` etc) now disable the whole field rather than the value field only.
*   `Expandable` now works properly with `ReadOnly` (`DisabledIf` etc):

    *   The toggle will never be disabled
    *   IMGUI: The field will be disabled accordingly. (In UI Toolkit they will not be disabled because of the limitation from Unity)

## 2.3.1

1.  UI Toolkit: fix labelWidth didn't get fixed unless you change the layout (e.g. resize the inspector)
2.  UI Toolkit: fix `AdvancedDropdown` didn't display a label because of last version's fix.
3.  UI Toolkit: `AdvancedDropdown` now use the width of the full field.
4.  UI Toolkit: `AdvancedDropdown` now hide breadcrumbs if it's not a nested list, thus it looks more like a normal dropdown when you use it as a searchable dropdown.
5.  Custom object picker now share the same preview scales between all instances, making it feels more like the Unity's default picker.
6.  Custom object picker preview panel now have a background color to distinct it from the select area.
7.  Custom object picker search field now have a "clean" button. Due to some IMGUI limitation, clicking it will make the input field LOSE focus.

## 2.3.0

Fix the UI Toolkit buggy label width, finally!

1.  UIToolkit: when using a long label, the label will take more space (rather than be truncated in the previous version). Now it behaves the same as UI Toolkit components.
2.  `Scene` attribute now have a "Edit Scenes In Build..." option to directly open the "Build Settings" window where you can change building scenes.
3.  `InputAxis` attribute now have a "Open Input Manager..." option to directly open "Input Manager" tab from "Project Settings" window where you can change input axes.
4.  `SortingLayer` attribute now have a "Edit Sorting Layers..." option to directly open "Sorting Layers" tab from "Tags & Layers" inspector where you can change sorting layers.

As the most bothering issue fixed in UIToolkit, I'm now removing "experimental" from UIToolkit support. If you face any issue, please submit in [Github Issue Page](https://github.com/TylerTemp/SaintsField/issues).

Considering the following labels with UI Toolkit enabled:

```csharp
public string itsALongRideForPeopleWhoHaveNothingToThinkAbout;
public string aBitLongerThanDefault;
public string s;  // short
```

By default (or with `PropertyField` from UI Toolkit), Unity display it as this (it's a IMGUI style even with UI Toolkit on):

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/059bd138-8178-4958-950a-daef7cd6ca9a)

Now, let's apply any UI Toolkit component (except `PropertyField`), it will (surprisingly!) use the modern UI Toolkit flavor label layout:

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/2cdea75f-5a39-46ae-91d2-023f861b593f)

This inconsistency can make your inspector looks sooooooo weird and very funny because of un-aligned fields. This problem is reported to Unity but never got fixed. Considering:

```csharp
// default field with UI Toolkit, Unity will truncate it to IMGUI width, instead of UI Toolkit flavor
public string thereIsSomeGoodNewsForPeopleWhoLoveBadNews;
// UI Toolkit component! Unity will grow the space like UI Toolkit
[UIToolkit] public string weWereDeadBeforeTheShipEvenSank;
// another default, Unity will use the IMGUI style width (some mix of a percent, min-width and caculation result) instead of UI Toolkit
public string myString;
// another UI Toolkit component! Unity will use the UI Toolkit style width (120px) like UI Toolkit
[UIToolkit] public string myUiToolkit;
```

The field indent is a mess, even you're sticking to the UI Toolkit inspector:

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/04303517-fe3d-4c42-992e-cf97f86524ad)

This issue is so difficult to solve, that even OdinInspector does not try to fit it. (Or maybe they just don't care...?)

SaintsField now use some trick to make `PropertyField` label behaves much more like the vanilla UI Toolkit component:

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/a8b90764-7053-4dcc-9af4-5f804f5e12fb)

That means:

1.  Label will by default get 120px width, which is UI Toolkit's default
2.  The space gets grow to fix the label when the label gets longer, which is also UI Toolkit's default
3.  If you have a very looooong label, the value field will be shrank out of view. This is also how UI Toolkit works.

## 2.2.3

1.  Add `[Conditional("UNITY_EDITOR")]` for attributes so it won't be included in your build.
2.  Fix example scene will break the build (if you import it in your project).
3.  Fix `InfoBox` can not get correct callback when it's nested inside an array.
4.  Add `ResourcePath` to get a string path of a resource which allows you to specific required types, and have a custom object picker.

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
