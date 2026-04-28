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
*   Or disable related functions with `Tools` - `SaintsField` - `Disable DOTween Support`
*   If you can not find this menu, please read the "Add a Macro" section about how to manually disable DOTween support in SaintsField.

[**Optional**] To use the full functions of this project, please also do: `Tools` - `SaintsField` - `Enable SaintsEditor`. Note this will break your existing Editor plugin like `OdinInspector`, `NaughtyAttributes`, `MyToolbox`, `Tri-Inspector`.

If you need to put this project under another folder rather than `Packages/today.comes.saintsfield`, please also do the following:

*   Create `Assets/Editor Default Resources/SaintsField`.
*   Copy files from the project's `Editor/Editor Default Resources/SaintsField` into your project's `Assets/Editor Default Resources/SaintsField`.
    If you're using a file browser instead of Unity's project tab to copy files, you may want to exclude the `.meta` file to avoid GUID conflict.

**Troubleshoot**

After installation, you can use `Tools` - `SaintsField` - `Troubleshoot` to check if some attributes do not work.

namespace: `SaintsField`

### Change Log ###

1.  Fix: Code parser custom path was not used correctly on load.
2.  Add: `Button` now can display the error message if any error happens. This works for both normal method and `IEnumerator` method.
3.  Add: `Button` with `IEnumerator` now can be manually terminated when it's finished yet.
4.  Fix: `SaintsSerialized` might failed to deserialize the collection type data and resulted in empty data.

## Usage ##

