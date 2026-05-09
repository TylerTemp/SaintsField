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

### Change Log ###

Fix: The callback might get an old (not up to date) value when manually assign a serializable class value [#387](https://github.com/TylerTemp/SaintsField/issues/387)

## Usage ##

*   General Attributes
    *   Label & Text
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/labeltext" target="_blank">`LabelText`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/fieldlabeltext" target="_blank">`FieldLabelText`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/nolabel" target="_blank">`NoLabel`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/abovetext--belowtext" target="_blank">`AboveText` / `BelowText`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/fieldabovetext--fieldbelowtext" target="_blank">`FieldAboveText` / `FieldBelowText`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/overlaytext" target="_blank">`OverlayText`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/endtext" target="_blank">`EndText`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/infoboxbelowinfobox" target="_blank">`InfoBox`/`BelowInfoBox`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/fieldinfoboxfieldbelowinfobox" target="_blank">`FieldInfoBox`/`FieldBelowInfoBox`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/separatorbelowseparator" target="_blank">`Separator`/`BelowSeparator`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/fieldseparator--fieldbelowseparator" target="_blank">`FieldSeparator` / `FieldBelowSeparator`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/septitle" target="_blank">`SepTitle`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/guicolor" target="_blank">`GUIColor`</a>
    *   Button
        *   <a href="https://saintsfield.comes.today/general-attributes/button/button" target="_blank">`Button`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/button/abovebuttonbelowbuttonpostfieldbutton" target="_blank">`AboveButton`/`BelowButton`/`PostFieldButton`</a>
    *   Game Related
        *   <a href="https://saintsfield.comes.today/general-attributes/game-related/layer" target="_blank">`Layer`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/game-related/scene" target="_blank">`Scene`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/game-related/sortinglayer" target="_blank">`SortingLayer`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/game-related/tag" target="_blank">`Tag`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/game-related/inputaxis" target="_blank">`InputAxis`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/game-related/shaderparam" target="_blank">`ShaderParam`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/game-related/shaderkeyword" target="_blank">`ShaderKeyword`</a>
    *   Toggle & Switch
        *   <a href="https://saintsfield.comes.today/general-attributes/toggle--switch/gameobjectactive" target="_blank">`GameObjectActive`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/toggle--switch/spritetoggle" target="_blank">`SpriteToggle`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/toggle--switch/materialtoggle" target="_blank">`MaterialToggle`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/toggle--switch/colortoggle" target="_blank">`ColorToggle`</a>
    *   Data Editor
        *   <a href="https://saintsfield.comes.today/general-attributes/data-editor/expandable" target="_blank">`Expandable`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/data-editor/referencepicker" target="_blank">`ReferencePicker`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/data-editor/saintsrow" target="_blank">`SaintsRow`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/data-editor/listdrawersettings" target="_blank">`ListDrawerSettings`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/data-editor/table" target="_blank">`Table`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/data-editor/showininspector" target="_blank">`ShowInInspector`</a>
    *   Numerical
        *   <a href="https://saintsfield.comes.today/general-attributes/numerical/rate" target="_blank">`Rate`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/numerical/proprange" target="_blank">`PropRange`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/numerical/minmaxslider" target="_blank">`MinMaxSlider`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/numerical/progressbar" target="_blank">`ProgressBar`</a>
    *   Animation
        *   <a href="https://saintsfield.comes.today/general-attributes/animation/animatorparam" target="_blank">`AnimatorParam`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/animation/animatorstate" target="_blank">`AnimatorState`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/animation/curverange" target="_blank">`CurveRange`</a>
    *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter" target="_blank">Auto Getter</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getcomponent" target="_blank">`GetComponent`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getcomponentinchildrengetinchildren" target="_blank">`GetComponentInChildren`/`GetInChildren`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getcomponentinparent--getcomponentinparents" target="_blank">`GetComponentInParent` / `GetComponentInParents`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/findobjectsbytypegetinscene" target="_blank">`FindObjectsByType`/`GetInScene`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getprefabwithcomponent" target="_blank">`GetPrefabWithComponent`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getscriptableobject" target="_blank">`GetScriptableObject`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getinsiblings" target="_blank">`GetInSiblings`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getbyxpath" target="_blank">`GetByXPath`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getmaincamera" target="_blank">`GetMainCamera`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/addcomponent" target="_blank">`AddComponent`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/findcomponent" target="_blank">`FindComponent`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getcomponentbypath" target="_blank">`GetComponentByPath`</a>
    *   Validate & Restrict
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/fieldtype" target="_blank">`FieldType`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/onvaluechanged" target="_blank">`OnValueChanged`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/onarraysizechanged" target="_blank">`OnArraySizeChanged`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/readonlydisableifenableif" target="_blank">`ReadOnly`/`DisableIf`/`EnableIf`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/fieldenableiffielddisableiffieldreadonly" target="_blank">`FieldEnableIf`/`FieldDisableIf`/`FieldReadOnly`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/showifhideif" target="_blank">`ShowIf`/`HideIf`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/fieldshowif--fieldhideif" target="_blank">`FieldShowIf` / `FieldHideIf`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/required" target="_blank">`Required`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/requiredif" target="_blank">`RequiredIf`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/validateinput" target="_blank">`ValidateInput`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/minvalue--maxvalue" target="_blank">`MinValue` / `MaxValue`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/requiretype" target="_blank">`RequireType`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/arraysize" target="_blank">`ArraySize`</a>
    *   Miscellaneous
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/dropdown" target="_blank">`Dropdown`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/optionsdropdown--pairsdropdown" target="_blank">`OptionsDropdown` / `PairsDropdown`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/flagsdropdown" target="_blank">`FlagsDropdown`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/advanceddropdown" target="_blank">`AdvancedDropdown`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/advancedoptionsdropdown--advancedpairsdropdown" target="_blank">`AdvancedOptionsDropdown` / `AdvancedPairsDropdown`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/menudropdown" target="_blank">`MenuDropdown`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/customcontextmenu" target="_blank">`CustomContextMenu`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/fieldcustomcontextmenu" target="_blank">`FieldCustomContextMenu`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/valuebuttons" target="_blank">`ValueButtons`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/optionsvaluebuttons--pairsvaluebuttons" target="_blank">`OptionsValueButtons` / `PairsValueButtons`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/enumtogglebuttons" target="_blank">`EnumToggleButtons`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/resizabletextarea" target="_blank">`ResizableTextArea`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/lefttoggle" target="_blank">`LeftToggle`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/resourcepath" target="_blank">`ResourcePath`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/resourcefolder" target="_blank">`ResourceFolder`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/fielddefaultexpand" target="_blank">`FieldDefaultExpand`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/defaultexpand" target="_blank">`DefaultExpand`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/assetfolder" target="_blank">`AssetFolder`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/assetpreview" target="_blank">`AssetPreview`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/aboveimagebelowimage" target="_blank">`AboveImage`/`BelowImage`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/particleplay" target="_blank">`ParticlePlay`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/buttonaddonclick" target="_blank">`ButtonAddOnClick`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/onbuttonclick" target="_blank">`OnButtonClick`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/onevent" target="_blank">`OnEvent`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/colorpalette" target="_blank">`ColorPalette`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/searchable" target="_blank">`Searchable`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/datetime" target="_blank">`DateTime`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/timespan" target="_blank">`TimeSpan`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/guid" target="_blank">`Guid`</a>
*   Layout System
    *   <a href="https://saintsfield.comes.today/layout-system/overview" target="_blank">Overview</a>
    *   <a href="https://saintsfield.comes.today/layout-system/layout" target="_blank">`Layout`</a>
    *   <a href="https://saintsfield.comes.today/layout-system/layoutstart--layoutend" target="_blank">`LayoutStart` / `LayoutEnd`</a>
    *   <a href="https://saintsfield.comes.today/layout-system/layoutclosehere--layoutterminatehere" target="_blank">`LayoutCloseHere` / `LayoutTerminateHere`</a>
    *   <a href="https://saintsfield.comes.today/layout-system/layoutdisableif--layoutenableif" target="_blank">`LayoutDisableIf` / `LayoutEnableIf`</a>
    *   <a href="https://saintsfield.comes.today/layout-system/layoutshowif--layouthideif" target="_blank">`LayoutShowIf` / `LayoutHideIf`</a>
*   <a href="https://saintsfield.comes.today/handles" target="_blank">Handles</a>
    *   <a href="https://saintsfield.comes.today/handles/sceneviewpicker" target="_blank">`SceneViewPicker`</a>
    *   <a href="https://saintsfield.comes.today/handles/drawlabel" target="_blank">`DrawLabel`</a>
    *   <a href="https://saintsfield.comes.today/handles/positionhandle" target="_blank">`PositionHandle`</a>
    *   <a href="https://saintsfield.comes.today/handles/drawline" target="_blank">`DrawLine`</a>
    *   <a href="https://saintsfield.comes.today/handles/saintsarrow" target="_blank">`SaintsArrow`</a>
    *   <a href="https://saintsfield.comes.today/handles/arrowhandlecap" target="_blank">`ArrowHandleCap`</a>
    *   <a href="https://saintsfield.comes.today/handles/drawwiredisc" target="_blank">`DrawWireDisc`</a>
    *   <a href="https://saintsfield.comes.today/handles/spherehandlecap" target="_blank">`SphereHandleCap`</a>
*   <a href="https://saintsfield.comes.today/component-header" target="_blank">Component Header</a>
    *   <a href="https://saintsfield.comes.today/component-header/headerbutton--headerleftbutton" target="_blank">`HeaderButton` / `HeaderLeftButton`</a>
    *   <a href="https://saintsfield.comes.today/component-header/headerghostbutton--headerghostleftbutton" target="_blank">`HeaderGhostButton` / `HeaderGhostLeftButton`</a>
    *   <a href="https://saintsfield.comes.today/component-header/headerlabel--headerleftlabel" target="_blank">`HeaderLabel` / `HeaderLeftLabel`</a>
    *   <a href="https://saintsfield.comes.today/component-header/headerdraw--headerleftdraw" target="_blank">`HeaderDraw` / `HeaderLeftDraw`</a>
*   Data Types
    *   <a href="https://saintsfield.comes.today/data-types/saintsarraysaintslist" target="_blank">`SaintsArray`/`SaintsList`</a>
    *   <a href="https://saintsfield.comes.today/data-types/saintsdictionary" target="_blank">`SaintsDictionary<,>`</a>
    *   <a href="https://saintsfield.comes.today/data-types/saintsinterface" target="_blank">`SaintsInterface<>`</a>
    *   <a href="https://saintsfield.comes.today/data-types/saintshashset--referencehashset" target="_blank">`SaintsHashSet<>` / `ReferenceHashSet<>`</a>
    *   <a href="https://saintsfield.comes.today/data-types/saintsdecimal" target="_blank">`SaintsDecimal`</a>
    *   <a href="https://saintsfield.comes.today/data-types/typereference" target="_blank">`TypeReference`</a>
    *   <a href="https://saintsfield.comes.today/data-types/saintsevent" target="_blank">`SaintsEvent`</a>
    *   <a href="https://saintsfield.comes.today/data-types/scenereference" target="_blank">`SceneReference`</a>
*   <a href="https://saintsfield.comes.today/addressable" target="_blank">Addressable</a>
    *   <a href="https://saintsfield.comes.today/addressable/addressablelabel" target="_blank">`AddressableLabel`</a>
    *   <a href="https://saintsfield.comes.today/addressable/addressableaddress" target="_blank">`AddressableAddress`</a>
    *   <a href="https://saintsfield.comes.today/addressable/addressableresource" target="_blank">`AddressableResource`</a>
    *   <a href="https://saintsfield.comes.today/addressable/addressablescene" target="_blank">`AddressableScene`</a>
    *   <a href="https://saintsfield.comes.today/addressable/addressablesubassetrequired" target="_blank">`AddressableSubAssetRequired`</a>
*   <a href="https://saintsfield.comes.today/ai-navigation" target="_blank">AI Navigation</a>
    *   <a href="https://saintsfield.comes.today/ai-navigation/navmeshareamask" target="_blank">`NavMeshAreaMask`</a>
    *   <a href="https://saintsfield.comes.today/ai-navigation/navmesharea" target="_blank">`NavMeshArea`</a>
*   <a href="https://saintsfield.comes.today/spine" target="_blank">Spine</a>
    *   <a href="https://saintsfield.comes.today/spine/spineanimationpicker" target="_blank">`SpineAnimationPicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spineskinpicker" target="_blank">`SpineSkinPicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spineslotpicker" target="_blank">`SpineSlotPicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spineattachmentpicker" target="_blank">`SpineAttachmentPicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spinebonepicker" target="_blank">`SpineBonePicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spineeventpicker" target="_blank">`SpineEventPicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spineikconstraintpicker" target="_blank">`SpineIkConstraintPicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spinepathconstraintpicker" target="_blank">`SpinePathConstraintPicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spinetransformconstraintpicker" target="_blank">`SpineTransformConstraintPicker`</a>
*   DOTween
    *   <a href="https://saintsfield.comes.today/dotween/dotweenplay" target="_blank">`DOTweenPlay`</a>
*   <a href="https://saintsfield.comes.today/wwise" target="_blank">Wwise</a>
    *   <a href="https://saintsfield.comes.today/wwise/getwwise" target="_blank">`GetWwise`</a>
*   <a href="https://saintsfield.comes.today/i-localization" target="_blank">I2 Localization</a>
    *   <a href="https://saintsfield.comes.today/i-localization/localizedstringpicker" target="_blank">`LocalizedStringPicker`</a>
*   <a href="https://saintsfield.comes.today/saintseditor" target="_blank">SaintsEditor</a>
    *   <a href="https://saintsfield.comes.today/saintseditor/setup" target="_blank">Setup</a>
    *   <a href="https://saintsfield.comes.today/saintseditor/inherent" target="_blank">Inherent</a>
    *   <a href="https://saintsfield.comes.today/saintseditor/extend" target="_blank">Extend</a>
    *   <a href="https://saintsfield.comes.today/saintseditor/integerate" target="_blank">Integerate</a>
    *   <a href="https://saintsfield.comes.today/saintseditor/netcode-for-game-objects" target="_blank">Netcode for Game Objects</a>
    *   <a href="https://saintsfield.comes.today/saintseditor/scriptable-renderer-data" target="_blank">Scriptable Renderer Data</a>
    *   <a href="https://saintsfield.comes.today/saintseditor/saintsbuild-support" target="_blank">SaintsBuild Support</a>
*   <a href="https://saintsfield.comes.today/extended-serialization" target="_blank">Extended Serialization</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/dictionary" target="_blank">`Dictionary<,>`</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/hashset" target="_blank">`HashSet<>`</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/interface" target="_blank">`interface`</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/longulong-enum" target="_blank">`long`/`ulong` Enum</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/datetime" target="_blank">`DateTime`</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/timespan" target="_blank">`TimeSpan`</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/guid" target="_blank">`Guid`</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/decimal" target="_blank">`decimal`</a>
*   <a href="https://saintsfield.comes.today/saintseditorwindow" target="_blank">`SaintsEditorWindow`</a>
    *   <a href="https://saintsfield.comes.today/saintseditorwindow/usage--example" target="_blank">Usage & Example</a>
    *   <a href="https://saintsfield.comes.today/saintseditorwindow/life-cycle--functions" target="_blank">Life Cycle & Functions</a>
    *   <a href="https://saintsfield.comes.today/saintseditorwindow/windowinlineeditor" target="_blank">`WindowInlineEditor`</a>
*   Misc
    *   <a href="https://saintsfield.comes.today/misc/about-groupby" target="_blank">About GroupBy</a>
    *   <a href="https://saintsfield.comes.today/misc/emode" target="_blank">`EMode`</a>
    *   <a href="https://saintsfield.comes.today/misc/callback" target="_blank">Callback</a>
    *   <a href="https://saintsfield.comes.today/misc/syntax-for-showhideenabledisablerequired-if" target="_blank">Syntax for Show/Hide/Enable/Disable/Required-If</a>
    *   Saints XPath-like Syntax
        *   <a href="https://saintsfield.comes.today/misc/saints-xpath-like-syntax/xpath" target="_blank">XPath</a>
        *   <a href="https://saintsfield.comes.today/misc/saints-xpath-like-syntax/exp" target="_blank">`EXP`</a>
    *   <a href="https://saintsfield.comes.today/misc/add-a-macro" target="_blank">Add a Macro</a>
    *   <a href="https://saintsfield.comes.today/misc/auto-validator" target="_blank">Auto Validator</a>
    *   <a href="https://saintsfield.comes.today/misc/use-with-other-drawers" target="_blank">Use With Other Drawers</a>

## Donation ##

### Donation Link ###

PayPal: [![Image](https://github.com/user-attachments/assets/af35c913-151f-463d-9635-e562683b1ce8)](https://www.paypal.com/donate/?hosted_button_id=B38BUN42VQ73N)

### Donation List ###

Thanks for the following generous donors:

- [bilemedimkq](https://github.com/bilemedimkq) donated on 2025-09-17
