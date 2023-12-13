## 1.1.3 ##

*   `AnimatorParam` no longer offers `null` value
*   `AnimatorParam` and `AnimatorState` now will try to find the `animator` on current object if the name of `animator` is not set
*   Use standard field picker for `layer` and `tag`

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
