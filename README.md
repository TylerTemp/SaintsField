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

### Label ###

#### `RichLabel` ####

*   `string richTextXml` the content of the label, supported tag:

    *   All Unity rich label tag, like `<color=#ff0000>red</color>`
