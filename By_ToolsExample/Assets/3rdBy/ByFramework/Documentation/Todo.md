# ByFramework Todo

## 当前已知技术债

* EventManager 模块公开命名空间仍保留 _3rdBy.MetaFramework 兼容命名，最终收敛到 ByFramework 命名空间需要独立迁移方案
* 命名空间仍保留 _3rdBy.MetaFramework 兼容命名，后续需要单独做命名空间迁移
* CodingStandard 需要细化纯 C# 逻辑类字段注释规则，避免仅为 HeaderAttribute 引入 UnityEngine 依赖
* UIAutoCreate 的业务输出目录仍固定为 Assets/Resources/Prefab/UI 与 Assets/Scripts/UI，后续可纳入 FrameworkConfig 或独立配置
* ByFramework 当前没有顶层 Runtime 与 Samples 目录，模块级 Samples 路径需要由对应模块自行管理
* UIManager 依赖 Resources
* Singleton 需要防重复实例
* 缺少统一日志系统
* 缺少 FrameworkEntry
