# R1.1 PlatformServiceRegistry Runtime API Freeze

> 阶段性质：Runtime API Freeze  
> 不是 Runtime Implementation  
> 本阶段只冻结 PlatformServiceRegistry 的运行时 API、数据结构、行为规则、错误策略和验证方向  
> 不实现代码  

---

# 一、阶段目标

R1.1 的目标是冻结 PlatformServiceRegistry Runtime API，为 R1.2 PlatformServiceRegistry Runtime Implementation 提供明确实现依据。

本阶段只允许完成：

```text
PlatformServiceRegistry 定位冻结
注册 API 冻结
获取 API 冻结
注销 API 冻结
替换 API 冻结
服务 Key 规则冻结
重复注册规则冻结
服务状态模型冻结
ServiceDescriptor 模型冻结
线程安全规则冻结
FeatureModule 注册边界冻结
验证工具方向冻结
```

本阶段不进入具体代码实现。

---

# 二、架构位置

PlatformServiceRegistry 属于：

```text
Platform/PlatformServiceRegistry
```

它是 Platform 层的服务注册中心。

它不属于 Core。

它不属于 FeatureModule。

---

# 三、核心定位

PlatformServiceRegistry 的核心定位选择：

```text
A：只负责注册 / 获取 / 注销服务
```

---

## 3.1 含义

PlatformServiceRegistry 是轻量平台服务注册表。

它负责：

```text
注册服务
获取服务
尝试获取服务
替换服务
注销服务
查询服务
调试服务
```

它不负责：

```text
服务生命周期
服务初始化顺序
服务配置加载
服务业务逻辑
服务依赖注入
模块启动流程
```

---

## 3.2 建议

PlatformServiceRegistry 应保持极简。

它不是：

```text
生命周期管理器
IOC 容器
配置中心
业务管理器
万能 ServiceLocator
```

它只是 Platform 服务的统一入口。

---

# 四、服务注册范围

PlatformServiceRegistry 当前只注册 Platform 服务。

例如：

```text
IFrameworkConfigService
IInputService
IUIService
IDisplayService
ILocalizationService
IResourceService
ISaveService
INetworkService
IBuildProfileService
ILicenseService
```

---

# 五、FeatureModule 注册规则

FeatureModule 是否允许注册服务选择：

```text
C：预留 FeatureServiceRegistry，当前不允许
```

---

## 5.1 含义

当前 PlatformServiceRegistry 不允许 FeatureModule 业务服务注册进来。

禁止：

```text
VehicleSimulationService
TrainingService
CustomerAService
ScenarioService
SimulationSyncService
```

直接注册到 PlatformServiceRegistry。

---

## 5.2 建议

未来如有需要，可新增：

```text
FeatureServiceRegistry
```

或者：

```text
FeatureModuleRegistry
```

用于业务模块注册。

这样可以避免业务污染 Platform 服务表。

---

# 六、服务 Key 规则

服务 Key 方式选择：

```text
A：只按 Type 注册
预留 B：Type + Name
```

---

## 6.1 当前规则

当前阶段只支持按接口 Type 注册。

示例：

```csharp
Register<IInputService>(inputService);
Get<IInputService>();
TryGet<IInputService>(out var inputService);
```

---

## 6.2 预留规则

未来如需要多实例服务，可扩展：

```text
Type + Name
```

例如：

```csharp
Register<IResourceService>("Default", defaultResourceService);
Register<IResourceService>("Remote", remoteResourceService);
```

但 R1.1 不冻结命名服务 API。

---

# 七、注册 API

建议冻结以下 API 方向：

```csharp
void Register<TService>(TService service);

bool TryRegister<TService>(TService service);

void Replace<TService>(TService service);

bool Unregister<TService>();

bool Contains<TService>();
```

---

## 7.1 Register

行为：

```text
服务不存在：注册成功
服务已存在：抛出异常
service 为 null：抛出异常
```

适用场景：

```text
必须成功注册的平台服务
```

---

## 7.2 TryRegister

行为：

```text
服务不存在：注册成功，返回 true
服务已存在：不覆盖，返回 false
service 为 null：返回 false 或抛出参数异常，具体实现阶段冻结
```

建议：

```text
service 为 null 抛出 ArgumentNullException
重复注册返回 false
```

适用场景：

```text
可选服务
防御性注册
测试注册
```

---

## 7.3 Replace

行为：

```text
服务不存在：注册新服务
服务已存在：显式替换
service 为 null：抛出异常
```

适用场景：

