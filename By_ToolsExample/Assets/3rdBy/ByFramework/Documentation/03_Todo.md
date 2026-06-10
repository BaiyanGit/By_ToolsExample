# ByFramework Todo

> 当前 Todo 只保留本轮修复焦点与禁止越界项。

---

# 当前任务

```text
Post-R9 Compile Fix
```

目标：

```text
先修复已发现 C# 编译错误，再进入 R10.2 NetworkSystem Runtime Implementation。
```

---

# 修复范围

```text
FrameworkConfig:
- JsonFileConfigProvider 可访问性必须不低于 public 子类。

InputSystem:
- MockInputBackend 不作为公开 Runtime API 暴露。
- MockInputBackend.SetResolvedState 必须满足 IInputBackend 实现要求。
- InputBackendSignal 不得通过 public class 的 public event 造成可访问性冲突。
- InputService public 构造函数不得暴露 internal MockInputBackend。
```

---

# 当前禁止

```text
修改 R7/R8/R9 Freeze
扩展 IInputService / IDisplayService / IUIService
修改已关闭 Runtime API 面
进入 R10.2 Runtime Implementation
实现 NetworkSystem Runtime
引入 FeatureModule 业务逻辑
顺手重构
```

---

# 验收条件

```text
1. 已知 CS0060 / CS0737 / CS7025 / CS0051 类访问性错误修复。
2. 文档状态同步到 R9 CLOSED / R10.1 CLOSED / R10.2 Pending。
3. 用户在 Unity 中确认无新增编译错误后，允许进入 R10.2。
```
