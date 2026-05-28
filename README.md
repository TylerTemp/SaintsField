# SaintsField #

[![unity_version](https://github.com/user-attachments/assets/dffbf530-6212-481b-bfdb-1e9d9ce3712d)](https://unity.com/download)
[![license_mit](https://github.com/TylerTemp/SaintsField/assets/6391063/a093811a-5dbc-46ad-939e-a9e207ae5bfb)](https://github.com/TylerTemp/SaintsField/blob/master/LICENSE)
[![openupm](https://img.shields.io/npm/v/today.comes.saintsfield?label=OpenUPM&registry_uri=https://package.openupm.com)](https://openupm.com/packages/today.comes.saintsfield/)
[![Percentage of issues still open](https://isitmaintained.com/badge/open/TylerTemp/SaintsField.svg)](http://isitmaintained.com/project/TylerTemp/SaintsField "Percentage of issues still open")
[![Average time to resolve an issue](https://isitmaintained.com/badge/resolution/TylerTemp/SaintsField.svg)](http://isitmaintained.com/project/TylerTemp/SaintsField "Average time to resolve an issue")
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=Downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Ftoday.comes.saintsfield)](https://openupm.com/packages/today.comes.saintsfield/)
[![repo-stars](https://img.shields.io/github/stars/TylerTemp/SaintsField)](https://github.com/TylerTemp/SaintsField/)

`SaintsField` is a Unity extension tool for enhancing inspector and data serialization.

Developed by: [TylerTemp](https://github.com/TylerTemp), [墨瞳](https://github.com/xc13308)

Unity: 2022.2 or higher

(Yes, the project name comes from, of course, [Saints Row 2](https://saintsrow.fandom.com/wiki/Saints_Row_2))

![](https://github.com/user-attachments/assets/3088cb89-742a-4c13-86b7-9e2afa78f327)

![](https://github.com/user-attachments/assets/e91dd9f1-96c4-4e45-9161-bd8eeecc3b1e)

![](https://github.com/user-attachments/assets/7223f0f3-af1e-4900-9516-fd9cb4cddcfc)

![](https://github.com/user-attachments/assets/7c053391-15aa-4c0c-af19-1ec4209e73c6)

![](https://github.com/user-attachments/assets/b19d67a2-c39f-4ff8-9a92-310bdac42d27)

![](https://github.com/user-attachments/assets/b6ce0687-eba4-49e6-97f3-4cbd54f9f07b)

![](https://github.com/user-attachments/assets/7cb7901f-eeb1-40b4-b72c-3458dd3a4b87)

## Getting Started ##

### Highlights ###

1.  Works on deep nested fields!
2.  When a target is drawn by the old IMGUI drawer, it will be rendered correctly inside UI Toolkit.
3.  Allow stack on many cases. Only attributes that modified the label itself, and the field itself can not be stacked. All other attributes can mostly be stacked.
4.  Allow dynamic arguments in many cases
5.  Directly serialize dictionary, interface, hashset and more
6.  Easily group different fields with box

SaintsField is the [Top 3 updated Unity Package](https://openupm.com/blog/openupm-2025-recap-6283fcd0217e/) in 2025 openupm. Hooray!

![](https://github.com/user-attachments/assets/82666e9a-268e-46f8-8413-fe57b90d854a)

### Installation ###

*   Using [Unity Asset Store](https://assetstore.unity.com/packages/slug/269741)

*   Using [OpenUPM](https://openupm.com/packages/today.comes.saintsfield/)

    ```bash
    openupm add today.comes.saintsfield
    ```

*   Using git upm:

    add to `Packages/manifest.json` in your project

    ```javascript
    {
        "dependencies": {
            "today.comes.saintsfield": "https://github.com/TylerTemp/SaintsField.git",
            // your other dependencies...
        }
    }
    ```

*   Using git upm (Unity UI):

    1. `Window` - `Package Manager`
    2. Click `+` button, `Add package from git URL`
    3. Enter the following URL:

    ```
    https://github.com/TylerTemp/SaintsField.git
    ```


*   Using a `unitypackage`:

    Go to the [Release Page](https://github.com/TylerTemp/SaintsField/releases) to download a desired version of `unitypackage` and import it to your project

*   Using a git submodule:

    ```bash
    git submodule add https://github.com/TylerTemp/SaintsField.git Packages/today.comes.saintsfield
    ```

If you have DOTween installed
*   Please also ensure you do: `Tools` - `Demigaint` - `DOTween Utility Panel`, click `Create ASMDEF`
*   Or disable related functions with `Tools` - `Saints Field` - `Disable DOTween Support`
*   If you can not find this menu, please read the "Add a Macro" section about how to manually disable DOTween support in SaintsField.

[**Optional**] To use the full functions of this project, please also do: `Tools` - `Saints Field` - `Enable SaintsEditor`. Note this will break your existing Editor plugin like `OdinInspector`, `NaughtyAttributes`, `MyToolbox`, `Tri-Inspector`.

If you need to put this project under another folder rather than `Packages/today.comes.saintsfield`, please also do the following:

*   Create `Assets/Editor Default Resources/SaintsField`.
*   Copy files from the project's `Editor/Editor Default Resources/SaintsField` into your project's `Assets/Editor Default Resources/SaintsField`.
    If you're using a file browser instead of Unity's project tab to copy files, you may want to exclude the `.meta` file to avoid GUID conflict.

**Troubleshoot**

After installation, you can use `Tools` - `Saints Field` - `Troubleshoot` to check if some attributes do not work.

namespace: `SaintsField`

## Change Log ##

**5.17.3**

1.  Fix: Align logic of field button with `Button`
2.  Add: `OnEvent` and `OnButtonClick` now has a UI so you can see where it's been bind, and shows error if the bind failed

## Usage ##

*   General Attributes
    *   Label & Text
        *   [`LabelText`](https://saintsfield.comes.today/general-attributes/label--text/labeltext)
        *   [`FieldLabelText`](https://saintsfield.comes.today/general-attributes/label--text/fieldlabeltext)
        *   [`NoLabel`](https://saintsfield.comes.today/general-attributes/label--text/nolabel)
        *   [`AboveText` / `BelowText`](https://saintsfield.comes.today/general-attributes/label--text/abovetext--belowtext)
        *   [`FieldAboveText` / `FieldBelowText`](https://saintsfield.comes.today/general-attributes/label--text/fieldabovetext--fieldbelowtext)
        *   [`OverlayText`](https://saintsfield.comes.today/general-attributes/label--text/overlaytext)
        *   [`EndText`](https://saintsfield.comes.today/general-attributes/label--text/endtext)
        *   [`InfoBox`/`BelowInfoBox`](https://saintsfield.comes.today/general-attributes/label--text/infoboxbelowinfobox)
        *   [`FieldInfoBox`/`FieldBelowInfoBox`](https://saintsfield.comes.today/general-attributes/label--text/fieldinfoboxfieldbelowinfobox)
        *   [`Separator`/`BelowSeparator`](https://saintsfield.comes.today/general-attributes/label--text/separatorbelowseparator)
        *   [`FieldSeparator` / `FieldBelowSeparator`](https://saintsfield.comes.today/general-attributes/label--text/fieldseparator--fieldbelowseparator)
        *   [`SepTitle`](https://saintsfield.comes.today/general-attributes/label--text/septitle)
        *   [`GUIColor`](https://saintsfield.comes.today/general-attributes/label--text/guicolor)
    *   Button
        *   [`Button`](https://saintsfield.comes.today/general-attributes/button/button)
        *   [`AboveButton`/`BelowButton`/`PostFieldButton`](https://saintsfield.comes.today/general-attributes/button/abovebuttonbelowbuttonpostfieldbutton)
    *   Game Related
        *   [`Layer`](https://saintsfield.comes.today/general-attributes/game-related/layer)
        *   [`Scene`](https://saintsfield.comes.today/general-attributes/game-related/scene)
        *   [`SortingLayer`](https://saintsfield.comes.today/general-attributes/game-related/sortinglayer)
        *   [`Tag`](https://saintsfield.comes.today/general-attributes/game-related/tag)
        *   [`InputAxis`](https://saintsfield.comes.today/general-attributes/game-related/inputaxis)
        *   [`ShaderParam`](https://saintsfield.comes.today/general-attributes/game-related/shaderparam)
        *   [`ShaderKeyword`](https://saintsfield.comes.today/general-attributes/game-related/shaderkeyword)
    *   Toggle & Switch
        *   [`GameObjectActive`](https://saintsfield.comes.today/general-attributes/toggle--switch/gameobjectactive)
        *   [`SpriteToggle`](https://saintsfield.comes.today/general-attributes/toggle--switch/spritetoggle)
        *   [`MaterialToggle`](https://saintsfield.comes.today/general-attributes/toggle--switch/materialtoggle)
        *   [`ColorToggle`](https://saintsfield.comes.today/general-attributes/toggle--switch/colortoggle)
    *   Data Editor
        *   [`Expandable`](https://saintsfield.comes.today/general-attributes/data-editor/expandable)
        *   [`ReferencePicker`](https://saintsfield.comes.today/general-attributes/data-editor/referencepicker)
        *   [`SaintsRow`](https://saintsfield.comes.today/general-attributes/data-editor/saintsrow)
        *   [`ListDrawerSettings`](https://saintsfield.comes.today/general-attributes/data-editor/listdrawersettings)
        *   [`Table`](https://saintsfield.comes.today/general-attributes/data-editor/table)
        *   [`ShowInInspector`](https://saintsfield.comes.today/general-attributes/data-editor/showininspector)
    *   Numerical
        *   [`Rate`](https://saintsfield.comes.today/general-attributes/numerical/rate)
        *   [`PropRange`](https://saintsfield.comes.today/general-attributes/numerical/proprange)
        *   [`MinMaxSlider`](https://saintsfield.comes.today/general-attributes/numerical/minmaxslider)
        *   [`ProgressBar`](https://saintsfield.comes.today/general-attributes/numerical/progressbar)
    *   Animation
        *   [`AnimatorParam`](https://saintsfield.comes.today/general-attributes/animation/animatorparam)
        *   [`AnimatorState`](https://saintsfield.comes.today/general-attributes/animation/animatorstate)
        *   [`CurveRange`](https://saintsfield.comes.today/general-attributes/animation/curverange)
    *   [Auto Getter](https://saintsfield.comes.today/general-attributes/auto-getter)
        *   [`GetComponent`](https://saintsfield.comes.today/general-attributes/auto-getter/getcomponent)
        *   [`GetComponentInChildren`/`GetInChildren`](https://saintsfield.comes.today/general-attributes/auto-getter/getcomponentinchildrengetinchildren)
        *   [`GetComponentInParent` / `GetComponentInParents`](https://saintsfield.comes.today/general-attributes/auto-getter/getcomponentinparent--getcomponentinparents)
        *   [`FindObjectsByType`/`GetInScene`](https://saintsfield.comes.today/general-attributes/auto-getter/findobjectsbytypegetinscene)
        *   [`GetPrefabWithComponent`](https://saintsfield.comes.today/general-attributes/auto-getter/getprefabwithcomponent)
        *   [`GetScriptableObject`](https://saintsfield.comes.today/general-attributes/auto-getter/getscriptableobject)
        *   [`GetInSiblings`](https://saintsfield.comes.today/general-attributes/auto-getter/getinsiblings)
        *   [`GetByXPath`](https://saintsfield.comes.today/general-attributes/auto-getter/getbyxpath)
        *   [`GetMainCamera`](https://saintsfield.comes.today/general-attributes/auto-getter/getmaincamera)
        *   [`AddComponent`](https://saintsfield.comes.today/general-attributes/auto-getter/addcomponent)
        *   [`FindComponent`](https://saintsfield.comes.today/general-attributes/auto-getter/findcomponent)
        *   [`GetComponentByPath`](https://saintsfield.comes.today/general-attributes/auto-getter/getcomponentbypath)
    *   Validate & Restrict
        *   [`FieldType`](https://saintsfield.comes.today/general-attributes/validate--restrict/fieldtype)
        *   [`OnValueChanged`](https://saintsfield.comes.today/general-attributes/validate--restrict/onvaluechanged)
        *   [`OnArraySizeChanged`](https://saintsfield.comes.today/general-attributes/validate--restrict/onarraysizechanged)
        *   [`ReadOnly`/`DisableIf`/`EnableIf`](https://saintsfield.comes.today/general-attributes/validate--restrict/readonlydisableifenableif)
        *   [`FieldEnableIf`/`FieldDisableIf`/`FieldReadOnly`](https://saintsfield.comes.today/general-attributes/validate--restrict/fieldenableiffielddisableiffieldreadonly)
        *   [`ShowIf`/`HideIf`](https://saintsfield.comes.today/general-attributes/validate--restrict/showifhideif)
        *   [`FieldShowIf` / `FieldHideIf`](https://saintsfield.comes.today/general-attributes/validate--restrict/fieldshowif--fieldhideif)
        *   [`Required`](https://saintsfield.comes.today/general-attributes/validate--restrict/required)
        *   [`RequiredIf`](https://saintsfield.comes.today/general-attributes/validate--restrict/requiredif)
        *   [`ValidateInput`](https://saintsfield.comes.today/general-attributes/validate--restrict/validateinput)
        *   [`MinValue` / `MaxValue`](https://saintsfield.comes.today/general-attributes/validate--restrict/minvalue--maxvalue)
        *   [`RequireType`](https://saintsfield.comes.today/general-attributes/validate--restrict/requiretype)
        *   [`ArraySize`](https://saintsfield.comes.today/general-attributes/validate--restrict/arraysize)
    *   Miscellaneous
        *   [`Dropdown`](https://saintsfield.comes.today/general-attributes/miscellaneous/dropdown)
        *   [`OptionsDropdown` / `PairsDropdown`](https://saintsfield.comes.today/general-attributes/miscellaneous/optionsdropdown--pairsdropdown)
        *   [`FlagsDropdown`](https://saintsfield.comes.today/general-attributes/miscellaneous/flagsdropdown)
        *   [`AdvancedDropdown`](https://saintsfield.comes.today/general-attributes/miscellaneous/advanceddropdown)
        *   [`AdvancedOptionsDropdown` / `AdvancedPairsDropdown`](https://saintsfield.comes.today/general-attributes/miscellaneous/advancedoptionsdropdown--advancedpairsdropdown)
        *   [`MenuDropdown`](https://saintsfield.comes.today/general-attributes/miscellaneous/menudropdown)
        *   [`CustomContextMenu`](https://saintsfield.comes.today/general-attributes/miscellaneous/customcontextmenu)
        *   [`FieldCustomContextMenu`](https://saintsfield.comes.today/general-attributes/miscellaneous/fieldcustomcontextmenu)
        *   [`ValueButtons`](https://saintsfield.comes.today/general-attributes/miscellaneous/valuebuttons)
        *   [`OptionsValueButtons` / `PairsValueButtons`](https://saintsfield.comes.today/general-attributes/miscellaneous/optionsvaluebuttons--pairsvaluebuttons)
        *   [`EnumToggleButtons`](https://saintsfield.comes.today/general-attributes/miscellaneous/enumtogglebuttons)
        *   [`ResizableTextArea`](https://saintsfield.comes.today/general-attributes/miscellaneous/resizabletextarea)
        *   [`LeftToggle`](https://saintsfield.comes.today/general-attributes/miscellaneous/lefttoggle)
        *   [`ResourcePath`](https://saintsfield.comes.today/general-attributes/miscellaneous/resourcepath)
        *   [`ResourceFolder`](https://saintsfield.comes.today/general-attributes/miscellaneous/resourcefolder)
        *   [`FieldDefaultExpand`](https://saintsfield.comes.today/general-attributes/miscellaneous/fielddefaultexpand)
        *   [`DefaultExpand`](https://saintsfield.comes.today/general-attributes/miscellaneous/defaultexpand)
        *   [`AssetFolder`](https://saintsfield.comes.today/general-attributes/miscellaneous/assetfolder)
        *   [`AssetPreview`](https://saintsfield.comes.today/general-attributes/miscellaneous/assetpreview)
        *   [`AboveImage`/`BelowImage`](https://saintsfield.comes.today/general-attributes/miscellaneous/aboveimagebelowimage)
        *   [`ParticlePlay`](https://saintsfield.comes.today/general-attributes/miscellaneous/particleplay)
        *   [`ButtonAddOnClick`](https://saintsfield.comes.today/general-attributes/miscellaneous/buttonaddonclick)
        *   [`OnButtonClick`](https://saintsfield.comes.today/general-attributes/miscellaneous/onbuttonclick)
        *   [`OnEvent`](https://saintsfield.comes.today/general-attributes/miscellaneous/onevent)
        *   [`ColorPalette`](https://saintsfield.comes.today/general-attributes/miscellaneous/colorpalette)
        *   [`Searchable`](https://saintsfield.comes.today/general-attributes/miscellaneous/searchable)
        *   [`DateTime`](https://saintsfield.comes.today/general-attributes/miscellaneous/datetime)
        *   [`TimeSpan`](https://saintsfield.comes.today/general-attributes/miscellaneous/timespan)
        *   [`Guid`](https://saintsfield.comes.today/general-attributes/miscellaneous/guid)
*   Layout System
    *   [Overview](https://saintsfield.comes.today/layout-system/overview)
    *   [`Layout`](https://saintsfield.comes.today/layout-system/layout)
    *   [`LayoutStart` / `LayoutEnd`](https://saintsfield.comes.today/layout-system/layoutstart--layoutend)
    *   [`LayoutCloseHere` / `LayoutTerminateHere`](https://saintsfield.comes.today/layout-system/layoutclosehere--layoutterminatehere)
    *   [`LayoutDisableIf` / `LayoutEnableIf`](https://saintsfield.comes.today/layout-system/layoutdisableif--layoutenableif)
    *   [`LayoutShowIf` / `LayoutHideIf`](https://saintsfield.comes.today/layout-system/layoutshowif--layouthideif)
*   [Handles](https://saintsfield.comes.today/handles)
    *   [`SceneViewPicker`](https://saintsfield.comes.today/handles/sceneviewpicker)
    *   [`DrawLabel`](https://saintsfield.comes.today/handles/drawlabel)
    *   [`PositionHandle`](https://saintsfield.comes.today/handles/positionhandle)
    *   [`RotationHandle`](https://saintsfield.comes.today/handles/rotationhandle)
    *   [`ScaleHandle`](https://saintsfield.comes.today/handles/scalehandle)
    *   [`DrawLine`](https://saintsfield.comes.today/handles/drawline)
    *   [`SaintsArrow`](https://saintsfield.comes.today/handles/saintsarrow)
    *   [`ArrowHandleCap`](https://saintsfield.comes.today/handles/arrowhandlecap)
    *   [`DrawWireDisc`](https://saintsfield.comes.today/handles/drawwiredisc)
    *   [`SphereHandleCap`](https://saintsfield.comes.today/handles/spherehandlecap)
    *   [`RadiusHandle`](https://saintsfield.comes.today/handles/radiushandle)
    *   [`PrimitiveBoundsHandle`](https://saintsfield.comes.today/handles/primitiveboundshandle)
*   [Component Header](https://saintsfield.comes.today/component-header)
    *   [Runtime Saver](https://saintsfield.comes.today/component-header/runtime-saver)
    *   [`HeaderButton` / `HeaderLeftButton`](https://saintsfield.comes.today/component-header/headerbutton--headerleftbutton)
    *   [`HeaderGhostButton` / `HeaderGhostLeftButton`](https://saintsfield.comes.today/component-header/headerghostbutton--headerghostleftbutton)
    *   [`HeaderLabel` / `HeaderLeftLabel`](https://saintsfield.comes.today/component-header/headerlabel--headerleftlabel)
    *   [`HeaderDraw` / `HeaderLeftDraw`](https://saintsfield.comes.today/component-header/headerdraw--headerleftdraw)
*   Data Types
    *   [`SaintsArray`/`SaintsList`](https://saintsfield.comes.today/data-types/saintsarraysaintslist)
    *   [`SaintsDictionary<,>`](https://saintsfield.comes.today/data-types/saintsdictionary)
    *   [`SaintsInterface<>`](https://saintsfield.comes.today/data-types/saintsinterface)
    *   [`SaintsHashSet<>` / `ReferenceHashSet<>`](https://saintsfield.comes.today/data-types/saintshashset--referencehashset)
    *   [`SaintsDecimal`](https://saintsfield.comes.today/data-types/saintsdecimal)
    *   [`TypeReference`](https://saintsfield.comes.today/data-types/typereference)
    *   [`SaintsEvent`](https://saintsfield.comes.today/data-types/saintsevent)
    *   [`SceneReference`](https://saintsfield.comes.today/data-types/scenereference)
*   [Addressable](https://saintsfield.comes.today/addressable)
    *   [`AddressableLabel`](https://saintsfield.comes.today/addressable/addressablelabel)
    *   [`AddressableAddress`](https://saintsfield.comes.today/addressable/addressableaddress)
    *   [`AddressableResource`](https://saintsfield.comes.today/addressable/addressableresource)
    *   [`AddressableScene`](https://saintsfield.comes.today/addressable/addressablescene)
    *   [`AddressableSubAssetRequired` ##](https://saintsfield.comes.today/addressable/addressablesubassetrequired-)
*   [AI Navigation](https://saintsfield.comes.today/ai-navigation)
    *   [`NavMeshAreaMask`](https://saintsfield.comes.today/ai-navigation/navmeshareamask)
    *   [`NavMeshArea`](https://saintsfield.comes.today/ai-navigation/navmesharea)
*   [Spine](https://saintsfield.comes.today/spine)
    *   [`SpineAnimationPicker`](https://saintsfield.comes.today/spine/spineanimationpicker)
    *   [`SpineSkinPicker`](https://saintsfield.comes.today/spine/spineskinpicker)
    *   [`SpineSlotPicker`](https://saintsfield.comes.today/spine/spineslotpicker)
    *   [`SpineAttachmentPicker`](https://saintsfield.comes.today/spine/spineattachmentpicker)
    *   [`SpineBonePicker`](https://saintsfield.comes.today/spine/spinebonepicker)
    *   [`SpineEventPicker`](https://saintsfield.comes.today/spine/spineeventpicker)
    *   [`SpineIkConstraintPicker`](https://saintsfield.comes.today/spine/spineikconstraintpicker)
    *   [`SpinePathConstraintPicker`](https://saintsfield.comes.today/spine/spinepathconstraintpicker)
    *   [`SpineTransformConstraintPicker`](https://saintsfield.comes.today/spine/spinetransformconstraintpicker)
*   DOTween
    *   [`DOTweenPlay`](https://saintsfield.comes.today/dotween/dotweenplay)
*   [Wwise](https://saintsfield.comes.today/wwise)
    *   [`GetWwise`](https://saintsfield.comes.today/wwise/getwwise)
*   [I2 Localization](https://saintsfield.comes.today/i-localization)
    *   [`LocalizedStringPicker`](https://saintsfield.comes.today/i-localization/localizedstringpicker)
*   [SaintsEditor](https://saintsfield.comes.today/saintseditor)
    *   [Setup](https://saintsfield.comes.today/saintseditor/setup)
    *   [Inherent](https://saintsfield.comes.today/saintseditor/inherent)
    *   [Extend](https://saintsfield.comes.today/saintseditor/extend)
    *   [Integerate](https://saintsfield.comes.today/saintseditor/integerate)
    *   [Netcode for Game Objects](https://saintsfield.comes.today/saintseditor/netcode-for-game-objects)
    *   [Scriptable Renderer Data](https://saintsfield.comes.today/saintseditor/scriptable-renderer-data)
    *   [`Unity.Mathematics`](https://saintsfield.comes.today/saintseditor/unitymathematics)
    *   [SaintsBuild Support ##](https://saintsfield.comes.today/saintseditor/saintsbuild-support-)
*   [Extended Serialization](https://saintsfield.comes.today/extended-serialization)
    *   [`Dictionary<,>`](https://saintsfield.comes.today/extended-serialization/dictionary)
    *   [`HashSet<>`](https://saintsfield.comes.today/extended-serialization/hashset)
    *   [`interface`](https://saintsfield.comes.today/extended-serialization/interface)
    *   [`long`/`ulong` Enum](https://saintsfield.comes.today/extended-serialization/longulong-enum)
    *   [`DateTime`](https://saintsfield.comes.today/extended-serialization/datetime)
    *   [`TimeSpan`](https://saintsfield.comes.today/extended-serialization/timespan)
    *   [`Guid`](https://saintsfield.comes.today/extended-serialization/guid)
    *   [`decimal`](https://saintsfield.comes.today/extended-serialization/decimal)
*   [`SaintsEditorWindow`](https://saintsfield.comes.today/saintseditorwindow)
    *   [Usage & Example](https://saintsfield.comes.today/saintseditorwindow/usage--example)
    *   [Life Cycle & Functions](https://saintsfield.comes.today/saintseditorwindow/life-cycle--functions)
    *   [`WindowInlineEditor`](https://saintsfield.comes.today/saintseditorwindow/windowinlineeditor)
*   Misc
    *   [About GroupBy](https://saintsfield.comes.today/misc/about-groupby)
    *   [`EMode`](https://saintsfield.comes.today/misc/emode)
    *   [Callback](https://saintsfield.comes.today/misc/callback)
    *   [Syntax for Show/Hide/Enable/Disable/Required-If](https://saintsfield.comes.today/misc/syntax-for-showhideenabledisablerequired-if)
    *   Saints XPath-like Syntax
        *   [XPath](https://saintsfield.comes.today/misc/saints-xpath-like-syntax/xpath)
        *   [`EXP`](https://saintsfield.comes.today/misc/saints-xpath-like-syntax/exp)
    *   [Add a Macro](https://saintsfield.comes.today/misc/add-a-macro)
    *   [Auto Validator](https://saintsfield.comes.today/misc/auto-validator)
    *   [Use With Other Drawers](https://saintsfield.comes.today/misc/use-with-other-drawers)

## Donation ##

### Donation Link ###

PayPal: [![Image](https://github.com/user-attachments/assets/af35c913-151f-463d-9635-e562683b1ce8)](https://www.paypal.com/donate/?hosted_button_id=B38BUN42VQ73N)

### Donation List ###

Thanks for the following generous donors:

- [bilemedimkq](https://github.com/bilemedimkq) donated on 2025-09-17
