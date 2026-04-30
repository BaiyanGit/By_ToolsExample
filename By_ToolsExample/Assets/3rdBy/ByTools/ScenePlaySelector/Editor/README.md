# ScenePlaySelector 场景快速切换工具

## 功能简介

`ScenePlaySelector` 是一个 Unity Editor 工具栏扩展，用于在 Unity 顶部 Toolbar 中快速切换场景。

当前版本会把工具挂载到 Unity 顶部工具栏的 **Play / Pause / Step** 按钮右侧，显示为：

```text
Play  Pause  Step  Source: BuildSettings  Scene: Main  ↻  ⚙
```

其中：

- `Source`：选择场景来源。
- `Scene`：选择并打开场景。
- `↻`：刷新场景列表。
- `⚙`：打开场景显示配置窗口。

---

## 文件说明

| 文件 | 作用 |
|---|---|
| `EditorToolbarUtil.cs` | 通过反射获取 Unity Editor Toolbar，并把工具挂载到指定区域。 |
| `ScenePlaySelector.cs` | Toolbar 主逻辑，负责显示 Source / Scene 菜单以及切换场景。 |
| `ScenePlaySelectorStorage.cs` | 配置存储逻辑，负责读写 `ProjectSettings/ScenePlaySelectorConfig.json`。 |
| `SceneSelectorEditorWindow.cs` | 场景显示配置窗口，可以控制哪些场景显示到 Toolbar。 |
| `VisualElementFactory.cs` | 创建 ToolbarMenu / ToolbarButton，并处理鼠标悬停、点击样式。 |

---

## 安装方式

把这 5 个脚本放到 Unity 项目的任意 `Editor` 目录下，例如：

```text
Assets/Editor/ScenePlaySelector/
```

推荐目录结构：

```text
Assets/
  Editor/
    ScenePlaySelector/
      EditorToolbarUtil.cs
      ScenePlaySelector.cs
      ScenePlaySelectorStorage.cs
      SceneSelectorEditorWindow.cs
      VisualElementFactory.cs
```

放入后等待 Unity 编译完成，工具会自动显示在顶部 Toolbar 的 Step 按钮右侧。

---

## 使用方式

### 1. 选择场景来源

点击 Toolbar 中的 `Source` 菜单，可以选择：

| 来源 | 说明 |
|---|---|
| `BuildSettings` | 使用 Unity Build Settings 中的场景。 |
| `ProjectAssets` | 扫描项目中所有 Scene 资源。 |

### 2. 快速切换场景

点击 Toolbar 中的 `Scene` 菜单，选择一个场景后会立即打开该场景。

如果当前场景有未保存修改，Unity 会弹出保存确认窗口。

### 3. 刷新场景列表

点击 `↻` 按钮可以刷新 Toolbar 场景列表。

常见使用场景：

- 新增了场景。
- 删除了场景。
- 修改了 Build Settings 场景列表。
- 修改了场景显示配置。

### 4. 打开配置窗口

点击 `⚙` 按钮，或通过菜单打开：

```text
ByTools / 🖼️ 场景显示配置器
```

配置窗口中可以：

- 切换场景来源。
- 搜索场景。
- 设置某个场景是否显示在 Toolbar 中。
- 全选。
- 全不选。
- 反选。
- 保存并刷新 Toolbar。
- 打开场景。
- 定位场景资源。
- 重置当前来源配置。
- 打开配置目录。

---

## 配置文件位置

配置文件保存在：

```text
ProjectSettings/ScenePlaySelectorConfig.json
```

这个文件属于项目级配置，建议团队协作时可以根据项目需求决定是否加入版本管理。

---

## 显示规则

### BuildSettings 来源

读取：

```csharp
EditorBuildSettings.scenes
```

默认显示 enabled 的场景。

### ProjectAssets 来源

读取：

```csharp
AssetDatabase.FindAssets("t:Scene")
```

默认显示项目内所有 Scene 资源。

---

## 样式说明

当前版本使用：

```csharp
UnityEditor.UIElements.ToolbarMenu
UnityEditor.UIElements.ToolbarButton
```

风格设计为：

- 默认没有背景。
- 鼠标悬停时显示 Unity Toolbar hover 背景。
- 鼠标点击时显示 Unity Toolbar active 背景。
- 文字颜色跟随 Unity ToolbarButton 主题。

这样可以更贴近 Unity 原生 Toolbar 视觉，并避免 `PopupField` 在 Toolbar 中出现白底或文字不可见的问题。

---

## 注意事项

### 1. Toolbar 是 Unity 内部 API

Unity 并没有正式公开 Editor Toolbar 扩展 API，所以本工具通过反射访问：

```text
UnityEditor.Toolbar
m_Root
ToolbarZonePlayMode
```

如果未来 Unity 改变内部 Toolbar 结构，可能需要调整 `EditorToolbarUtil.cs`。

### 2. 播放中不能切换场景

工具会阻止在播放中或即将进入播放模式时切换场景。

### 3. 场景路径发生变化后需要刷新

如果场景被移动或重命名，建议点击 `↻` 或打开配置窗口后保存刷新。

