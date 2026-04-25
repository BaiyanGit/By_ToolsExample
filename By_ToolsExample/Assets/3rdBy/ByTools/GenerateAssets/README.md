# Asset Generator Editor Tool

Unity 编辑器扩展工具，用于批量生成和管理 `ScriptableObject` 资产文件（.asset）。支持普通手动添加资产计划，也支持通过自定义拆分器将数据列表自动拆分为多个独立资产。

---

## 功能概述

- 拖拽 `ScriptableObject` 脚本到窗口，自动识别并管理。
- 为每个脚本创建多个“资产计划”，每个计划对应一个待生成的 `.asset` 文件。
- 支持手动添加、重命名、编辑草稿、删除资产计划。
- 支持自定义拆分生成器：通过继承 `AssetSplitGenerator<T>` 实现数据驱动的一键生成多资产。
- 支持生成时覆盖同名资产，并可选择保留原有数据。
- 提供通用资产编辑窗口（基于 Inspector），可编辑任意 `UnityEngine.Object`，支持草稿与持久化资产。
- 使用 `SessionState` 持久化窗口数据（脚本列表、输出目录等）。

---

## 使用说明

### 1. 打开工具

菜单栏：**ByTools → 🗂️ Asset资产生成工具**

### 2. 设置输出目录

- 工具顶部显示当前输出路径（默认为工具脚本所在目录的上两级 + `AssetFiles` 文件夹）。
- 点击 **选择** 按钮可自定义输出目录（必须位于 `Assets` 目录下）。
- 点击 **重置** 可恢复默认路径。

### 3. 添加脚本

- 将继承自 `ScriptableObject` 的 `.cs` 脚本文件**拖拽**到窗口的虚线区域。
- 若脚本名称重复，会弹出窗口要求输入新名称。
- 添加后，工具会自动：
  - 尝试调用该脚本类型的自定义预览器（如果已注册），生成资产计划列表。
  - 若无预览器，则自动创建一个默认资产计划（名称与脚本基础名相同）。

### 4. 管理资产计划

每个脚本卡片包含：

- 脚本名称、路径（只读）
- **添加资产📩**：手动新增一个资产计划
- **移除脚本🪠**：删除该脚本及其所有计划
- **一键生成🔨**：生成当前脚本下的所有资产

#### 资产计划项

每个计划显示：

- 状态图标（🟢 已生成 / 🔘 待生成）
- 资产名称（可重命名）
- 操作按钮：
  - **重命名✏️**：修改资产文件名（不含后缀）
  - **编辑📝**：打开编辑器修改资产数据（仅对非预览的普通计划有效，预览计划需先生成后再编辑）
  - **移除🧹**：删除该计划
  - **生成🥏**：单独生成该计划对应的资产

> 预览计划（来自自定义拆分器）的编辑按钮会提示先生成资产，避免覆盖拆分逻辑。

### 5. 生成资产

- **单独生成**：点击某个计划的 **生成🥏** 按钮
- **一键生成**：点击脚本卡片上的 **一键生成🔨**
- **批量生成**：工具栏 **批量生成** 按钮，生成所有脚本的所有计划

生成时如果目标路径已存在同名资产，会弹出选择：

- **覆盖并带入数据**：删除旧资产，但将旧资产的序列化数据（JSON）应用到新资产上。
- **仅覆盖**：直接删除旧资产，生成全新资产（不保留数据）。
- **取消**：跳过该资产。

### 6. 编辑资产数据

#### 编辑待生成资产（草稿）

- 点击普通计划（非预览）的 **编辑📝** 按钮，打开通用资产编辑窗口。
- 修改 Inspector 中的数据，点击 **保存** 会将草稿 JSON 写回资产计划，下次生成时自动填充。
- 临时对象在关闭窗口时可自动回写草稿。

#### 编辑已生成资产

- 在“已生成资产”区域，点击资产旁的 **编辑📝** 按钮，打开编辑窗口。
- 修改后保存会直接写入 `.asset` 文件，并刷新 AssetDatabase。

### 7. 自定义拆分生成器（高级用法）

当需要根据数据源（如配置表、枚举列表）自动生成多个资产时，可实现拆分生成器。

#### 实现步骤

1. 创建一个继承自 `AssetSplitGenerator<T>` 的类，其中 `T` 是目标 `ScriptableObject` 类型。
2. 给类加上 `[InitializeOnLoad]` 特性，并创建一个静态实例（触发基类构造函数注册）。
3. 实现 `protected abstract IEnumerable<object> GetSplitDataList()`，返回数据项列表。
4. 可选重写：
   - `GetAssetName(object dataItem)`：返回文件名（不含后缀）。
   - `GetAssetKey(object dataItem)`：用于预览时的唯一标识（默认使用名称）。
   - `FillAsset(T asset, object dataItem)`：将数据填充到新创建的 `ScriptableObject` 实例中。

#### 示例

参考 `AssetExampleSplitter.cs`：

```csharp
[InitializeOnLoad]
public class AssetExampleSplitter : AssetSplitGenerator<AssetExamplePreset>
{
    private static AssetExampleSplitter _instance = new();

    protected override IEnumerable<object> GetSplitDataList()
    {
        return new List<AssetExampleData>
        {
            new() { id = 0, description = "示例_1", name = "[插件] 资源生成示例_1.0.0", price = 100 },
            new() { id = 1, description = "示例_2", name = "[插件] 资源生成示例_1.0.1", price = 200 },
            new() { id = 2, description = "示例_3", name = "[插件] 资源生成示例_1.0.2", price = 300 }
        };
    }

    protected override string GetAssetName(object dataItem)
    {
        return (dataItem as AssetExampleData)?.description ?? "item";
    }

    protected override void FillAsset(AssetExamplePreset asset, object dataItem)
    {
        asset.presets = dataItem as AssetExampleData;
    }
}
```

