# Saints XPath 开发笔记 #

模仿 XPath，只实现部分功能。扩展部分 Unity 特有的

基本: `/step/step/step/...`

step: `axisname::nodetest[predicate]`

`axisname` 其实可以混合 `@` 来选择属性: `name@attribute`。但是不允许混用`::`，如`ancestor::name@attr`。正确写法为 `ancestor::name/@attr`

`nodetest` 本身不带有任何`[0]`, `[last()]` 这些。这些都属于 `predicate` 的范畴

`predicate` 同样 `@` 过滤属性，但是不选择

复杂的语法并不被支持，如 `name[ancestor::note()[@attr=1]]`，类似于前后向断言

支持的 XPath 的:

*   `ancestor::`
*   `ancestor-or-self::`
*   `parent::`
*   `parent-or-self::`
*   其它的不支持

基于 Unity 特有的，加上 `::nodetest` 其实有方向指向性，所以用其指向 Unity 特有资源:

*   `ancestor-inside-prefab::`
*   `ancestor-or-self-inside-prefab::`
*   `parent-inside-prefab::`
*   `parent-or-self-inside-prefab::`
*   `scene::`: 场景根节点
*   `prefab::`: 预制体根节点
*   `resources::`: 资源
*   `asset::`: `AssetDatabase` 资源

大部分的函数均不支持，支持的有

*   `index()`（XPath 并没有这个函数，只有 `position()`，但 `position()` 是以1算的）
*   `last()`

并支持负向索引

函数运算也只支持基本的大小比较，等于，不等于等

**属性**

所有大括号其实都是获取属性值

*   `@layer`: Unity 的 Layer，必须是 string，不能是 mask: `[@layer = UI]`, `[@layer != 'Ignore Raycast']`。层名使用单引号或双引号均可
*   `@{layer}`: Unity 的 Layer，这个是int值
*   `@{tag}`: Unity 的 Tag，可以比较: `[@tag = "Main Camera" or @tag = "Particle Camera"]`（其实是读取对应属性）
*   `@{gameObject}`: 这个是默认行为，获得 `gameObject`。（其实是读取对应属性）
*   `@{transform}`: 获得 `transform`。（其实是读取对应属性）
*   `@{rectTransform}`: 获得 `RectTransform`。（其实是`GetComponents(RectTransform)[0]`的缩写）
*   `@{activeSelf}`/`@{gameObject.activeSelf}`（其实是读取对应属性）
*   `@{enabled}`/`@{GetComponent(MyScript).enabled}`/`@{GetComponents(MyScript).enabled}` （其实是读取对应属性）
*   `@{GetComponents()}`
*   `@resource-path()`
*   `@asset-path()`
*   其它的不支持

**Callback**

与其它相同，使用 `$` 开头表示这个 XPath 本身通过 callback 获取。

而基于 Unity 脚本的 Callback，因为 `@` 其实是属性选择器，因此拓展以下:

*   `[@{GetComponent(MyComponent)}]` 用来测试组件存在。同样，`myNode/@{GetComponent(MyComponent)}` 用来获取组件
*   `myNode@{GetComponents(MyComponent)[-1].MyFunction().someField['key']}`来调用组件函数。其中：
    *    `GetComponents` 支持指定 `NameSpace`，支持指定 `baseClass`，不支持 `generic-class`
    *    函数调用不支持参数，只能是一个括号
    *    `someField` 就是字段/属性获取。公开非公开的都行。`static`不保证
    *    `-1`这类数字是取下标的
    *    `'key'`是取字典的键值，单引号双引号均可。字典只支持 string key，其它的都不支持

**资源路径匹配**

不采用Linux/Git的写法，而是参考XPath本身的路径写法。

比如搜索 `resource` 下的目录: `/*Cards*[0]/*.prefab`，这个是搜索目标目录下直接目录名包含 `Cards` 的第一个目录下的所有直接 `prefab` 文件

同理: `//Card-*//*.asset`，这个是搜索目标目录下任何以 `Card-` 开头的目录下的递归所有 `asset` 文件

然后通过 `@resource-path`, `@asset-path` 来做过滤

比如，搜索某个资源可以直接: `::resources/Cards/Wizard-*/*`

复杂点的例子：过滤某些资源则可以: `myNode/player*/[@resource-path = "Cards/Wizard-*"]`

过滤资源必须是 Resources 下的Prefab 的: `myNode/player*/[@resources]`

**predicate**

必须有空格。属性支持的都支持
