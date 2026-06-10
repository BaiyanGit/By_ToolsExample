# ByFramework 编码规范

## 1. 文件头注释

每个 C# 文件顶部必须包含文件头注释：

```csharp
//=====================================================
// 文件名称: Xxx.cs
// 创 建 者: wangbaiyan
// 创建日期: yyyy-MM-dd
// 描    述: 当前脚本的主要职责
//=====================================================
```

## 2. 命名空间规范

所有框架代码必须放入命名空间中。

示例：

```csharp
namespace _3rdBy.ByFramework.xxx
{
    public class AssetsBundleManager
    {
    }
}
```

## 3. 类和结构体注释

所有 public class、public struct、public interface、public enum 必须添加 XML 注释。

示例：

```csharp
/// <summary>
/// 翻斗车货斗控制器，管理货斗的举升、下降和卸货动画。
/// 需要依赖 HydraulicSystem 组件计算实际压力。
/// </summary>
public class DumpTruckBedController : MonoBehaviour
{
}
```

## 4. 方法注释

所有 public 方法必须使用 XML 注释。

必须说明：

* 方法作用
* 参数含义
* 返回值含义
* 可能抛出的异常，如果没有可不写

示例：

```csharp
/// <summary>
/// 执行挖掘动作，根据当前斗杆角度和铲斗姿态进行挖掘。
/// </summary>
/// <param name="digDepth">目标挖掘深度，单位为米，必须大于 0。</param>
/// <returns>是否挖掘到材料，true 表示挖掘成功。</returns>
public bool PerformDig(float digDepth)
{
}
```

## 5. 字段注释

所有 public 字段和 private 字段前面都必须添加 Unity Header 注释。

注意：使用 Unity 原生 `[Header]`，不要写成 `[Head]`。

私有字段示例：

```csharp
[Header("挖掘机的当前速度，单位为米/秒")]
private float _currentSpeed;
```

公有字段示例：

```csharp
[Header("挖掘机的当前速度，单位为米/秒")]
public int currentSpeed;
```

## 6. 属性规范

属性使用 PascalCase。

示例：

```csharp
public int CurrentSpeed
{
    get { return currentSpeed; }
    set { currentSpeed = value; }
}

private int CurrentSpeed
{
    get { return currentSpeed; }
    set { currentSpeed = value; }
}
```

## 7. 复杂逻辑注释

对于算法、非直观业务逻辑、兼容性处理、Hack 写法，必须说明“为什么这样做”，而不是重复说明代码做了什么。

示例：

```csharp
// 由于物理引擎在高速运动时可能穿透地面，这里采用连续碰撞检测。
// 这样可以确保铲斗与地面发生正确交互。
if (_rigidbody.isKinematic == false)
{
    _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
}
```

## 8. 命名建议

private 字段使用下划线开头：

```csharp
private int _count;
```

public 字段使用 camelCase：

```csharp
public int count;
```

属性使用 PascalCase：

```csharp
public int Count { get; set; }
```

方法使用 PascalCase：

```csharp
public void OpenWindow()
{
}
```

## 9. 修改旧代码时的要求

修改已有代码时：

* 保持原有功能不变
* 补充缺失文件头
* 补充 public 类型 XML 注释
* 补充 public 方法 XML 注释
* 给字段补充 `[Header]`
* 不为了注释大范围重构逻辑
* 不破坏现有 API

## 10. 中文优先规则

ByFramework 默认面向中文开发团队。文档语言、Editor UI、Console 日志、验证报告和新增 Runtime 日志均默认中文优先，目标用户主要为国内开发人员、实施人员和测试人员。

### 10.1 Editor UI Language Convention

Editor 工具默认使用中文的范围包括：

* `MenuItem`
* `EditorWindow` 标题
* Button
* Label
* HelpBox
* Validation Report
* Console Log
* Build Tool
* Verification Tool
* Config Tool

菜单路径优先使用中文分组：

```text
ByFramework/工具/xxx
ByFramework/验证/xxx
ByFramework/配置/xxx
```

已有菜单较多或可能被团队习惯依赖时，可以先只修改新增工具与明显英文菜单，避免破坏使用习惯。

### 10.2 Debug Log Language Convention

新增日志默认使用中文的范围包括：

* `Debug.Log`
* `Debug.LogWarning`
* `Debug.LogError`
* `Debug.Assert`
* EditorWindow 日志
* Verification Tool 日志
* Build Tool 日志
* Config Tool 日志
* Console 输出
* 验证报告输出

Runtime 面向开发者的日志采用“中文说明 + 英文对象名”：

```csharp
Debug.Log("[EventManager] 初始化完成");
Debug.LogWarning("[FSMManager] 检测到重复状态机");
Debug.LogError("[ResourceSystem] 未找到资源：UI.Common.ButtonConfirm");
```

避免新增纯英文提示：

```csharp
Debug.Log("Initialization Completed");
Debug.LogWarning("Duplicate FSM Found");
Debug.LogError("FrameworkEntry Not Found");
```

必要技术对象名可以保留英文，例如 FSM、EventManager、FSMManager、ThreadDispatcher、ResourceKey、InputAction、BuildProfile、Protobuf、TCP、UDP、WebSocket、AssetBundle、Domain Reload、Play Mode。

禁止为了中文化强行翻译稳定技术对象名；提示信息与说明文字使用中文。新增 Editor 工具默认采用中文 UI，新增 Runtime 日志默认采用中文提示。

### 10.3 暂缓中文化范围

以下内容不得为了中文化而破坏兼容性：

* API 名称
* 类名
* 方法名
* 字段名
* 命名空间
* 文件名
* 资源路径
* 配置 Key
* EventKey
* ResourceKey
* Protobuf 生成代码
* 原始协议字段名
* 仅用于调试的数值、路径或对象名直出
