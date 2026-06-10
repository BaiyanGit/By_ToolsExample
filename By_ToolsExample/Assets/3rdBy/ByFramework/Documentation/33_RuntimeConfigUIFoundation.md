# P3.19 RuntimeConfigUI Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 RuntimeConfigUI Runtime 脚本  
> 不实现配置页面、权限系统、状态监控、导入导出、备份恢复或远程维护代码  

---

# 一、阶段目标

P3.19 的目标是冻结 RuntimeConfigUI 的基础设计，为后续现场配置、运行时运维、状态查看、权限控制、插件扩展、导入导出、备份恢复和远程维护预留提供明确边界。

本阶段只允许完成：

```text
RuntimeConfigUI 定位冻结
RuntimeConfigUI 模块归属冻结
配置修改权限规则冻结
配置页面生成方式冻结
配置生效策略冻结
系统状态查看规则冻结
配置导入导出规则冻结
配置备份恢复规则冻结
插件扩展规则冻结
页面隐藏规则冻结
远程维护预留规则冻结
RuntimeConfigUI 与 FrameworkConfig / SaveSystem / UISystem / LicenseSystem / BuildProfileSystem 的关系冻结
```

本阶段不进入具体 Runtime 实现。

---

# 二、RuntimeConfigUI 定位

RuntimeConfigUI 的定位选择：

```text
D：配置中心 + 状态查看器 + 运维工具入口
```

RuntimeConfigUI 不是普通设置窗口。

最终定位为：

```text
Runtime Operations Center
运行时运维中心
```

它负责：

```text
查看配置
修改配置
导入配置
导出配置
备份配置
恢复配置
查看系统状态
查看授权状态
查看资源状态
查看网络状态
查看设备状态
查看日志状态
进入运维工具
```

---

# 三、模块归属

RuntimeConfigUI 放置位置选择：

```text
C：独立 Platform/RuntimeConfigUI
```

RuntimeConfigUI 不属于 FrameworkConfig。

RuntimeConfigUI 不属于 UISystem。

RuntimeConfigUI 也不属于 FeatureModule。

推荐目录方向：

```text
Platform/RuntimeConfigUI
├─ Runtime
├─ UI
├─ Pages
├─ Services
├─ Extensions
├─ Permission
├─ Status
└─ Backup
```

原因：

```text
RuntimeConfigUI 跨 FrameworkConfig、SaveSystem、DisplaySystem、NetworkSystem、ResourceSystem、LicenseSystem、DeviceIntegration 等多个系统。
```

---

# 四、权限规则

配置修改权限选择：

```text
D：分级权限 + 密码保护
```

建议权限等级：

```text
游客
现场人员
技术支持
开发人员
管理员
```

示例：

```text
游客：只查看状态
现场人员：修改显示配置、语言、主题
技术支持：修改网络、资源路径、设备配置
开发人员：查看调试页面
管理员：完整权限
```

危险操作必须要求权限和二次确认。

---

# 五、配置界面来源

配置界面来源选择：

```text
D：自动生成 + 自定义页面 + 插件扩展
```

RuntimeConfigUI 应支持：

```text
根据配置 Schema 自动生成页面
模块自定义复杂页面
FeatureModule / CustomerModule 注册扩展页面
权限控制页面显示
```

自动生成适合：

```text
布尔值
数字
字符串
枚举
路径
IP
端口
开关
```

自定义页面适合：

```text
DisplayProfile
License 状态
Network 状态
AssetBundle 路径
Device 状态
```

---

# 六、配置生效策略

配置修改是否立即生效选择：

```text
C：配置自己决定
```

每个配置项可声明：

```text
立即生效
重载生效
重启生效
手动应用
```

示例：

```text
语言：立即生效
主题：立即生效
网络地址：重新连接后生效
AssetBundle 路径：资源重载后生效
显示器布局：通常重启生效
License 文件：重新加载授权后生效
```

