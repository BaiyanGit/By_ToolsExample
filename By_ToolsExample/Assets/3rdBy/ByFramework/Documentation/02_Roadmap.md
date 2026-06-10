# ByFramework Roadmap

> 当前 Roadmap 只保留 Foundation 关闭状态、Runtime 阶段状态与当前阻塞项。

---

# P0 ~ P3

```text
P0 核心整改：完成
P1 稳定性审计：完成
P2 平台化总体设计：完成
P3 Foundation：100% CLOSED
```

---

# 已关闭 Runtime 阶段

```text
[x] R1 PlatformServiceRegistry CLOSED
[x] R2 FrameworkConfig CLOSED
[x] R3 SaveSystem CLOSED
[x] R4 ResourceSystem CLOSED
[x] R5 AssetBundle CLOSED
[x] R6 LocalizationSystem CLOSED
[x] R7 InputSystem CLOSED
[x] R8 DisplaySystem CLOSED
[x] R9 UISystem CLOSED
```

---

# 当前阶段

```text
[x] R10 预审 PASS
[x] R10.1 NetworkSystem Runtime API Freeze PASS / CLOSED
[ ] R10.2 NetworkSystem Runtime Implementation
```

R10.2 当前暂缓，原因：

```text
需要先完成 Post-R9 编译错误修复并由用户在 Unity 中确认编译通过。
```

---

# 下一步

```text
1. 合入编译错误修复
2. Unity 编译验证
3. 启动 R10.2 NetworkSystem Runtime Implementation
```
