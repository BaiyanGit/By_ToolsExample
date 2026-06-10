# R6.1 LocalizationSystem Runtime API Freeze

> 阶段性质：Runtime API Freeze  
> 不是 Runtime Implementation  
> 本阶段只冻结 LocalizationSystem 的运行时 API、上下文模型、错误策略、语言切换顺序和验证方向  
> 不实现代码  

---

# 一、阶段目标

R6.1 的目标是冻结 LocalizationSystem Runtime API，为 R6.2 LocalizationSystem Runtime Implementation 提供明确实现依据。

本阶段只允许完成：

```text
LocalizationSystem 运行时职责冻结
FrameworkConfig 与 SaveSystem 的语言归属冻结
ILocalizationService 方法面冻结
LocalizationContext 模型冻结
LocalizationArguments 模型冻结
查询错误模型冻结
语言切换错误模型冻结
LanguageChanged 事件行为冻结
Provider 集合与生命周期边界冻结
验证方式冻结
```

本阶段不进入具体代码实现。

---

# 二、架构位置

LocalizationSystem 属于：

```text
Platform/LocalizationSystem
```

它是 Platform 层的统一多语言运行时服务。

它不属于 Core。

它不属于 FeatureModule。

---

# 三、核心定位

LocalizationSystem 的核心定位选择：

```text
A：只负责语言选择、文本查询、格式化、语言回退和语言切换通知
```

---

## 3.1 它负责

```text
维护当前生效语言
根据 LocalizationKey 查询文本
执行回退链解析
执行参数格式化
维护可用语言集合快照
暴露只读 LocalizationContext
切换语言并发布 LanguageChanged
聚合 Provider 查询与失败诊断
```

---

## 3.2 它不负责

```text
直接刷新 UI
持有 Text / TMP_Text 引用
决定字体与 Theme
保存 Framework 启动配置
直接处理 SaveSystem 存储细节
维护具体业务翻译内容
重写 Excel 转 JSON 工具链
实现阿拉伯语 shaping 或 RTL 排版算法
```

---

# 四、语言归属冻结

## 4.1 defaultLanguage 归属

`defaultLanguage` 归属：

```text
FrameworkConfig
```

含义：

```text
框架默认语言
项目启动时的基础回退语言
当没有用户偏好或用户偏好不可用时的默认选择
```

规则：

```text
defaultLanguage 是启动默认值，不是用户状态
defaultLanguage 可以被 LocalizationSystem 读取
defaultLanguage 不由 LocalizationSystem 在运行时写回 FrameworkConfig
defaultLanguage 不进入 SaveSystem 作为用户偏好
```

---

## 4.2 fallbackLanguage 归属

`fallbackLanguage` 归属：

```text
FrameworkConfig
```

含义：

```text
框架预设回退语言
用于当前语言缺失文本或指定语言包不完整时的首选回退节点
```

规则：

```text
fallbackLanguage 是回退策略配置，不是用户状态
fallbackLanguage 可以为空；为空时由 LocalizationSystem 只回退到 defaultLanguage
fallbackLanguage 不由 SaveSystem 持久化为用户偏好
fallbackLanguage 不在运行时被用户切换逻辑覆盖
```

---

## 4.3 currentLanguage 归属

`currentLanguage` 分为两层归属：

```text
运行时生效值：LocalizationContext
用户偏好持久化：SaveSystem
```

含义：

```text
LocalizationContext 持有当前实际生效语言
SaveSystem 可保存用户语言偏好，供下次启动恢复
FrameworkConfig 不保存 currentLanguage
```

规则：

```text
currentLanguage 是用户状态，不是框架启动配置
LocalizationSystem 启动时先读取 FrameworkConfig 的 defaultLanguage / fallbackLanguage
如果 SaveSystem 中存在合法用户语言偏好，则覆盖启动默认选择
如果用户偏好不存在、无效或当前不可用，则退回 defaultLanguage
LocalizationContext 始终反映当前真正生效的语言
```

