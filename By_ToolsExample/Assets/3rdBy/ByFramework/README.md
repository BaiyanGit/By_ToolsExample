# ByFramework

ByFramework 是一个面向 Unity 项目的轻量框架集合，用于沉淀通用基础能力、编辑器工具协作规范和后续框架化演进计划。当前阶段重点是完成从 MetaFramework 到 ByFramework 的目录与文档整理，并逐步统一事件、入口、配置和资源加载体系。

## 当前模块

* EventManager：事件系统，作为后续唯一事件入口。
* FSM：有限状态机。
* UI：基于 Resources 的 UI 管理、UI 自动生成、列表、Tab、红点等工具。
* Singleton：通用单例模板。
* Http：HTTP 请求、下载与序列化辅助。
* Socket：Socket 通信相关脚本与示例。
* Guide：引导系统。
* Extension：通用扩展与辅助组件。
* SoundManager：声音管理相关能力。
* Core：FrameworkEntry 统一启动入口与 FrameworkConfig 配置基础设施。

## 使用方式

1. 将 ByFramework 目录放入 Unity 工程的 `Assets` 下任意位置，Editor 工具会通过框架标记文件自动定位根目录。
2. 业务代码按需引用 ByFramework 中的模块。
3. UI 当前通过 `Resources/Prefab/UI` 加载，后续会引入 `IUILoader` 抽象。
4. 新增或修改框架能力前，请先确认已有模块是否已经提供同类能力。

## 配置系统

Runtime 配置通过 `FrameworkConfigProvider.Config` 统一访问，默认配置资源位于 `Core/Config/Resources/FrameworkConfig.asset`。

Editor 工具配置使用独立的 `Editor/Config/FrameworkEditorConfig.asset`。第一阶段仅建立配置基础设施，现有模块与 Editor 工具仍保持原有配置读取行为。

配置结构与后续接入规划参见 [Core/Config/README.md](Core/Config/README.md)。

## 文档入口

开发者请优先阅读：

* [AGENTS.md](AGENTS.md)
* [Documentation/01_Architecture.md](Documentation/01_Architecture.md)
* [Documentation/02_Roadmap.md](Documentation/02_Roadmap.md)

其他文档：

* [Documentation/05_CodingStandard.md](Documentation/05_CodingStandard.md)：编码规范
* [Documentation/01_Architecture.md](Documentation/01_Architecture.md)：架构设计
* [Documentation/02_Roadmap.md](Documentation/02_Roadmap.md)：路线图
* [Documentation/04_Changelog.md](Documentation/04_Changelog.md)：更新记录
* [Documentation/03_Todo.md](Documentation/03_Todo.md)：技术债记录