*   <a href="https://saintsfield.comes.today/getting-started">General Attributes</a>
    *   Label & Text
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/labeltext">`LabelText`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/fieldlabeltext">`FieldLabelText`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/nolabel">`NoLabel`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/abovetext--belowtext">`AboveText` / `BelowText`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/fieldabovetext--fieldbelowtext">`FieldAboveText` / `FieldBelowText`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/overlaytext">`OverlayText`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/endtext">`EndText`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/infoboxbelowinfobox">`InfoBox`/`BelowInfoBox`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/fieldinfoboxfieldbelowinfobox">`FieldInfoBox`/`FieldBelowInfoBox`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/separatorbelowseparator">`Separator`/`BelowSeparator`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/fieldseparator--fieldbelowseparator">`FieldSeparator` / `FieldBelowSeparator`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/septitle">`SepTitle`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/label--text/guicolor">`GUIColor`</a>
    *   Button
        *   <a href="https://saintsfield.comes.today/general-attributes/button/button">`Button`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/button/abovebuttonbelowbuttonpostfieldbutton">`AboveButton`/`BelowButton`/`PostFieldButton`</a>
    *   Game Related
        *   <a href="https://saintsfield.comes.today/general-attributes/game-related/layer">`Layer`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/game-related/scene">`Scene`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/game-related/sortinglayer">`SortingLayer`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/game-related/tag">`Tag`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/game-related/inputaxis">`InputAxis`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/game-related/shaderparam">`ShaderParam`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/game-related/shaderkeyword">`ShaderKeyword`</a>
    *   Toggle & Switch
        *   <a href="https://saintsfield.comes.today/general-attributes/toggle--switch/gameobjectactive">`GameObjectActive`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/toggle--switch/spritetoggle">`SpriteToggle`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/toggle--switch/materialtoggle">`MaterialToggle`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/toggle--switch/colortoggle">`ColorToggle`</a>
    *   Data Editor
        *   <a href="https://saintsfield.comes.today/general-attributes/data-editor/expandable">`Expandable`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/data-editor/referencepicker">`ReferencePicker`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/data-editor/saintsrow">`SaintsRow`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/data-editor/listdrawersettings">`ListDrawerSettings`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/data-editor/table">`Table`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/data-editor/showininspector">`ShowInInspector`</a>
    *   Numerical
        *   <a href="https://saintsfield.comes.today/general-attributes/numerical/rate">`Rate`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/numerical/proprange">`PropRange`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/numerical/minmaxslider">`MinMaxSlider`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/numerical/progressbar">`ProgressBar`</a>
    *   Animation
        *   <a href="https://saintsfield.comes.today/general-attributes/animation/animatorparam">`AnimatorParam`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/animation/animatorstate">`AnimatorState`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/animation/curverange">`CurveRange`</a>
    *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter">Auto Getter</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getcomponent">`GetComponent`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getcomponentinchildrengetinchildren">`GetComponentInChildren`/`GetInChildren`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getcomponentinparent--getcomponentinparents">`GetComponentInParent` / `GetComponentInParents`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/findobjectsbytypegetinscene">`FindObjectsByType`/`GetInScene`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getprefabwithcomponent">`GetPrefabWithComponent`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getscriptableobject">`GetScriptableObject`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getinsiblings">`GetInSiblings`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getbyxpath">`GetByXPath`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getmaincamera">`GetMainCamera`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/addcomponent">`AddComponent`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/findcomponent">`FindComponent`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/auto-getter/getcomponentbypath">`GetComponentByPath`</a>
    *   Validate & Restrict
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/fieldtype">`FieldType`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/onvaluechanged">`OnValueChanged`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/onarraysizechanged">`OnArraySizeChanged`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/readonlydisableifenableif">`ReadOnly`/`DisableIf`/`EnableIf`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/fieldenableiffielddisableiffieldreadonly">`FieldEnableIf`/`FieldDisableIf`/`FieldReadOnly`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/showifhideif">`ShowIf`/`HideIf`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/fieldshowif--fieldhideif">`FieldShowIf` / `FieldHideIf`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/required">`Required`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/requiredif">`RequiredIf`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/validateinput">`ValidateInput`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/minvalue--maxvalue">`MinValue` / `MaxValue`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/requiretype">`RequireType`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/validate--restrict/arraysize">`ArraySize`</a>
    *   Miscellaneous
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/dropdown">`Dropdown`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/optionsdropdown--pairsdropdown">`OptionsDropdown` / `PairsDropdown`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/flagsdropdown">`FlagsDropdown`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/advanceddropdown">`AdvancedDropdown`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/advancedoptionsdropdown--advancedpairsdropdown">`AdvancedOptionsDropdown` / `AdvancedPairsDropdown`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/menudropdown">`MenuDropdown`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/customcontextmenu">`CustomContextMenu`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/fieldcustomcontextmenu">`FieldCustomContextMenu`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/valuebuttons">`ValueButtons`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/optionsvaluebuttons--pairsvaluebuttons">`OptionsValueButtons` / `PairsValueButtons`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/enumtogglebuttons">`EnumToggleButtons`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/resizabletextarea">`ResizableTextArea`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/lefttoggle">`LeftToggle`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/resourcepath">`ResourcePath`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/resourcefolder">`ResourceFolder`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/fielddefaultexpand">`FieldDefaultExpand`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/defaultexpand">`DefaultExpand`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/assetfolder">`AssetFolder`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/assetpreview">`AssetPreview`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/aboveimagebelowimage">`AboveImage`/`BelowImage`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/particleplay">`ParticlePlay`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/buttonaddonclick">`ButtonAddOnClick`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/onbuttonclick">`OnButtonClick`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/onevent">`OnEvent`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/colorpalette">`ColorPalette`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/searchable">`Searchable`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/datetime">`DateTime`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/timespan">`TimeSpan`</a>
        *   <a href="https://saintsfield.comes.today/general-attributes/miscellaneous/guid">`Guid`</a>
*   <a href="https://saintsfield.comes.today/getting-started">Layout System</a>
    *   <a href="https://saintsfield.comes.today/layout-system/overview">Overview</a>
    *   <a href="https://saintsfield.comes.today/layout-system/layout">`Layout`</a>
    *   <a href="https://saintsfield.comes.today/layout-system/layoutstart--layoutend">`LayoutStart` / `LayoutEnd`</a>
    *   <a href="https://saintsfield.comes.today/layout-system/layoutclosehere--layoutterminatehere">`LayoutCloseHere` / `LayoutTerminateHere`</a>
    *   <a href="https://saintsfield.comes.today/layout-system/layoutdisableif--layoutenableif">`LayoutDisableIf` / `LayoutEnableIf`</a>
    *   <a href="https://saintsfield.comes.today/layout-system/layoutshowif--layouthideif">`LayoutShowIf` / `LayoutHideIf`</a>
