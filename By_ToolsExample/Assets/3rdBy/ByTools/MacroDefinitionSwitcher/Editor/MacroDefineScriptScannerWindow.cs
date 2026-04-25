//=====================================================
// 文件名称: MacroDefineScriptScannerWindow
// 创 建 者: wangbaiyan
// 创建日期: 2026-4-23
// 描    述: 脚本宏定义扫描窗口
//=====================================================

namespace MacroDefineBuildToolEditor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 独立的脚本宏定义扫描窗口。
    /// 只负责扫描项目中 C# 脚本里通过 #if / #elif 使用过的宏定义，不修改主工具配置，也不影响打包流程。
    /// </summary>
    public class MacroDefineScriptScannerWindow : EditorWindow
    {
        private static readonly Regex directiveRegex = new(@"^\s*#(?<directive>if|elif)\s+(?<expression>.+)$", RegexOptions.Compiled);
        private static readonly Regex symbolRegex = new(@"\b[A-Za-z_][A-Za-z0-9_]*\b", RegexOptions.Compiled);

        private readonly HashSet<string> _keywordFilter = new(StringComparer.OrdinalIgnoreCase)
        {
            "true",
            "false",
            "defined",
        };

        [Serializable]
        private class ScanLocation
        {
            public string assetPath;
            public int lineNumber;
            public string directive;
            public string expression;
        }

        [Serializable]
        private class ScanEntry
        {
            public string symbol;
            public bool isLikelyUnityBuiltin;
            public bool isCurrentlyDefined;
            public bool existsInSavedModes;
            public int occurrenceCount;
            public int fileCount;
            public bool isExpanded;
            public List<ScanLocation> locations = new();
        }

        private Vector2 _resultScroll;
        private string _searchText = string.Empty;
        private bool _includeAssetsFolder = true;
        private bool _includePackagesFolder;
        private bool _showOnlyCustomSymbols;
        private bool _showOnlySymbolsMissingInModes;
        private bool _showOnlyCurrentDefined;

        private int _scannedFileCount;
        private int _matchedDirectiveCount;
        private string _lastScanTimeText = "未扫描";
        private List<ScanEntry> _entries = new();

        /// <summary>
        /// 打开独立扫描窗口。
        /// </summary>
        // [MenuItem("ByTools/🔖宏定义切换与打包工具/脚本宏定义扫描")]
        public static void OpenWindow()
        {
            var window = GetWindow<MacroDefineScriptScannerWindow>("🔍脚本宏定义扫描");
            window.minSize = new Vector2(980f, 720f);
            window.Show();
        }

        private void OnGUI()
        {
            DrawScanOptions();
            EditorGUILayout.Space(6f);
            DrawSummary();
            EditorGUILayout.Space(6f);
            DrawResultToolbar();
            EditorGUILayout.Space(4f);
            DrawResults();
        }


        private void DrawScanOptions()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("扫描范围", EditorStyles.boldLabel);

            _includeAssetsFolder   = EditorGUILayout.ToggleLeft("扫描 Assets/**/*.cs", _includeAssetsFolder);
            _includePackagesFolder = EditorGUILayout.ToggleLeft("扫描 Packages/**/*.cs（仅项目 Packages 目录）", _includePackagesFolder);

            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = _includeAssetsFolder || _includePackagesFolder;
            if (GUILayout.Button("一键扫描", GUILayout.Height(28f)))
            {
                RunScan();
            }

            GUI.enabled = true;

            if (GUILayout.Button("清空结果", GUILayout.Width(100f), GUILayout.Height(28f)))
            {
                _entries.Clear();
                _scannedFileCount      = 0;
                _matchedDirectiveCount = 0;
                _lastScanTimeText      = "未扫描";
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawSummary()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("扫描结果概览", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"上次扫描时间：{_lastScanTimeText}");
            EditorGUILayout.LabelField($"扫描脚本数量：{_scannedFileCount}");
            EditorGUILayout.LabelField($"命中的 #if / #elif 行数：{_matchedDirectiveCount}");
            EditorGUILayout.LabelField($"检索出的唯一宏定义数量：{_entries.Count}");
            EditorGUILayout.EndVertical();
        }

        private void DrawResultToolbar()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("结果筛选", EditorStyles.boldLabel);

            _searchText = EditorGUILayout.TextField("搜索宏定义", _searchText);

            EditorGUILayout.BeginHorizontal();
            _showOnlyCustomSymbols         = EditorGUILayout.ToggleLeft("仅看自定义宏", _showOnlyCustomSymbols, GUILayout.Width(140f));
            _showOnlySymbolsMissingInModes = EditorGUILayout.ToggleLeft("仅看未进入模板的宏", _showOnlySymbolsMissingInModes, GUILayout.Width(170f));
            _showOnlyCurrentDefined        = EditorGUILayout.ToggleLeft("仅看当前已定义宏", _showOnlyCurrentDefined, GUILayout.Width(160f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全部展开", GUILayout.Width(100f)))
            {
                SetAllExpanded(true);
            }

            if (GUILayout.Button("全部收起", GUILayout.Width(100f)))
            {
                SetAllExpanded(false);
            }

            if (GUILayout.Button("复制全部宏定义", GUILayout.Width(140f)))
            {
                EditorGUIUtility.systemCopyBuffer = string.Join(";", _entries.Select(e => e.symbol));
                ShowNotification(new GUIContent("已复制全部宏定义"));
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawResults()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("宏定义列表", EditorStyles.boldLabel);
            EditorGUILayout.Space(2f);

            var filteredEntries = GetFilteredEntries();
            if (filteredEntries.Count == 0)
            {
                EditorGUILayout.HelpBox("当前没有可显示的扫描结果。", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            _resultScroll = EditorGUILayout.BeginScrollView(_resultScroll);
            foreach (var entry in filteredEntries)
            {
                DrawEntry(entry);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawEntry(ScanEntry entry)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            var foldoutRect = GUILayoutUtility.GetRect(18f, EditorGUIUtility.singleLineHeight, GUILayout.Width(18f));
            entry.isExpanded = EditorGUI.Foldout(foldoutRect, entry.isExpanded, GUIContent.none, true);

            var symbolStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                wordWrap = false,
                clipping = TextClipping.Clip,
            };

            var symbolContent = new GUIContent(entry.symbol, entry.symbol);
            var symbolRect    = GUILayoutUtility.GetRect(symbolContent, symbolStyle, GUILayout.ExpandWidth(true));
            EditorGUI.LabelField(symbolRect, symbolContent, symbolStyle);

            GUILayout.Space(6f);
            EditorGUILayout.LabelField($"出现 {entry.occurrenceCount} 次 / {entry.fileCount} 个脚本", GUILayout.Width(180f));

            if (GUILayout.Button("复制", GUILayout.Width(60f)))
            {
                EditorGUIUtility.systemCopyBuffer = entry.symbol;
                ShowNotification(new GUIContent($"已复制：{entry.symbol}"));
            }

            EditorGUILayout.EndHorizontal();

            var statusStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true,
                richText = false,
            };
            EditorGUILayout.LabelField(BuildStatusText(entry), statusStyle);

            if (entry.isExpanded)
            {
                if (entry.isLikelyUnityBuiltin)
                {
                    EditorGUILayout.HelpBox("该项看起来像 Unity / 平台 / 运行时内置宏，不一定来自自定义 Scripting Define Symbols。", MessageType.None);
                }

                for (int i = 0; i < entry.locations.Count; i++)
                {
                    DrawLocation(entry.locations[i]);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawLocation(ScanLocation location)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField($"{location.assetPath}  (第 {location.lineNumber} 行)");
            EditorGUILayout.LabelField($"{location.directive}: {location.expression}", EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("打开脚本", GUILayout.Width(90f)))
            {
                OpenScriptAtLocation(location);
            }

            if (GUILayout.Button("复制路径", GUILayout.Width(90f)))
            {
                EditorGUIUtility.systemCopyBuffer = location.assetPath;
                ShowNotification(new GUIContent("已复制路径"));
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void RunScan()
        {
            if (!_includeAssetsFolder && !_includePackagesFolder)
            {
                EditorUtility.DisplayDialog("提示", "请至少勾选一个扫描目录。", "确定");
                return;
            }

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            var    roots       = new List<string>();
            if (_includeAssetsFolder)
            {
                roots.Add(Application.dataPath);
            }

            if (_includePackagesFolder)
            {
                roots.Add(Path.Combine(projectRoot, "Packages"));
            }

            var entryMap = new Dictionary<string, ScanEntry>(StringComparer.OrdinalIgnoreCase);
            var currentCustomDefines = new HashSet<string>(
                MacroDefineBuildToolUtility.ExtractNormalizedSymbols(
                    MacroDefineBuildToolUtility.ParseDefineSymbolsString(
                        MacroDefineBuildToolUtility.GetScriptingDefineSymbols(MacroDefineBuildToolUtility.GetCurrentActivePlatform()))),
                StringComparer.OrdinalIgnoreCase);

            var symbolsInSavedModes = LoadSymbolsInSavedModes();

            int fileCounter      = 0;
            int directiveCounter = 0;

            try
            {
                string[] allFiles = CollectCsFiles(roots).ToArray();
                for (int fileIndex = 0; fileIndex < allFiles.Length; fileIndex++)
                {
                    string fullPath  = allFiles[fileIndex];
                    string assetPath = ConvertToProjectRelativePath(fullPath);
                    if (string.IsNullOrEmpty(assetPath))
                    {
                        continue;
                    }

                    EditorUtility.DisplayProgressBar(
                        "脚本宏定义扫描",
                        $"正在扫描：{assetPath}",
                        allFiles.Length <= 0 ? 1f : (fileIndex + 1f) / allFiles.Length);

                    fileCounter++;
                    string[] lines;
                    try
                    {
                        lines = File.ReadAllLines(fullPath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[脚本宏定义扫描] 读取失败：{assetPath}，原因：{ex.Message}");
                        continue;
                    }

                    for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                    {
                        var match = directiveRegex.Match(lines[lineIndex]);
                        if (!match.Success)
                        {
                            continue;
                        }

                        directiveCounter++;
                        string directive  = match.Groups["directive"].Value;
                        string expression = match.Groups["expression"].Value.Trim();

                        var symbols = ExtractSymbolsFromExpression(expression);
                        foreach (string symbol in symbols)
                        {
                            if (!entryMap.TryGetValue(symbol, out var entry))
                            {
                                entry = new ScanEntry
                                {
                                    symbol               = symbol,
                                    isLikelyUnityBuiltin = IsLikelyUnityBuiltinSymbol(symbol),
                                    isCurrentlyDefined   = currentCustomDefines.Contains(symbol),
                                    existsInSavedModes   = symbolsInSavedModes.Contains(symbol),
                                    isExpanded           = false,
                                };
                                entryMap.Add(symbol, entry);
                            }

                            entry.occurrenceCount++;
                            entry.locations.Add(new ScanLocation
                            {
                                assetPath  = assetPath,
                                lineNumber = lineIndex + 1,
                                directive  = directive,
                                expression = expression,
                            });
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            foreach (var entry in entryMap.Values)
            {
                entry.fileCount = entry.locations
                    .Select(location => location.assetPath)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();

                entry.locations = entry.locations
                    .OrderBy(location => location.assetPath, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(location => location.lineNumber)
                    .ToList();
            }

            _entries = entryMap.Values
                .OrderByDescending(entry => entry.occurrenceCount)
                .ThenBy(entry => entry.symbol, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _scannedFileCount      = fileCounter;
            _matchedDirectiveCount = directiveCounter;
            _lastScanTimeText      = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Repaint();
        }

        private IEnumerable<string> CollectCsFiles(IEnumerable<string> roots)
        {
            foreach (string root in roots)
            {
                if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                {
                    continue;
                }

                IEnumerable<string> files;
                try
                {
                    files = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[脚本宏定义扫描] 枚举目录失败：{root}，原因：{ex.Message}");
                    continue;
                }

                foreach (string file in files)
                {
                    yield return file;
                }
            }
        }

        private IEnumerable<string> ExtractSymbolsFromExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                yield break;
            }

            var unique  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var matches = symbolRegex.Matches(expression);
            foreach (Match match in matches)
            {
                string symbol = MacroDefineBuildToolUtility.NormalizeSymbol(match.Value);
                if (string.IsNullOrEmpty(symbol))
                {
                    continue;
                }

                if (_keywordFilter.Contains(symbol))
                {
                    continue;
                }

                if (!unique.Add(symbol))
                {
                    continue;
                }

                yield return symbol;
            }
        }

        private string ConvertToProjectRelativePath(string fullPath)
        {
            string normalized  = fullPath.Replace('\\', '/');
            string dataPath    = Application.dataPath.Replace('\\', '/');
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName.Replace('\\', '/') ?? dataPath;

            if (normalized.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase))
            {
                return "Assets" + normalized.Substring(dataPath.Length);
            }

            return normalized.StartsWith(projectRoot + "/", StringComparison.OrdinalIgnoreCase) ? normalized.Substring(projectRoot.Length + 1) : string.Empty;
        }

        private void OpenScriptAtLocation(ScanLocation location)
        {
            if (location == null || string.IsNullOrWhiteSpace(location.assetPath))
            {
                return;
            }

            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(location.assetPath);
            if (script == null)
            {
                EditorUtility.DisplayDialog("提示", $"无法打开脚本：{location.assetPath}", "确定");
                return;
            }

            AssetDatabase.OpenAsset(script, location.lineNumber);
        }

        private HashSet<string> LoadSymbolsInSavedModes()
        {
            var config = MacroDefineBuildToolStorage.Load();
            config = MacroDefineBuildToolStorage.FixNullFields(config);

            return new HashSet<string>(
                config.modes.SelectMany(mode => MacroDefineBuildToolUtility.ExtractNormalizedSymbols(mode.defines)),
                StringComparer.OrdinalIgnoreCase);
        }

        private string BuildStatusText(ScanEntry entry)
        {
            var tags = new List<string>();

            if (entry.isCurrentlyDefined)
            {
                tags.Add("✅ 当前自定义已定义");
            }
            else if (!entry.isLikelyUnityBuiltin)
            {
                tags.Add("⚪ 当前自定义未定义");
            }

            tags.Add(entry.existsInSavedModes ? "📁 已进入模板" : "📝 未进入模板");

            if (entry.isLikelyUnityBuiltin)
            {
                tags.Add("Unity内置/平台宏");
            }

            return string.Join("  ", tags);
        }

        private List<ScanEntry> GetFilteredEntries()
        {
            IEnumerable<ScanEntry> query = _entries;

            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                string keyword = _searchText.Trim();
                query = query.Where(entry => entry.symbol.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (_showOnlyCustomSymbols)
            {
                query = query.Where(entry => !entry.isLikelyUnityBuiltin);
            }

            if (_showOnlySymbolsMissingInModes)
            {
                query = query.Where(entry => !entry.existsInSavedModes);
            }

            if (_showOnlyCurrentDefined)
            {
                query = query.Where(entry => entry.isCurrentlyDefined);
            }

            return query.ToList();
        }

        private void SetAllExpanded(bool isExpanded)
        {
            foreach (var entry in _entries)
            {
                entry.isExpanded = isExpanded;
            }
        }

        private bool IsLikelyUnityBuiltinSymbol(string symbol)
        {
            return symbol.StartsWith("UNITY_", StringComparison.OrdinalIgnoreCase)
                   || symbol.StartsWith("ENABLE_", StringComparison.OrdinalIgnoreCase)
                   || symbol.StartsWith("NET_", StringComparison.OrdinalIgnoreCase)
                   || symbol.StartsWith("CSHARP_", StringComparison.OrdinalIgnoreCase)
                   || symbol.StartsWith("DEVELOPMENT_BUILD", StringComparison.OrdinalIgnoreCase)
                   || symbol.StartsWith("DEBUG", StringComparison.OrdinalIgnoreCase)
                   || symbol.StartsWith("TRACE", StringComparison.OrdinalIgnoreCase);
        }
    }
}