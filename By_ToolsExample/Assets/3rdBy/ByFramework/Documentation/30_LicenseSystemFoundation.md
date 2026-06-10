# P3.16 LicenseSystem Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 LicenseSystem Runtime 脚本  
> 不实现授权生成、授权校验、设备指纹、加密、签名、防篡改、在线校验或授权管理工具代码  

---

# 一、阶段目标

P3.16 的目标是冻结 LicenseSystem Foundation 的基础设计，为后续离线授权、在线校验预留、设备绑定、有效期、功能授权、授权文件、授权管理工具、防篡改加固和 BuildProfile 关联提供明确边界。

本阶段只允许完成：

```text
LicenseSystem 职责冻结
LicenseSystem 边界冻结
授权用途冻结
授权模式冻结
设备绑定方向冻结
有效期规则冻结
功能授权规则冻结
License 文件格式冻结
校验失败处理规则冻结
防反编译 / 防篡改原则冻结
License 管理工具方向冻结
LicenseSystem 与 BuildProfileSystem / FrameworkConfig / SaveSystem / NetworkSystem 的关系冻结
```

本阶段不进入具体运行时代码或工具实现。

---

# 二、架构位置

LicenseSystem 属于：

```text
Platform/LicenseSystem
```

LicenseSystem 不属于 Core。

LicenseSystem 不属于 FeatureModule。

LicenseSystem 是 Platform 层的通用授权系统。

---

# 三、LicenseSystem 定位

LicenseSystem 是 ByFramework 的统一授权系统。

它负责统一管理：

```text
项目运行授权
功能授权
客户授权
有效期
设备绑定
离线授权
在线校验预留
授权文件导入
授权校验
授权状态查询
授权失败处理
防篡改加固方向
```

LicenseSystem 的核心目标是：

```text
让不同客户、不同版本、不同功能组合可以通过统一 License 机制控制，而不把授权逻辑散落在业务代码中。
```

---

# 四、License 主要用途

License 主要用途选择：

```text
D：全部支持
```

包括：

```text
控制项目是否可运行
控制功能是否可用
控制客户版本
控制有效期
控制设备绑定
控制试用版 / 正式版
```

---

# 五、授权模式

授权模式选择：

```text
C：离线为主，在线预留
```

---

## 5.1 离线授权

离线授权是主要模式。

适合：

```text
局域网项目
现场部署项目
无公网环境
客户内网环境
工业仿真项目
驾驶模拟项目
培训系统
```

离线授权应支持：

```text
导入授权文件
校验授权文件
绑定设备
检查有效期
检查功能授权
```

---

## 5.2 在线授权预留

在线授权当前不作为主要实现目标。

但应预留：

```text
在线激活
在线校验
授权服务器
授权刷新
授权撤销
```

在线能力必须是可选能力。

不得因为在线校验不可用导致离线项目无法运行，除非该项目明确选择在线授权模式。

---

# 六、设备绑定

设备绑定选择：

```text
A：需要
```

---

## 6.1 设备绑定用途

用于防止授权文件被复制到其它机器直接使用。

可绑定：

```text
机器码
硬盘信息
主板信息
CPU 信息
网卡 MAC
系统信息
自定义设备信息
```

---

## 6.2 设备指纹原则

设备指纹不应只依赖单一硬件字段。

建议方向：

```text
多字段组合
容错匹配
可配置权重
允许部分硬件变化
```

原因：

```text
现场机器可能维修
网卡可能更换
系统可能重装
硬盘可能更换
```

---

## 6.3 边界

LicenseSystem 可以生成和校验设备指纹。

但不应把具体客户业务写入设备指纹逻辑。

---

# 七、有效期

有效期选择：

```text
A：需要
```

---

## 7.1 有效期类型

支持方向：

```text
试用授权
正式授权
到期授权
永久授权
指定日期授权
指定天数授权
```

---

## 7.2 有效期检查

LicenseSystem 应支持检查：

```text
开始时间
到期时间
当前时间
剩余天数
是否过期
是否未生效
```

---

## 7.3 时间篡改风险

需要考虑现场修改系统时间的风险。

后续实现可预留：

```text
上次运行时间记录
时间回拨检测
在线时间校验预留
本地时间异常提示
```

本阶段只冻结方向，不实现代码。

---

# 八、功能授权

功能授权选择：

```text
A：需要
```

---

## 8.1 可授权功能

可授权功能包括：

```text
VR
多屏
SimulationServer
Network Server
高级资源包
客户定制模块
AssetBundle 远程更新
RuntimeConfigUI
DeviceIntegration
FeatureModule
```

---

## 8.2 功能授权查询

LicenseSystem 应提供统一查询方向：

```text
某功能是否允许使用
某功能是否过期
某功能是否被当前客户授权
```