---

## 4.4 禁止事项

禁止：

```text
把 currentLanguage 写回 FrameworkConfig
把语言包内容写入 FrameworkConfig
把 Provider 运行时实例写入 FrameworkConfig
让 SaveSystem 持有 defaultLanguage 或 fallbackLanguage 的配置权威
把 LocalizationContext 直接持久化到 SaveSystem
```

---

# 五、配置依赖冻结

LocalizationSystem 当前只允许依赖以下轻量配置：

```text
defaultLanguage
fallbackLanguage
enableRuntimeSwitch
enableMissingKeyLog
showMissingKeyPlaceholder
preferredProviderProfileId
```

说明：

```text
FrameworkConfig 可以声明默认 Provider Profile 标识
FrameworkConfig 不持有运行时 Provider 对象
语言包路径、Provider 实例、缓存和上下文由 LocalizationSystem Runtime 自身管理
```

---

# 六、ILocalizationService 方法面冻结

建议冻结以下 API：

```csharp
public interface ILocalizationService
{
    string GetText(LocalizationKey key);

    bool TryGetText(
        LocalizationKey key,
        out string text,
        out LocalizationQueryStatus status);

    string GetFormattedText(
        LocalizationKey key,
        LocalizationArguments arguments);

    LocalizationChangeResult SetLanguage(
        LanguageCode language,
        bool persistPreference = true);

    LanguageCode GetCurrentLanguage();

    IReadOnlyList<LanguageCode> GetAvailableLanguages();

    LocalizationContext GetCurrentContext();

    event Action<LanguageChangedEvent> LanguageChanged;
}
```

---

## 6.1 GetText

行为：

```text
输入 LocalizationKey，返回最终可展示文本
执行当前语言查询、回退链查询和缺失占位策略
不因 Key 不存在、语言不存在或 Provider 失败而抛出运行时预期异常
返回值保证非 null
```

返回策略：

```text
成功：返回解析后的文本
缺失：返回 Missing 占位文本，例如 [Missing: ui.common.ok]
Provider 失败且无法恢复：返回 Missing 占位文本
```

适用场景：

```text
UI、日志、提示文本的直接消费路径
```

---

## 6.2 TryGetText

行为：

```text
输入 LocalizationKey，尝试解析文本
成功时返回 true，并输出 text 与 Success 状态
失败时返回 false，并输出空字符串与明确状态
不返回 Missing 占位文本
```

适用场景：

```text
需要自行决定降级策略的调用方
验证、导出、诊断和非 UI 逻辑
```

---

## 6.3 GetFormattedText

行为：

```text
先按 GetText 规则解析模板
再应用 LocalizationArguments 完成位置参数或命名参数格式化
格式化失败不抛出运行时预期异常
```

返回策略：

```text
文本解析成功且格式化成功：返回最终格式化文本
文本缺失：返回 Missing 占位文本
格式化失败：返回未格式化模板，并记录格式化诊断
```

---

## 6.4 SetLanguage

行为：

```text
请求切换当前语言
只接受稳定 LanguageCode，不接受显示名称或索引
切换成功后原子替换当前 LocalizationContext
可选择是否持久化用户偏好到 SaveSystem
```

返回策略：

```text
成功：返回 LocalizationChangeResult.Success
失败：返回具体失败状态，且保持旧语言与旧 Context
```

---

## 6.5 GetCurrentLanguage

行为：

```text
返回当前 LocalizationContext 的 CurrentLanguage
不读取 FrameworkConfig
不读取 SaveSystem 原始值
```

---

## 6.6 GetAvailableLanguages

行为：

```text
返回当前 Provider 集合在当前上下文下可解析的语言快照
返回值只读
顺序稳定
不因调用而触发 Provider 重载
```

排序规则：

```text
CurrentLanguage 优先
然后 fallbackLanguage
然后 defaultLanguage
其余按稳定比较规则排序
```

---

