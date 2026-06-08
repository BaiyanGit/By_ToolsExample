# ByFramework

ByFramework 是面向 Unity 的长期框架项目，目标支撑：

```text
工业仿真
驾驶模拟
数字孪生
培训系统
局域网项目
多机协同
设备接入
多客户定制
```

当前架构基线：

```text
Core
↓
Platform
↓
FeatureModule
↓
CustomerModules
```

---

# 当前文档入口

进入任何开发或 Codex 任务前，请优先阅读：

```text
AGENTS.md
Documentation/00_ByFramework_Current_Context.md
Documentation/01_Architecture.md
Documentation/06_Documentation_Reading_Order.md
Documentation/40_P3_Foundation_Closure_And_Runtime_Entry.md
```

当前唯一允许 Codex 执行的阶段：

```text
R1.2 PlatformServiceRegistry Runtime Implementation
```

---

# 重要说明

旧 `Core/Config/FrameworkConfig.cs` 仅保留兼容，不再作为最终配置中心。

最终配置中心属于：

```text
Platform/FrameworkConfig
```

历史工具和旧模块可以作为实现参考，但不得直接覆盖已冻结架构边界。

详细规则见：

```text
Documentation/36_ExistingToolsAssetAudit.md
Documentation/37_ToolAssetRegistry.md
```
