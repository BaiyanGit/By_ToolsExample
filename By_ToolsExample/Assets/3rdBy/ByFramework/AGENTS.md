# ByFramework Agent Guide

本文件用于约束 AI Agent 或自动化协作者在 ByFramework 中的工作方式。

## 工作前

* 修改代码前先阅读 `Documentation/Architecture.md`、`Documentation/Roadmap.md`、`Documentation/Changelog.md`。
* 不要根据空白或缺失文档自行做架构假设。
* 先检查现有模块，不重复实现已有系统。

## 修改原则

* 遵循 `Documentation/Architecture.md` 中已确定的架构决策。
* 不违背既定方向：EventManager 是最终唯一事件入口，旧事件中心兼容层不再保留。
* 不为短期需求引入与 Roadmap 冲突的临时架构。
* 保持业务代码与框架代码边界清晰。

## Coding Standard

Before modifying C# code, read:

`Documentation/CodingStandard.md`

All new or modified C# files must follow the coding standard.

Required:

* File header comment
* Namespace
* XML comments for public classes, structs, interfaces, enums
* XML comments for public methods
* `[Header("说明")]` for public and private fields
* Comments for complex logic
* Do not use `[Head]`, use UnityEngine.HeaderAttribute: `[Header("说明")]`

## 文档同步

* 修改后更新 `Documentation/Changelog.md`。
* 完成任务后更新 `Documentation/Roadmap.md` 的进度。
* 发现新的技术债，记录到 `Documentation/Todo.md`。
