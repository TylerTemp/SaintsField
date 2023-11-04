# SaintsField #

`SaintsField` is a Unity Inspector extension tools focusing on script fields like [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes) with more powerful features.

Developed by: TylerTemp, 墨瞳

## Highlights ##

1.  Use and only use `PropertyDrawer` and `DecoratorDrawer`, thus it will be compatible with most Unity Inspector enhancer like `Odin` & `NaughtyAttributes`.
2.  Allow stack on many cases
3.  Allow dynamic arguments on many cases

## Enhancements ##

All fields enhancement can generally be divided into:

1.  Label Decorator
2.  Field Decorator
3.  Above Decorator
4.  Below Decorator

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
    *   `"Assets/SaintsField/Editor/Editor Default Resources/SaintsField/"` (this is most likely to be when install using `unitypackage`)
    *   `"Packages/today.comes.saintsfield/Editor/Editor Default Resources/SaintsField/"` (this is most likely to be when install using `upm`)

    for `color` it supports:

    *   `Clear`, `White`, `Black`, `Gray`, `Red`, `Pink`, `Orange`, `Yellow`, `Green`, `Blue`, `Indigo`, `Violet`
    *   html color which supported by `ColorUtility.TryParseHtmlString`, like `#RRGGBB`, `#RRGGBBAA`, `#RGB`, `#RGBA`

*   `bool isCallback=false`

    if true, the `richTextXml` will be interpreted as a property / callback function, and the string value / the returned string value (tag supported) will be used as the label content

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

    // this is a workaround
    [SerializeField]
    [RichLabel("<color=green>Fix For Struct</color>")]
    [FieldDrawerConfig(FieldDrawerConfigAttribute.FieldDrawType.FullWidthOverlay)]
    private MyStruct _myStructWorkAround;
}
```

https://github.com/TylerTemp/SaintsField/assets/6391063/25f6c7cc-ee7e-444e-b078-007dd6df499e


#### `AboveRichLabel` / `BelowRichLabel` ####

Like `RichLabel` but it's rendered above/below the field in full width of view.


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

![above_below_richlabel](https://github.com/TylerTemp/SaintsField/assets/6391063/c5919a6e-bd52-45b7-bf29-37220564171b)

#### `InfoBox` ####

Draw a info box above/below the field.

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

    if true, the `content` will be interpreted as a property / callback function.

    If the value (or returned value) is string, then the content will be changed

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


https://github.com/TylerTemp/SaintsField/assets/6391063/03ac649a-9e89-407d-a59d-3e224a7f84c8


#### `SepTitle` ####

A separator with a text

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

    a callback or propery name for button label, same as `RichLabel`

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


https://github.com/TylerTemp/SaintsField/assets/6391063/f225115b-f7de-4273-be49-d830766e82e7


### Field Utilities ###

#### `GameObjectActive` ####

A toggle button to toggle the `GameObject.activeSelf` of the field.

This does not require the field is `GameObject`. It can be a component which already attached to a `GameObject`.

*   AllowMultiple: No

```csharp
public class GameObjectActiveExample : MonoBehaviour
{
    [GameObjectActive] public GameObject _go;
    [GameObjectActive] public GameObjectActiveExample _component;
}
```



https://github.com/TylerTemp/SaintsField/assets/6391063/ddb0bd02-8869-47b9-aac4-880ab9bfb81a



#### `SpriteToggle` ####

A toggle button to toggle the `Sprite` of the target.

The field itself must be `Sprite`.

*   `string imageOrSpriteRenderer`

    The target, must be either `UI.Image` or `SpriteRenderer`

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


https://github.com/TylerTemp/SaintsField/assets/6391063/705498e9-4d70-482f-9ae6-b231cd9497ca




#### `MaterialToggle` ####

A toggle button to toggle the `Material` of the target.

The field itself must be `Material`.

*   `string rendererName=null`

    The target, must be `Renderer` (or it's subClass like `MeshRenderer`). when using null, it will try to get the `Renderer` component from the current component

*   `int index=0`

    which slot index of `materials` on `Renderer` you want to swap



https://github.com/TylerTemp/SaintsField/assets/6391063/00c5702c-a41e-42a4-abb1-97a0713c3f66



### Field Enhancement ###

#### `DropDown` ####

A dropdown selector. Supports reference type and sub menu.

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


### GroupBy ###

group with any decorator that has the same `groupBy` for this field. Same group will share even width of the view width between them .

`""` means no group.