示例功能 Key：

```text
feature.vr
feature.multi_display
feature.simulation_server
feature.network_server
feature.remote_update
feature.customer_module_a
```

---

## 8.3 边界

LicenseSystem 不执行功能业务。

例如：

```text
LicenseSystem 只回答 VR 是否授权
DisplaySystem / VRSystem 决定是否启用 VR
```

---

# 九、License 文件格式

License 文件格式选择：

```text
D：多格式支持，默认加密 JSON
```

---

## 9.1 默认格式

默认方向：

```text
加密 JSON
```

原因：

```text
结构清晰
便于工具生成
便于版本扩展
便于签名校验
不直接暴露明文授权内容
```

---

## 9.2 可扩展格式

后续可扩展：

```text
Binary
加密 Binary
远程授权令牌
客户自定义格式
```

---

## 9.3 License 文件内容方向

License 文件可包含：

```text
licenseId
customerId
customerName
projectId
productName
licenseType
issueTime
startTime
expireTime
deviceBinding
features
version
signature
hash
issuer
```

本阶段不冻结具体 JSON Schema。

---

# 十、签名与完整性校验

License 文件必须预留签名和完整性校验。

方向：

```text
数字签名
Hash 校验
授权内容防篡改
授权文件版本校验
```

授权文件不能只靠普通 JSON 字段判断。

必须通过签名或等价机制验证授权文件未被篡改。

---

# 十一、校验失败处理

License 校验失败处理选择：

```text
C：按严重级别处理
```

---

## 11.1 核心授权失败

如果核心授权失败，例如：

```text
License 文件不存在
License 文件无效
设备绑定不匹配
项目运行授权无效
核心授权过期
授权签名无效
```

处理方向：

```text
阻止项目进入正式运行
显示授权错误界面
允许导入新授权
允许查看机器码
必要时退出程序
```

---

## 11.2 功能授权失败

如果只是某个功能未授权，例如：

```text
VR 未授权
多屏未授权
Network Server 未授权
SimulationServer 未授权
高级资源包未授权
```

处理方向：

```text
禁用对应功能
记录警告
提示功能未授权
基础功能继续运行
```

---

## 11.3 试用过期

试用过期时可根据项目配置决定：

```text
禁止运行
进入只读模式
进入授权导入界面
禁用高级功能
```

---

# 十二、防反编译与防篡改原则

用户关注：

```text
防止技术人员反编译后篡改授权判断逻辑
避免直接找到限制代码后绕过
```

本系统必须在设计上预留防篡改和加固策略。

但需要明确：

```text
客户端本地授权无法做到绝对不可破解
只能通过多层校验、代码混淆、签名校验、防篡改检测、逻辑分散和在线校验预留提高破解成本
```

---

## 12.1 加固方向

后续实现应考虑：

```text
代码混淆
关键授权逻辑分散
授权结果多点校验
授权文件签名验证
设备指纹校验
Hash 校验
完整性检测
反调试检测预留
关键字符串隐藏
授权状态缓存加密
关键功能二次校验
```

---

## 12.2 禁止单点校验

禁止只在一个地方写：

```text
if (!licenseValid) return;
```

然后其它功能完全相信该结果。

应采用：

```text
启动校验
功能入口校验
关键模块校验
资源加载前校验
Server 启动前校验
高级功能使用前校验
```

---

## 12.3 授权结果缓存风险

授权结果缓存必须谨慎。

如果缓存授权状态，应考虑：

```text
加密缓存
绑定设备
绑定时间
定期重新校验
检测缓存篡改
```

---

## 12.4 反编译风险边界

LicenseSystem 不承诺：

```text
完全防破解
完全防反编译
完全不可绕过
```

但设计目标是：

```text
提高破解成本
避免简单篡改
避免直接改一个布尔值即可绕过
让授权判断分散并可校验
```

---

# 十三、License 管理工具

第 8 项用户未明确选择。

当前设计按更稳方案冻结为：

```text
C：Editor 工具和 Runtime 导入工具都需要
```

---

## 13.1 Editor 工具

Editor 工具用于开发和交付阶段。

功能方向：

```text
生成授权文件
查看授权文件
校验授权文件
生成机器码
配置功能授权
配置有效期
配置客户信息
导出授权文件
```

---

## 13.2 Runtime 导入工具

Runtime 导入工具用于现场人员。

功能方向：

```text
查看当前授权状态
查看机器码
导入授权文件
校验授权文件
显示授权到期时间
显示已授权功能
显示未授权功能
```

Runtime 导入工具可通过 RuntimeConfigUI 或独立授权界面提供。

---

## 13.3 权限边界

Runtime 工具只允许：

```text
导入授权
查看授权
查看机器码
查看功能状态
```

不允许：