*   <a href="https://saintsfield.comes.today/getting-started">Handles</a>
    *   <a href="https://saintsfield.comes.today/handles/sceneviewpicker">`SceneViewPicker`</a>
    *   <a href="https://saintsfield.comes.today/handles/drawlabel">`DrawLabel`</a>
    *   <a href="https://saintsfield.comes.today/handles/positionhandle">`PositionHandle`</a>
    *   <a href="https://saintsfield.comes.today/handles/drawline">`DrawLine`</a>
    *   <a href="https://saintsfield.comes.today/handles/saintsarrow">`SaintsArrow`</a>
    *   <a href="https://saintsfield.comes.today/handles/arrowhandlecap">`ArrowHandleCap`</a>
    *   <a href="https://saintsfield.comes.today/handles/drawwiredisc">`DrawWireDisc`</a>
    *   <a href="https://saintsfield.comes.today/handles/spherehandlecap">`SphereHandleCap`</a>
*   <a href="https://saintsfield.comes.today/getting-started">Component Header</a>
    *   <a href="https://saintsfield.comes.today/component-header/headerbutton--headerleftbutton">`HeaderButton` / `HeaderLeftButton`</a>
    *   <a href="https://saintsfield.comes.today/component-header/headerghostbutton--headerghostleftbutton">`HeaderGhostButton` / `HeaderGhostLeftButton`</a>
    *   <a href="https://saintsfield.comes.today/component-header/headerlabel--headerleftlabel">`HeaderLabel` / `HeaderLeftLabel`</a>
    *   <a href="https://saintsfield.comes.today/component-header/headerdraw--headerleftdraw">`HeaderDraw` / `HeaderLeftDraw`</a>
*   <a href="https://saintsfield.comes.today/getting-started">Data Types</a>
    *   <a href="https://saintsfield.comes.today/data-types/saintsarraysaintslist">`SaintsArray`/`SaintsList`</a>
    *   <a href="https://saintsfield.comes.today/data-types/saintsdictionary">`SaintsDictionary<,>`</a>
    *   <a href="https://saintsfield.comes.today/data-types/saintsinterface">`SaintsInterface<>`</a>
    *   <a href="https://saintsfield.comes.today/data-types/saintshashset--referencehashset">`SaintsHashSet<>` / `ReferenceHashSet<>`</a>
    *   <a href="https://saintsfield.comes.today/data-types/saintsdecimal">`SaintsDecimal`</a>
    *   <a href="https://saintsfield.comes.today/data-types/typereference">`TypeReference`</a>
    *   <a href="https://saintsfield.comes.today/data-types/saintsevent">`SaintsEvent`</a>
    *   <a href="https://saintsfield.comes.today/data-types/scenereference">`SceneReference`</a>
*   <a href="https://saintsfield.comes.today/getting-started">Addressable</a>
    *   <a href="https://saintsfield.comes.today/addressable/addressablelabel">`AddressableLabel`</a>
    *   <a href="https://saintsfield.comes.today/addressable/addressableaddress">`AddressableAddress`</a>
    *   <a href="https://saintsfield.comes.today/addressable/addressableresource">`AddressableResource`</a>
    *   <a href="https://saintsfield.comes.today/addressable/addressablescene">`AddressableScene`</a>
    *   <a href="https://saintsfield.comes.today/addressable/addressablesubassetrequired">`AddressableSubAssetRequired`</a>
*   <a href="https://saintsfield.comes.today/getting-started">AI Navigation</a>
    *   <a href="https://saintsfield.comes.today/ai-navigation/navmeshareamask">`NavMeshAreaMask`</a>
    *   <a href="https://saintsfield.comes.today/ai-navigation/navmesharea">`NavMeshArea`</a>
