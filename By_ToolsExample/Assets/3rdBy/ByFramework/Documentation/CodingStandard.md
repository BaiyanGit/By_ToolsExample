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