## 6.7 GetCurrentContext

行为：

```text
返回当前只读 LocalizationContext 快照
调用方不得修改 Context
调用方不得持有 Context 并期待其内部可变更新
```

---

## 6.8 LanguageChanged

行为：

```text
仅在语言切换成功并完成 Context 原子替换后触发
失败切换不得触发
事件只发布事实，不直接刷新 UI
```

---

# 七、核心数据结构冻结

## 7.1 LanguageCode

```csharp
public readonly struct LanguageCode
{
    public string Value { get; }
}
```

规则：

```text
使用稳定标准化语言标识
作为持久化身份与查询身份
不使用显示名称、枚举序号或列表索引
```

---

## 7.2 LocalizationKey

```csharp
public readonly struct LocalizationKey
{
    public string Value { get; }
}
```

规则：

```text
Key 与显示文本分离
Key 必须稳定
Key 支持模块作用域
```

---

## 7.3 LocalizationArguments

```csharp
public readonly struct LocalizationArguments
{
    public IReadOnlyList<object> PositionalArguments { get; }
    public IReadOnlyDictionary<string, object> NamedArguments { get; }
}
```

规则：

```text
同时支持位置参数与命名参数
调用方可以只传其中一种
LocalizationSystem 不负责单位业务语义解释
```

---

## 7.4 LocalizationContext

```csharp
public sealed class LocalizationContext
{
    public LanguageCode CurrentLanguage { get; }
    public LanguageCode DefaultLanguage { get; }
    public LanguageCode? FallbackLanguage { get; }
    public IReadOnlyList<LanguageCode> FallbackChain { get; }
    public IReadOnlyList<LanguageCode> AvailableLanguages { get; }
    public IReadOnlyList<LocalizationProviderDescriptor> Providers { get; }
    public bool RuntimeSwitchEnabled { get; }
}
```

含义：

```text
LocalizationContext 是当前 Localization 运行时状态的只读快照
它不是配置文件
它不是可变对象
它不是持久化对象
```

---

## 7.5 LocalizationProviderDescriptor

```csharp
public sealed class LocalizationProviderDescriptor
{
    public string ProviderId { get; }
    public string ProviderType { get; }
    public int Order { get; }
    public bool IsWritable { get; }
}
```

含义：

```text
Context 只暴露 Provider 描述快照
ProviderType 用于运行时诊断和日志分析
不向外暴露可变 Provider 实例
不把 Provider 生命周期控制权交给调用方
```

---

## 7.6 LocalizationChangeResult

```csharp
public readonly struct LocalizationChangeResult
{
    public bool Success { get; }
    public LocalizationChangeStatus Status { get; }
    public LanguageCode RequestedLanguage { get; }
    public LanguageCode PreviousLanguage { get; }
    public LanguageCode EffectiveLanguage { get; }
}
```

---

## 7.7 LanguageChangedEvent

```csharp
public readonly struct LanguageChangedEvent
{
    public LanguageCode PreviousLanguage { get; }
    public LanguageCode CurrentLanguage { get; }
}
```

说明：

```text
调用方如需读取当前上下文，应通过 GetCurrentContext() 获取
LanguageChangedEvent 不长期持有 LocalizationContext 快照
```

---

# 八、LocalizationContext 生命周期冻结

LocalizationContext 生命周期选择：

```text
A：不可变快照，由 LocalizationService 独占创建与替换
```

---

## 8.1 创建时机

```text
LocalizationService 初始化完成后创建首个 Active Context
每次 SetLanguage 成功后创建新 Context
Provider 集合发生受支持的重建并成功提交后创建新 Context
```

---

## 8.2 替换规则

```text
Context 替换必须原子完成
不得先改 CurrentLanguage 再异步补 Context
不得发布半更新状态
旧 Context 在新 Context 提交前保持有效
```

---

## 8.3 销毁边界