*   <a href="https://saintsfield.comes.today/getting-started">Spine</a>
    *   <a href="https://saintsfield.comes.today/spine/spineanimationpicker">`SpineAnimationPicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spineskinpicker">`SpineSkinPicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spineslotpicker">`SpineSlotPicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spineattachmentpicker">`SpineAttachmentPicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spinebonepicker">`SpineBonePicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spineeventpicker">`SpineEventPicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spineikconstraintpicker">`SpineIkConstraintPicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spinepathconstraintpicker">`SpinePathConstraintPicker`</a>
    *   <a href="https://saintsfield.comes.today/spine/spinetransformconstraintpicker">`SpineTransformConstraintPicker`</a>
*   <a href="https://saintsfield.comes.today/getting-started">DOTween</a>
    *   <a href="https://saintsfield.comes.today/dotween/dotweenplay">`DOTweenPlay`</a>
*   <a href="https://saintsfield.comes.today/getting-started">Wwise</a>
    *   <a href="https://saintsfield.comes.today/wwise/getwwise">`GetWwise`</a>
*   <a href="https://saintsfield.comes.today/getting-started">I2 Localization</a>
    *   <a href="https://saintsfield.comes.today/i-localization/localizedstringpicker">`LocalizedStringPicker`</a>
*   <a href="https://saintsfield.comes.today/getting-started">SaintsEditor</a>
    *   <a href="https://saintsfield.comes.today/saintseditor/setup">Setup</a>
    *   <a href="https://saintsfield.comes.today/saintseditor/inherent">Inherent</a>
    *   <a href="https://saintsfield.comes.today/saintseditor/extend">Extend</a>
    *   <a href="https://saintsfield.comes.today/saintseditor/integerate">Integerate</a>
    *   <a href="https://saintsfield.comes.today/saintseditor/netcode-for-game-objects">Netcode for Game Objects</a>
    *   <a href="https://saintsfield.comes.today/saintseditor/scriptable-renderer-data">Scriptable Renderer Data</a>
    *   <a href="https://saintsfield.comes.today/saintseditor/saintsbuild-support">SaintsBuild Support</a>
*   <a href="https://saintsfield.comes.today/getting-started">Extended Serialization</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/dictionary">`Dictionary<,>`</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/hashset">`HashSet<>`</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/interface">`interface`</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/longulong-enum">`long`/`ulong` Enum</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/datetime">`DateTime`</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/timespan">`TimeSpan`</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/guid">`Guid`</a>
    *   <a href="https://saintsfield.comes.today/extended-serialization/decimal">`decimal`</a>
*   <a href="https://saintsfield.comes.today/getting-started">`SaintsEditorWindow`</a>
    *   <a href="https://saintsfield.comes.today/saintseditorwindow/usage--example">Usage & Example</a>
    *   <a href="https://saintsfield.comes.today/saintseditorwindow/life-cycle--functions">Life Cycle & Functions</a>
    *   <a href="https://saintsfield.comes.today/saintseditorwindow/windowinlineeditor">`WindowInlineEditor`</a>
*   <a href="https://saintsfield.comes.today/getting-started">Misc</a>
    *   <a href="https://saintsfield.comes.today/misc/about-groupby">About GroupBy</a>
    *   <a href="https://saintsfield.comes.today/misc/emode">`EMode`</a>
    *   <a href="https://saintsfield.comes.today/misc/callback">Callback</a>
    *   <a href="https://saintsfield.comes.today/misc/syntax-for-showhideenabledisablerequired-if">Syntax for Show/Hide/Enable/Disable/Required-If</a>
    *   Saints XPath-like Syntax
        *   <a href="https://saintsfield.comes.today/misc/saints-xpath-like-syntax/xpath">XPath</a>
        *   <a href="https://saintsfield.comes.today/misc/saints-xpath-like-syntax/exp">`EXP`</a>
    *   <a href="https://saintsfield.comes.today/misc/add-a-macro">Add a Macro</a>
    *   <a href="https://saintsfield.comes.today/misc/auto-validator">Auto Validator</a>
    *   <a href="https://saintsfield.comes.today/misc/use-with-other-drawers">Use With Other Drawers</a>

## Donation ##

### Donation Link ###

PayPal: [![Image](https://github.com/user-attachments/assets/af35c913-151f-463d-9635-e562683b1ce8)](https://www.paypal.com/donate/?hosted_button_id=B38BUN42VQ73N)

### Donation List ###

Thanks for the following generous donors:

- [bilemedimkq](https://github.com/bilemedimkq) donated on 2025-09-17
