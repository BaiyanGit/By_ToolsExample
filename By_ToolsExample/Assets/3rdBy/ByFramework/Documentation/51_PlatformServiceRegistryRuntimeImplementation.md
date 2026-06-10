# R1.2 PlatformServiceRegistry Runtime Implementation

> 阶段性质：Runtime Implementation  
> 当前阶段允许实现 PlatformServiceRegistry 运行时代码、Editor 监视工具和基础验证脚本  
> 不允许进入 FrameworkConfig / SaveSystem / ResourceSystem 等其它模块实现  
> 不允许修改 FrameworkEntry 生命周期  

---

# 一、阶段目标

R1.2 的目标是根据 R1.1 API Freeze 文档，实现 PlatformServiceRegistry 的最小可用 Runtime。

本阶段允许实现：

```text
IPlatformService
IPlatformServiceRegistry
PlatformServiceRegistry
PlatformServiceRegistryInstance
ServiceDescriptor
ServiceState
基础注册 / 获取 / 替换 / 注销 API
受控状态更新 API
服务枚举 API
线程安全处理
Editor 监视窗口
基础验证脚本 / 菜单
```

本阶段禁止实现：

```text
FrameworkConfig
SaveSystem
ResourceSystem
InputSystem
UISystem
NetworkSystem
任何 FeatureModule
```

---

# 二、实现范围

本阶段只实现：

```text
Platform/PlatformServiceRegistry
```

建议目录：

```text
Assets/ByFramework/Platform/PlatformServiceRegistry
├─ Runtime
│  ├─ IPlatformService.cs
│  ├─ IPlatformServiceRegistry.cs
│  ├─ PlatformServiceRegistry.cs
│  ├─ PlatformServiceRegistryInstance.cs
│  ├─ ServiceDescriptor.cs
│  └─ ServiceState.cs
│
└─ Editor
   ├─ PlatformServiceRegistryMonitorWindow.cs
   └─ PlatformServiceRegistryValidationMenu.cs
```

如果当前项目已有不同目录结构，可在不破坏架构边界的前提下适配。

---

# 三、Registry 形式

选择：

```text
C：静态入口 + 内部实例
```

---

## 3.1 含义

外部使用静态入口：

```csharp
PlatformServiceRegistry.Register<IInputService>(service);
PlatformServiceRegistry.Get<IInputService>();
PlatformServiceRegistry.TryGet<IInputService>(out var service);
```

内部由实例对象承载真实逻辑：

```csharp
PlatformServiceRegistryInstance
```

---

## 3.2 建议

这样既保持使用简单，又便于后续测试和替换内部实现。

禁止把所有逻辑都写成无法替换的纯静态状态。

---

# 四、接口设计

选择：

```text
A：需要 IPlatformServiceRegistry
```

---

## 4.1 IPlatformServiceRegistry

建议接口方向：

```csharp
public interface IPlatformServiceRegistry
{
    void Register<TService>(TService service);

    bool TryRegister<TService>(TService service);

    void Replace<TService>(TService service);

    bool Unregister<TService>();

    bool Contains<TService>();

    TService Get<TService>();

    bool TryGet<TService>(out TService service);

    bool MarkInitializing<TService>();

    bool MarkInitialized<TService>();

    bool MarkDisposing<TService>();

    bool MarkDisposed<TService>();

    bool MarkFailed<TService>();

    IReadOnlyList<ServiceDescriptor> GetAllDescriptors();

    void Clear();
}
```

实现阶段可以根据 C# 版本微调返回类型，但不能改变核心能力。

---

# 五、IPlatformService

选择：

```text
不强制，但验证工具警告
```

---

## 5.1 接口方向

建议实现：

```csharp
public interface IPlatformService
{
}
```

它是 Platform 服务标记接口。

---

## 5.2 注册约束

Register 不强制：

```csharp
where TService : IPlatformService
```

原因：

```text
保持兼容性
避免影响旧代码
避免早期接口设计过硬
```

但验证工具应检查：

```text
注册服务是否实现 IPlatformService
```

未实现则给出警告。

---

# 六、注册 API 行为

实现以下规则：