拖拽 `AssetExamplePreset.cs` 脚本到工具窗口后，会自动生成 3 个资产计划，名称分别为 `AssetExamplePreset_示例_1`、`AssetExamplePreset_示例_2`、`AssetExamplePreset_示例_3`。生成时每个资产都会用对应的数据填充。

---

## 整体逻辑架构

### 核心组件

| 类/文件 | 职责 |
|--------|------|
| `CustomAssetGenerator` | 静态注册中心，存储每个 `ScriptableObject` 类型的自定义生成器与预览器委托。 |
| `AssetSplitGenerator<T>` | 抽象基类，自动注册到 `CustomAssetGenerator`。提供拆分数据、命名、填充的模板方法，并实现生成与预览逻辑。 |
| `AssetGeneratorEditorWindow` | 主窗口 UI。管理脚本列表、资产计划、输出目录，协调普通生成与自定义生成流程。 |
| `UniversalAssetEditorWindow` | 通用 Inspector 编辑窗口，支持编辑任意 `UnityEngine.Object`，可用于草稿编辑或直接修改持久化资产。 |
| `AssetsInputDialogWindow` | 简单的输入框弹窗，用于处理名称重复时用户输入新名称。 |

### 数据流

1. **脚本拖拽** → 窗口创建 `ScriptableObjectInfo`，存储脚本路径、资产计划列表、已生成资产路径。
2. **初始化资产计划** → 尝试调用 `CustomAssetGenerator.TryPreview`，若返回非空则作为预览计划（标记 `isPreviewAsset = true`），否则创建默认计划。
3. **生成资产**：
   - 若计划标记为预览且对应的类型已注册自定义生成器 → 调用 `TryGenerate`，由拆分器完成生成。
   - 否则 → 创建默认 `ScriptableObject` 实例，若存在草稿 JSON 则填充，然后保存为 `.asset`。
4. **覆盖逻辑** → 如果目标路径已有资产，根据用户选择决定是否删除、是否反序列化旧数据并应用到新资产。
5. **持久化** → 窗口数据（脚本列表、输出目录）通过 `SessionState` 保存，Unity 会话期间有效。

### 注册机制

- 每个 `AssetSplitGenerator<T>` 子类在构造函数中调用 `CustomAssetGenerator.RegisterHandler`，将自己的生成与预览方法注册到 `typeof(T)`。
- 主窗口在添加脚本时，通过 `CustomAssetGenerator.TryPreview` 获取预览计划列表，用于初始化 `assetPlans`。
- 生成时通过 `CustomAssetGenerator.TryGenerate` 判断是否存在自定义生成器，若有则委托给拆分器。

### 扩展性

- 如需为某个 `ScriptableObject` 实现完全自定义的生成逻辑（非拆分模式），可直接调用 `CustomAssetGenerator.RegisterHandler` 注册委托，不依赖 `AssetSplitGenerator`。
- 拆分器只需关注数据源与填充逻辑，生成路径、覆盖、草稿等都由框架统一处理。

---

## 注意事项

- 拆分器类必须添加 `[InitializeOnLoad]` 并创建静态实例，确保 Unity 启动时自动注册。
- 输出目录必须在 `Assets` 目录下，否则选择时会报错。
- 预览计划的资产名称由拆分器的 `GetAssetName` 决定，重命名不会影响拆分器内部逻辑（但会改变最终文件名）。
- 编辑待生成资产时，草稿 JSON 保存在 `AssetPlanInfo.editorJson` 字段中，仅用于生成时填充，不会影响已生成的资产。
- 已生成资产路径列表会在窗口关闭时自动清理无效路径（资产被删除后）。

---

## 菜单入口

`ByTools → 🗂️ Asset资产生成工具`

---

## 依赖

- Unity 2019.4 或更高版本（使用了 `SessionState`、`EditorJsonUtility` 等 API）。
- 所有代码位于 `_3rdBy.ByTools.GenerateAssets.Editor` 命名空间下，建议放入 `Editor` 文件夹。

---

## 常见问题

**Q：拖拽脚本后没有生成任何资产计划？**  
A：请确保脚本正确继承自 `ScriptableObject`，并且编译无报错。如果实现了拆分器但没有预览计划，检查 `GetSplitDataList()` 是否返回了有效数据。

**Q：拆分器生成的资产名称与预览不一致？**  
A：预览时显示的名称是 `{baseAssetName}_{GetAssetName(dataItem)}`，生成时实际文件名来自 `assetPlan.name`。如果手动重命名了预览计划，拆分器生成时仍会根据 `GetAssetKey` 匹配计划，但文件名会使用重命名后的值。

**Q：如何修改拆分器的数据源？**  
A：直接修改 `GetSplitDataList()` 的返回值，重新打开工具或重新拖拽脚本即可刷新预览计划（注意：已有的资产计划不会被自动更新，建议先移除旧脚本再添加）。

**Q：覆盖资产时“带入数据”的原理？**  
A：工具会先将旧资产序列化为 JSON（`EditorJsonUtility.ToJson`），删除旧资产，创建新资产，再将 JSON 反序列化到新资产上。这可以保留旧资产的字段值，适用于结构未变或向后兼容的情况。