```text
Service Shutdown 后 Active Context 失效
已发出的旧 Context 作为历史快照可读，但不得再被当作活动状态使用
调用方不得缓存 Context 作为未来真相来源
```

---

## 8.4 Provider 集合边界

```text
Provider 实例由 LocalizationService 持有和管理
LocalizationContext 只保存 ProviderDescriptor 快照
调用方不能通过 Context 增删 Provider
Provider 集合顺序一旦进入 Active Context，在该 Context 生命周期内保持稳定
```

---

# 九、错误模型冻结

## 9.1 查询状态

```csharp
public enum LocalizationQueryStatus
{
    Success = 0,
    MissingKey = 1,
    MissingLanguage = 2,
    ProviderFailure = 3
}
```

---

## 9.2 语言切换状态

```csharp
public enum LocalizationChangeStatus
{
    Success = 0,
    RuntimeSwitchDisabled = 1,
    LanguageNotAvailable = 2,
    ProviderFailure = 3,
    PersistenceFailure = 4
}
```

---

## 9.3 Key 不存在

定义：

```text
当前语言与整个回退链中都找不到目标 LocalizationKey
```

返回策略：

```text
GetText：返回 Missing 占位文本
TryGetText：返回 false，status = MissingKey
GetFormattedText：返回 Missing 占位文本
不抛出预期运行时异常
可按配置记录缺失诊断
```

---

## 9.4 Language 不存在

定义：

```text
请求语言不在当前 Provider 集合可用语言快照内
或者启动恢复的用户语言偏好当前不可用
```

返回策略：

```text
启动恢复时：回退到 defaultLanguage，再构建 Active Context
SetLanguage：返回 LanguageNotAvailable，保持旧语言
查询期间若当前语言上下文异常缺失：沿回退链解析；仍失败则按 MissingLanguage 处理
TryGetText：返回 false，status = MissingLanguage
GetText：返回 Missing 占位文本
```

---

## 9.5 Provider 失败

定义：

```text
Provider 初始化、查询、切换预加载或上下文构建阶段出现可诊断失败
```

返回策略：

```text
单个 Provider 失败但其它 Provider 可恢复：继续按 Provider 顺序和回退链解析
所有可用 Provider 均失败：查询返回 ProviderFailure
GetText：返回 Missing 占位文本
TryGetText：返回 false，status = ProviderFailure
SetLanguage：返回 ProviderFailure，保持旧语言
```

---

## 9.6 Language 切换失败

定义：

```text
SetLanguage 在验证、预加载、Context 构建或持久化阶段失败
```

返回策略：

```text
返回 LocalizationChangeResult，Success = false
EffectiveLanguage 保持 PreviousLanguage
不发布 LanguageChanged
不暴露半切换状态
```

---

## 9.7 异常原则

原则：

```text
缺失 Key、缺失语言、Provider 不可用、切换失败属于预期运行时结果，使用结果模型表达
参数为 null、非法空 LanguageCode、非法空 LocalizationKey 属于调用约束错误，可抛出参数异常
```

---

# 十、LanguageChanged 行为冻结

## 10.1 切换顺序

SetLanguage 的冻结顺序：

```text
1. 校验 RuntimeSwitchEnabled
2. 校验目标 LanguageCode 合法
3. 校验目标语言在当前 Provider 集合中可用
4. 使用目标语言构建候选 FallbackChain
5. 让 Provider 集合完成必要的预查询 / 预加载验证
6. 构建候选 LocalizationContext
7. 如要求持久化，则写入 SaveSystem 用户偏好
8. 原子替换 Active Context
9. 更新 CurrentLanguage
10. 发布 LanguageChanged
```

---

## 10.2 失败恢复

规则：

```text
第 1 到 7 步任一步失败，都不得提交新 Context
持久化失败视为切换失败
切换失败时继续保持旧语言、旧 Context、旧 AvailableLanguages
失败后不得发布 LanguageChanged
```

---

## 10.3 是否保持旧语言

选择：

```text
A：保持旧语言
```

含义：