```text
生成正式授权
修改授权内容
修改授权有效期
修改授权功能
```

正式授权生成应由 Editor 工具或独立授权工具完成。

---

# 十四、LicenseSystem 与 BuildProfileSystem

LicenseSystem 必须和 BuildProfileSystem 关联。

选择：

```text
A：需要
```

---

## 14.1 BuildProfile 影响 License

BuildProfile 可决定：

```text
默认授权模式
默认客户信息
默认启用功能
默认禁用功能
试用版 / 正式版
License 配置输出路径
```

---

## 14.2 正确关系

```text
BuildProfileSystem
↓
生成默认 License 配置
↓
LicenseSystem
↓
运行时授权校验
```

BuildProfile 不执行授权校验。

LicenseSystem 执行授权校验。

---

# 十五、LicenseSystem 与远程服务器

远程服务器关联选择：

```text
A：当前不需要，只预留在线校验
```

---

## 15.1 当前策略

当前以离线授权为主。

不要求实现：

```text
在线激活
在线授权服务器
在线撤销
在线续期
```

---

## 15.2 预留方向

未来可扩展：

```text
在线激活
在线校验
授权服务器
授权续期
授权撤销
客户授权管理后台
```

在线能力应通过 NetworkSystem 实现网络通信。

LicenseSystem 不直接实现底层网络。

---

# 十六、LicenseSystem 与 FrameworkConfig

FrameworkConfig 提供 License 配置。

例如：

```text
licenseMode
licenseFilePath
enableOnlineCheck
licenseServerUrl
allowTrial
showLicenseUI
```

LicenseSystem 消费最终配置。

FrameworkConfig 不负责授权校验。

---

# 十七、LicenseSystem 与 SaveSystem

SaveSystem 可保存：

```text
授权导入记录
授权状态缓存
上次运行时间
机器码缓存
```

敏感数据必须考虑加密。

LicenseSystem 不应把敏感授权数据明文保存。

---

# 十八、LicenseSystem 与 NetworkSystem

NetworkSystem 用于未来在线校验。

关系：

```text
LicenseSystem
↓
请求在线校验
↓
NetworkSystem
↓
License Server
```

LicenseSystem 不直接实现 HTTP / Socket。

---

# 十九、LicenseSystem 与 ResourceSystem

ResourceSystem 可在加载某些资源组前询问 LicenseSystem。

例如：

```text
VR 资源包
高级车辆资源包
客户定制资源包
SimulationServer 资源包
```

ResourceSystem 不决定授权规则。

LicenseSystem 返回授权结果。

---

# 二十、LicenseSystem 与 DisplaySystem

DisplaySystem 可询问 LicenseSystem：

```text
多屏是否授权
VR 是否授权
高级驾驶舱布局是否授权
```

DisplaySystem 不判断授权规则。

---

# 二十一、LicenseSystem 与 NetworkSystem Server

NetworkSystem Server 启用前可询问 LicenseSystem：

```text
Network Server 是否授权
SimulationServer 基础网络能力是否授权
```

未授权时：

```text
禁止启动对应 Server
基础客户端功能可继续
```

---

# 二十二、LicenseSystem 与 FeatureModule

FeatureModule 可询问 LicenseSystem：

```text
某业务模块是否授权
某客户模块是否授权
某训练功能是否授权
```

但 FeatureModule 不应复制一套授权逻辑。

授权判断统一通过 LicenseSystem。

---

# 二十三、LicenseFeature

LicenseFeature 是功能授权标识。

示例：

```text
feature.vr
feature.multi_display
feature.simulation_server
feature.network_server
feature.remote_update
feature.assetbundle_advanced
feature.customer_module_a
feature.device_integration
```

---

# 二十四、LicenseResult

LicenseSystem 应提供明确授权结果方向。

结果类型：

```text
Valid
Invalid
Expired
NotStarted
DeviceMismatch
SignatureInvalid
FeatureNotLicensed
LicenseFileMissing
LicenseFileCorrupted
OnlineCheckFailed
```

结果应包含：

```text
是否允许继续
错误码
错误信息
授权功能
到期时间
客户信息
```

---

# 二十五、建议核心对象

后续实现可围绕以下对象设计。

本阶段只冻结概念，不实现代码。

```text
LicenseInfo
LicenseFile
LicenseFeature
LicenseResult
LicenseMode
LicenseValidator
LicenseProvider
LicenseSignatureVerifier
LicenseDeviceBinder
LicenseDeviceFingerprint
LicenseTimeValidator
LicenseFeatureValidator
LicenseImportResult
LicenseSecurityPolicy
LicenseAntiTamperPolicy
LicenseConfig
```

---

# 二十六、建议目录结构

本阶段仅冻结目录方向，不要求创建代码文件。

