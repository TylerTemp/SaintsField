# SaintsField #

[![unity_version](https://github.com/TylerTemp/SaintsField/assets/6391063/c01626a1-9329-4c26-be31-372f8704df1d)](https://unity.com/download)
[![license_mit](https://github.com/TylerTemp/SaintsField/assets/6391063/a093811a-5dbc-46ad-939e-a9e207ae5bfb)](https://github.com/TylerTemp/SaintsField/blob/master/LICENSE)
[![openupm](https://img.shields.io/npm/v/today.comes.saintsfield?label=OpenUPM&registry_uri=https://package.openupm.com)](https://openupm.com/packages/today.comes.saintsfield/)
[![Percentage of issues still open](https://isitmaintained.com/badge/open/TylerTemp/SaintsField.svg)](http://isitmaintained.com/project/TylerTemp/SaintsField "Percentage of issues still open")
[![Average time to resolve an issue](https://isitmaintained.com/badge/resolution/TylerTemp/SaintsField.svg)](http://isitmaintained.com/project/TylerTemp/SaintsField "Average time to resolve an issue")
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=Downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Ftoday.comes.saintsfield)](https://openupm.com/packages/today.comes.saintsfield/)
[![repo-stars](https://img.shields.io/github/stars/TylerTemp/SaintsField)](https://github.com/TylerTemp/SaintsField/)

`SaintsField` is a Unity Inspector extension tool focusing on script fields like [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes) but different.

Developed by: [TylerTemp](https://github.com/TylerTemp), [墨瞳](https://github.com/xc13308)

Unity: 2019.1 or higher

> [!TIP]
> A better document with TOC & Search: [saintsfield.comes.today](https://saintsfield.comes.today/)

(Yes, the project name comes from, of course, [Saints Row 2](https://saintsrow.fandom.com/wiki/Saints_Row_2))

## Getting Started ##

### Highlights ###

1.  Works on deep nested fields!
2.  Supports both IMGUI and UI Toolkit! And it can properly handle IMGUI drawer even with UI Toolkit enabled!
3.  Use and only use `PropertyDrawer` and `DecoratorDrawer` (except `SaintsEditor`, which is disabled by default), thus it will be compatible with most Unity Inspector enhancements like `NaughtyAttributes` and your custom drawer.
4.  Allow stack on many cases. Only attributes that modified the label itself, and the field itself can not be stacked. All other attributes can mostly be stacked.
5.  Allow dynamic arguments in many cases

> [!NOTE]
> Since [SaintsField 4.0](https://github.com/TylerTemp/SaintsField/discussions/183), the IMGUI is no longer a major maintaining codebase in this project.
>
> IMGUI will only be focused on issues and small features. Big features like editing in `ShowInInspector` will not be supported in IMGUI.

> [!NOTE]
> If you want some specific feature been backport from UI Toolkit to IMGUI, open [an issue](https://github.com/TylerTemp/SaintsField/issues) or [discussion](https://github.com/TylerTemp/SaintsField/discussions) to request for it (not guaranteed tho)


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
    git submodule add https://github.com/TylerTemp/SaintsField.git Assets/SaintsField
    ```

If you have DOTween installed
*   Please also ensure you do: `Tools` - `Demigaint` - `DOTween Utility Panel`, click `Create ASMDEF`
*   Or disable related functions with `Window` - `Saints` - `Disable DOTween Support`
*   If you can not find this menu, please read "Add a Macro" section about how to manually disable DOTween support in SaintsField.

[**Optional**] To use the full functions of this project, please also do: `Window` - `Saints` - `Enable SaintsEditor`. Note this will break your existing Editor plugin like `OdinInspector`, `NaughtyAttributes`, `MyToolbox`, `Tri-Inspector`.

If you're using `unitypackage` or git submodule, but you put this project under another folder rather than `Assets/SaintsField`, please also do the following:

*   Create `Assets/Editor Default Resources/SaintsField`.
*   Copy files from project's `Editor/Editor Default Resources/SaintsField` into your project's `Assets/Editor Default Resources/SaintsField`.
    If you're using a file browser instead of Unity's project tab to copy files, you may want to exclude the `.meta` file to avoid GUID conflict.

**Troubleshoot**

After installed, you can use `Window` - `Saints` - `Troubleshoot` to check if some attributes do not work.

namespace: `SaintsField`

### Change Log ###

**4.24.5**

UI Toolkit: Fix blue indicator for prefab modification not display in some property [#276](https://github.com/TylerTemp/SaintsField/issues/276)

Note: all `Handle` attributes (draw stuff in the scene view) are in stage 1, which means the arguments might change in the future.

See [the full change log](https://github.com/TylerTemp/SaintsField/blob/master/CHANGELOG.md).

## General Attributes ##

### Label & Text ###

#### `RichLabel`/`NoLabel` ####

*   `string|null richTextXml` the content of the label, supported tag:

    *   All Unity rich label tag, like `<color=#ff0000>red</color>`
    *   `<icon=path/to/image.png />` for icon
    *   `<label />` for current field name
    *   `<field />`, `<field.subField/>`, `<field.subField=formatControl />` read the value from the field first, if tag has sub-field, continue to read, then use `string.Format` if there is a `formatControl`. See the example below.
    *   `<container.Type />` for the class/struct name of the container of the field
    *   `<container.Type.BaseType />` for the class/struct name of the field's container's parent
    *   `<index />`, `<index=formatControl />` for the index if the target is an array/list

    Note about format control:

    *   If the format contains `{}`, it will be used like a `string.Format`. E.g. `<field.subField=(--<color=red>{0}</color>--)/>` will be interpreted like `string.Format("(--<color=red>{0}</color>--)", this.subField)`.
    *   Otherwise, it will be re-written to `{0:formatControl}`. E.g. `<index=D4/>` will be interpreted like `string.Format("{0:D4}", index)`.

    `null` means no label

    for `icon` it will search the following path:

    *   `"Assets/Editor Default Resources/SaintsField/"`  (You can override things here)
    *   `"Assets/SaintsField/Editor/Editor Default Resources/SaintsField/"` (this is most likely to be when installed using `unitypackage`)
    *   `"Packages/today.comes.saintsfield/Editor/Editor Default Resources/SaintsField/"` (this is most likely to be when installed using `upm`)
    *   `Assets/Editor Default Resources/`, then fallback to built-in editor resources by name (using [`EditorGUIUtility.Load`](https://docs.unity3d.com/ScriptReference/EditorGUIUtility.Load.html))

    You can also use Unity Editor's built-in icons. See [UnityEditorIcons](https://github.com/nukadelic/UnityEditorIcons). e.g. `<icon=d_AudioListener Icon/>`

    for `color` it supports:

    *   Standard [Unity Rich Label](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html#ColorNames) colors:

        `aqua`, `black`, `blue`, `brown`, `cyan`, `darkblue`, `fuchsia`, `green`, `gray`, `grey`, `lightblue`, `lime`, `magenta`, `maroon`, `navy`, `olive`, `orange`, `purple`, `red`, `silver`, `teal`, `white`, `yellow`

    *   Some extra colors from [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes/blob/master/Assets/NaughtyAttributes/Scripts/Core/Utility/EColor.cs):

        `clear`, `pink`, `indigo`, `violet`

    *   Some extra colors from UI Toolkit:

        `charcoalGray`, `oceanicSlate`

    *   html color which is supported by [`ColorUtility.TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility.TryParseHtmlString.html), like `#RRGGBB`, `#RRGGBBAA`, `#RGB`, `#RGBA`

    ![color_list_ui_toolkit_add](https://github.com/TylerTemp/SaintsField/assets/6391063/50ec511b-b914-4395-8b42-793a4389c8da)

    If it starts with `$`, the leading `$` will be removed and `isCallback` will be set to `true`. Use `\$` to escape the starting `$`.

*   `bool isCallback=false`

    if true, the `richTextXml` will be interpreted as a property/callback function, and the string value / the returned string value (tag supported) will be used as the label content.

    This is override to be `true` when `richLabelXml` starts with `$`

*   AllowMultiple: No. A field can only have one `RichLabel`

`[NoLabel]` is a shortcut for `[RichLabel(null)]`

Special Note:

Use it on an array/list will apply it to all the direct child element instead of the field label itself.
You can use this to modify elements of an array/list field, in this way:

1.  Ensure you make it a callback: `isCallback=true`, or the `richTextXml` starts with `$`
2.  It'll pass the element value and index to your function
3.  Return the desired label content from the function

```csharp
using SaintsField;

[RichLabel("<color=indigo><icon=eye.png /></color><b><color=red>R</color><color=green>a</color><color=blue>i</color><color=yellow>i</color><color=cyan>n</color><color=magenta>b</color><color=pink>o</color><color=orange>w</color></b>: <color=violet><label /></color>")]
public string _rainbow;

[RichLabel("$" + nameof(LabelCallback))]
public bool _callbackToggle;
private string LabelCallback() => _callbackToggle ? "<color=green><icon=eye.png /></color> <label/>" : "<icon=eye-slash.png /> <label/>";

[Space]
[RichLabel("$" + nameof(_propertyLabel))]
public string _propertyLabel;
private string _rainbow;

[Serializable]
private struct MyStruct
{
    [RichLabel("<color=green>HI!</color>")]
    public float LabelFloat;
}

[SerializeField]
[RichLabel("<color=green>Fixed For Struct!</color>")]
private MyStruct _myStructWorkAround;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/5e865350-6eeb-4f2a-8305-c7d8b8720eac)](https://github.com/TylerTemp/SaintsField/assets/6391063/25f6c7cc-ee7e-444e-b078-007dd6df499e)

Here is an example of using on an array:

```csharp
using SaintsField;

[RichLabel(nameof(ArrayLabels), true)]
public string[] arrayLabels;

// if you do not care about the actual value, use `object` as the first parameter
private string ArrayLabels(object _, int index) => $"<color=pink>[{(char)('A' + index)}]";
```

![label_array](https://github.com/TylerTemp/SaintsField/assets/6391063/232da62c-9e31-4415-a09a-8e1e95ae9441)

Example of using `<field />` to display field value/propery value:

```csharp
using SaintsField;

public class SubField : MonoBehaviour
{
    [SerializeField] private string _subLabel;

    public double doubleVal;
}

[Separator("Field")]
// read field value
[RichLabel("<color=lime><field/>")] public string fieldLabel;
// read the `_subLabel` field/function from the field
[RichLabel("<field._subLabel/>"), GetComponentInChildren, Expandable] public SubField subField;
// again read the property
[RichLabel("<color=lime><field.gameObject.name/>")] public SubField subFieldName;

[Separator("Field Null")]
// not found target will be rendered as empty string
[RichLabel("<field._subLabel/>")] public SubField notFoundField;
[RichLabel("<field._noSuch/>"), GetComponentInChildren] public SubField notFoundField2;

[Separator("Formatter")]
// format as percent
[RichLabel("<field=P2/>"), PropRange(0f, 1f)] public float percent;
// format `doubleVal` field as exponential
[RichLabel("<field.doubleVal=E/>")] public SubField subFieldCurrency;
```

[![video](https://github.com/user-attachments/assets/dc65d897-fcbf-4a40-b4aa-d99a8a4975a7)](https://github.com/user-attachments/assets/a6d93600-500b-4a0e-bf2d-9f2e8fb8bc32)

Example of quoted fancy formatting:

```csharp
[RichLabel("<field=\">><color=yellow>{0}</color><<\"/> <index=\"[<color=blue>>></color>{0}<color=blue><<</color>]\"/>")]
public string[] sindices;
```

![Image](https://github.com/user-attachments/assets/8232e42e-21ec-43ec-92c3-fbfeaebe4de1)

#### `AboveRichLabel` / `BelowRichLabel` ####

Like `RichLabel`, but it's rendered above/below the field in full width of view instead.


*   `string|null richTextXml` Same as `RichLabel`
*   `bool isCallback=false` Same as `RichLabel`
*   `string groupBy = ""` See `GroupBy` section
*   AllowMultiple: Yes

```csharp
using SaintsField;

[SerializeField]
[AboveRichLabel("┌<icon=eye.png/><label />┐")]
[RichLabel("├<icon=eye.png/><label />┤")]
[BelowRichLabel("$" + nameof(BelowLabel))]
[BelowRichLabel("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~", groupBy: "example")]
[BelowRichLabel("==================================", groupBy: "example")]
private int _intValue;

private string BelowLabel() => "└<icon=eye.png/><label />┘";
```

![full_width_label](https://github.com/TylerTemp/SaintsField/assets/6391063/9283e25a-34b3-4192-8a07-5d97a4e55406)

#### `OverlayRichLabel` ####

Like `RichLabel`, but it's rendered on top of the field.

Only supports string/number type of field. Does not work with any kind of `TextArea` (multiple line) and `Range`.

Parameters:

*   `string richTextXml` the content of the label, or a property/callback. Supports tags like `RichLabel`

    If it starts with `$`, the leading `$` will be removed and `isCallback` will be set to `true`. Use `\$` to escape the starting `$`.

*   `bool isCallback=false` if true, the `richTextXml` will be interpreted as a property/callback function, and the string value / the returned string value (tag supported) will be used as the label content
*   `float padding=5f` padding between your input and the label. Not work when `end=true`
*   `bool end=false` when false, the label will follow the end of your input. Otherwise, it will stay at the end of the field.
*   `string GroupBy=""` this is only for the error message box.
*   AllowMultiple: No

```csharp
using SaintsField;

[OverlayRichLabel("<color=grey>km/s")] public double speed = double.MinValue;
[OverlayRichLabel("<icon=eye.png/>")] public string text;
[OverlayRichLabel("<color=grey>/int", padding: 1)] public int count = int.MinValue;
[OverlayRichLabel("<color=grey>/long", padding: 1)] public long longInt = long.MinValue;
[OverlayRichLabel("<color=grey>suffix", end: true)] public string atEnd;
```

![overlay_rich_label](https://github.com/TylerTemp/SaintsField/assets/6391063/bd85b5c8-3ef2-4899-9bc3-b9799e3331ed)

#### `PostFieldRichLabel` ####

Like `RichLabel`, but it's rendered at the end of the field.

Parameters:

*   `string richTextXml` the content of the label, or a property/callback. Supports tags like `RichLabel`

    If it starts with `$`, the leading `$` will be removed and `isCallback` will be set to `true`. Use `\$` to escape the starting `$`.

*   `bool isCallback=false` if true, the `richTextXml` will be interpreted as a property/callback function, and the string value / the returned string value (tag supported) will be used as the label content
*   `float padding=5f` padding between the field and the label.
*   `string GroupBy=""` this is only for the error message box.
*   AllowMultiple: Yes

```csharp
using SaintsField;

[PostFieldRichLabel("<color=grey>km/s")] public float speed;
[PostFieldRichLabel("<icon=eye.png/>", padding: 0)] public GameObject eye;
[PostFieldRichLabel("$" + nameof(TakeAGuess))] public int guess;

public string TakeAGuess()
{
    if(guess > 20)
    {
        return "<color=red>too high";
    }

    if (guess < 10)
    {
        return "<color=blue>too low";
    }

    return "<color=green>acceptable!";
}
```

![post_field_rich_label](https://github.com/TylerTemp/SaintsField/assets/6391063/bdd9446b-97fe-4cd2-900a-f5ed5f1ccb56)

#### `PlayaRichLabel` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

This is like `RichLabel`, but it can change label of an array/list

Please note: at the moment it only works for serialized property, and is only tested on array/list. It's suggested to use `RichLabel` for non-array/list
serialized fields.

Parameters:

*   `string richTextXml` the rich text xml for the label. Note: custom rich label tag created by this project only works in UI Toolkit mode.
*   `bool isCallback=false` if it's a callback (a method/property/field)

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

[PlayaRichLabel("<color=lame>It's Labeled!")]
public List<string> myList;

[PlayaRichLabel(nameof(MethodLabel), true)]
public string[] myArray;

private string MethodLabel(string[] values)
{
    return $"<color=green><label /> {string.Join("", values.Select(_ => "<icon=star.png />"))}";
}
```

![PlayaRichLabel](https://github.com/TylerTemp/SaintsField/assets/6391063/fbc132fc-978a-4b35-9a69-91fcb72db55a)

#### `PlayaAboveRichLabel` /  `PlayaBelowRichLabel` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

Just like `AboveRichLabel` / `BelowRichLabel` but it can be applied on a top of an array/list, a property or a method

For `AboveRichLabel`, it can also be applied on a class/struct.

Parameters:

*   `string content` the content to show. If it starts with `$`, then a callback/propery/field value is used. When a callback gives null or empty string, the label will be hidden.

```csharp
using SaintsField;
using SaintsField.Playa;

[PlayaAboveRichLabel("<color=gray>-- Above --")]
[PlayaAboveRichLabel("$" + nameof(dynamicContent))]
[PlayaBelowRichLabel("$" + nameof(dynamicContent))]
[PlayaBelowRichLabel("<color=gray>-- Below --")]
public string[] s;

[Space(20)]
public string dynamicContent;
```

![Image](https://github.com/user-attachments/assets/5c29a43b-7276-488a-98fa-da133e77edc4)

Example of using on a class/struct like a data comment:

```csharp
using SaintsField;
using SaintsField.Playa;

[PlayaAboveRichLabel("<color=gray>This is a class message")]
[PlayaAboveRichLabel("$" + nameof(dynamicContent))]
public class ClassPlayaAboveRichLabelExample : MonoBehaviour
{
    [ResizableTextArea]
    public string dynamicContent;

    [Serializable]
    [PlayaAboveRichLabel("<color=gray>--This is a struct message--")]
    public struct MyStruct
    {
        public string structString;
    }

    public MyStruct myStruct;
}
```

![Image](https://github.com/user-attachments/assets/de9e6bf2-5e7b-4a4a-92f5-66801984544e)

#### `InfoBox`/`BelowInfoBox` ####

Draw an info box above/below the field.

*   `string content`

    The content of the info box.

    If it starts with `$`, the leading `$` will be removed and `isCallback` will be set to `true`. Use `\$` to escape the starting `$`.

*   `EMessageType messageType=EMessageType.Info`

    Message icon. Options are

    *   `None`
    *   `Info`
    *   `Warning`
    *   `Error`

*   `string show=null`

    a callback name or property name for show or hide this info box.

*   `bool isCallback=false`

    if true, the `content` will be interpreted as a property/callback function.

    If the value (or returned value) is a string, then the content will be changed

    If the value is `(EMessageType messageType, string content)` then both content and message type will be changed

*   ~~`bool above=false`~~

    ~~Draw the info box above the field instead of below~~

    Renamed to `below` since version `3.3.0`

*   `bool below=false`

    Draw the info box below the field instead of above

*   `string groupBy=""` See `GroupBy` section

*   AllowMultiple: Yes

`BelowInfoBox` is a shortcut for `[InfoBox(..., below: true)]`

```csharp
using SaintsField;

[field: SerializeField] private bool _show;

[Space]
[InfoBox("Hi\nwrap long line content content content content content content content content content content content content content content content content content content content content content content content content content", EMessageType.None)]
[BelowInfoBox("$" + nameof(DynamicMessage), EMessageType.Warning)]
[BelowInfoBox("$" + nameof(DynamicMessageWithIcon))]
[BelowInfoBox("Hi\n toggle content ", EMessageType.Info, nameof(_show))]
public bool _content;

private (EMessageType, string) DynamicMessageWithIcon => _content ? (EMessageType.Error, "False!") : (EMessageType.None, "True!");
private string DynamicMessage() => _content ? "False" : "True";
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/c96b4b14-594d-4bfd-9cc4-e53390ed99be)](https://github.com/TylerTemp/SaintsField/assets/6391063/03ac649a-9e89-407d-a59d-3e224a7f84c8)

#### `PlayaInfoBox`/`PlayaBelowInfoBox` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

This is like `InfoBox`, but it can be applied to array/list/method etc.

For `PlayaInfoBox`, it can also be directly applied on a class/struct definition.

*   `string content`

    The content of the info box.

    If it starts with `$`, the leading `$` will be removed and `isCallback` will be set to `true`. Use `\$` to escape the starting `$`.

*   `EMessageType messageType=EMessageType.Info`

    Message icon. Options are

    *   `None`
    *   `Info`
    *   `Warning`
    *   `Error`

*   `string show=null`

    a callback name or property name for show or hide this info box.

*   `bool isCallback=false`

    if true, the `content` will be interpreted as a property/callback function.

    If the value (or returned value) is a string, then the content will be changed

    If the value is `(EMessageType messageType, string content)` then both content and message type will be changed

*   `bool below=false`

    Draw the info box below the field instead of above

*   `string groupBy=""` See `GroupBy` section

*   AllowMultiple: Yes

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

[PlayaInfoBox("Please Note: special label like <icon=star.png/> only works for <color=lime>UI Toolkit</color> <color=red>(not IMGUI)</color> in InfoBox.")]
[PlayaBelowInfoBox("$" + nameof(DynamicFromArray))]  // callback
public string[] strings = {};

public string dynamic;

private string DynamicFromArray(string[] value) => value.Length > 0? string.Join("\n", value): "null";

[PlayaInfoBox("MethodWithButton")]
[Button("Click Me!")]
[PlayaBelowInfoBox("GroupExample", groupBy: "group")]
[PlayaBelowInfoBox("$" + nameof(dynamic), groupBy: "group")]
public void MethodWithButton()
{
}

[PlayaInfoBox("Method")]
[PlayaBelowInfoBox("$" + nameof(dynamic))]
public void Method()
{
}
```

![image](https://github.com/user-attachments/assets/81d82ee4-4f8d-4ae3-bae5-dcb13d3af7c5)

Example of using on a class/struct definition like a data comment:

```csharp
using SaintsField;
using SaintsField.Playa;

[PlayaInfoBox("This is a class message", EMessageType.None)]
[PlayaInfoBox("$" + nameof(dynamicContent))]
public class ClassInfoBoxExample : MonoBehaviour  // The info box will show in inspector wherever you attach this component
{
    public string dynamicContent;

    [Serializable]
    [PlayaInfoBox("This is a struct message")]
    public struct MyStruct  // The info box will show at first row wherever you use this struct
    {
        public string structString;
    }

    public MyStruct myStruct;
}
```

![Image](https://github.com/user-attachments/assets/70a8613e-e17f-4463-8653-a6500b9e757f)

#### `Separator` / `BelowSeparator` ####

Draw text, separator, spaces for field on above / below with rich text & dynamic text support.

Parameters:

*   `string title=null` display a title. `null` for no title, only separator.

    If it starts with `$`, the leading `$` will be removed and `isCallback` will be set to `true`. Use `\$` to escape the starting `$`.

*   `EColor color=EColor.Gray` color for the title and the separator
*   `EAlign eAlign=EAlign.Start` how the title is positioned, options are:
    *   `EAlign.Start`
    *   `EAlign.Center`
    *   `EAlign.End`
*   `bool isCallback=false` when `true`, use `title` as a callback to get a dynamic title
*   `int space=0` leave some space above or below the separator, like what `Space` does.
*   `bool below=false` when `true`, draw the separator below the field.

```csharp
using SaintsField;

[Space(50)]

[Separator("Start")]
[Separator("Center", EAlign.Center)]
[Separator("End", EAlign.End)]
[BelowSeparator("$" + nameof(Callback))]
public string s3;
public string Callback() => s3;

[Space(50)]

[Separator]
public string s1;

[Separator(10)]  // this behaves like a space
[Separator("[ Hi <color=LightBlue>Above</color> ]", EColor.Aqua, EAlign.Center)]
[BelowSeparator("[ Hi <color=Silver>Below</color> ]", EColor.Brown, EAlign.Center)]
[BelowSeparator(10)]
public string hi;

[BelowSeparator]
public string s2;
```

![image](https://github.com/user-attachments/assets/ae7f5eae-d94f-4cb3-88dc-8250a4e0a4ec)

This is very useful when you what to separate parent fields from the inherent:

```csharp
using SaintsField;

public class SeparatorParent : MonoBehaviour
{
    [BelowSeparator("End Of <b><color=Aqua><container.Type/></color></b>", EAlign.Center, space: 10)]
    public string parent;
}

public class SeparatorInherent : SeparatorParent
{
    public string inherent;
}
```

![image](https://github.com/user-attachments/assets/2f6e369d-1260-4379-9504-6036fb89e15b)

#### `PlayaSeparator` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

Draw text, separator, spaces for field/property/button/layout on above / below with rich text & dynamic text support.

This is UI Toolkit only.

Parameters:

*   `string title=null` display a title. `null` for no title, only separator.

    If it starts with `$`, the leading `$` will be removed and `isCallback` will be set to `true`. Use `\$` to escape the starting `$`.

*   `EColor color=EColor.Gray` color for the title and the separator
*   `EAlign eAlign=EAlign.Start` how the title is positioned, options are:
    *   `EAlign.Start`
    *   `EAlign.Center`
    *   `EAlign.End`
*   `bool isCallback=false` when `true`, use `title` as a callback to get a dynamic title
*   `int space=0` leave some space above or below the separator, like what `Space` does.
*   `bool below=false` when `true`, draw the separator below the field.

```csharp
[PlayaSeparator("Separator", EAlign.Center)]
public string separator;

[PlayaSeparator("Left", EAlign.Start)] public string left;

[PlayaSeparator("$" + nameof(right), EAlign.End)]
public string right;

[PlayaSeparator(20)]
[PlayaSeparator("Space 20")]
public string[] arr;

[PlayaSeparator("End", below: true)] public string end;
```

![image](https://github.com/user-attachments/assets/7aaee07a-d8f5-4b13-a166-157df57e9d3d)

Using it with `Layout`, you can create some fancy appearance:

```csharp
[LayoutStart("Equipment", ELayout.TitleBox)]

[LayoutStart("./Head")]
[PlayaSeparator("Head", EAlign.Center)]
public string st;
[LayoutCloseHere]
public MyStruct inOneStruct;

[LayoutStart("./Upper Body")]

[PlayaInfoBox("Note：left hand can be empty, but not right hand", EMessageType.Warning)]

[LayoutStart("./Horizontal", ELayout.Horizontal)]

[LayoutStart("./Left Hand")]
[PlayaSeparator("Left Hand", EAlign.Center)]
public string g11;
public string g12;
public MyStruct myStruct;
public string g13;

[LayoutStart("../Right Hand")]
[PlayaSeparator("Right Hand", EAlign.Center)]
public string g21;
[RichLabel("<color=lime><label/>")]
public string g22;
[RichLabel("$" + nameof(g23))]
public string g23;

public bool toggle;
```

![image](https://github.com/user-attachments/assets/792960eb-50eb-4a26-b563-37282c20a174)

#### `SepTitle` ####

A separator with text. This is a decorator type attribute, which means it can be used on top of a list/array.

*   `string title=null` display a title. `null` for no title, only separator.

    UI Toolkit: support rich labels except `<field/>` & `<container/>` tag.

    IMGUI: only support unity standard rich labels

*   `EColor color=EColor.Gray` color for the title and the separator
*   `EAlign eAlign=EAlign.Start` how the title is positioned, options are:
    *   `EAlign.Start`
    *   `EAlign.Center`
    *   `EAlign.End`
*   `int space=0` leave some space above or below the separator, like what `Space` does.

```csharp
using SaintsField;

[SepTitle("Separate Here", EColor.Pink)]
public string content1;

[SepTitle(EColor.Green)]
public string content2;
```

![sep_title](https://github.com/TylerTemp/SaintsField/assets/6391063/55e08b48-4463-4be3-8f87-7afd5ce9e451)

#### `GUIColor` ####

Change the color of the field.

**Override**:

*   `GUIColorAttribute(EColor eColor, float alpha = 1f)`

    Use `EColor` with custom alpha value

*   `GUIColorAttribute(string hexColorOrCallback)`

    Use hex color which starts with `#`, or use a callback, to get the color

*   `GUIColorAttribute(float r, float g, float b, float a = 1f)`

    Use rgb/rgba color (0-1 range)

```csharp
// EColor + alpha
[GUIColor(EColor.Cyan, 0.9f)] public int intField;
// Hex color
[GUIColor("#FFC0CB")] public string[] stringArray;
// rgb/rgba
[GUIColor(112 / 255f, 181 / 255f, 131 / 255f)]
public GameObject lightGreen;
[GUIColor(0, 136 / 255f, 247 / 255f, 0.3f)]
public Transform transparentBlue;

// Dynamic color of field
[GUIColor("$" + nameof(dynamicColor)), Range(0, 10)] public int rangeField;
public Color dynamicColor;

// Dynamic color of callback. `$` can be omitted
[GUIColor(nameof(DynamicColorFunc)), TextArea] public string textArea;
private Color DynamicColorFunc() => dynamicColor;

// validation
[GUIColor("$" + nameof(ValidateColor))]
public int validate;

private Color ValidateColor()
{
    const float c = 207 / 255f;
    return validate < 5 ? Color.red : new Color(c, c, c);
}
```

[![video](https://github.com/user-attachments/assets/94c1aab5-e606-411b-b87b-6e98e632c579)](https://github.com/user-attachments/assets/9ce32483-17f9-4166-b878-f8e067761ca3)

(UI Toolkit implementation code is partly from [EditorAttributes](https://github.com/v0lt13/EditorAttributes/),
go give a star to them!)

### Button ###

#### `AboveButton`/`BelowButton`/`PostFieldButton` ####

There are 3 general buttons:

*   `AboveButton` will draw a button on above
*   `BelowButton` will draw a button on below
*   `PostFieldButton` will draw a button at the end of the field

All of them have the same arguments:

*   `string funcName`

    called when you click the button

*   `string buttonLabel=null`

    label of the button, support tags like `RichLabel`. `null` means using function name as label.

    If it starts with `$`, the leading `$` will be removed and `isCallback` will be set to `true`. Use `\$` to escape the starting `$`.

*   `bool isCallback = false`

    a callback or property name for button's label, same as `RichLabel`

*   `string groupBy = ""`

    See `GroupBy` section. Does **NOT** work on `PostFieldButton`

*   AllowMultiple: Yes

Note: Compared to `Button` in `SaintsEditor`, these buttons can receive the value of the decorated field, and will not get parameter drawers.

```csharp
using SaintsField;

[SerializeField] private bool _errorOut;

[field: SerializeField] private string _labelByField;

[AboveButton(nameof(ClickErrorButton), nameof(_labelByField), true)]
[AboveButton(nameof(ClickErrorButton), "Click <color=green><icon='eye.png' /></color>!")]
[AboveButton(nameof(ClickButton), "$" + nameof(GetButtonLabel), groupBy: "OK")]
[AboveButton(nameof(ClickButton), "$" + nameof(GetButtonLabel), groupBy:  "OK")]

[PostFieldButton(nameof(ToggleAndError), nameof(GetButtonLabelIcon), true)]

[BelowButton(nameof(ClickButton), "$" + nameof(GetButtonLabel), groupBy: "OK")]
[BelowButton(nameof(ClickButton), "$" + nameof(GetButtonLabel), groupBy: "OK")]
[BelowButton(nameof(ClickErrorButton), "Below <color=green><icon='eye.png' /></color>!")]
public int _someInt;

private void ClickErrorButton() => Debug.Log("CLICKED!");

private string GetButtonLabel() =>
    _errorOut
        ? "Error <color=red>me</color>!"
        : "No <color=green>Error</color>!";

private string GetButtonLabelIcon() => _errorOut
    ? "<color=red><icon='eye.png' /></color>"
    : "<color=green><icon='eye.png' /></color>";

private void ClickButton(int intValue)
{
    Debug.Log($"get value: {intValue}");
    if(_errorOut)
    {
        throw new Exception("Expected exception!");
    }
}

private void ToggleAndError()
{
    Toggle();
    if(_errorOut)
    {
        throw new Exception("Expected exception!");
    }
}

private void Toggle() => _errorOut = !_errorOut;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/4e02498e-ae90-4b11-8076-e26256ea0369)](https://github.com/TylerTemp/SaintsField/assets/6391063/f225115b-f7de-4273-be49-d830766e82e7)


#### `Button` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

Draw a button for a function. If the method have arguments (required or optional), it'll draw inputs for these arguments.

*   `string buttonLabel = null` the button label. If null, it'll use the function name. If it starts with `$`, use a callback or field value as the label.
    Rich text is supported.

**Known Issue**: Using dynamic label in `SaintsRow`, the label will not update in real time. This is because a `Serializable` class/struc
field value will be cached by Unity, and reflection can not get an updated value. This issue can not be solved unless
there is a way to reflect the actual value from a cached container.

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

public string dynamicLabel;

[Button("$" + nameof(dynamicLabel))]
private void ButtonWithDynamicLabel()
{
}

[Button("Normal <icon=star.png/>Button Label")]
private void ButtonWithNormalLabel()
{
}

[Button]
private void ButtonWithoutLabel()
{
}
```

![image](https://github.com/user-attachments/assets/54c4d9c1-9309-4a1d-b5e3-f9f69be88305)

Example with arguments:

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

[Button]
private void OnButtonParams(UnityEngine.Object myObj, int myInt, string myStr = "hi")
{
    Debug.Log($"{myObj}, {myInt}, {myStr}");
}
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/7a79ed1f-e227-4cf4-8885-e2ea81f4df3a)

### Game Related ###

#### `Layer` ####

A dropdown selector for layer.

*   AllowMultiple: No

Note: want a bitmask layer selector? Unity already has it. Just use `public LayerMask myLayerMask;`

```csharp
using SaintsField;

[Layer] public string layerString;
[Layer] public int layerInt;

// Unity supports multiple layer selector
public LayerMask myLayerMask;
```

![layer](https://github.com/TylerTemp/SaintsField/assets/6391063/a7ff79a3-f7b8-48ca-8233-5facc975f5eb)

#### `Scene` ####

A dropdown selector for a scene in the build list, plus "Edit Scenes In Build..." option to directly open the "Build Settings" window where you can change building scenes.

**Parameters**:

*   `bool fullPath = false`: `true` to use the full-path name, `false` to use the scene name only. Useful if you have the same scene name under different path. Only works for string field type.

*   AllowMultiple: No

```csharp
using SaintsField;

[Scene] public int _sceneInt;
[Scene] public string _sceneString;
[Scene(true)] public string _sceneFullPath;
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/0da47bd1-0741-4707-b96b-6c08e4c5844c)

#### `SortingLayer` ####

A dropdown selector for sorting layer, plus an "Edit Sorting Layers..." option to directly open "Sorting Layers" tab from "Tags & Layers" inspector where you can change sorting layers.

*   AllowMultiple: No

```csharp
using SaintsField;

[SortingLayer] public string _sortingLayerString;
[SortingLayer] public int _sortingLayerInt;
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/f6633689-012b-4d55-af32-885aa2a2e3cf)

#### `Tag` ####

A dropdown selector for a tag.

*   AllowMultiple: No

```csharp
using SaintsField;

[Tag] public string tag;
```

![tag](https://github.com/TylerTemp/SaintsField/assets/6391063/1a705bce-60ac-4434-826f-69c34055450c)

#### `InputAxis` ####

A string dropdown selector for an input axis, plus an "Open Input Manager..." option to directly open "Input Manager" tab from "Project Settings" window where you can change input axes.

*   AllowMultiple: No

```csharp
using SaintsField;

[InputAxis] public string inputAxis;
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/68dc47d9-7211-48df-bbd1-c11faa536bd1)

#### `ShaderParam` ####

Select a shader parameter from a `shader`, `material` or `renderer`. (Requires Unity 2021.2+)

**Parameters**:

*   [Optional] `string name`: the target. Be a property or a callback that returns a `shader`, `material` or `renderer`.
    When omitted, it will try to get the `Renderer` component from the current component.
*   [Optional] `ShaderPropertyType propertyType`: filter the shader parameters by type. Omitted to show all types.
*   [Optional] `int index=0`: which material index to use when the target is a `Renderer`.

```csharp
[ShaderParam] public string shaderParamString;
[ShaderParam(0)] public int shaderParamInt;
[ShaderParam(ShaderPropertyType.Texture)] public int shaderParamFilter;

[Separator("By Target")]
[GetComponent] public Renderer targetRenderer;

[ShaderParam(nameof(targetRenderer))] public int shaderParamRenderer;

private Material GetMat() => targetRenderer.sharedMaterial;
[ShaderParam(nameof(GetMat))] public int shaderParamMat;

private Shader GetShader() => targetRenderer.sharedMaterial.shader;
[ShaderParam(nameof(GetShader))] public int shaderParamShader;
```

![image](https://github.com/user-attachments/assets/c16ebc4b-434d-4e6c-afd8-4a714c842a06)

#### `ShaderKeyword` ####

Select a shader keyword from a `shader`, `material` or `renderer`. (Requires Unity 2021.2+)

**Parameters**:

*   [Optional] `string name`: the target. Be a property or a callback that returns a `shader`, `material` or `renderer`.
    When omitted, it will try to get the `Renderer` component from the current component.
*   [Optional] `int index=0`: which material index to use when the target is a `Renderer`.

```csharp
[ShaderKeyword] public string shaderKeywordString;
[ShaderKeyword(0)] public string shaderKeywordIndex;

[Separator("By Target")]
[GetComponent] public Renderer targetRenderer;

[ShaderKeyword(nameof(targetRenderer))] public string shaderKeywordRenderer;

private Material GetMat() => targetRenderer.sharedMaterial;
[ShaderKeyword(nameof(GetMat))] public string shaderKeywordMat;

private Shader GetShader() => targetRenderer.sharedMaterial.shader;
[ShaderKeyword(nameof(GetShader))] public string shaderKeywordShader;
```

![image](https://github.com/user-attachments/assets/aff67ea7-1dbc-4f8c-8eaa-572456b7dd07)

### Toggle & Switch ###

#### `GameObjectActive` ####

A toggle button to toggle the `GameObject.activeSelf` of the field.

This does not require the field to be `GameObject`. It can be a component which already attached to a `GameObject`.

*   AllowMultiple: No

```csharp
using SaintsField;

[GameObjectActive] public GameObject _go;
[GameObjectActive] public GameObjectActiveExample _component;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/19944339-4bfa-4b10-b1fb-b50e0b0433e0)](https://github.com/TylerTemp/SaintsField/assets/6391063/ddb0bd02-8869-47b9-aac4-880ab9bfb81a)

#### `SpriteToggle` ####

A toggle button to toggle the `Sprite` of the target.

The field itself must be `Sprite`.

*   `string imageOrSpriteRenderer`

    the target, must be either `UI.Image` or `SpriteRenderer`

*   AllowMultiple: Yes

```csharp
using SaintsField;

[field: SerializeField] private Image _image;
[field: SerializeField] private SpriteRenderer _sprite;

[SerializeField
 , SpriteToggle(nameof(_image))
 , SpriteToggle(nameof(_sprite))
] private Sprite _sprite1;
[SerializeField
 , SpriteToggle(nameof(_image))
 , SpriteToggle(nameof(_sprite))
] private Sprite _sprite2;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/70ae0697-d13b-460d-be8b-30d4f823659b)](https://github.com/TylerTemp/SaintsField/assets/6391063/705498e9-4d70-482f-9ae6-b231cd9497ca)

#### `MaterialToggle` ####

A toggle button to toggle the `Material` of the target.

The field itself must be `Material`.

*   `string rendererName=null`

    the target, must be `Renderer` (or its subClass like `MeshRenderer`). When using null, it will try to get the `Renderer` component from the current component

*   `int index=0`

    which slot index of `materials` on `Renderer` you want to swap

*   AllowMultiple: Yes

```csharp
using SaintsField;

public Renderer targetRenderer;
[MaterialToggle(nameof(targetRenderer))] public Material _mat1;
[MaterialToggle(nameof(targetRenderer))] public Material _mat2;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/cd949e21-e07e-4ee7-8239-280d5d7b8ce1)](https://github.com/TylerTemp/SaintsField/assets/6391063/00c5702c-a41e-42a4-abb1-97a0713c3f66)

#### `ColorToggle` ####

A toggle button to toggle color for `Image`, `Button`, `SpriteRenderer` or `Renderer`

The field itself must be `Color`.

*   `string compName=null`

    the target, must be `Image`, `Button`, `SpriteRenderer`, or `Renderer` (or its subClass like `MeshRenderer`).

    When using `null`, it will try to get the correct component from the target object of this field by order.

    When it's a `Renderer`, it will change the material's `.color` property.

    When it's a `Button`, it will change the button's `targetGraphic.color` property.

*   `int index=0`

    (only works for `Renderer` type) which slot index of `materials` on `Renderer` you want to apply the color

*   AllowMultiple: Yes

```csharp
using SaintsField;

// auto find on the target object
[SerializeField, ColorToggle] private Color _onColor;
[SerializeField, ColorToggle] private Color _offColor;

[Space]
// by name
[SerializeField] private Image _image;
[SerializeField, ColorToggle(nameof(_image))] private Color _onColor2;
[SerializeField, ColorToggle(nameof(_image))] private Color _offColor2;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/ce592999-0912-4c94-85dd-a8e428b1c321)](https://github.com/TylerTemp/SaintsField/assets/6391063/236eea74-a902-4f40-b0b9-ab3f2b7c1dbe)




### Data Editor ###

#### `Expandable` ####

Make serializable object expandable. (E.g. `ScriptableObject`, `MonoBehavior`)

Known issues:
1.  IMGUI: if the target itself has a custom editor drawer, the drawer will not be used, because `PropertyDrawer` is not allowed to create an `Editor` class. Instead, it will use `SaintsEditor` regardless whatever the actual editor is.

    For more information about why this is impossible under IMGUI, see [Issue 25](https://github.com/TylerTemp/SaintsField/issues/25)

2.  IMGUI: the `Foldout` will NOT be placed at the left space like a Unity's default foldout component, because Unity limited the `PropertyDrawer` to be drawn inside the rect Unity gives. Trying outside the rect will make the target non-interactable.
    But in early Unity (like 2019.1), Unity will force `Foldout` to be out of rect on top leve, but not on array/list level... so you may see different outcomes on different Unity version.

    If you see unexpected space or overlap between foldout and label, use `Window` - `Saints` - `Create or Edit SaintsField Config` to change the config.

3.  UI Toolkit: `ReadOnly` (and `DisableIf`, `EnableIf`) can NOT disable the expanded fields. This is because `InspectorElement` does not work with `SetEnable(false)`, neither with `pickingMode=Ignore`. This can not be fixed unless Unity fixes it.

*   Allow Multiple: No

```csharp
using SaintsField;

[Expandable] public ScriptableObject _scriptable;
```

![expandable](https://github.com/TylerTemp/SaintsField/assets/6391063/92fd1f45-82c5-4d5e-bbc4-c9a70fefe158)

#### `ReferencePicker` ####

A dropdown to pick a referenced value for Unity's [`SerializeReference`](https://docs.unity3d.com/ScriptReference/SerializeReference.html).

You can use this to pick non UnityObject object like `interface` or polymorphism `class`.

Limitation:
1.  The target must have a public constructor with no required arguments.
2.  It'll try to copy field values when changing types but not guaranteed. `struct` will not get copied value (it's too tricky to deal a struct)

Parameters:

*   `bool hideLabel=false`: true to hide the label of picked type

*   Allow Multiple: No

```csharp
using SaintsField;

[Serializable]
public class Base1Fruit
{
    public GameObject base1;
}

[Serializable]
public class Base2Fruit: Base1Fruit
{
    public int base2;
}

[Serializable]
public class Apple : Base2Fruit
{
    public string apple;
    public GameObject applePrefab;
}

[Serializable]
public class Orange : Base2Fruit
{
    public bool orange;
}

[SerializeReference, ReferencePicker]
public Base2Fruit item;

public interface IRefInterface
{
    public int TheInt { get; }
}

// works for struct
[Serializable]
public struct StructImpl : IRefInterface
{
    [field: SerializeField]
    public int TheInt { get; set; }
    public string myStruct;
}

[Serializable]
public class ClassDirect: IRefInterface
{
    [field: SerializeField, Range(0, 10)]
    public int TheInt { get; set; }
}

// abstruct type will be skipped
public abstract class ClassSubAbs : ClassDirect
{
    public abstract string AbsValue { get; }
}

[Serializable]
public class ClassSub1 : ClassSubAbs
{
    public string sub1;
    public override string AbsValue => $"Sub1: {sub1}";
}

[Serializable]
public class ClassSub2 : ClassSubAbs
{
    public string sub2;
    public override string AbsValue => $"Sub2: {sub2}";
}

[SerializeReference, ReferencePicker]
public IRefInterface myInterface;
```

![reference_picker](https://github.com/TylerTemp/SaintsField/assets/6391063/06b1a8f6-806e-49c3-b491-a810bc885595)

#### `SaintsRow` ####

`SaintsRow` attribute allows you to draw `Button`, `Layout`, `ShowInInspector`, `DOTweenPlay` etc. (all `SaintsEditor` attributes) in a `Serializable` object (usually a class or a struct).

This attribute does NOT need `SaintsEditor` enabled. It's an out-of-box tool.

Parameters:

*   `bool inline=false`

    If true, it'll draw the `Serializable` inline like it's directly in the `MonoBehavior`

*   AllowMultiple: No

Special Note:

1.  After applying this attribute, only pure `PropertyDrawer`, and decorators from `SaintsEditor` works on this target. Which means, using third party's `PropertyDrawer` is fine, but decorator of Editor level (e.g. Odin's `Button`, NaughtyAttributes' `Button`) will not work.
2.  IMGUI: `DOTweenPlay` might be a bit buggy displaying the playing/pause/stop status for each function.

```csharp
using SaintsField;
using SaintsField.Playa;  // SaintsEditor is not required here

[Serializable]
public struct Nest
{
    public string nest2Str;  // normal field
    [Button]  // function button
    private void Nest2Btn() => Debug.Log("Call Nest2Btn");
    // static field (non serializable)
    [ShowInInspector] public static Color StaticColor => Color.cyan;
    // const field (non serializable)
    [ShowInInspector] public const float Pi = 3.14f;
    // normal attribute drawer works as expected
    [BelowImage(maxWidth: 25)] public SpriteRenderer spriteRenderer;

    [DOTweenPlay]  // DOTween helper
    private Sequence PlayColor()
    {
        return DOTween.Sequence()
            .Append(spriteRenderer.DOColor(Color.red, 1f))
            .Append(spriteRenderer.DOColor(Color.green, 1f))
            .Append(spriteRenderer.DOColor(Color.blue, 1f))
            .SetLoops(-1);
    }
    [DOTweenPlay("Position")]
    private Sequence PlayTween2()
    {
        return DOTween.Sequence()
                .Append(spriteRenderer.transform.DOMove(Vector3.up, 1f))
                .Append(spriteRenderer.transform.DOMove(Vector3.right, 1f))
                .Append(spriteRenderer.transform.DOMove(Vector3.down, 1f))
                .Append(spriteRenderer.transform.DOMove(Vector3.left, 1f))
                .Append(spriteRenderer.transform.DOMove(Vector3.zero, 1f))
            ;
    }
}

[SaintsRow]
public Nest n1;
```

![saints_row](https://github.com/TylerTemp/SaintsField/assets/6391063/d8465de6-0741-4bfb-aa0d-3042422ca56c)

To show a `Serializable` inline like it's directly in the `MonoBehavior`:

```csharp
using SaintsField;

[Serializable]
public struct MyStruct
{
    public int structInt;
    public bool structBool;
}

[SaintsRow(inline: true)]
public MyStruct myStructInline;

public string normalStringField;
```

![saints_row_inline](https://github.com/TylerTemp/SaintsField/assets/6391063/571f4a05-91e0-4860-9ea2-bff6b1fe1d58)

**SerializeReference**

`SaintsRow` can work on `SerializeReference`. If using it together with `ReferencePicker`, ensure `ReferencePicker` is before `SaintsRow`!

```csharp
using SaintsField;

public interface IRefInterface
{
    public int TheInt { get; }
}

[Serializable]
public struct StructImpl : IRefInterface
{
    [field: SerializeField]
    public int TheInt { get; set; }
    [LayoutStart("Hi", ELayout.FoldoutBox)]
    public string myStruct;

    public ClassDirect nestedClass;
}

[SerializeReference, ReferencePicker, SaintsRow]
public IRefInterface saints;

[SerializeReference, ReferencePicker(hideLabel: true), SaintsRow(inline: true)]
public IRefInterface inline;
```

![image](https://github.com/user-attachments/assets/a3735317-68bf-4a43-a97f-5d19c2d0c96c)

**Drawer**

alternatively, you can make a drawer for your data type to omit `[SaintsRow]` everywhere:

```csharp
using SaintsField.Editor.Playa;

[CustomPropertyDrawer(typeof(Nest))]
public class MySaintsRowAttributeDrawer: SaintsRowAttributeDrawer {}
```

#### `ListDrawerSettings` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

Allow you to search and paging a large list/array.

Parameters:

*   `bool searchable = false`: allow search in the list/array
*   `int numberOfItemsPerPage = 0`: how many items per page by default. `<=0` means no paging
*   [IMGUI] `bool delayedSearch = false`: when `true`, delay the search until you hit enter or blur the search field
*   `string extraSearch = null`: set a callback function to use your custom search. If not match, use the default search.
*   `string overrideSearch = null`: set a callback function as a custom search. When present, ignore `extraSearch` and default search.

`delayedSearch` only works for IMGUI. For UI Toolkit, it already has a debounced search, which means:

*   When input anything, it'll wait for 0.6 seconds for next input, then perform the actual searching
*   You can always use `Enter` to search immediately

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

[Serializable]
public struct MyData
{
    public int myInt;
    public string myString;
    public GameObject myGameObject;
    public string[] myStrings;
}

[ListDrawerSettings(searchable: true, numberOfItemsPerPage: 3)]
public MyData[] myDataArr;
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/08c6da9a-e613-4e94-8933-3d7a92f7cb33)

The first input is where you can search. The next input can adjust how many items per page. The last part is the paging.

**Async Search**

In UI Toolkit you can also see the async searching which does not block the editor when searching in a BIG list:

[![video](https://github.com/user-attachments/assets/f76a68c5-fc27-4ecd-ab4b-eebec37f882d)](https://github.com/user-attachments/assets/3b4a1fd7-c5fb-4ed5-98ed-66645abb5512)

**Custom Search**

`extraSearch` & `overrideSearch` uses the following signiture:

*   `bool CustomSearch(T item, int index, IReadOnlyList<SaintsField.Playa.ListSearchToken> seachToken)`
*   `bool CustomSearch(T item, IReadOnlyList<SaintsField.Playa.ListSearchToken> seachToken)`
*   `bool CustomSearch(int index, IReadOnlyList<SaintsField.Playa.ListSearchToken> seachToken)`

`ListSearchToken` is a struct of:

```csharp
public readonly struct ListSearchToken
{
    public readonly ListSearchType Type;  // filter type: Include, Exclude
    public readonly string Token;  // search string
}
```

example:

```csharp
[Serializable]
public enum WeaponType
{
    Sword,
    Arch,
    Hammer,
}

[Serializable]
public struct Weapon
{
    public WeaponType weaponType;
    public string description;
}

private bool ExtraSearch(Weapon weapon, int _, IReadOnlyList<ListSearchToken> tokens)
{
    string searchName = new Dictionary<WeaponType, string>
    {
        { WeaponType.Arch , "弓箭 双手" },
        { WeaponType.Sword , "刀剑 单手" },
        { WeaponType.Hammer, "大锤 双手" },
    }[weapon.weaponType];

    foreach (ListSearchToken token in tokens)
    {
        if (token.Type == ListSearchType.Exclude && searchName.Contains(token.Token))
        {
            return false;
        }

        if (token.Type == ListSearchType.Include && !searchName.Contains(token.Token))
        {
            return false;
        }
    }

    return true;
}

[ListDrawerSettings(extraSearch: nameof(ExtraSearch))]
public Weapon[] weapons;
```

You can now search as you want, both your custom search & serialized property search:

[![video](https://github.com/user-attachments/assets/a32b3592-68f6-4207-8142-3d895c926de1)](https://github.com/user-attachments/assets/d7fd4e08-355f-460b-88fb-2e874f58cb01)

#### `Table` ####

Show a list/array of class/struct/`ScriptableObject`(or `MonoBehavior` if you like) as a table.

It allows to resize the rows, hide rows.

UI Toolkit: `Button`, `ShowInInspector` & `Playa*` will work as expected, and `Layout` will be ignored.

Note:
1.  It's highly recommended to enable `SaintsEditor`, otherwise the outside `list` will always be visible with some empty rows.
2.  for UI Toolkit user: it requires Unity 2022.2+, otherwise it'll fall back to IMGUI.
3.  IMGUI: complex field might not resize properly even I've set up the height function as Unity's document said. Drag component like `Range` sometimes will get triggered even out of the drawing area. This can not be fixed unless Unity gives a guild of how to resolve it.

**Parameters**:

*   `bool defaultExpanded=false`: Should the foldout be expanded by default?
*   `bool hideAddButton=false`: Should the add button be hidden?
*   `bool hideRemoveButton=false`: Shoule the remove button be hidden?

```csharp
using SaintsField;

[Table]
public Scriptable[] scriptableArray;

[Serializable]
public struct MyStruct
{
    public int myInt;
    public string myString;
    public GameObject myObject;
}

[Table]
public MyStruct[] myStructs;
```

[![video](https://github.com/user-attachments/assets/82193a57-c051-4188-950d-9e7a9ee6e08d)](https://github.com/user-attachments/assets/1c574c0c-56e0-4912-8e00-49fb7e29d80c)

**`TableColumn`**

`TableColumn` allows you to merge multiple fields into one column.

```csharp
[Serializable]
public struct MyStruct
{
    public int myInt;

    [TableColumn("Value"), AboveRichLabel]
    public string myString;
    [TableColumn("Value"), AboveRichLabel]
    public GameObject myObject;
}

[Table]
public List<MyStruct> myStructs;
```

![image](https://github.com/user-attachments/assets/53a20670-c281-49e1-a034-c11d96d270bc)

For UI Toolkit, You can also use `Button`, `ShowInInspector` etc.:

```csharp
using SaintsField;
using SaintsField.Playa;

[Serializable]
public struct MyValueStruct
{
    // ...

    [TableColumn("Buttons")]
    [Button("Ok")]
    public void BtnOk() {}

    [TableColumn("Buttons")]
    [Button("Cancel")]
    public void BtnCancel() {}

    [ShowInInspector] private int _showI;
}

[Table, DefaultExpand]
public MyValueStruct[] myStructs;
```

![image](https://github.com/user-attachments/assets/01190ea1-97b7-4654-84ea-1ca4388739a9)

**`TableHide`**

> [!NOTE]
> This feature is UI Toolkit only

You can use `TableHide` attribute to exclude some column from the table. It'll hide the column by default, and you can still toggle it in header - right click menu

```csharp
[Serializable]
public struct MyStruct
{
    // Hide a single row
    [TableHide] public int hideMeInTable;

    // Hide a grouped column
    [TableColumn("HideGroup"), TableHide]
    public int hideMeGroup1;

    [TableColumn("HideGroup")]
    [ShowInInspector] private const int HideMeGroup2 = 2;

}

[Table]
public List<MyStruct> myStructs;
```

[![video](https://github.com/user-attachments/assets/2bf6480a-65f7-4dfc-bf96-bfee9497428e)](https://github.com/user-attachments/assets/66fa7d10-427c-4f8c-b23f-f4bb29faa9f6)

**`TableHeaders`/`TableHeadersHide`**

> [!NOTE]
> This feature is UI Toolkit only

You can use `TableHeaders` to default show some columns for the table, or `TableHeadersHide` to hide them.

Note: this does not remove the header, but hide it by default. You can still toggle it in header - right click menu.

Thus, it'll only affect the appearance when the table is rendered, and will NOT dynamicly update it, unless you select away and back, as it will trigger the re-paint process.

**Parameters**:

*   `string headers...`: the headers to show/hide.

    If it starts with `$`, a callback/property/field value is used. The target must return a string, or `IEnumerable<string>`

```csharp
using SaintsField;

[Serializable]
public struct TableHeaderStruct
{
    public int i1;

    [TableColumn("Custom Header")] public int i2;
    [TableColumn("Custom Header")] [Button] private void D() {}

    public string str1;
    public string str2;

    [TableColumn("String")] public string str3;
    [TableColumn("String")] public string str4;

    public string str5;
    public string str6;
}

[Table]
[TableHeaders(  // what should be shown by default
    nameof(TableHeaderStruct.i1),  // directly name
    "Custom Header",  // directly custom name
    "$" + nameof(showTableHeader),  // callback of a single name
    "$" + nameof(ShowTableHeaders))  // callback of mutiple names
]
public TableHeaderStruct[] tableStruct;

[Table]
[TableHeadersHide(  // what should be hidden by default
        nameof(TableHeaderStruct.i1),  // directly name
        "Custom Header",  // directly custom name
        "$" + nameof(showTableHeader),  // callback of a single name
        "$" + nameof(ShowTableHeaders))  // callback of mutiple names
]
public TableHeaderStruct[] tableHideStruct;

[Space]
public string showTableHeader = nameof(TableHeaderStruct.str2);

protected virtual IEnumerable<string> ShowTableHeaders() => new[]
{
    nameof(TableHeaderStruct.str5),
    nameof(TableHeaderStruct.str6),
};
```

Then you can inherent or change field to make the table display differently

```csharp
public class TableHeadersExampleInh : TableHeadersExample
{
    protected override IEnumerable<string> ShowTableHeaders() => new[]
    {
        "String",
    };
}
```

Results:

![image](https://github.com/user-attachments/assets/c33a6875-bebd-4998-bfec-97575ac781ec)

#### `ShowInInspector` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

Show a non-field property.

For UI Toolkit: this attribute allow you to edit the corresponing field like odin do. Limitation:

1.  UI Toolkit only
2.  Does not use custom drawer even the type has one (Same as Odin)

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

// const
[ShowInInspector, Ordered] public const float MyConstFloat = 3.14f;
// static
[ShowInInspector, Ordered] public static readonly Color MyColor = Color.green;

// auto-property
[ShowInInspector, Ordered]
public Color AutoColor
{
    get => Color.green;
    set {}
}
```

![show_in_inspector](https://github.com/TylerTemp/SaintsField/assets/6391063/3e6158b4-6950-42b1-b102-3c8884a59899)

UI Toolkit: A null-class can be created, edited and set to `null`:

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

private class MyClass
{
    public string MyString;
}

[ShowInInspector] private MyClass _myClass;
[ShowInInspector] private MyClass _myClassD = new MyClass
{
    MyString = "Hi",
};
```

[![video](https://github.com/user-attachments/assets/b1c69072-76af-46ac-9c20-c3e8f6671e84)](https://github.com/user-attachments/assets/8928b38c-d756-4ff5-b924-e4b5a0f3543e)

UI Toolkit: An array/list can be created, edited and set to `null`:

```csharp
using SaintsField;

private class MyClass
{
    public string MyString;
    public GameObject MyObj;
    private MyEnum _myEnum;
}

[ShowInInspector] private Color[] _colors = {Color.red, Color.green, Color.blue};
[ShowInInspector, Ordered] private Color[] _colorEmptyArray;

[Button, Ordered]
private void ArrayChange0ToRed()
{
    _colorEmptyArray[0] = Color.red;
}

[ShowInInspector, Ordered] private MyClass[] _myClasses;
```

[![video](https://github.com/user-attachments/assets/99201b88-439c-4508-a1cf-04cf32748ca7)](https://github.com/user-attachments/assets/0baed2e8-13ad-41f8-88e4-9a8bb32d0dd1)

UI Toolkit: It can also create/edit an interface. Depending on the actual type is Unity Object or general class/struct, it'll show object picker or field editor accordingly.

```csharp
public class GeneralDummyClass: IDummy
{
    public string GetComment()
    {
        return "DummyClass";
    }

    public int MyInt { get; set; }
    public int GenDumInt;
    public string GenDumString;
}

[ShowInInspector] private static IDummy _dummy;

[Button]
private void DebugDummy() => Debug.Log(_dummy);
```

[![video](https://github.com/user-attachments/assets/ea978ef2-6c6f-492d-8c2d-1befffe014d9)](https://github.com/user-attachments/assets/ca9bc4e5-abb3-4635-b41b-96101d63d264)

UI Toolkit: dictionary/`IReadOnlyDictionary` is now supported

```csharp
[ShowInInspector] private Dictionary<string, int> _myDictionaryNull;

[Button]
private void DictExternalAdd()
{
    _myDictionaryNull["External"] = 1;
}
```

[![video](https://github.com/user-attachments/assets/dd3e7add-36f3-4f59-918c-58022d68cac6)](https://github.com/user-attachments/assets/57baefa0-144c-4c7f-8100-dd7b102d3935)

### Numerical ###

#### `Rate` ####

A rating stars tool for an `int` field.

Parameters:

*   `int min` minimum value of the rating. Must be equal to or greater than 0.

    When it's equal to 0, it'll draw a red slashed star to select `0`.

    When it's greater than 0, it will draw `min` number of fixed stars that you can not un-rate.

*   `int max` maximum value of the rating. Must be greater than `min`.

*   AllowMultiple: No

```csharp
using SaintsField;

[Rate(0, 5)] public int rate0To5;
[Rate(1, 5)] public int rate1To5;
[Rate(3, 5)] public int rate3To5;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/c3a0509e-b211-4a62-92c8-7bc8c3866cf4)](https://github.com/TylerTemp/SaintsField/assets/6391063/a506681f-92f8-42ab-b08d-483b26b2f7c3)

#### `PropRange` ####

Very like Unity's `Range` but allow you to dynamically change the range, plus allow to set range step.

For each argument:

*   `string minCallback` or `float min`: the minimum value of the slider, or a property/callback name.
*   `string maxCallback` or `float max`: the maximum value of the slider, or a property/callback name.
*   `float step=-1f`: the step for the range. `<= 0` means no limit.

```csharp
using SaintsField;

public int min;
public int max;

[PropRange(nameof(min), nameof(max))] public float rangeFloat;
[PropRange(nameof(min), nameof(max))] public int rangeInt;

[PropRange(nameof(min), nameof(max), step: 0.5f)] public float rangeFloatStep;
[PropRange(nameof(min), nameof(max), step: 2)] public int rangeIntStep;
```

![range](https://github.com/TylerTemp/SaintsField/assets/6391063/55f3fc5a-3ee4-4bd8-9b0a-cd016a7a79e7)

**Adapt**

`PropRange` can work with `[Adapt(EUnit.Percent)]` to show a percent value, but still get the actual float value:

```csharp
[
    PropRange(0f, 1f, step: 0.05f),
    Adapt(EUnit.Percent),
    OverlayRichLabel("<color=gray>%", end: true),
    BelowRichLabel("$" + nameof(DisplayActualValue)),
] public float stepRange;

private string DisplayActualValue(float av) => $"<color=gray>Actual Value: {av}";
```

[![video](https://github.com/user-attachments/assets/82381ab1-9405-4fdc-bf5f-5c9debb56136)](https://github.com/user-attachments/assets/bb322c3a-56ba-48a4-bcba-7c3908ae2c34)

#### `MinMaxSlider` ####

A range slider for `Vector2` or `Vector2Int`

For each argument:

*   `int|float min` or `string minCallback`: the minimum value of the slider, or a property/callback name.
*   `int|float max` or `string maxCallback`: the maximum value of the slider, or a property/callback name.
*   `int|float step=1|-1f`: the step of the slider, `<= 0` means no limit. By default, int type use `1` and float type use `-1f`
*   `float minWidth=50f`: (IMGUI Only) the minimum width of the value label. `< 0` for auto size (not recommended)
*   `float maxWidth=50f`: (IMGUI Only) the maximum width of the value label. `< 0` for auto size (not recommended)
*   `bool free=false`: `true` to allow you manually input the value without getting limited by the slider (and the min/max value).

*   AllowMultiple: No

a full-featured example:

```csharp
using SaintsField;

[MinMaxSlider(-1f, 3f, 0.3f)]
public Vector2 vector2Step03;

[MinMaxSlider(0, 20, 3)]
public Vector2Int vector2IntStep3;

[MinMaxSlider(-1f, 3f)]
public Vector2 vector2Free;

[MinMaxSlider(0, 20)]
public Vector2Int vector2IntFree;

// not recommended
[SerializeField]
[MinMaxSlider(0, 100, minWidth:-1, maxWidth:-1)]
private Vector2Int _autoWidth;

[field: SerializeField, MinMaxSlider(-100f, 100f)]
public Vector2 OuterRange { get; private set; }

[SerializeField, MinMaxSlider(nameof(GetOuterMin), nameof(GetOuterMax), 1)] public Vector2Int _innerRange;

private float GetOuterMin() => OuterRange.x;
private float GetOuterMax() => OuterRange.y;

[field: SerializeField]
public float DynamicMin { get; private set; }
[field: SerializeField]
public float DynamicMax { get; private set; }

[SerializeField, MinMaxSlider(nameof(DynamicMin), nameof(DynamicMax))] private Vector2 _propRange;
[SerializeField, MinMaxSlider(nameof(DynamicMin), 100f)] private Vector2 _propLeftRange;
[SerializeField, MinMaxSlider(-100f, nameof(DynamicMax))] private Vector2 _propRightRange;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/3da0ea31-d830-4ac6-ab1d-8305764162f5)](https://github.com/TylerTemp/SaintsField/assets/6391063/2ffb659f-a5ed-4861-b1ba-65771db5ab47)

Example of free input mode:

```csharp
[MinMaxSlider(0, 10, free: true)] public Vector2 freeInput;
```

[![video](https://github.com/user-attachments/assets/6843ae58-0742-402b-a96a-5ae6ce531271)](https://github.com/user-attachments/assets/00ef50ba-98bd-4812-8664-8066b31c769a)

#### `ProgressBar` ####

A progress bar for `float` or `int` field. This behaves like a slider but more fancy.

Note: Unlike NaughtyAttributes (which is read-only), this **is interactable**.

Parameters:

*   (Optional) `float minValue=0` | `string minCallback=null`: minimum value of the slider
*   `float maxValue=100` | `string maxCallback=null`: maximum value of the slider
*   `float step=-1`: the growth step of the slider, `<= 0` means no limit.
*   `EColor color=EColor.OceanicSlate`: filler color
*   `EColor backgroundColor=EColor.CharcoalGray`: background color
*   `string colorCallback=null`: a callback or property name for the filler color. The function must return a `EColor`, `Color`, a name of `EColor`/`Color`, or a hex color string (starts with `#`). This will override `color` parameter.
*   `string backgroundColorCallback=null`: a callback or property name for the background color.
*   `string titleCallback=null`: a callback for displaying the title. The function signature is:

    ```csharp
    string TitleCallback(float curValue, float min, float max, string label);
    ```

    rich text is not supported here

```csharp
using SaintsField;

[ProgressBar(10)] public int myHp;
// control step for float rather than free value
[ProgressBar(0, 100f, step: 0.05f, color: EColor.Blue)] public float myMp;

[Space]
public int minValue;
public int maxValue;

[ProgressBar(nameof(minValue)
        , nameof(maxValue)  // dynamic min/max
        , step: 0.05f
        , backgroundColorCallback: nameof(BackgroundColor)  // dynamic background color
        , colorCallback: nameof(FillColor)  // dynamic fill color
        , titleCallback: nameof(Title)  // dynamic title, does not support rich label
    ),
]
[RichLabel(null)]  // make this full width
public float fValue;

private EColor BackgroundColor() => fValue <= 0? EColor.Brown: EColor.CharcoalGray;

private Color FillColor() => Color.Lerp(Color.yellow, EColor.Green.GetColor(), Mathf.Pow(Mathf.InverseLerp(minValue, maxValue, fValue), 2));

private string Title(float curValue, float min, float max, string label) => curValue < 0 ? $"[{label}] Game Over: {curValue}" : $"[{label}] {curValue / max:P}";
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/74085d85-e447-4b6b-a3ff-1bd2f26c5d73)](https://github.com/TylerTemp/SaintsField/assets/6391063/11ad0700-32ba-4280-ae7b-6b6994c9de83)

### Animation ###

#### `AnimatorParam` ###

A dropdown selector for an animator parameter.

*   `string animatorName=null`

    name of the animator. When omitted, it will try to get the animator from the current component

*   (Optional) `AnimatorControllerParameterType animatorParamType`

    type of the parameter to filter

```csharp
using SaintsField;

[field: SerializeField]
public Animator Animator { get; private set;}

[AnimatorParam(nameof(Animator))]
private string animParamName;

[AnimatorParam(nameof(Animator))]
private int animParamHash;
```

![animator_params](https://github.com/TylerTemp/SaintsField/assets/6391063/3cd5fb6d-1a75-457c-9bbd-e1e6b377a83c)

#### `AnimatorState` ###

A dropdown selector for animator state.

*   `string animatorName=null`

    name of the animator. When omitted, it will try to get the animator from the current component

to get more useful info from the state, you can use `AnimatorStateBase`/`AnimatorState` type instead of `string` type.

`AnimatorStateBase` has the following properties:

*   `int layerIndex` index of layer
*   `int stateNameHash` hash value of state
*   `string stateName` actual state name
*   `float stateSpeed` the `Speed` parameter of the state
*   `string stateTag` the `Tag` of the state
*   `string[] subStateMachineNameChain` the substate machine hierarchy name list of the state

`AnimatorState` added the following attribute(s):

*   `AnimationClip animationClip` is the actual animation clip of the state (can be null). It has a `length` value for the length of the clip. For more detail see [Unity Doc of AnimationClip](https://docs.unity3d.com/ScriptReference/AnimationClip.html)

Special Note: using `AniamtorState`/`AnimatorStateBase` with `OnValueChanged`, you can get a `AnimatorStateChanged` on the callback (rather than the value of the field).
This is because `AnimatorState` expected any class/struct with satisfied fields.

```csharp
using SaintsField;

[AnimatorState, OnValueChanged(nameof(OnChanged))]
public string stateName;

#if UNITY_EDITOR
[AnimatorState, OnValueChanged(nameof(OnChangedState))]
#endif
public AnimatorState state;

// This does not have a `animationClip`, thus it won't include a resource when serialized: only pure data.
[AnimatorState, OnValueChanged(nameof(OnChangedState))]
public AnimatorStateBase stateBase;

private void OnChanged(string changedValue) => Debug.Log(changedValue);
#if UNITY_EDITOR
private void OnChangedState(AnimatorStateChanged changedValue) => Debug.Log($"layerIndex={changedValue.layerIndex}, AnimatorControllerLayer={changedValue.layer}, AnimatorState={changedValue.state}, animationClip={changedValue.animationClip}, subStateMachineNameChain={string.Join("/", changedValue.subStateMachineNameChain)}");
#endif
```

![animator_state](https://github.com/TylerTemp/SaintsField/assets/6391063/8ee35de5-c7d5-4f0d-b8b7-8feeac41c31d)

#### `CurveRange` ####

A curve drawer for `AnimationCurve` which allow to set bounds and color

Override 1:

*   `Vector2 min = Vector2.zero` bottom left for bounds
*   `Vector2 max = Vector2.one` top right for bounds
*   `EColor color = EColor.Green` curve line color

Override 2:

*   `float minX = 0f` bottom left x for bounds
*   `float minY = 0f` bottom left y for bounds
*   `float maxX = 1f` top right x for bounds
*   `float maxY = 1f` top right y for bounds
*   `EColor color = EColor.Green` curve line color

```csharp
using SaintsField;

[CurveRange(-1, -1, 1, 1)]
public AnimationCurve curve;

[CurveRange(EColor.Orange)]
public AnimationCurve curve1;

[CurveRange(0, 0, 5, 5, EColor.Red)]
public AnimationCurve curve2;
```

![curverange](https://github.com/TylerTemp/SaintsField/assets/6391063/7c10ebb4-ab93-4192-ad05-5e2c3addcfe9)


### Auto Getter ###

Note: You can change the default behavior of these attributes using `Window/Saints/Create or Edit SaintsField Config`

#### `GetComponent` ####

Automatically sign a component to a field, if the field value is null and the component is already attached to current target. (First one found will be used)

*   (Optional) `EXP config`: config. See `Saints XPath-like Syntax` section for more information.

    Note: You can change the default behavior of these attributes using `Window/Saints/Create or Edit SaintsField Config`

*   `Type compType = null`

    The component type to sign. If null, it'll use the field type.

*   `string groupBy = ""`

    For error message grouping.

*   AllowMultiple: No

```csharp
using SaintsField;

[GetComponent] public BoxCollider otherComponent;
[GetComponent] public GameObject selfGameObject;  // get the GameObject itself
[GetComponent] public RectTransform selfRectTransform;  // useful for UI

[GetComponent] public GetComponentExample selfScript;  // yeah you can get your script itself
[GetComponent] public Dummy otherScript;  // other script
```

![get_component](https://github.com/TylerTemp/SaintsField/assets/6391063/a5e9ca85-ab23-4b4a-b228-5d19c66c4052)

#### `GetComponentInChildren` ####

Automatically sign a component to a field, if the field value is null and the component is already attached to itself or its child GameObjects. (First one found will be used)

NOTE: Like `GetComponentInChildren` by Unity, this **will** check the target object itself.

*   (Optional) `EXP config`: config. See `Saints XPath-like Syntax` section for more information.

    Note: You can change the default behavior of these attributes using `Window/Saints/Create or Edit SaintsField Config`

*   `bool includeInactive = false`

    Should inactive children be included? `true` to include inactive children.

*   `Type compType = null`

    The component type to sign. If null, it'll use the field type.

*   `bool excludeSelf = false`

    When `true`, skip checking the target itself.

*   `string groupBy = ""`

    For error message grouping.

*   AllowMultiple: No

```csharp
using SaintsField;

[GetComponentInChildren] public BoxCollider childBoxCollider;
// by setting compType, you can sign it as a different type
[GetComponentInChildren(compType: typeof(Dummy))] public BoxCollider childAnotherType;
// and GameObject field works too
[GetComponentInChildren(compType: typeof(BoxCollider))] public GameObject childBoxColliderGo;
```

![get_component_in_children](https://github.com/TylerTemp/SaintsField/assets/6391063/854aeefc-6456-4df2-a4a7-40a5cd5e2290)

#### `GetComponentInParent` / `GetComponentInParents` ####

Automatically sign a component to a field, if the field value is null and the component is already attached to its parent GameObject(s). (First one found will be used)

Note:

1.  Like Unity's `GetComponentInParent`, this **will** check the target object itself.
2.  `GetComponentInParent` will only check the target & its direct parent. `GetComponentInParents` will search all the way up to the root.

Parameters:

*   (Optional) `EXP config`: config. See `Saints XPath-like Syntax` section for more information.

    Note: You can change the default behavior of these attributes using `Window/Saints/Create or Edit SaintsField Config`

*   (For `GetComponentInParents` only) `bool includeInactive = false`

    Should inactive GameObject be included? `true` to include inactive GameObject.

    Note: **only `GetComponentInParents` has this parameter!**

*   `Type compType = null`

    The component type to sign. If null, it'll use the field type.

*   `string groupBy = ""`

    For error message grouping.

*   AllowMultiple: No

```csharp
using SaintsField;

[GetComponentInParent] public SpriteRenderer directParent;  // equals [GetByXPath("//parent::")]
[GetComponentInParent(typeof(SpriteRenderer))] public GameObject directParentDifferentType;  // equals [GetByXPath("//parent::/[@GetComponent(SpriteRenderer)]")]
[GetComponentInParent] public BoxCollider directNoSuch;

[GetComponentInParents] public SpriteRenderer searchParent;  // equals [GetByXPath("//ancestor::")]
[GetComponentInParents(compType: typeof(SpriteRenderer))] public GameObject searchParentDifferentType;
[GetComponentInParents] public BoxCollider searchNoSuch;
```

![get_component_in_parents](https://github.com/TylerTemp/SaintsField/assets/6391063/02836529-1aff-4bc9-b849-203d7bdaad21)

#### `GetComponentInScene` ####

Automatically sign a component to a field, if the field value is null and the component is in the currently opened scene. (First one found will be used)

*   (Optional) `EXP config`: config. See `Saints XPath-like Syntax` section for more information.

    Note: You can change the default behavior of these attributes using `Window/Saints/Create or Edit SaintsField Config`

*   `bool includeInactive = false`

    Should inactive GameObject be included? `true` to include inactive GameObject.

*   `Type compType = null`

    The component type to sign. If null, it'll use the field type.

*   `string groupBy = ""`

    For error message grouping.

*   AllowMultiple: No

```csharp
using SaintsField;

[GetComponentInScene] public Dummy dummy;
// by setting compType, you can sign it as a different type
[GetComponentInScene(compType: typeof(Dummy))] public RectTransform dummyTrans;
// and GameObject field works too
[GetComponentInScene(compType: typeof(Dummy))] public GameObject dummyGo;
```

![get_component_in_scene](https://github.com/TylerTemp/SaintsField/assets/6391063/95a008a2-c7f8-4bc9-90f6-57c58724ebaf)

#### `GetPrefabWithComponent` ####

Automatically sign a prefab to a field, if the field value is null and the prefab has the component. (First one found will be used)

Recommended to use it with `FieldType`!

*   (Optional) `EXP config`: config. See `Saints XPath-like Syntax` section for more information.

    Note: You can change the default behavior of these attributes using `Window/Saints/Create or Edit SaintsField Config`

*   `Type compType = null`

    The component type to sign. If null, it'll use the field type.

*   `string groupBy = ""`

    For error message grouping.

*   AllowMultiple: No

```csharp
using SaintsField;

[GetPrefabWithComponent] public Dummy dummy;
// get the prefab itself
[GetPrefabWithComponent(compType: typeof(Dummy))] public GameObject dummyPrefab;
// works so good with `FieldType`
[GetPrefabWithComponent(compType: typeof(Dummy)), FieldType(typeof(Dummy))] public GameObject dummyPrefabFieldType;
```

![get_prefab_with_component](https://github.com/TylerTemp/SaintsField/assets/6391063/07eae93c-d2fc-4641-b71f-55a98f17b360)

#### `GetScriptableObject` ####

Automatically sign a `ScriptableObject` file to this field. (First one found will be used)

Recommended to use it with `Expandable`!

*   (Optional) `EXP config`: config. See `Saints XPath-like Syntax` section for more information.

    Note: You can change the default behavior of these attributes using `Window/Saints/Create or Edit SaintsField Config`

*   `string pathSuffix=null` the path suffix for this `ScriptableObject`. `null` for no limit. for example: if it's `/Resources/mySo`, it will only sign the file whose path is ends with `/Resources/mySo.asset`, like `Assets/proj/Resources/mySo.asset`
*   AllowMultiple: No

```csharp
using SaintsField;

[GetScriptableObject] public Scriptable mySo;
[GetScriptableObject("RawResources/ScriptableIns")] public Scriptable mySoSuffix;
```

![GetScriptableObject](https://github.com/TylerTemp/SaintsField/assets/6391063/191c3b4b-a58a-4475-80cd-3dbc809a9511)

#### `GetByXPath` ####

Note: You can change the default behavior of these attributes using `Window/Saints/Create or Edit SaintsField Config`

Please read `Saints XPath-like Syntax` section for more information.

**Parameters**

*   (Optional) `EXP config`: config tweak
*   `string path...`: resource searching paths.

    Using `$` as a start to get a path from a callback/property/field.

*    Allow multiple: Yes. With multiple decorators, all results from each decorator will be used.

Showcase:

```csharp
// get the main camera from scene
[GetByXPath("scene:://[@Tag = MainCamera]")] public Camera mainCamera;

// only allow the user to pick from the target folder, which the `Hero` script returns `isAvaliable` as true
[GetByXPath(EXP.JustPicker, "assets::/Art/Heros/*.prefab[@{GetComponent(Hero).isAvaliable}]")]
public GameObject[] heroPrefabs;

// get all prefabs under `Art/Heros` AND `Art/Monsters`
[GetByXPath("assets::/Art/Heros/*.prefab")]
[GetByXPath("assets::/Art/Monsters/*.prefab")]
public GameObject[] entityPrefabs;

// callback: auto find a resource depending on another resource
public Sprite normalIcon;
[GetByXPath("$" + nameof(EditorGetFallbackXPath))]
public Sprite alternativeIcon;

public string EditorGetFallbackXPath() => normalIcon == null
    ? ""
    : $"assets::/Alternative/{AssetDatabase.GetAssetPath(normalIcon)["Assets/".Length..]}";
```

#### `AddComponent` ####

Automatically add a component to the current target if the target does not have this component. (This will not sign the component added)

Recommended to use it with `GetComponent`!

*   `Type compType = null`

    The component type to add. If null, it'll use the field type.

*   `string groupBy = ""`

    For error message grouping.

*   AllowMultiple: Yes

```csharp
using SaintsField;

[AddComponent, GetComponent] public Dummy dummy;
[AddComponent(typeof(BoxCollider)), GetComponent] public GameObject thisObj;
```

![add_component](https://github.com/TylerTemp/SaintsField/assets/6391063/84002879-875f-42aa-9aa0-cca8961f6b2c)

#### `FindComponent` ####

**Deprecated**: use `GetByXPath` instead.

Automatically find a component under the current target. This is very similar to Unity's [`transform.Find`](https://docs.unity3d.com/ScriptReference/Transform.Find.html), except it accepts many paths, and it's returning value is not limited to `transform`

*   (Optional) `EXP config`: config. See `Saints XPath-like Syntax` section for more information.
*   `string path` a path to search
*   `params string[] paths` more paths to search
*   AllowMultiple: Yes but not necessary

```csharp
using SaintsField;

[FindComponent("sub/dummy")] public Dummy subDummy;
[FindComponent("sub/dummy")] public GameObject subDummyGo;
[FindComponent("sub/noSuch", "sub/dummy")] public Transform subDummyTrans;
```

![find_component](https://github.com/TylerTemp/SaintsField/assets/6391063/6620e643-3f8a-4c33-a136-6cbfc889d2ac)

#### `GetComponentByPath` ####

**Deprecated**: use `GetByXPath` instead.

Automatically sign a component to a field by a given path.

*   (Optional)`EGetComp config`

    Options are:

    *   `EGetComp.ForceResign`: when the target changed (e.g. you delete/create one), automatically resign the new correct component.
    *   `EGetComp.NoResignButton`: do not display a re-sign button when the target mismatches.

*   `string paths...`

    Paths to search.

*   AllowMultiple: Yes. But not necessary.

The `path` is a bit like html's `XPath` but with less function:

| Path            | Meaning                                                        |
|-----------------|----------------------------------------------------------------|
| `/`             | Separator. Using at start means the root of the current scene. |
| `//`            | Separator. Any descendant children                             |
| `.`             | Node. Current node                                             |
| `..`            | Node. Parent node                                              |
| `*`             | All nodes                                                      |
| name            | Node. Any nodes with this name                                 |
| `[last()]`      | Index Filter. Last of results                                  |
| `[index() > 1]` | Index Filter. Node index that is greater than 1                |
|  `[0]`          | Index Filter. First node in the results                        |

For example:

*   `./sth` or `sth`: direct child object of current object named `sth`
*   `.//sth`: any descendant child under current. (descendant::sth)
*   `..//sth`: first go to parent, then find the direct child named `sth`
*   `/sth`: top level node in current scene named `sth`
*   `//sth`: first go to top level, then find the direct child named `sth`
*   `///sth`: first go to top level, then find any node named `sth`
*   `./get/sth[1]`: the child named `get` of current node, then the second node named `sth` in the direct children list of `get`

```csharp
using SaintsField;

// starting from root, search any object with name "Dummy"
[GetComponentByPath("///Dummy")] public GameObject dummy;
// first child of current object
[GetComponentByPath("./*[1]")] public GameObject direct1;
// child of current object which has index greater than 1
[GetComponentByPath("./*[index() > 1]")] public GameObject directPosTg1;
// last child of current object
[GetComponentByPath("./*[last()]")] public GameObject directLast;
// re-sign the target if mis-match
[GetComponentByPath(EGetComp.NoResignButton | EGetComp.ForceResign, "./DirectSub")] public GameObject directSubWatched;
// without "ForceResign", it'll display a reload button if mis-match
// with multiple paths, it'll search from left to right
[GetComponentByPath("/no", "./DirectSub1")] public GameObject directSubMulti;
// if no match, it'll show an error message
[GetComponentByPath("/no", "///sth/else/../what/.//ever[last()]/goes/here")] public GameObject notExists;
```

![get_component_by_path](https://github.com/TylerTemp/SaintsField/assets/6391063/a0fdca83-0c96-4d57-9c9d-989efbac0f07)

### Validate & Restrict ###

#### `FieldType` ####

Ask the inspector to display another type of field rather than the field's original type.

This is useful when you want to have a `GameObject` prefab, but you want this target prefab to have a specific component (e.g. your own `MonoScript`, or a `ParticalSystem`). By using this you force the inspector to sign the required object that has your expected component but still gives you the original typed value to field.

This can also be used when you just want a type reference to a prefab, but Unity does not allow you to pick a prefab because "performance consideration".

Overload:

*   `FieldTypeAttribute(Type compType, EPick editorPick = EPick.Assets | EPick.Scene, bool customPicker = true)`
*   `FieldTypeAttribute(Type compType, bool customPicker)`
*   `FieldTypeAttribute(EPick editorPick = EPick.Assets | EPick.Scene, bool customPicker = true)`

For each argument:

*   `Type compType` the type of the component you want to pick. `null` for using current type
*   `EPick editorPick` where you want to pick the component. Options are:
    *   `EPick.Assets` for assets
    *   `EPick.Scene` for scene objects

    For the default Unity picker: if no `EPick.Scene` is set,  will not show the scene objects. However, omit `Assets` will still show the assets. This limitation is from Unity's API.

    The custom picker does **NOT** have this limitation.
*   `customPicker` show an extra button to use a custom picker. Disable this if you have serious performance issue.

*   AllowMultiple: No

```csharp
using SaintsField;

[SerializeField, FieldType(typeof(SpriteRenderer))]
private GameObject _go;

[SerializeField, FieldType(typeof(FieldTypeExample))]
private ParticleSystem _ps;

// this allows you to pick a perfab with field component on, which Unity will only give an empty picker.
[FieldType(EPick.Assets)] public Dummy dummyPrefab;
```

![field_type](https://github.com/TylerTemp/SaintsField/assets/6391063/7bcc058f-5cb4-4a4f-9d8e-ec08bcb8da2c)

#### `OnValueChanged` ####

Call a function every time the field value is changed

*   `string callback` the callback function name

    It'll try to pass the new value and the index (only if it's in an array/list). You can set the corresponding parameter in your callback if you want to receive them.

*   AllowMultiple: Yes

Special Note: `AnimatorState` will have a different `OnValueChanged` parameter passed in. See `AnimatorState` for more detail.

**Known Issue**:

Unity changed how the `TrackPropertyValue` and `RegisterValueChangeCallback` works. Using on a `SerializeReference`, you can still get the correct callback, but the callback will happen multiple times for one change.

Using `OnValueChanged` on an array/list of `SerializeReference` can cause some problem when you add/remove an element: the `Console` will give error, and the inspector view will display incorrect data. Selecting out then selecting back will fix this issue.
However, you can just switch back to the old way if you do not care about the field change in the reference field. (Because Unity, still, does not fix related issues about property tracking...)

These two issues can not be fixed unless Unity fixes it.

See: [1](https://issuetracker.unity3d.com/issues/visualelements-that-use-trackpropertyvalue-keep-tracking-properties-when-they-are-removed), [2](https://issuetracker.unity3d.com/issues/visualelement-dot-trackpropertyvalue-doesnt-invoke-the-callback-when-the-property-is-under-serializereference-and-serializefield-attributes)

```csharp
using SaintsField;

// no params
[OnValueChanged(nameof(Changed))]
public int value;
private void Changed()
{
    Debug.Log($"changed={value}");
}

// with params to get the new value
[OnValueChanged(nameof(ChangedAnyType))]
public GameObject go;

// it will pass the index too if it's inside an array/list
[OnValueChanged(nameof(ChangedAnyType))]
public SpriteRenderer[] srs;

// it's ok to set it as the super class
private void ChangedAnyType(object anyObj, int index=-1)
{
    Debug.Log($"changed={anyObj}@{index}");
}
```

#### `OnArraySizeChanged` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

`OnValueChanged` can not detect if an array/list is changed in size. `OnArraySizeChanged` attribute will call a callback for that.

Using it together with `OnValueChanged` to get all changing notification for an array/list.

Parameters:

*   `string callback`: the callback function when the size changed.
*   Allow Multiple: No

```csharp
using SaintsField;  // namespace for OnValueChanged
using SaintsField.Playa;  // namespace for OnArraySizeChanged

[Serializable]
public class MyClass  // generic class change is also detectable
{
    public string unique;
}

[OnValueChanged(nameof(ValueChanged))]  // optional
[OnArraySizeChanged(nameof(SizeChanged))]
public MyClass[] myClasses;

public void ValueChanged(MyClass myClass, int index) => Debug.Log($"OnValueChanged: {myClass.unique} at {index}");

// if you do not care about values, just omit the parameters
public void SizeChanged(IReadOnlyList<MyClass> myClassNewValues) => Debug.Log($"OnArraySizeChanged {myClassNewValues.Count}: {string.Join("; ", myClassNewValues.Select(each => each?.unique))}");
```

#### `ReadOnly`/`DisableIf`/`EnableIf` ####

A tool to set field enable/disable status. Supports callbacks (function/field/property) and **enum** types. by using multiple arguments and decorators, you can make logic operation with it.

`ReadOnly` equals `DisableIf`, `EnableIf` is the opposite of `DisableIf`

Arguments:

For callback (functions, fields, properties):

*   (Optional) `EMode editorMode`

    Condition: if it should be in edit mode, play mode for Editor or in some prefab stage. By default, (omitting this parameter) it does not check the mode at all.

    See `Misc` - `EMode` for more information.

*   `object by...`

    callbacks or attributes for the condition. For more information, see `Callback` section

*   AllowMultiple: Yes

For `ReadOnly`/`DisableIf`: The field will be disabled if **ALL** condition is true (`and` operation)

For `EnableIf`: The field will be enabled if **ANY** condition is true (`or` operation)

For multiple attributes: The field will be disabled if **ANY** condition is true (`or` operation)

Logic example:

*   `EnableIf(A)` == `DisableIf(!A)`
*   `EnableIf(A, B)` == `EnableIf(A || B)` == `DisableIf(!(A || B))` == `DisableIf(!A && !B)`
*   `[EnableIf(A), EnableIf(B)]` == `[DisableIf(!A), DisableIf(!B)]` == `DisableIf(!A || !B)`

A simple example:

```csharp
using SaintsField;

[ReadOnly(nameof(ShouldBeDisabled))] public string disableMe;

private bool ShouldBeDisabled  // change the logic here
{
    return true;
}

// This also works on static/const callbacks using `$:`
[DisableIf("$:" + nameof(Util) + "." + nameof(_shouldDisable))] public int disableThis;
// you can put this under another file like `Util.cs`
public static class Util
{
    [ShowInIspector] private static bool _shouldDisable;
}
```

It also supports `enum` types. The syntax is like this:

```csharp
using SaintsField;

[Serializable]
public enum EnumToggle
{
    Off,
    On,
}
public EnumToggle enum1;
[ReadOnly(nameof(enum1), EnumToggle.On)] public string enumReadOnly;
```

A more complex example:

```csharp
using SaintsField;

[Serializable]
public enum EnumToggle
{
    Off,
    On,
}

public EnumToggle enum1;
public EnumToggle enum2;
public bool bool1;
public bool bool2 {
    return true;
}

// example of checking two normal callbacks and two enum callbacks
[EnableIf(nameof(bool1), nameof(bool2), nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On)] public string bool12AndEnum12;
```

A more complex example about logic operation:

```csharp
using SaintsField;

[ReadOnly] public string directlyReadOnly;

[SerializeField] private bool _bool1;
[SerializeField] private bool _bool2;
[SerializeField] private bool _bool3;
[SerializeField] private bool _bool4;

[SerializeField]
[ReadOnly(nameof(_bool1))]
[ReadOnly(nameof(_bool2))]
[RichLabel("readonly=1||2")]
private string _ro1and2;


[SerializeField]
[ReadOnly(nameof(_bool1), nameof(_bool2))]
[RichLabel("readonly=1&&2")]
private string _ro1or2;


[SerializeField]
[ReadOnly(nameof(_bool1), nameof(_bool2))]
[ReadOnly(nameof(_bool3), nameof(_bool4))]
[RichLabel("readonly=(1&&2)||(3&&4)")]
private string _ro1234;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/e267567b-469c-4156-a82c-82f21fc43021)](https://github.com/TylerTemp/SaintsField/assets/6391063/6761a0f2-07c2-4252-9dd0-c1a60091a891)

EMode example:

```csharp
using SaintsField;

public bool boolVal;

[DisableIf(EMode.Edit)] public string disEditMode;
[DisableIf(EMode.Play)] public string disPlayMode;

[DisableIf(EMode.Edit, nameof(boolVal))] public string disEditAndBool;
[DisableIf(EMode.Edit), DisableIf(nameof(boolVal))] public string disEditOrBool;

[EnableIf(EMode.Edit)] public string enEditMode;
[EnableIf(EMode.Play)] public string enPlayMode;

[EnableIf(EMode.Edit, nameof(boolVal))] public string enEditOrBool;
// dis=!editor || dis=!bool => en=editor&&bool
[EnableIf(EMode.Edit), EnableIf(nameof(boolVal))] public string enEditAndBool;
```

It also supports sub-field, and value comparison like `==`, `>`, `<=`. Read more in the "Syntax for Show/Hide/Enable/Disable/Required-If" section.

#### `PlayaEnableIf`/`PlayaDisableIf` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

This is the same as `EnableIf`, `DisableIf`, plus it can be applied to array, `Button`

Different from `EnableIf`/`DisableIf` in the following:
1.  apply on an array will directly enable or disable the array itself, rather than each element.
2.  Callback function can not receive value and index
3.  this method can not detect foldout, which means using it on `Expandable`, the foldout button will also be disabled. For this case, use `DisableIf`/`EnableIf` instead.

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

[PlayaDisableIf] public int[] justDisable;
[PlayaEnableIf] public int[] justEnable;

[PlayaDisableIf(nameof(boolValue))] public int[] disableIf;
[PlayaEnableIf(nameof(boolValue))] public int[] enableIf;

[PlayaDisableIf(EMode.Edit)] public int[] disableEdit;
[PlayaDisableIf(EMode.Play)] public int[] disablePlay;
[PlayaEnableIf(EMode.Edit)] public int[] enableEdit;
[PlayaEnableIf(EMode.Play)] public int[] enablePlay;

[Button, PlayaDisableIf(nameof(boolValue))] private void DisableIfBtn() => Debug.Log("DisableIfBtn");
[Button, PlayaEnableIf(nameof(boolValue))] private void EnableIfBtn() => Debug.Log("EnableIfBtn");
[Button, PlayaDisableIf(EMode.Edit)] private void DisableEditBtn() => Debug.Log("DisableEditBtn");
[Button, PlayaDisableIf(EMode.Play)] private void DisablePlayBtn() => Debug.Log("DisablePlayBtn");
[Button, PlayaEnableIf(EMode.Edit)] private void EnableEditBtn() => Debug.Log("EnableEditBtn");
[Button, PlayaEnableIf(EMode.Play)] private void EnablePlayBtn() => Debug.Log("EnablePlayBtn");
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/b57f3a65-fad3-4de6-975f-14b945c85a30)

It also supports sub-field, and value comparison like `==`, `>`, `<=`. Read more in the "Syntax for Show/Hide/Enable/Disable/Required-If" section.

#### `ShowIf` / `HideIf` ####

Show or hide the field based on a condition. . Supports callbacks (function/field/property) and **enum** types. by using multiple arguments and decorators, you can make logic operation with it.

Arguments:

*   (Optional) `EMode editorMode`

    Condition: if it should be in edit mode, play mode for Editor or in some prefab stage. By default, (omitting this parameter) it does not check the mode at all.

    See `Misc` - `EMode` for more information.

*   `object by...`

    callbacks or attributes for the condition. For more information, see `Callback` section.

*   AllowMultiple: Yes

You can use multiple `ShowIf`, `HideIf`, and even a mix of the two.

For `ShowIf`: The field will be shown if **ALL** condition is true (`and` operation)

For `HideIf`: The field will be hidden if **ANY** condition is true (`or` operation)

For multiple attributes: The field will be shown if **ANY** condition is true (`or` operation)

For example, `[ShowIf(A...), ShowIf(B...)]` will be shown if `ShowIf(A...) || ShowIf(B...)` is true.

`HideIf` is the opposite of `ShowIf`. Please note "the opposite" is like the logic operation, like `!(A && B)` is `!A || !B`, `!(A || B)` is `!A && !B`.

*   `HideIf(A)` == `ShowIf(!A)`
*   `HideIf(A, B)` == `HideIf(A || B)` == `ShowIf(!(A || B))` == `ShowIf(!A && !B)`
*   `[Hideif(A), HideIf(B)]` == `[ShowIf(!A), ShowIf(!B)]` == `ShowIf(!A || !B)`

A simple example:

```csharp
using SaintsField;

[ShowIf(nameof(ShouldShow))]
public int showMe;

public bool ShouldShow()  // change the logic here
{
    return true;
}

// This also works on static/const callbacks using `$:`
[HideIf("$:" + nameof(Util) + "." + nameof(_shouldHide))] public int hideMe;
// you can put this under another file like `Util.cs`
public static class Util
{
    [ShowInIspector] private static bool _shouldHide;
}
```

It also supports `enum` types. The syntax is like this:

```csharp
using SaintsField;

[Serializable]
public enum EnumToggle
{
    Off,
    On,
}
public EnumToggle enum1;
[ShowIf(nameof(enum1), EnumToggle.On)] public string enum1Show;
```

A more complex example:

```csharp
using SaintsField;

[Serializable]
public enum EnumToggle
{
    Off,
    On,
}

public EnumToggle enum1;
public EnumToggle enum2;
public bool bool1;
public bool bool2 {
    return true;
}

// example of checking two normal callbacks and two enum callbacks
[ShowIf(nameof(bool1), nameof(bool2), nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On)] public string bool12AndEnum12;
```

A more complex example about logic operation:

```csharp
using SaintsField;

public bool _bool1;
public bool _bool2;
public bool _bool3;
public bool _bool4;

[ShowIf(nameof(_bool1))]
[ShowIf(nameof(_bool2))]
[RichLabel("<color=red>show=1||2")]
public string _showIf1Or2;


[ShowIf(nameof(_bool1), nameof(_bool2))]
[RichLabel("<color=green>show=1&&2")]
public string _showIf1And2;

[HideIf(nameof(_bool1))]
[HideIf(nameof(_bool2))]
[RichLabel("<color=blue>show=!1||!2")]
public string _hideIf1Or2;


[HideIf(nameof(_bool1), nameof(_bool2))]
[RichLabel("<color=yellow>show=!(1||2)=!1&&!2")]
public string _hideIf1And2;

[ShowIf(nameof(_bool1))]
[HideIf(nameof(_bool2))]
[RichLabel("<color=magenta>show=1||!2")]
public string _showIf1OrNot2;

[ShowIf(nameof(_bool1), nameof(_bool2))]
[ShowIf(nameof(_bool3), nameof(_bool4))]
[RichLabel("<color=orange>show=(1&&2)||(3&&4)")]
public string _showIf1234;

[HideIf(nameof(_bool1), nameof(_bool2))]
[HideIf(nameof(_bool3), nameof(_bool4))]
[RichLabel("<color=pink>show=!(1||2)||!(3||4)=(!1&&!2)||(!3&&!4)")]
public string _hideIf1234;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/1625472e-5769-4c16-81a3-637511437e1d)](https://github.com/TylerTemp/SaintsField/assets/6391063/dc7f8b78-de4c-4b12-a383-005be04c10c0)

Example about EMode:

```csharp
using SaintsField;

public bool boolValue;

[ShowIf(EMode.Edit)] public string showEdit;
[ShowIf(EMode.Play)] public string showPlay;

[ShowIf(EMode.Edit, nameof(boolValue))] public string showEditAndBool;
[ShowIf(EMode.Edit), ShowIf(nameof(boolValue))] public string showEditOrBool;

[HideIf(EMode.Edit)] public string hideEdit;
[HideIf(EMode.Play)] public string hidePlay;

[HideIf(EMode.Edit, nameof(boolValue))] public string hideEditOrBool;
[HideIf(EMode.Edit), HideIf(nameof(boolValue))] public string hideEditAndBool;
```

It also supports sub-field, and value comparison like `==`, `>`, `<=`. Read more in the "Syntax for Show/Hide/Enable/Disable/Required-If" section.


#### `PlayaShowIf`/`PlayaHideIf` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

This is the same as `ShowIf`, `HideIf`, plus it's allowed to be applied to array, `Button`, `ShowInInspector`

Different from `ShowIf`/`HideIf`:
1.  apply on an array will directly show or hide the array itself, rather than each element.
2.  Callback function can not receive value and index

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

public bool boolValue;

[PlayaHideIf] public int[] justHide;
[PlayaShowIf] public int[] justShow;

[PlayaHideIf(nameof(boolValue))] public int[] hideIf;
[PlayaShowIf(nameof(boolValue))] public int[] showIf;

[PlayaHideIf(EMode.Edit)] public int[] hideEdit;
[PlayaHideIf(EMode.Play)] public int[] hidePlay;
[PlayaShowIf(EMode.Edit)] public int[] showEdit;
[PlayaShowIf(EMode.Play)] public int[] showPlay;

[ShowInInspector, PlayaHideIf(nameof(boolValue))] public const float HideIfConst = 3.14f;
[ShowInInspector, PlayaShowIf(nameof(boolValue))] public const float ShowIfConst = 3.14f;
[ShowInInspector, PlayaHideIf(EMode.Edit)] public const float HideEditConst = 3.14f;
[ShowInInspector, PlayaHideIf(EMode.Play)] public const float HidePlayConst = 3.14f;
[ShowInInspector, PlayaShowIf(EMode.Edit)] public const float ShowEditConst = 3.14f;
[ShowInInspector, PlayaShowIf(EMode.Play)] public const float ShowPlayConst = 3.14f;

[ShowInInspector, PlayaHideIf(nameof(boolValue))] public static readonly Color HideIfStatic = Color.green;
[ShowInInspector, PlayaShowIf(nameof(boolValue))] public static readonly Color ShowIfStatic = Color.green;
[ShowInInspector, PlayaHideIf(EMode.Edit)] public static readonly Color HideEditStatic = Color.green;
[ShowInInspector, PlayaHideIf(EMode.Play)] public static readonly Color HidePlayStatic = Color.green;
[ShowInInspector, PlayaShowIf(EMode.Edit)] public static readonly Color ShowEditStatic = Color.green;
[ShowInInspector, PlayaShowIf(EMode.Play)] public static readonly Color ShowPlayStatic = Color.green;

[Button, PlayaHideIf(nameof(boolValue))] private void HideIfBtn() => Debug.Log("HideIfBtn");
[Button, PlayaShowIf(nameof(boolValue))] private void ShowIfBtn() => Debug.Log("ShowIfBtn");
[Button, PlayaHideIf(EMode.Edit)] private void HideEditBtn() => Debug.Log("HideEditBtn");
[Button, PlayaHideIf(EMode.Play)] private void HidePlayBtn() => Debug.Log("HidePlayBtn");
[Button, PlayaShowIf(EMode.Edit)] private void ShowEditBtn() => Debug.Log("ShowEditBtn");
[Button, PlayaShowIf(EMode.Play)] private void ShowPlayBtn() => Debug.Log("ShowPlayBtn");
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/eb07de01-3210-4f4b-be58-b5fadd899f1a)

It also supports sub-field, and value comparison like `==`, `>`, `<=`. Read more in the "Syntax for Show/Hide/Enable/Disable/Required-If" section.

#### `Required` ####

Reminding a given reference type field to be required.

This will check if the field value is a `truly` value, which means:

1.  `ValuedType` like `struct` will always be `truly` because `struct` is not nullable and Unity will fill a default value for it no matter what
2.  It works on reference type and will NOT skip Unity's life-circle null check
3.  You may not want to use it on `int`, `float` (because only `0` is not `truly`) or `bool`, but it's still allowed if you insist

If you have addressable installed, using `Required` on addressable's `AssetReference` will check if the target asset is valid

If you have `RequiredIf`, `Required` will work as a config privider instead. See `RequiredIf` section for more information.

Parameters:

*   `string errorMessage = null` Error message. Default is `{label} is required`
*   `EMessageType messageType = EMessageType.Error` Custom message type.
*   Allow Multiple: No

```csharp
using SaintsField;

[Required("Add this please!")] public Sprite _spriteImage;
// works for the property field
[field: SerializeField, Required] public GameObject Go { get; private set; }
[Required] public UnityEngine.Object _object;
[SerializeField, Required] private float _wontWork;

[Serializable]
public struct MyStruct
{
    public int theInt;
}

[Required]
public MyStruct myStruct;
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/04d29948-bd1c-4448-9148-ef1103f3feab)

```csharp
[Required(messageType: EMessageType.Info)]
public GameObject empty2;
```

![Image](https://github.com/user-attachments/assets/7c099777-11f8-4d4c-8adf-8f03ce217f00)

#### `RequiredIf` ####

Like `Required`, but only required if the condition is a `truly` result.

Parameters:

Arguments:

*   (Optional) `EMode editorMode`

    Condition: if it should be in edit mode, play mode for Editor or in some prefab stage. By default, (omitting this parameter) it does not check the mode at all.

    See `Misc` - `EMode` for more information.

*   `object by...`

    callbacks or attributes for the condition.

*   Allow Multiple: Yes

You can use multiple `RequiredIf`. The field will be required if **ALL** condition is true (`and` operation)

For multiple `RequiredIf`: The field will be required if **ANY** condition is true (`or` operation)

It also supports sub-field, and value comparison like `==`, `>`, `<=`. Read more in the "Syntax for Show/Hide/Enable/Disable/Required-If" section.

You can use `Required` to change the notice message & icon. See the example below

```csharp
using SaintsField;

[Separator("Depende on other field or callback")]
public GameObject go;
[RequiredIf(nameof(go))]  // if a field is a dependence of another field
public GameObject requiredIfGo;

public int intValue;
[RequiredIf(nameof(intValue) + ">=", 0)]
public GameObject requiredIfPositive;  // if meet some condition; callback is also supported.

[Separator("EMode condition")]

[RequiredIf(EMode.InstanceInScene)]
public GameObject sceneObj;  // if it's a prefab in a scene

[Separator("Suggestion")]

// use as a notice
public Transform hand;
[RequiredIf(nameof(hand))]
[Required("It's suggested to set this field if 'hand' is set", EMessageType.Info)]  // this is now a config provider
public GameObject suggestedIfHand;

[Separator("And")]

// You can also chain multiple conditions as "and" operation
public GameObject andCondition;
[RequiredIf(EMode.InstanceInScene, nameof(andCondition))]
public GameObject instanceInSceneAndCondition;  // if it's a prefab in a scene and 'andCondition' is set

[Separator("Or")]

// You can also chain multiple RequiredIf as "or" operation
public GameObject orCondition;
public int orValue;
[RequiredIf(nameof(orCondition))]
[RequiredIf(nameof(orValue) + ">=", 0)]
public GameObject requiredOr;  // if it's a prefab in a scene and 'andCondition' is set
```

[![video](https://github.com/user-attachments/assets/1dbd5e3b-1fcd-4b79-a1e5-d990628794db)](https://github.com/user-attachments/assets/9ffd8fef-60dd-482d-b644-ec97cae76451)

#### `ValidateInput` ####

Validate the input of the field when the value changes.

*   `string callback` is the callback function to validate the data.

    **Parameters**:

    1.  If the function accepts no arguments, then no argument will be passed
    2.  If the function accepts required arguments, the first required argument will receive the field's value. If there is another required argument and the field is inside a list/array, the index will be passed.
    3.  If the function only has optional arguments, it will try to pass the field's value and index if possible, otherwise the default value of the parameter will be passed.

    **Return**:

    1.  If return type is `string`, then `null` or empty string for valid, otherwise, the string will be used as the error message
    2.  If return type is `bool`, then `true` for valid, `false` for invalid with message "\`{label}\` is invalid`"

*   AllowMultiple: Yes

```csharp
using SaintsField;

// string callback
[ValidateInput(nameof(OnValidateInput))]
public int _value;
private string OnValidateInput() => _value < 0 ? $"Should be positive, but gets {_value}" : null;

// property validate
[ValidateInput(nameof(boolValidate))]
public bool boolValidate;

// bool callback
[ValidateInput(nameof(BoolCallbackValidate))]
public string boolCallbackValidate;
private bool BoolCallbackValidate() => boolValidate;

// with callback params
[ValidateInput(nameof(ValidateWithReqParams))]
public int withReqParams;
private string ValidateWithReqParams(int v) => $"ValidateWithReqParams: {v}";

// with optional callback params
[ValidateInput(nameof(ValidateWithOptParams))]
public int withOptionalParams;

private string ValidateWithOptParams(string sth="a", int v=0) => $"ValidateWithOptionalParams[{sth}]: {v}";

// with array index callback
[ValidateInput(nameof(ValidateValArr))]
public int[] valArr;

private string ValidateValArr(int v, int index) => $"ValidateValArr[{index}]: {v}";
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/f554084c-78f3-43ca-a6ab-b3f59ecbf44c)](https://github.com/TylerTemp/SaintsField/assets/6391063/9d52e663-c9f8-430a-814c-011b17b67a86)

#### `MinValue` / `MaxValue` ####

Limit for int/float field

They have the same overrides:

*   `float value`: directly limit to a number value
*   `string valueCallback`: a callback or property for limit

*   AllowMultiple: Yes

```csharp
using SaintsField;

public int upLimit;

[MinValue(0), MaxValue(nameof(upLimit))] public int min0Max;
[MinValue(nameof(upLimit)), MaxValue(10)] public float fMinMax10;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/7714fa76-fc5c-4ebc-9aae-be189cef7743)](https://github.com/TylerTemp/SaintsField/assets/6391063/ea2efa8d-86e6-46ba-bd7d-23e7577f7604)


#### `RequireType` ####

Allow you to specify the required component(s) or **interface**(s) for a field.

If the signed field does not meet the requirement, it'll:

*   show an error message, if `freeSign=false`
*   prevent the change, if `freeSign=true`

`customPicker` will allow you to pick an object which are already meet the requirement(s).

Overload:

*   `RequireTypeAttribute(bool freeSign = false, bool customPicker = true, params Type[] requiredTypes)`
*   `RequireTypeAttribute(bool freeSign, params Type[] requiredTypes)`
*   `RequireTypeAttribute(EPick editorPick, params Type[] requiredTypes)`
*   `RequireTypeAttribute(params Type[] requiredTypes)`

For each argument:

*   `bool freeSign=false`

    If true, it'll allow you to sign any object to this field, and display an error message if it does not meet the requirement(s).

    Otherwise, it will try to prevent the change.

*   `bool customPicker=true`

    Show a custom picker to pick an object. The showing objects are already meet the requirement(s).

*   `EPick editorPick=EPick.Assets | EPick.Scene`

    The picker type for the custom picker. `EPick.Assets` for assets, `EPick.Scene` for scene objects.

*   `params Type[] requiredTypes`

    The required component(s) or interface(s) for this field.

*   AllowMultiple: No

```csharp
using SaintsField;

public interface IMyInterface {}

public class MyInter1: MonoBehaviour, IMyInterface {}
public class MySubInter: MyInter1 {}

public class MyInter2: MonoBehaviour, IMyInterface {}

[RequireType(typeof(IMyInterface))] public SpriteRenderer interSr;
[RequireType(typeof(IMyInterface), typeof(SpriteRenderer))] public GameObject interfaceGo;

[RequireType(true, typeof(IMyInterface))] public SpriteRenderer srNoPickerFreeSign;
[RequireType(true, typeof(IMyInterface))] public GameObject goNoPickerFreeSign;
```

![RequireType](https://github.com/TylerTemp/SaintsField/assets/6391063/fa296163-611b-4e4a-8218-f682e464ee50)

#### `ArraySize` ####

A decorator that limit the size of the array or list.

Note: Because of the limitation of `PropertyDrawer`:

1.  Delete an element will first be deleted, then the array will duplicate the last element.
2.  UI Toolkit: you might see the UI flicked when you remove an element.

Enable `SaintsEditor` if possible, otherwise:

1.  When the field is 0 length, it'll not be filled to target size.
2.  You can always change it to 0 size.

If you have `SaintsEditor` enabled, recommend to use it together with `ListDrawerSettings`, the `+` & `-` will be enabled/disabled accordingly.

Parameters:

*   `int size` the size of the array or list
*   `int min` min value of the size
*   `int max` max value of the size
*   `string groupBy = ""` for error message grouping

Parameters overload:

*   `string callback`: a callback or property for the size.

    If the value is an integer, the size is fixed to this value.

    If the value is a `(int, int)` tuple, a `Vector2`/`Vector2Int`, the size will be limited to the range. If any value in the range is `< 0`, then the side is not limited. For example, `(1, -1)` means the size is at least 1. `(-1, 20)` means the max size is 20.

    If the value is a `Vector3`/`Vector3Int`, then the `x`, `y` value will be used as the limit

    If the min `>= 0` and the max `< min`, the max value will be ignored

*   `string groupBy = ""` for error message grouping

*   Allow Multiple: Yes

For example:

*   `[ArraySize(3)]` will make the array size fixed to 3
*   `[ArraySize(1, 5)]` will make the array size range to 1-5 (both included)
*   `[ArraySize(min: 1)]` will make the array size at least 1 (no max value specific). Useful to require a non-empty array.

```csharp
using SaintsField;

[ArraySize(3)]
public string[] myArr;
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/4a2f3d42-d574-4212-a57a-76328fbf218f)

```csharp
using SaintsField;
using SaintsField.Playa;

[MinValue(1), Range(1, 10)] public int intValue;
[ArraySize(nameof(intValue)), ListDrawerSettings] public string[] dynamic1;

[Space]
public Vector2Int v2Value;
[ArraySize(nameof(v2Value)), ListDrawerSettings] public string[] dynamic2;
```

[![video](https://github.com/user-attachments/assets/fdf756bc-b548-4047-a667-b15887055b2e)](https://github.com/user-attachments/assets/bc3b3387-08b7-45da-9597-1333edb31c95)

#### `PlayaArraySize` ####

**Deprecated**. Use `ArraySize` instead.

### Layout ###

#### `Ordered` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

`SaintsEditor` uses reflection to get each field. However, c# reflection does not give all the orders: `PropertyInfo`, `MethodInfo` and `FieldInfo` does not order with each other.

Thus, if the order is incorrect, you can use `[Ordered]` to specify the order. But also note: `Ordered` ones are always after the ones without an `Ordered`. So if you want to add it, add it to every field.

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

[Ordered] public string myStartField;

[ShowInInspector, Ordered] public const float MyConstFloat = 3.14f;
[ShowInInspector, Ordered] public static readonly Color MyColor = Color.green;

[ShowInInspector, Ordered]
public Color AutoColor
{
    get => Color.green;
    set {}
}

[Button, Ordered]
private void EditorButton()
{
    Debug.Log("EditorButton");
}

[Ordered] public string myOtherFieldUnderneath;
```

![ordered](https://github.com/TylerTemp/SaintsField/assets/6391063/a64ff7f1-55d7-44c5-8f1c-7804734831f4)

#### `Layout` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

A layout decorator to group fields.

*   `string groupBy` the grouping key. Use `/` to separate different groups and create subgroups.
*   `ELayout layout=ELayout.Vertical` the layout of the current group. Note this is a `EnumFlag`, means you can mix with options.
*   `bool keepGrouping=false`: See `LayoutStart` below
*   `float marginTop = -1f` add some space before the layout. `-1` for using default spacing.
*   `float marginBottom = -1f` add some space after the layout. `-1` for using default spacing.

Options are:

*   `Vertical`
*   `Horizontal`
*   `Background` draw a background color for the whole group
*   `Title` show the title
*   `TitleOut` make `title` more visible. Add this will by default add `Title`. On `IMGUI` it will draw a separator between title and the rest of the content.
    On `UI Toolkit` it will draw a background color for the title.
*   `Foldout` allow to fold/unfold this group. If you have no `Tab` on, then this will automatically add `Title`
*   `Collapse` Same as `Foldout` but is collapsed by default.
*   `Tab` make this group a tab page separated rather than grouping it
*   `TitleBox` = `Background | Title | TitleOut`
*   `FoldoutBox` = `Background | Title | TitleOut | Foldout`
*   `CollapseBox` = `Background | Title | TitleOut | Collapse`

**Known Issue**

About `Horizental` style:

1.  On IMGUI, `HorizontalScope` does **NOT** shrink when there are many items, and will go off-view without a scrollbar. Both `Odin` and `Markup-Attributes` have the same issue. However, `Markup-Attribute` uses `labelWidth` to make the situation a bit better, which `SaintsEditor` does not provide (at this point at least).
2.  On UI Toolkit the label will be put into a new line above

![layout_compare_with_other](https://github.com/TylerTemp/SaintsField/assets/6391063/1376b585-c381-46a9-b22d-5a96808dab7f)

**Appearance**

![layout](https://github.com/user-attachments/assets/1e2e6dfa-85a9-4225-ac8f-8beefc26ae52)

**Example**

```csharp
using SaintsField;
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

[Layout("Titled", ELayout.Title | ELayout.TitleOut)]
public string titledItem1, titledItem2;

// title
[Layout("Titled Box", ELayout.Background | ELayout.TitleOut)]
public string titledBoxItem1;
[Layout("Titled Box")]  // you can omit config when you already declared one somewhere (no need to be the first one)
public string titledBoxItem2;

// foldout
[LayoutStart("Collapse", ELayout.CollapseBox)]
public string collapseItem1;
public string collapseItem2;

[LayoutStart("Foldout", ELayout.FoldoutBox)]
public string foldoutItem1;
public string foldoutItem2;

// tabs
[Layout("Tabs", ELayout.Tab | ELayout.Collapse)]
[LayoutStart("./Tab1")]
public string tab1Item1;
public int tab1Item2;

[LayoutStart("../Tab2")]
public string tab2Item1;
public int tab2Item2;

[LayoutStart("../Tab3")]
public string tab3Item1;
public int tab3Item2;

// nested groups
[LayoutStart("Nested", ELayout.Background | ELayout.TitleOut)]
public int nestedOne;

[LayoutStart("./Nested Group 1", ELayout.TitleOut)]
public int nestedTwo;
public int nestedThree;

[LayoutStart("./Nested Group 2", ELayout.TitleOut)]
public int nestedFour;
public string nestedFive;

// Unlabeled Box
[Layout("Unlabeled Box", ELayout.Background)]
public int unlabeledBoxItem1, unlabeledBoxItem2;

// Foldout In A Box
[Layout("Foldout In A Box", ELayout.Foldout | ELayout.Background | ELayout.TitleOut)]
public int foldoutInABoxItem1, foldoutInABoxItem2;

// Complex example. Button and ShowInInspector works too
[Ordered]
[Layout("Root", ELayout.Tab | ELayout.Foldout | ELayout.Background)]
[Layout("Root/V1")]
[SepTitle("Basic", EColor.Pink)]
public string hv1Item1;

[Ordered]
[Layout("Root/V1/buttons", ELayout.Horizontal)]
[Button("Root/V1 Button1")]
public void RootV1Button()
{
    Debug.Log("Root/V1 Button");
}
[Ordered]
[Layout("Root/V1/buttons")]
[Button("Root/V1 Button2")]
public void RootV1Button2()
{
    Debug.Log("Root/V1 Button");
}

[Ordered]
[Layout("Root/V1")]
[ShowInInspector]
public static Color color1 = Color.red;

[Ordered]
[DOTweenPlay("Tween1", "Root/V1")]
public Tween RootV1Tween1()
{
    return DOTween.Sequence();
}

[Ordered]
[DOTweenPlay("Tween2", "Root/V1")]
public Tween RootV1Tween2()
{
    return DOTween.Sequence();
}

[Ordered]
[Layout("Root/V1")]
public string hv1Item2;

// public string below;

[Ordered]
[Layout("Root/V2")]
public string hv2Item1;

[Ordered]
[Layout("Root/V2/H", ELayout.Horizontal), RichLabel(null)]
public string hv2Item2, hv2Item3;

[Ordered]
[Layout("Root/V2")]
public string hv2Item4;

[Ordered]
[Layout("Root/V3", ELayout.Horizontal)]
[ResizableTextArea, RichLabel(null)]
public string hv3Item1, hv3Item2;

[Ordered]
[Layout("Root/Buggy")]
[InfoBox("Sadly, Horizontal is buggy either in UI Toolkit or IMGUI", above: true)]
public string buggy = "See below:";

[Ordered]
[Layout("Root/Buggy/H", ELayout.Horizontal)]
public string buggy1, buggy2, buggy3;

[Ordered]
[Layout("Title+Tab", ELayout.Tab | ELayout.TitleBox)]
[Layout("Title+Tab/g1")]
public string titleTabG11, titleTabG21;

[Ordered]
[Layout("Title+Tab/g2")]
public string titleTabG12, titleTabG22;

[Ordered]
[Layout("All Together", ELayout.Tab | ELayout.Foldout | ELayout.Title | ELayout.TitleOut | ELayout.Background)]
[Layout("All Together/g1")]
public string allTogetherG11, allTogetherG21;

[Ordered]
[Layout("All Together/g2")]
public string allTogetherG12, allTogetherG22;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/0b8bc596-6a5d-4f90-bf52-195051a75fc9)](https://github.com/TylerTemp/SaintsField/assets/6391063/5b494903-9f73-4cee-82f3-5a43dcea7a01)

By combining `Layout` with `Playa*`, you can create some complex layout struct:

```csharp
[LayoutStart("Equipment", ELayout.TitleBox | ELayout.Vertical)]
[LayoutStart("./Head", ELayout.TitleBox)]
public string st;
[LayoutCloseHere]
public MyStruct inOneStruct;

[LayoutStart("./Upper Body", ELayout.TitleBox)]

[PlayaInfoBox("Note：left hand can be empty, but not right hand", EMessageType.Warning)]

[LayoutStart("./Horizontal", ELayout.Horizontal)]

[LayoutStart("./Left Hand", ELayout.TitleBox)]
public string g11;
public string g12;
public MyStruct myStruct;
public string g13;

[LayoutStart("../Right Hand", ELayout.TitleBox)]
public string g21;
[RichLabel("<color=lime><label/>")]
public string g22;
[RichLabel("$" + nameof(g23))]
public string g23;

public bool toggle;
```

![image](https://github.com/user-attachments/assets/d2185e50-845a-47a5-abb4-fae0faac7ba4)

If titled box is too heavy, you can use `PlayaSeparator` instead. See `PlayaSeparator` section for more information

#### `LayoutStart` / `LayoutEnd` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

`LayoutStart` allows you to continuously grouping fields with layout, until a new group appears. `LayoutEnd` will stop the grouping.

`LayoutStart(name)` is the same as `Layout(name, keepGrouping: true)`

For `LayoutStart`:

*   `string groupBy` same as `Layout`
*   `ELayout layout=0` same as `Layout`
*   `float marginTop = -1f` same as `Layout`
*   `float marginBottom = -1f` same as `Layout`

For `LayoutEnd`:

*   `string groupBy=null` same as `Layout`. When `null`, close all existing groups.

It supports `./SubGroup` to create a nested subgroup:

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

[LayoutStart("Root", ELayout.FoldoutBox)]
public string root1;
public string root2;

[LayoutStart("./Sub", ELayout.FoldoutBox)]  // equals "Root/Sub"
public string sub1;
public string sub2;
[LayoutEnd(".")]

[LayoutStart("./Another", ELayout.FoldoutBox)]  // equals "Root/Another"
public string another1;
public string another2;

[LayoutEnd(".")]  // equals "Root"
public string root3;  // this should still belong to "Root"
public string root4;

[LayoutEnd]  // this should close any existing group
public string outOfAll;

[LayoutStart("Tabs", ELayout.Tab | ELayout.Collapse)]
[LayoutStart("./Tab1")]
public string tab1Item1;
public int tab1Item2;
[LayoutEnd(".")]

[LayoutStart("./Tab2")]
public string tab2Item1;
public int tab2Item2;
```

![image](https://github.com/user-attachments/assets/ebd29cbe-cd84-4f76-8834-91d1ae44fd59)

example of using `LayoutStart` with `LayoutEnd`:

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

public string beforeGroup;

[LayoutStart("Group", ELayout.Background | ELayout.TitleOut)]
public string group1;
public string group2;  // starts from this will be automatically grouped into "Group"
public string group3;

[LayoutEnd("Group")]  // this will end the "Group"
public string afterGroup;
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/ce1f52ce-9717-4929-95bf-a6dae580631e)

example of using new group name to stop grouping:

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

public string breakBefore;

[LayoutStart("break", ELayout.Background | ELayout.TitleOut)]
public string breakGroup1;
public string breakGroup2;

// this group will stop the grouping of "break"
[LayoutStart("breakIn", ELayout.Background | ELayout.TitleOut)]
public string breakIn1;
public string breakIn2;

[LayoutStart("break")]  // this will be grouped into "break", and also end the "breakIn" group
public string breakGroup3;
public string breakGroup4;

[LayoutEnd("break")]  // end, it will not be grouped
public string breakAfter;
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/ab45aa2f-0dbb-44e4-be54-e17913e8aba9)

example of using `keepGrouping: false` to stop grouping, but keep the last one in group:

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

public string beforeGroupLast;

[LayoutStart("GroupLast")]
public string groupLast1;
public string groupLast2;
public string groupLast3;
[Layout("GroupLast", ELayout.Background | ELayout.TitleOut)]  // close this group, but be included
public string groupLast4;

public string afterGroupLast;
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/1aaf80f0-3505-42a9-bd33-27e6aac118a5)

#### `LayoutCloseHere` / `LayoutTerminateHere` ####

Include the current field into the coresponding group, then:
*   `LayoutCloseHere` will close the most recent group, like a `LayoutEnd(".")`
*   `LayoutTerminateHere` will close all groups, like a `LayoutEnd`

`LayoutCloseHere` is useful when you're done with your subgroup, but you might add some field later, but at the point you don't have a field to put a `LayoutEnd`

```csharp
[LayoutStart("Tab", ELayout.TitleBox)] public string tab;

[LayoutStart("./1", ELayout.TitleBox)]
public string tab1Sub1;
public string tab1Sub2;
[LayoutCloseHere]
// same as: [Layout(".", keepGrouping: false), LayoutEnd(".")]
public string tab1Sub3;

// some feature day you might add some field below, `LayoutCloseHere` ensures you don't accidently include them into the subgroup
// ... you field added in the feature

[Button]
public void AFunction() {}
[Button]
public void BFunction() {}
```

![image](https://github.com/user-attachments/assets/c4ea66c9-2706-45fe-9fbf-c7a0023677c6)

`LayoutTerminateHere` is useful when you're done with your group, and your script is also done here (so nowhere to put `EndLayout`). Oneday you come back and add some new fields, this attribute can avoid them to be included in the group accidently.

```csharp
[LayoutStart("Tab", ELayout.TitleBox)] public string tab;

[LayoutStart("./1", ELayout.TitleBox)]
public string tab1Sub1;
public string tab1Sub2;
[LayoutTerminateHere]
// same as: [Layout("."), LayoutEnd]
public string tab1Sub3;

[Button]
public void AFunction() {}
[Button]
public void BFunction() {}
```

![image](https://github.com/user-attachments/assets/b5afa6ae-3d44-4499-b0b9-3b5ba96c24a3)

#### `LayoutDisableIf` / `LayoutEnableIf` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

Disable or enable an entire layout group. These attributes will work on the first layout underneath it.

Arguments:

*   (Optional) `EMode editorMode`

    Condition: if it should be in edit mode, play mode for Editor or in some prefab stage. By default, (omitting this parameter) it does not check the mode at all.

    See `Misc` - `EMode` for more information.

*   `object by...`

    callbacks or attributes for the condition.

*   AllowMultiple: Yes

You can use multiple `LayoutDisableIf`, `LayoutEnableIf`, and even a mix of the two.

For `LayoutDisableIf`: The layout group will be disabled if **ALL** condition is true (`and` operation)

For `LayoutEnableIf`: The layout group will be enabled if **ANY** condition is true (`or` operation)

For multiple attributes: The layout group will be disabled if **ANY** condition is true (`or` operation)

It also supports sub-field, and value comparison like `==`, `>`, `<=`. Read more in the "Syntax for Show/Hide/Enable/Disable/Required-If" section.

```csharp
using SaintsField.Playa;

public bool editableMain;

[LayoutEnableIf(nameof(editableMain))]
[LayoutStart("Main", ELayout.FoldoutBox)]
public bool editable1;

[LayoutEnableIf(nameof(editable1))]
[LayoutStart("./1", ELayout.FoldoutBox, marginBottom: 10)]
public int int1;
public string string1;

[LayoutStart("..")]
public bool editable2;

[LayoutEnableIf(nameof(editable2))]
[LayoutStart("./2", ELayout.FoldoutBox)]
public int int2;
public string string2;

[LayoutEnd]
[Space]
public string layoutEnd;
```

[![video](https://github.com/user-attachments/assets/f437ebe4-b4f0-4d3e-be8b-646dbdb74eca)](https://github.com/user-attachments/assets/fac5fce5-6458-4853-893c-23fa50f84872)

#### `LayoutShowIf` / `LayoutHideIf` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

Show or hide an entire layout group. These attributes will work on the first layout underneath it.

Arguments:

*   (Optional) `EMode editorMode`

    Condition: if it should be in edit mode, play mode for Editor or in some prefab stage. By default, (omitting this parameter) it does not check the mode at all.

    See `Misc` - `EMode` for more information.

*   `object by...`

    callbacks or attributes for the condition.

*   Allow Multiple: Yes

You can use multiple `LayoutShowIf`, `LayoutHideIf`, and even a mix of the two.

For `LayoutShowIf`: The layout group will be shown if **ALL** condition is true (`and` operation)

For `LayoutHideIf`: The layout group will be hidden if **ANY** condition is true (`or` operation)

For multiple attributes: The layout group will be shown if **ANY** condition is true (`or` operation)

```csharp
using SaintsField.Playa;

public bool visibleMain;

[LayoutShowIf(nameof(visibleMain))]
[LayoutStart("Main", ELayout.FoldoutBox)]
public bool visible1;

[LayoutShowIf(nameof(visible1))]
[LayoutStart("./1", ELayout.FoldoutBox, marginBottom: 10)]
public int int1;
public string string1;

[LayoutStart("..")]
public bool visible2;

[LayoutShowIf(nameof(visible2))]
[LayoutStart("./2", ELayout.FoldoutBox)]
public int int2;
public string string2;

[LayoutEnd]
[Space]
public string layoutEnd;
```

[![video](https://github.com/user-attachments/assets/f437ebe4-b4f0-4d3e-be8b-646dbdb74eca)](https://github.com/user-attachments/assets/fac5fce5-6458-4853-893c-23fa50f84872)


### Miscellaneous ###

#### `Dropdown` ####

A dropdown selector. Supports reference type, sub-menu, separator, and disabled select item.

If you want a searchable dropdown, see `AdvancedDropdown`.

*   `string funcName=null` callback function. Must return a `DropdownList<T>`.
    When using on an `enum`, you can omit this parameter, and the dropdown will use the enum values as the dropdown items.
*   `bool slashAsSub=true` treat `/` as a sub item.

    Note: In `IMGUI`, this just replace `/` to Unicode [`\u2215` Division Slash ∕](https://www.compart.com/en/unicode/U+2215), and WILL have a little bit of overlap with nearby characters.

*   `EUnique unique=EUnique.None`: When using on a list/array, a duplicated option can be removed if `Enique.Remove`, or disabled if `EUnique.Disable`. No use for non-list/array.
*   AllowMultiple: No

If you're using UI Toolkit, the search box can also search the path too (rather than just the value).

**Example**

```csharp
using SaintsField;

[Dropdown(nameof(GetDropdownItems))] public float _float;

public GameObject _go1;
public GameObject _go2;
[Dropdown(nameof(GetDropdownRefs))] public GameObject _refs;

private DropdownList<float> GetDropdownItems()
{
    return new DropdownList<float>
    {
        { "1", 1.0f },
        { "2", 2.0f },
        { "3/1", 3.1f },
        { "3/2", 3.2f },
    };
}

private DropdownList<GameObject> GetDropdownRefs => new DropdownList<GameObject>
{
    {_go1.name, _go1},
    {_go2.name, _go2},
    {"NULL", null},
};
```

![dropdown](https://github.com/TylerTemp/SaintsField/assets/6391063/aa0da4aa-dfe1-4c41-8d70-e49cc674bd42)

To control the separator and disabled item

```csharp
using SaintsField;

[Dropdown(nameof(GetDropdownItems))]
public Color color;

private DropdownList<Color> GetDropdownItems()
{
    return new DropdownList<Color>
    {
        { "Black", Color.black },
        { "White", Color.white },
        DropdownList<Color>.Separator(),
        { "Basic/Red", Color.red, true },  // the third arg means it's disabled
        { "Basic/Green", Color.green },
        { "Basic/Blue", Color.blue },
        DropdownList<Color>.Separator("Basic/"),
        { "Basic/Magenta", Color.magenta },
        { "Basic/Cyan", Color.cyan },
    };
}
```

And you can always manually add it:

```csharp
DropdownList<Color> dropdownList = new DropdownList<Color>();
dropdownList.Add("Black", Color.black);  // add an item
dropdownList.Add("White", Color.white, true);  // and a disabled item
dropdownList.AddSeparator();  // add a separator
```

![color](https://github.com/TylerTemp/SaintsField/assets/6391063/d7f8c9c1-ba43-4c2d-b53c-f6b0788202e6)

The look in the UI Toolkit with `slashAsSub: false`:

![dropdown_ui_toolkit](https://github.com/TylerTemp/SaintsField/assets/6391063/e6788204-ff04-4096-a37a-26d68e852737)

Finally, using it on an `enum` to select one `enum` without needing to specify the callback function.

If you add `RichLabel` to the `enum`, the item name will be changed to the `RichLabel` content.

```csharp
[Serializable]
public enum MyEnum
{
    [RichLabel("1")]  // RichLabel is optional. Just for you to have more fancy control
    First,
    [RichLabel("2")]
    Second,
    [RichLabel("3")]
    Third,
    [RichLabel("4/0")]
    ForthZero,
    [RichLabel("4/1")]
    ForthOne,
}

[Dropdown] public MyEnum myEnumDropdown;
```

![image](https://github.com/user-attachments/assets/46ddc541-8773-4571-9aeb-f3fe25c5f783)

#### `AdvancedDropdown` ####

A dropdown selector. Supports reference type, sub-menu, separator, search, and disabled select item, plus icon.

**Known Issue**:

1.  IMGUI: Using Unity's [`AdvancedDropdown`](https://docs.unity3d.com/ScriptReference/IMGUI.Controls.AdvancedDropdown.html). Unity's `AdvancedDropdown` allows to click the disabled item and close the popup, thus you can still click the disable item.
    This is a BUG from Unity. I managed to "hack" it around to show again the popup when you click the disabled item, but you will see the flick of the popup.

    This issue is not fixable unless Unity fixes it.

    This bug only exists in IMGUI

2.  UI Toolkit:

    The group indicator uses `ToolbarBreadcrumbs`. Sometimes you can see text get wrapped into lines. This is because Unity's UI Toolkit has some layout issue, that it can not have the same layout even with same elements+style+boundary size.

    This issue is not fixable unless Unity fixes it. This issue might be different on different Unity (UI Toolkit) version.

**Arguments**

*   `string funcName=null` callback function. Must return either a `AdvancedDropdownList<T>` or a `IEnumerable<object>` (list/array etc.).
    When using on an `enum`, you can omit this parameter, and the dropdown will use the enum values as the dropdown items.
    When omitted, it will try to find all the static values from the field type.
*   `EUnique unique=EUnique.None`: When using on a list/array, a duplicated option can be removed if `Enique.Remove`, or disabled if `EUnique.Disable`. No use for non-list/array.
*   AllowMultiple: No

**`AdvancedDropdownList<T>`**

*   `string displayName` item name to display
*   `T value` or `IEnumerable<AdvancedDropdownList<T>> children`: value means it's a value item. Otherwise, it's a group of items, which the values are specified by `children`
*   `bool disabled = false` if item is disabled
*   `string icon = null` the icon for the item.

    Note: setting an icon for a parent group will result a weird issue on its subpage's title and block the items. This is not fixable unless Unity decide to fix it.

*   `bool isSeparator = false` if item is a separator. You should not use this, but `AdvancedDropdownList<T>.Separator()` instead

```csharp
using SaintsField;

[AdvancedDropdown(nameof(AdvDropdown)), BelowRichLabel(nameof(drops), true)] public int drops;

public AdvancedDropdownList<int> AdvDropdown()
{
    return new AdvancedDropdownList<int>("Days")
    {
        // a grouped value
        new AdvancedDropdownList<int>("First Half")
        {
            // with icon
            new AdvancedDropdownList<int>("Monday", 1, icon: "eye.png"),
            // no icon
            new AdvancedDropdownList<int>("Tuesday", 2),
        },
        new AdvancedDropdownList<int>("Second Half")
        {
            new AdvancedDropdownList<int>("Wednesday")
            {
                new AdvancedDropdownList<int>("Morning", 3, icon: "eye.png"),
                new AdvancedDropdownList<int>("Afternoon", 8),
            },
            new AdvancedDropdownList<int>("Thursday", 4, true, icon: "eye.png"),
        },
        // direct value
        new AdvancedDropdownList<int>("Friday", 5, true),
        AdvancedDropdownList<int>.Separator(),
        new AdvancedDropdownList<int>("Saturday", 6, icon: "eye.png"),
        new AdvancedDropdownList<int>("Sunday", 7, icon: "eye.png"),
    };
}
```

**IMGUI**

![advanced_dropdown](https://github.com/TylerTemp/SaintsField/assets/6391063/d22d56b1-39c2-4ec9-bfbb-5e61dfe1b8a2)

**UI Toolkit**

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/ad2f556b-7d98-4f49-a1ad-e2a5a52bf8f0)](https://github.com/TylerTemp/SaintsField/assets/6391063/157838e7-1f63-4b44-9503-bbb0004db7e8)

There is also a parser to automatically separate items as sub items using `/`:

```csharp
using SaintsField;

[AdvancedDropdown(nameof(AdvDropdown))] public int selectIt;

public AdvancedDropdownList<int> AdvDropdown()
{
    return new AdvancedDropdownList<int>("Days")
    {
        {"First Half/Monday", 1, false, "star.png"},  // enabled, with icon
        {"First Half/Tuesday", 2},

        {"Second Half/Wednesday/Morning", 3, false, "star.png"},
        {"Second Half/Wednesday/Afternoon", 4},
        {"Second Half/Thursday", 5, true, "star.png"},  // disabled, with icon
        "",  // root separator
        {"Friday", 6, true},  // disabled
        "",
        {"Weekend/Saturday", 7, false, "star.png"},
        "Weekend/",  // separator under `Weekend` group
        {"Weekend/Sunday", 8, false, "star.png"},
    };
}
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/1bbad2f3-e1aa-4175-a6b1-fd350c58feb3)

You can use this to make a searchable dropdown:

```csharp
using SaintsField;

[AdvancedDropdown(nameof(AdvDropdownNoNest))] public int searchableDropdown;

public AdvancedDropdownList<int> AdvDropdownNoNest()
{
    return new AdvancedDropdownList<int>("Days")
    {
        {"Monday", 1},
        {"Tuesday", 2, true},  // disabled
        {"Wednesday", 3, false, "star.png"},  // enabled with icon
        {"Thursday", 4, true, "star.png"},  // disabled with icon
        {"Friday", 5},
        "",  // separator
        {"Saturday", 6},
        {"Sunday", 7},
    };
}
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/1e0ad6f4-e65d-4953-9f2a-fa9e22e706af)

Example of returning an array/list:

```csharp
// field:
[AdvancedDropdown(nameof(childTrans))] public Transform selected;
[GetComponentInChildren] public Transform[] childTrans;

// or a callback of IEnumerable:
[AdvancedDropdown(nameof(ChildTrans))] public Transform selectedCallback;
private IEnumerable<Transform> ChildTrans() => transform.Cast<Transform>();
```

![image](https://github.com/user-attachments/assets/f6ff44c4-64eb-43f6-b12b-d2121aef095c)

Finally, using it on an `enum` to select one `enum` without needing to specify the callback function.

If you add `RichLabel` to the `enum`, the item name will be changed to the `RichLabel` content.

```csharp
[Serializable]
public enum MyEnum
{
    [RichLabel("1")]  // RichLabel is optional. Just for you to have more fancy control
    First,
    [RichLabel("2")]
    Second,
    [RichLabel("3")]
    Third,
    [RichLabel("4/0")]
    ForthZero,
    [RichLabel("4/1")]
    ForthOne,
}

[AdvancedDropdown] public MyEnum myEnumAdvancedDropdown;
```

![image](https://github.com/user-attachments/assets/ebc2e2f7-3534-4ff7-8710-29a990f5dea4)

Also, using on a type like `Color` to pick a pre-defined static value:

```csharp
[AdvancedDropdown] public Color builtInColor;
[AdvancedDropdown] public Vector2 builtInV2;
[AdvancedDropdown] public Vector3Int builtInV3Int;
```

![image](https://github.com/user-attachments/assets/404d4cd6-b4bf-4521-b633-2dd745ec4de1)

#### `OptionsDropdown` / `PairsDropdown` ####

Like `AdvancedDropdown`, but allows you to quickly set some const expression value

Useful when you don't want the entire enum

```csharp
use SaintsField;

[OptionsDropdown(0.5f, 1f, 1.5f, 2f, 2.5f, 3f)]
public float floatOpt;

[OptionsDropdown(EUnique.Disable, "Left", "Right", "Top", "Bottom", "Center")]
public string[] stringOpt;
```

![](https://github.com/user-attachments/assets/a26d89c6-5be7-4d66-8782-a927585b01dd)

```csharp
use SaintsField;

[PairsDropdown("negative/1", -1, "negative/2", 2, "negative/3", -3, "zero", 0, "positive/1", 1, "positive/2", 2, "positive/3", 3)]
public int intOpt;

public enum Direction
{
    None,
    Left,
    Right,
    Up,
    Down,
    Center,
}

// useful if you don't want the entire enum
[PairsDropdown(EUnique.Disable, "<-", Direction.Left, "->", Direction.Right, "↑", Direction.Up, "↓", Direction.Down)]
public Direction[] direOpt;
```

![](https://github.com/user-attachments/assets/01501513-d00d-4320-94e9-6c76a81a3c2a)

#### `EnumToggleButtons` ####

A toggle buttons group for enum flags (bit mask) or a normal enum. It provides a button to toggle all bits on/off for flags, or a quick selector for normal enum.

This field has compact mode and expanded mode.

Note: Use `DefaultExpand` if you want it to be expanded by default.

(Old Name: `EnumFlags`)

```csharp
using SaintsField;

[Serializable, Flags]
public enum BitMask
{
    None = 0,  // this will be hide as we will have an all/none button
    Mask1 = 1,
    Mask2 = 1 << 1,
    Mask3 = 1 << 2,
}

[EnumToggleButtons] public BitMask myMask;
```

[![video](https://github.com/user-attachments/assets/13f86449-6632-4489-ba3f-31e55f718966)](https://github.com/user-attachments/assets/0a589a01-f00f-4bfd-a845-57b1ddce45fa)

For a normal enum it allows you to do a quick select

```csharp
[Serializable]
public enum EnumNormal  // normal enum, not flags
{
    First,
    Second,
    [RichLabel("<color=lime><label /></color>")]
    Third,
}

[EnumToggleButtons] public EnumNormal myEnumNormal;

[Serializable]
public enum EnumExpand
{
    Value1,
    Value2,
    Value3,
    Value4,
    Value5,
    Value6,
    Value7,
    Value8,
    Value9,
    Value10,
}
// expand it by default
[EnumToggleButtons, DefaultExpand] public EnumExpand enumExpand;
```

![image](https://github.com/user-attachments/assets/0acb17ee-9866-41a3-abb7-5718cdc398f8)

You can use `RichLabel` to change the name of the buttons. Note: only standard Unity RichText tag is supported at this point.

```csharp
[Serializable, Flags]
public enum BitMask
{
    None = 0,
    [RichLabel("M<color=red>1</color>")]
    Mask1 = 1,
    [RichLabel("M<color=green>2</color>")]
    Mask2 = 1 << 1,
    [RichLabel("M<color=blue>3</color>")]
    Mask3 = 1 << 2,
    [RichLabel("M4")]
    Mask4 = 1 << 3,
    Mask5 = 1 << 4,
}

[EnumToggleButtons]
public BitMask myMask;
```

![image](https://github.com/user-attachments/assets/556ff203-aa55-44c9-9cc1-6ca2675b995f)

#### `FlagsDropdown` ####

A searchable dropdown for enum flags (bit mask). Useful when you have a big enum flags type.

```csharp
using SaintsField;

[Serializable, Flags]
public enum F
{
    [RichLabel("[Null]")]  // RichLabel is optional
    Zero,
    [RichLabel("Options/Value1")]
    One = 1,
    [RichLabel("Options/Value2")]
    Two = 1 << 1,
    [RichLabel("Options/Value3")]
    Three = 1 << 2,
    Four = 1 << 3,
}

[FlagsDropdown]
public F flags;
```

![image](https://github.com/user-attachments/assets/3080d503-3bd5-4624-936a-dc5e150e0e43)

#### `ResizableTextArea` ####

This `TextArea` will always grow its height to fit the content. (minimal height is 3 rows).

Note: Unlike NaughtyAttributes, this does not have a text-wrap issue.

*   AllowMultiple: No

```csharp
using SaintsField;

[SerializeField, ResizableTextArea] private string _short;
[SerializeField, ResizableTextArea] private string _long;
[SerializeField, RichLabel(null), ResizableTextArea] private string _noLabel;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/202a742a-965c-4e68-a829-4a8aa4c8fe9e)](https://github.com/TylerTemp/SaintsField/assets/6391063/64ad9c16-19e2-482d-9186-60d42fb34922)

#### `LeftToggle` ####

A toggle button on the left of the bool field. Only works on boolean field.

```csharp
using SaintsField;

[LeftToggle] public bool myToggle;
[LeftToggle, RichLabel("<color=green><label />")] public bool richToggle;
```

![left_toggle](https://github.com/TylerTemp/SaintsField/assets/6391063/bb3de042-bfd8-4fb7-b8d6-7f0db070a761)

#### `ResourcePath` ####

A tool to pick a resource path (a string) with:
1.  required types or interfaces
2.  display a type instead of showing a string
3.  pick a suitable object using a custom picker

Parameters:

*   `EStr eStr = EStr.Resource`: which kind of string value you expected:
    *  `Resource`: a resource path
    *  `AssetDatabase`: an asset path. You should NOT use this unless you know what you are doing.
    *  `Guid`: the GUID of the target object. You should NOT use this unless you know what you are doing.
*   `bool freeSign=false`:

    `true` to allow to sign any object, and gives a message if the signed value does not match.

    `false` to only allow to sign matched object, and trying to prevent the change if it's illegal.

*   `bool customPicker=true`: use a custom object pick that only display objects which meet the requirements
*   `Type compType`: the type of the component. It can be a component, or an object like `GameObject`, `Sprite`. The field will be this type. It can NOT be an interface
*   `params Type[] requiredTypes`: a list of required components or interfaces you want. Only objects with all the types can be signed.
*   AllowMultiple: No

**Known Issue**: IMGUI, manually sign a null object by using Unity's default pick will sign an empty string instead of null. Use custom pick to avoid this inconsistency.

```csharp
using SaintsField;

// resource: display as a MonoScript, requires a BoxCollider
[ResourcePath(typeof(Dummy), typeof(BoxCollider))]
[InfoBox(nameof(myResource), true)]
public string myResource;

// AssetDatabase path
[Space]
[ResourcePath(EStr.AssetDatabase, typeof(Dummy), typeof(BoxCollider))]
[InfoBox(nameof(myAssetPath), true)]
public string myAssetPath;

// GUID
[Space]
[ResourcePath(EStr.Guid, typeof(Dummy), typeof(BoxCollider))]
[InfoBox(nameof(myGuid), true)]
public string myGuid;

// prefab resource
[ResourcePath(typeof(GameObject))]
[InfoBox(nameof(resourceNoRequire), true)]
public string resourceNoRequire;

// requires to have a Dummy script attached, and has interface IMyInterface
[ResourcePath(typeof(Dummy), typeof(IMyInterface))]
[InfoBox(nameof(myInterface), true)]
public string myInterface;
```

![resource_path](https://github.com/TylerTemp/SaintsField/assets/6391063/35d683bf-7d19-4854-bdf6-ee63532fed80)

#### `ResourceFolder` ###

A folder picker to pick a resource folder under any `Resources`. It'll give error if the selected folder is not a resource.

Parameters:

*   `string folder=""` default folder to open. If it's an empty string, it'll first try the current value of the field, then the first `Resources` folder found.
*   `string title="Choose a folder inside resources"` title of the picker
*   `string groupBy = ""` See the `GroupBy` section

```csharp
using SaintsField;

[ResourceFolder] public string resourcesFolder;
[ResourceFolder] public string[] resourcesFolders;
```

[![video](https://github.com/user-attachments/assets/ad5042db-6de3-4f98-8c5d-6387178e3dec)](https://github.com/user-attachments/assets/3c391078-8fcf-4dba-954d-28b6db21b57b)

#### `DefaultExpand` ####

Expand the field by default. Only works on the field that can be expanded, e.g. `struct/class`, `Expandable`, `EnumFlags`, `SaintsRow`.

```csharp
using SaintsField;

[Serializable]
public struct SaintsRowStruct
{
    [LayoutStart("Hi", ELayout.TitleBox)]
    public string s1;
    public string s2;

}

[DefaultExpand]
public SaintsRowStruct defaultStruct;

[DefaultExpand, SaintsRow] public SaintsRowStruct row;

[DefaultExpand, GetScriptableObject, Expandable] public Scriptable so;

[Serializable, Flags]
public enum BitMask
{
    None = 0,  // this will be replaced for all/none button
    [RichLabel("M<color=red>1</color>")]
    Mask1 = 1,
    [RichLabel("M<color=green>2</color>")]
    Mask2 = 1 << 1,
    [RichLabel("M<color=blue>3</color>")]
    Mask3 = 1 << 2,
    [RichLabel("M4")]
    Mask4 = 1 << 3,
    Mask5 = 1 << 4,
}

[DefaultExpand, EnumFlags] public BitMask mask;
```

#### `ArrayDefaultExpand` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

Expand all the elements in the array by default. This works exactly the same as `DefaultExpand`, plus `list`, `array`, `ListDrawerSettings`

```csharp
using SaintsField;
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

[ArrayDefaultExpand]
public string[] arrayDefault;

[ArrayDefaultExpand]
public List<string> listDefault;

[ArrayDefaultExpand, ListDrawerSettings]
public string[] arrayDrawer;

[Serializable]
public struct TableStruct
{
    public string name;
    public int value;
}

[ArrayDefaultExpand, Table] public TableStruct[] table;
```

#### `AssetFolder` ####

A folder picker to pick a folder under `Assets`. It'll give error if the selected folder is outside of `Assets`.

Parameters:

*   `string folder="Assets"` default folder to open.
*   `string title="Choose a folder inside assets"` title of the picker
*   `string groupBy = ""` See the `GroupBy` section

```csharp
using SaintsField;

[AssetFolder] public string assetsFolder;
[AssetFolder] public string[] assetsFolders;
```

[![video](https://github.com/user-attachments/assets/bb11f6b5-5940-4057-9c5a-e3877ab41b54)](https://github.com/user-attachments/assets/322c6dbb-6fb7-4887-90ad-22aaf3905152)

#### `AssetPreview` ####

Show an image preview for prefabs, Sprite, Texture2D, Addressable `AssetReference`, etc. (Internally use `AssetPreview.GetAssetPreview`), or on  type.

Note: Recommended to use `AboveImage`/`BelowImage` for image/sprite/texture2D.

*   `int width=-1`

    preview width, -1 for original image size that returned by Unity. If it's greater than current view width, it'll be scaled down to fit the view. Use `int.MaxValue` to always fit the view width.

*   `int height=-1`

    preview height, -1 for auto resize (with the same aspect) using the width

*   `EAlign align=EAlign.End`

    Align of the preview image. Options are `Start`, `End`, `Center`, `FieldStart`

*   `bool above=false`

    if true, render above the field instead of below

*   `string groupBy=""`

    See the `GroupBy` section

*   AllowMultiple: No

```csharp
using SaintsField;

[AssetPreview(20, 100)] public Texture2D _texture2D;
[AssetPreview(50)] public GameObject _go;
[AssetPreview(above: true)] public Sprite _sprite;
```

![asset_preview](https://github.com/TylerTemp/SaintsField/assets/6391063/ffed3715-f531-43d0-b4c3-98d20d419b3e)

#### `AboveImage`/`BelowImage` ####

Show an image above/below the field.

*   `string image = null`

    An image to display. This can be a property or a callback, which returns a `Sprite`, `Texture2D`, `SpriteRenderer`, `UI.Image`, `UI.RawImage`, `UI.Button`, or Addressable `AssetReference` type.

    If it's null, it'll try to get the image from the field itself.

*   `string maxWidth=-1`

    preview max width, -1 for original image size. If it's greater than current view width, it'll be scaled down to fit the view. . Use `int.MaxValue` to always fit the view width.

*   `int maxHeight=-1`

    preview max height, -1 for auto resize (with the same aspect) using the width

*   `EAlign align=EAlign.Start`

    Align of the preview image. Options are `Start`, `End`, `Center`, `FieldStart`

*   `string groupBy=""`

    See the `GroupBy` section

*   AllowMultiple: No

```csharp
using SaintsField;

[AboveImage(nameof(spriteField))]
// size and group
[BelowImage(nameof(spriteField), maxWidth: 25, groupBy: "Below1")]
[BelowImage(nameof(spriteField), maxHeight: 20, align: EAlign.End, groupBy: "Below1")]
public Sprite spriteField;

// align
[BelowImage(nameof(spriteField), maxWidth: 20, align: EAlign.FieldStart)]
[BelowImage(nameof(spriteField), maxWidth: 20, align: EAlign.Start)]
[BelowImage(nameof(spriteField), maxWidth: 20, align: EAlign.Center)]
[BelowImage(nameof(spriteField), maxWidth: 20, align: EAlign.End)]
public string alignField;
```

![show_image](https://github.com/TylerTemp/SaintsField/assets/6391063/8fb6397f-12a7-4eaf-9e2b-65f563c89f97)

#### `ParticlePlay` ####

A button to play a particle system of the field value, or the one on the field value.

Unity allows play ParticleSystem in the editor, but only if you selected the target GameObject. It can only play one at a time.

This decorator allows you to play multiple ParticleSystem as long as you have the expected fields.

Parameters:

*   `string groupBy = ""` for error grouping.

*   Allow Multiple: No

Note: because of the limitation from Unity, it can NOT detect if a `ParticleSystem` is finished playing

```csharp
[ParticlePlay] public ParticleSystem particle;
// It also works if the field target has a particleSystem component
[ParticlePlay, FieldType(typeof(ParticleSystem), false)] public GameObject particle2;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/18ab2c32-9be9-49f5-9a3a-058fa4c3c7bd)](https://github.com/TylerTemp/SaintsField/assets/6391063/2473df2b-39fc-47bc-829e-eeb65c411131)

#### `ButtonAddOnClick` ####

Add a callback to a button's `onClick` event. Note this at this point does only supports callback with no arguments.

Note: `SaintsEditor` has a more powerful `OnButtonClick`. If you have `SaintsEditor` enabled, it's recommended to use `OnButtonClick` instead.

*   `string funcName` the callback function name
*   `string buttonComp=null` the button component name.

    If null, it'll try to get the button component by this order:

    1.  the field itself
    2.  get the `Button` component from the field itself
    3.  get the `Button` component from the current target

    If it's not null, the search order will be:

    1.  get the field of this name from current target
    2.  call a function of this name from current target

```csharp
using SaintsField;

[GetComponent, ButtonAddOnClick(nameof(OnClick))] public Button button;

private void OnClick()
{
    Debug.Log("Button clicked!");
}
```

![buttonaddonclick](https://github.com/TylerTemp/SaintsField/assets/6391063/9c827d24-677c-437a-ad50-fe953a07d6c2)

#### `OnButtonClick` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

This is a method decorator, which will bind this method to the target button's click event.

Parameters:

*   `string buttonTarget=null` the target button. `null` to get it form the current target.
*   `object value=null` the value passed to the method. Note unity only support `bool`, `int`, `float`, `string` and `UnityEngine.Object`. To pass a `UnityEngine.Object`, use a string name of the target, and set the `isCallback` parameter to `true`
*   `bool isCallback=false`: when `value` is a string, set this to `true` to obtain the actual value from a method/property/field

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

[OnButtonClick]
public void OnButtonClickVoid()
{
    Debug.Log("OnButtonClick Void");
}

[OnButtonClick(value: 2)]
public void OnButtonClickInt(int value)
{
    Debug.Log($"OnButtonClick ${value}");
}

[OnButtonClick(value: true)]
public void OnButtonClickBool(bool value)
{
    Debug.Log($"OnButtonClick ${value}");
}

[OnButtonClick(value: 0.3f)]
public void OnButtonClickFloat(float value)
{
    Debug.Log($"OnButtonClick ${value}");
}

private GameObject ThisGo => this.gameObject;

[OnButtonClick(value: nameof(ThisGo), isCallback: true)]
public void OnButtonClickComp(UnityEngine.Object value)
{
    Debug.Log($"OnButtonClick ${value}");
}
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/25c6b4fc-77f4-4731-a9a3-b84573fce179)

Note:

1.  In UI Toolkit, it will only check once when you select the GameObject. In IMGUI, it'll constantly check as long as you're on this object.
2.  It'll only check the method name. Which means, if you change the value of the callback, it'll not update the callback value.

#### `OnEvent` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

This is a method decorator, which will bind this method to the target `UnityEvent` (allows generic type) invoke event.

Parameters:

*   `string eventTarget` the target `UnityEvent`. If you have dot in it, it will first find the field (or property/function), then find the target event on the found field using the name after the dot(s) recursively.
*   `object value=null` the value passed to the method. Note unity only support `bool`, `int`, `float`, `string` and `UnityEngine.Object`. To pass a `UnityEngine.Object`, use a string name of the target, and set the `isCallback` parameter to `true`
*   `bool isCallback=false`: when `value` is a string, set this to `true` to obtain the actual value from a method/property/field

Note:

1.  In UI Toolkit, it will only check once when you select the GameObject. In IMGUI, it'll constantly check as long as you're on this object.
2.  It'll only check the method name. Which means, if you change the value of the callback, it'll not update the callback value.

Example:

```csharp
public UnityEvent<int, int> intIntEvent;

[OnEvent(nameof(intIntEvent))]
public void OnInt2(int int1, int int2)  // dynamic parameter binding
{
}

[OnEvent(nameof(intIntEvent), value: 1)]
public void OnInt1(int int1)  // static parameter binding
{
}
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/34db0516-6aad-4394-a6bc-e57bd97b6b57)

Example of using dot(s):

```csharp
// CustomEventChild.cs
public class CustomEventChild : MonoBehaviour
{
    [field: SerializeField] private UnityEvent<int> _intEvent;
}

// CustomEventExample.cs
public class CustomEventExample : SaintsMonoBehaviour
{
    public CustomEventChild _child;

    // it will find the `_intEvent` on the `_child` field
    [OnEvent(nameof(_child) + "._intEvent")]
    public void OnChildInt(int int1)
    {
    }
}
```

#### `ColorPalette` ####

A simple color palette tool to select a color from a list of colors.

Use `Window` - `Saints` - `Color Palette` to manage the color palette.

**Parameters**:

*   `string[] names`: the tags of the palette. If null, it'll use all the palette in the project. If it starts with `$`, then a property/callback will be invoked,
    which should returns a string (or a collection of string) for the tags.
*   Allow Multiple: No

```csharp
[ColorPalette] public Color allPalette;
```

[![video](https://github.com/user-attachments/assets/7cec6366-e731-4cd1-9d13-a6b0f0f2fa1c)](https://github.com/user-attachments/assets/e5b93ec2-ab77-47d9-9e3b-15f87fd5cecd)

`Window` - `Saints` - `Color Palette`:

[![video](https://github.com/user-attachments/assets/526bb4e9-990b-4d7a-8bba-6293e880ee78)](https://github.com/user-attachments/assets/30c2613c-c8c5-4a3e-b188-fb03e8b06ee7)

[![video](https://github.com/user-attachments/assets/f58da949-7d2a-4a52-b2d7-237d7747e88a)](https://github.com/user-attachments/assets/cbbe5269-09f3-49c1-ac02-5ea49d256d9d)

#### `Searchable` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

> [!NOTE]
> This is UI Toolkit only

This allows you to search for a field in a `MonoBehavior` (`Component`) or `ScriptableObject`. This is useful is you have a big list of fields.

It will draw a search icon in the header. Once clicked, you can input the field name you want to search.

You can also use `Ctrl` + `F` (`Command` + `F` on macOS) to open the search bar.

Note: this only search the field name. It does not search the nested fields, and it does not check with the `RichLabel`, `PlayaRichLabel`.

```csharp
using SaintsField.Playa;

[Searchable]
public class SearchableMono : SaintsMonoBehaviour
{
    public string myString;
    public int myInt;
    [ShowInInspector] private string MyInspectorString => "Non Ser Prop";
    [ShowInInspector] private string otherInspectorString = "Non Ser Field";
    public string otherString;
    public int otherInt;

    [ListDrawerSettings(searchable: true)]
    public string[] myArray;

    [Serializable]
    public struct MyStruct
    {
        public string MyStructString;
    }

    [Table] public MyStruct[] myTable;
}
```

[![video](https://github.com/user-attachments/assets/b8273285-d24e-441d-9ceb-f277685372f3)](https://github.com/user-attachments/assets/5a6b9aae-481d-4898-9cbe-6634c51cb3e4)

## Handles ##

`Handles` is drawn in the scene view instead of inspector.

When using handles (except `SceneViewPicker` and `DrawLabel`), you can use right click to show/hide some handles.

![image](https://github.com/user-attachments/assets/d86350c6-bd97-43d0-a7ef-1c959cd22364)

### `SceneViewPicker` ###

Allow you to pick a target from a scene view, then sign it into your field.

Once clicked the picking icon, use left mouse to choose a target. Once a popup is displayed, choose the target you want.

If you just want the closest one, just click, then click again (because the closest one is always at the position of your cursor)

Usage:

*   Left Mouse: pick; when popup is displayed, click away to close the popup
*   Middle Mouse: cancel

```csharp
using SaintsField;

[SceneViewPicker] public Collider myCollider;
// works with SaintsInterface
[SceneViewPicker] public SaintsObjInterface<IInterface1> interf;

// a notice will diplay if no target is found
[SceneViewPicker] public NoThisInScene noSuch;
// works for list elements too
[SceneViewPicker] public Object[] anything;
```

[![video](https://github.com/user-attachments/assets/2f51ce8f-145e-4216-bd00-30ca1081ab4d)](https://github.com/user-attachments/assets/b994cc62-ad92-419e-9b6e-5029d662b601)

This feature is heavily inspired by [Scene-View-Picker](https://github.com/RoyTheunissen/Scene-View-Picker)! If you like this feature, please consider go give them a star!

Because Scene-View-Picker does not [provide API for script calling](https://github.com/RoyTheunissen/Scene-View-Picker/issues/4), I have to completely re-write the logic for in SaintsField instead of depended on it.

### `DrawLabel` ###

Draw a text in the view scene where the field object is. The decorated field need to be either a `GameObject`/`Component` or a `Vector3`/`Vector2`.

This is useful if you want to track an object's state (e.g. a character's basic states) in the scene.

Parameters:

*   [Optional] `EColor eColor`: color of the label. Default is white.
*   `string content = null`: the label text to show. Starting with `$` to make it an attribute/callback. `null` means using the field's name.
*   `string space = "this"`: when using on a `Vector3` or `Vector2`, `"this"` means using current object as the space container, null means world space, otherwise use the space from this callback/field value.
*   `string color = null`: use a html color if this starts with `#`, otherwise use a callback/field value as the color.

```csharp
using SaintsField;

[DrawLabel("Test"), GetComponent]
public GameObject thisObj;

[Serializable]
public enum MonsterState
{
    Idle,
    Attacking,
    Dead,
}

public MonsterState monsterState;

[DrawLabel(EColor.Yellow ,"$" + nameof(monsterState))] public GameObject child;
```

![draw-label](https://github.com/user-attachments/assets/4f1ec6e7-fed9-4889-9920-d2d2a9b8c0a9)


### `PositionHandle` ###

Draw and use a position handle in the scene. If The decorated field is a `GameObject`/`Component`, the handle will just it's position. If the field is a `Vector3`/`Vector2`, the handle will write the world/local position to the field.

Parameters:

*   `string space="this"`:the containing space. `"this"` means using the current target, `null` means using the world space, otherwise means using a callback or a field value. Only works for `Vector3`/`Vector2` type.

You can use right click to show/hide handles.

Example of using it with vector types + `DrawLabel`:

```csharp
using SaintsField;

[PositionHandle(space: null), DrawLabel(nameof(worldPos3), space: null)] public Vector3 worldPos3;
[PositionHandle(space: null), DrawLabel(nameof(worldPos2), space: null)] public Vector2 worldPos2;

[PositionHandle, DrawLabel(nameof(localPos3))] public Vector3 localPos3;
[PositionHandle, DrawLabel(nameof(localPos2))] public Vector2 localPos2;
```

[![video](https://github.com/user-attachments/assets/16a85513-ad65-4da7-9fec-a3388af9ff43)](https://github.com/user-attachments/assets/b74cf98d-c83d-4eb3-ba8d-4baeb8f49e1d)

Example of using with objects:

```csharp
using SaintsField;

[PositionHandle, DrawLabel("$" + nameof(LabelName)), GetComponentInChildren(excludeSelf: true)]
public MeshRenderer[] meshChildren;

private string LabelName(MeshRenderer target, int index) => $"{target.name}[{index}]";
```

[![video](https://github.com/user-attachments/assets/e8971069-182e-4ea6-b23a-4dc93fc05457)](https://github.com/user-attachments/assets/358001c8-f433-40e9-8a61-2fc63f9998c6)

### `DrawLine` ###

Draw a line between different objects. The decorated field need to be a `GameObject`/`Component` or a `Vector3`/`Vector2`, or a list/array of them.

You can use right click to show/hide handles.

Parameters:

*   `string start = null`: where does the line start. `null` for the current field.
*   `int startIndex = 0`: when `start` is not `null`, and the start is a list/array, specify the index of the start.
*   `string startSpace = "this"`: the containing space. `"this"` means using the current target, `null` means using the world space, otherwise means using a callback or a field value
*   `string end = null`: where does the line end. `null` for the current field.
*   `int endIndex = 0`: when `end` is not `null`, and the end is a list/array, specify the index of the end.
*   `string endSpace = "this"`: the containing space. `"this"` means using the current target, `null` means using the world space, otherwise means using a callback or a field value
*   `EColor eColor = EColor.White`: color
*   `float alpha = 1f`: the alpha of the color. Not works with `color`.
*   `string color = null`: the color of the line. If it starts with `#`, use html hex color, otherwise use as a callback. This overrides the `eColor`.
*   `float dotted = -1f`: when `>=0`, draw dotted line instead.

And also `DrawLineFrom`, `DrawLineTo` as a shortcut to connect current field with another:

*   `string target = null`: target point of the line from current field
*   `int targetIndex = 0`: if the target is a list/array, specify the index of the target.
*   `string targetSpace = "this"`: the containing space. `"this"` means using the current target, `null` means using the world space, otherwise means using a callback or a field value
*   `string space = "this"`: the containing space. `"this"` means using the current target, `null` means using the world space, otherwise means using a callback or a field value
*   `EColor eColor = EColor.White`: color
*   `float alpha = 1f`: the alpha of the color. Not works with `color`.
*   `string color = null`: the color of the line. If it starts with `#`, use html hex color, otherwise use as a callback. This overrides the `eColor`.
*   `float dotted = -1f`: when `>=0`, draw dotted line instead.

```csharp
using SaintsField;

[SerializeField, GetComponent, DrawLabel("Entrance"),
 // connect this to worldPos[0]
 DrawLineTo(target: nameof(localPos), targetIndex: 0, targetSpace: Space.Self),
] private GameObject entrance;

[
    // connect every element in the list
    DrawLine(color: EColor.Green, endSpace: Space.Self),
    // connect every element to the `centerPoint`
    DrawLineTo(target: nameof(centerPoint), color: EColor.Red, colorAlpha: 0.4f),

    DrawLabel("$" + nameof(PosIndexLabel)),
]
public Vector3[] localPos;

[DrawLabel("Center")] public Vector3 centerPoint;

[DrawLabel("Exit"), GetComponentInChildren(excludeSelf: true),
 // connect worldPos[0] to this
 DrawLineFrom(target: nameof(localPos), targetIndex: -1, targetSpace: Space.Self),
] public Transform exit;

private string PosIndexLabel(Vector3 pos, int index) => $"[{index}]\n{pos}";
```

![image](https://github.com/user-attachments/assets/cdfca17a-1a11-4517-a6ff-8f7e90ec4e8a)

### `SaintsArrow` ###

Note: this feature requires [`SaintsDraw` 4.0.5](https://github.com/TylerTemp/SaintsDraw) installed

Draw an arrow between different objects. The decorated field need to be a `GameObject`/`Component` or a `Vector3`/`Vector2`, or a list/array of them.

Parameters:

*   `string start = null`: where does the arrow start. `null` for the current field.
*   `int startIndex = 0`: when `start` is not `null`, and the start is a list/array, specify the index of the start.
*   `string startSpace = "this"`: the containing space. `"this"` means using the current target, `null` means using the world space, otherwise means using a callback or a field value
*   `string end = null`: where does the arrow end. `null` for the current field.
*   `int endIndex = 0`: when `end` is not `null`, and the end is a list/array, specify the index of the end.
*   `string endSpace = "this"`: the containing space. `"this"` means using the current target, `null` means using the world space, otherwise means using a callback or a field value
*   `EColor eColor = EColor.White`: color
*   `float alpha = 1f`: the alpha of the color. Not works with `color`.
*   `string color = null`: the color of the line. If it starts with `#`, use html hex color, otherwise use as a callback. This overrides the `eColor`.
*   `float dotted = -1f`: when `>=0`, draw dotted line instead.
*   `float headLength = 0.5f`: the length of the arrow head.
*   `float headAngle = 20.0f`: the angle of the arrow head.

Specially
1.  using on an array/list without specifying `start` and `end` will arrow-connect the element from first to last.
2.  `startIndex` & `endIndex` can be negative, which means to count from the end. `-1` means the last element.

A complex showcase:

```csharp
using SaintsField;

[SerializeField, GetComponent, DrawLabel("Entrance"),
 // connect this to worldPos[0]
 SaintsArrow(end: nameof(worldPos), endIndex: 0, endSpace: Space.Self),
] private GameObject entrance;

[
    // connect every element in the list
    SaintsArrow(color: EColor.Green, startSpace: Space.Self, headLength: 0.1f),
    // connect every element to the `centerPoint`
    SaintsArrow(start: nameof(centerPoint), color: EColor.Red, startSpace: Space.Self, endSpace: Space.Self, headLength: 0.1f, colorAlpha: 0.4f),

    PositionHandle,
    DrawLabel("$" + nameof(PosIndexLabel)),
]
public Vector3[] worldPos;

[DrawLabel("Center"), PositionHandle] public Vector3 centerPoint;

[DrawLabel("Exit"), GetComponentInChildren(excludeSelf: true), PositionHandle,
 // connect worldPos[0] to this
 SaintsArrow(start: nameof(worldPos), startIndex: -1, startSpace: Space.Self),
] public Transform exit;

private string PosIndexLabel(Vector3 pos, int index) => $"[{index}]\n{pos}";
```

[![video](https://github.com/user-attachments/assets/39003fcf-bc20-40e8-947c-c14829d9d357)](https://github.com/user-attachments/assets/44982e29-edc6-4c3e-892e-228b134d0bb2)

### `ArrowHandleCap` ###

Like `SaintsArrow` but using Unity's default `ArrowHandleCap` to draw. (No dependency required)

Draw an arrow between different objects. The decorated field need to be a `GameObject`/`Component` or a `Vector3`/`Vector2`, or a list/array of them.

Parameters:

*   `string start = null`: where does the arrow start. `null` for the current field.
*   `int startIndex = 0`: when `start` is not `null`, and the start is a list/array, specify the index of the start.
*   `Space startSpace = Space.World`: if the start is a `Vector3`/`Vector2`, should it be in world space or local space.
*   `string end = null`: where does the arrow end. `null` for the current field.
*   `int endIndex = 0`: when `end` is not `null`, and the end is a list/array, specify the index of the end.
*   `Space endSpace = Space.World`: if the end is a `Vector3`/`Vector2`, should it be in world space or local space.
*   `EColor eColor = EColor.White`: color
*   `float alpha = 1f`: the alpha of the color. Not works with `color`.
*   `string color = null`: the color of the line. If it starts with `#`, use html hex color, otherwise use as a callback. This overrides the `eColor`.
*   `float dotted = -1f`: when `>=0`, draw dotted line instead.

Specially
1.  using on an array/list without specifying `start` and `end` will arrow-connect the element from first to last.
2.  `startIndex` & `endIndex` can be negative, which means to count from the end. `-1` means the last element.

Example:

```csharp
using SaintsField;

[SerializeField, GetComponent, DrawLabel("Entrance"),
 // connect this to worldPos[0]
 ArrowHandleCap(end: nameof(worldPos), endIndex: 0),
] private GameObject entrance;

[
    // connect every element in the list
    ArrowHandleCap(eColor: EColor.Green),
    // connect every element to the `centerPoint`
    ArrowHandleCap(end: nameof(centerPoint), eColor: EColor.Red),

    PositionHandle,
    DrawLabel("$" + nameof(PosIndexLabel)),
]
public Vector3[] worldPos;

[DrawLabel("Center"),
 PositionHandle
] public Vector3 centerPoint;

[DrawLabel("Exit"), GetComponentInChildren(excludeSelf: true),
 PositionHandle,
 // connect worldPos[0] to this
 ArrowHandleCap(start: nameof(worldPos), startIndex: -1),
] public Transform exit;

private string PosIndexLabel(Vector3 pos, int index) => $"[{index}]\n{pos}";
```

![image](https://github.com/user-attachments/assets/e78c94c0-803b-436d-b8ff-ba319aefbe93)

### `DrawWireDisc` ###

Like Unity's [`DrawWireDisc`](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Handles.DrawWireDisc.html)， this attributes allows you to draw a disc in the scene.

The field can be a `GameObject`, a `Component`, a `Vector2`(2D game), a `Vector3` (3D game), or a number.

You can specify which parent you want the circle be, the rotate/offset/facing of the circle, and color of course.

Parameters:

*   `float radius = 1f`: radis of the disk. If the target field is a number, use the field's value instead
*   `string radisCallback = null`: use a callback or a field value as the radius. If the target field is a number, use the field's value instead
*   `string space = "this"`: the containing space of the disc. `this` means using the current target, `null` means using the world space, otherwise means using a callback or a field value
*   `float norX = 0f, float norY = 0f, float norZ = 1f`: `Vector3` direction for the `normal` (facing) of the disc. It's facing forward by default
*   `string norCallback = null`: use a callback or a field value as the normal direction, the value must be a `Vector3`
*   `float posXOffset = 0f, float posYOffset = 0f, float posZOffset = 0f`: `Vector3` position offset for the disc related to the `space`
*   `string posOffsetCallback = null`: use a callback or a field value as the position offset. The value must be a `Vector3`
*   `float rotX = 0f, float rotY = 0f, float rotZ = 0f`: rotation of the disc related to `space`
*   `string rotCallback = null`: use a callback or a field value as the rotation. The value must be a `Quaternion`
*   `EColor eColor = EColor.White`: color
*   `float alpha = 1f`: the alpha of the color. Not works with `color`.
*   `string color = null`: the color of the line. If it starts with `#`, use html hex color, otherwise use as a callback. This overrides the `eColor`.

```csharp
using SaintsField;

// Draw a circle for my character
[DrawWireDisc(radis: 0.2f, EColor.Yellow)] public MyCharacter character2D;

// Draw a circle on the ground for my character (disc facing upward)
// the hight from center to the ground is 0.5f
[DrawWireDisc(norY: 1, norZ: 0, posYOffset: -0.5f)] public MyCharacter character3D;

// Make a struct to let it follow
[Serializable]
public struct PlayerWeapon
{
    [PositionHandle(Space.Self)]
    [DrawWireDisc]
    public Vector3 firePointOffset;

    // your other fields
}
```

[![video](https://github.com/user-attachments/assets/0627d463-5f29-4cba-ac2c-b8a7b1f1106a)](https://github.com/user-attachments/assets/37e48f8a-906b-407c-8d95-6faa5210517c)

A simple example to show debugging a player's alert/idle range:

```csharp
[GetComponent]
[DrawWireDisc(norY: 1, norZ: 0, posYOffset: -1f, color: nameof(curColor), radisCallback: nameof(curRadius))]
[DrawLabel(EColor.Brown, "$" + nameof(curStatus))]
public Transform player;

[Range(1f, 1.5f)] public float initRadius;
[Range(1f, 1.5f)] public float alertRadius;

[AdvancedDropdown] public Color initColor;
[AdvancedDropdown] public Color alertColor;

public Transform enemy;

[InputAxis] public string horizontalAxis;
[InputAxis] public string verticalAxis;

[ShowInInspector]
private Color curColor;

[ShowInInspector] private float curRadius = 0.5f;
[ShowInInspector] private string curStatus = "Idle";

private void Awake()
{
    curColor = initColor;
    curRadius = initRadius;
}

public void Update()
{
    Vector3 playerPos = player.position;
    Vector3 enemyPos = enemy.position;

    float distance = Vector3.Distance(playerPos, enemyPos);

    float nowRadius = distance < alertRadius ? alertRadius : initRadius;
    Color nowColor = distance < alertRadius ? alertColor : initColor;
    curStatus = distance < alertRadius ? "Alert" : "Idle";

    curRadius = Mathf.Lerp(curRadius, nowRadius, Time.deltaTime * 10);
    curColor = Color.Lerp(curColor, nowColor, Time.deltaTime * 10);

    float horizontal = Input.GetAxis(horizontalAxis);
    float vertical = Input.GetAxis(verticalAxis);

    Vector3 move = new Vector3(horizontal, 0, vertical);
    player.Translate(move * Time.deltaTime * 3);
}
```

[![video](https://github.com/user-attachments/assets/ce94f758-7a72-442e-9ced-a972cd0783a9)](https://github.com/user-attachments/assets/2d12f926-bc66-4c41-8dfd-92bdf81ba55d)

### `SphereHandleCap` ###

Draw a sphere in the scene like Unity's [`SphereHandleCap`](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Handles.SphereHandleCap.html).

**Parameters**:

*   `float radius = 1f`: radius of the sphere. If the target field is a number, use the field's value instead
*   `string radiusCallback = null`: use a callback or a field value as the radius
*   `string space = "this"`: the containing space of the sphere. `"this"` means using the current target, `null` means using the world space, otherwise means using a callback or a field value
*   `float posXOffset = 0f, float posYOffset = 0f, float posZOffset = 0f`: `Vector3` position offset for the sphere related to the `space`
*   `string posOffsetCallback = null`: use a callback or a field value as the position offset. The value must be a `Vector3`
*   `EColor eColor = EColor.White`: color
*   `float alpha = 1f`: the alpha of the color. Not works with `color`.
*   `string color = null`: the color of the line. If it starts with `#`, use html hex color, otherwise use as a callback. This overrides the `eColor`.

```csharp
[DrawLine]  // also draw the lines
[SphereHandleCap(color: "#FF000099", radius: 0.1f)]
public Vector3[] localPos;

[SphereHandleCap(radius: 0.1f)]
public GameObject[] objPos;  // use obj's position
```

![image](https://github.com/user-attachments/assets/4f31ad1e-1818-409c-91ae-48f4e766a8fb)

## Component Header ##

Component Header allows you to draw extra stuffs on a component like this:

![image](https://github.com/user-attachments/assets/18385c3d-bc65-4761-a1be-a406a731d963)

This component can work if `SaintsEditor` is enabled, or work stand-alone.

To work as stand-alone, go `window` - `Saints` - `Enable Stand-Alone Header GUI Support`

### `HeaderButton` / `HeaderLeftButton` ###

Draw a button in the component header. Method which returns an `IEnumerator` will start a coroutine

Arguments:

*   `string label = null`: label of the button. `null` means using function name. If starts with `$`, then a dynamic label will be created to use a field/property/callback as label. Dynamic label when returning null or empty string will hide the button. Rich Label supported.
*   `string toolTip = null`: tool tip for the button.

```csharp
using SaintsField;
using SaintsField.ComponentHeader;

[HeaderLeftButton]
public void L1()
{
}

[HeaderLeftButton("<color=brown><icon=star.png/>")]
public void OnClickL2()
{
}

[HeaderButton]
public void R1()
{
    Debug.Log("R1");
}

[HeaderButton("<color=lime><icon=star.png/></color>+1", "Add a star")]
public void StartAdd()
{
    rate = (rate + 1) % 6;
}

[HeaderButton("<color=gray><icon=star.png/></color>-1", "Remove a star")]
public void StartRemove()
{
    rate = (rate - 1 + 6) % 6;
    // Debug.Log("OnClickR2");
}

[Rate(1, 5)] public int rate;
```

[![video](https://github.com/user-attachments/assets/ec0bce22-3359-4aee-b029-f42e68d6ae0f)](https://github.com/user-attachments/assets/d116e764-a537-47b3-a746-29474db8de21)

### `HeaderGhostButton` / `HeaderGhostLeftButton` ###

Draw a button in the component header, without frame and background color.

This is useful for icon buttons.

Method which returns an `IEnumerator` will start a coroutine

Arguments:

*   `string label = null`: label of the button. `null` means using function name. If starts with `$`, then a dynamic label will be created to use a field/property/callback as label. Dynamic label when returning null or empty string will hide the button. Rich Label supported.
*   `string toolTip = null`: tool tip for the button.

```csharp
using SaintsField;
using SaintsField.ComponentHeader;

[HeaderGhostLeftButton("<icon=pencil.png/>")]
public void Edit()
{
}

[HeaderGhostButton("<icon=refresh.png/>", "Play")]
public void Play()
{
}

[HeaderGhostButton("<color=gray><icon=save.png/>", "Pause")]
public void Pause()
{
}

[HeaderGhostButton("<color=gray><icon=trash.png/>", "Resume")]
public void Resume()
{
}
```

![image](https://github.com/user-attachments/assets/fa1d081b-cf71-45be-b78c-92973c0b2a50)

A more complex example of dynamic buttons:

```csharp
using SaintsField;
using SaintsField.ComponentHeader;

private string _editButtonIcon = "<icon=pencil.png/>";
private bool _editing;

[HeaderGhostButton("$" + nameof(_editButtonIcon), "Edit")]
private void StartEdit()
{
    _editing = true;
    _editButtonIcon = "";

    _saveLabel = "<color=brown><icon=save.png/>";
}

[HeaderGhostButton("$" + nameof(_saveLabel), "Save")]
private IEnumerator Click()
{
    _editing = false;
    _saveLabel = "<color=gray><icon=save.png/>";
    foreach (int i in Enumerable.Range(0, 200))
    {
        // Debug.Log($"saving {i}");
        yield return null;
    }
    _saveLabel = "<color=lime><icon=check.png/>";
    foreach (int i in Enumerable.Range(0, 200))
    {
        // Debug.Log($"checked {i}");
        yield return null;
    }
    _saveLabel = "";

    _editButtonIcon = "<icon=pencil.png/>";
}

private string _saveLabel = "";

[EnableIf(nameof(_editing)), OnValueChanged(nameof(OnChanged))] public string nickName;
[EnableIf(nameof(_editing)), OnValueChanged(nameof(OnChanged))] public string password;
[EnableIf(nameof(_editing)), OnValueChanged(nameof(OnChanged))] public int age;

private void OnChanged() => _saveLabel = "<color=lime><icon=save.png/>";
```

[![video](https://github.com/user-attachments/assets/510d5964-fab5-4df8-8681-0b86c833d676)](https://github.com/user-attachments/assets/6720a80c-a8d5-42d8-ba91-503874c68af0)

### `HeaderLabel` / `HeaderLeftLabel` ###

Draw a label in the component header. This can be used on a method, property, field, or a Component class.

```csharp
using SaintsField;
using SaintsField.ComponentHeader;

[HeaderLeftLabel("Fixed Text")]
[HeaderLabel]  // dynamic text
public string label;  // also works if it's a private (non-serialized) type
```

![Image](https://github.com/user-attachments/assets/0eb849f3-0325-4799-b41e-af3b5c15d940)

Can be used on a component class:

```csharp
using SaintsField;
using SaintsField.ComponentHeader;

[HeaderLabel("$" + nameof(value))]
[HeaderLeftLabel("dynamic:")]
public class HeaderLabelClassSaExample : MonoBehaviour
{
    public string value;
}
```

![Image](https://github.com/user-attachments/assets/165cdb74-4d44-48ba-8c9f-8a9f6557b1e9)

### `HeaderDraw` / `HeaderLeftDraw` ###

Allow you to manually draw items on the component headers

Parameters:

*   `string groupBy = null`: group the header items virtically by this name. If `null`, it will not share space with anyone.

Signature:

The method must have this signaure:

```csharp
HeaderUsed FuncName(HeaderArea headerArea)
```

`SaintsField.ComponentHeader.HeaderArea` has the following fields:

```csharp
/// <summary>
/// Rect.y for drwaing
/// </summary>
public readonly float Y;
/// <summary>
/// Rect.height for drawing
/// </summary>
public readonly float Height;
/// <summary>
/// the x value where the title (component name) started
/// </summary>
public readonly float TitleStartX;
/// <summary>
/// the x value where the title (component name) ended
/// </summary>
public readonly float TitleEndX;
/// <summary>
/// the x value where the empty space start. You may want to start draw here
/// </summary>
public readonly float SpaceStartX;
/// <summary>
/// the x value where the empty space ends. When drawing from right, you need to backward drawing starts here
/// </summary>
public readonly float SpaceEndX;

/// <summary>
/// The x drawing position. It's recommend to use this as your start drawing point, as SaintsField will
/// change this value accordingly everytime an item is drawn.
/// </summary>
public readonly float GroupStartX;
/// <summary>
/// When using `GroupBy`, you can see the vertical rect which already used by others in this group
/// </summary>
public readonly IReadOnlyList<Rect> GroupUsedRect;

public float TitleWidth => TitleEndX - TitleStartX;
public float SpaceWidth => SpaceEndX - SpaceStartX;

/// <summary>
/// A quick way to make a rect
/// </summary>
/// <param name="x">where to start</param>
/// <param name="width">width of the rect</param>
/// <returns>rect space you want to draw</returns>
public Rect MakeXWidthRect(float x, float width) => new Rect(x, Y, width, Height);
```

After you draw your item, use `return new HeaderUsed(useRect);` to tell the space you've used.

A simple example of progress bar

```csharp
using SaintsField;
using SaintsField.ComponentHeader;

#if UNITY_EDITOR
[HeaderDraw]
private HeaderUsed HeaderDrawRight1G1(HeaderArea headerArea)
{
    // this is drawing from right to left, so we need to backward the rect space
    Rect useRect = new Rect(headerArea.MakeXWidthRect(headerArea.GroupStartX - 100, 100))
    {
        y = headerArea.Y + 2,
        height = headerArea.Height - 4,
    };
    Rect progressRect = new Rect(useRect)
    {
        width = range1 * useRect.width,
    };

    EditorGUI.DrawRect(useRect, Color.gray);
    EditorGUI.DrawRect(progressRect, Color.red);

    return new HeaderUsed(useRect);
}
#endif

[Range(0f, 1f)] public float range1;
```

[![video](https://github.com/user-attachments/assets/f2a08b20-8dde-4fa0-aa5f-c7c402f62c6c)](https://github.com/user-attachments/assets/8f437f21-a4e8-4d8a-9116-090eaef5ac60)

A more complex example:

```csharp
using SaintsField;
using SaintsField.ComponentHeader;

#if UNITY_EDITOR
private bool _started;

[HeaderGhostButton("<icon=play.png/>")]
private IEnumerator BeforeBotton()
{
    _started = true;
    while (_started)
    {
        range1 = (range1 + 0.01f) % 1;
        range2 = (range2 + 0.03f) % 1;
        range3 = (range3 + 0.02f) % 1;
        yield return null;
    }
}

[HeaderDraw("group1")]
private HeaderUsed HeaderDrawRight1G1(HeaderArea headerArea)
{
    Rect useRect = new Rect(headerArea.MakeXWidthRect(headerArea.GroupStartX - 40, 40))
    {
        height = headerArea.Height / 3,
    };
    Rect progressRect = new Rect(useRect)
    {
        width = range1 * useRect.width,
    };

    EditorGUI.DrawRect(useRect, Color.gray);
    EditorGUI.DrawRect(progressRect, Color.red);

    return new HeaderUsed(useRect);
}

[HeaderDraw("group1")]
private HeaderUsed HeaderDrawRight1G2(HeaderArea headerArea)
{
    Rect useRect = new Rect(headerArea.MakeXWidthRect(headerArea.GroupStartX - 40, 40))
    {
        y = headerArea.Y + headerArea.Height / 3,
        height = headerArea.Height / 3,
    };
    Rect progressRect = new Rect(useRect)
    {
        width = range2 * useRect.width,
    };

    EditorGUI.DrawRect(useRect, Color.gray);
    EditorGUI.DrawRect(progressRect, Color.yellow);

    return new HeaderUsed(useRect);
}

[HeaderDraw("group1")]
private HeaderUsed HeaderDrawRight1G3(HeaderArea headerArea)
{
    Rect useRect = new Rect(headerArea.MakeXWidthRect(headerArea.GroupStartX - 40, 40))
    {
        y = headerArea.Y + headerArea.Height / 3 * 2,
        height = headerArea.Height / 3,
    };
    Rect progressRect = new Rect(useRect)
    {
        width = range3 * useRect.width,
    };

    EditorGUI.DrawRect(useRect, Color.gray);
    EditorGUI.DrawRect(progressRect, Color.cyan);

    return new HeaderUsed(useRect);
}

[HeaderGhostButton("<icon=pause.png/>")]
private void AfterBotton()
{
    _started = false;
}
#endif

[Range(0f, 1f)] public float range1;
[Range(0f, 1f)] public float range2;
[Range(0f, 1f)] public float range3;
```

[![video](https://github.com/user-attachments/assets/2eac324f-aadb-47dd-97a2-ff0c563bb906)](https://github.com/user-attachments/assets/23fdafd2-27a8-4412-b56e-43d09989609d)

## Data Types ##

### `SaintsArray`/`SaintsList` ###

Unity does not allow to serialize two-dimensional array or list. `SaintsArray` and `SaintsList` are there to help.

```csharp
using SaintsField;

// two dimensional array
public SaintsArray<GameObject>[] gameObjects2;
public SaintsArray<SaintsArray<GameObject>> gameObjects2Nest;
// four dimensional array, if you like.
// it can be used with array, but ensure the `[]` is always at the end.
public SaintsArray<SaintsArray<SaintsArray<GameObject>>>[] gameObjects4;
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/5c6a5d8d-ec56-45d2-9860-1ecdd6badd3f)

`SaintsArray` implements `IReadOnlyList`, `SaintsList` implements `IList`:

```csharp
using SaintsField;

// SaintsArray
GameObject firstGameObject = saintsArrayGo[0];
Debug.Log(saintsArrayGo.value); // the actual array value

// SaintsList
saintsListGo.Add(new GameObject());
saintsListGo.RemoveAt(0);
Debug.Log(saintsListGo.value);  // the actual list value
```

These two can be easily converted to array/list:

```csharp
using SaintsField;

// SaintsArray to Array
GameObject[] arrayGo = saintsArrayGo;
// Array to SaintsArray
SaintsArray<GameObject> expSaintsArrayGo = (SaintsArray<GameObject>)arrayGo;

// SaintsList to List
List<GameObject> ListGo = saintsListGo;
// List to SaintsList
SaintsList<GameObject> expSaintsListGo = (SaintsList<GameObject>)ListGo;
```

Because it's actually a struct, you can also implement your own Array/List, using `[SaintsArray]`. Here is an example of customize your own struct:

```csharp
using SaintsField;

// example: using IWrapProp so you don't need to specify the type name everytime
[Serializable]
public class MyList : IWrapProp
{
    [SerializeField] public List<string> myStrings;

#if UNITY_EDITOR
    private static readonly string EditorPropertyName = nameof(myStrings);
#endif
}

[SaintsArray]
public MyList[] myLis;


// example: any Serializable which hold a serialized array/list is fine
[Serializable]
public struct MyArr
{
    [RichLabel(nameof(MyInnerRichLabel), true)]
    public int[] myArray;

    private string MyInnerRichLabel(object _, int index) => $"<color=pink> Inner [{(char)('A' + index)}]";
}

[RichLabel(nameof(MyOuterLabel), true), SaintsArray("myArray")]
public MyArr[] myArr;

private string MyOuterLabel(object _, int index) => $"<color=Lime> Outer {index}";
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/ff4bcdef-f3e6-4ece-8204-d4ba8798e164)

alternatively, you can make a custom drawer for your data type to avoid adding `[SaintsArray]` to every field:

```csharp
// Put it under an Editor folder, or with UNITY_EDITOR
#if UNITY_EDITOR
using SaintsField.Editor.Drawers.TypeDrawers;

[CustomPropertyDrawer(typeof(MyList))]
public class MyArrayDrawer: SaintsArrayDrawer {}
#endif
```

### `SaintsDictionary<,>` ###

A simple dictionary serialization tool. It allows:

1.  Allow any type of kay/value type as long as `Dictionary<,>` allows
2.  Give a warning for duplicated keys
3.  Allow search for keys & values
4.  Allow paging for large dictionary
5.  Allow inherence to add some custom attributes, especually with auto getters to gain the auto-fulfill ability.

This is very like [Serialized Dictionary](https://assetstore.unity.com/packages/tools/utilities/serialized-dictionary-243052) with differences: `SaintsDictionaryBase` allows you to customize the dictionary behavior, for example, together with `GetComponentInChildren`, you can allow auto-fulfill.

In short, if you don't need the custom attributes ability, it's suggested to use [Serialized Dictionary](https://assetstore.unity.com/packages/tools/utilities/serialized-dictionary-243052) instead.

Basic usage:

```csharp
public SaintsDictionary<string, GameObject> genDict;
```

![image](https://github.com/user-attachments/assets/a0d087fd-98ba-4419-82ef-1acf0d0a5243)

Add `SaintsDictionary` attribute to control:

1.  Change keys' label & values' label
2.  Disable the search, if you want
3.  Enable the paging

**Parameters**:

*   `string keyLabel = "Keys"`
*   `string valueLabel = "Values"`
*   `bool searchable = true`: false to disable the search ability
*   `int numberOfItemsPerPage = 0`: items per page. 0 for no paging

```csharp
suing SaintsField;

[SaintsDictionary("Slot", "Enemy", numberOfItemsPerPage: 5)]
public SaintsDictionary<int, GameObject> slotToEnemyPrefab;
```

[![video](https://github.com/user-attachments/assets/c05e0e54-dc74-4a58-b83f-9005a16fdc8d)](https://github.com/user-attachments/assets/886203a6-ed24-4ef2-a254-00f7e0ff14e2)

Using on a general struct/class is supported:

```csharp
suing SaintsField;

[Serializable]
public struct MyStruct
{
    public string myStringField;
    public int myIntField;
}


public SaintsDictionary<int, MyStruct> basicType;
```

![image](https://github.com/user-attachments/assets/c2dad54d-bfa6-4c52-acee-e2aa74898d71)

You can also inherit `SaintsDictionaryBase<TKey, TValue>` to create your own custom dictionary.

> [!WARNING]
> Custom Dictionary is still under some test and need some API changes. Please avoid inherit a custom dictionary, but use `SaintsDictionary` directly.
> If you still need it, please fork this project, use the forked one, and carefully exam the project when you upgrade, as it might break your inherence.

See [DictInterface](https://github.com/TylerTemp/SaintsField/blob/master/Samples~/Scripts/IssueAndTesting/Issue/Issue241DictInterface.cs) as an example of making an `SerializedReference` dictionary.

![Image](https://github.com/user-attachments/assets/7b252440-c11d-4bd0-b206-4808cd4c3c01)

See [SaintsDictFiller](https://github.com/TylerTemp/SaintsField/blob/master/Samples~/Scripts/SaintsDictExamples/SaintsDictFillerExample.cs) as an example of making dictionary with auto getters.

[![video](https://github.com/user-attachments/assets/ce2efb49-2723-4e43-a3a7-9969f229f591)](https://github.com/user-attachments/assets/38dcb22c-d30f-40d4-bd6b-420aa1b41588)

### `SaintsInterface<,>`/`SaintsObjInterface<>` ###

`SaintsInterface` is a simple tool to serialize a `UnityEngine.Object` (usually your script component) with a required interface.

You can access the interface with the `.I` field, and actual object with `.V` field.

It provides a drawer to let you only select the object that implements the interface.

For `SaintsInterface<TObject, TInterface>`:

*   `TObject` a serializable type. Use `Component` (for your `MonoBehavior`), `ScriptableObject` or even `UnityEngine.Object` (for any serializable object) if you only want any limitation. Don't use `GameObject`.
*   `TInterface` the interface type.
*   `.I`: the interface value, which is the instance of `TInterface`
*   `.V`: the actual object value, which is the instance of `TObject`

`SaintsObjInterface<>` is a shortcut for `SaintsInterface<UnityEngine.Object, TInterface>`.

```csharp
using SaintsField;

public SaintsObjInterface<IInterface1> myInter1;

// for old unity
[Serializable]
public class Interface1 : SaintsInterface<Component, IInterface1>
{
}

public Interface1 myInherentInterface1;

private void Awake()
{
    Debug.Log(myInter1.I);  // the actual interface
    Debug.Log(myInter1.V);  // the actual serialized object
}
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/e7ea662d-34d2-4d4b-88b9-0c4bbbbdee33)

This component is inspired by [Serialize Interfaces!](https://assetstore.unity.com/packages/tools/utilities/serialize-interfaces-187505), you may go there and support him!

Compared to [Serialize Interfaces!](https://assetstore.unity.com/packages/tools/utilities/serialize-interfaces-187505), this version has several differences:

*   It supports UI Toolkits too.
*   Many SaintsField attributes can work together with this one, especially these auto getters, validators etc.

### `SaintsHashSet<>` / `ReferenceHashSet<>` ###

> [!WARNING]
> UI Toolkit only. (IMGUI will have only default drawer)

A serializable `HashSet<>` for normal type and `SerializedReference` type. Duplicated element will have a warning color.

You can use `SaintsHashSet` attribute to control paging & searching

Parameters:

*   `bool searchable = true`: `false` to disable the search function
*   `int numberOfItemsPerPage = 0`: how many items per page. `0` for no paging

```csharp
public SaintsHashSet<string> stringHashSet;  // default

[SaintsHashSet(numberOfItemsPerPage: 5)]  // paging control
public SaintsHashSet<int> integerHashSet = new SaintsHashSet<int>
{
    1, 2, 3, 4, 5, 6, 7, 8, 9, 0,
};

public interface IReference
{
    string Name { get; }
}

// ... implement of IReference omited here

public ReferenceHashSet<IReference> refHashSet;
```

[![video](https://github.com/user-attachments/assets/0ff1ce5a-6432-4aba-bfda-d71f5f56a54f)](https://github.com/user-attachments/assets/8e01cb94-b8bb-49fb-ac58-384ec3c9c2a4)

### `TypeReference` ###

Serialize a `System.Type`

> [!WARNING]
> This function saves the target's domain and type name. Which means if you change the name, the data will be lost.
> Carefully think about it before using it.

By default, it searchs the current assembly and referenced assembly and shows all visible (public) types.

You can add an extra `[TypeReference]` to control the behavior.

**Parameters**:

*   `EType eType = EType.Current`: Options. See below
*   `Type[] superTypes = null`: A list of type/interface, the option list types are inhirent from these types. The type in the list is also included in list.
*   `string[] onlyAssemblies = null`: only use these assembly names
*   `string[] extraAssemblies = null`: extrally add these assemblies to the result list.

EType:

```csharp
[Flags]
public enum EType
{
    /// <summary>
    /// Current assembly
    /// </summary>
    CurrentOnly = 1,
    /// <summary>
    ///  Current referenced assemblies.
    /// </summary>
    CurrentReferenced = 1 << 1,

    /// <summary>
    /// Current & referenced assemblies.
    /// </summary>
    Current = CurrentOnly | CurrentReferenced,

    /// <summary>
    /// Include "mscorlib" assembly.
    /// </summary>
    MsCorLib = 1 << 2,
    /// <summary>
    /// Include "System" assembly.
    /// </summary>
    System = 1 << 3,
    /// <summary>
    /// Include "System.Core" assembly.
    /// </summary>
    SystemCore = 1 << 4,
    /// <summary>
    /// Anything except "mscorlib", "System", "System.Core" assemblies.
    /// </summary>
    NonEssential = 1 << 5,
    /// <summary>
    /// All assemblies.
    /// </summary>
    AllAssembly = MsCorLib | System | SystemCore | NonEssential,

    /// <summary>
    /// Allow non-public types.
    /// </summary>
    AllowInternal = 1 << 6,

    /// <summary>
    /// Group the list by the assmbly short name.
    /// </summary>
    GroupAssmbly = 1 << 7,
    /// <summary>
    /// Group the list by the type namespace.
    /// </summary>
    GroupNameSpace = 1 << 8,
}
```

Please note: if you have many type options, the big dropdown list might be SLOW.

Example:

```csharp
using SaintsField;

// default using current & referenced assembly
public TypeReference typeReference;

// current assembly, and group it
[TypeReference(EType.CurrentOnly | EType.GroupAssmbly | EType.GroupNameSpace)]
[BelowButton(nameof(TestCreate))]
public TypeReference typeReference2;

private void TestCreate(TypeReference tr)
{
    // you can also use `tr.Type`
    object t = Activator.CreateInstance(tr);
    Debug.Log(t);
}

// all assembly with non-public types, and group it
[TypeReference(EType.AllAssembly | EType.AllowInternal | EType.GroupAssmbly)]
public TypeReference typeReference3;

public interface IMyTypeRef {}
private struct MyTypeStruct: IMyTypeRef{}
private class MyTypeClass : IMyTypeRef{}

// Only types that implement IMyTypeRef
[TypeReference(EType.AllAssembly | EType.AllowInternal, superTypes: new[]{typeof(IMyTypeRef)})]
public TypeReference typeReferenceOf;
```
[![](https://github.com/user-attachments/assets/a11c8c68-019b-4b13-abde-f8c21ed2fd15)](https://github.com/user-attachments/assets/edac2a97-b9e1-4e8e-aa7f-567f25a50528)

This feature is heavy inspired by [ClassTypeReference-for-Unity](https://github.com/SolidAlloy/ClassTypeReference-for-Unity), please go give them a star!

### `SaintsEvent` ###

`SaintsEvent` is an alternative to Unity's `UnityEvent`. It's inspired by [UltEvents](https://assetstore.unity.com/packages/tools/gui/ultevents-111307) & [ExtEvents](https://github.com/SolidAlloy/ExtEvents)

You need to install [Unity Serialization](https://docs.unity3d.com/Packages/com.unity.serialization@3.1/manual/index.html) to use this type.

Features:

*   Support any serializable parameter type (Unity Object or Non Unity Object)
*   Support up to 4 serialized parameters (UnityEvent: 0-1)
*   Support static method (UnityEvent: Only Unity Object's instance method)
*   Support non-public methods (UnityEvent: Only public methods)

Here are some features that is supported by other plugins:

*   IMGUI is not supported yet, while `UltEvents` & `ExtEvents` does
*   Chained call (use one callback's result as another one's input) is not supported and will not be added, while `UltEvents` does
*   Renamed type is partly supported. If a renamed type is a `MonoBehavior`, then rename works as expected. However, `ExtEvent` will try to find a general type's rename
*   Implicit conversions for arguments is not supported, while `ExtEvents` does
*   Performance optimization is limited to first-time cache, while `ExtEvents` using code generator to make the runtime much more fast. So in general, speed comparison is (fast to slow) `UnityEvent` - `ExtEvent` - `SaintsEvent` - `UltEvent`

I'm not sure if I want to back-port the IMGUI support yet. If you really love this feature, please open an [issue](https://github.com/TylerTemp/SaintsField/issues) or [discussions](https://github.com/TylerTemp/SaintsField/discussions)

> [!WARNING]
> This component is quite heavy and might not be stable for using (compare to others), and I understand a callback can be very wildly used in project.
> To avoid breaking changes, you may consider using it after some iteration of this component so it'll be more stable to use.

**Basics**:

```csharp
using SaintsField.Events;

public SaintsEvent sEvent;
```

![image](https://github.com/user-attachments/assets/7a5b87b2-a999-494f-9795-d38bbed6fca1)


Here, `+s` to add a static callback, `+i` for an instance's callback, `-` to remove a callback.

`R` is a switch to change to `Off`, `Editor & Runtime` or `Runtime Only`, which is the same as `UnityEvent`.

![image](https://github.com/user-attachments/assets/4104704b-a323-4e1a-9a2d-7b0df2099576)

`S` is a switch to change mode between "static callback" and "instance callback".

Then, the object picker is to decide which type you want to limit:

*   For static callback, it'll use the object's type you pick here. use `None` if your target is not any Unity Object (e.g. just a helper class)
*   For instance callback, it'll use the object as the instance

The next dropdown is depending on the mode:

*   For static mode with no target, you need to select a type

    ![](https://github.com/user-attachments/assets/885fee95-b7ef-476d-9aa1-1ad8a86eeab4)
*   For static mode with target, or instance mode, you need to select a component on that target

    ![](https://github.com/user-attachments/assets/4da3a5c2-76e4-4ad9-89f6-a42fbc258705)

The dropdown below is where you pick your actual callback:

![](https://github.com/user-attachments/assets/815ba613-dcd1-45de-9273-daa5061bf392)

Finally, if your function has parameters, you need to check how each parameter is processed. Lets using another example:

```csharp
using SaintsField.Events;

public class MyClass
{
    public int N;
    public override string ToString()
    {
        return $"<MyClass N={N}/>";
    }
}

public SaintsEvent<MyClass, int, string> withName;

public void Callback(MyClass d, string s, int i = -1)
{
    Debug.Log($"Callback: i={i}, s={s}, d={d}");
}
```

![](https://github.com/user-attachments/assets/458e891a-5381-49d4-b2f4-ec1f40a63c71)

In the picture
*   `S` is serialized, which allows you to pick a subclass (if any) and fill public fields.
*   `D` is dynamic, which allows you to bind its value to the event's value. In this example, it's binded to `T2 (string)` in `SaintsEvent<MyClass, int, string>`
*   `X` is default, which uses function parameter's default value. In this example, `int i = -1`, so use `-1`

> [!WARNING]
> ATM it will not check if the parameter type can accept the corresponding value type. Please check it by yourself carefully.

**Runtime**

In runtime, you can use `SaintsEvent.Invoke()`, `SaintsEvent.AddListener(callback)` and `SaintsEvent.RemoveListener(callback)`, `SaintsEvent.RemoveAllListeners()` just like `UnityEvent`.

**Config**

For a better naming, use `SaintsEventArgs` to rename the event generic parameters.

```csharp
using SaintsField.Events;

[SaintsEventArgs("Custom", "Number", "Text")]
public SaintsEvent<MyClass, int, string> withName;
```

![](https://github.com/user-attachments/assets/7826c5a6-8173-4dc0-a634-1707557fd0ac)

For static mode, you can also use `TypeReference` to filter the types you want.

```csharp
using SaintsField;
using SaintsField.Events;

[TypeReference(onlyAssemblies: new []{"mscorlib"})]  // we only want types from mscorlib
public SaintsEvent sEvent;

[TypeReference(EType.CurrentOnly)]  // we only want types from current assembly
public SaintsEvent sEvent2;
```

## Addressable ##

These tools are for [Unity Addressable](https://docs.unity3d.com/Packages/com.unity.addressables@latest). It's there only if you have `Addressable` installed.

Namespace: `SaintsField.Addressable`

If you encounter issue because of version incompatible with your installation, you can add a macro `SAINTSFIELD_ADDRESSABLE_DISABLE` to disable this component (See "Add a Macro" section for more information)

### `AddressableLabel` ###

A picker to select an addressable label.

*   Allow Multiple: No

```csharp
using SaintsField.Addressable;

[AddressableLabel]
public string addressableLabel;
```

![addressable_label](https://github.com/user-attachments/assets/55122753-7247-42a1-8743-ffec6e60e6ef)

### `AddressableAddress` ###

A picker to select an addressable address (key).

*   `string group = null` the Addressable group name. `null` for all groups
*   `params string[] orLabels` the addressable label names to filter. Only `entries` with this label will be shown. `null` for no filter.

    If it requires multiple labels, use `A && B`, then only entries with both labels will be shown.

    If it requires any of the labels, just pass them separately, then entries with either label will be shown. For example, pass `"A && B", "C"` will show entries with both `A` and `B` label, or with `C` label.

*   Allow Multiple: No

```csharp
using SaintsField.Addressable;

[AddressableAddress]  // from all group
public string address;

[AddressableAddress("Packed Assets")]  // from this group
public string addressInGroup;

[AddressableAddress(null, "Label1", "Label2")]  // either has label `Label1` or `Label2`
public string addressLabel1Or2;

// must have both label `default` and `Label1`
// or have both label `default` and `Label2`
[AddressableAddress(null, "default && Label1", "default && Label2")]
public string addressLabelAnd;
```

![addressable_address](https://github.com/TylerTemp/SaintsField/assets/6391063/5646af00-c167-4131-be06-7e0b8e9b102e)

### `AddressableResource` ###

A simple inline editor for `AssetReference` field.

This tool allows you to add/edit/delete an addressable asset's address, label, and group, without opening the `Addressable` window.

*   Allow Multiple: No

```csharp
[AddressableResource]
public AssetReferenceSprite spriteRef;
```

[![video](https://github.com/user-attachments/assets/7cd6d490-6551-4a51-b1e8-0fbff41ac519)](https://github.com/user-attachments/assets/48601cc1-85c5-487f-b87e-94531517f849)

### `AddressableScene` ###

A picker to select an addressable scene into a string field.

**Parameters**:

*   [Optional] `bool sepAsSub = true`: if true, use `/` as a seperator as sub-list
*   `string group = null`: the Addressable group name. `null` for all groups
*   `params string[] orLabels`: the addressable label names to filter. Only `entries` with this label will be shown. `null` for no filter.

    If it requires multiple labels, use `A && B`, then only entries with both labels will be shown.

    If it requires any of the labels, just pass them separately, then entries with either label will be shown. For example, pass `"A && B", "C"` will show entries with both `A` and `B` label, or with `C` label.

```csharp
[AddressableScene] public string sceneKey;
// don't use nested list when picking
// only use scenes from `Scenes` group
// with label `Battle`, or `Profile`
[AddressableScene(false, "Scenes", "Battle", "Profile")] public string sceneKeySep;
```

[![video](https://github.com/user-attachments/assets/743def95-8b60-4453-9b1d-1a2f263165a1)](https://github.com/user-attachments/assets/e2718c8d-6372-4e16-bbdc-3b5fb331ce5e)

### `AddressableSubAssetRequired` ##

Validate if a sub-asset is signed in type like `Addressable.AssetReferenceSprite`

```csharp
[AddressableSubAssetRequired] public AssetReferenceSprite sprite2;
[AddressableSubAssetRequired("Please pick a sub asset", EMessageType.Warning)] public AssetReferenceSprite sprite3;
```

## AI Navigation ##

These tools are for [Unity AI Navigation](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/) (`NavMesh`). It's there only if you have `AI Navigation` installed.

Namespace: `SaintsField.AiNavigation`

Adding marco `SAINTSFIELD_AI_NAVIGATION_DISABLED` to disable this component. (See "Add a Macro" section for more information)

### `NavMeshAreaMask` ###

Select `NavMesh` area bit mask for an integer field. (So the integer value can be used in `SamplePathPosition`)

*   Allow Multiple: No

```csharp
using SaintsField.AiNavigation;

[NavMeshAreaMask]
public int areaMask;
```

![nav_mesh_area_mask](https://github.com/TylerTemp/SaintsField/assets/6391063/acbd016b-6001-4440-86b6-e3278216bdde)

### `NavMeshArea` ###

Select a `NavMesh` area for a string or an interger field.

*   `bool isMask=true` if true, it'll use the bit mask, otherwise, it'll use the area value. Has no effect if the field is a string.
*   `string groupBy = ""` for error message grouping

*   Allow Multiple: No

```csharp
using SaintsField.AiNavigation;

[NavMeshArea]  // then you can use `areaSingleMask1 | areaSingleMask2` to get multiple masks
public int areaSingleMask;

[NavMeshArea(false)]  // then you can use `1 << areaValue` to get areaSingleMask
public int areaValue;

[NavMeshArea]  // then you can use `NavMesh.GetAreaFromName(areaName)` to get areaValue
public int areaName;

[NavMeshArea] public string areaNameString;  // sting name
```

![nav_mesh_area](https://github.com/TylerTemp/SaintsField/assets/6391063/41da521c-df9e-45a0-aea6-ff1a139a5ff1)

## Netcode for Game Objects ##

Unity's [Netcode for Game Objects](https://docs-multiplayer.unity3d.com/netcode/current/about/) uses a custom editor that
`SaintsEditor` can not be applied to.

To use ability from `SaintsEditor`, the most simple way is to inherent from `SaintsField.Playa.SaintsNetworkBehaviour`

```csharp
using SaintsField.Playa;
using Unity.Netcode;
using UnityEngine;

public class RpcTestSaints : SaintsNetworkBehaviour  // inherent this one
{
    [PlayaInfoBox("Saints Info Box for Array")]  // SaintsEditor specific decorator
    public int[] normalIntArrays;

    [LayoutStart("SaintsLayout", ELayout.FoldoutBox)]  // SaintsEditor specific decorator
    public string normalString;

    [ResizableTextArea]
    public string content;

    public NetworkVariable<int> testVar = new NetworkVariable<int>(0);
    public NetworkList<bool> TestList = new NetworkList<bool>();

    [Button]  // SaintsEditor specific decorator
    private void TestRpc()
    {
        Debug.Log("Button Invoked");
    }
}
```

Result using `SaintsNetworkBehaviour`:

![image](https://github.com/user-attachments/assets/1ee1cf4e-8f3f-49d8-94c3-c37449246cdc)

Result using default one:

![image](https://github.com/user-attachments/assets/74952ea4-60f1-4327-8f17-4db6c06b820d)

The drawer is called `SaintsField.Editor.Playa.NetCode.SaintsNetworkBehaviourEditor`, in case if you want to apply it manually.

Please note: `NetworkVariable` and `NetworkList` will always be rendered at the top, just like Unity's default behavior. Putting it under `Layout` will not change this order and will have no effect.

## Spine ##

[`Spine`](http://en.esotericsoftware.com/spine-in-depth) has [Unity Attributes](http://en.esotericsoftware.com/spine-unity) like `SpineAnimation`,
but it has some limit, e.g. it can not be used on string, it can not report an error if the target is changed, mismatch with skeleton or missing etc.

`SainsField`'s spine attributes allow more robust references, and are supported by `Auto Validator` tool, with searching supported.

These tools are there only if you have `Spine` installed.

Namespace: `SaintsField.Spine`

### `SpineAnimationPicker` ###

Pick a spine animation from a spine skeleton renderer, to a string field or a `AnimationReferenceAsset` field.

**Parameters**

*   `string skeletonTarget = null`: the target, either be a `SkeletonData`, `SkeletonRenderer`, or component/gameObject with `SkeletonRenderer` attached.
    Use `GetComponent<SkeletonRenderer>()` to the current object if null.

```csharp
using SaintsField.Spine;

// get on current target
[SpineAnimationPicker] private string animationName;

// get from other field or callback
public SkeletonAnimation _spine;
[SpineAnimationPicker(nameof(_spine))] private AnimationReferenceAsset animationRef;
```

[![video](https://github.com/user-attachments/assets/16c41cfe-3b27-474b-a0c0-40fad4a12c39)](https://github.com/user-attachments/assets/a26a2417-5b22-4a03-b17c-bd1883c65de2)

### `SpineSkinPicker` ###

Pick a spine skin from a spine skeleton renderer, into a string field.

**Parameters**

*   `string skeletonTarget = null`: the target, either be a `SkeletonData`, `SkeletonRenderer`, or component/gameObject with `SkeletonRenderer` attached.
    Use `GetComponent<SkeletonRenderer>()` to the current object if null.

```csharp
using SaintsField.Spine;

// get on current target
[SpineSkinPicker] private string skinName;

// get from other field or callback
public SkeletonAnimation _spine;
[SpineSkinPicker(nameof(_spine))] private string skinNameFromTarget;
```

![spine_skin](https://github.com/user-attachments/assets/d5832ffb-ebc1-4482-897b-6126998db285)

### `SpineSlotPicker` ###

Pick a spine slot from a spine skeleton renderer, into a string field.

**Parameters**

*   `bool containsBoundingBoxes = false`: Disables popup results that don't contain bounding box attachments when true.
*   `string skeletonTarget = null`: the target, either be a `SkeletonData`, `SkeletonRenderer`, or component/gameObject with `SkeletonRenderer` attached.
    Use `GetComponent<SkeletonRenderer>()` to the current object if null.

```csharp
using SaintsField.Spine;

// get on current target
[SpineSlotPicker] private string slotName;

// get from other field or callback
public SkeletonAnimation _spine;
[SpineSlotPicker(nameof(_spine))] private string slotNameFromTarget;
```

![](https://github.com/user-attachments/assets/a7d280c1-76c1-49ca-bc63-be97dc7fbc70)

### `SpineAttachmentPicker` ###

Pick a spine attachment from a spine skeleton - skin - slot, into a string field.

**Parameters**

*   `string skeletonTarget = null`: the target, either be a `SkeletonData`, `SkeletonRenderer`, or component/gameObject with `SkeletonRenderer` attached.
    Use `GetComponent<SkeletonRenderer>()` to the current object if null.
*   `string skinTarget = null`: If specified, a locally scoped field with the name supplied by in `skinTarget` will be used to limit the popup results to entries of the named skin
*   `string slotTarget = null`: If specified, a locally scoped field with the name supplied by in `slotTarget` will be used to limit the popup results to children of a named slot
*   `bool currentSkinOnly = true`: Filters results to only include the current Skin. Only valid when a `SkeletonRenderer` is the data source.
*   `bool returnAttachmentPath = false`: Returns a fully qualified path for an Attachment in the format "Skin/Slot/AttachmentName". This path format is only used by the SpineAttachment helper methods like `SpineAttachment.GetAttachment` and `.GetHierarchy`. Do not use full path anywhere else in Spine's system
*   `bool placeholdersOnly = false`: Filters results to exclude attachments that are not children of Skin Placeholders
*   `bool sepAsSub = true`: do not seperate as sub items in the picker.

```csharp
using SaintsField.Spine;

// get on current target
[SpineAttachmentPicker] private string spineAttachmentCurrent;

// get from other field or callback
public SkeletonAnimation _spine;
[SpineAttachmentPicker(nameof(_spine))] private string spineAttachment;
```

![image](https://github.com/user-attachments/assets/2d05f473-5789-482d-8d63-69ec2c732bce)

## DOTween ##

### `DOTweenPlay` ###

> [!IMPORTANT]
> Enable `SaintsEditor` before using

A method decorator to play a `DOTween` animation returned by the method.

The method should not have required parameters, and need to return a `Tween` or a `Sequence` (`Sequence` is actually also a tween).

Parameters:

*   `[Optional] string label = null` the label of the button. Use method name if null. Rich label not supported.
*   `ETweenStop stopAction = ETweenStop.Rewind` the action after the tween is finished or killed. Options are:
    *   `None`: do nothing
    *   `Complete`: complete the tween. This only works if the tween get killed
    *   `Rewind`: rewind to the start state

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;
using SaintsField;

[GetComponent]
public SpriteRenderer spriteRenderer;

[DOTweenPlay]
private Sequence PlayColor()
{
    return DOTween.Sequence()
        .Append(spriteRenderer.DOColor(Color.red, 1f))
        .Append(spriteRenderer.DOColor(Color.green, 1f))
        .Append(spriteRenderer.DOColor(Color.blue, 1f))
        .SetLoops(-1);  // Yes you can make it a loop
}

[DOTweenPlay("Position")]
private Sequence PlayTween2()
{
    return DOTween.Sequence()
        .Append(spriteRenderer.transform.DOMove(Vector3.up, 1f))
        .Append(spriteRenderer.transform.DOMove(Vector3.right, 1f))
        .Append(spriteRenderer.transform.DOMove(Vector3.down, 1f))
        .Append(spriteRenderer.transform.DOMove(Vector3.left, 1f))
        .Append(spriteRenderer.transform.DOMove(Vector3.zero, 1f))
    ;
}
```

The first row is global control. Stop it there will stop all preview.

The check of each row means autoplay when you click the start in the global control.

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/d9479943-b254-4819-af91-c390a9fb2268)](https://github.com/TylerTemp/SaintsField/assets/6391063/34f36f5d-6697-4b68-9773-ce37672b850c)

**Setup**

To use `DOTweenPlay`: `Tools` - `Demigaint` - `DOTween Utility Panel`, click `Create ASMDEF`

### `DOTweenPlayStart` / `DOTweenPlayEnd` ###

> [!IMPORTANT]
> Enable `SaintsEditor` before using

A convenient way to add many method to `DOTweenPlay`.

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

[DOTweenPlayStart(groupBy: "Color")]
private Sequence PlayColor()
{
    return DOTween.Sequence()
        .Append(spriteRenderer.DOColor(Color.red, 1f))
        .Append(spriteRenderer.DOColor(Color.green, 1f))
        .Append(spriteRenderer.DOColor(Color.blue, 1f))
        .SetLoops(-1);
}

private Sequence PlayColor2()  // this will be automaticlly added to DOTweenPlay
{
    return DOTween.Sequence()
        .Append(spriteRenderer.DOColor(Color.cyan, 1f))
        .Append(spriteRenderer.DOColor(Color.magenta, 1f))
        .Append(spriteRenderer.DOColor(Color.yellow, 1f))
        .SetLoops(-1);
}

// this will be automaticlly added to DOTweenPlay
// Note: if you want to add this in DOTweenPlay but also stop the grouping, use:
// [DOTweenPlay("Color", keepGrouping: false)]
private Sequence PlayColor3()
{
    return DOTween.Sequence()
        .Append(spriteRenderer.DOColor(Color.yellow, 1f))
        .Append(spriteRenderer.DOColor(Color.magenta, 1f))
        .Append(spriteRenderer.DOColor(Color.cyan, 1f))
        .SetLoops(-1);
}

[DOTweenPlayEnd("Color")]
public Sequence DoNotIncludeMe() => DOTween.Sequence();    // this will NOT be added
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/db6b60b5-0d1d-43e2-9ab9-b2c7912d7e8d)

## Wwise ##

Wwise itself already has very nice drawer. SaintsField only provide some utility to make it easier to use.

If you can't see the wwise related attributes, please add marco `SAINTSFIELD_WWISE` to enable it.

If you face compatibility issue because of API changes in Wwise, please add marco `SAINTSFIELD_WWISE_DISABLE` to disable it.

### `GetWwise` ###

> [!NOTE]
> This feature is UI Toolkit only

Like the auto getters, this can auto-sign a wwise object (a state, a switch, a soundBank etc.) to a field.

```csharp
using SaintsField.Wwise;

[GetWwise("BGM*")]  // Get a soundBank starts with `BGM`
public Bank bank;

[GetWwise("*BGM*")]  // Get all events contains `BGM`
public Event[] events;

[GetWwise]  // Get the first rtpc found
public RTPC rtpc;

[GetWwise("*/BGM/Stop*")]  // Get events that's under any work unit, under BGM folder, and starts with `Stop`
public Event stopEvents;
```

![image](https://github.com/user-attachments/assets/72007c35-a0f5-4c66-bc29-803947b4d74a)

## I2 Localization ##

Tools for [I2 Localization](https://inter-illusion.com/tools/i2-localization). Enable it in `Window` - `Saints` - `Enable I2 Localization Support`

NameSpace: `SaintsField.I2Loc`

### `LocalizedStringPicker` ###

Pick a term from `I2 Localization` to a string field or a `LocalizedString` field. Support search so you don't need to deal with I2's default painful picker.

```csharp
using SaintsField.I2Loc;

[LocalizedStringPicker] public LocalizedString displayNameTerm;
[LocalizedStringPicker] public string descriptionTerm;
```

## SaintsEditor ##

`SaintsEditor` is a `UnityEditor.Editor` level component.

Namespace: `SaintsField.Playa`

Compared with `NaughtyAttributes` and `MarkupAttributes`:

1.  `NaughtyAttributes` has `Button`, and has a way to show a non-field property(`ShowNonSerializedField`, `ShowNativeProperty`), but it does not retain the order of these fields, but only draw them at the end. It has layout functions (`Foldout`, `BoxGroup`) but it has not `Tab` layout, and much less powerful compared to `MarkupAttributes`. It's IMGUI only.
2.  `MarkupAttributes` is super powerful in layout, but it does not have a way to show a non-field property. It's IMGUI only. It also supports shader editor.
3.  `SaintsEditor`

    *   `Layout` like markup attributes. Compared to `MarkupAttributes`, it allows a non-field property (e.g. a button or a `ShowInInspector` inside a group) (like `OdinInspector`). it has `LayoutGrooup`/`LayoutEnd` for convenience coding.
    *   It provides `Button` (with less function) and a way to show a non-field property (`ShowInInspector`).
    *   It tries to retain the order, and allows you to use `[Ordered]` when it can not get the order (c# does not allow to obtain all the orders).
    *   Supports both `UI Toolkit` and `IMGUI`.

Please note, any `Editor` level component can not work together with each other (it will not cause trouble, but only one will actually work). Which means, `OdinInspector`, `NaughtyAttributes`, `MarkupAttributes`, `SaintsEditor` can not work together.

If you are interested, here is how to use it.

**Setup SaintsEditor**

`Window` - `Saints` - `Enable SaintsEditor`. After the project finish re-compile, go `Window` - `Saints` - `SaintsEditor` to tweak configs.

If you want to do it manually, check [ApplySaintsEditor.cs](https://github.com/TylerTemp/SaintsField/blob/master/Editor/Playa/ApplySaintsEditor.cs) for more information

## `SaintsEditorWindow` ##

An `EditorWindow` class to easily create an editor (a bit like Odin's `OdinEditorWindow`), with support of `StartEditorCoroutine`
and `StopEditorCoroutine`

### Usage & Example ###


Basic usage: inherent from `SaintsField.Editor.SaintsEditorWindow`

```csharp
#if UNITY_EDITOR
using SaintsField.Editor;

public class ExamplePanel: SaintsEditorWindow
{

    [MenuItem("Window/Saints/Example/SaintsEditor")]
    public static void OpenWindow()
    {
        EditorWindow window = GetWindow<ExamplePanel>(false, "My Panel");
        window.Show();
    }

    // fields
    [ResizableTextArea]
    public string myString;

    [ProgressBar(100f)] public float myProgress;

    // life-circle: OnUpdate function
    public override void OnEditorUpdate()
    {
        myProgress = (myProgress + 1f) % 100;
    }

    [ProgressBar(100f)] public float myCoroutine;

    // Layout supported
    [LayoutStart("Coroutine", ELayout.Horizontal)]

    private IEnumerator _startProcessing;

    // Button etc supported
    // EditorCoroutine supported
    [Button]
    public void StartIt()
    {
        StartEditorCoroutine(_startProcessing = StartProcessing());
    }

    [Button]
    public void StopIt()
    {
        if (_startProcessing != null)
        {
            StopEditorCoroutine(_startProcessing);
        }

        _startProcessing = null;
    }

    private IEnumerator StartProcessing()
    {
        myCoroutine = 0;
        while (myCoroutine < 100f)
        {
            myCoroutine += 1f;
            yield return null;
        }
    }

    // Other life-circle
    public override void OnEditorEnable()
    {
        Debug.Log("Enable");
    }

    public override void OnEditorDisable()
    {
        Debug.Log("Disable");
    }

    public override void OnEditorDestroy()
    {
        Debug.Log("Destroy");
    }
}

#endif
```

[![video](https://github.com/user-attachments/assets/a724f450-0d36-4b39-aada-c180d2d8990b)](https://github.com/user-attachments/assets/f80f4de2-4d12-47dc-be82-aff2e3ba2c0b)

An example of using as a `ScriptableObject` editor (or any serializable object)

```csharp
#if UNITY_EDITOR
using SaintsField.Editor;
using SaintsField.Playa;

public class ExampleSo: SaintsEditorWindow
{
    [MenuItem("Window/Saints/Example/ScriptableEditor")]
    public static void OpenWindow()
    {
        EditorWindow window = GetWindow<ExampleSo>(false, "Scriptable Editor");
        window.Show();
    }

    [
        AdvancedDropdown(nameof(ShowDropdown)),
        OnValueChanged(nameof(TargetChanged))
    ]
    public ScriptableObject inspectTarget;

    [WindowInlineEditor]  // this will inline the serialized object editor
    public Object editorInlineInspect;

    private IReadOnlyList<ScriptableObject> GetAllSo() => Resources.LoadAll<ScriptableObject>("");

    private AdvancedDropdownList<ScriptableObject> ShowDropdown()
    {
        AdvancedDropdownList<ScriptableObject> down = new AdvancedDropdownList<ScriptableObject>();
        down.Add("[Null]", null);
        foreach (ScriptableObject scriptableObject in GetAllSo())
        {
            down.Add(scriptableObject.name, scriptableObject);
        }

        return down;
    }

    private void TargetChanged(ScriptableObject so)
    {
        Debug.Log($"changed to {so}");
        editorInlineInspect = so;
        titleContent = new GUIContent(so == null? "Pick One": $"Edit {so.name}");
    }

    [LayoutStart("Buttons", ELayout.Horizontal)]

    [Button]
    private void Save() {}

    [Button]
    private void Discard() {}
}
#endif
```

[![video](https://github.com/user-attachments/assets/855b628a-0c3a-4cc1-90b7-115d401f1cfa)](https://github.com/user-attachments/assets/665ebe8a-22b1-4df8-8f20-47fa8b23e3b9)

### Life Circle & Functions ###

The following are the life circle functions, some are wrapped around Unity EditorWindow's default life circle callback:

*   `public virtual void OnEditorDestroy()` -> [OnDestroy](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/EditorWindow.OnDestroy.html)
*   `public virtual void OnEditorEnable()` -> [OnEnable](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ScriptableObject.OnEnable.html)
*   `public virtual void OnEditorDisable()` -> [OnDisable](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ScriptableObject.OnDisable.html)
*   `public virtual void OnEditorUpdate()`: like `OnUpdate`, gets called every 1 millisecond in UI Toolkit, and every frame when user have interactive in IMGUI.
*   `public void StartEditorCoroutine(IEnumerator routine)`: like `StartCoroutine`, but for editor coroutine. No return value.
*   `public void StopEditorCoroutine(IEnumerator routine)`: like `StopCoroutine`, but for editor coroutine.

### `WindowInlineEditor` ###

This decorator only works in `SaintsEditorWindow`. It'll render the target object's editor in the window. Not work for array/list.

See the example above.

## Misc ##

### About GroupBy ##

group with any decorator that has the same `groupBy` for this field. The same group will share even the width of the view width between them.

This only works for decorator draws above or below the field. The above drawer will not groupd with the below drawer, and vice versa.

`""` means no group.

### `EMode` ###

*   `EMode.Edit`: the Unity Editor is not playing
*   `EMode.Play`: the Unity Editor is playing
*   `EMode.InstanceInScene`: target is a prefab placed in a scene
*   `EMode.InstanceInPrefab`: target is inside a prefab (but is not the top root of that prefab)
*   `EMode.Regular`: target is at the top root of the prefab
*   `EMode.Variant`: target is at the top root of the prefab, and is also a variant prefab
*   `EMode.NonPrefabInstance`: target is not a prefab (but can be inside a prefab)
*   `EMode.PrefabInstance` = `InstanceInPrefab | InstanceInScene`
*   `EMode.PrefabAsset` = `Variant | Regular`

### Callback ###

For decorators that accept a callback, you can usually use `$` to indicate that you want a callback. The callback can be a method, a property, or a field.

Use `\\$` if you do not want it to be a callback. This is useful for decorators like `RichLabel`, `InfoBox` that the displaying string itself starts with `$`.

Using `$:` if the callback is a static/const field. We support the following style:

```csharp
namespace SaintsField.Samples.Scripts
{
    public class StaticCallback : SaintsMonoBehaviour
    {
        private static readonly string StaticString = "This is a static string";
        private const string ConstString = "This is a constant string";

        // using full type name
        [AboveRichLabel("$:SaintsField.Samples.Scripts." + nameof(StaticCallback) + "." + nameof(StaticString))]
        // using only type name. This is slow and might find the incorrect target.
        // We'll first search the assembly of this object. If not found, then search all assemblies.
        [InfoBox("$:" + nameof(StaticCallback) + "." + nameof(ConstString))]
        public int field;

#if UNITY_EDITOR
        private static Texture2D ImageCallback(string name) =>
            AssetDatabase.LoadAssetAtPath<Texture2D>(
                $"Assets/SaintsField/Editor/Editor Default Resources/SaintsField/{name}.png");
#endif

#if UNITY_EDITOR
        // use only field/method name. This will only try to search on the current target.
        [BelowImage("$:" + nameof(ImageCallback), maxWidth: 20)]
#endif
        public string imgName;

        [ShowInInspector] private static bool _disableMe = true;

#if UNITY_EDITOR
        [DisableIf("$:" + nameof(_disableMe))]
        [RequiredIf("$:" + nameof(_disableMe), false)]
#endif
        public string disableIf;
    }
}
```

You can skip the `namespace` part. And if you also skip the `type` part, we'll try to find the callback from the current type first, then search all types in the current assembly, and finally search all types in all assemblies.

Note: decorators like `OnEvent`, `OnButtonClick` does not support this `$:` yet. I'm still working on making all APIs consistent.

### Syntax for Show/Hide/Enable/Disable/Required-If ##

This applies to `ShowIf`, `HideIf`, `EnableIf`, `DisableIf`, `RequiredIf`, `PlayaShowIf`, `PlayaHideIf`, `PlayaEnableIf`, `PlayaDisableIf`.

These decorators accept many objects.

**By Default**

Passing many strings, each string is represent either a field, property or a method. SaintsField will check the value accordingly. If it's a method, it'll also receive the field's value and index if it's inside an array/list.

**Sub Field**

You can use dot (`.`) to obtain a sub-field from a field/property/method. This is useful if your condition is relayed on a sub-field of a field.

```csharp
using SaintsField;
[GetComponentInChildren, Expandable] public ToggleSub toggle;

// Show if toggle.requireADescription is a truly value
[ShowIf(nameof(toggle) + ".requireADescription")]
public string description;

// ToggleSub.cs
public class ToggleSub : MonoBehaviour
{
    [LeftToggle] public bool requireADescription;
}
```

[![video](https://github.com/user-attachments/assets/f001fd35-3dc3-469f-b08f-127ab0448984)](https://github.com/user-attachments/assets/11e3029b-a03c-47b1-949a-a0d59b150cbd)

**Value Equality**

You can also pass a string, then followed by a value you want to compare with. For example:

```csharp
using SaintsField;

public int myInt;

[EnableIf(nameof(myInt), 2] public int enableIfEquals2;
```

This field will only be enabled if the `myInt` is equal to `2`.

This can be mixed with many pairs:

```csharp
using SaintsField;

public int myInt1;
public int myInt2;

[EnableIf(nameof(myInt1), 2, nameof(myInt2), 3] public int enableIfMultipleCondition;
```

multiple conditions are logically chained accordingly. See each section of these decorators for more information.

**Value Comparison**

The string can have `!` prefix to negate the comparison.

And `==`, `!=`, `>` etc. suffix for more comparison you want. The suffix supports:

Comparison Suffixes:

*  `==`: equal to the next parameter
*  `==$`: equal, but the value is a callback by the next parameter
*  `!=`: not equal to the next parameter
*  `>`: greater than the next parameter
*  `>$`: greater than, but the value is a callback by the next parameter
*  `<`: less than the next parameter
*  `<$`: less than, but the value is a callback by the next parameter
*  `>=`: greater or equal to the next parameter
*  `>=$`: greater or equal, but the value is a callback by the next parameter
*  `<=`: less or equal to the next parameter
*  `<=$`: less or equal, but the value is a callback by the next parameter

Bitwise Suffixes:

*  `&`: bitwise and with the next parameter
*  `&$`: bitwise and, but the value is a callback by the next parameter
*  `^`: bitwise xor with the next parameter
*  `^$`: bitwise xor, but the value is a callback by the next parameter
*  `&==`: bitwise has flag of the next parameter
*  `&==$`: bitwise has flag, but the value is a callback by the next parameter

Some examples:

```csharp
using SaintsField;

public bool boolValue;
[EnableIf("!" + nameof(boolValue)), RichLabel("Reverse!")] public string boolValueEnableN;

[Range(0, 2)] public int int01;

[EnableIf(nameof(int01), 1), RichLabel("default")] public string int01Enable1;
[EnableIf(nameof(int01) + ">=", 1), RichLabel(">=1")] public string int01EnableGe1;
[EnableIf("!" + nameof(int01) + "<=", 1), RichLabel("! <=1")] public string int01EnableNLe1;

[Range(0, 2)] public int int02;
// we need the "==$" to tell the next string is a value callback, not a condition checker
[EnableIf(nameof(int01) + "==$", nameof(int02)), RichLabel("==$")] public string int01Enable1Callback;
[EnableIf(nameof(int01) + "<$", nameof(int02)), RichLabel("<$")] public string int01EnableLt1Callback;
[EnableIf("!" + nameof(int01) + ">$", nameof(int02)), RichLabel("! >$")] public string int01EnableNGt1Callback;

// example of bitwise
[Serializable]
public enum EnumOnOff
{
    A = 1,
    B = 1 << 1,
}

[Space]
[Range(0, 3)] public int enumOnOffBits;

[EnableIf(nameof(enumOnOffBits) + "&", EnumOnOff.A), RichLabel("&01")] public string bitA;
[EnableIf(nameof(enumOnOffBits) + "^", EnumOnOff.B), RichLabel("^10")] public string bitB;
[EnableIf(nameof(enumOnOffBits) + "&==", EnumOnOff.B), RichLabel("hasFlag(B)")] public string hasFlagB;
```

**Default Value Comparison**

When passing the parameters, any parameter that is not a string, means it's a value comparison to the previous one.

(Which also means, to compare with a literal string value, you need to suffix the previous string with `==`)

`[ShowIf(nameof(MyFunc), 3)]` means it will check if the result of `MyFunc` equals to `3`.

However, if the later parameter is a bitMask (an enum with `[Flags]`), it'll check if the target has the required bit on:

```csharp
using SaintsField;

[Flags, Serializable]
public enum EnumF
{
    A = 1,
    B = 1 << 1,
}

[EnumFlags]
public EnumF enumF;

[EnableIf(nameof(enumF), EnumF.A), RichLabel("hasFlag(A)")] public string enumFEnableA;
[EnableIf(nameof(enumF), EnumF.B), RichLabel("hasFlag(B)")] public string enumFEnableB;
[EnableIf(nameof(enumF), EnumF.A | EnumF.B), RichLabel("hasFlag(A | B)")] public string enumFEnableAB;
```

### Saints XPath-like Syntax ##

#### XPath ####

This part is how a target is found, a simplified [XML Path Language](https://developer.mozilla.org/en-US/docs/Web/XPath).

basic syntax: `step/step/step...`

for each step: `axis::nodetest[predicate][predicate OR predicate]/othersteps...`

Please note: powerful as the seems, there are many edge cases I have not covered yet. Report an issue if you face any.

**`nodetest`**

`nodetest` is like a path, use `/` to separate, `//` means any descendant

`.` means current object, `..` means parent, `*` means any node.

`nodetest` always starts from the current object.

```csharp
// `DirectChild` object under this object
[GetByXPath("/DirectChild")] public GameObject directChild;
// Search all children that starts with `StartsWith`,
// under which, search all children ends with `Child`
// and get all the direct children of that.
[GetByXPath("//StartsWith*//*Child/*")] public Transform[] searchChildren;
```

**`axis`**

`axis` redirect the target point

*   `ancestor::`: all parents
*   `ancestor-or-self::`: the object itself, and all it's parents.
*   `ancestor-inside-prefab::`: all parents inside this prefab
*   `ancestor-or-self-inside-prefab::`: the object itself, and all it's parents inside this prefab
*   `parent::`: parent of the object
*   `parent-or-self::`: the object itself, and it's parent
*   `parent-inside-prefab::`: parent inside this prefab
*   `parent-or-self-inside-prefab::`: this object itself, and it's parent inside this prefab
*   `scene::`: root of the active scene
*   `prefab::`: root of the current prefab
*   `resources::`: `Resources`
*   `assets::`: root folder `Assets`

```csharp
// search all parents that starts with `Sub`
[GetByXPath("ancestor:://Sub*")] public Transform ancestorStartsWithSub;

// search object itself, and all it's parents, which contains `This`
[GetByXPath("ancestor-or-self::*This*")] public Transform[] parentsSelfWithThis;

// search current scene that ends with `Camera`
[GetByXPath("scene:://*Camera")] public Camera[] sceneCameras;

// get the first folder starts with `Issue`, and get all the prefabs
[GetByXPath("assets:://Issue*/*.prefab")] public GameObject[] prefabs;
```

**`attribute`**

`attribute` allows you to extract an attribute from a target, starting with a `@` letter. Normally, `{}` means it
can be directly executed on the target.

*   `@layer`: Get the layer string name
*   `@{layer}`: Get the layer mask (int)
*   `@{tag}`: Get the tag value
*   `@{gameObject}`: Get the `gameObject` (this is the default behavior)
*   `@{transform}`: Get the `transform`
*   `@{rectTransform}`: Get the `RectTransform`. This is just a shortcut of`GetComponent(RectTransform)`
*   `@{activeSelf}`/`@{gameObject.activeSelf}`
*   `@{GetComponent(MyScript)}`/`@{GetComponents(MyScript)[2]}` Get a component from the target.
    You can continuously chain the calling like: `@{GetComponents(MyComponent)[-1].MyFunction().someField['key']}`.

    Please note: this is not an actual code executing, and with these limits:

    1.  Parameters are not supported
    2.  indexing for array/list is allowed
    3.  indexing for dictionary only supports `string` key type, and single quote / double quote are the same

    `GetComponent` & `GetComponents` are a special function. You can pass type name. If you have multiple type with the same name,
    prefix it with some `namespace` is allowed: `GetComponent(MyNameSpace.MyScript)`.

    Base class is allow allowed, but generic class is not supported.

*   `@{GetComponents()}`: Get all components of the target
*   ~~`@resource-path()`~~
*   ~~`@asset-path()`~~

```csharp
// 1.  search all the children which has `FunctionProvider` script, grab the first result
// 2.  call `GetTransforms()` as the results
// 3.  from the results, get first direct children named "ok"
[GetByXPath("//*@{GetComponent(FunctionProvider).GetTransforms()}/ok")] public GameObject[] getObjs;

// FunctionProvider.cs
public class FunctionProvider : MonoBehaviour
{
    // Example of returning some target
    public Transform[] GetTransforms() => transform.Cast<Transform>().ToArray();
}
```

**`filter`**

`filter` check if the results match the expected condition. There are two types of filter:

*   index filter:

    either just use the index: `child*[1]` (second one), `child*[last()]`/`child*[-1]` (last one)

    or compared value: `child*[index() > 3]`

*   value filter: use any `attribute` mentioned above, with `>`, `!=` etc. for comparison. e.g. `[@{gameObject.activeSelf}][@layer = "UI"]`

using multiple filters means all conditions must be met. Otherwise, use the keyword `OR`: `[@{GetComponent(Collider)} OR @{GetComponent(MyScript)}]`

```csharp
// find the first main camera in the scene
[GetByXPath("scene:://[@{tag} = MainCamera]")] public Camera mainCamera;

// find the prefabs with component "Hero"
[GetByXPath("assets:://Heros/*.prefab[@GetComponent(Hero)]")] public Camera mainCamera;
```

#### `EXP` ####

`EXP` is how all the auto getters works, behaviors in the inspector. The values are:

*   `NoInitSign`: do not sign the value if the value is null on firsts rendering.
*   `NoAutoResignToValue`: do not sign the value to the correct value on the following renderings.
*   `NoAutoResignToNull`: do not sign the value to null value if the target disappears on the following renderings.
*   `NoResignButton`: when `NoAutoResign` is on, by default there will be a `reload` button when value is mismatched. Turn this on to hide the `reload` button.
*   `NoMessage`: when `NoAutoResign` and `NOResignButton` is on, by default there will be an error box when value is mismatched. Turn this on to hide the error message.
*   `NoPicker`: this will remove the custom picker. This is on by default (if you do not pass `EXP` as first argument) to keep the consistency.
*   `KeepOriginalPicker`: UI Toolkit only. By default, when a custom picker is shown, Unity's default picker will hide. This will keep Unity's picker together.
*   `ForceReOrder`: Force the auto-getters to changes the order the way how the resources are found for list/array. This is useful when you want to get children of way points, as the order is important.

And some shortcut:

*   `NoAutoResign` = `NoAutoResignToValue | NoAutoResignToNull`
*   `Silent` = `NoAutoResign | NoMessage`. Useful if you want to allow you to manually sign a different value with no buttons and error box.
*   `JustPicker` = `NoInitSign | NoAutoResignToValue | NoAutoResignToNull | NoResignButton | NoMessage`. Do nothing but just give you a picker with matched targets.
*   `Message` = `NoAutoResignToValue | NoAutoResignToNull | NoResignButton`. Just give an error message if target is mismatched.

### Add a Macro ##

Pick a way that is most convenient for you:

**Using Saints Menu**

Go to `Window` - `Saints` to enable/disable functions you want

![Saints Menu](https://github.com/TylerTemp/SaintsField/assets/6391063/272e72c3-656c-47e4-adc6-75ba62f7f432)

**Using csc.rsp**

1.  Create file `Assets/csc.rsp`
2.  Write marcos like this:

    ```bash
    #"Disable DOTween"
    -define:SAINTSFIELD_DOTWEEN_DISABLE

    #"Disable Addressable"
    -define:SAINTSFIELD_ADDRESSABLE_DISABLE

    #"Disable AI Navigation"
    -define:SAINTSFIELD_AI_NAVIGATION_DISABLED

    #"Disable UI Toolkit"
    -define:SAINTSFIELD_UI_TOOLKIT_DISABLE

    #"Enable SaintsEditor project wide"
    -define:SAINTSFIELD_SAINTS_EDITOR_APPLY

    #"Disable SaintsEditor IMGUI constant repaint"
    -define:SAINTSFIELD_SAINTS_EDITOR_IMGUI_CONSTANT_REPAINT_DISABLE

    #"Enable IMGUI duplicated decorators drawing fix"
    -define:SAINTSFIELD_IMGUI_DUPLICATE_DECORATOR_FIX
    ```

Note: `csc.rsp` can override settings by Saints Menu.

**Using project settings**

`Edit` - `Project Settings` - `Player`, find your platform, then go `Other Settings` - `Script Compliation` - `Scripting Define Symbols` to add your marcos. Don't forget to click `Apply` before closing the window.

### Auto Validator ###

UI Toolkit: A simple validation tool under `Window` - `Saints` - `Auto Validator`, related to [#115](https://github.com/TylerTemp/SaintsField/discussions/115)

This tool allows you to check if some target has `Required` but not filled, or an auto getter (e.g. `GetComponentInChildren`) but not filled or mismatched. Auto getters error will give you a button to fix it there. Note the fix function might be broken if the target is inside a prefab.

You can specify the targets as you want. Currently, it supports scenes, and folder searching.

It can also specify if you want to skip the hidden fields (hidden by `ShowIf`, `HideIf`. Not work for `LayoutShowIf`, `LayoutHideIf`)

This tool is very simple, and will get more update in the future.

See [Auto Validator example code](https://github.com/TylerTemp/SaintsField/blob/master/Editor/AutoRunner/AutoRunnerTemplate.cs) to learn how to make a quick auto validator for a specific group of assets.

[![video](https://github.com/user-attachments/assets/bf5e7b7a-c15c-4fa4-92b9-53621d41ccb4)](https://github.com/user-attachments/assets/76683bc3-cfea-4952-9377-788e02d7e075)

### Common Pitfalls & Compatibility ###

#### List/Array & Nesting ####

Directly using on list/array will apply to every direct element of the list, this is a limit from Unity.

Unlike NaughtyAttributes, `SaintsField` does not need a `AllowNesting` attribute to work on nested element.

```csharp
public class ArrayLabelExample : MonoBehaviour
{
    // works for list/array
    [Scene] public int[] myScenes;

    [System.Serializable]
    public struct MyStruct
    {
        public bool enableFlag;

        [AboveRichLabel("No need for `[AllowNesting]`, it just works")]
        public int integer;
    }

    public MyStruct myStruct;
}
```

#### Order Matters ####

`SaintsField` only uses `PropertyDrawer` to draw the field, and will properly fall back to the rest drawers if there is one.
This works for both 3rd party drawer, your custom drawer, and Unity's default drawer.

However, Unity only allows decorators to be loaded from top to bottom, left to right. Any drawer that does not properly handle the fallback
will override `PropertyDrawer` follows by. Thus, ensure `SaintsField` is always the first decorator.

An example of working with NaughtyAttributes:

```csharp
using SaintsField;

[RichLabel("<color=green>+NA</color>"),
 NaughtyAttributes.CurveRange(0, 0, 1, 1, NaughtyAttributes.EColor.Green),
 NaughtyAttributes.Label(" ")  // a little hack for label issue
]
public AnimationCurve naCurve;

[RichLabel("<color=green>+Native</color>"), Range(0, 5)]
public float nativeRange;

// this wont work. Please put `SaintsField` before other drawers
[Range(0, 5), RichLabel("<color=green>+Native</color>")]
public float nativeRangeHandled;

// this wont work too. Please put `SaintsField` before other drawers
[NaughtyAttributes.CurveRange(0, 0, 1, 1, NaughtyAttributes.EColor.Green)]
[RichLabel("<color=green>+NA</color>")]
public AnimationCurve naCurveHandled;
```

#### Fallback To Other Drawers ####

`SaintsField` is designed to be compatible with other drawers if

1.  the drawer itself respects the `GUIContent` argument in `OnGUI` for IMGUI, or add `unity-label` class to Label for UI Toolkit

    NOTE: `NaughtyAttributes` (IMGUI) uses `property.displayName` instead of `GUIContent`. You need to set `Label(" ")` if you want to use `RichLabel`.
    Might not work very well with `NaughtyAttributes`'s meta attributes because they are editor level components.

2.  if the drawer hijacks the `CustomEditor`, it must fall to the rest drawers

    NOTE: In many cases `Odin` does not fall back to the rest drawers, but only to `Odin` and Unity's default drawers. So sometimes things will not work with `Odin`

Special Note:

NaughtyAttributes uses only IMGUI. If you're using Unity 2022.2+, `NaughtyAttributes`'s editor will try fallback default drawers and Unity will decide to use UI Toolkit rendering `SaintsField` and cause troubles.
Please disable `SaintsField`'s UI Toolkit ability by `Window` - `Saints` - `Disable UI Toolkit Support` (See "Add a Macro" section for more information)

My (not full) test about compatibility:

*   [Markup-Attributes](https://github.com/gasgiant/Markup-Attributes): Works very well.
*   [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes): Works well, need that `Label` hack.
*   [OdinInspector](https://odininspector.com/): Works mostly well for MonoBehavior/ScriptableObject. Not so good when it's inside Odin's own serializer.
