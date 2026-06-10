# ByFramework 当前项目上下文

> 快照日期：2026-06-10  
> 本文是 Codex / AI Agent 的最高优先级上下文入口。  
> 当前阶段优先级：先修复已发现编译错误，再进入 R10.2。

---

# 一、项目定位

ByFramework 是面向 Unity 的通用平台化框架，长期支持：

```text
Windows
Linux
VR
多屏
多语言
BuildProfile
License
AssetBundle
SimulationServer
多机协同
设备接入
Runtime 运维配置
```

---

# 二、架构主线

```text
Core
-> Platform
-> FeatureModule
-> CustomerModules
```

依赖规则：

```text
Core 不依赖 Platform
Platform 不依赖 FeatureModule
FeatureModule 可以依赖 Platform + Core
```

架构红线：

```text
禁止 Platform -> FeatureModule
禁止 NetworkCore 依赖 ByFramework
禁止 NetworkCore 依赖 UnityEngine
禁止 NetworkCore 依赖 PlatformServiceRegistry / FrameworkConfig / SaveSystem / ResourceSystem / UISystem
禁止扩展 Core/Config/FrameworkConfig.cs
禁止未确认修改 FrameworkEntry 生命周期
SimulationSync 不属于 NetworkSystem
```

---

# 三、已关闭阶段

```text
R1 PlatformServiceRegistry CLOSED
R2 FrameworkConfig CLOSED
R3 SaveSystem CLOSED
R4 ResourceSystem CLOSED
R5 AssetBundle CLOSED
R6 LocalizationSystem CLOSED
R7 InputSystem CLOSED
R8 DisplaySystem CLOSED
R9 UISystem CLOSED
```

---

# 四、当前阶段状态

```text
R10 预审 PASS
R10.1 NetworkSystem Runtime API Freeze PASS / CLOSED
R10.2 NetworkSystem Runtime Implementation 尚未开始
```

当前必须先完成：

```text
Post-R9 编译错误修复与源码一致性清理
```

已知修复范围：

```text
FrameworkConfig JsonFileConfigProvider 可访问性
InputSystem MockInputBackend / IInputBackend / InputBackendSignal 可访问性一致性
InputService 构造函数不暴露 internal MockInputBackend
```

修复完成并由用户确认 Unity 编译通过后，才允许进入：

```text
R10.2 NetworkSystem Runtime Implementation
```

---

# 五、当前允许事项

允许：

```text
修复纯 C# 编译错误
修复访问修饰符不一致
同步文档状态
补充 NetworkSystemRuntimeAPIFreeze.md
```

禁止：

```text
修改 R7/R8/R9 已冻结 Runtime API
扩展 IInputService / IDisplayService / IUIService
重设计已关闭架构
进入 R10.2 实现 NetworkSystem Runtime
引入业务逻辑
让 Platform 依赖 FeatureModule
```

---

# 六、用户验证规则

由于本地 Unity Licensing 与运行环境限制：

Codex / AI Agent 不负责：

```text
Unity Licensing 排查
Unity Test Runner 实际执行证明
PlayMode 实际运行验证
ZIP 打包
```

用户负责：

```text
Unity 实际运行验证
测试执行验证
ZIP 导出
```

架构审查以源码、Freeze 一致性与静态编译级检查为准。

---

# 七、下一步

```text
1. 使用修复后的 ZIP 覆盖项目
2. 用户在 Unity 中验证是否仍有编译错误
3. 若无编译错误，进入 R10.2 NetworkSystem Runtime Implementation
```