```text
Register<T>()
    - service 为 null：抛出 ArgumentNullException
    - 已注册：抛出 InvalidOperationException
    - 未注册：注册成功，状态 Registered

TryRegister<T>()
    - service 为 null：抛出 ArgumentNullException
    - 已注册：返回 false
    - 未注册：注册成功，返回 true

Replace<T>()
    - service 为 null：抛出 ArgumentNullException
    - 已注册：替换服务，状态 Registered
    - 未注册：注册服务，状态 Registered

Unregister<T>()
    - 已注册：移除服务，返回 true
    - 未注册：返回 false

Contains<T>()
    - 已注册：true
    - 未注册：false
```

---

# 七、获取 API 行为

实现以下规则：

```text
Get<T>()
    - 已注册：返回服务
    - 未注册：抛出 InvalidOperationException

TryGet<T>(out service)
    - 已注册：返回 true，service 为实例
    - 未注册：返回 false，service 为 default
```

---

# 八、服务 Key 规则

当前只支持：

```text
Type
```

即：

```csharp
typeof(TService)
```

预留命名服务，但本阶段不实现：

```text
Type + Name
```

禁止 Codex 在 R1.2 中实现命名服务。

---

# 九、服务状态

选择：

```text
完整生命周期状态
```

---

## 9.1 ServiceState

建议实现：

```csharp
public enum ServiceState
{
    None,
    Registered,
    Initializing,
    Initialized,
    Disposing,
    Disposed,
    Failed
}
```

---

## 9.2 状态更新 API

选择：

```text
C：受控 API
```

实现方向：

```csharp
bool MarkInitializing<TService>();
bool MarkInitialized<TService>();
bool MarkDisposing<TService>();
bool MarkDisposed<TService>();
bool MarkFailed<TService>();
```

---

## 9.3 状态边界

PlatformServiceRegistry 不调用服务初始化或释放。

它只记录状态。

禁止实现：

```text
InitializeAll
ShutdownAll
自动调用 service.Initialize
自动调用 service.Dispose
```

---

# 十、ServiceDescriptor

选择：

```text
C：Runtime 暴露只读，Editor 可查看
```

---

## 10.1 Descriptor 字段方向

建议实现：

```csharp
public sealed class ServiceDescriptor
{
    public Type ServiceType { get; }
    public Type ImplementationType { get; }
    public object Instance { get; }
    public ServiceState State { get; }
    public DateTime RegisterTime { get; }
    public string Source { get; }
    public bool IsPlatformService { get; }
}
```

---

## 10.2 只读原则

Descriptor 对外只读。

外部不能直接修改 State。

State 只能通过受控 API 修改。

---

# 十一、线程安全

选择：

```text
A：需要线程安全
```

实现方向可选：

```text
ConcurrentDictionary
lock
ReaderWriterLockSlim
```

要求：

```text
注册安全
获取安全
替换安全
注销安全
状态更新安全
枚举安全
Clear 安全
```

GetAllDescriptors 应返回只读副本，不能暴露内部集合。

---

# 十二、Clear

选择：

```text
C：提供 Clear()，但不自动调用
```

---

## 12.1 含义

Clear 用于：

```text
测试
验证
Editor 调试
退出清理
```

但 Registry 不主动参与 FrameworkEntry 生命周期。

---

## 12.2 禁止事项

禁止在 R1.2 中修改 FrameworkEntry 自动调用 Clear。

后续若需要接入生命周期，必须另开阶段讨论。

---

# 十三、异常类型

选择：

```text
C：先标准异常，后期再自定义
```

---

## 13.1 当前阶段异常

建议使用：

```text
ArgumentNullException
InvalidOperationException
```

暂不创建自定义异常类。

---

# 十四、Editor 监视工具

选择：

```text
A：本阶段实现
```

---

## 14.1 菜单路径

建议：

```text
ByFramework/平台/PlatformServiceRegistry 监视器
```

---

## 14.2 显示内容

监视窗口显示：

```text
服务接口
实现类型
状态
注册时间
来源
是否实现 IPlatformService
```

---

## 14.3 功能

至少提供：

```text
刷新服务列表
验证服务表
清空服务表（可选，需二次确认）
```

---

## 14.4 中文化

Editor UI 必须中文。

示例：

```text
按钮：刷新服务列表
按钮：验证服务表
提示：当前未注册任何 Platform 服务
警告：发现未实现 IPlatformService 的服务
日志：[PlatformServiceRegistry] 服务注册表验证完成
```

---

# 十五、验证脚本 / 菜单

选择：

```text
A：需要
```

---

## 15.1 验证菜单

建议菜单：