RuntimeConfigUI 只展示并触发生效流程，不擅自决定业务如何生效。

---

# 七、系统状态查看

RuntimeConfigUI 是否显示系统状态选择：

```text
C：显示完整运行状态
```

建议状态页包含：

```text
FPS
CPU
内存
GPU
当前语言
当前主题
BuildProfile
License 状态
资源状态
AssetBundle 状态
网络状态
下载状态
设备状态
日志状态
运行时间
```

状态查看不等于业务逻辑。

RuntimeConfigUI 只显示各系统提供的状态。

---

# 八、配置导入导出

配置导入导出选择：

```text
A：支持
```

支持：

```text
导出当前配置
导入配置
导出配置包
导入配置包
```

建议格式：

```text
Json
Zip
```

导入配置后必须执行校验。

导入前建议自动备份当前配置。

---

# 九、配置备份恢复

配置备份恢复选择：

```text
C：自动 + 手动
```

自动备份场景：

```text
修改配置前
导入配置前
恢复默认前
版本迁移前
```

手动备份场景：

```text
现场人员创建配置快照
技术支持导出问题现场配置
客户迁移配置
```

备份与恢复底层由 SaveSystem 负责。

RuntimeConfigUI 只提供界面入口。

---

# 十、插件扩展

RuntimeConfigUI 是否支持插件扩展选择：

```text
C：支持注册配置页 + 权限控制
```

允许以下模块注册页面：

```text
Platform 模块
FeatureModule
CustomerModule
工具模块
```

例如：

```text
VehicleSimulation 配置页
TrainingSystem 配置页
DeviceIntegration 配置页
CustomerA 配置页
```

注册页面必须声明：

```text
页面 Key
页面标题
所属模块
权限要求
BuildProfile 可见性
License 可见性
```

---

# 十一、页面隐藏机制

RuntimeConfigUI 是否允许客户隐藏页面选择：

```text
C：BuildProfile + License 控制
```

页面可见性由：

```text
BuildProfile
+
License
+
Permission
```

共同决定。

示例：

```text
客户 A 隐藏资源管理页面
客户 B 显示全部页面
普通版隐藏设备高级配置
专业版显示设备配置和 License 页面
```

---

# 十二、远程维护预留

RuntimeConfigUI 是否支持远程维护模式选择：

```text
B：预留
```

当前不实现：

```text
远程桌面
远程配置修改
远程运维服务
远程日志拉取
```

但必须预留：

```text
RemoteMaintenanceProvider
RemoteStatusProvider
RemoteLogProvider
```

未来可通过：

```text
局域网服务
HTTP
WebSocket
专用运维工具
```

接入远程维护能力。

---

# 十三、与 FrameworkConfig 的关系

FrameworkConfig 负责配置加载、合并、校验和热重载策略。

RuntimeConfigUI 负责配置展示、编辑和操作入口。

关系：

```text
RuntimeConfigUI
↓
显示 / 编辑配置
↓
FrameworkConfig
↓
加载 / 合并 / 校验配置
```

RuntimeConfigUI 不重复实现配置合并规则。

---

# 十四、与 SaveSystem 的关系

SaveSystem 负责：

```text
保存现场配置
备份配置
恢复配置
导入导出相关文件
```

RuntimeConfigUI 负责：

```text
触发保存
触发备份
触发恢复
展示操作结果
```

---

# 十五、与 UISystem 的关系

UISystem 负责 UI 展示和窗口管理。

RuntimeConfigUI 负责运维页面逻辑和页面模型。

关系：

```text
RuntimeConfigUI
↓
请求打开运维中心
↓
UISystem
↓
显示 RuntimeConfigUI 页面
```

---

# 十六、与 LicenseSystem 的关系

LicenseSystem 控制：

```text
页面是否可见
功能是否授权
运维工具是否可用
```

RuntimeConfigUI 可显示：

```text
授权状态
授权到期时间
已授权功能
未授权功能
```

