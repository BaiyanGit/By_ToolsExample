# Documentation Reading Order

> 本文定义 Documentation 的阅读顺序，Codex / AI Agent 必须按此顺序读取。

---

# 00 入口文档

```text
00_ByFramework_Current_Context.md
01_Architecture.md
02_Roadmap.md
03_Todo.md
04_Changelog.md
05_CodingStandard.md
06_Documentation_Reading_Order.md
07_Existing_Tools_Asset_Audit.md
08_Documentation_Review_Report.md
```

---

# 10 Core 历史冻结

```text
10_SingletonAudit.md
11_SingletonSafetyAudit.md
12_FrameworkEntryPhase2Design.md
13_EventManagerIntegrationReview.md
14_FSMManagerIntegrationReview.md
15_FrameworkEntryPhase2AEventManagerImplementation.md
16_FrameworkEntryPhase2AFSMManagerImplementation.md
17_FrameworkEntryPhase2AClosureReview.md
18_CoreEarlyLifecycleUnityVerification.md
```

---

# 20 P3 Foundation

```text
20_InputSystemFoundation.md
21_UISystemFoundation.md
22_DisplaySystemFoundation.md
23_ResourceSystemFoundation.md
24_AssetBundleFoundation.md
25_SaveSystemFoundation.md
26_FrameworkConfigFoundation.md
27_LocalizationSystemFoundation.md
28_NetworkSystemFoundation.md
29_BuildProfileSystemFoundation.md
30_LicenseSystemFoundation.md
31_FeatureModuleBoundaryFoundation.md
32_EditorToolsFoundation.md
33_RuntimeConfigUIFoundation.md
34_DeviceIntegrationFoundation.md
35_SamplesFoundation.md
36_ExistingToolsAssetAudit.md
37_ToolAssetRegistry.md
```

---

# 40 Runtime 入口

```text
40_P3_Foundation_Closure_And_Runtime_Entry.md
RuntimeDevelopmentRules.md
```

---

# 当前 Runtime 阶段

当前必须先读：

```text
00_ByFramework_Current_Context.md
02_Roadmap.md
03_Todo.md
04_Changelog.md
RuntimeDevelopmentRules.md
NetworkSystemFoundation.md
NetworkSystemRuntimeAPIFreeze.md
```

---

# 当前允许执行阶段

```text
Post-R9 Compile Fix
```

完成 Unity 编译验证后，才允许进入：

```text
R10.2 NetworkSystem Runtime Implementation
```