```text
ByFramework/验证/PlatformServiceRegistry 基础验证
```

---

## 15.2 验证内容

验证以下内容：

```text
Register 正常
重复 Register 抛异常
TryRegister 重复返回 false
Replace 可替换
Get 可获取
Get 未注册抛异常
TryGet 未注册返回 false
Unregister 正常
Contains 正常
MarkInitialized 正常
Clear 正常
ServiceDescriptor 正常
```

---

## 15.3 验证输出

输出中文日志：

```text
[PlatformServiceRegistry] Register 验证通过
[PlatformServiceRegistry] 重复注册验证通过
[PlatformServiceRegistry] 基础验证完成
```

失败时输出：

```text
[PlatformServiceRegistry] 验证失败：原因
```

---

# 十六、FrameworkEntry 规则

选择：

```text
B：不允许改 FrameworkEntry
```

R1.2 禁止：

```text
修改 FrameworkEntry
修改 Core 生命周期
修改 ThreadDispatcher / EventManager / FSMManager 初始化顺序
```

PlatformServiceRegistry 当前不进入 Core 生命周期链。

---

# 十七、文档同步

R1.2 完成后必须同步：

```text
Documentation/00_ByFramework_Current_Context.md
02_Roadmap.md
03_Todo.md
Documentation/04_Changelog.md
```

同步内容：

```text
R1.2 PlatformServiceRegistry Runtime Implementation 已完成
已实现 PlatformServiceRegistry Runtime
已实现 Editor 监视工具
已实现基础验证菜单
未修改 FrameworkEntry 生命周期
未进入其它模块实现
```

---

# 十八、R1.2 禁止事项

R1.2 阶段禁止：

```text
实现 FrameworkConfig
实现 SaveSystem
实现 ResourceSystem
实现 InputSystem
实现 UISystem
实现 NetworkSystem
实现 FeatureModule
实现命名服务
实现 IOC 容器
实现生命周期管理器
修改 FrameworkEntry
扩展 Core/Config/FrameworkConfig.cs
让 FeatureModule 注册 PlatformServiceRegistry
让 NetworkCore 依赖 PlatformServiceRegistry
```

---

# 十九、R1.2 关闭条件

R1.2 关闭条件：

```text
IPlatformService 已实现
IPlatformServiceRegistry 已实现
PlatformServiceRegistry 静态入口已实现
PlatformServiceRegistryInstance 内部实例已实现
ServiceDescriptor 已实现
ServiceState 已实现
Register / TryRegister / Replace / Unregister / Contains 已实现
Get / TryGet 已实现
受控状态更新 API 已实现
GetAllDescriptors 已实现
Clear 已实现但未自动接入 FrameworkEntry
线程安全已处理
Editor 监视窗口已实现
基础验证菜单已实现
中文日志 / 中文 UI 已符合规范
未修改 FrameworkEntry 生命周期
未实现非当前阶段模块
文档已同步
```

满足后允许进入：

```text
R2.1 FrameworkConfig Runtime API Freeze
```

---

# 二十、给 Codex 的执行指令

可直接给 Codex：

```text
进入 R1.2 PlatformServiceRegistry Runtime Implementation。

请先阅读：
AGENTS.md
Documentation/00_ByFramework_Current_Context.md
Documentation/40_P3_Foundation_Closure_And_Runtime_Entry.md
Documentation/50_PlatformServiceRegistryRuntimeAPIFreeze.md
Documentation/51_PlatformServiceRegistryRuntimeImplementation.md

本阶段只允许实现 PlatformServiceRegistry Runtime、Editor 监视窗口和基础验证菜单。

必须采用：
静态入口 + 内部实例
IPlatformServiceRegistry 接口
IPlatformService 标记接口
ServiceDescriptor
ServiceState
线程安全
受控状态更新 API
Clear() 但不自动接入 FrameworkEntry
标准异常

必须实现 Editor 监视工具和基础验证菜单，UI 与日志使用中文。

禁止：
修改 FrameworkEntry 生命周期
实现 FrameworkConfig
实现 SaveSystem
实现 ResourceSystem
实现任何其它 Platform 模块
扩展 Core/Config/FrameworkConfig.cs
让 FeatureModule 注册到 PlatformServiceRegistry

完成后同步：
Documentation/00_ByFramework_Current_Context.md
02_Roadmap.md
03_Todo.md
Documentation/04_Changelog.md
```
