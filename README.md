# SaintsField #

[![unity_version](https://github.com/TylerTemp/SaintsField/assets/6391063/c01626a1-9329-4c26-be31-372f8704df1d)](https://unity.com/download)
[![license_mit](https://github.com/TylerTemp/SaintsField/assets/6391063/a093811a-5dbc-46ad-939e-a9e207ae5bfb)](https://github.com/TylerTemp/SaintsField/blob/master/LICENSE)
[![openupm](https://img.shields.io/npm/v/today.comes.saintsfield?label=OpenUPM&registry_uri=https://package.openupm.com)](https://openupm.com/packages/today.comes.saintsfield/)
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

namespace: `SaintsField`

### Change Log ###

**3.5.2**

1.  Fix multiple auto getters on a same field will cause partly filled values.
2.  Add `bool delayedSearch = false` for `ListDrawerSettings` to delay the search until you hit enter or blur the search field

See [the full change log](https://github.com/TylerTemp/SaintsField/blob/master/CHANGELOG.md).

## General Attributes ##

### Label & Text ###

#### `RichLabel` ####

*   `string|null richTextXml` the content of the label, supported tag:

    *   All Unity rich label tag, like `<color=#ff0000>red</color>`
    *   `<label />` for current field name
    *   `<icon=path/to/image.png />` for icon
    *   `<container.Type />` for the class/struct name of the container of the field
    *   `<container.Type.BaseType />` for the class/struct name of the field's container's parent

    `null` means no label

    for `icon` it will search the following path:

    *   `"Assets/Editor Default Resources/SaintsField/"`  (You can override things here)
    *   `"Assets/SaintsField/Editor/Editor Default Resources/SaintsField/"` (this is most likely to be when installed using `unitypackage`)
    *   `"Packages/today.comes.saintsfield/Editor/Editor Default Resources/SaintsField/"` (this is most likely to be when installed using `upm`)
    *   `Assets/Editor Default Resources/`, then fallback to built-in editor resources by name (using [`EditorGUIUtility.Load`](https://docs.unity3d.com/ScriptReference/EditorGUIUtility.Load.html))

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

Here is an example of using on a array:

```csharp
using SaintsField;

[RichLabel(nameof(ArrayLabels), true)]
public string[] arrayLabels;

// if you do not care about the actual value, use `object` as the first parameter
private string ArrayLabels(object _, int index) => $"<color=pink>[{(char)('A' + index)}]";
```

![label_array](https://github.com/TylerTemp/SaintsField/assets/6391063/232da62c-9e31-4415-a09a-8e1e95ae9441)

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

This is like `InfoBox`, but it can be applied to array/list/button etc.

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

#### `SepTitle` ####

A separator with text. (Recommend to use `Separator` instead.)

*   `string title=null` title, `null` for no title at all. Does **NOT** support rich text
*   `EColor color`, color for title and line separator
*   `float gap = 2f`, space between title and line separator
*   `float height = 2f`, height of this decorator

```csharp
using SaintsField;

[SepTitle("Separate Here", EColor.Pink)]
public string content1;

[SepTitle(EColor.Green)]
public string content2;
```

![sep_title](https://github.com/TylerTemp/SaintsField/assets/6391063/55e08b48-4463-4be3-8f87-7afd5ce9e451)

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

*   `string buttonLabel = null` the button label. If null, it'll use the function name.

```csharp
// Please ensure you already have SaintsEditor enabled in your project before trying this example
using SaintsField.Playa;

[Button]
private void EditorButton()
{
    Debug.Log("EditorButton");
}

[Button("Label")]
private void EditorLabeledButton()
{
    Debug.Log("EditorLabeledButton");
}
```

![button](https://github.com/TylerTemp/SaintsField/assets/6391063/2f32336d-ca8b-46e0-9ac8-7bc44aada54b)

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

A dropdown selector for a scene in the build list, plus a "Edit Scenes In Build..." option to directly open the "Build Settings" window where you can change building scenes.

*   AllowMultiple: No

```csharp
using SaintsField;

[Scene] public int _sceneInt;
[Scene] public string _sceneString;
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/0da47bd1-0741-4707-b96b-6c08e4c5844c)

#### `SortingLayer` ####

A dropdown selector for sorting layer, plus a "Edit Sorting Layers..." option to directly open "Sorting Layers" tab from "Tags & Layers" inspector where you can change sorting layers.

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

A string dropdown selector for an input axis, plus a "Open Input Manager..." option to directly open "Input Manager" tab from "Project Settings" window where you can change input axes.

*   AllowMultiple: No

```csharp
using SaintsField;

[InputAxis] public string inputAxis;
```

![image](https://github.com/TylerTemp/SaintsField/assets/6391063/68dc47d9-7211-48df-bbd1-c11faa536bd1)

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

Known issue:
1.  IMGUI: if the target itself has a custom drawer, the drawer will not be used, because `PropertyDrawer` is not allowed to create an `Editor` class, thus it'll just iterate and draw all fields in the object.

    For more information about why this is impossible under IMGUI, see [Issue 25](https://github.com/TylerTemp/SaintsField/issues/25)

2.  IMGUI: the `Foldout` will NOT be placed at the left space like a Unity's default foldout component, because Unity limited the `PropertyDrawer` to be drawn inside the rect Unity gives. Trying outside of the rect will make the target non-interactable.
    But in early Unity (like 2019.1), Unity will force `Foldout` to be out of rect on top leve, but not on array/list level... so you may see different outcomes on different Unity version.
3.  UI Toolkit: `ReadOnly` (and `DisableIf`, `EnableIf`) can NOT disable the expanded fields. This is because `InspectorElement` does not work with `SetEnable(false)`, neither with `pickingMode=Ignore`. This can not be fixed unless Unity fixes it.

*   AllowMultiple: No

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

`SaintsRow` attribute allows you to draw `Button`, `Layout`, `ShowInInspector`, `DOTweenPlay` etc (all `SaintsEditor` attributes) in a `Serializable` object (usually a class or a struct).

This attribute does NOT need `SaintsEditor` enabled. It's an out-of-box tool.

Parameters:

*   `bool inline=false`

    If true, it'll draw the `Serializable` inline like it's directly in the `MonoBehavior`

*   AllowMultiple: No

Special Note:

1.  After applying this attribute, only pure `PropertyDrawer`, and decorators from `SaintsEditor` works on this target. Which means, using third party's `PropertyDrawer` is fine, but decorator of Editor level (e.g. Odin's `Button`, NaughtyAttributes' `Button`) will not work.
2.  IMGUI: `ELayout.Horizontal` does not work here
3.  IMGUI: `DOTweenPlay` might be a bit buggy displaying the playing/pause/stop status for each function.

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
*   `bool delayedSearch = false`: when `true`, delay the search until you hit enter or blur the search field

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

#### `ShowInInspector` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

Show a non-field property.

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
*   `string[] subStateMachineNameChain` the sub-state machine hierarchy name list of the state

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
[GetByXPath("scene:://[@Tag = MainCamera][]")] public Camera mainCamera;

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
    *   `EGetComp.NoResignButton`: do not display a resign button when the target mismatches.

*   `string paths...`

    Paths to search.

*   AllowMultiple: Yes. But not necessary.

The `path` is a bit like html's `XPath` but with less functions:

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

*   (Optional) `EMode editorMode=EMode.Edit | EMode.Play`

    Condition: if it should be in edit mode or play mode for Editor. By default (omitting this parameter) it does not check the mode at all.

*   `object by...`

    callbacks or attributes for the condition.

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

It also supports value comparison like `==`, `>`, `<=`. Read more in the "Value Comparison for Show/Hide/Enable/Disable-If" section.

#### `PlayaEnableIf`/`PlayaDisableIf` ####

> [!IMPORTANT]
> Enable `SaintsEditor` before using

This is the same as `EnableIf`, `DisableIf`, plus it can be applied to array, `Button`

Different from `EnableIf`/`DisableIf` in the following:
1.  apply on an array will directly enable or disable the array itself, rather than each element.
2.  Callback function can not receive value and index
3.  this method can not detect foldout, which means using it on `Expandable`, `EnumFlags`, the foldout button will also be disabled. For this case, use `DisableIf`/`EnableIf` instead.

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

It also supports value comparison like `==`, `>`, `<=`. Read more in the "Value Comparison for Show/Hide/Enable/Disable-If" section.

#### `ShowIf` / `HideIf` ####

Show or hide the field based on a condition. . Supports callbacks (function/field/property) and **enum** types. by using multiple arguments and decorators, you can make logic operation with it.

Arguments:

*   (Optional) `EMode editorMode=EMode.Edit | EMode.Play`

    Condition: if it should be in edit mode or play mode for Editor. By default (omitting this parameter) it does not check the mode at all.

*   `object by...`

    callbacks or attributes for the condition.

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

It also supports value comparison like `==`, `>`, `<=`. Read more in the "Value Comparison for Show/Hide/Enable/Disable-If" section.


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

It also supports value comparison like `==`, `>`, `<=`. Read more in the "Value Comparison for Show/Hide/Enable/Disable-If" section.

#### `Required` ####

Reminding a given reference type field to be required.

This will check if the field value is a `truly` value, which means:

1.  `ValuedType` like `struct` will always be `truly` because `struct` is not nullable and Unity will fill a default value for it no matter what
2.  It works on reference type and will NOT skip Unity's life-circle null check
3.  You may not want to use it on `int`, `float` (because only `0` is not `truly`) or `bool`, but it's still allowed if you insist

Parameters:

*   `string errorMessage = null` Error message. Default is `{label} is required`
*   AllowMultiple: No

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

#### `ValidateInput` ####

Validate the input of the field when the value changes.

*   `string callback` is the callback function to validate the data.

    **Parameters**:

    1.  If the function accepts no arguments, then no argument will be passed
    2.  If the function accepts required arguments, the first required argument will receive the field's value. If there is another required argument and the field is inside a list/array, the index will be passed.
    3.  If the function only has optional arguments, it will try to pass the field's value and index if possible. Otherwise the default value of the parameter will be passed.

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

1.  Delete an element will first be deleted, then the array will duplicated the last element.
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
*   AllowMultiple: No

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

*   `string groupBy` the grouping key. Use `/` to separate different groups and create sub groups.
*   `ELayout layout=ELayout.Vertical` the layout of the current group. Note this is a `EnumFlag`, means you can mix with options.
*   `bool keepGrouping=false`: See `LayoutStart` below
*   `float marginTop = -1f` add some space before the layout. `-1` for using default spacing.
*   `float marginBottom = -1f` add some space after the layout. `-1` for using default spacing.

Options are:

*   `Vertical`
*   `Horizontal`
*   `Background` draw a background color for the whole group
*   `Title` show the title
*   `TitleOut` make `title` more visible. Add this will by default add `Title`. On `IMGUI` it will draw an separator between title and the rest of the content.
    On `UI Toolkit` it will draw a background color for the title.
*   `Foldout` allow to fold/unfold this group. If you have no `Tab` on, then this will automatically add `Title`
*   `Collapse` Same as `Foldout` but is collapsed by default.
*   `Tab` make this group a tab page separated rather than grouping it
*   `TitleBox` = `Background | Title | TitleOut`
*   `FoldoutBox` = `Background | Title | TitleOut | Foldout`
*   `CollapseBox` = `Background | Title | TitleOut | Collapse`

**Known Issue**

`Horizental` style is buggy, for the following reasons:

1.  On IMGUI, `HorizontalScope` does **NOT** shrink when there are many items, and will go off-view without a scrollbar. Both `Odin` and `Markup-Attributes` have the same issue. However, `Markup-Attribute` uses `labelWidth` to make the situation a bit better, which `SaintsEditor` does not provide (at this point at least).
2.  On UI Toolkit we have the well-behaved layout system, but because Unity will try to align the first label, all the field except the first one will get the super-shrank label width which makes it unreadable.

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




### Miscellaneous ###

#### `Dropdown` ####

A dropdown selector. Supports reference type, sub-menu, separator, and disabled select item.

If you want a searchable dropdown, see `AdvancedDropdown`.

*   `string funcName=null` callback function. Must return a `DropdownList<T>`.
    When using on an `enum`, you can omit this parameter, and the dropdown will use the enum values as the dropdown items.
*   `bool slashAsSub=true` treat `/` as a sub item.

    Note: In `IMGUI`, this just replace `/` to unicode [`\u2215` Division Slash ∕](https://www.compart.com/en/unicode/U+2215), and WILL have a little bit overlap with nearby characters.

*   `EUnique unique=EUnique.None`: When using on a list/array, a duplicated option can be removed if `Enique.Remove`, or disabled if `EUnique.Disable`. No use for non-list/array.
*   AllowMultiple: No

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

    The group indicator uses `ToolbarBreadcrumbs`. Sometimes you can see text get wrapped into lines. This is because Unity's UI Toolkit has some layout issue, that it can not has the same layout even with same elements+style+boundary size.

    This issue is not fixable unless Unity fixes it. This issue might be different on different Unity (UI Toolkit) version.

**Arguments**

*   `string funcName=null` callback function. Must return a `AdvancedDropdownList<T>`.
    When using on an `enum`, you can omit this parameter, and the dropdown will use the enum values as the dropdown items.
*   `EUnique unique=EUnique.None`: When using on a list/array, a duplicated option can be removed if `Enique.Remove`, or disabled if `EUnique.Disable`. No use for non-list/array.
*   AllowMultiple: No

**`AdvancedDropdownList<T>`**

*   `string displayName` item name to display
*   `T value` or `IEnumerable<AdvancedDropdownList<T>> children`: value means it's a value item. Otherwise it's a group of items, which the values are specified by `children`
*   `bool disabled = false` if item is disabled
*   `string icon = null` the icon for the item.

    Note: setting an icon for a parent group will result an weird issue on it's sub page's title and block the items. This is not fixable unless Unity decide to fix it.

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



#### `EnumFlags` ####

A toggle buttons group for enum flags (bit mask). It provides a button to toggle all bits on/off.

This field has compact mode and expanded mode.

For each argument:

*   `bool autoExpand=true`: if the view is not enough to show all buttons in a row, automatically expand to a vertical group.
*   `bool defaultExpanded=false`: if true, the buttons group will be expanded as a vertical group by default.
*   AllowMultiple: No

Known Issue:

1.  IMGUI: If you have a lot of flags and you turn **OFF** `autoExpand`, The buttons **WILL** go off-view.
2.  UI Toolkit: when `autoExpand=true`, `defaultExpanded` will be ignored

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

[EnumFlags] public BitMask myMask;
```

[![video](https://github.com/TylerTemp/SaintsField/assets/6391063/710d3efc-5cba-471b-a0f1-a4319ded86fd)](https://github.com/TylerTemp/SaintsField/assets/6391063/48f4c25b-a4cd-40c6-bb42-913a0dc18daa)

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

[EnumFlags]
public BitMask myMask;
```

![image](https://github.com/user-attachments/assets/556ff203-aa55-44c9-9cc1-6ca2675b995f)

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

IMGUI: To use with `RichLabel`, you need to add 6 spaces ahead as a hack

```csharp
using SaintsField;

[LeftToggle] public bool myToggle;
[LeftToggle, RichLabel("      <color=green><label />")] public bool richToggle;
```

![left_toggle](https://github.com/TylerTemp/SaintsField/assets/6391063/bb3de042-bfd8-4fb7-b8d6-7f0db070a761)


#### `ResourcePath` ####

A tool to pick an resource path (a string) with:
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
*   `params Type[] requiredTypes`: a list of required components or interfaces you want. Only objects with all of the types can be signed.
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



#### `AssetPreview` ####

Show an image preview for prefabs, Sprite, Texture2D, etc. (Internally use `AssetPreview.GetAssetPreview`)

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

    An image to display. This can be a property or a callback, which returns a `Sprite`, `Texture2D`, `SpriteRenderer`, `UI.Image`, `UI.RawImage` or `UI.Button`.

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

## Data Types ##

### `SaintsArray`/`SaintsList` ###

Unity does not allow to serialize two dimensional array or list. `SaintsArray` and `SaintsList` are there to help.

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
    public string EditorPropertyName => nameof(myStrings);
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

### `SaintsInterface<,>` ###

`SaintsInterface` is a simple tool to serialize a `UnityEngine.Object` (usually your script component) with a required interface.

You can access the interface with the `.I` field, and actual object with `.V` field.

It provides a drawer to let you only select the object that implements the interface.

For `SaintsInterface<TObject, TInterface>`:

*   `TObject` a serializable type. Use `Component` (for your `MonoBehavior`), `ScriptableObject` or even `UnityEngine.Object` (for any serializable object) if you only want any limitation. Don't use `GameObject`.
*   `TInterface` the interface type.
*   `.I`: the interface value, which is the instance of `TInterface`
*   `.V`: the actual object value, which is the instance of `TObject`

```csharp
using SaintsField;

public SaintsInterface<Component, IInterface1> myInter1;

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

Special Note:

Though you can inherit `SaintsInterface`, but don't inherit it with 2 steps of generic, for example:

```csharp
// Don't do this!
class AnyObjectInterface<T>: SaintsInterface<UnityEngine.Object, T> {}
class MyInterface: AnyObjectInterface<IInterface1> {}
```

The drawer will fail for `AnyObjectInterface` and `MyInterface` because in Unity's C# runtime, it can not report correctly generic arguments.
For more information, see [the comment of the answer in this stackoverflow](https://stackoverflow.com/questions/78513347/getgenericarguments-recursively-on-inherited-class-type-in-c?noredirect=1#comment138415538_78513347).

This component is inspired by [Serialize Interfaces!](https://assetstore.unity.com/packages/tools/utilities/serialize-interfaces-187505), you may go there and support him!

Compared to [Serialize Interfaces!](https://assetstore.unity.com/packages/tools/utilities/serialize-interfaces-187505), this version has several differences:

*   It supports UI Toolkits too.
*   Many SaintsField attributes can work together with this one, especially these auto getters, validators etc.

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

![addressable_label](https://github.com/TylerTemp/SaintsField/assets/6391063/c0485d73-0f5f-4748-9684-d16f712e00e9)

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
```

![nav_mesh_area](https://github.com/TylerTemp/SaintsField/assets/6391063/41da521c-df9e-45a0-aea6-ff1a139a5ff1)


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

## SaintsEditor ##

`SaintsEditor` is a `UnityEditor.Editor` level component.

Namespace: `SaintsField.Playa`

Compared with `NaughtyAttributes` and `MarkupAttributes`:

1.  `NaughtyAttributes` has `Button`, and has a way to show a non-field property(`ShowNonSerializedField`, `ShowNativeProperty`), but it does not retain the order of these fields, but only draw them at the end. It has layout functions (`Foldout`, `BoxGroup`) but it has not `Tab` layout, and much less powerful compared to `MarkupAttributes`. It's IMGUI only.
2.  `MarkupAttributes` is super powerful in layout, but it does not have a way to show a non-field property. It's IMGUI only. It also supports shader editor.
3.  `SaintsEditor`

    *   `Layout` like markup attributes. Compared to `MarkupAttributes`, it allows a non-field property (e.g. a button or a `ShowInInspector` inside a group) (like `OdinInspector`). it has `LayoutGrooup`/`LayoutEnd` for convenience coding.
    *   It provides `Button` (with less functions) and a way to show a non-field property (`ShowInInspector`).
    *   It tries to retain the order, and allows you to use `[Ordered]` when it can not get the order (c# does not allow to obtain all the orders).
    *   Supports both `UI Toolkit` and `IMGUI`.

Please note, any `Editor` level component can not work together with each other (it will not cause trouble, but only one will actually work). Which means, `OdinInspector`, `NaughtyAttributes`, `MarkupAttributes`, `SaintsEditor` can not work together.

If you are interested, here is how to use it.

**Setup SaintsEditor**

`Window` - `Saints` - `Enable SaintsEditor`. After the project finish re-compile, go `Window` - `Saints` - `SaintsEditor` to tweak configs.

If you want to do it manually, check [ApplySaintsEditor.cs](https://github.com/TylerTemp/SaintsField/blob/master/Editor/Playa/ApplySaintsEditor.cs) for more information

## Misc ##

### About GroupBy ##

group with any decorator that has the same `groupBy` for this field. The same group will share even the width of the view width between them.

This only works for decorator draws above or below the field. The above drawer will not groupd with the below drawer, and vice versa.

`""` means no group.

### Value Comparison for Show/Hide/Enable/Disable-If ##


This applies to `ShowIf`, `HideIf`, `EnableIf`, `DisableIf`, `PlayaShowIf`, `PlayaHideIf`, `PlayaEnableIf`, `PlayaDisableIf`.

These decorators accept many objects.

**By Default**

Passing many strings, each string is represent either a field, property or a method. SaintsField will check the value accordingly. If it's a method, it'll also receive the field's value and index if it's inside an array/list.

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
    You can continuously chaining the calling like: `@{GetComponents(MyComponent)[-1].MyFunction().someField['key']}`.

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

    NOTE: In many cases `Odin` does not fallback to the rest drawers, but only to `Odin` and Unity's default drawers. So sometimes things will not work with `Odin`

Special Note:

NaughtyAttributes uses only IMGUI. If you're using Unity 2022.2+, `NaughtyAttributes`'s editor will try fallback default drawers and Unity will decide to use UI Toolkit rendering `SaintsField` and cause troubles.
Please disable `SaintsField`'s UI Toolkit ability by `Window` - `Saints` - `Disable UI Toolkit Support` (See "Add a Macro" section for more information)

My (not full) test about compatibility:

*   [Markup-Attributes](https://github.com/gasgiant/Markup-Attributes): Works very well.
*   [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes): Works well, need that `Label` hack.
*   [OdinInspector](https://odininspector.com/): Works mostly well for MonoBehavior/ScriptableObject. Not so good when it's inside Odin's own serializer.
