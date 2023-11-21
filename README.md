# SaintsField #

[![unity_version](https://github.com/TylerTemp/SaintsField/assets/6391063/4f48380e-900c-4298-8719-c443c355890c)](https://unity.com/download)
[![license_mit](https://github.com/TylerTemp/SaintsField/assets/6391063/a093811a-5dbc-46ad-939e-a9e207ae5bfb)](https://github.com/TylerTemp/SaintsField/blob/master/LICENSE)
[![openupm](https://img.shields.io/npm/v/today.comes.saintsfield?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/today.comes.saintsfield/)
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Ftoday.comes.saintsfield)](https://openupm.com/packages/today.comes.saintsfield/)

`SaintsField` is a Unity Inspector extension tool focusing on script fields like [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes) but different.

Developed by: [TylerTemp](https://github.com/TylerTemp), [墨瞳](https://github.com/xc13308)

Unity: 2020.2 or higher

## Highlights ##

1.  Use and only use `PropertyDrawer` and `DecoratorDrawer`, thus it will be compatible with most Unity Inspector enhancements like `Odin` & `NaughtyAttributes`.
2.  Allow stack on many cases. Only attributes that modified the label itself, and the field itself can not be stacked. All other attributes can mostly be stacked.
3.  Allow dynamic arguments in many cases

## Installation ##

*   Using [OpenUPM](https://openupm.com/)

    ```bash
    openupm add today.comes.saintsfield
    ```

*   Using git upm:

    add this line to `Packages/manifest.json` in your project

    ```javascript
    {
        // your other dependencies...
        "today.comes.saintsfield": "https://github.com/TylerTemp/SaintsField.git"
    }
    ```

*   Using a `unitypackage`:

    Go to the [Release Page](https://github.com/TylerTemp/SaintsField/releases) to download a desired version of `unitypackage` and import it to your project

*   Using a git submodule:

    ```bash
    git submodule add https://github.com/TylerTemp/SaintsField.git Assets/SaintsField
    ```

If you're using unitypackage or git submodule but you put this project under another folder rather than `Assets/SaintsField`, please also do the following:

*   Create `Assets/Editor Default Resources/SaintsField`.
*   Copy only image files (no `.meta` files) from project's `Editor/Editor Default Resources/SaintsField` into your project's `Assets/Editor Default Resources/SaintsField`.
*   Select all the image files you copied, and enable the `Advanced - Read/Write` option for these icons.

## Change Log ##

**1.0.5**

1.  Add `RateAttribute` for rating stars.
2.  Fix `ValidateInputAttribute` won't call the validator the first time it renders.

## Enhancements ##

### Label & Text ###

#### `RichLabel` ####

*   `string|null richTextXml` the content of the label, supported tag:

    *   All Unity rich label tag, like `<color=#ff0000>red</color>`
    *   `<label />` for current field name
    *   `<icon=path/to/image.png />` for icon

    `null` means no label

    for `icon` it will search the following path:

    *   `"Assets/Editor Default Resources/"`  (You can override things here, or put your own icons)
    *   `"Assets/Editor Default Resources/SaintsField/"`  (again for override)
    *   `"Assets/SaintsField/Editor/Editor Default Resources/SaintsField/"` (this is most likely to be when installed using `unitypackage`)
    *   `"Packages/today.comes.saintsfield/Editor/Editor Default Resources/SaintsField/"` (this is most likely to be when installed using `upm`)

    for `color` it supports:

    *   `Clear`, `White`, `Black`, `Gray`, `Red`, `Pink`, `Orange`, `Yellow`, `Green`, `Blue`, `Indigo`, `Violet`
    *   html color which is supported by [`ColorUtility.TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility.TryParseHtmlString.html), like `#RRGGBB`, `#RRGGBBAA`, `#RGB`, `#RGBA`

*   `bool isCallback=false`

    if true, the `richTextXml` will be interpreted as a property/callback function, and the string value / the returned string value (tag supported) will be used as the label content

*   AllowMultiple: No. A field can only have one `RichLabel`

```csharp
public class RichLabel: MonoBehaviour
{
    [RichLabel("<color=indigo><icon=eye.png /></color><b><color=red>R</color><color=green>a</color><color=blue>i</color><color=yellow>i</color><color=cyan>n</color><color=magenta>b</color><color=pink>o</color><color=orange>w</color></b>: <color=violet><label /></color>")]
    public string _rainbow;

    [RichLabel(nameof(LabelCallback), true)]
    public bool _callbackToggle;
    private string LabelCallback() => _callbackToggle ? "<color=green><icon=eye.png /></color> <label/>" : "<icon=eye-slash.png /> <label/>";

    [Space]
    [RichLabel(nameof(_propertyLabel), true)]
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
}
```

[![richlabel](https://github.com/TylerTemp/SaintsField/assets/6391063/5e865350-6eeb-4f2a-8305-c7d8b8720eac)](https://github.com/TylerTemp/SaintsField/assets/6391063/25f6c7cc-ee7e-444e-b078-007dd6df499e)


#### `AboveRichLabel` / `BelowRichLabel` ####

Like `RichLabel`, but it's rendered above/below the field in full width of view instead.


*   `string|null richTextXml` Same as `RichLabel`
*   `bool isCallback=false` Same as `RichLabel`
*   `string groupBy = ""` See `GroupBy` section
*   AllowMultiple: Yes

```csharp
public class FullWidthRichLabelExample: MonoBehaviour
{
    [SerializeField]
    [AboveRichLabel("┌<icon=eye.png/><label />┐")]
    [RichLabel("├<icon=eye.png/><label />┤")]
    [BelowRichLabel(nameof(BelowLabel), true)]
    [BelowRichLabel("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~", groupBy: "example")]
    [BelowRichLabel("==================================", groupBy: "example")]
    private int _intValue;

    private string BelowLabel() => "└<icon=eye.png/><label />┘";
}
```

![full_width_label](https://github.com/TylerTemp/SaintsField/assets/6391063/9283e25a-34b3-4192-8a07-5d97a4e55406)

#### `InfoBox` ####

Draw an info box above/below the field.

*   `string content`

    The content of the info box

*   `EMessageType messageType=EMessageType.Info`

    Message icon. Options are

    *   `None`
    *   `Info`
    *   `Warning`
    *   `Error`

*   `string show=null`

    a callback name or property name for show or hide this info box.

*   `bool contentIsCallback=false`

    if true, the `content` will be interpreted as a property/callback function.

    If the value (or returned value) is a string, then the content will be changed

    If the value is `(string content, EMessageType messageType)` then both content and message type will be changed

*   `bool above=false`

    Draw the info box above the field instead of below

*   `string groupBy=""` See `GroupBy` section

*   AllowMultiple: Yes

```csharp
public class InfoBoxExample : MonoBehaviour
{
    [field: SerializeField] private bool _show;

    [Space]
    [InfoBox("Hi\nwrap long line content content content content content content content content content content content content content content content content content content content content content content content content content", EMessageType.None, above: true)]
    [InfoBox(nameof(DynamicMessage), EMessageType.Warning, contentIsCallback: true, above: true)]
    [InfoBox(nameof(DynamicMessageWithIcon), contentIsCallback: true)]
    [InfoBox("Hi\n toggle content ", EMessageType.Info, nameof(_show))]
    public bool _content;

    private (EMessageType, string) DynamicMessageWithIcon => _content ? (EMessageType.Error, "False!") : (EMessageType.None, "True!");
    private string DynamicMessage() => _content ? "False" : "True";
}
```

[![infobox](https://github.com/TylerTemp/SaintsField/assets/6391063/c96b4b14-594d-4bfd-9cc4-e53390ed99be)](https://github.com/TylerTemp/SaintsField/assets/6391063/03ac649a-9e89-407d-a59d-3e224a7f84c8)


#### `SepTitle` ####

A separator with text

*   `string title=null` title, `null` for no title at all. Does **NOT** support rich text
*   `EColor color`, color for title and line separator
*   `float gap = 2f`, space between title and line separator
*   `float height = 2f`, height of this decorator

```csharp
public class SepTitleExample: MonoBehaviour
{
    [SepTitle("Separate Here", EColor.Pink)]
    public string content1;

    [SepTitle(EColor.Green)]
    public string content2;
}
```

![sep_title](https://github.com/TylerTemp/SaintsField/assets/6391063/55e08b48-4463-4be3-8f87-7afd5ce9e451)

### General Buttons ###

There are 3 general buttons:

*   `AboveButton` will draw a button on above
*   `BelowButton` will draw a button on below
*   `PostFieldButton` will draw a button at the end of the field

All of them have the same arguments:

*   `string funcName`

    called when you click the button

*   `string buttonLabel`

    label of the button, support tags like `RichLabel`

*   `bool buttonLabelIsCallback = false`

    a callback or property name for button's label, same as `RichLabel`

*   `string groupBy = ""`

    See `GroupBy` section. Does **NOT** work on `PostFieldButton`

*   AllowMultiple: Yes

```csharp
public class ButtonsExample : MonoBehaviour
{
    [SerializeField] private bool _errorOut;

    [field: SerializeField] private string _labelByField;

    [AboveButton(nameof(ClickErrorButton), nameof(_labelByField), true)]
    [AboveButton(nameof(ClickErrorButton), "Click <color=green><icon='eye.png' /></color>!")]
    [AboveButton(nameof(ClickButton), nameof(GetButtonLabel), true, "OK")]
    [AboveButton(nameof(ClickButton), nameof(GetButtonLabel), true, "OK")]

    [PostFieldButton(nameof(ToggleAndError), nameof(GetButtonLabelIcon), true)]

    [BelowButton(nameof(ClickButton), nameof(GetButtonLabel), true, "OK")]
    [BelowButton(nameof(ClickButton), nameof(GetButtonLabel), true, "OK")]
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

    private void ClickButton()
    {
        Debug.Log("CLICKED 2!");
        if(_errorOut)
        {
            throw new Exception("Expected exception!");
        }
    }

    private void ToggleAndError()
    {
        Toggle();
        ClickButton();
    }

    private void Toggle() => _errorOut = !_errorOut;
}
```

[![button](https://github.com/TylerTemp/SaintsField/assets/6391063/4e02498e-ae90-4b11-8076-e26256ea0369)](https://github.com/TylerTemp/SaintsField/assets/6391063/f225115b-f7de-4273-be49-d830766e82e7)


### Field Modifier ###

#### `GameObjectActive` ####

A toggle button to toggle the `GameObject.activeSelf` of the field.

This does not require the field to be `GameObject`. It can be a component which already attached to a `GameObject`.

*   AllowMultiple: No

```csharp
public class GameObjectActiveExample : MonoBehaviour
{
    [GameObjectActive] public GameObject _go;
    [GameObjectActive] public GameObjectActiveExample _component;
}
```

[![gameobjectactive](https://github.com/TylerTemp/SaintsField/assets/6391063/19944339-4bfa-4b10-b1fb-b50e0b0433e0)](https://github.com/TylerTemp/SaintsField/assets/6391063/ddb0bd02-8869-47b9-aac4-880ab9bfb81a)

#### `SpriteToggle` ####

A toggle button to toggle the `Sprite` of the target.

The field itself must be `Sprite`.

*   `string imageOrSpriteRenderer`

    the target, must be either `UI.Image` or `SpriteRenderer`

*   AllowMultiple: Yes

```csharp
    public class SpriteToggleExample : MonoBehaviour
    {
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
    }
```

[![spritetoggle](https://github.com/TylerTemp/SaintsField/assets/6391063/70ae0697-d13b-460d-be8b-30d4f823659b)](https://github.com/TylerTemp/SaintsField/assets/6391063/705498e9-4d70-482f-9ae6-b231cd9497ca)

#### `MaterialToggle` ####

A toggle button to toggle the `Material` of the target.

The field itself must be `Material`.

*   `string rendererName=null`

    the target, must be `Renderer` (or its subClass like `MeshRenderer`). When using null, it will try to get the `Renderer` component from the current component

*   `int index=0`

    which slot index of `materials` on `Renderer` you want to swap

*   AllowMultiple: Yes

```csharp
public class MaterialToggleExample: MonoBehaviour
{
    public Renderer targetRenderer;
    [MaterialToggle(nameof(targetRenderer))] public Material _mat1;
    [MaterialToggle(nameof(targetRenderer))] public Material _mat2;
}
```

[![mattoggle](https://github.com/TylerTemp/SaintsField/assets/6391063/cd949e21-e07e-4ee7-8239-280d5d7b8ce1)](https://github.com/TylerTemp/SaintsField/assets/6391063/00c5702c-a41e-42a4-abb1-97a0713c3f66)

#### `ColorToggle` ####

A toggle button to toggle color for `Image`, `Button`, `SpriteRenderer` or `Renderer`

The field itself must be `Material`.

*   `string compName=null`

    the target, must be `Image`, `Button`, `SpriteRenderer`, or `Renderer` (or its subClass like `MeshRenderer`).

    When using `null`, it will try to get the correct component from the target object of this field by order.

    When it's a `Renderer`, it will change the material's `.color` property.

    When it's a `Button`, it will change the button's `targetGraphic.color` property.

*   `int index=0`

    (only works for `Renderer` type) which slot index of `materials` on `Renderer` you want to apply the color

*   AllowMultiple: Yes

```csharp
public class ColorToggleImage: MonoBehaviour
{
    // auto find on the target object
    [SerializeField, ColorToggle] private Color _onColor;
    [SerializeField, ColorToggle] private Color _offColor;

    [Space]
    // by name
    [SerializeField] private Image _image;
    [SerializeField, ColorToggle(nameof(_image))] private Color _onColor2;
    [SerializeField, ColorToggle(nameof(_image))] private Color _offColor2;
}
```

[![color_toggle](https://github.com/TylerTemp/SaintsField/assets/6391063/ce592999-0912-4c94-85dd-a8e428b1c321)](https://github.com/TylerTemp/SaintsField/assets/6391063/236eea74-a902-4f40-b0b9-ab3f2b7c1dbe)

#### `Expandable` ####

Make scriptable objects expandable.

*   AllowMultiple: No

```csharp
public class ExpandableExample : MonoBehaviour
{
    [Expandable] public ScriptableObject _scriptable;
}
```

![expandable](https://github.com/TylerTemp/SaintsField/assets/6391063/92fd1f45-82c5-4d5e-bbc4-c9a70fefe158)

### Field Re-Draw ###

This will change the look & behavior of a field.

#### Rate ####

A rating stars tool for an `int` field.

Parameters:

*   `int min` minimum value of the rating. Must be equal to or greater than 0.

    When it's equal to 0, it'll draw a red slashed star to select `0`.

    When it's greater than 0, it will draw `min` number of fixed stars that you can not un-rate.

*   `int max` maximum value of the rating. Must be greater than `min`.

*   AllowMultiple: No

```csharp
public class RateExample: MonoBehaviour
{
    [Rate(0, 5)] public int rate0To5;
    [Rate(1, 5)] public int rate1To5;
    [Rate(3, 5)] public int rate3To5;
}
```

[![stars]([https://github.com/TylerTemp/SaintsField/assets/6391063/55658254-94f4-4d39-9270-e22dd095b3aa](https://github.com/TylerTemp/SaintsField/assets/6391063/c3a0509e-b211-4a62-92c8-7bc8c3866cf4))](https://github.com/TylerTemp/SaintsField/assets/6391063/a506681f-92f8-42ab-b08d-483b26b2f7c3)

#### `FieldType` ####

Ask the inspector to display another type of field rather than the field's original type.

This is useful when you want to have a `GameObject` prefab, but you want this target prefab to have a specific component (e.g. your own `MonoScript`, or a `ParticalSystem`). By using this you force the inspector to sign the required object that has your expected component but still gives you the original typed value to field.

*   AllowMultiple: No

```csharp
public class FieldTypeExample: MonoBehaviour
{
    [SerializeField, FieldType(typeof(SpriteRenderer))]
    private GameObject _go;

    [SerializeField, FieldType(typeof(FieldTypeExample))]
    private ParticleSystem _ps;
}
```

![field_type](https://github.com/TylerTemp/SaintsField/assets/6391063/7bcc058f-5cb4-4a4f-9d8e-ec08bcb8da2c)

#### `Dropdown` ####

A dropdown selector. Supports reference type, sub-menu, separator, and disabled select item.

*   AllowMultiple: No

```csharp
public class DropdownExample : MonoBehaviour
{
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
}
```

![dropdown](https://github.com/TylerTemp/SaintsField/assets/6391063/aa0da4aa-dfe1-4c41-8d70-e49cc674bd42)

To control the separator and disabled item

```csharp
    [Dropdown(nameof(GetAdvancedDropdownItems))]
    public Color color;

    private DropdownList<Color> GetAdvancedDropdownItems()
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
dropdownList.Add("Black", Color.black);  # add an item
dropdownList.Add("White", Color.white, true);  # and a disabled item
dropdownList.AddSeparator();  # add a separator
```

![color](https://github.com/TylerTemp/SaintsField/assets/6391063/d7f8c9c1-ba43-4c2d-b53c-f6b0788202e6)

#### `MinMaxSlider` ####

A range slider for `Vector2` or `Vector2Int`

For each argument:

*   `int|float min` or `string minCallback`: the minimum value of the slider, or a property/callback name.
*   `int|float max` or `string maxCallback`: the maximum value of the slider, or a property/callback name.
*   `int|float step=1|-1f`: the step of the slider, `<= 0` means no limit. By default, int type use `1` and float type use `-1f`
*   `float minWidth`: the minimum width of the value label. -1 for auto size (not recommended)
*   `float maxWidth`: the maximum width of the value label. -1 for auto size (not recommended)

*   AllowMultiple: No

a full-featured example:

```csharp
public class MinMaxSliderExample: MonoBehaviour
{
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
}
```

[![minmaxslider](https://github.com/TylerTemp/SaintsField/assets/6391063/3da0ea31-d830-4ac6-ab1d-8305764162f5)](https://github.com/TylerTemp/SaintsField/assets/6391063/2ffb659f-a5ed-4861-b1ba-65771db5ab47)

#### `ResizableTextArea` ####

This `TextArea` will always grow its height to fit the content. (minimal height is 3 rows).

Note: Unlike NaughtyAttributes, this does not have a text-wrap issue.

*   AllowMultiple: No

```csharp
public class ResizableTextAreaExample : MonoBehaviour
{
    [SerializeField, ResizableTextArea] private string _short;
    [SerializeField, ResizableTextArea] private string _long;
    [SerializeField, RichLabel(null), ResizableTextArea] private string _noLabel;
}
```

[![resizabletextarea](https://github.com/TylerTemp/SaintsField/assets/6391063/202a742a-965c-4e68-a829-4a8aa4c8fe9e)](https://github.com/TylerTemp/SaintsField/assets/6391063/64ad9c16-19e2-482d-9186-60d42fb34922)

#### `AnimatorParam` ###

A dropdown selector for an animator parameter.

*   `string animatorName`

    name of the animator

*   (Optional) `AnimatorControllerParameterType animatorParamType`

    type of the parameter to filter

```csharp
public class Anim : MonoBehaviour
{
    [field: SerializeField]
    public Animator Animator { get; private set;}

    [AnimatorParam(nameof(Animator))]
    private string animParamName;

    [AnimatorParam(nameof(Animator))]
    private int animParamHash;
}
```

![animator_params](https://github.com/TylerTemp/SaintsField/assets/6391063/3cd5fb6d-1a75-457c-9bbd-e1e6b377a83c)

#### `AnimatorState` ###

A dropdown selector for animator state.

*   `string animatorName`

    name of the animator

to get more useful info from the state, you can use `AnimatorState` type instead of `string` type.

`AnimatorState` has the following properties:

*   `int layerIndex` index of layer
*   `int stateNameHash` hash value of state
*   `string stateName` actual state name
*   `float stateSpeed` the `Speed` parameter of the state
*   `AnimationClip animationClip` is the actual animation clip of the state (can be null). It has a `length` value for the length of the clip. For more detail see [Unity Doc of AnimationClip](https://docs.unity3d.com/ScriptReference/AnimationClip.html)


```csharp
public class Anim : MonoBehaviour
{
    [field: SerializeField]
    public Animator Animator { get; private set; }

    [AnimatorState(nameof(Animator))]
    public AnimatorState animatorState;

    [AnimatorState(nameof(Animator))]
    public string animStateName;
}
```

![animator_state](https://github.com/TylerTemp/SaintsField/assets/6391063/f3e63d15-57b6-4e48-a769-f3743df22dbb)

#### `Layer` ####

A dropdown selector for layer.

*   AllowMultiple: No

```csharp
public class LayerAttributeExample: MonoBehaviour
{
    [Layer] public string layerString;
    [Layer] public int layerInt;
}
```

![layer](https://github.com/TylerTemp/SaintsField/assets/6391063/17fdc955-d637-4750-9856-3d5012fa0cb2)

#### `Scene` ####


A dropdown selector for a scene in the build list.

*   AllowMultiple: No

```csharp
public class SceneExample: MonoBehaviour
{
    [Scene] public int _sceneInt;
    [Scene] public string _sceneString;
}
```

![scene](https://github.com/TylerTemp/SaintsField/assets/6391063/94ae457e-44e2-4434-ab68-d8d51df1e2fa)


#### `SortingLayer` ####

A dropdown selector for sorting layer.

*   AllowMultiple: No

```csharp
public class SortingLayerExample: MonoBehaviour
{
    [SortingLayer] public string _sortingLayerString;
    [SortingLayer] public int _sortingLayerInt;
}
```

![sorting_layer](https://github.com/TylerTemp/SaintsField/assets/6391063/cab99c5c-0ec2-49c5-843c-89f65f402b93)

#### `Tag` ####

A dropdown selector for a tag.

*   AllowMultiple: No

```csharp
public class TagExample: MonoBehaviour
{
    [Tag] public string _tag;
}
```

![tag](https://github.com/TylerTemp/SaintsField/assets/6391063/df1526a5-75fe-4fc9-ba47-19e7f2d63dda)

### Field Utilities ###

#### `AssetPreview` ####

Show an image preview for prefabs, Sprite, Texture2D, etc. (Internally use `AssetPreview.GetAssetPreview`)

*   `int maxWidth=-1`

    preview max-width, -1 for current view width

*   `int maxHeight=-1`

    preview max height, -1 for auto resize (with the same aspect) using the width

*   `bool above=false`

    if true, render above the field instead of below

*   `string groupBy=""`

    See the `GroupBy` section

*   AllowMultiple: No

```csharp
public class AssetPreviewExample: MonoBehaviour
{
    [AssetPreview(20, 100)] public Texture2D _texture2D;
    [AssetPreview(50)] public GameObject _go;
    [AssetPreview(above: true)] public Sprite _sprite;
}
```

![asset_preview](https://github.com/TylerTemp/SaintsField/assets/6391063/ffed3715-f531-43d0-b4c3-98d20d419b3e)

#### `OnValueChanged` ####

Call a function every time the field value is changed

*   `string callback` the callback function name
*   AllowMultiple: Yes

```csharp
public class OnChangedExample : MonoBehaviour
{
    [OnValueChanged(nameof(Changed))]
    public int _value;

    private void Changed()
    {
        Debug.Log($"changed={_value}");
    }
}
```

#### `ReadOnly` ####

This has two overrides:

*   `ReadOnlyAttribute(bool directValue)`
*   `ReadOnlyAttribute(params string[] by)`

Each arguments:

*   `bool directValue=false`

    if true, the field will be read-only

*   `string[] by`

    a callback or property name, if **ALL** the value is truly, the field will be read-only

*   AllowMultiple: Yes

    When using multiple `ReadOnly` on a field, the field will be read only if **ANY** of them is read-only

```csharp
public class ReadOnlyGroupExample: MonoBehaviour
{
    [ReadOnly(true)] public string directlyReadOnly;

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
}
```
[![readonly](https://github.com/TylerTemp/SaintsField/assets/6391063/e267567b-469c-4156-a82c-82f21fc43021)](https://github.com/TylerTemp/SaintsField/assets/6391063/6761a0f2-07c2-4252-9dd0-c1a60091a891)

#### `Required` ####

Remide a given reference type field to be required.

This will check if the field value is a `truly` value, which means:

1.  Won't work for int and float (It'll give an error, asking you to not use on int/float)
2. The `struct` value will always be `truly` because `struct` is not nullable and Unity will fill a default value for it no matter what
3.  It works on reference type and will NOT skip Unity's life-circle null check

*   `string errorMessage = null` Error message. Default is `{label} is required`
*   AllowMultiple: No

![required](https://github.com/TylerTemp/SaintsField/assets/6391063/1e93c51c-ada6-4848-8fc8-a963266ee2cf)


```csharp
public class RequiredExample: MonoBehaviour
{
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
    public MyStruct wontWorkWontNoticeYou;
}
```

#### `ValidateInput` ####

Validate the input of the field when the value changes.

*   `string callback` is the callback function to validate the data. note: return type is **`string`** not bool! return a null or empty string for valid, otherwise, the string will be used as the error message

*   AllowMultiple: Yes

```csharp
public class ValidateInputExample : MonoBehaviour
{
    [ValidateInput(nameof(OnValidateInput))]
    public int _value;

    private string OnValidateInput() => _value < 0 ? $"Should be positive, but gets {_value}" : null;
}
```

[![validateinput](https://github.com/TylerTemp/SaintsField/assets/6391063/f554084c-78f3-43ca-a6ab-b3f59ecbf44c)](https://github.com/TylerTemp/SaintsField/assets/6391063/9d52e663-c9f8-430a-814c-011b17b67a86)

#### `ShowIf` / `HideIf` ####

Show or hide the field based on a condition.

For `ShowIf`:

*   `string andCallbacks...` a list of callback or property names, if **ALL** the value is truly, the field will be shown/hidden

*   AllowMultiple: Yes

    When using multiple `ShowIf` on a field, the field will be shown if **ANY** of them is shown

`HideIf` is the opposite of `ShowIf`. You can use multiple `ShowIf`, `HideIf`, and even a mix of the two

A full featured example:


```csharp
public class ShowHideExample: MonoBehaviour
{
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
    [RichLabel("<color=yellow>show=!1&&!2")]
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
    [RichLabel("<color=pink>show=(!1&&!2)||(!3&&!4)")]
    public string _hideIf1234;
}
```

[![showifhideif](https://github.com/TylerTemp/SaintsField/assets/6391063/1625472e-5769-4c16-81a3-637511437e1d)](https://github.com/TylerTemp/SaintsField/assets/6391063/dc7f8b78-de4c-4b12-a383-005be04c10c0)

#### `MinValue` / `MaxValue` ####

Limit for int/float field

They have the same overrides:

*   `float value`: directly limit to a number value
*   `string valueCallback`: a callback or property for limit

*   AllowMultiple: Yes

```csharp
public class MinMaxExample: MonoBehaviour
{
    public int upLimit;

    [MinValue(0), MaxValue(nameof(upLimit))] public int min0Max;
    [MinValue(nameof(upLimit)), MaxValue(10)] public float fMinMax10;
}
```

[![minmax](https://github.com/TylerTemp/SaintsField/assets/6391063/7714fa76-fc5c-4ebc-9aae-be189cef7743)](https://github.com/TylerTemp/SaintsField/assets/6391063/ea2efa8d-86e6-46ba-bd7d-23e7577f7604)

## GroupBy ##

group with any decorator that has the same `groupBy` for this field. The same group will share even the width of the view width between them.

This only works for decorator draws above or below the field. The above drawer will not grouped with the below drawer, and vice versa.

`""` means no group.


## Common Pitfalls & Compatibility ##

**List/Array**

1.  Directly using on list/array will not work
2.  Using on list/array's element works

Unlike NaughtyAttributes, `SaintsField` does not need a `Nested` attribute to work on list/array's element.

```csharp
public class ArrayLabelExample : MonoBehaviour
{
    // this won't work
    [RichLabel("HI"), InfoBox("this actually wont work", EMessageType.Warning)] public int[] _ints;

    [Serializable]
    public struct MyStruct
    {
        // this works
        [RichLabel("HI"), MinMaxSlider(0f, 1f)] public Vector2 minMax;
        public float normalFloat;
    }

    public MyStruct[] myStructs;
}
```

**Order Matters**

`SaintsField` only uses `PropertyDrawer` to draw the field, and will properly fall back to the rest drawers if there is one.
This works for both 3rd party drawer, your custom drawer, and Unity's default drawer.

However, Unity only allows decorators to be loaded from top to bottom, left to right. Any drawer that does not properly handle the fallback
will override `PropertyDrawer` follows by. Thus, ensure `SaintsField` is always the first decorator.

An example of working with NaughtyAttributes:

```csharp
public class CompatibilityNaAndDefault : MonoBehaviour
{
    [RichLabel("<color=green>+NA</color>"), NaughtyAttributes.CurveRange(0, 0, 1, 1, NaughtyAttributes.EColor.Green)]
    public AnimationCurve naCurve;

    [RichLabel("<color=green>+Native</color>"), Range(0, 5)]
    public float nativeRange;

    // this wont work. Please put `SaintsField` before other drawers
    [Range(0, 5), RichLabel("<color=green>+Native</color>")]
    public float nativeRangeHandled;

    // this wont work too.
    [NaughtyAttributes.CurveRange(0, 0, 1, 1, NaughtyAttributes.EColor.Green), RichLabel("<color=green>+NA</color>")]
    public AnimationCurve naCurveHandled;
}
```

**Multiple Fields Handling**

Unlike `NaghthyAttributes`/`Odin`, `SaintsField` does not have a decorator like `Tag`, or `GroupBox` that puts several fields into one place
because it does not inject a global `CustomEditor`.

For the same reason, it can not handle `NonSerializedField` and `AutoPropertyField`. Because they are all not `PropertyAttribute`.

**Other Drawers**

`SaintsField` is designed to be compatible with other drawers if

1.  the drawer itself respects the `GUIContent` argument in `OnGUI`

    NOTE: `NaughtyAttributes` uses `property.displayName` instead of `GUIContent`. `SaintsField` deals with it by drawing a background rect
    before drawing the label, so it's fine

2.  if the drawer hijacks the `CustomEditor`, it must fall to the rest drawers

    NOTE: In many cases `Odin` does not fallback to the rest drawers, but only to `Odin` and Unity's default drawers. So sometimes things will not work with `Odin`
