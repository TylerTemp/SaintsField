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