```text
测试替换
平台服务重建
配置切换后的显式替换
```

注意：

```text
Replace 必须显式调用
Register 不允许隐式覆盖
```

---

## 7.4 Unregister

行为：

```text
服务存在：注销并返回 true
服务不存在：返回 false
```

注销后服务状态应进入：

```text
Disposed
```

或至少标记为：

```text
Unregistered / Disposed
```

具体状态由 ServiceState 冻结。

---

## 7.5 Contains

行为：

```text
服务存在：true
服务不存在：false
```

用于：

```text
验证
调试
可选功能检测
```

---

# 八、获取 API

获取不存在的服务规则选择：

```text
D：Get<T>() 抛异常，TryGet<T>() 返回 bool
```

---

## 8.1 Get

建议 API：

```csharp
TService Get<TService>();
```

行为：

```text
服务存在：返回服务实例
服务不存在：抛出异常
```

适用场景：

```text
必须存在的平台服务
```

例如：

```csharp
var config = PlatformServiceRegistry.Get<IFrameworkConfigService>();
```

---

## 8.2 TryGet

建议 API：

```csharp
bool TryGet<TService>(out TService service);
```

行为：

```text
服务存在：返回 true，并输出服务
服务不存在：返回 false，service 为 default
```

适用场景：

```text
可选平台服务
可选功能模块
降级逻辑
```

---

# 九、重复注册规则

重复注册规则选择：

```text
A + C + D
```

即：

```text
Register 禁止重复
TryRegister 重复返回 false
Replace 显式替换
```

---

## 9.1 含义

不允许普通 Register 静默覆盖。

避免：

```text
服务被意外替换
调试困难
初始化顺序错误被掩盖
```

---

## 9.2 建议

如果确实需要替换，必须调用：

```csharp
Replace<TService>(service);
```

这样代码意图更明确。

---

# 十、Null 注册规则

是否允许注册 null 选择：

```text
A：禁止
```

---

## 10.1 含义

禁止：

```csharp
Register<IInputService>(null);
```

---

## 10.2 建议

实现阶段应直接抛出：

```text
ArgumentNullException
```

这样可以在注册时暴露错误，而不是等到获取服务时才发现。

---

# 十一、服务状态

服务状态选择：

```text
C：需要完整生命周期状态
```

注意：

```text
Registry 记录状态
但不管理生命周期
```

---

## 11.1 ServiceState 方向

建议冻结状态方向：

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

## 11.2 含义

状态用于：

```text
验证工具
调试窗口
日志
运行时诊断
服务可用性检查
```

---

## 11.3 边界

PlatformServiceRegistry 不调用服务的 Initialize / Shutdown。

但可以提供状态更新 API。

例如：

```csharp
void SetState<TService>(ServiceState state);
```

是否开放 SetState 需要在实现阶段进一步确认。

建议：

```text
可以提供内部或受控状态更新能力
但不让外部随意修改所有服务状态
```

---

# 十二、ServiceDescriptor

是否需要服务描述信息选择：

```text
B：需要 ServiceDescriptor
```

---

## 12.1 ServiceDescriptor 含义

ServiceDescriptor 用于保存服务元数据。

建议字段方向：

```text
ServiceType
ImplementationType
Instance
State
RegisterTime
Source
Description
IsPlatformService
```

---

## 12.2 用途

用于：

```text
服务调试
验证工具
Editor 监视窗口
运行时诊断
文档报告
```

---

## 12.3 边界

ServiceDescriptor 不应承载业务数据。

它只描述服务注册信息。

---

# 十三、线程安全

线程安全选择：

```text
A：需要线程安全
```

---

## 13.1 含义

PlatformServiceRegistry 后续可能被以下模块访问：

```text
NetworkSystem
LargeFileDownloader
SaveSystem
ResourceSystem
异步任务
后台线程
```

因此注册表必须考虑线程安全。

---

## 13.2 实现方向

实现阶段可选择：

```text
ConcurrentDictionary
lock
ReaderWriterLockSlim
```

本阶段不冻结具体实现方式。

但必须保证：

```text
注册安全
获取安全
注销安全
替换安全
枚举安全
```

---

# 十四、服务枚举 API

为了验证工具和调试工具，建议预留枚举能力：

```csharp
IReadOnlyList<ServiceDescriptor> GetAllDescriptors();
```

---

## 14.1 含义

验证窗口可以读取所有服务描述。

显示：

```text
服务接口
实现类型
状态
注册时间
来源
```

---

## 14.2 边界

