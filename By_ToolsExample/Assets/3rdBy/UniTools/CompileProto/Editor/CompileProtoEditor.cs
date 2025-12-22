namespace _3rdBy.UniTools.CompileProto.Editor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEditor;
    using UnityEditor.ShortcutManagement;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    public class CompileProtoEditor : EditorWindow
    {
        private Dictionary<string, string> _protoFileDic; //key:名字，value:路径
        private string _exePath;
        private string _batPath;

        [MenuItem("ByTools/🧩 编译.Proto文件")]
        public static void ShowWindow()
        {
            var window = GetWindow<CompileProtoEditor>("编译.Proto文件");
            window.minSize = Vector2.one * 600;
            window.maxSize = Vector2.one * 900;
        }

        private void OnGUI()
        {
            GUILayout.Label(".Proto文件编译.cs工具", TitleStyle());
            GUILayout.Space(20);
            SelectProtoToolView();

            GUILayout.Space(10);
            GUILayout.Label("Proto文件", EditorStyles.boldLabel);
            SelectProtoFileView();

            GUILayout.Space(10);

            GUILayout.Label("Proto文件", EditorStyles.boldLabel);
            CompileProtoFileView();
        }

        /// <summary>
        /// 显示要编译的protoc.exe工具
        /// </summary>
        private void SelectProtoToolView()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("protoc.exe工具", EditorStyles.boldLabel, GUILayout.Width(90));
            GUILayout.TextField(_exePath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                _exePath = EditorUtility.OpenFilePanel("选择protoc.exe工具", PresetsProtoToolPath(), "exe");
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 显示选择要编译的Proto文件
        /// </summary>
        private void SelectProtoFileView()
        {
            _protoFileDic ??= new Dictionary<string, string>();
            if (_protoFileDic.Count == 0)
            {
                if (GUILayout.Button("添加.proto文件"))
                {
                    OpenProtoFileFolder();
                }
            }

            var fileIndex = 0;
            foreach (var (key, value) in _protoFileDic)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{fileIndex}. ", GUILayout.Width(20)); //序号
                GUILayout.TextField(value); //路径

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    _protoFileDic.Remove(key);
                    GUILayout.EndHorizontal();
                    break;
                }

                if (fileIndex == _protoFileDic.Count - 1)
                {
                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        OpenProtoFileFolder();
                        GUILayout.EndHorizontal();
                        break;
                    }
                }

                fileIndex++;
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 显示点击编译界面
        /// </summary>
        private void CompileProtoFileView()
        {
            if (!GUILayout.Button("开始编译")) return;

            if (!File.Exists(_exePath))
            {
                EditorUtility.DisplayDialog("提示", "请选择编译工具 protoc.exe", "OK");
                return;
            }

            if (_protoFileDic.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请添加编译文件 .proto", "OK");
                return;
            }

            ClearConsole();

            StartCompileBat().Forget();
        }

        /// <summary>
        /// 打开Proto文件夹
        /// </summary>
        private void OpenProtoFileFolder()
        {
            var path = EditorUtility.OpenFilePanel("选择.proto文件", PresetsProtoFilesPath(), "proto");
            if (string.IsNullOrEmpty(path)) return;
            var fileName = GetProtoName(path);
            if (_protoFileDic.ContainsKey(fileName))
            {
                EditorUtility.DisplayDialog("提示", $"{fileName}.proto 文件在列表已存在！", "OK");
                return;
            }

            _protoFileDic.Add(fileName, path);
        }

        /// <summary>
        /// 开始编译Proto文件
        /// </summary>
        private async UniTaskVoid StartCompileBat()
        {
            //组装命令
            var cmdBuilder = new StringBuilder();
            foreach (var (key, value) in _protoFileDic)
            {
                var csFilePath = CSharpFileSavePath() + key;
                if (File.Exists(csFilePath)) File.Delete(csFilePath);
                var cmdContent = SpliceCommand(_exePath, value, CSharpFileSavePath());
                cmdBuilder.Append(cmdContent);
                cmdBuilder.Append("\n");
            }

            // Debug.Log(cmdBuilder.ToString());
            var cmdCodeFilePath = $"{Path.GetDirectoryName(_exePath)}/CmdCode.bat";
            await File.WriteAllTextAsync(cmdCodeFilePath, cmdBuilder.ToString()); //生成命令文件 CmdCode.bat

            Process.Start(cmdCodeFilePath); // 执行这个CmdCode.bat命令文件

            Thread.Sleep(1000);
            foreach (var (key, value) in _protoFileDic)
            {
                Debug.Log(value);
            }

            Thread.Sleep(1000);
            Debug.Log(".Proto文件编译完成");
            AssetDatabase.Refresh(); // 刷新编译器

            // if (EditorUtility.DisplayDialog("提示", "编译完成", "OK")) { }

            /* 命令执行速度过快，很难捕获异常
            Process process = new();
            process.StartInfo.FileName = CompileBatFilePath();
            process.OutputDataReceived += (sender, e) => { Debug.Log(e.Data); };
            process.ErrorDataReceived += (sender, e) => { Debug.Log(e.Data); };
            Debug.Log(Time.time);
            if (process.Start())
            {
                Debug.Log(process.HasExited);
                if (!process.HasExited)
                {
                    Debug.Log(Time.time);
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }
            }
            */
        }

        /// <summary>
        /// 拼接命令
        /// </summary>
        /// <param name="toolPath">工具路径</param>
        /// <param name="protoFilePath">pb文件路径</param>
        /// <param name="outPath">输出CS脚本路径</param>
        /// <returns>返回一条完整的命令</returns>
        private static string SpliceCommand(string toolPath, string protoFilePath, string outPath)
        {
            // 这里比较坑，路径连接proto文件需要空格，且路径‘/’符号需要‘\’
            var protoName = GetProtoName(protoFilePath) + ".proto";
            var replace = protoFilePath.Replace("/" + protoName, "");
            protoFilePath = (replace + "/ " + protoName).Replace("/", @"\");

            toolPath = toolPath.Replace("/", @"\");
            outPath = outPath.Replace("/", @"\");

            return $"{toolPath} --proto_path={protoFilePath} --csharp_out={outPath}";
        }

        /// <summary>
        /// CS 脚本保存的位置
        /// </summary>
        /// <returns></returns>
        private static string CSharpFileSavePath()
        {
            var saveFilePath = $"{Application.dataPath}/Scripts/ProtoCSharp/";
            if (!Directory.Exists(saveFilePath))
            {
                Directory.CreateDirectory(saveFilePath);
            }

            return saveFilePath;
        }

        /// <summary>
        /// 根据路径获取Proto文件名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetProtoName(string path)
        {
            var protoName = path.Split('/').Last();
            protoName = protoName.Split('.').First();
            return protoName;
        }


        /// <summary>
        /// 预设proto编译工具
        /// </summary>
        /// <returns>返回proto编译工具路径</returns>
        private static string PresetsProtoToolPath()
        {
            var protoFilesPath = Directory.GetParent(Application.dataPath)?.ToString();
            if (string.IsNullOrEmpty(protoFilesPath)) return "";
            protoFilesPath = $"{protoFilesPath}/ProtoTool/protoc-24.0-win64/bin/";
            return !Directory.Exists(protoFilesPath) ? "" : protoFilesPath;
        }

        /// <summary>
        /// 预设Proto文件路径
        /// </summary>
        /// <returns></returns>
        private static string PresetsProtoFilesPath()
        {
            var protoFilesPath = Directory.GetParent(Application.dataPath)?.ToString();
            if (string.IsNullOrEmpty(protoFilesPath)) return "";
            protoFilesPath = $"{protoFilesPath}/ProtoTool/ProtoFiles/";
            return !Directory.Exists(protoFilesPath) ? "" : protoFilesPath;
        }

        /// <summary>
        /// 标题文字的样式
        /// </summary>
        /// <returns></returns>
        private static GUIStyle TitleStyle()
        {
            var labelStyle = new GUIStyle
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            return labelStyle;
        }

        /// <summary>
        /// 清除控制台
        /// </summary>
        private static void ClearConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(SceneView));
            var logEntries = assembly.GetType("UnityEditor.LogEntries");
            var clearMethod = logEntries.GetMethod("Clear");
            clearMethod?.Invoke(new object(), null);
        }
    }
}