```text
Assets/ByFramework/Platform/LicenseSystem
├─ Runtime
│  ├─ Core
│  │  ├─ LicenseInfo
│  │  ├─ LicenseFile
│  │  ├─ LicenseFeature
│  │  ├─ LicenseResult
│  │  ├─ LicenseMode
│  │  └─ LicenseConfig
│  │
│  ├─ Validation
│  │  ├─ LicenseValidator
│  │  ├─ LicenseSignatureVerifier
│  │  ├─ LicenseTimeValidator
│  │  ├─ LicenseFeatureValidator
│  │  └─ LicenseDeviceBinder
│  │
│  ├─ Security
│  │  ├─ LicenseDeviceFingerprint
│  │  ├─ LicenseSecurityPolicy
│  │  └─ LicenseAntiTamperPolicy
│  │
│  ├─ Import
│  │  └─ LicenseImportResult
│  │
│  └─ Service
│     └─ ILicenseService
│
└─ Editor
   ├─ LicenseGeneratorWindow
   ├─ LicenseViewerWindow
   ├─ LicenseValidationWindow
   └─ LicenseFeatureConfigWindow
```

---

# 二十七、配置文件方向

建议配置文件：

```text
StreamingAssets/ByFramework/Config/license_config.json
PersistentDataPath/ByFramework/Config/license_config.json
```

建议授权文件位置：

```text
StreamingAssets/ByFramework/License/
PersistentDataPath/ByFramework/License/
```

字段方向：

```text
licenseMode
licenseFilePath
enableOfflineLicense
enableOnlineCheck
licenseServerUrl
allowTrial
showLicenseUI
enableDeviceBinding
enableFeatureLicense
enableExpireCheck
enableAntiTamper
```

本阶段不冻结具体 JSON Schema。

---

# 二十八、PlatformServiceRegistry 接入方向

LicenseSystem 后续可注册为 Platform 服务。

接口方向：

```text
ILicenseService
```

注册方向：

```text
PlatformServiceRegistry.Register<ILicenseService>(licenseService)
```

获取方向：

```text
PlatformServiceRegistry.Get<ILicenseService>()
PlatformServiceRegistry.TryGet<ILicenseService>(out licenseService)
```

本阶段不实现注册代码，只冻结接入方向。

---

# 二十九、禁止事项

P3.16 阶段禁止：

```text
实现 LicenseSystem Runtime 服务
实现授权文件生成
实现授权文件校验
实现设备指纹算法
实现加密算法
实现签名算法
实现防篡改算法
实现在线校验
实现 License EditorWindow
实现 Runtime 授权导入界面
实现授权缓存
实现反调试检测
修改 FrameworkEntry 生命周期
扩展 Core/Config/FrameworkConfig.cs
进入 LicenseSystem Implementation
进入 NetworkSystem Implementation
进入 BuildProfileSystem Implementation
```

---

# 三十、P3.16 输出物

P3.16 应输出：

```text
Documentation/30_LicenseSystemFoundation.md
Documentation/00_ByFramework_Current_Context.md 更新
02_Documentation/02_Roadmap.md 更新
03_Documentation/03_Todo.md 更新
Documentation/04_Changelog.md 更新
```

不要求输出 Runtime 或 Editor 代码。

---

# 三十一、阶段关闭条件

P3.16 关闭条件：

```text
LicenseSystem 职责明确
LicenseSystem 边界明确
项目运行授权 / 功能授权 / 客户授权 / 有效期 / 设备绑定全部支持方向明确
离线为主、在线预留规则明确
设备绑定方向明确
有效期规则明确
功能授权规则明确
多格式支持、默认加密 JSON 规则明确
核心授权失败退出或阻止运行、功能授权失败禁用功能的分级处理明确
防反编译 / 防篡改加固原则明确
Editor 授权工具与 Runtime 导入工具方向明确
LicenseSystem 与 BuildProfileSystem / FrameworkConfig / SaveSystem / NetworkSystem / ResourceSystem / DisplaySystem / FeatureModule 的关系明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，P3.16 可关闭。

下一阶段建议进入：

```text
P3.17 FeatureModule Boundary Foundation
```

或进入：

```text
P3 Foundation 总结与 Runtime Implementation 入口设计
```

---

# 三十二、最终结论

P3.16 LicenseSystem Foundation 是设计冻结阶段。

它只确定：

```text
LicenseSystem 是什么
LicenseSystem 管什么
LicenseSystem 不管什么
授权如何分类
离线授权如何定位
在线授权如何预留
设备绑定如何定位
有效期如何定位
功能授权如何定位
授权失败如何处理
如何提高反编译和篡改成本
授权工具如何定位
后续 Runtime / Editor 实现应遵守什么边界
```

不得在本阶段进入具体 Runtime、Editor、加密、签名、防篡改或在线校验实现。
