# Core/Config 兼容说明

> 本目录属于早期兼容配置基础设施，不是 ByFramework 最终配置中心。

---

# 当前定位

以下文件暂时保留兼容：

```text
Core/Config/FrameworkConfig.cs
Core/Config/FrameworkConfigProvider.cs
Core/Config/Resources/FrameworkConfig.asset
```

它们用于维持旧模块可运行，不得继续扩展为万能配置中心。

---

# 禁止事项

禁止继续向 `Core/Config/FrameworkConfig.cs` 添加：

```text
UI 配置
Network 配置
Download 配置
Display 配置
Resource 配置
Save 配置
License 配置
BuildProfile 配置
FeatureModule 配置
Device 配置
```

---

# 最终方向

最终配置中心属于：

```text
Platform/FrameworkConfig
```

配置规则已冻结在：

```text
Documentation/26_FrameworkConfigFoundation.md
```

后续 Runtime 实现阶段应逐步迁移旧配置，而不是继续扩展 Core/Config。
