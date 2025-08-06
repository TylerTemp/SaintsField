# Changelog

## 4.24.5 ##

UI Toolkit: Fix blue indicator for prefab modification not display in some property [#276](https://github.com/TylerTemp/SaintsField/issues/276)

## 4.24.4 ##

1.  UI Toolkit: Add blue indicator for prefab modification in:
    *   `AnimatorParam`
    *   `AddressableLable AddressableAddress CurveRange`
    *   `AddressableScene`
    *   `NavMeshArea`
    *   `NavMeshAreaMask`
    *   `SpineSkin`
    *   `SpineSlot`
    *   `SpineAttachment`

    Note: There are still some attributes that does not work with prefab's modification blue line. This is basiclly Unity's fault... Read more from [here](https://discussions.unity.com/t/writing-drawers-for-classes-with-properties-that-cant-be-bound/904711/2). There is solution that [should fix this](https://github.com/OscarAbraham/UITKEditorAid/tree/development), but that package seems not work (maybe my setup is wrong).
    I'm still trying to make this work for these attributes/types, but it'll need some time: `AdvancedDropdown`, `Dropdown`, `SaintsArray`, `SaintsList`, `SaintsDictionary`, `SaintsInterface`, `SaintsHashSet`, `ReferenceHashSet`, `TypeReference`, `SaintsEvent`, `FlagsDropdown`, `EnumToggleButtons`, `ResourcePath`, `ResourceFolder`, `AssetFolder`

2.  Fix: when editing a UI prefab, because Unity will add a "Context Canvas" above the root of the prefab, "Auto Getters" now will ignore that object.
3.  UI Toolkit: Fix right click context menu for `Spine` related attributes.

## 4.24.3 ##

1.  Fix `enum` of `byte` type gives error on picker [#278](https://github.com/TylerTemp/SaintsField/issues/278)
2.  Add blue indicator for prefab modification in:
    *   `Layer`
    *   `Scene`
    *   `SortingLayer`
    *   `Tag`
    *   `InputAxis`
    *   `ShaderParam`
    *   `ShaderKeyword`

## 4.24.2 ##

1.  UI Toolkit: Attempting to inherit the existing Unity context menu in nested classes when right-click on a field [#254](https://github.com/TylerTemp/SaintsField/issues/254)
2.  UI Toolkit: Fix `AdvancedDropdown` can not show the changed indicator (blue line on left) in prefab when edited. This also fixes enum dropdown if you have `SaintsEditor` enabled
3.  UI Toolkit: Fix `ResiableTextArea` label did not have a right-click context menu

## 4.24.0 ##

Add `OptionsDropdown` and `PairsDropdown`

## 4.23.0 ##

Introduce `SaintsEvent`. `SaintsEvent` is an alternative to Unity's `UnityEvent`. It's inspired by [UltEvents](https://assetstore.unity.com/packages/tools/gui/ultevents-111307) & [ExtEvents](https://github.com/SolidAlloy/ExtEvents)

## 4.22.2 ##

1.  UI Toolkit: Fix `Table` re-order function not working [#272](https://github.com/TylerTemp/SaintsField/issues/272)
2.  `TypeReference` add `string[] onlyAssemblies = null` and `string[] extraAssemblies = null`
3.  **Experimental**: UI Toolkit: Add `SaintsEvent` types:
    *   Not documented
    *   Requires [Unity Serialization](https://docs.unity3d.com/Packages/com.unity.serialization@3.0/manual/index.html) installed
    *   Don't use it in a production env. It still needs some work

## 4.22.1 ##

1.  Fix `<field />` not get updated in `ComponentHeader`  [#266](https://github.com/TylerTemp/SaintsField/issues/266)
2.  Fix `EnumToogleButtons` expanded button not get disabled with `DisableIf`/`EnableIf`/`ReadOnly`
3.  Add support for Unity's [`InspectorNameAttribute`](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/InspectorNameAttribute.html) for `enum` type
4.  IMGUI: fix `Table` foldout clicking on label didn't expand the field [#265](https://github.com/TylerTemp/SaintsField/issues/265)

## 4.22.0 ##

1.  Add `TypeReference` type to serialize a `System.Type`
2.  UI Toolkit: Fix data type in SaintsField with attributes will render the attributes twice
3.  UI Toolkit: `AdvancedDropdown` fix long text align, and now it hides the icon space if all items have no icon

## 4.21.1 ##

1.  Fix Header Drawers might read a disposed texture and give error [#255](https://github.com/TylerTemp/SaintsField/issues/255)
2.  UI Toolkit: fix drawer still trying to access a disposed property in `Button`
3.  UI Toolkit: fix `SaintsDictionary` can not hide a label if the target is drawn by an IMGUI component
4.  Add `HeaderButtonGhost` support for Unity < 2021.3 [#257](https://github.com/TylerTemp/SaintsField/issues/257)

## 4.21.0 ##

1.  UI Toolkit: Add `SaintsHashSet<T>` & `ReferenceHashSet<T>` data type as serializable `HashSet` [#251](https://github.com/TylerTemp/SaintsField/issues/251)
2.  Saints XPath now can compare `[@{myProp} = false]` if `myProp` is a bool type
3.  UI Toolkit: Fix `SaintsDictionary` paging button can not click

## 4.20.1 ##

Fix `ColorPalette` attribute arguments not working, fix its auto validatior.

## 4.20.0 ##

**Breaking Changes**: `ColorPalette` overhaul.

NOTE: if you're using the previous `ColorPalette`, you WILL lose this palette data.

`ColorPalette` now using a tag to mark every color. For UI Toolkit, it now also have a way much better UI for you to create palette with drag/drop support.

This version removes the `group` idea. A color is only marked with many tags. In the feature version, it'll have main tags for you to easily grouping them.

The search bar now also support `#RRGGBB` search. If you're using UI Toolkit, a nicely `TypeAhead` popup will show you the possible options.

## 4.19.1 ##

1.  Fix search function got `StackOverflow` when a target contains a looped-reference. [#250](https://github.com/TylerTemp/SaintsField/issues/250)
2.  Add `<index/>`, `<index=D4/>` tag for rich label. If the property is in a list/array, the coresponding index value will be used for this tag
3.  Add `{0:formatControl}` support for `<index/>` and `<field/>` tag like a Unity's standard `string.Format`. You can now even do weird shit like: `<field.subField=(--<color=red>{0}</color>--)/>` will be interpreted like `string.Format("(--<color=red>{0}</color>--)", this.subField)`.

## 4.19.0 ##

1.  You can now use `$:ClassName.CallbackName` or `$:ClassName.FieldName` to call a static/const value/method in most place like `ShowIf/HideIf`, `EnableIf/DisableIf`, `RequiredIf`, `BelowImage/AboveImage` etc.
2.  When a callback returns a `null` result, `AboveImage`, `BelowImage` now shows nothing, instead of giving an error notice.

## 4.18.1 ##

1.  `Handles` displays now can be toggled using right click context menu [#217](https://github.com/TylerTemp/SaintsField/issues/217)
2.  Fix some handles not displayed on list
3.  Add `dotted` for line, arrow type of handles
4.  Add `alpha` for handles' `eColor` color control
5.  Improved `ArrowHandleCap` with better scaling

## 4.18.0 ##

1.  Add `RequiredIf` for conditional required field; `EMode` supported
2.  Add Auto Validator for `OnEvent` & `OnButtonClick`
3.  Fix `Button` of `IEnumerator` won't hide the loading icon even the coroutine is done
4.  Fix Auto Validator won't check sub object of a prefab

## 4.17.0 ##

1.  Add `EMode.InstanceInScene`, `EMode.InstanceInPrefab`, `EMode.Regular`, `EMode.Variant`, `EMode.NonPrefabInstance` which can be used for `ShowIf`, `HideIf`, `EnableIf`, `DisableIf`, and `Playa*` version of them.

    *   `EMode.InstanceInScene`: target is a prefab placed in a scene
    *   `EMode.InstanceInPrefab`: target is inside a prefab (but is not the top root of that prefab)
    *   `EMode.Regular`: target is at the top root of the prefab
    *   `EMode.Variant`: target is at the top root of the prefab, and is also a variant prefab
    *   `EMode.NonPrefabInstance`: target is not a prefab (but can be inside a prefab)
    *   `EMode.PrefabInstance` = `InstanceInPrefab | InstanceInScene`
    *   `EMode.PrefabAsset` = `Variant | Regular`

2.  Provide a workaround solution for [#240](https://github.com/TylerTemp/SaintsField/issues/240), for editing a field inside a serializable struct.

## 4.16.5 ##

If you have multiple targets selected, `AboveButton`, `BelowButton`, `PostFieldButton` can be triggered on all selected targets.

## 4.16.4 ##

1.  `ListDrawerSetting`, `Table`, `SaintsDictionary` search now support `SerializeReference` field search, and is case-insensitive.
2.  UI Toolkit: `SaintsDictionary` now support debounce search

## 4.16.3 ##

Fix project failed to compile due to code changed in SaintsDictionary

## 4.16.2 ##


1.  UI Toolkit: If you have multiple targets selected, `Button` can be triggered on all the targets. (NOTE: this does not work on `AboveButton`, `BelowButton`, `PostFieldButton` yet)
2.  Change the logic of `SaintsDictionary` to allow inherent
3.  Add constructor method for `SaintsList` & `SaintsArray` so they can be used more like a native array/list

## 4.16.1 ##

1.  UI Toolkit: `Searchable` now can search a field when you input a field name from code. In previous version it need to match the display name
2.  UI Toolkit: if you're using Unity 6k+, we now `Unbind` the element to stop the property tracking [#239](https://github.com/TylerTemp/SaintsField/issues/239)
3.  Fix `OnValueChanged` callback does not work when the target field is an `Enum` and the callback receives the corresponding type of `Enum`

## 4.16.0 ##

UI Toolkit: Add `Searchable` to search field name for MonoBehavior (Component) or ScriptableObject

## 4.15.1 ##

1.  UI Toolkit: fix `ShowInInspector` setting a list to `null` gave an error
2.  UI Toolkit: `ShowInInspector` now fold the struct/class by default. This is to avoid a looped references stack overflow rendering. The data will only be filled the first time you expand it.

    Some genius decide to use loop-referenced data in the game save data type. I can not tell them not to do so. No cap frfr.

## 4.15.0 ##

1.  UI Toolkit + IMGUI: Add `SceneViewPicker` to pick an object from the scene view into a field [#231](https://github.com/TylerTemp/SaintsField/issues/231)
2.  Improve the compatibility with Odin Inspector. Now most attributes can be used together with Odin Inspector.

## 4.14.0 ##

UI Toolkit: Add `TableHeaders` & `TableHeadersHide` to default show/hide table columns

## 4.13.5 ##

1.  UI Toolkit: fix unity might give an error when removing an element from list
2.  IMGUI: add right click context menu support for `Table` [#211](https://github.com/TylerTemp/SaintsField/issues/211)
3.  UI Toolkit: fix auto validator UI logic; add validation if an animator state has any `StateMachineBehaviour` scripts
4.  Auto Validator add button for quick adding Addressable scenes and assets

## 4.13.4 ##

1.  UI Toolkit: fix default item in object picker might give an error
2.  Fix `AdvancedDropdown` can not property handle rich tags because `/` is used as path separator

## 4.13.3 ##

1.  UI Toolkit: Fix `GetWwise` can not find `State`, `Switch`
2.  UI Toolkit: Fix `GetWwise` can not work with `IWrapProp` (`SaintsDictionary`)
3.  UI Toolkit: Fix some label issue when fallback to IMGUI drawer
4.  Change the behavior of auto getters when `ResignToNull` is on: it'll not try to reduce the size of the array
5.  UI Toolkit: Fix duplicated `ReferencePicker` when user already uses one [#237](https://github.com/TylerTemp/SaintsField/issues/237)

## 4.13.2 ##

Fix the way to get IMGUI fallback drawer height. This inflects both IMGUI, and UI Toolkit falling-back to IMGUI

## 4.13.1 ##

1.  UI Toolkit: fix `SaintsDictionary` enum dropdown didn't display the correct selected value [#236](https://github.com/TylerTemp/SaintsField/issues/236)
2.  Fix auto getter order checking issue in auto validator
3.  Auto validator now only check prefab & `ScriptableObject` for assets
4.  Add `Add Scenes In Build` & `Add All Assets` buttons in auto validator window for quick checking
5.  UI Toolkit: fix `bool` type display in horizontal layout

## 4.13.0 ##

1.  UI Toolkit: Add `GetWwise` to automatically get a Wwise object
2.  Fix a false array detection in SaintsEditor

## 4.12.0 ##

1.  Add `PlayaAboveRichLabel`/`PlayaBelowRichLabel` to draw a rich label above/below a field/method/property (including array/list)
2.  UI Toolkit: Allow `PlayaAboveRichLabel` & `PlayaInfoBox` be applied to a class/struct definition

## 4.11.0 ##

1.  Add `HeaderLabel` to draw a label in component header
2.  `Required` now check truly value for `I2.LocalizedString`
3.  Add customize `Required` message type [#234](https://github.com/TylerTemp/SaintsField/issues/234)
4.  Add `AddressableSubAssetRequired` to validate `subAsset` in types like `Addressable.AssetReferenceSprite`

## 4.10.0 ##

1.  Add `Component Header` related attributes. Now you can draw buttons, icons etc on the component header. [#154](https://github.com/TylerTemp/SaintsField/issues/154)
2.  UI Toolkit: `MinMaxSlider` friendly error if been used on wrong type [#232](https://github.com/TylerTemp/SaintsField/issues/232)
3.  UI Toolkit: `ShowInInspector` now shows an error box if the target attributes raises an error in its getter
4.  UI Toolkit: `ShowInInspector` now always update the sub-field value display if the target (e.g. a class) has sub-fields/properties
5.  UI Toolkit: if you have `SaintsEditor` enabled, a bare `[SerializedReference]` will automatically use `ReferencePicker` + `SaintsRow` drawer

## 4.9.0 ##

1.  UI Toolkit: if you have `SaintsEditor` enabled, `enum` will automatically use `AdvancedDropdown` drawer
2.  UI Toolkit: if you have `SaintsEditor` enabled, `enum` with `[Flags]` will automatically use `FlagsDropdown` drawer
3.  UI Toolkit: now you can use keyboard (up/down/left/right arrow, return key) to select an `AdvancedDropdown`
4.  UI Toolkit: fix custom picker when you have search in block view, arrow key gives error if the currently selected item is not in loaded results
5.  UI Toolkit: fix custom picker show placeholder information when the selected item has not appeared in loading results yet
6.  UI Toolkit: fix custom picker showing results that does not match the search when you start typing before the loading process finished
7.  UI Toolkit: fix `AdvancedDropdown` layout issue that an item can overlap a bit with the search box
8.  UI Toolkit: fix `AdvancedDropdown` didn't show the selected group when you backward a page
9.  If you use a low version of `.NET` which does not support `B`, `B8` formatting string, `RichLabel` can now property format it like `<field=B/>`, `<field=B16/>`

## 4.8.0 ##

1.  UI Toolkit: Add `TableHide` to exclude a field/column from `Table` [#225](https://github.com/TylerTemp/SaintsField/issues/225)
2.  Fix the error check in SpineAnimationPickerAttributeDrawerUIToolkit:UpdateDisplay for SkeletonData being null is backwards [#229](https://github.com/TylerTemp/SaintsField/issues/229)
3.  Fix (SpineAnimationPicker) skeletonTarget callbacks do not check for null prefabs or null instances [#230](https://github.com/TylerTemp/SaintsField/issues/230)

## 4.7.5 ##

1.  UI Toolkit: `ListDrawerSettings` now allow async search to avoid blocking the editor thread.
2.  Add constructor and Editor setter for `SaintsInterface` [#228](https://github.com/TylerTemp/SaintsField/issues/228)

## 4.7.4 ##

1.  UI Toolkit: Fix ReadOnly does not work with 3rd party & Unity default drawer [#227](https://github.com/TylerTemp/SaintsField/issues/227)
2.  UI Toolkit: Fix ReferencePicker dropdown appears in the wrong place [#226](https://github.com/TylerTemp/SaintsField/issues/226)
3.  UI Toolkit: Fix duplicated label shown in Table on every column [#224](https://github.com/TylerTemp/SaintsField/issues/224)

## 4.7.3 ##

1.  UI Toolkit: `FieldType`, `ResourcePath`, `RequireType` now use the new object picker for an async and fast experience
2.  UI Toolkit: fix native decorator header with no other attributes does not draw [#223](https://github.com/TylerTemp/SaintsField/issues/223)

## 4.7.2 ##

1.  UI Toolkit: auto getters now can use async resource loading instead of blocking the Unity Editor thread. The picker view will continuously add resources with a loading icon.
    It'll also cache the results instead of loading them again every time you open the picker.
2.  UI Toolkit: fix `SaintsInterface` cache refresh when project changed.
3.  UI Toolkit: fix `SaintsDictionary` object type can not display a correct label. [#220](https://github.com/TylerTemp/SaintsField/issues/220)
4.  UI Toolkit: fix `SaintsDictionary` value get drawn multiple times after dragging.
5.  UI Toolkit: workaround for a Unity bug that `AuxWindow` can not be properly closed (affected: `SaintsInterface` drawer, auto getter drawer)

## 4.7.1 ##

1.  UI Toolkit: `SaintsInterface` now will limit the scene object to current ones when in prefab:
    *   For prefab context/isolated editing, or prefab project inspecting, the scene object will only list the prefab itself or its children objects
    *   For prefab already instantiated in the scene hierarchy, the scene object will only list the scene objects that the instance in
2.  UI Toolkit: Optimize `SaintsInterface` drawer to allow async object loading & cached results. It no longer searches unnecessary resources. [#199](https://github.com/TylerTemp/SaintsField/issues/199)
3.  UI Toolkit: `SaintsInterface` will now display a loading icon if there are resources fetching
4.  UI Toolkit: `SaintsInterface` now allows keyboard arrow up/down key in list view, and keyboard arrow up/down/left/right key in block view to select objects

## 4.7.0 ##

**Breaking Changes**: inherent from `SaintsDictionaryBase` has been changed. Read the document in the `SaintsDictionary` section for more information

1.  `SaintsDictionary` now support list/array as value
2.  Change `SaintsList`, `SaintsArray` to `class` type so it can be null, just like `List<T>` and `Array`

## 4.6.4 ##

1.  UI Toolkit: Fix `double` & `float` editing round [#215](https://github.com/TylerTemp/SaintsField/issues/215)
2.  Fix error should be dismissed when user override the `Equal` method which raises an error
3.  UI Toolkit: Fix `ShowInInspector` an error when add element to a list/array
4.  UI Toolkit: `ShowInInspector` now only show `public` `instance` field to avoid loop calling
5.  UI Toolkit: Fix fallback drawer breaks `AYellowpaper.SerializedDictionary`
6.  UI Toolkit: Fix `WindowInlineEditor` not re-paint when target changed [#214](https://github.com/TylerTemp/SaintsField/issues/214)

## 4.6.3 ##

Fix broken auto getters since 4.6.2... (sad face)

## 4.6.2 ##

Fix auto getters sign in-scene objects to not-in-scene object fields (e.g. `GetComponentInScene` used on a prefab, the prefab sould not reference to a scene-object. But if you put the target in the scene, then the field should be signed)

## 4.6.1 ##

1.  UI Toolkit: Fix `ShowInInspector` can not correctly draw `Gradient`, `Curve` & `Hash128`
2.  UI Toolkit: Improve the label layout in horizontal layout for Unity's native types (`Vector2` etc.)
3.  UI Toolkit: Fix `ShowInInspector` might fail to draw some properties, give errors and block the rest drawing process

## 4.6.0 ##

UI Toolkit: `ListDrawerSettings` add `string extraSearch`, `string overrideSearch` to allow custom search.

## 4.5.3 ##

1.  UI Toolkit: fix `ListDrawerSettings` can not render items correctly in the new fallback flow
2.  UI Toolkit: fix `ResizableTextArea` did not get disabled with `ReadOnly`, `DisableIf`
3.  Fix demo code blocked the build process [#208](https://github.com/TylerTemp/SaintsField/issues/208)
4.  UI Toolkit: fix `ListDrawerSettings` didn't refresh size when changed by auto getters

## 4.5.1 ##

UI Toolkit: fix `OnValueChanged` broken

## 4.5.0 ##

1.  UI Toolkit: when `SaintsEditor` enabled, list/array can automatically add `SaintsRow` if necessary
2.  UI Toolkit: `ListDrawerSettings`, same as above. Fix incorrect indent for the foldout
3.  UI Toolkit: fix an issue that `PropRange` gave an error when working with list/array
4.  IMGUI: fix height issue in `Table`, [#201](https://github.com/TylerTemp/SaintsField/issues/201)
5.  UI Toolkit: fix width changes in horizontal `Layout`
6.  UI Toolkit: fix `Expandable`, `AnimatorState` foldout out of space when using inside `Layout`
7.  UI Toolkit: fix `UnityEvent` can not be drawn correctly under some circumstances
8.  UI Toolkit: fix `ShowInInspector` can not correctly draw a field when using a parent class type

## 4.4.0 ##

1.  UI Toolkit: `Table` now support showing `Button` & `ShowInInspector`. (Note: Any kind of `Layout` will be ignored)
2.  UI Toolkit: Fix `NoLabel` didn't work when inside a horizontal layout
3.  UI Toolkit: Fix `Table` foldout out of area. If a struct is a field, that field will remove the `foldout`

## 4.3.3 ##

UI Toolkit: Some unity built-in class type has some very weird behavior. Fallback to default drawing flow instead [#200](https://github.com/TylerTemp/SaintsField/issues/200)

## 4.3.2 ##

1.  Fix `SaintsInterface` can not pick a component when there are multiple component matches [#199](https://github.com/TylerTemp/SaintsField/issues/199)
2.  `ReferencePicker` now gives a more friendly error if it's been used on a wrong type
3.  UI Toolkit: fix `ResizableTextArea` did not save the changes

## 4.3.1 ##

1.  Fix `SaintsEditor` can not find a correct typed drawer on first drawing after a domain reload
2.  `SaintsDictionary` now can use the new fallback system, which means if you have a struct/class typed key/value, you can use `Layout`, `Button` etc. in it without add a `SaintsRow` decorator
3.  Fix `AnimatorParam` didn't work on an `AnimatorOverrideController` animator.

## 4.3.0 ##

1.  **Breaking Changes**: `SepTitle` arguments now aligned with `Separator`: `string title`, `EColor color`, `EAlign eAlign`, `int space`
2.  UI Toolkit: `SepTitle` now supports rich text tags.
3.  Fix single `Layout(keepGrouping: false)` still continuously grouping, `LayoutToggle` not working, `LayoutEnd(name)` did not close layout in some cases after version 4.2.0

## 4.2.1 ##

Fix DOTweenPlay... Holy \*\*\*\*...

## 4.2.0 ##

1.  UI Toolkit: Add `PlayaSeparator`, similar to `Separator`, but it can be applied to list/array, and layout system
2.  Improved `Playa*` with layout system. Now `PlayaInfoBox`, `PlayaSeparator` will be applied to the layout if there is one, otherwise to a field/property/method

## 4.1.5 ##

UI Toolkit: Fix IMGUI fallback in Unity 6k

## 4.1.4 ##

UI Toolkit: Fix IMGUI fallback. The fallback flow in 4.1.3 works in Unity 6k, but not in lower version like Unity 2022. This release uses different fallback flow for it.

## 4.1.3 ##

1.  UI Toolkit: Fix disable related function not working since 4.0, [#194](https://github.com/TylerTemp/SaintsField/issues/194)
2.  UI Toolkit: Fix fallback to IMGUI gave empty space drawing [#193](https://github.com/TylerTemp/SaintsField/issues/193)
3.  UI Toolkit: Fix Unity 6k changes that breaks `ResizableTextArea`
4.  UI Toolkit: Fix `SaintsRow(inline: true)` not using inline drawing
5.  UI Toolkit: Improved Layout for `Button`

## 4.1.2 ##

1.  UI Toolkit: Fix fallback to IMGUI lead to multiple drawing [#193](https://github.com/TylerTemp/SaintsField/issues/193)
2.  UI Toolkit: Fix logic of fixing out-area foldout icon

## 4.1.1 ##

1.  Fix a bug that the fallback drawer can not be created if the drawer is inherited from an abstract class [#192](https://github.com/TylerTemp/SaintsField/issues/192)
2.  If a bool field is under any horizontal layout, an `LeftToggle` will be auto applied
3.  If a struct/class/interface field is under any horizontal layout, the label will not be put into a new line

## 4.1.0 ##

1.  UI Toolkit: Improve horizontal layout label (still have some issue for struct/class. Will be fixed soon)
2.  UI Toolkit: Fixes SerializedProperty disposed error [#192](https://github.com/TylerTemp/SaintsField/issues/192)

## 4.0.4 ##

1.  UI Toolkit: Copy/Paste a row in `ListDrawerSettings`/`Table` using either ctrl+c/ctrl+v or RMB is now supported.
2.  UI Toolkit: Add copy/paste ability to many fields, e.g. `Tag`, `Layer`, `Scene`
3.  Fix `SaintsInterface` gave an error when working with auto getters.
4.  Add `SaintsObjInterface<TInterface>` as a shortcut for `SaintsInterface<UnityEngine.Object, TInterface>`.

## 4.0.3 ##

1.  UI Toolkit: `Layer` of string type should not give error when the layer name is an empty string
2.  UI Toolkit: Add right click copy/paste ability to `AdvancedDropdown`, `Dropdown`, `ListDrawerSettings` & `Table`
3.  UI Toolkit: Fix an error when using `PropRange` with int type
4.  IMGUI: Change `EnumToggleButtons` selected one with green background color

## 4.0.2 ##

1.  UI Toolkit: Fix `SaintsEditor` failed to fall back a type drawer if the drawer is inheritance from `SaintsPropertyDrawer` [#187](https://github.com/TylerTemp/SaintsField/issues/187)
2.  `Button` now can display overload and override methods
3.  UI Toolkit: `ShowInInspector` support `char` type

## 4.0.1 ##

1.  UI Toolkit: Fix Unity 6000 injected serialized field can not be properly drawn [#184](https://github.com/TylerTemp/SaintsField/issues/184)
2.  IMGUI: `Table` only rebuild if there are changes happened [#180](https://github.com/TylerTemp/SaintsField/issues/180)
3.  UI Toolkit: `SaintsRow` support copy/paste. Which means serializable struct/class/interface drawn by `SaintsEditor` now support copy/paste just like Unity's default behavior

## 4.0.0 ##

Fix rich text gives error if a close tag has no opening tag

## 4.0.0-preview.5 ##

Fix Enum picker

## 4.0.0-preview.3 ##

1.  UI Toolkit: fix `IMGUIContainer` incorrect draw height when the target field has SaintsField attributes too
2.  UI Toolkit: fix `SaintsEditor` field won't shrink properly when the field is long

## 4.0.0-preview.2 ##

1.  UI Toolkit: fix fallback flow that treats `string` as array/list
2.  UI Toolkit: fix drawer incorrect height when falling back to IMGUI drawer using `IMGUIContainer`
3.  UI Toolkit: fix `SaintsEditor` won't draw a `DecoratorDrawer` of a field

This version is a preview release. For stable release, please use [version 3.36.8](https://github.com/TylerTemp/SaintsField/releases/tag/3.36.8)

## 4.0.0-preview.1 ##

1.  Refactor the `SaintsRow` drawer
2.  Change the flow of how `SaintsPropertyDrawer` & `SaintsEditor` in UI Toolkit fallback to drawers
3.  UI Toolkit: `ShowInInspector` now shows a more grayed-out color for label, to distinguish from the serializable field
4.  UI Toolkit: If you have `SaintsEditor` enabled, or have any saints property added to a serializable class/struct/interface, the `SaintsRow` attribute will automatically be used if the target has no explicit drawer
5.  UI Toolkit: If you have `SaintsEditor` enabled, the order of the property no longer matters. Things like `[Range(0, 1), InfoBox("Saints InfoBox")]` will work as expected. The 3rd party drawer no longer block the saintfield drawer

This version is a preview release. For stable release, please use [version 3.36.8](https://github.com/TylerTemp/SaintsField/releases/tag/3.36.8)

## 3.36.8 ##

1.  UI Toolkit: Add copy/paste ability to `ResizableTextArea`
2.  UI Toolkit: Add copy/paste ability to `PropRange`
3.  UI Toolkit: Add copy/paste ability to `Layer`. `Layer` now gives an error if the layer is not a valid value in project
4.  Add auto validator ability to `Layer`
5.  Fix `LayoutDisableIf`/`LayoutEnableIf`/`LayoutHideIf`/`LayoutShowIf` not working due to previous code changes
6.  Fix `PreferredLabel` not set for some drawers, which protentially cause some label incorrect issue in previous versions

## 3.36.7 ##

1.  UI Toolkit: `Button` with parameters now support interface, class, struct type
2.  Fix `SaintsDictionary` compile error in builds [PR #179](https://github.com/TylerTemp/SaintsField/pull/179) by [@Insprill](https://github.com/Insprill)

## 3.36.6 ##

1.  `EnumToggleButtons` now allow to quick select an enum if it's not a flag. [#139](https://github.com/TylerTemp/SaintsField/issues/139)
2.  Change `EnumFlags` as an alias of `EnumToggleButtons`
3.  UI Toolkit: fix an issue when editing a valued-type key for a dictionary type using `ShowInInspector`

## 3.36.5 ##

1.  UI Toolkit: `ShowInInspector` can now edit a dictionary
2.  Fix a critical issue that when a field is inherited from a `List<,>` or `IReadOnlyList<,>`, some field drawer will recognize its element type wrong and fail (e.g. auto getters)
3.  UI Toolkit: fix some fallback drawer can not display a correct label [#176](https://github.com/TylerTemp/SaintsField/issues/176)
4.  Fix AnimatorParam won't work if the target is inside a prefab which is not loaded into memory by Unity

## 3.36.4 ##

1.  Fix a critical issue that when a field inherited inhered from a `List<,>` or `IReadOnlyList<,>`, some field drawer will recognize it as list and fail (e.g. auto getters)
2.  Cache MemberInfo attributes and other misc optimizations by [@Insprill](https://github.com/Insprill), [PR #175](https://github.com/TylerTemp/SaintsField/pull/175)
3.  Inherent `SaintsDictionaryBase` from `IDictionary` to make it more like a normal dictionary

## 3.36.3 ##

1.  UI Toolkit: `ShowInInspector` can now create/edit polymorphism class, struct & interface
2.  UI Toolkit: Fix `ShowInInspector` losing editing focus when editing an element inside an array/list

## 3.36.2 ##

UI Toolkit: `ShowInInspector` can now edit list/array type

## 3.36.1 ##

1.  UI Toolkit: `ShowInInspector` can now edit a general class type
2.  `Button` no longer gives "Method is never used" warning if you have jetbrains ide package installed (this package is installed default by Unity) [PR](https://github.com/TylerTemp/SaintsField/pull/171) by [@Insprill](https://github.com/Insprill)

## 3.36.0 ##

1.  UI Toolkit: Allow `ShowInInspector` to editor in inspector like Odin. (Does not support list/array, dictionary, null-class yet)
2.  `ShowInInspector` add `byte`/`sbyte` type support ([PR](https://github.com/TylerTemp/SaintsField/pull/164) by [@Insprill](https://github.com/Insprill))
3.  Suppress all compiler warnings of CS0168 & CS0219 caused by preprocessors ([PR](https://github.com/TylerTemp/SaintsField/pull/165) by [@Insprill](https://github.com/Insprill))
4.  `ShowInInspector` no longer try to display a property without a getter

## 3.35.1 ##

Fix auto getters won't apply changes in some cases [#161](https://github.com/TylerTemp/SaintsField/issues/161)

## 3.35.0 ##

1.  `Show/Hide/Disable/Endable-If` now accept sub-fields.
2.  rich label now support `<field />`, `<field.subField/>`, `<field.subField=formatControl />` to display the target field's value.

## 3.34.0 ##

1.  Add `SpineAttachmentPicker` to pick an `attachment` from spine.
2.  Fix Spine related attributes gave error when a target `skeletonData` is missing.
3.  Add `LayoutCloseHere` as a shortcut of `[Layout(".", keepGrouping: false), LayoutEnd(".")]` to include the current field and then close the last named layout group

    `LayoutCloseHere` is useful when you're done with your subgroup, but you might add some field later, but at the point you don't have a field to put a `LayoutEnd`

4.  Add `LayoutTerminateHere` as a shortcut of `[Layout(".", keepGrouping: false), LayoutEnd]` to include the current field and then ternimate the whole layout group

    `LayoutTerminateHere` is useful when you're done with your group, and your script is also done here (so nowhere to put `EndLayout`). Oneday you come back and add some new fields, this attribute can avoid them to be included in the group accidently.
5.  Add `DefaultExpand` and `ArrayDefaultExpand`
6.  Remove `defaultExpanded` from `Table`, use `ArrayDefaultExpand` instead

## 3.33.3 ##

1.  UI Toolkit: fix `Table` add/remove button can not click after version 3.32.0
2.  `Table` add `bool defaultExpanded=false`, `bool hideAddButton=false`, `bool hideRemoveButton=false` parameters [#125](https://github.com/TylerTemp/SaintsField/issues/125)

## 3.33.2 ##

1.  Change the layout of `EnumFlags` so it can handle a large enum without displaying a super long list
2.  **Breaking Changes**: `AutoExpanded` is removed from `EnumFlags`

## 3.33.1 ##

1.  Add constructor for `SaintsDictionary<,>` so it can be created exactly like a standard c-sharp `Dictionary<,>`
2.  If the key/value for `SaintsDictionary<,>` is a general class/struct which already has a custom drawer, use that custom drawer rather than flat it.
3.  Fix `SaintsDictionary<,>` can not detect the serialized field when get inherited

## 3.33.0 ##

1.  Add `Adapt` which can work with `PropRange`
2.  Add `PropRange` support for `Auto Validator`9
3.  UI Toolkit: fix `OverlayRichLabel(end: true)` incorrect overlap
4.  IMGUI: update `Table` on every draw [#157](https://github.com/TylerTemp/SaintsField/issues/157)

## 3.32.1 ##

1.  Add Auto Validator for `MinValue`, fix `MinValue` not get called on first inspecting of the field in IMGUI. Fix `MinValue` use shared error message when using on a list in IMGUI.
2.  Same fix for `MaxValue`
3.  Refactor IMGUI drawer when no attribute is provided
4.  `Scene` add a parameter to allow to save a full-path scene name in build rather than just the name
5.  `ParticlePlay` now will disable the button if the target gameObject is not active. It no longer report an error if the target is `null`. Use `Required` for this purpose.
6.  Rename `FuckDOTween` to `DOTweenDetectPanel` [#152](https://github.com/TylerTemp/SaintsField/issues/152)
7.  Add `EXP.ForceReOrder` to config auto getters to re-order the result (old behavior). This will make the list/array un-reorderable. [#153](https://github.com/TylerTemp/SaintsField/issues/153)

## 3.32.0 ##

1.  `ArraySize` now support callback argument to dynamically limit the size.
2.  UI Toolkit: fix a bug that `ListDrawerSettings` without paging can not set the size correctly.

## 3.31.1 ##

1.  `SaintsDictionary` now can flatten fields the key/value of a general strict/class
2.  `OnButtonClick` and `OnEvent` in UI Toolkit now can check the event signing while the target is being inspected
3.  Remove a log when rich label has a bad label format
4.  `Expandable` now support addressable reference
5.  UI Toolkit: `Expandable` now support a `GameObject`

## 3.31.0 ##

Add `SaintsDictionary<,>`. It allows:
1.  Allow any type of kay/value type as long as `Dictionary<,>` allows
2.  Give a warning for duplicated keys
3.  Allow search for keys & values
4.  Allow paging for large dictionary
5.  Allow inherent to add some custom attributes, especially with auto getters to gain the auto-fulfill ability.

## 3.30.2 ##

Fix auto getters didn't refresh the sources when hierarchy changed [#150](https://github.com/TylerTemp/SaintsField/issues/150)

## 3.30.1 ##

1.  Add `SphereHandleCap` which can draw a sphere in the scene
2.  **Breaking Changes**: change the arguments of `DrawLabel` to support dynamic color, arguments of `DrawLine`, `DrawArrow` for better usage case support
3.  `AdvancedDropdown` now can accept any `IEnumerable<object>` return type
4.  Add alpha argument for `GUIColor` when using `EColor` parameter

Note: all `Handle` attributes (draw stuff in the scene view) are in stage 1, which means the arguments might change in the future.

## 3.29.0 ##

1.  Add `DrawWireDisc` to trace an object in the scene drawing a disc.
2.  `AdvancedDropdown` now will use all the static value from the type when you omit `funcName`.

## 3.28.0 ##

1.  Add `TableColumn` to merge multiple columns into one
2.  UI Toolkit: all buttons that returns `IEnumerator` now will display a loading icon when the coroutine is running
3.  fix `Table` can not disable related buttons when using with `ArraySize`
4.  IMGUI: fix multiple target editing gives error when table sizes are not equal, [#140](https://github.com/TylerTemp/SaintsField/issues/140)
5.  Auto getters on lists/array now no longer force the target to be ordered as what the resources are found, [#145](https://github.com/TylerTemp/SaintsField/issues/145)

## 3.27.1 ##

1.  IMGUI: `Expandable` now internally uses `SaintsRow` for a better rendering result [#142](https://github.com/TylerTemp/SaintsField/issues/142)
2.  IMGUI: Fix a bug that when you select an object, then select nothing, then select back, some attribute might get broken with disposed target error
3.  IMGUI: Fix a potential bug in auto getters
4.  Fix `Troubleshoot` didn't display a completed status

## 3.27.0 ##

1.  Add `SpineSlotPicker`
2.  UI Toolkit: `AdvancedDropdown` remove logs when searching

## 3.26.1 ##

1.  IMGUI: fix `Table` didn't update when size is changed externally
2.  Fix `Table` drag and drop
3.  Fix `ListDrawerSettings` drag and drop [#127](https://github.com/TylerTemp/SaintsField/issues/127)

## 3.26.0 ##

1.  Optimize auto getters. Some auto getters now will use Unity's built-in API first which is way faster than the `XPath` logic.

    They'll also only refresh the hitting resources when you have changes in project (rather than inside a fixed loop)
2.  Add `SpineSkinPicker` to pick a spine skin from a spine skeleton.
3.  Improve auto getters how they find the scene for `scene::` selector.
4.  Fix `I2Loc` package compile error on build.

## 3.25.1 ##

1.  IMGUI: `LocalizedStringPicker` is now supported in IMGUI too.
2.  (Experimental): Optimize auto getters. This feature is disabled by default. Add `SAINTSFIELD_AUTO_GETTER_RESOURCE_OPTIMIZE` to try. This feature will be enabled by default when it's stable.

## 3.25.0 ##

1.  UI Toolkit: Add `LocalizedStringPicker` for [I2 Localization](https://inter-illusion.com/tools/i2-localization). Enable it in `Window` - `Saints` - `Enable I2 Localization Support`
2.  UI Toolkit: fix data didn't get saved if the fallback drawer is a IMGUI drawer
3.  UI Toolkit: fix `ResiziableTextArea` didn't update the display when the value is changed by external code.
4.  UI Toolkit: fix `AdvancedDropdown` search might miss some results when multiple value uses the same last name.
5.  UI Toolkit: now `AdvancedDropdown` support search for paths too (previously only support value search). This is only avaiable for UI Toolkit because IMGUI uses Unity's built-in version and lack of this ability.
6.  UI Toolkit: now `AdvancedDropdown` search will display its parent path.

## 3.24.0 ##

Add `SaintsField.Playa.SaintsNetworkBehaviour` to allow rendering `Button` etc. inside Unity's [Netcode for Game Objects](https://docs-multiplayer.unity3d.com/netcode/current/about/) behavior.

## 3.23.0 ##

1.  Add `AddressableScene` to pick a scene from `Addressable` assets
2.  Move `Addressable` related function to a separated `asmdef` for a better code organization

## 3.22.2 ##

1.  UI Toolkit: Fix `ListDrawerSettings` didn't update the total count when size is changed externally [#123](https://github.com/TylerTemp/SaintsField/issues/123)
2.  UI Toolkit: Fix `ValidateInput` has debug log [#134](https://github.com/TylerTemp/SaintsField/issues/134)
3.  Improve the logic of `Troubleshoot` so it can detect method etc.
4.  IMGUI: Fix `InfoBox` show extra space when hidden [#126](https://github.com/TylerTemp/SaintsField/issues/126)

## 3.22.1 ##

1.  Fix `ListDrawerSettings` can not detect a size change and update display [#123](https://github.com/TylerTemp/SaintsField/issues/123)
2.  Fix UI Toolkit code leak to IMGUI version [#124](https://github.com/TylerTemp/SaintsField/issues/124)

## 3.22.0 ##

1.  Add `Table` to show a list/array of class/struct/`ScriptableObject`(or `MonoBehavior` if you like) as a table
2.  Add `Window/Saints/Troubleshoot` to quickly check why some attributes not working.
3.  UI Toolkit: Fix `AdvancedDropdown` won't update the label when the value is changed externally.
4.  Fix `SpineAnimationPicker` made the project unable to build.

## 3.21.1 ##

1.  IMGUI: `SpineAnimationPicker` is now supported in IMGUI too.
2.  Modify the icon of `SpineAnimationPicker` to be more clear.

## 3.21.0 ##

1.  Add `SpineAnimationPicker` to pick a spine animation.

    [`Spine`](http://en.esotericsoftware.com/spine-in-depth) has [Unity Attributes](http://en.esotericsoftware.com/spine-unity) like `SpineAnimation`,
    but it has some limit, e.g. it can not be used on string, it can not report an error if the target is changed, mismatch with skeleton or missing etc.

    `SainsField`'s spine attributes allow more robust references, and are supported by `Auto Validator` tool, with searching supported.
2.  Rename `Auto Runner` to `Auto Validator`
3.  Add `Scene` validation in `Auto Validator`

## 3.20.0 ##

1.  Add `ShaderParam` to select a shader parameter from a `shader`, `material` or `renderer`
2.  Add `ShaderKeyword` to select a shader keyword from a `shader`, `material` or `renderer`

## 3.19.3 ##

1.  UI Toolkit: if the color is not in the `ColorPalette`, it will give a warning icon.
2.  `ColorPalette` now supported by auto runner validation.
3.  Fix auto runner extra resources won't work with GameObject type.
4.  UI Toolkit: fix `Button` IEnumerator broken
5.  IMGUI: `ColorPalette` is now supported in IMGUI

## 3.19.2 ##

UI Toolkit: fix button Enumerator

## 3.19.1 ##

UI Toolkit: fix fallback drawer with decorator attribute(s).

## 3.19.0 ##

1.  UI Toolkit: Add `ColorPalette` to pick a color from a list of colors (IMGUI support will be added later)
2.  IMGUI: Fix drawer fallback issue [#119](https://github.com/TylerTemp/SaintsField/issues/119)
3.  UI Toolkit: Fix drawer can not fall back if a third party attribute is in the middle of attributes
4.  Fix `Button` gives errors when using non-dynamic label with some `Playa*` attributes

## 3.18.1 ##

1.  `Button` label now support rich text, and also support dynamic callback text
2.  Fix auto getters gives errors when the list/array need to reduce the size

## 3.18.0 ##

1.  Auto Runner now can check the `OnValidate` method, and will notice you if the method throw an error and/or make a `Debug.LogError`
2.  Fix auto getters throw an error if the target asset doesn't match some required condition
3.  Auto Runner no longer serialize the result into the file
4.  Fix Auto Runner can not properly display the field because the scene gets close during the process
5.  Auto Runner now will allow you restore your opened scenes after finished
6.  Add an [Auto Runner example code](https://github.com/TylerTemp/SaintsField/blob/master/Editor/AutoRunner/AutoRunnerTemplate.cs) so you can easily make a target assets group for validation

## 3.17.1 ##

1.  Fix auto getters won't work if multiple targets use the same field name for array/list
2.  IMGUI: `AddressableResource` is not supported in IMGUI too

## 3.17.0 ##

1.  `ValidateInput` is now supported by Auto Runner.
2.  `AboveImage`, `BelowImage`, `AssetPreview` now works on Addressable `AssetReference` type.
3.  UI Toolkit: `ValidateInput` now only run when:
    1.  the inspecting target is changed
    2.  the project is changed
    3.  some assets are changed

    instead of every 100 ms. You can still enable the loop checking in `SaintsConfig`
4.  Change Auto Runner so you can have multiple config for different purposes.

## 3.16.0 ##

1.  Auto runner now supports auto getters!
2.  UI Toolkit: fix auto getters update loop won't get triggered
3.  Fix auto getters can not process `SaintsInterface` correctly
4.  Improve the performance of auto runner, also give a dialog message if the opening scene is dirty

## 3.15.1 ##

1. Better `Auto Runner` serialization and drawer
2. IMGUI: Fix auto getters might fail when refreshing the resources
3. IMGUI: All kind of buttons now support `IEnumerator` return type (same as UI Toolkit)
4. IMGUI: `Auto Runner` is now supported in IMGUI
5. IMGUI: `Layout` with border or foldout, can now indent a bit for array and generic type, to increase the readability
6. IMGUI: `Layout` foldout icon is now in gray color instead of white

## 3.15.0 ##

1.  UI Toolkit: Add `AddressableResource` for `AssetReference` inline editing
2.  Using `Required` on addressable's `AssetReference` will check if the target asset is valid
3.  UI Toolkit: `AutoRunner` now can specify if you want to skip the hidden fields (hidden by `ShowIf`, `HideIf`. Not work for `LayoutShowIf`, `LayoutHideIf`)

## 3.14.1 ##

1.  Overhaul the auto getters. This might be related to [#102](https://github.com/TylerTemp/SaintsField/issues/102).

    The issue is because, usually you can just `SerializedProperty.SerializedObject.ApplyModifiedProperties()`. However, if the serialized target is newly created, this behavior will fail, for no good reason. This behavior is not documented in Unity's API.

    To resolve this, UI Toolkit will delay the action a bit later. IMGUI will attempt multiple times.

    This version also makes the auto getters for list not depending on the first element drawer. This means three things:

    1.  When working with `ListDrawerSettings`, search function will not trigger the auto getters re-running.
    2.  Draging element in list will not cause any troubles now. But the value will swap back once auto getters auto updated. (auto updating is depended on your configuration)
    3.  Better performance, especially for IMGUI.

2.  UI Toolkit: A simple validation tool under `Window` - `Saints` - `Auto Runner`, related to [#115](https://github.com/TylerTemp/SaintsField/discussions/115)

    This tool allows you to check if some target has `Required` but not filled. You can specify the targets as you want. Currently, it supports scenes, and folder searching.

    This tool is very simple, and will get more update in the future.

    This tool is only available for UI Toolkit at the moment.

3.  **Breaking Changes**: If you use `IWrapProp`, you need to change

    ```csharp
    // an auto getter
    private string EditorPropertyName => nameof(myField);
    ```

    to

    ```csharp
    // a static field (private or public, with or without readonly)
    private static readonly string EditorPropertyName = nameof(myField);
    ```

## 3.13.1 ##

1.  UI Toolkit: Fix `SaintsEditor` created many unused empty `VisualElement`
2.  UI Toolkit: Fix rich label style might be null when falling back to IMGUI drawer
3.  UI Toolkit: Fix `SaintsEditorWindow` can not vertically scroll when the window is very high

## 3.13.0 ##

1.  Add `AssetFolder` to pick a folder under `Assets` folder.
2.  Add `ResourceFolder` to pick a folder under Unity's `Resources` folders

## 3.12.1 ##

1.  Fix `RequireType` didn't give a correct component when using with `interface`
2.  Fix `ShowInInspector` sometimes can not draw a correct value when a nested field is null

## 3.12.0 ##

1.  Add `SaintsEditorWindow` to easily make an `EditorWindow`.
2.  IMGUI: Fix `OnValueChanged` didn't work with `Dropdown` and `AdvancedDropdown`

## 3.11.0 ##

1.  Add `FlagsDropdown` to toggle flags with search support, [#96](https://github.com/TylerTemp/SaintsField/issues/96)
2.  `AnimatorState` now supports `RuntimeAnimatorController` type, [#113](https://github.com/TylerTemp/SaintsField/discussions/113)
3.  IMGUI: Fix `EnumFlags` does not support rich text

## 3.10.0 ##

1.  Add `DrawLine` to draw a line between different objects
2.  Add `ArrowHandleCap` to draw an arrow between different objects without `SaintsDraw` installed
3.  Add `GUIColor` to color a field, [#107](https://github.com/TylerTemp/SaintsField/issues/107)

## 3.9.1 ##

1.  IMGUI: Fix `DrawLabel` won't work on array/list
2.  IMGUI: `SaintsArrow` is now available in IMGUI
3.  Fix `ReferencePicker` didn't work if the definition is inside a generic class etc. [#112](https://github.com/TylerTemp/SaintsField/issues/112)

## 3.9.0 ##

1.  UI Toolkit: Add `SaintsArrow` to draw arrows in the scene (IMGUI support will be added later)
2.  UI Toolkit: Fix auto-getters get looped calls when changing ordered in an array/list

## 3.8.0 ##

1.  Add `PositionHandle` which can change position of target field in scene view. The target can be either a `GameObject`, a `Component`, or a Vector2/Vector3 target.
2.  IMGUI: fix `DrawLabel` won't disappear when you select away
3.  UI Toolkit: fix auto-getters might get looped calls in list/array
4.  UI Toolkit: fix `AnimatorParam` can't display the correct label with `RichLabel`
5.  `DrawLabel` now support to draw a label for a `Vector2` or `Vector3` field with `Space` argument
6.  `AnimatorState` now support `AnimatorOverrideController`

## 3.7.2 ##

UI Toolkit: Fix `PlayaInfoBox` won't hide when `show` returns `false`

## 3.7.1 ##

1.  UI Toolkit: All the buttons now support `Coroutine`. If the target function returns an `IEnumerator`, the button will start a coroutine and wait for it to finish.
2.  UI Toolkit: Fix `ProgressBar` won't display updated value if the value is changed externally.

## 3.7.0 ##

1.  Add `LayoutShowIf`, `LayoutHideIf`, `LayoutEnableIf`, `LayoutDisableIf` to toggle show/enable status of an entire layout group. [#100](https://github.com/TylerTemp/SaintsField/issues/100), [#73](https://github.com/TylerTemp/SaintsField/issues/73)
2.  Fix auto getter accesses disposed property in some cases in `SaintsEditor`

## 3.6.1 ##

1.  IMGUI: Fix accessing disposed `SerializedProperty`, [#102](https://github.com/TylerTemp/SaintsField/issues/102)
2.  IMGUI: Split config for auto getters from UI Toolkit, and change the default behavior of IMGUI auto getters to be never update while on inspector (same as old behavior of auto getters). Might be related to [#98](https://github.com/TylerTemp/SaintsField/issues/98)
3.  IMGUI: Fix `RichLabel` has some indent and truncate issue with `LeftToggle` and `ResiziableTextArea`
4.  Fix Auto Getters won't work if you disabled the update
5.  UI Toolkit: Remove some unnecessary call to improve some performance
6.  UI Toolkit: Now scene view will notice you if there is an auto-getter signed a value to a field.

## 3.6.0 ##

1.  Fix auto getters `includeInactive` checked the `gameObject` itself is enabled, but should be `activeInHierarchy`, [#103](https://github.com/TylerTemp/SaintsField/issues/103).
2.  Add `DrawLabel` handle to draw label in the scene view, [#95](https://github.com/TylerTemp/SaintsField/issues/95)
3.  Improve the logic of how `SaintsField Config` is loaded to reduce the times of loading the config.
4.  UI Toolkit: fix auto getters won't work if you completely disable the update loop.

Since this version we start to use the `semantic versioning` for version number.

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