但 RuntimeConfigUI 不负责授权校验规则。

---

# 十七、与 BuildProfileSystem 的关系

BuildProfileSystem 控制：

```text
默认是否包含 RuntimeConfigUI
哪些页面默认可见
客户版本默认配置
```

打包后现场人员不能修改 BuildProfile 本身，只能通过 RuntimeConfigUI 覆盖允许运行时修改的配置。

---

# 十八、与 DeviceIntegration 的关系

RuntimeConfigUI 应接入设备配置和设备状态。

支持：

```text
串口配置
PLC 配置
CAN 配置
Modbus 配置
设备启用状态
真实 / 模拟 / 回放模式
设备数据状态查看
```

视频监控和推流状态可通过未来 Media / Streaming 工具扩展接入，不属于 RuntimeConfigUI 核心职责。

---

# 十九、建议核心对象

后续实现可围绕以下对象设计。

本阶段只冻结概念，不实现代码。

```text
RuntimeConfigCenter
RuntimeConfigPage
RuntimeConfigPageDescriptor
RuntimeConfigPermission
RuntimeConfigRole
RuntimeConfigSchema
RuntimeConfigApplyMode
RuntimeConfigStatusProvider
RuntimeConfigImportExport
RuntimeConfigBackup
RuntimeConfigExtension
RemoteMaintenanceProvider
RemoteStatusProvider
RemoteLogProvider
```

---

# 二十、建议目录结构

本阶段仅冻结目录方向，不要求创建代码文件。

```text
Assets/ByFramework/Platform/RuntimeConfigUI
├─ Runtime
│  ├─ Core
│  ├─ Page
│  ├─ Permission
│  ├─ Status
│  ├─ ImportExport
│  ├─ Backup
│  ├─ Extension
│  ├─ Remote
│  └─ Service
│
└─ Documentation
```

---

# 二十一、配置文件方向

建议配置文件：

```text
StreamingAssets/ByFramework/Config/runtime_config_ui_config.json
PersistentDataPath/ByFramework/Config/runtime_config_ui_config.json
```

字段方向：

```text
enabled
defaultRole
passwordEnabled
pages
hiddenPages
permissions
enableImportExport
enableBackup
enableRemoteMaintenance
```

---

# 二十二、禁止事项

P3.19 阶段禁止：

```text
实现 RuntimeConfigUI 代码
实现配置页面
实现权限系统
实现密码系统
实现导入导出
实现备份恢复
实现远程维护
实现设备配置页面
实现状态监控页面
修改 FrameworkConfig Runtime
修改 UISystem Runtime
进入 Runtime Implementation
```

---

# 二十三、P3.19 输出物

P3.19 应输出：

```text
Documentation/33_RuntimeConfigUIFoundation.md
Documentation/00_ByFramework_Current_Context.md 更新
Documentation/02_Roadmap.md 更新
Documentation/03_Todo.md 更新
Documentation/04_Changelog.md 更新
```

不要求输出 Runtime 代码。

---

# 二十四、阶段关闭条件

P3.19 关闭条件：

```text
RuntimeConfigUI 定位明确
模块归属明确
权限规则明确
自动生成 + 自定义页面 + 插件扩展明确
配置生效策略明确
完整系统状态查看明确
导入导出明确
自动 + 手动备份明确
页面隐藏由 BuildProfile + License + Permission 控制明确
远程维护预留明确
与 FrameworkConfig / SaveSystem / UISystem / LicenseSystem / BuildProfileSystem / DeviceIntegration 的关系明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，可进入：

```text
P3.20 DeviceIntegration Foundation
```

---

# 二十五、最终结论

RuntimeConfigUI 是 ByFramework 的运行时运维中心，不是普通设置窗口。

它为现场人员、技术支持和开发人员提供运行时配置、状态查看、导入导出、备份恢复、权限控制和运维工具入口。

不得在 Foundation 阶段进入具体实现。
