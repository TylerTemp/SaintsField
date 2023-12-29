## 1.2.3 ##

1.  Add `FindComponent`
2.  Add `UnsaintlyEditor`

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
