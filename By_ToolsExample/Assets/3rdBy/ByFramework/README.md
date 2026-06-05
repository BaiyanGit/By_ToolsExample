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

## 使用方式

1. 将 ByFramework 目录放入 Unity 工程的 `Assets` 下任意位置，Editor 工具会通过框架标记文件自动定位根目录。
2. 业务代码按需引用 ByFramework 中的模块。
3. UI 当前通过 `Resources/Prefab/UI` 加载，后续会引入 `IUILoader` 抽象。
4. 新增或修改框架能力前，请先确认已有模块是否已经提供同类能力。

## 文档入口

开发者请优先阅读：

* [AGENTS.md](AGENTS.md)
* [Documentation/Architecture.md](Documentation/Architecture.md)
* [Documentation/Roadmap.md](Documentation/Roadmap.md)

其他文档：

* [Documentation/CodingStandard.md](Documentation/CodingStandard.md)：编码规范
* [Documentation/Architecture.md](Documentation/Architecture.md)：架构设计
* [Documentation/Roadmap.md](Documentation/Roadmap.md)：路线图
* [Documentation/Changelog.md](Documentation/Changelog.md)：更新记录
* [Documentation/Todo.md](Documentation/Todo.md)：技术债记录
