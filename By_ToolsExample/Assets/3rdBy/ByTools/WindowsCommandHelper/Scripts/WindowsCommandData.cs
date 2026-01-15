namespace _3rdBy.ByTools.WindowsCommandHelper.Scripts
{
    using System.Collections.Generic;

    /// <summary>
    /// Windows 命令数据文件(完整分类版)
    /// 可用于 Unity 命令面板、帮助窗口或直接执行
    /// </summary>
    public static class WindowsCommandData
    {
        public class CommandInfo
        {
            public string command;
            public string fullName;
            public string fullNameChina;
            public string description;
            public string syntax;
            public string example;
            public string runExample;
            public string input;
        }

        public static readonly List<CommandInfo> Commands = new()
        {
            #region 🎏语法参数

            new CommandInfo
            {
                command       = "/s",
                fullName      = "Shutdown",
                fullNameChina = "[语法] 关闭计算机",
                description   = "关闭计算机。",
            },
            new CommandInfo
            {
                command       = "/r",
                fullName      = "Restart",
                fullNameChina = "[语法] 重新启动计算机",
                description   = "重启计算机。",
            },
            new CommandInfo
            {
                command       = "/l",
                fullName      = "Log off",
                fullNameChina = "[语法] 注销当前用户",
                description   = "注销当前用户。不能与 /m 或 /d 一起使用。",
            },
            new CommandInfo
            {
                command       = "/t",
                fullName      = "Time Delay",
                fullNameChina = "[语法] 设置关机或重启延迟时间",
                description   = "设置关机或重启延迟时间，单位为秒。默认值为 30 秒。",
            },
            new CommandInfo
            {
                command       = "/f",
                fullName      = "Force",
                fullNameChina = "[语法] 强制关闭正在运行的程序",
                description   = "强制关闭正在运行的程序，不保存数据。",
            },
            new CommandInfo
            {
                command       = "/m",
                fullName      = "Machine",
                fullNameChina = "[语法] 关闭远程计算机",
                description   = "指定远程计算机，必须加 \\\\ 例如：\\\\ComputerName。",
            },
            new CommandInfo
            {
                command       = "/a",
                fullName      = "Abort",
                fullNameChina = "[语法] 取消关机",
                description   = "取消关机，必须在超时期间内使用。",
            },
            new CommandInfo
            {
                command       = "/p",
                fullName      = "Power off",
                fullNameChina = "[语法] 关闭本地计算机",
                description   = "立即关闭本地计算机，无延迟或警告。",
            },
            new CommandInfo
            {
                command       = "/h",
                fullName      = "Hibernate",
                fullNameChina = "[语法] 将计算机置于休眠状态",
                description   = "将计算机置于休眠状态。可以与 /f 一起使用。",
            },
            new CommandInfo
            {
                command       = "/e",
                fullName      = "Event",
                fullNameChina = "[语法] 记录意外关机的原因",
                description   = "记录意外关机的原因，必须在关机前使用。",
            },
            new CommandInfo
            {
                command       = "/i",
                fullName      = "GUI",
                fullNameChina = "[语法] 显示远程关机图形界面",
                description   = "显示远程关机图形界面，必须是第一个参数。",
            },
            new CommandInfo
            {
                command       = "/g",
                fullName      = "Graceful Restart",
                fullNameChina = "[语法] 重新启动计算机并关闭所有应用程序",
                description   = "关机并重启计算机，重启后重新启动已注册的应用程序。",
            },
            new CommandInfo
            {
                command       = "/o",
                fullName      = "Advanced Boot Options",
                fullNameChina = "[语法] 显示高级启动选项菜单",
                description   = "重新启动计算机并进入高级启动选项菜单。",
            },
            new CommandInfo
            {
                command       = "/f",
                fullName      = "Force",
                fullNameChina = "[语法] 强制关闭正在运行的程序",
                description   = "强制关闭正在运行的程序，不保存数据。",
            },
            new CommandInfo
            {
                command       = "/d",
                fullName      = "Reason",
                fullNameChina = "[语法] 指定关机或重启的原因",
                description   = "指定关机或重启的原因，格式为 /d [p|u:]xx:yy。",
            },
            new CommandInfo
            {
                command       = "/c",
                fullName      = "Comment",
                fullNameChina = "[语法] 添加自定义注释",
                description   = "添加自定义注释，最多 512 个字符。",
            },
            new CommandInfo
            {
                command       = "/?",
                fullName      = "Help",
                fullNameChina = "[语法] 显示帮助信息",
                description   = "显示帮助信息，包括已定义的主要和次要原因列表。",
            },
            new CommandInfo
            {
                command       = ">nul、2>nul、组合>nul 2>&1",
                fullName      = ">nul",
                fullNameChina = "[语法] 输入输出重定向提示符",
                description =
                    "taskkill /f /im vrserver.exe >nul\n" +
                    "taskkill /f /im vrserver.exe 2>nul\n" +
                    "taskkill /f /im vrserver.exe >nul 2>&1\n" +
                    ">nul      (等价于 1>nul)将命令的标准输出(stdout)重定向到nul设备(即空设备，相当于丢弃输出),会执行任务终止,并且不会在屏幕上显示任何标准输出消息(比如成功终止的信息)。\n" +
                    "2>nul     将命令的标准错误(stderr)重定向到nul设备，从而丢弃错误消息。如果命令执行出错(比如要终止的进程不存在)，错误信息将不会显示。\n" +
                    ">nul 2>&1 这个组合表示将标准输出重定向到nul，并且将标准错误重定向到标准输出(即同样到nul)。这样，无论是正常输出还是错误信息都不会显示，批处理中，2>&1的意思是将标准错误(文件描述符2)重定向到标准输出(文件描述符1)的同一位置。",
            },

            #endregion

            #region 🧩 基础系统与通用命令

            new CommandInfo
            {
                command       = "cls",
                fullName      = "Clear Screen",
                fullNameChina = "[基础] 清除命令提示符屏幕",
                description   = "清除命令提示符屏幕。",
                syntax        = "cls",
                example       = "cls                                    - 清除屏幕内容",
                runExample    = "cls"
            },
            new CommandInfo
            {
                command       = "echo",
                fullName      = "Echo Text",
                fullNameChina = "[基础] 显示文本",
                description   = "显示消息或打开/关闭命令回显",
                syntax        = "echo [on|off] [message]",
                example = "echo Hello World                        - 显示 'Hello World'\n" +
                          "echo off                                - 关闭命令回显\n" +
                          "echo on                                 - 打开命令回显",
                runExample = "echo Hello World"
            },
            new CommandInfo
            {
                command       = "set",
                fullName      = "Set Environment Variable",
                fullNameChina = "[基础] 显示、设置或删除环境变量",
                description   = "显示、设置或删除环境变量。",
                syntax        = "set [Variable=[Value]]",
                example = "set PATH=C:\\MyTools;%PATH%           - 设置 PATH 环境变量\n" +
                          "set                                   - 显示所有环境变量\n" +
                          "set MYVAR=                             - 删除 MYVAR 变量",
                runExample = "set PATH=C:\\MyTools;%PATH%"
            },
            new CommandInfo
            {
                command       = "pause",
                fullName      = "Pause Command",
                fullNameChina = "[基础] 暂停批处理文件执行并显示提示",
                description   = "暂停批处理文件的执行并显示提示。",
                syntax        = "pause",
                example       = "pause                                  - 暂停执行，显示 'Press any key to continue...'",
                runExample    = "pause"
            },
            new CommandInfo
            {
                command       = "exit",
                fullName      = "Exit CMD",
                fullNameChina = "[基础] 退出命令提示符窗口",
                description   = "退出命令提示符窗口。",
                syntax        = "exit",
                example       = "exit                                   - 退出命令提示符",
                runExample    = "exit"
            },
            new CommandInfo
            {
                command       = "help",
                fullName      = "Command Help",
                fullNameChina = "[基础] 显示命令帮助信息",
                description   = "显示命令帮助信息。",
                syntax        = "help [command]",
                example = "help dir                                - 显示 dir 命令的帮助信息\n" +
                          "help copy                               - 显示 copy 命令的帮助信息",
                runExample = "help dir"
            },
            new CommandInfo
            {
                command       = "hostname",
                fullName      = "Host Name",
                fullNameChina = "[基础] 显示当前计算机名称",
                description   = "显示当前计算机名称。",
                syntax        = "hostname",
                example       = "hostname                                - 显示本机计算机名",
                runExample    = "hostname"
            },

            #endregion

            #region 🧰 工具集：系统管理 / 网络 / 磁盘 / 用户 / 系统诊断

            new CommandInfo
            {
                command       = "dir",
                fullName      = "Directory List",
                fullNameChina = "[工具集] 显示目录内容",
                description   = "列出目录中的文件和子目录。",
                syntax        = "dir [<Drive>:][<Path>][<FileName>] [/A] [/O] [/P] [/S]",
                example = "例如：\n" +
                          "dir                - 列出当前目录内容\n" +
                          "dir /p             - 分页显示结果\n" +
                          "dir /s *.txt       - 搜索所有子目录中的 .txt 文件",
                runExample = "dir C:\\Windows /A /O"
            },
            new CommandInfo
            {
                command       = "cd",
                fullName      = "Change Directory",
                fullNameChina = "[工具集] 显示或更改当前目录",
                description   = "显示当前目录名或更改当前目录。",
                syntax        = "cd [<Drive>:][<Path>]",
                example = "cd                 - 显示当前目录\n" +
                          "cd C:\\Windows      - 切换到 C:\\Windows 目录\n" +
                          "cd ..               - 返回上一级目录",
                runExample = "cd C:\\Program Files"
            },
            new CommandInfo
            {
                command       = "copy",
                fullName      = "Copy File",
                fullNameChina = "[工具集] 复制文件",
                description   = "将一个或多个文件复制到另一个位置。",
                syntax        = "copy <Source> <Destination> [/Y]",
                example = "copy C:\\temp\\a.txt D:\\backup\\a.txt /Y   - 复制 a.txt 并自动覆盖\n" +
                          "copy C:\\temp\\*.txt D:\\backup\\               - 复制所有 txt 文件",
                runExample = "copy C:\\temp\\a.txt D:\\backup\\a.txt /Y"
            },
            new CommandInfo
            {
                command       = "xcopy",
                fullName      = "Extended Copy",
                fullNameChina = "[工具集] 复制文件和目录",
                description   = "复制文件和目录，包括子目录。",
                syntax        = "xcopy <Source> <Destination> [/E] [/Y]",
                example = "xcopy C:\\MyProject D:\\Backup\\MyProject /E /Y   - 复制目录及子目录并覆盖\n" +
                          "xcopy C:\\Data\\*.txt D:\\Backup /Y               - 复制指定类型文件",
                runExample = "xcopy C:\\MyProject D:\\Backup\\MyProject /E /Y"
            },
            new CommandInfo
            {
                command       = "robocopy",
                fullName      = "Robust Copy",
                fullNameChina = "[工具集] 高级文件复制命令",
                description   = "强大的文件复制命令，支持断点续传和容错。",
                syntax        = "robocopy <Source> <Destination> [<File>[ ...]] [options]",
                example = "robocopy C:\\Data D:\\Backup /E /Z /R:3      - 复制目录及子目录，支持断点续传，重试 3 次\n" +
                          "robocopy C:\\Source C:\\Dest *.txt /S /MOV      - 复制 txt 文件并移动到目标目录",
                runExample = "robocopy C:\\Data D:\\Backup /E /Z /R:3"
            },
            new CommandInfo
            {
                command       = "del",
                fullName      = "Delete File",
                fullNameChina = "[工具集] 删除文件",
                description   = "删除一个或多个文件。",
                syntax        = "del <FileName> [/F] [/Q]",
                example = "del C:\\temp\\*.log /F /Q          - 删除所有日志文件，强制删除并静默\n" +
                          "del C:\\temp\\a.txt /Q            - 静默删除指定文件",
                runExample = "del C:\\temp\\*.log /F /Q"
            },
            new CommandInfo
            {
                command       = "mkdir",
                fullName      = "Make Directory",
                fullNameChina = "[工具集] 创建目录",
                description   = "创建一个新目录。",
                syntax        = "mkdir <DirectoryName>",
                example = "mkdir C:\\Backup\\Logs           - 创建日志备份目录\n" +
                          "mkdir D:\\Projects\\NewProject     - 创建新项目目录",
                runExample = "mkdir C:\\Backup\\Logs"
            },
            new CommandInfo
            {
                command       = "rmdir",
                fullName      = "Remove Directory",
                fullNameChina = "[工具集] 删除目录",
                description   = "删除一个目录。",
                syntax        = "rmdir <DirectoryName> [/S] [/Q]",
                example = "rmdir C:\\Backup\\Logs /S /Q      - 删除目录及所有子目录，静默执行\n" +
                          "rmdir D:\\OldProject /Q          - 静默删除目录(无子目录)",
                runExample = "rmdir C:\\Backup\\Logs /S /Q"
            },
            new CommandInfo
            {
                command       = "ipconfig",
                fullName      = "Internet Protocol Configuration",
                fullNameChina = "[工具集] 显示网络配置信息",
                description   = "显示系统的 IP 配置。",
                syntax        = "ipconfig [/all]",
                example = "ipconfig                         - 显示基本 IP 配置\n" +
                          "ipconfig /all                     - 显示完整 IP 配置",
                runExample = "ipconfig /all"
            },
            new CommandInfo
            {
                command       = "ping",
                fullName      = "Packet Internet Groper",
                fullNameChina = "[工具集] 测试网络连接",
                description   = "测试与主机的网络连接。",
                syntax        = "ping <hostname> [-t]",
                example = "ping 8.8.8.8                      - 测试与 8.8.8.8 的连接\n" +
                          "ping 8.8.8.8 -t                   - 连续测试直到手动停止",
                runExample = "ping 8.8.8.8 -t"
            },
            new CommandInfo
            {
                command       = "tracert",
                fullName      = "Trace Route",
                fullNameChina = "[工具集] 显示到目标主机路径上经过的节点",
                description   = "显示到目标主机路径上经过的节点。",
                syntax        = "tracert <hostname>",
                example = "tracert www.google.com                - 显示到 www.google.com 的路由信息\n" +
                          "tracert 8.8.8.8                        - 显示到 8.8.8.8 的路由信息",
                runExample = "tracert www.google.com"
            },
            new CommandInfo
            {
                command       = "netstat",
                fullName      = "Network Statistics",
                fullNameChina = "[工具集] 显示网络连接、路由表和端口使用情况",
                description   = "显示网络连接、路由表和端口使用情况。",
                syntax        = "netstat [-a] [-n] [-o]",
                example = "netstat -a                              - 显示所有连接和监听端口\n" +
                          "netstat -ano                            - 显示所有连接并附加 PID",
                runExample = "netstat -ano"
            },
            new CommandInfo
            {
                command       = "tasklist",
                fullName      = "Task List",
                fullNameChina = "[工具集] 显示当前运行的进程",
                description   = "显示所有当前运行的进程。",
                syntax        = "tasklist [/FI <Filter>]",
                example = "tasklist                                 - 列出所有运行中的进程\n" +
                          "tasklist /FI \"IMAGENAME eq notepad.exe\" - 列出所有 notepad.exe 进程",
                runExample = "tasklist /FI \"IMAGENAME eq notepad.exe\""
            },
            new CommandInfo
            {
                command       = "taskkill",
                fullName      = "Task Kill",
                fullNameChina = "[工具集] 终止进程",
                description   = "终止一个或多个进程。",
                syntax        = "taskkill /IM <ProcessName> [/F] [/T]",
                example = "taskkill /IM vrmonitor.exe /F           - 强制终止 vrmonitor.exe 进程\n" +
                          "taskkill /IM notepad.exe                - 终止 notepad.exe 进程",
                runExample = "taskkill /IM vrmonitor.exe /F"
            },
            new CommandInfo
            {
                command       = "net start",
                fullName      = "Network Service Start",
                fullNameChina = "[工具集] 启动 Windows 服务",
                description   = "启动 Windows 服务。",
                syntax        = "net start \"<ServiceName>\"",
                example = "net start \"Leap Service\"             - 启动 Leap Service 服务\n" +
                          "net start \"Spooler\"                  - 启动打印机后台处理程序服务",
                runExample = "net start \"Leap Service\""
            },
            new CommandInfo
            {
                command       = "net stop",
                fullName      = "Network Service Stop",
                fullNameChina = "[工具集] 停止 Windows 服务",
                description   = "停止 Windows 服务。",
                syntax        = "net stop \"<ServiceName>\"",
                example = "net stop \"UltraleapTracking\"        - 停止 UltraleapTracking 服务\n" +
                          "net stop \"Spooler\"                  - 停止打印机后台处理程序服务",
                runExample = "net stop \"UltraleapTracking\""
            },
            new CommandInfo
            {
                command       = "sc query",
                fullName      = "Service Controller Query",
                fullNameChina = "[工具集] 显示服务状态",
                description   = "显示服务状态。",
                syntax        = "sc query [ServiceName]",
                example = "sc query \"UltraleapTracking\"         - 查询 UltraleapTracking 服务状态\n" +
                          "sc query \"Spooler\"                  - 查询打印机后台处理程序服务状态",
                runExample = "sc query \"UltraleapTracking\""
            },
            new CommandInfo
            {
                command       = "shutdown",
                fullName      = "System Shutdown",
                fullNameChina = "[工具集] 关闭、重新启动或注销计算机",
                description   = "关闭、重新启动或注销计算机。",
                syntax        = "shutdown /s /t <seconds>",
                example = "shutdown /s /t 0                     - 立即关机\n" +
                          "shutdown /r /t 5                     - 5 秒后重启\n" +
                          "shutdown /l                           - 注销当前用户\n" +
                          "shutdown /s /f /t 10                  - 10 秒后强制关机\n" +
                          "shutdown /r /m \\\\OfficePC /t 10      - 远程重启 OfficePC\n" +
                          "shutdown /a                           - 取消已计划的关机",
                runExample = "shutdown /s /t 0"
            },
            new CommandInfo
            {
                command       = "systeminfo",
                fullName      = "System Information",
                fullNameChina = "[工具集] 显示计算机的详细配置信息",
                description   = "显示计算机的详细配置信息。",
                syntax        = "systeminfo",
                example = "systeminfo                              - 显示完整系统信息\n" +
                          "systeminfo | find \"System Boot Time\"    - 查询系统启动时间",
                runExample = "systeminfo | find \"System Boot Time\""
            },
            new CommandInfo
            {
                command       = "msconfig",
                fullName      = "System Configuration",
                fullNameChina = "[工具集] 打开系统配置工具",
                description   = "打开系统配置工具，用于启动项和服务管理。",
                syntax        = "msconfig",
                example       = "msconfig                                - 打开系统配置工具",
                runExample    = "msconfig"
            },
            new CommandInfo
            {
                command       = "services.msc",
                fullName      = "Services Manager",
                fullNameChina = "[工具集] 打开服务管理控制台",
                description   = "打开服务管理控制台。",
                syntax        = "services.msc",
                example       = "services.msc                           - 打开服务管理控制台",
                runExample    = "services.msc"
            },
            new CommandInfo
            {
                command       = "devmgmt.msc",
                fullName      = "Device Manager",
                fullNameChina = "[工具集] 打开设备管理器",
                description   = "打开设备管理器。",
                syntax        = "devmgmt.msc",
                example       = "devmgmt.msc                            - 打开设备管理器",
                runExample    = "devmgmt.msc"
            },
            new CommandInfo
            {
                command       = "eventvwr.msc",
                fullName      = "Event Viewer",
                fullNameChina = "[工具集] 打开事件查看器",
                description   = "打开事件查看器。",
                syntax        = "eventvwr.msc",
                example       = "eventvwr.msc                           - 打开事件查看器",
                runExample    = "eventvwr.msc"
            },
            new CommandInfo
            {
                command       = "dxdiag",
                fullName      = "DirectX Diagnostic Tool",
                fullNameChina = "[工具集] 显示系统的 DirectX 信息",
                description   = "显示系统的 DirectX 信息。",
                syntax        = "dxdiag [/t <FileName>]",
                example       = "dxdiag /t C:\\dxinfo.txt               - 将 DirectX 信息导出到文件",
                runExample    = "dxdiag /t C:\\dxinfo.txt"
            },

            #endregion

            #region 🧠 高级与系统管理员命令(注册表、磁盘、安全、性能、事件、网络诊断、系统引导等)

            new CommandInfo
            {
                command       = "reg query",
                fullName      = "Registry Query",
                fullNameChina = "[高级] 查询注册表",
                description   = "查询注册表键值。",
                syntax        = "reg query <KeyPath> [/v ValueName]",
                example       = "reg query HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion /v ProductName - 查询 Windows 产品名称",
                runExample    = "reg query HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion /v ProductName"
            },
            new CommandInfo
            {
                command       = "reg add",
                fullName      = "Registry Add",
                fullNameChina = "[高级] 添加或修改注册表值",
                description   = "向注册表中添加新值或修改值。",
                syntax        = "reg add <KeyPath> /v <ValueName> /t <Type> /d <Data> /f",
                example       = "reg add HKCU\\Environment /v MyVar /t REG_SZ /d Test /f - 添加或修改注册表值",
                runExample    = "reg add HKCU\\Environment /v MyVar /t REG_SZ /d Test /f"
            },
            new CommandInfo
            {
                command       = "reg delete",
                fullName      = "Registry Delete",
                fullNameChina = "[高级] 删除注册表值",
                description   = "删除注册表键或值。",
                syntax        = "reg delete <KeyPath> [/v ValueName] /f",
                example       = "reg delete HKCU\\Environment /v MyVar /f - 删除指定注册表值",
                runExample    = "reg delete HKCU\\Environment /v MyVar /f"
            },
            new CommandInfo
            {
                command       = "reg export",
                fullName      = "Registry Export",
                fullNameChina = "[高级] 导出注册表分支",
                description   = "导出注册表分支到文件。",
                syntax        = "reg export <KeyPath> <FilePath>",
                example       = "reg export HKLM\\SOFTWARE C:\\backup.reg - 导出注册表到文件",
                runExample    = "reg export HKLM\\SOFTWARE C:\\backup.reg"
            },
            new CommandInfo
            {
                command       = "reg import",
                fullName      = "Registry Import",
                fullNameChina = "[高级] 导入注册表分支",
                description   = "从文件导入注册表项。",
                syntax        = "reg import <FilePath>",
                example       = "reg import C:\\backup.reg - 导入注册表文件",
                runExample    = "reg import C:\\backup.reg"
            },
            new CommandInfo
            {
                command       = "bcdedit",
                fullName      = "Boot Configuration Data Editor",
                fullNameChina = "[高级] 编辑引导配置数据",
                description   = "编辑引导配置数据存储，用于管理启动项。",
                syntax        = "bcdedit [/set {<ID>} <Option> <Value>]",
                example       = "bcdedit /set {current} safeboot minimal - 设置当前启动为安全模式",
                runExample    = "bcdedit /set {current} safeboot minimal"
            },
            new CommandInfo
            {
                command       = "bootrec",
                fullName      = "Boot Recovery",
                fullNameChina = "[高级] 修复引导记录和主引导扇区",
                description   = "修复引导记录和主引导扇区(WinRE使用)。",
                syntax        = "bootrec [/fixmbr | /fixboot | /scanos | /rebuildbcd]",
                example       = "bootrec /fixmbr - 修复主引导记录",
                runExample    = "bootrec /fixmbr"
            },
            new CommandInfo
            {
                command       = "wmic",
                fullName      = "Windows Management Instrumentation Command-line",
                fullNameChina = "[高级] 执行 WMI 操作",
                description   = "从命令行执行 WMI 操作。",
                syntax        = "wmic <Alias> <Command>",
                example       = "wmic process where \"name='notepad.exe'\" get ProcessId - 查询 notepad.exe 的进程ID",
                runExample    = "wmic process where \"name='notepad.exe'\" get ProcessId"
            },
            new CommandInfo
            {
                command       = "fsutil",
                fullName      = "File System Utility",
                fullNameChina = "[高级] 显示或更改文件系统属性",
                description   = "显示和配置文件系统属性(管理员命令)。",
                syntax        = "fsutil behavior query DisableDeleteNotify",
                example       = "fsutil behavior set DisableDeleteNotify 0 - 启用或禁用 TRIM 功能",
                runExample    = "fsutil behavior set DisableDeleteNotify 0"
            },
            new CommandInfo
            {
                command       = "cipher",
                fullName      = "File Encryption Utility",
                fullNameChina = "[高级] 显示或更改文件加密状态",
                description   = "显示或更改文件加密状态。",
                syntax        = "cipher [/E|/D] [Path]",
                example       = "cipher /E C:\\Sensitive - 加密指定目录",
                runExample    = "cipher /E C:\\Sensitive"
            },
            new CommandInfo
            {
                command       = "manage-bde",
                fullName      = "BitLocker Drive Encryption Tool",
                fullNameChina = "[高级] 配置和管理 BitLocker 加密驱动器",
                description   = "配置和管理 BitLocker 加密驱动器。",
                syntax        = "manage-bde -status [<Drive>:]",
                example       = "manage-bde -off C: - 关闭 C 盘 BitLocker",
                runExample    = "manage-bde -off C:"
            },
            new CommandInfo
            {
                command       = "powercfg /energy",
                fullName      = "Power Efficiency Diagnostics",
                fullNameChina = "[高级] 生成系统电源效率诊断报告",
                description   = "生成系统电源效率诊断报告。",
                syntax        = "powercfg /energy /output <file.html>",
                example       = "powercfg /energy /output C:\\energy.html - 生成电源诊断报告",
                runExample    = "powercfg /energy /output C:\\energy.html"
            },
            new CommandInfo
            {
                command       = "logman",
                fullName      = "Performance Log Manager",
                fullNameChina = "[高级] 创建与管理性能日志与计数器集",
                description   = "创建与管理性能日志与计数器集。",
                syntax        = "logman create counter <Name> -c <Counter> -si <Interval>",
                example       = "logman create counter CPUUsage -c \"\\Processor(_Total)\\% Processor Time\" -si 10 - 创建 CPU 使用率日志",
                runExample    = "logman create counter CPUUsage -c \"\\Processor(_Total)\\% Processor Time\" -si 10"
            },
            new CommandInfo
            {
                command       = "typeperf",
                fullName      = "Type Performance Counter",
                fullNameChina = "[高级] 实时显示性能计数器数据",
                description   = "实时显示性能计数器数据。",
                syntax        = "typeperf \"\\Processor(_Total)\\% Processor Time\"",
                example       = "typeperf \"\\Memory\\Available MBytes\" - 显示可用内存性能数据",
                runExample    = "typeperf \"\\Memory\\Available MBytes\""
            },
            new CommandInfo
            {
                command       = "driverquery",
                fullName      = "Driver Query",
                fullNameChina = "[高级] 显示已安装设备驱动程序列表",
                description   = "显示已安装设备驱动程序列表。",
                syntax        = "driverquery [/FO list|table|csv] [/V]",
                example       = "driverquery /V /FO table - 显示详细驱动程序信息，表格格式",
                runExample    = "driverquery /V /FO table"
            },

            #endregion
        };
    }
}