返回结果应为只读副本或只读视图。

避免外部直接修改内部字典。

---

# 十五、异常与错误策略

建议错误类型方向：

```text
ServiceAlreadyRegisteredException
ServiceNotRegisteredException
InvalidServiceException
```

也可以先使用标准异常：

```text
InvalidOperationException
KeyNotFoundException
ArgumentNullException
```

本阶段不强制自定义异常。

实现阶段可根据代码复杂度决定。

---

# 十六、验证工具

是否需要验证工具选择：

```text
A：需要
```

---

## 16.1 验证工具方向

建议提供 Editor 验证窗口：

```text
PlatformServiceRegistryMonitorWindow
```

菜单示例：

```text
ByFramework/平台/PlatformServiceRegistry 监视器
```

---

## 16.2 显示内容

验证工具显示：

```text
服务接口
实现类型
服务状态
注册时间
来源
是否 Platform 服务
```

---

## 16.3 验证内容

验证工具检查：

```text
是否存在重复服务
是否存在 null 服务
是否存在非 Platform 服务
是否存在 Failed 状态服务
是否存在状态异常服务
```

---

## 16.4 中文化规则

验证工具必须中文化。

示例：

```text
按钮：刷新服务列表
提示：当前已注册 Platform 服务
警告：[PlatformServiceRegistry] 发现异常服务状态
日志：[PlatformServiceRegistry] 服务注册表验证完成
```

---

# 十七、建议目录结构

R1.2 实现阶段建议目录：

```text
Assets/ByFramework/Platform/PlatformServiceRegistry
├─ Runtime
│  ├─ PlatformServiceRegistry.cs
│  ├─ ServiceDescriptor.cs
│  ├─ ServiceState.cs
│  ├─ IPlatformService.cs
│  └─ Exceptions
│
└─ Editor
   └─ PlatformServiceRegistryMonitorWindow.cs
```

本阶段不创建代码文件，只冻结目录方向。

---

# 十八、IPlatformService

建议预留接口：

```csharp
public interface IPlatformService
{
}
```

---

## 18.1 含义

IPlatformService 是 Platform 服务标记接口。

用于区分：

```text
Platform 服务
FeatureModule 服务
普通对象
```

---

## 18.2 是否强制实现

建议：

```text
Platform 服务建议实现 IPlatformService
Register<TService> 可不强制约束 TService : IPlatformService
但验证工具应提示未实现标记接口的服务
```

原因：

```text
保持灵活
避免接口约束过早影响旧代码
同时通过验证工具约束规范
```

---

# 十九、PlatformServiceRegistry 边界

PlatformServiceRegistry 不负责：

```text
创建服务实例
销毁服务实例
服务初始化
服务 Shutdown
读取配置
加载资源
派发业务事件
管理 FeatureModule
管理 NetworkCore
管理 RuntimeConfigUI
```

---

# 二十、禁止事项

R1.1 阶段禁止：

```text
实现 PlatformServiceRegistry 代码
实现 Editor 监视窗口
修改 FrameworkEntry 生命周期
扩展 Core/Config/FrameworkConfig.cs
实现 FrameworkConfig
实现 SaveSystem
实现 ResourceSystem
实现任何其它 Platform 模块
允许 FeatureModule 注册到 PlatformServiceRegistry
让 NetworkCore 依赖 PlatformServiceRegistry
```

---

# 二十一、R1.1 输出物

R1.1 应输出：

```text
Documentation/PlatformServiceRegistryRuntimeAPIFreeze.md
Documentation/ByFramework_Current_Context.md 更新
Roadmap.md 更新
Todo.md 更新
Documentation/Changelog.md 更新
```

---

# 二十二、R1.1 阶段关闭条件

R1.1 关闭条件：

```text
PlatformServiceRegistry 定位明确
注册 API 明确
获取 API 明确
注销 API 明确
替换 API 明确
重复注册规则明确
Type Key 规则明确
命名服务预留明确
服务状态模型明确
ServiceDescriptor 明确
线程安全规则明确
Null 注册禁止明确
FeatureModule 当前不允许注册规则明确
验证工具方向明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，允许进入：

```text
R1.2 PlatformServiceRegistry Runtime Implementation
```

---

# 二十三、最终结论

R1.1 PlatformServiceRegistry Runtime API Freeze 只冻结 API 与行为规则。

它不实现代码。

后续 R1.2 才允许实现：

```text
PlatformServiceRegistry
ServiceDescriptor
ServiceState
IPlatformService
Editor 验证工具
```
