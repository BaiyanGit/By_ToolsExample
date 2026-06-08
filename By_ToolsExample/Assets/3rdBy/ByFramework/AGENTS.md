# ByFramework Agent Guide

本文件用于约束 AI Agent 或自动化协作者在 ByFramework 中的工作方式。

---

# 架构执行规则

Codex 不负责重新设计 ByFramework 架构。

Codex 只负责根据 Documentation 中已经冻结的架构文档执行代码实现、文档同步和验证工具编写。

---

# 文档优先级

进入任何任务前，必须按顺序阅读：

```text
Documentation/00_ByFramework_Current_Context.md
Documentation/01_Architecture.md
Documentation/06_Documentation_Reading_Order.md
Documentation/40_P3_Foundation_Closure_And_Runtime_Entry.md
Documentation/02_Roadmap.md
Documentation/03_Todo.md
Documentation/04_Changelog.md
```

如果是当前 R1.2 阶段，还必须阅读：

```text
Documentation/50_PlatformServiceRegistryRuntimeAPIFreeze.md
Documentation/51_PlatformServiceRegistryRuntimeImplementation.md
```

如发现文档之间存在冲突，Codex 必须停止实现并报告冲突，不允许自行推断或重设计。

---

# 禁止事项

禁止在未经确认的情况下：

```text
回退已完成阶段
重审 P0、P1、P2
修改 FrameworkEntry 生命周期链
把 FeatureModule 业务逻辑写入 Platform
让 Platform 依赖 FeatureModule
把 NetworkCore 写成依赖 ByFramework 或 UnityEngine
提前实现尚未冻结的模块
一次实现多个模块
扩展 Core/Config/FrameworkConfig.cs 为万能配置中心
```

---

# 当前允许阶段

当前唯一允许阶段：

```text
R1.2 PlatformServiceRegistry Runtime Implementation
```

不允许进入：

```text
FrameworkConfig Runtime
SaveSystem Runtime
ResourceSystem Runtime
NetworkSystem Runtime
FeatureModule Implementation
```

---

# 工作前

* 修改代码前先阅读 `Documentation/05_CodingStandard.md`。
* 不要根据空白或缺失文档自行做架构假设。
* 先检查现有模块，不重复实现已有系统。
* 若发现旧代码和 Foundation 文档冲突，以 Foundation 文档为准，并报告冲突。

---

# 修改原则

* 遵循 `Documentation/01_Architecture.md` 中已确定的架构决策。
* 不违背既定方向：Core 管生命线、Platform 管通用能力、FeatureModule 管业务实现。
* 不为短期需求引入与 Roadmap 冲突的临时架构。
* 保持业务代码与框架代码边界清晰。
* 不允许旧工具代码反向改变新架构。

---

# Coding Standard

Before modifying C# code, read:

```text
Documentation/05_CodingStandard.md
```

All new or modified C# files must follow the coding standard.

Required:

* File header comment
* Namespace
* XML comments for public classes, structs, interfaces, enums
* XML comments for public methods
* `[Header("说明")]` for public and private fields
* Comments for complex logic
* Do not use `[Head]`, use UnityEngine.HeaderAttribute: `[Header("说明")]`

---

# 文档同步

完成任务后必须更新：

```text
Documentation/00_ByFramework_Current_Context.md
Documentation/02_Roadmap.md
Documentation/03_Todo.md
Documentation/04_Changelog.md
```

必要时更新：

```text
Documentation/06_Documentation_Reading_Order.md
```