```text
LocalizationSystem 不允许切到半成功状态
新语言只有在完整可用并成功提交后才成为 Active Language
否则继续使用旧语言
```

---

## 10.4 事件发布时机

规则：

```text
LanguageChanged 只在新 Context 已成为 Active Context 后触发
事件订阅方拿到的 Context 必须与 GetCurrentContext 返回值一致
UIThemeSystem、UISystem、FeatureModule 只能消费事件后自行刷新
LocalizationSystem 不反向驱动具体 UI 对象
```

---

# 十一、回退链冻结

默认回退链：

```text
CurrentLanguage
↓
fallbackLanguage（如果存在且不同）
↓
defaultLanguage（如果不同）
↓
Missing 占位文本
```

规则：

```text
回退链中不得出现重复语言
回退链顺序在单个 Context 生命周期内保持稳定
Provider 解析顺序先于语言回退链内部文本选择顺序
```

---

# 十二、Provider 集合冻结

Provider 集合规则：

```text
LocalizationSystem 可以组合多个 Provider
Provider 顺序由服务启动组合层决定
Active Context 内只暴露 ProviderDescriptor 快照
Provider 集合不是公开可变集合
```

禁止：

```text
调用方通过 ILocalizationService 直接注册或卸载 Provider
在语言切换过程中同时修改 Provider 集合并暴露半状态
把 FeatureModule 业务逻辑写入通用 Provider
```

---

# 十三、PlatformServiceRegistry 接入

接口方向：

```text
ILocalizationService
```

注册方向：

```csharp
PlatformServiceRegistry.Register<ILocalizationService>(localizationService);
```

获取方向：

```csharp
PlatformServiceRegistry.Get<ILocalizationService>();
PlatformServiceRegistry.TryGet<ILocalizationService>(out var localizationService);
```

---

# 十四、验证方式

R6.1 需要完成以下验证说明：

```text
检查 FrameworkConfig 与 SaveSystem 的语言归属不冲突
检查 ILocalizationService 方法面覆盖查询、格式化、切换、上下文与事件
检查 LocalizationContext 是否为只读快照，不承载持久化职责
检查错误模型是否覆盖 MissingKey、MissingLanguage、ProviderFailure、LanguageSwitchFailure
检查 LanguageChanged 是否只在成功提交后触发
检查失败切换是否保持旧语言
检查文档未进入 R6.2 代码实现
```

---

# 十五、禁止事项

R6.1 阶段禁止：

```text
进入 R6.2 Runtime Implementation
创建 LocalizationSystem Runtime 代码
直接复用旧 ByFunc/Localization 作为新 Runtime 基线
让 LocalizationSystem 持有 UI 文本组件引用
把 currentLanguage 放回 FrameworkConfig
把 LocalizationContext 当作可变单例公开
把 Provider 失败改成静默吞没且不返回状态
在失败切换后仍然发布 LanguageChanged
```

---

# 十六、阶段输出物

R6.1 应输出：

```text
Documentation/LocalizationSystemRuntimeAPIFreeze.md
Documentation/00_ByFramework_Current_Context.md 更新
Documentation/02_Roadmap.md 更新
Documentation/03_Todo.md 更新
Documentation/06_Documentation_Reading_Order.md 更新
Documentation/Changelog.md 更新
```

---

# 十七、关闭条件

R6.1 关闭条件：

```text
defaultLanguage / fallbackLanguage / currentLanguage 归属明确
ILocalizationService 方法面明确
LocalizationContext 只读快照模型明确
查询错误模型明确
语言切换错误模型明确
LanguageChanged 切换顺序与失败恢复明确
保持旧语言策略明确
PlatformServiceRegistry 接入方向明确
Current Context / Roadmap / Todo / Reading Order / Changelog 已同步
```

满足以上条件后，R6.1 可关闭。

下一阶段仍为：

```text
R6.2 LocalizationSystem Runtime Implementation
```

但本次不进入。