### 4. 同名场景会显示路径辅助区分

如果多个场景名字相同，Scene 菜单中会显示类似：

```text
Main  (Assets/Scenes/Login)
Main  (Assets/Scenes/Battle)
```

### 5. 当前激活场景会有标记

当前场景会显示：

```text
● Main
```

---

## 常见问题

### Q：为什么工具没有显示？

可以尝试：

1. 等 Unity 编译完成。
2. 切换一下 Layout。
3. 点击菜单重新打开配置窗口。
4. 检查脚本是否放在 `Editor` 目录下。
5. 查看 Console 是否有编译错误。

### Q：为什么 Source / Scene 没背景？

这是当前设计：默认无背景，只有鼠标悬停或点击时才显示背景，更贴近 Unity 原生 Toolbar 的轻量风格。

### Q：为什么有些场景不显示？

打开配置窗口检查该场景的“显示”是否勾选，然后点击“保存并刷新”。

### Q：配置文件可以删除吗？

可以。删除后工具会重新生成默认配置。

配置文件路径：

```text
ProjectSettings/ScenePlaySelectorConfig.json
```

也可以在配置窗口中点击“重置当前来源配置”。

---

## 版本说明

当前版本特性：

- 挂载到 Step 按钮右侧。
- 使用 Unity 原生 `ToolbarMenu`。
- 默认透明背景，悬停/点击显示背景。
- 支持 BuildSettings / ProjectAssets 两种来源。
- 支持场景显示配置。
- 支持配置持久化到 ProjectSettings。
- 脚本字段已添加 `Header` 注释。
- 关键类、字段、方法已补充 XML 注释。



---

## Fixed: 补全 RefreshSceneDropdownAfterSourceChanged

上一版调用了：

```csharp
RefreshSceneDropdownAfterSourceChanged(source);
```

但方法本体漏掉了。本版已补全。

同时普通刷新逻辑也调整为：如果当前激活场景和保存的场景都不在当前 Source 的可见列表中，则 Scene 显示 `<未选择场景>`，不再错误地默认选中第一个场景。


---

## Fixed: Scene 下拉菜单勾选状态同步

修复 `Source` 切换后：

```text
Scene 按钮文字已经切到保存的 Scene
但打开 Scene 下拉菜单时，菜单项没有显示 Checked 勾选状态
```

原因是旧逻辑在 `SetChoices()` 中先重建菜单，再设置 `_index`。  
部分 Unity 版本会缓存 `ToolbarMenu` 菜单状态，导致勾选状态没有同步。

现在改成：

```text
SetChoices
    ↓
先更新 _index
    ↓
更新按钮文字
    ↓
再 RebuildMenu
    ↓
Checked 状态使用本次重建时的索引快照
```


---

## 本版 Source 切换准确逻辑

```text
Source 改变
    ↓
刷新 Scene 下拉
    ↓
如果这个 Source 保存过的 Scene 还在显示列表中：
    Scene 按钮显示这个 Scene
    Scene 下拉菜单里对应项显示 Checked / Toggle 勾选状态
    不自动打开这个 Scene

否则：
    Scene 显示为 <未选择场景>
    Scene 列表中不勾选任何场景
    当前 Unity 已打开的场景保持不变
    不自动打开
    ↓
用户手动点 Scene 菜单里的场景时才打开
```

实现点：

- `BuildSceneDropdownDataForSourceChanged()` 只认新 Source 自己保存过的 Scene。
- 保存过的 Scene 不在当前可见列表时，返回 `selectedIndex = -1`。
- `NativeToolbarDropdown` 支持 `index = -1`，用于显示 `<未选择场景>`。
- `ToolbarMenu` 的 Checked 状态直接读取当前 `_index`，确保按钮文字和菜单勾选状态同步。


---

## ActiveSceneDriven 版本逻辑

这版重新整理了 Source / Scene 的状态关系。

核心规则：

```text
Scene 按钮永远优先显示 Unity 当前实际打开的 Scene
Source 切换不自动打开任何 Scene
Scene 菜单是否勾选，只看当前实际打开的 Scene 是否存在于当前 Source 的可见列表中
用户手动点击 Scene 菜单项时，才打开并保存该 Scene
```

具体表现：

```text
Source 改变
    ↓
刷新 Scene 下拉列表
    ↓
检查当前 Unity 实际打开的 Scene 是否存在于新的 Source 显示列表中

    如果存在：
        Scene 按钮显示当前实际打开的 Scene
        Scene 菜单里这个 Scene 显示 Checked / 勾选状态

    如果不存在：
        Scene 按钮仍然显示当前实际打开的 Scene 名称
        Scene 菜单里不勾选任何项

    ↓
不自动打开任何 Scene
    ↓
用户手动点击 Scene 菜单里的某个 Scene 时：
        打开这个 Scene
        保存为当前 Source 的 selectedScenePath
        Scene 菜单勾选这个 Scene
```

这版不再使用 `<未选择场景>` 作为 Source 切换后的显示文本，因为 Unity 实际上始终有当前打开的 Scene。
