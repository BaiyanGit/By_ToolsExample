//=====================================================
// 文件名称: FFmpegAllInOneParameterGuideWindow
// 描    述:
//  Unity Editor 下的 FFmpeg 参数大词典 / 搜索查看器。
//  覆盖录屏、转码、编码、音频、滤镜、字幕、硬件加速、容器、流媒体、调试排错等常用参数。
//=====================================================

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// FFmpeg 参数大词典。
///
/// 菜单路径：
/// ByTools / 🎬 FFmpeg 参数大词典
///
/// 注意：
/// FFmpeg 的官方参数会随版本、编译选项、编码器、滤镜、封装器、平台设备而变化。
/// 这个工具覆盖的是开发录屏/转码工具时最常遇到的大部分参数，不等同于官方全量文档镜像。
/// </summary>
public sealed class FFmpegAllInOneParameterGuideWindow : EditorWindow
{
    private const string WINDOW_TITLE = "FFmpeg 参数大词典";

    private const string OFFICIAL_DOCUMENT_URL = "https://ffmpeg.org/ffmpeg.html";
    private const string OFFICIAL_DOCUMENT_BUTTON_TEXT = "打开 FFmpeg 官方文档";

    private const float MIN_LEFT_WIDTH = 240f;
    private const float MAX_LEFT_WIDTH = 340f;
    private const float CARD_MAX_TEXT_WIDTH_PADDING = 360f;
    private const float TOP_HEADER_HEIGHT = 74f;
    private const float QUICK_BAR_HEIGHT = 58f;
    private const float SEARCH_BAR_HEIGHT = 30f;


    private readonly List<ParamInfo> _items = new List<ParamInfo>();
    private readonly List<string> _categories = new List<string>();

    private Vector2 _leftScroll;
    private Vector2 _rightScroll;
    private string _searchText = string.Empty;
    private int _selectedCategoryIndex;
    private bool _onlyRecorderRelated;
    private bool _onlyRecommended;
    private bool _showExamples = true;
    private bool _searchAllWords = true;

    private GUIStyle _titleStyle;
    private GUIStyle _subtitleStyle;
    private GUIStyle _categoryButtonStyle;
    private GUIStyle _selectedCategoryButtonStyle;
    private GUIStyle _cardTitleStyle;
    private GUIStyle _fieldStyle;
    private GUIStyle _wrappedStyle;
    private GUIStyle _miniWrappedStyle;
    private GUIStyle _tagStyle;
    private GUIStyle _codeStyle;
    private GUIStyle _panelStyle;
    private GUIStyle _cardStyle;
    private GUIStyle _cardHeaderStyle;
    private GUIStyle _pillStyle;
    private GUIStyle _activePillStyle;
    private GUIStyle _mutedPillStyle;
    private GUIStyle _searchHintStyle;
    private GUIStyle _sectionBoxStyle;
    private GUIStyle _selectedCategoryAccentStyle;
    private GUIStyle _categoryCountStyle;
    private GUIStyle _selectedCategoryCountStyle;

    [MenuItem("🟡 FFmpeg/🎬 FFmpeg 参数")]
    public static void ShowWindow()
    {
        var window = GetWindow<FFmpegAllInOneParameterGuideWindow>(WINDOW_TITLE);
        window.minSize = new Vector2(1160, 720);
        window.Show();
    }

    private void OnEnable()
    {
        BuildData();
        RebuildCategories();
    }

    private void OnGUI()
    {
        EnsureStyles();

        var fullRect = new Rect(8, 8, position.width - 16, position.height - 16);
        GUILayout.BeginArea(fullRect);

        DrawHeader();
        DrawQuickCommands();
        DrawSearchToolbar();

        EditorGUILayout.Space(6);

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        DrawCategoryList();
        GUILayout.Space(8);
        DrawParameterList();
        EditorGUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical(_panelStyle, GUILayout.Height(TOP_HEADER_HEIGHT));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("FFmpeg 参数大词典", _titleStyle);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(OFFICIAL_DOCUMENT_BUTTON_TEXT, EditorStyles.miniButton, GUILayout.Width(150), GUILayout.Height(24)))
        {
            OpenOfficialDocument();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField(
            "用于快速理解参数含义、适合场景、注意事项和示例。建议先用搜索框输入关键词，例如：mp4、webm、aac、crf、码率、帧率、硬件编码、兼容、低配置。",
            _wrappedStyle);

        EditorGUILayout.EndVertical();
    }

    private void DrawQuickCommands()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("常用命令片段", _fieldStyle);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Windows 录屏 MP4", GUILayout.Height(26)))
        {
            CopyText("-f gdigrab -framerate 30 -i desktop -c:v libx264 -preset veryfast -crf 23 -pix_fmt yuv420p output.mp4");
        }

        if (GUILayout.Button("Windows 录屏 WebM", GUILayout.Height(26)))
        {
            CopyText("-f gdigrab -framerate 30 -i desktop -c:v libvpx -b:v 3M -deadline realtime -cpu-used 8 -pix_fmt yuv420p output.webm");
        }

        if (GUILayout.Button("Linux X11 录屏", GUILayout.Height(26)))
        {
            CopyText("-f x11grab -framerate 30 -video_size 1920x1080 -i :0.0+0,0 -c:v libx264 -preset veryfast -crf 23 -pix_fmt yuv420p output.mp4");
        }

        if (GUILayout.Button("合并音视频", GUILayout.Height(26)))
        {
            CopyText("-i video.mp4 -i audio.wav -map 0:v:0 -map 1:a:0 -c:v copy -c:a aac -b:a 192k -shortest output.mp4");
        }

        if (GUILayout.Button("MP4 快速在线播放", GUILayout.Height(26)))
        {
            CopyText("-i input.mp4 -c copy -movflags +faststart output.mp4");
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawSearchToolbar()
    {
        EditorGUILayout.BeginVertical(_panelStyle, GUILayout.Height(SEARCH_BAR_HEIGHT + 12));
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("搜索", _fieldStyle, GUILayout.Width(42));

        string nextSearch = GUILayout.TextField(_searchText, EditorStyles.toolbarSearchField, GUILayout.MinWidth(320), GUILayout.Height(22));
        if (nextSearch != _searchText)
        {
            _searchText  = nextSearch;
            _rightScroll = Vector2.zero;
        }

        if (GUILayout.Button("清空", EditorStyles.toolbarButton, GUILayout.Width(52), GUILayout.Height(22)))
        {
            _searchText  = string.Empty;
            _rightScroll = Vector2.zero;
        }

        GUILayout.Space(8);
        _onlyRecorderRelated = GUILayout.Toggle(_onlyRecorderRelated, "只看录屏相关", EditorStyles.toolbarButton, GUILayout.Width(104), GUILayout.Height(22));
        _onlyRecommended     = GUILayout.Toggle(_onlyRecommended, "只看推荐", EditorStyles.toolbarButton, GUILayout.Width(78), GUILayout.Height(22));
        _showExamples        = GUILayout.Toggle(_showExamples, "显示示例", EditorStyles.toolbarButton, GUILayout.Width(78), GUILayout.Height(22));
        _searchAllWords      = GUILayout.Toggle(_searchAllWords, "多词全部匹配", EditorStyles.toolbarButton, GUILayout.Width(104), GUILayout.Height(22));

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("复制当前结果 Markdown", EditorStyles.toolbarButton, GUILayout.Width(160), GUILayout.Height(22)))
        {
            CopyText(BuildMarkdown(GetFilteredItems()));
        }

        EditorGUILayout.EndHorizontal();

        if (string.IsNullOrWhiteSpace(_searchText))
        {
            EditorGUILayout.LabelField("搜索描述也能匹配：比如“文件大”“文字不清楚”“Unity 播放”“音画不同步”“低配置”。", _searchHintStyle);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawCategoryList()
    {
        float leftWidth = Mathf.Clamp(position.width * 0.24f, MIN_LEFT_WIDTH, MAX_LEFT_WIDTH);

        EditorGUILayout.BeginVertical(_sectionBoxStyle, GUILayout.Width(leftWidth), GUILayout.ExpandHeight(true));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("分类", _subtitleStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label(_categories.Count.ToString(), _mutedPillStyle, GUILayout.Width(38), GUILayout.Height(22));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);

        _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll, false, true);

        for (int i = 0; i < _categories.Count; i++)
        {
            string category = _categories[i];
            int    count    = GetFilteredItems(category).Count;
            bool   selected = i == _selectedCategoryIndex;

            EditorGUILayout.BeginHorizontal(GUILayout.Height(32));

            GUILayout.Label(string.Empty, selected ? _selectedCategoryAccentStyle : GUIStyle.none, GUILayout.Width(5), GUILayout.Height(28));

            var buttonStyle = selected ? _selectedCategoryButtonStyle : _categoryButtonStyle;
            if (GUILayout.Button(category, buttonStyle, GUILayout.Height(28)))
            {
                _selectedCategoryIndex = i;
                _rightScroll           = Vector2.zero;
            }

            GUILayout.Label(
                count.ToString(),
                selected ? _selectedCategoryCountStyle : _categoryCountStyle,
                GUILayout.Width(48),
                GUILayout.Height(24));

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(2);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(6);

        if (GUILayout.Button("全部分类", GUILayout.Height(28)))
        {
            _selectedCategoryIndex = 0;
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawParameterList()
    {
        EditorGUILayout.BeginVertical(_sectionBoxStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        string currentCategory = GetCurrentCategory();
        var items = currentCategory == "全部"
                        ? GetFilteredItems()
                        : GetFilteredItems(currentCategory);

        EditorGUILayout.BeginHorizontal(_cardHeaderStyle, GUILayout.Height(38));
        EditorGUILayout.LabelField(currentCategory, _titleStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label(items.Count + " 项", _selectedCategoryCountStyle, GUILayout.Width(64), GUILayout.Height(24));
        EditorGUILayout.EndHorizontal();

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            EditorGUILayout.LabelField("搜索：" + _searchText, _miniWrappedStyle);
        }

        EditorGUILayout.Space(4);

        _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll, false, true);

        if (items.Count == 0)
        {
            EditorGUILayout.HelpBox("没有匹配结果。可以尝试取消筛选，或搜索：录屏、码率、crf、aac、webm、mp4、nvenc、字幕、滤镜、推流。", MessageType.Info);
        }
        else
        {
            foreach (var item in items)
            {
                DrawParamCard(item);
                EditorGUILayout.Space(6);
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawParamCard(ParamInfo item)
    {
        EditorGUILayout.BeginVertical(_cardStyle);

        EditorGUILayout.BeginHorizontal(_cardHeaderStyle, GUILayout.MinHeight(32));

        EditorGUILayout.LabelField(item.parameter, _cardTitleStyle);

        if (item.recorderRelated)
        {
            GUILayout.Label("录屏相关", _activePillStyle, GUILayout.Width(72), GUILayout.Height(22));
        }

        if (item.recommended)
        {
            GUILayout.Label("推荐", _activePillStyle, GUILayout.Width(48), GUILayout.Height(22));
        }

        if (!string.IsNullOrWhiteSpace(item.level))
        {
            GUILayout.Label(item.level, _pillStyle, GUILayout.Width(60), GUILayout.Height(22));
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("复制参数名", GUILayout.Width(80), GUILayout.Height(22)))
        {
            CopyText(item.parameter);
        }

        if (!string.IsNullOrWhiteSpace(item.example) && GUILayout.Button("复制示例", GUILayout.Width(72), GUILayout.Height(22)))
        {
            CopyText(item.example);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        DrawTextBlock("分类", item.category);
        DrawTextBlock("常见写法 / 取值", item.values);
        DrawTextBlock("含义", item.meaning);
        DrawTextBlock("适合", item.whenToUse);
        DrawTextBlock("注意", item.warning);
        DrawTextBlock("相关参数", item.related);

        if (_showExamples && !string.IsNullOrWhiteSpace(item.example))
        {
            EditorGUILayout.LabelField("示例：", _fieldStyle);
            EditorGUILayout.SelectableLabel(item.example, _codeStyle, GUILayout.MinHeight(24), GUILayout.MaxHeight(62));
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawTextBlock(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(title, _fieldStyle, GUILayout.Width(92));
        GUILayout.Label(content, _wrappedStyle, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
    }

    private string GetCurrentCategory()
    {
        if (_categories.Count == 0)
        {
            return "全部";
        }

        _selectedCategoryIndex = Mathf.Clamp(_selectedCategoryIndex, 0, _categories.Count - 1);
        return _categories[_selectedCategoryIndex];
    }

    private List<ParamInfo> GetFilteredItems(string category = null)
    {
        IEnumerable<ParamInfo> query = _items;

        if (_onlyRecorderRelated)
        {
            query = query.Where(item => item.recorderRelated);
        }

        if (_onlyRecommended)
        {
            query = query.Where(item => item.recommended);
        }

        if (!string.IsNullOrWhiteSpace(category) && category != "全部")
        {
            query = query.Where(item => item.category == category);
        }

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            string[] words = _searchText
                .Split(new[] { ' ', '\t', ',', '，', ';', '；', '/', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word.Trim())
                .Where(word => !string.IsNullOrEmpty(word))
                .ToArray();

            if (words.Length > 0)
            {
                query = _searchAllWords
                            ? query.Where(item => words.All(word => MatchItem(item, word)))
                            : query.Where(item => words.Any(word => MatchItem(item, word)));
            }
        }

        return query
            .OrderBy(item => item.category)
            .ThenByDescending(item => item.recorderRelated)
            .ThenByDescending(item => item.recommended)
            .ThenBy(item => item.parameter)
            .ToList();
    }

    private static bool MatchItem(ParamInfo item, string keyword)
    {
        return Contains(item.category, keyword) ||
               Contains(item.parameter, keyword) ||
               Contains(item.values, keyword) ||
               Contains(item.meaning, keyword) ||
               Contains(item.whenToUse, keyword) ||
               Contains(item.warning, keyword) ||
               Contains(item.example, keyword) ||
               Contains(item.related, keyword) ||
               Contains(item.keywords, keyword) ||
               Contains(item.level, keyword);
    }

    private static bool Contains(string source, string keyword)
    {
        return !string.IsNullOrEmpty(source) &&
               !string.IsNullOrEmpty(keyword) &&
               source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void RebuildCategories()
    {
        _categories.Clear();
        _categories.Add("全部");
        _categories.AddRange(_items.Select(item => item.category).Distinct().OrderBy(category => category));
    }

    private static void OpenOfficialDocument()
    {
        Application.OpenURL(OFFICIAL_DOCUMENT_URL);
        Debug.Log("[FFmpeg 参数大词典] 打开官方文档：" + OFFICIAL_DOCUMENT_URL);
    }

    private static void CopyText(string text)
    {
        EditorGUIUtility.systemCopyBuffer = text ?? string.Empty;
        Debug.Log("[FFmpeg 参数大词典] 已复制到剪贴板：" + EditorGUIUtility.systemCopyBuffer);
    }

    private static string BuildMarkdown(List<ParamInfo> items)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# FFmpeg 参数大词典搜索结果");
        builder.AppendLine();

        foreach (var group in items.GroupBy(item => item.category))
        {
            builder.AppendLine("## " + group.Key);
            builder.AppendLine();

            foreach (var item in group)
            {
                builder.AppendLine("### " + item.parameter);
                if (!string.IsNullOrWhiteSpace(item.values)) builder.AppendLine("- 常见写法 / 取值：" + item.values);
                builder.AppendLine("- 含义：" + item.meaning);
                if (!string.IsNullOrWhiteSpace(item.whenToUse)) builder.AppendLine("- 适合：" + item.whenToUse);
                if (!string.IsNullOrWhiteSpace(item.warning)) builder.AppendLine("- 注意：" + item.warning);
                if (!string.IsNullOrWhiteSpace(item.related)) builder.AppendLine("- 相关参数：" + item.related);
                if (!string.IsNullOrWhiteSpace(item.example)) builder.AppendLine("- 示例：`" + item.example + "`");
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    private void EnsureStyles()
    {
        if (_titleStyle != null)
        {
            return;
        }

        bool pro = EditorGUIUtility.isProSkin;

        var panelColor = pro
                             ? new Color(0.16f, 0.16f, 0.16f, 1f)
                             : new Color(0.84f, 0.84f, 0.84f, 1f);

        var cardColor = pro
                            ? new Color(0.225f, 0.225f, 0.225f, 1f)
                            : new Color(0.96f, 0.96f, 0.96f, 1f);

        var cardHeaderColor = pro
                                  ? new Color(0.12f, 0.12f, 0.12f, 1f)
                                  : new Color(0.76f, 0.80f, 0.86f, 1f);

        var selectedColor = pro
                                ? new Color(0.12f, 0.34f, 0.58f, 1f)
                                : new Color(0.24f, 0.52f, 0.86f, 1f);

        var selectedHoverColor = pro
                                     ? new Color(0.16f, 0.42f, 0.70f, 1f)
                                     : new Color(0.18f, 0.46f, 0.78f, 1f);

        var normalCategoryColor = pro
                                      ? new Color(0.24f, 0.24f, 0.24f, 1f)
                                      : new Color(0.91f, 0.91f, 0.91f, 1f);

        var normalCategoryHoverColor = pro
                                           ? new Color(0.30f, 0.30f, 0.30f, 1f)
                                           : new Color(0.82f, 0.88f, 0.96f, 1f);

        var textColor         = pro ? Color.white : new Color(0.08f, 0.08f, 0.08f, 1f);
        var selectedTextColor = Color.white;

        var pillColor = pro
                            ? new Color(0.30f, 0.30f, 0.30f, 1f)
                            : new Color(0.80f, 0.80f, 0.80f, 1f);

        var activePillColor = pro
                                  ? new Color(0.08f, 0.46f, 0.70f, 1f)
                                  : new Color(0.12f, 0.42f, 0.72f, 1f);

        var selectedCountColor = pro
                                     ? new Color(0.05f, 0.56f, 0.86f, 1f)
                                     : new Color(0.06f, 0.36f, 0.72f, 1f);

        _panelStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(12, 12, 8, 8),
            margin  = new RectOffset(0, 0, 0, 6)
        };
        _panelStyle.normal.background = MakeTexture(1, 1, panelColor);

        _sectionBoxStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(10, 10, 8, 10),
            margin  = new RectOffset(0, 0, 0, 0)
        };
        _sectionBoxStyle.normal.background = MakeTexture(1, 1, panelColor);

        _cardStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(10, 10, 8, 10),
            margin  = new RectOffset(0, 0, 0, 2)
        };
        _cardStyle.normal.background = MakeTexture(1, 1, cardColor);

        _cardHeaderStyle = new GUIStyle()
        {
            padding = new RectOffset(10, 10, 5, 5),
            margin  = new RectOffset(0, 0, 0, 6)
        };
        _cardHeaderStyle.normal.background = MakeTexture(1, 1, cardHeaderColor);

        _titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 16,
            wordWrap  = true,
            richText  = true,
            alignment = TextAnchor.MiddleLeft,
            normal    = { textColor = textColor }
        };

        _subtitleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 14,
            wordWrap  = true,
            alignment = TextAnchor.MiddleLeft,
            normal    = { textColor = textColor }
        };

        _selectedCategoryAccentStyle                   = new GUIStyle();
        _selectedCategoryAccentStyle.normal.background = MakeTexture(1, 1, selectedColor);

        _categoryButtonStyle = new GUIStyle(EditorStyles.miniButton)
        {
            alignment = TextAnchor.MiddleLeft,
            padding   = new RectOffset(10, 8, 3, 3),
            wordWrap  = false,
            fontSize  = 12,
            fontStyle = FontStyle.Normal
        };
        _categoryButtonStyle.normal.background = MakeTexture(1, 1, normalCategoryColor);
        _categoryButtonStyle.hover.background  = MakeTexture(1, 1, normalCategoryHoverColor);
        _categoryButtonStyle.active.background = MakeTexture(1, 1, selectedHoverColor);
        _categoryButtonStyle.normal.textColor  = textColor;
        _categoryButtonStyle.hover.textColor   = textColor;
        _categoryButtonStyle.active.textColor  = selectedTextColor;

        _selectedCategoryButtonStyle = new GUIStyle(_categoryButtonStyle)
        {
            fontStyle = FontStyle.Bold
        };
        _selectedCategoryButtonStyle.normal.background  = MakeTexture(1, 1, selectedColor);
        _selectedCategoryButtonStyle.hover.background   = MakeTexture(1, 1, selectedHoverColor);
        _selectedCategoryButtonStyle.active.background  = MakeTexture(1, 1, selectedHoverColor);
        _selectedCategoryButtonStyle.focused.background = MakeTexture(1, 1, selectedColor);
        _selectedCategoryButtonStyle.normal.textColor   = selectedTextColor;
        _selectedCategoryButtonStyle.hover.textColor    = selectedTextColor;
        _selectedCategoryButtonStyle.active.textColor   = selectedTextColor;
        _selectedCategoryButtonStyle.focused.textColor  = selectedTextColor;

        _cardTitleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 15,
            richText  = true,
            wordWrap  = true,
            alignment = TextAnchor.MiddleLeft,
            normal    = { textColor = textColor }
        };

        _fieldStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            richText  = true,
            wordWrap  = true,
            alignment = TextAnchor.UpperLeft,
            normal    = { textColor = textColor }
        };

        _wrappedStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
        {
            richText  = true,
            wordWrap  = true,
            alignment = TextAnchor.UpperLeft,
            normal    = { textColor = textColor }
        };

        _miniWrappedStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel)
        {
            richText = true,
            wordWrap = true
        };

        _tagStyle = new GUIStyle(EditorStyles.miniButton)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        _tagStyle.normal.background = MakeTexture(1, 1, activePillColor);
        _tagStyle.normal.textColor  = Color.white;

        _pillStyle = new GUIStyle(EditorStyles.miniButton)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize  = 10,
            padding   = new RectOffset(4, 4, 1, 1)
        };
        _pillStyle.normal.background = MakeTexture(1, 1, pillColor);
        _pillStyle.normal.textColor  = pro ? new Color(0.88f, 0.88f, 0.88f) : new Color(0.12f, 0.12f, 0.12f);

        _activePillStyle                   = new GUIStyle(_pillStyle);
        _activePillStyle.normal.background = MakeTexture(1, 1, activePillColor);
        _activePillStyle.normal.textColor  = Color.white;

        _mutedPillStyle = new GUIStyle(_pillStyle)
        {
            normal =
            {
                textColor = pro ? new Color(0.78f, 0.78f, 0.78f) : new Color(0.32f, 0.32f, 0.32f)
            }
        };

        _categoryCountStyle = new GUIStyle(_pillStyle)
        {
            fontSize = 11
        };

        _selectedCategoryCountStyle = new GUIStyle(_pillStyle)
        {
            fontSize = 11
        };
        _selectedCategoryCountStyle.normal.background = MakeTexture(1, 1, selectedCountColor);
        _selectedCategoryCountStyle.normal.textColor  = Color.white;
        _selectedCategoryCountStyle.fontStyle         = FontStyle.Bold;

        _searchHintStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            richText  = true,
            normal =
            {
                textColor = pro ? new Color(0.74f, 0.74f, 0.74f) : new Color(0.34f, 0.34f, 0.34f)
            }
        };

        _codeStyle = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true,
            fontSize = 12,
            padding  = new RectOffset(7, 7, 5, 5),
            normal =
            {
                textColor = textColor
            }
        };
    }

    private static Texture2D MakeTexture(int width, int height, Color color)
    {
        var pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        var texture = new Texture2D(width, height);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private void BuildData()
    {
        _items.Clear();
        Add("基础 / 全局", "-h / -help", "无值 / encoder=xxx / muxer=xxx / filter=xxx", "查看 FFmpeg 帮助。可以查看全局帮助，也可以查看某个编码器、封装器、滤镜的专属参数。", "想查某个编码器或滤镜支持哪些参数时。", "不同 ffmpeg 版本输出内容不同。",
            "ffmpeg -h encoder=libx264", "ffmpeg -encoders, -filters", true, "入门", "帮助 文档 参数 查询");
        Add("基础 / 全局", "-version", "无值", "显示 ffmpeg 版本、编译信息和启用的库。", "确认当前 ffmpeg 是否支持某些编码器。", "输出很长，但排查问题很有用。", "ffmpeg -version", "-buildconf", true, "入门", "版本 编译");
        Add("基础 / 全局", "-buildconf", "无值", "显示 ffmpeg 编译配置。", "确认是否启用了 libx264、libvpx、libopus、nvenc 等。", "某些发行版会裁剪功能。", "ffmpeg -buildconf", "-version", true, "排查", "编译 支持");
        Add("基础 / 全局", "-formats", "无值", "列出支持的封装格式 / 解封装格式。", "确认支持 mp4、webm、matroska、gdigrab、x11grab、pulse 等。", "D 表示 demuxing，E 表示 muxing。", "ffmpeg -formats", "-muxers, -demuxers", true, "排查",
            "格式 容器 muxer demuxer");
        Add("基础 / 全局", "-muxers", "无值", "列出可输出的封装器。", "确认能否输出 mp4、webm、mkv。", "只代表封装器支持，不代表编码器也支持。", "ffmpeg -muxers", "-formats", false, "排查", "");
        Add("基础 / 全局", "-demuxers", "无值", "列出可读取的输入格式。", "确认能否读取 gdigrab、x11grab、pulse、dshow 等。", "平台相关输入不一定都有。", "ffmpeg -demuxers", "-formats", false, "排查", "");
        Add("基础 / 全局", "-codecs", "无值", "列出支持的编解码器。", "查 H.264、AAC、VP8、VP9、Opus 是否支持。", "编码和解码能力要看标记。", "ffmpeg -codecs", "-encoders, -decoders", true, "排查", "");
        Add("基础 / 全局", "-encoders", "无值", "列出可用编码器。", "确认 libx264、aac、libvpx、h264_nvenc 是否可用。", "有编码器名不代表硬件一定可用。", "ffmpeg -encoders", "-h encoder=xxx", true, "排查", "");
        Add("基础 / 全局", "-decoders", "无值", "列出可用解码器。", "确认某种输入文件能否被解码。", "录屏输出通常更关心 encoders。", "ffmpeg -decoders", "-codecs", false, "排查", "");
        Add("基础 / 全局", "-filters", "无值", "列出可用滤镜。", "确认 scale、crop、fps、volume、aresample 等是否可用。", "滤镜很多，具体参数建议再用 -h filter=xxx。", "ffmpeg -filters", "-h filter=scale", true, "排查", "");
        Add("基础 / 全局", "-devices", "无值", "列出可用输入/输出设备。", "查 gdigrab、dshow、x11grab、pulse、avfoundation 等。", "设备支持强依赖平台和编译选项。", "ffmpeg -devices", "-formats", true, "排查", "");
        Add("基础 / 全局", "-protocols", "无值", "列出支持的协议，如 file、http、rtmp、rtsp、tcp、udp。", "做网络流、拉流、推流时。", "协议支持取决于编译选项。", "ffmpeg -protocols", "-i", false, "流媒体", "");
        Add("基础 / 全局", "-hide_banner", "无值", "隐藏启动横幅和编译信息，使日志更干净。", "Unity Console 嵌入使用。", "排查编译能力时不要隐藏。", "-hide_banner -i input.mp4 output.mp4", "-loglevel", true, "常用", "");
        Add("基础 / 全局", "-loglevel", "quiet / panic / fatal / error / warning / info / verbose / debug / trace", "控制日志详细程度。", "减少 Unity Console 刷屏或排查错误。", "太低看不到错误细节，太高日志太多。",
            "-loglevel error -i input.mp4 output.mp4", "-stats, -nostats", true, "常用", "");
        Add("基础 / 全局", "-report", "无值", "生成详细日志报告文件。", "复杂问题排查。", "会在工作目录生成日志，可能包含本地路径。", "-report -i input.mp4 output.mp4", "-loglevel debug", false, "排查", "");
        Add("基础 / 全局", "-y", "无值", "自动覆盖输出文件。", "脚本自动生成和后台合并。", "可能覆盖已有文件，输出路径最好带时间戳。", "-y -i input.mp4 output.mp4", "-n", true, "常用", "");
        Add("基础 / 全局", "-n", "无值", "如果输出文件存在就不覆盖并失败。", "保护已有文件。", "录屏自动输出时可能导致失败。", "-n -i input.mp4 output.mp4", "-y", false, "常用", "");
        Add("基础 / 全局", "-nostdin", "无值", "禁用从标准输入读取交互命令。", "后台进程、Unity 子进程中更安全。", "如果你依赖按 q 停止 ffmpeg，则不要禁用 stdin。", "-nostdin -i input.mp4 output.mp4", "q 停止", true, "进阶", "");
        Add("基础 / 全局", "-progress", "url / pipe:1", "输出机器可解析的进度信息。", "做进度条 UI。", "需要自己解析 key=value 输出。", "-progress pipe:1 -i input.mp4 output.mp4", "-nostats", false, "进阶", "");
        Add("基础 / 全局", "-stats", "无值", "输出进度统计。", "命令行看进度。", "Unity Console 可能刷屏。", "-stats -i input.mp4 output.mp4", "-nostats", false, "常用", "");
        Add("基础 / 全局", "-nostats", "无值", "不输出持续刷新的统计信息。", "Unity 后台合并，减少日志。", "看不到实时进度。", "-nostats -i input.mp4 output.mp4", "-loglevel", true, "常用", "");
        Add("基础 / 全局", "-benchmark", "无值", "输出转码耗时和资源统计。", "性能对比。", "正式录制一般不用。", "-benchmark -i input.mp4 output.mp4", "-benchmark_all", false, "排查", "");
        Add("基础 / 全局", "-threads", "0 / 1 / 2 / 4 / 8", "设置线程数量。0 通常表示自动。", "限制 CPU 或提高转码速度。", "不是所有编码器都完全遵守。", "-threads 4 -i input.mp4 output.mp4", "-preset, -cpu-used", true, "进阶", "");
        Add("输入 / 输出", "-i", "文件 / 设备 / URL / desktop / :0.0", "指定输入源。可以是视频、音频、设备、网络流。", "几乎所有命令都需要。", "多个 -i 时输入编号从 0 开始。", "-i input.mp4 output.mp4", "-map", true, "核心", "");
        Add("输入 / 输出", "-f", "gdigrab / x11grab / pulse / dshow / mp4 / webm / null", "强制指定输入或输出格式。放在 -i 前通常是输入格式。", "设备采集、指定容器。", "位置非常重要。", "-f gdigrab -i desktop output.mp4", "-i", true, "核心", "");
        Add("输入 / 输出", "-map", "0:v:0 / 1:a:0 / 0 / -0:s", "选择输入流进入输出。", "合并音视频、指定音轨、去字幕。", "不写时 ffmpeg 自动选流，不一定符合预期。", "-map 0:v:0 -map 1:a:0", "-shortest", true, "核心", "");
        Add("输入 / 输出", "-map_chapters", "0 / -1", "复制或禁用章节信息。-1 表示不复制。", "处理带章节的视频。", "录屏通常用不到。", "-map_chapters -1", "-map_metadata", false, "进阶", "");
        Add("输入 / 输出", "-map_metadata", "0 / -1", "复制或禁用元数据。", "去除原文件元数据。", "不会影响画面声音。", "-map_metadata -1", "-metadata", false, "进阶", "");
        Add("输入 / 输出", "-an", "无值", "禁用音频输出。", "只要视频。", "会丢掉所有音频。", "-i input.mp4 -an output.mp4", "-vn", true, "常用", "");
        Add("输入 / 输出", "-vn", "无值", "禁用视频输出。", "只提取音频。", "会丢掉视频。", "-i input.mp4 -vn -c:a aac audio.m4a", "-an", true, "常用", "");
        Add("输入 / 输出", "-sn", "无值", "禁用字幕输出。", "去掉字幕流。", "不影响烧录到画面里的字幕。", "-i input.mkv -sn output.mp4", "-map", false, "常用", "");
        Add("输入 / 输出", "-dn", "无值", "禁用数据流输出。", "去掉特殊 data stream。", "普通录屏少用。", "-i input.mkv -dn output.mp4", "-map", false, "进阶", "");
        Add("输入 / 输出", "-stream_loop", "-1 / 0 / 1 / 2", "循环输入。-1 表示无限循环。", "循环背景视频或音频。", "用于实时命令时要注意停止条件。", "-stream_loop -1 -i bg.mp4", "-t", false, "进阶", "");
        Add("输入 / 输出", "-readrate", "1 / 0.5 / 2", "按指定速率读取输入，1 接近实时。", "模拟实时输入。", "不是录屏采集帧率。", "-readrate 1 -i input.mp4", "-re", false, "进阶", "");
        Add("输入 / 输出", "-re", "无值", "按输入原始帧率实时读取。", "推流时模拟实时输入。", "对真实设备采集通常不需要。", "-re -i input.mp4 -f flv rtmp://...", "-readrate", false, "流媒体", "");
        Add("录屏采集", "-f gdigrab", "Windows 输入格式", "Windows 下抓取桌面画面。", "Windows 桌面录屏。", "不同 Windows 缩放/多屏布局要测试。", "-f gdigrab -framerate 30 -i desktop output.mp4", "-framerate, -video_size", true,
            "录屏", "");
        Add("录屏采集", "-i desktop", "gdigrab 输入名", "gdigrab 下表示采集整个桌面。", "Windows 全屏桌面录制。", "多显示器时配合 offset/video_size 选区域。", "-f gdigrab -i desktop output.mp4", "-offset_x, -offset_y", true, "录屏", "");
        Add("录屏采集", "-framerate", "15 / 24 / 30 / 60", "输入采集帧率。", "录屏时控制抓帧频率。", "不要和输出侧 -r 混淆。", "-framerate 30", "-r, fps", true, "录屏", "");
        Add("录屏采集", "-video_size", "1920x1080 / 1280x720", "输入采集区域大小。", "指定显示器或区域录制。", "分辨率越高编码压力越大。", "-video_size 1920x1080", "-offset_x, -offset_y", true, "录屏", "");
        Add("录屏采集", "-offset_x", "0 / 1920", "gdigrab 区域左上角 X 坐标。", "多显示器、区域录制。", "坐标依赖系统显示器布局。", "-offset_x 1920", "-offset_y", true, "录屏", "");
        Add("录屏采集", "-offset_y", "0 / 1080", "gdigrab 区域左上角 Y 坐标。", "多显示器、区域录制。", "坐标不对会录错区域。", "-offset_y 0", "-offset_x", true, "录屏", "");
        Add("录屏采集", "-draw_mouse", "0 / 1", "是否录制鼠标指针。", "教程录屏通常开启。", "某些场景鼠标绘制可能和实际略有差异。", "-draw_mouse 1", "-f gdigrab", true, "录屏", "");
        Add("录屏采集", "-show_region", "0 / 1", "显示录制区域边框。", "调试录制区域。", "正式录制不建议开。", "-show_region 1", "-video_size", false, "调试", "");
        Add("录屏采集", "-f x11grab", "Linux X11 输入格式", "Linux X11 下抓屏。", "Linux X11 桌面录制。", "Wayland 下可能不可用。", "-f x11grab -i :0.0+0,0", "-video_size", true, "录屏", "");
        Add("录屏采集", ":0.0+X,Y", "X11 输入坐标", "X11 显示器和偏移坐标。", "Linux 区域录制。", "DISPLAY 不一定是 :0.0。", "-i :0.0+0,0", "-f x11grab", true, "录屏", "");
        Add("录屏采集", "-f pulse", "Linux PulseAudio 输入", "从 PulseAudio 采集音频。", "Linux 系统声音/麦克风录制。", "系统声音通常要选择 .monitor 源。", "-f pulse -i default", "-ar, -ac", true, "录音", "");
        Add("录屏采集", "-f dshow", "Windows DirectShow 输入", "采集摄像头/麦克风设备。", "Windows 摄像头和麦克风录制。", "设备名需要准确。", "-f dshow -i video=\"Camera\":audio=\"Microphone\"", "-list_devices", false, "设备", "");
        Add("录屏采集", "-list_devices true", "dshow 设备列表", "列出 DirectShow 设备。", "查询摄像头/麦克风名称。", "命令通常不会正常输出视频，只用于查询。", "-f dshow -list_devices true -i dummy", "-f dshow", false, "设备", "");
        Add("录屏采集", "-thread_queue_size", "128 / 512 / 1024", "输入队列大小，防止多输入时来不及处理。", "录屏+录音、多输入合并。", "过大占内存，过小可能丢包。", "-thread_queue_size 512 -f pulse -i default", "-map", true, "录制", "");
        Add("录屏采集", "-rtbufsize", "100M / 256M / 512M", "实时设备输入缓冲大小。", "dshow 摄像头/麦克风采集。", "gdigrab 不一定需要。", "-rtbufsize 256M -f dshow -i video=\"Camera\"", "-f dshow", false, "设备", "");
        Add("视频编码", "-c:v", "libx264 / h264_nvenc / libvpx / copy", "指定视频编码器。", "决定视频如何压缩。", "编码器可用性取决于 ffmpeg 编译和硬件。", "-c:v libx264", "-codec:v, -vcodec", true, "核心", "");
        Add("视频编码", "-codec:v", "同 -c:v", "-c:v 的完整写法。", "想写得明确时。", "一般用 -c:v 更短。", "-codec:v libx264", "-c:v", false, "核心", "");
        Add("视频编码", "-vcodec", "同 -c:v", "旧式视频编码器写法。", "兼容旧命令。", "新命令建议用 -c:v。", "-vcodec libx264", "-c:v", false, "旧写法", "");
        Add("视频编码", "libx264", "作为 -c:v 的值", "H.264 软件编码器，兼容性最好。", "MP4 通用录屏输出。", "CPU 编码，分辨率高时吃 CPU。", "-c:v libx264 -preset veryfast -crf 23", "-preset, -crf", true, "核心", "");
        Add("视频编码", "libx265", "作为 -c:v 的值", "H.265/HEVC 软件编码器，压缩效率更高。", "离线高压缩。", "编码慢，兼容性不如 H.264。", "-c:v libx265 -crf 26", "-tag:v hvc1", false, "进阶", "");
        Add("视频编码", "-preset", "ultrafast / veryfast / fast / medium / slow", "编码速度预设。越快越省 CPU，但同质量文件更大。", "实时录屏常用 ultrafast/veryfast。", "越慢越可能卡顿。", "-preset veryfast", "-crf", true, "核心", "");
        Add("视频编码", "-crf", "16~35，常用 18/20/23/26", "恒定质量模式。越小越清晰，文件越大。", "libx264/libx265 质量控制。", "不适用于所有编码器。", "-crf 23", "-preset, -b:v", true, "核心", "");
        Add("视频编码", "-b:v", "1M / 3M / 5M / 8000k", "视频码率。越高越清晰，文件越大。", "WebM、硬件编码、固定码率控制。", "太低会糊，太高浪费体积。", "-b:v 3M", "-maxrate, -bufsize", true, "核心", "");
        Add("视频编码", "-minrate", "1M / 3M", "最小码率。", "CBR/码率约束场景。", "普通录屏少用。", "-minrate 3M -maxrate 3M -bufsize 6M", "-maxrate", false, "进阶", "");
        Add("视频编码", "-maxrate", "3M / 5M / 8M", "最大码率限制。", "限制码率峰值。", "太低影响复杂画面质量。", "-maxrate 4M", "-bufsize", false, "进阶", "");
        Add("视频编码", "-bufsize", "2M / 8M / 16M", "码率控制缓冲区。", "和 maxrate 配合控制波动。", "不是所有场景需要。", "-maxrate 4M -bufsize 8M", "-maxrate", false, "进阶", "");
        Add("视频编码", "-pix_fmt", "yuv420p / yuv422p / yuv444p / nv12", "像素格式。yuv420p 最兼容。", "最终输出推荐 yuv420p。", "yuv444p 可能播放器不兼容。", "-pix_fmt yuv420p", "format filter", true, "核心", "");
        Add("视频编码", "-r", "30 / 60", "输出帧率。", "统一输出帧率。", "录屏输入帧率应优先用 -framerate。", "-r 30", "-framerate, fps", false, "常用", "");
        Add("视频编码", "-g", "30 / 60 / 120", "关键帧间隔 GOP。", "控制拖动进度、低延迟、码率稳定。", "关键帧越频繁文件可能越大。", "-g 60", "-keyint_min", false, "进阶", "");
        Add("视频编码", "-keyint_min", "1 / 30 / 60", "最小关键帧间隔。", "配合 GOP 控制。", "普通录屏不常用。", "-g 60 -keyint_min 60", "-g", false, "进阶", "");
        Add("视频编码", "-sc_threshold", "0 / 40", "场景切换触发关键帧阈值。0 常用于禁止场景切换插关键帧。", "直播/固定 GOP。", "乱设会影响编码效率。", "-sc_threshold 0", "-g", false, "进阶", "");
        Add("视频编码", "-tune", "zerolatency / film / animation / grain / fastdecode", "x264 调优模式。", "实时低延迟用 zerolatency。", "可能牺牲压缩效率。", "-tune zerolatency", "-preset", false, "进阶", "");
        Add("视频编码", "-profile:v", "baseline / main / high", "H.264 Profile。", "兼容特定设备。", "baseline 兼容好但压缩弱。", "-profile:v high", "-level", false, "进阶", "");
        Add("视频编码", "-level", "3.1 / 4.0 / 4.1 / 5.1", "H.264 Level。", "兼容特定播放器/硬件。", "不匹配分辨率帧率可能出问题。", "-level 4.1", "-profile:v", false, "进阶", "");
        Add("视频编码", "-c:v copy", "copy", "不重新编码视频，直接复制。", "快速合并/换容器。", "不能改变画质、分辨率、像素格式。", "-c:v copy", "-c:a copy", true, "常用", "");
        Add("硬件编码", "h264_nvenc", "NVIDIA H.264", "NVIDIA 硬件 H.264 编码器。", "NVIDIA 显卡实时录屏。", "需要驱动和 ffmpeg 支持。", "-c:v h264_nvenc -b:v 5M", "hevc_nvenc", false, "硬件", "");
        Add("硬件编码", "hevc_nvenc", "NVIDIA HEVC", "NVIDIA 硬件 H.265 编码器。", "想减小体积且目标支持 HEVC。", "兼容性不如 H.264。", "-c:v hevc_nvenc -b:v 5M", "h264_nvenc", false, "硬件", "");
        Add("硬件编码", "h264_qsv", "Intel QSV H.264", "Intel Quick Sync 硬件编码。", "Intel 核显机器。", "需要硬件、驱动、ffmpeg 支持。", "-c:v h264_qsv -b:v 5M", "hevc_qsv", false, "硬件", "");
        Add("硬件编码", "hevc_qsv", "Intel QSV HEVC", "Intel Quick Sync HEVC 编码。", "Intel 平台 HEVC。", "兼容性和驱动要测试。", "-c:v hevc_qsv -b:v 5M", "h264_qsv", false, "硬件", "");
        Add("硬件编码", "h264_amf", "AMD AMF H.264", "AMD 硬件 H.264 编码器。", "AMD 显卡机器。", "需要 AMD 驱动和 ffmpeg 支持。", "-c:v h264_amf -b:v 5M", "hevc_amf", false, "硬件", "");
        Add("硬件编码", "hevc_amf", "AMD AMF HEVC", "AMD 硬件 H.265 编码器。", "AMD 显卡 HEVC。", "兼容性不如 H.264。", "-c:v hevc_amf -b:v 5M", "h264_amf", false, "硬件", "");
        Add("硬件编码", "-hwaccel", "auto / cuda / qsv / dxva2 / d3d11va / vaapi", "硬件解码加速。", "解码输入文件时减轻 CPU。", "录屏采集编码不一定需要。", "-hwaccel auto -i input.mp4", "-c:v h264_nvenc", false, "硬件", "");
        Add("硬件编码", "-hwaccel_device", "0 / 1", "指定硬件加速设备。", "多 GPU 场景。", "设备编号要确认。", "-hwaccel cuda -hwaccel_device 0", "-hwaccel", false, "硬件", "");
        Add("硬件编码", "-gpu", "0 / 1 / any", "指定 NVENC GPU。", "多 NVIDIA 显卡。", "不适用于所有编码器。", "-c:v h264_nvenc -gpu 0", "h264_nvenc", false, "硬件", "");
        Add("硬件编码", "-cq", "18 / 23 / 28", "部分硬件编码器恒定质量参数。", "NVENC 质量控制。", "不同编码器含义不同。", "-c:v h264_nvenc -cq 23", "-rc", false, "硬件", "");
        Add("硬件编码", "-rc", "cbr / vbr / constqp", "硬件编码码率控制模式。", "控制码率策略。", "各编码器支持值不同。", "-c:v h264_nvenc -rc vbr", "-b:v, -cq", false, "硬件", "");
        Add("WebM / VPx", "libvpx", "VP8", "VP8 编码器，WebM 常用。", "实时 WebM 录屏。", "压缩效率不如 VP9。", "-c:v libvpx -b:v 3M", "-deadline, -cpu-used", true, "WebM", "");
        Add("WebM / VPx", "libvpx-vp9", "VP9", "VP9 编码器，压缩效率更好。", "高压缩或机器较强。", "实时编码更吃 CPU。", "-c:v libvpx-vp9 -b:v 3M", "-row-mt", true, "WebM", "");
        Add("WebM / VPx", "-deadline", "best / good / realtime", "VPx 编码速度模式。", "WebM 实时录屏用 realtime。", "best 不适合实时。", "-deadline realtime", "-cpu-used", true, "WebM", "");
        Add("WebM / VPx", "-cpu-used", "0~8", "VPx 速度/质量取舍，越大越快。", "实时 WebM 常用 6/8。", "越大画质越弱。", "-cpu-used 8", "-deadline", true, "WebM", "");
        Add("WebM / VPx", "-row-mt", "0 / 1", "VP9 行多线程。", "加速 VP9 编码。", "版本支持要测试。", "-row-mt 1", "-threads", false, "WebM", "");
        Add("WebM / VPx", "-lag-in-frames", "0 / 16 / 25", "编码前瞻帧数。", "离线压缩可增大。", "实时录制常设 0。", "-lag-in-frames 0", "-deadline", false, "WebM", "");
        Add("WebM / VPx", "-auto-alt-ref", "0 / 1", "VPx 备用参考帧。", "离线提高压缩效率。", "实时低延迟可能关闭。", "-auto-alt-ref 0", "-lag-in-frames", false, "WebM", "");
        Add("WebM / VPx", "-crf", "VP9 常用 15~40", "VP9 也支持 CRF 风格质量控制。", "VP9 质量控制。", "常和 -b:v 0 配合。", "-c:v libvpx-vp9 -b:v 0 -crf 30", "-b:v", false, "WebM", "");
        Add("音频编码", "-c:a", "aac / libopus / libvorbis / copy / pcm_s16le", "指定音频编码器。", "最终音频压缩。", "容器和编码器要匹配。", "-c:a aac -b:a 192k", "-b:a, -ar, -ac", true, "核心", "");
        Add("音频编码", "aac", "AAC 编码器", "MP4 最常见音频编码。", "MP4 通用输出。", "WebM 不推荐 AAC。", "-c:a aac", "audioBitrate", true, "核心", "");
        Add("音频编码", "libopus", "Opus 编码器", "现代高效率音频编码。", "WebM、语音、低码率。", "老播放器可能不支持。", "-c:a libopus -b:a 128k", "webmAudioCodec", true, "核心", "");
        Add("音频编码", "libvorbis", "Vorbis 编码器", "WebM 常见音频编码。", "WebM 兼容方案。", "同码率通常不如 Opus 高效。", "-c:a libvorbis -b:a 192k", "webmAudioCodec", true, "核心", "");
        Add("音频编码", "pcm_s16le", "PCM 16-bit", "未压缩音频。", "临时 wav、调试。", "最终文件很大。", "-c:a pcm_s16le", "wav", false, "进阶", "");
        Add("音频编码", "-b:a", "64k / 96k / 128k / 192k / 256k / 320k", "音频码率。", "录屏常用 128k~192k。", "过高意义不大。", "-b:a 192k", "-c:a", true, "核心", "");
        Add("音频编码", "-ar", "44100 / 48000 / 96000", "音频采样率。", "视频/游戏录制推荐 48000。", "无意义提高会增大压力。", "-ar 48000", "-ac", true, "核心", "");
        Add("音频编码", "-ac", "1 / 2 / 6", "音频声道数。", "普通录屏推荐 2。", "1 会丢方向感。", "-ac 2", "-ar", true, "核心", "");
        Add("音频编码", "-sample_fmt", "s16 / fltp / s32", "音频采样格式。", "专业音频或兼容问题。", "普通录屏少用。", "-sample_fmt s16", "-ar", false, "进阶", "");
        Add("音频编码", "-vol", "整数，旧参数", "旧式音量参数。", "老命令兼容。", "新命令建议用 volume 滤镜。", "-vol 256", "volume filter", false, "进阶", "");
        Add("音频编码", "-c:a copy", "copy", "直接复制音频流。", "换容器/合并且音频已兼容。", "WAV 合并到 MP4/WebM 通常需要编码。", "-c:a copy", "-c:v copy", true, "核心", "");
        Add("滤镜", "-vf", "scale=... / crop=... / fps=... / format=...", "视频滤镜链。", "缩放、裁剪、帧率、像素格式。", "会触发重新编码，不能配合 -c:v copy 改画面。", "-vf scale=1280:720", "-filter:v", true, "滤镜", "");
        Add("滤镜", "-filter:v", "同 -vf", "-vf 的完整写法。", "明确写视频滤镜。", "一般 -vf 更短。", "-filter:v scale=1280:720", "-vf", false, "滤镜", "");
        Add("滤镜", "-filter_complex", "复杂滤镜图", "处理多输入、多输出滤镜。", "画中画、混音、拼接、叠加。", "语法复杂，调试成本高。", "-filter_complex \"[0:v][1:v]overlay=10:10\"", "overlay, amix", false, "滤镜", "");
        Add("滤镜", "scale", "scale=w:h / scale=-2:720", "缩放视频。", "降低分辨率或统一尺寸。", "文字会变糊。", "-vf scale=-2:720", "setsar", true, "滤镜", "");
        Add("滤镜", "fps", "fps=30 / fps=60", "通过滤镜改变帧率。", "输出固定帧率。", "录屏输入仍用 -framerate。", "-vf fps=30", "-r", false, "滤镜", "");
        Add("滤镜", "crop", "crop=w:h:x:y", "裁剪画面。", "只保留一部分画面。", "参数错会裁错区域。", "-vf crop=1280:720:0:0", "pad", false, "滤镜", "");
        Add("滤镜", "pad", "pad=w:h:x:y:color", "补边扩展画布。", "补成固定比例。", "画面会加边框。", "-vf pad=1920:1080:(ow-iw)/2:(oh-ih)/2:black", "crop", false, "滤镜", "");
        Add("滤镜", "format", "format=yuv420p", "滤镜中转换像素格式。", "滤镜链最后保证兼容。", "也可用 -pix_fmt。", "-vf format=yuv420p", "-pix_fmt", true, "滤镜", "");
        Add("滤镜", "setsar", "setsar=1", "设置像素宽高比。", "修正画面拉伸。", "普通录屏少用。", "-vf scale=1280:720,setsar=1", "setdar", false, "滤镜", "");
        Add("滤镜", "setdar", "setdar=16/9", "设置显示宽高比。", "修正显示比例。", "不要和真实分辨率混淆。", "-vf setdar=16/9", "setsar", false, "滤镜", "");
        Add("滤镜", "transpose", "transpose=1", "旋转视频 90 度。", "竖屏/横屏转换。", "参数值表示方向，要测试。", "-vf transpose=1", "rotate", false, "滤镜", "");
        Add("滤镜", "rotate", "rotate=PI/2", "按角度旋转。", "任意角度旋转。", "会产生黑边或裁切。", "-vf rotate=PI/2", "transpose", false, "滤镜", "");
        Add("滤镜", "hflip / vflip", "无值", "水平/垂直翻转。", "镜像画面。", "录屏一般少用。", "-vf hflip", "", false, "滤镜", "");
        Add("滤镜", "overlay", "overlay=x:y", "叠加一个视频/图片到另一个上。", "水印、画中画。", "需要 filter_complex。", "-filter_complex \"[0:v][1:v]overlay=10:10\"", "filter_complex", false, "滤镜", "");
        Add("滤镜", "drawtext", "text=...:x=...:y=...", "在画面上绘制文字。", "加时间、水印、字幕。", "需要字体路径，跨平台麻烦。", "-vf \"drawtext=text='Hello':x=10:y=10\"", "fontfile", false, "滤镜", "");
        Add("滤镜", "drawbox", "x:y:w:h:color", "画矩形框。", "标注区域。", "会改变画面。", "-vf drawbox=x=10:y=10:w=200:h=100:color=red", "drawtext", false, "滤镜", "");
        Add("滤镜", "setpts", "setpts=PTS/2 / 2*PTS", "修改视频速度。", "倍速/慢放。", "音频要配合 atempo。", "-vf setpts=PTS/2", "atempo", false, "滤镜", "");
        Add("滤镜", "-af", "volume=... / atempo=... / aresample=...", "音频滤镜链。", "调音量、重采样、音频倍速。", "会触发音频重新编码。", "-af volume=1.5", "-filter:a", true, "滤镜", "");
        Add("滤镜", "-filter:a", "同 -af", "-af 的完整写法。", "明确写音频滤镜。", "一般 -af 更短。", "-filter:a volume=1.5", "-af", false, "滤镜", "");
        Add("滤镜", "volume", "volume=0.5 / 1.5 / 2.0", "调整音量。", "声音太小/太大时。", "过大可能爆音。", "-af volume=1.5", "-af", true, "滤镜", "");
        Add("滤镜", "atempo", "0.5~2.0", "调整音频速度。", "音频倍速/慢放。", "超过范围要串联多个 atempo。", "-af atempo=1.25", "setpts", false, "滤镜", "");
        Add("滤镜", "aresample", "aresample=48000", "音频重采样。", "修正采样率或同步。", "普通场景可直接用 -ar。", "-af aresample=48000", "-ar", false, "滤镜", "");
        Add("滤镜", "amix", "inputs=2", "混合多个音频输入。", "合并麦克风和系统声音。", "需要 filter_complex。", "-filter_complex \"[0:a][1:a]amix=inputs=2\"", "filter_complex", false, "滤镜", "");
        Add("滤镜", "adelay", "1000|1000", "延迟音频。", "手动对齐音画。", "单位和声道写法要注意。", "-af adelay=500|500", "-itsoffset", false, "滤镜", "");
        Add("滤镜", "loudnorm", "I=-16:TP=-1.5:LRA=11", "响度标准化。", "统一音量。", "参数复杂，处理较慢。", "-af loudnorm=I=-16:TP=-1.5:LRA=11", "volume", false, "滤镜", "");
        Add("同步 / 时长", "-shortest", "无值", "最短输入结束时停止输出。", "合并音视频避免尾部多余。", "某条流异常短会截断输出。", "-shortest", "-map", true, "常用", "");
        Add("同步 / 时长", "-ss", "5 / 00:00:05", "跳转到指定时间。", "截取片段。", "放在 -i 前后精度和速度不同。", "-ss 5 -i input.mp4", "-t, -to", true, "常用", "");
        Add("同步 / 时长", "-t", "10 / 00:01:00", "输出持续时长。", "截取固定长度。", "和 -to 不同，-t 是时长。", "-ss 5 -i input.mp4 -t 10 output.mp4", "-ss", true, "常用", "");
        Add("同步 / 时长", "-to", "00:01:00", "输出到某个结束时间点。", "截取到指定时间。", "和 -t 不要混淆。", "-i input.mp4 -to 00:01:00 output.mp4", "-t", false, "进阶", "");
        Add("同步 / 时长", "-itsoffset", "0.5 / -0.2", "给某个输入加时间偏移。", "音画不同步校正。", "要放在对应 -i 前。", "-i video.mp4 -itsoffset 0.5 -i audio.wav", "-map", false, "进阶", "");
        Add("同步 / 时长", "-copyts", "无值", "保留输入时间戳。", "复杂同步场景。", "普通录屏少用。", "-copyts -i input.mp4 output.mp4", "-start_at_zero", false, "进阶", "");
        Add("同步 / 时长", "-start_at_zero", "无值", "配合 copyts 让时间戳从 0 开始。", "修正时间轴。", "不熟悉不要乱用。", "-copyts -start_at_zero", "-copyts", false, "进阶", "");
        Add("同步 / 时长", "-avoid_negative_ts", "make_zero / disabled / auto", "避免负时间戳。", "合并后时间轴异常。", "普通少用。", "-avoid_negative_ts make_zero", "-copyts", false, "进阶", "");
        Add("同步 / 时长", "-fps_mode", "auto / passthrough / cfr / vfr", "控制帧率模式。", "固定/可变帧率处理。", "新版本参数，旧版本可能没有。", "-fps_mode cfr", "-vsync", false, "进阶", "");
        Add("同步 / 时长", "-vsync", "0 / 1 / 2 / vfr / cfr", "旧式视频同步模式。", "旧版本 ffmpeg。", "新版本建议关注 fps_mode。", "-vsync 2", "-fps_mode", false, "进阶", "");
        Add("容器 / 封装", "-movflags +faststart", "+faststart", "把 MP4 元数据移动到文件开头，便于边下载边播放。", "MP4 网络播放/上传。", "只适合 MP4/MOV 类容器。", "-movflags +faststart", "-c copy", true, "容器", "");
        Add("容器 / 封装", "-f mp4", "mp4", "强制输出 MP4 容器。", "扩展名不明确时。", "通常 .mp4 自动识别。", "-f mp4 output.mp4", "-movflags", false, "容器", "");
        Add("容器 / 封装", "-f webm", "webm", "强制输出 WebM 容器。", "WebM 输出。", "通常搭配 VP8/VP9 + Vorbis/Opus。", "-f webm output.webm", "libvpx", false, "容器", "");
        Add("容器 / 封装", "-f matroska", "mkv / matroska", "输出 MKV 容器。", "中间文件、多轨字幕。", "平台兼容不如 MP4 普及。", "-f matroska output.mkv", "-map", false, "容器", "");
        Add("容器 / 封装", "-metadata", "title=xxx / artist=xxx", "写入元数据。", "设置标题作者等。", "不影响画面声音。", "-metadata title=\"Recording\"", "-map_metadata", false, "容器", "");
        Add("容器 / 封装", "-brand", "mp42 / isom", "设置 MP4 brand。", "特定兼容需求。", "普通不需要。", "-brand mp42", "-movflags", false, "容器", "");
        Add("容器 / 封装", "-tag:v", "avc1 / hvc1", "设置视频流标签。", "HEVC 在 Apple 设备兼容常用 hvc1。", "乱设可能导致播放器识别错误。", "-tag:v hvc1", "libx265", false, "容器", "");
        Add("容器 / 封装", "-frag_duration", "微秒", "分片 MP4 片段时长。", "流式/分片输出。", "普通录屏不需要。", "-movflags frag_keyframe -frag_duration 1000000", "-movflags", false, "容器", "");
        Add("字幕", "-c:s", "copy / srt / mov_text / ass", "指定字幕编码器。", "保留或转换字幕。", "MP4 常用 mov_text；硬字幕不走字幕流。", "-c:s mov_text", "-sn", false, "字幕", "");
        Add("字幕", "-scodec", "同 -c:s", "旧式字幕编码器写法。", "旧命令兼容。", "新命令建议 -c:s。", "-scodec copy", "-c:s", false, "字幕", "");
        Add("字幕", "-vf subtitles", "subtitles=file.srt", "把字幕烧录到视频画面里。", "需要硬字幕。", "烧录后无法关闭字幕，且会重新编码视频。", "-vf subtitles=sub.srt", "ass", false, "字幕", "");
        Add("字幕", "-vf ass", "ass=file.ass", "烧录 ASS 字幕。", "复杂样式字幕。", "需要 libass 支持。", "-vf ass=sub.ass", "subtitles", false, "字幕", "");
        Add("字幕", "-fix_sub_duration", "无值", "修复部分字幕持续时间。", "字幕时间轴异常时。", "普通录屏不用。", "-fix_sub_duration", "-c:s", false, "字幕", "");
        Add("流媒体 / 网络", "-f flv", "flv", "输出 FLV 容器，常用于 RTMP 推流。", "直播推流。", "普通本地录屏不需要。", "-f flv rtmp://server/live/key", "-re", false, "流媒体", "");
        Add("流媒体 / 网络", "-rtsp_transport", "tcp / udp / http", "RTSP 使用的传输协议。", "拉 RTSP 摄像头流。", "TCP 稳定，UDP 延迟低但可能丢包。", "-rtsp_transport tcp -i rtsp://...", "-stimeout", false, "流媒体", "");
        Add("流媒体 / 网络", "-timeout", "微秒", "网络输入超时。", "网络流防止卡死。", "不同协议支持不同。", "-timeout 5000000 -i http://...", "-rw_timeout", false, "流媒体", "");
        Add("流媒体 / 网络", "-rw_timeout", "微秒", "读写超时。", "网络协议读写卡住时。", "协议支持不一。", "-rw_timeout 5000000 -i rtsp://...", "-timeout", false, "流媒体", "");
        Add("流媒体 / 网络", "-listen", "1", "作为服务器监听输入。", "接收推流。", "普通录屏不用。", "-listen 1 -i rtmp://0.0.0.0/live", "rtmp", false, "流媒体", "");
        Add("流媒体 / 网络", "-user_agent", "字符串", "设置 HTTP User-Agent。", "某些网络资源需要伪装客户端。", "普通录屏不用。", "-user_agent \"Mozilla/5.0\" -i http://...", "headers", false, "流媒体", "");
        Add("流媒体 / 网络", "-headers", "多行 Header", "设置 HTTP 请求头。", "访问需要特殊 Header 的资源。", "注意转义换行。", "-headers \"Referer: xxx\" -i http://...", "user_agent", false, "流媒体", "");
        Add("流媒体 / 网络", "-reconnect", "1", "网络断开后重连。", "拉 HTTP 流。", "只对部分协议有效。", "-reconnect 1 -i http://...", "reconnect_streamed", false, "流媒体", "");
        Add("调试 / 分析", "-f null -", "null 输出", "不生成输出文件，只处理流程。", "测试解码/速度/错误。", "没有实际文件产物。", "-i input.mp4 -f null -", "-benchmark", true, "排查", "");
        Add("调试 / 分析", "-probesize", "32M / 100M", "探测输入格式读取的数据量。", "输入识别不完整时增大。", "增加启动耗时。", "-probesize 100M -i input.ts", "-analyzeduration", false, "排查", "");
        Add("调试 / 分析", "-analyzeduration", "10M / 100M", "探测输入持续时间。", "音视频流识别不全时。", "增加启动耗时。", "-analyzeduration 100M -i input.ts", "-probesize", false, "排查", "");
        Add("调试 / 分析", "-err_detect", "ignore_err / explode", "错误检测策略。", "坏文件读取。", "可能掩盖问题。", "-err_detect ignore_err -i broken.mp4", "", false, "排查", "");
        Add("调试 / 分析", "-xerror", "无值", "遇到错误立即退出。", "自动化任务希望失败即停。", "容错性降低。", "-xerror -i input.mp4 output.mp4", "-err_detect", false, "排查", "");
        Add("调试 / 分析", "-dump", "无值", "dump 输入包。", "底层调试。", "日志巨大。", "-dump -i input.mp4 -f null -", "-hex", false, "排查", "");
        Add("调试 / 分析", "-hex", "无值", "以十六进制 dump。", "底层调试。", "普通不用。", "-dump -hex -i input.mp4", "-dump", false, "排查", "");
        Add("图片 / 序列帧", "-frames:v", "1 / 100", "限制输出视频帧数。", "截一张图或导出固定帧。", "录屏不常用。", "-frames:v 1 cover.png", "-ss", false, "图片", "");
        Add("图片 / 序列帧", "-f image2", "image2", "图片序列格式。", "导出/读取序列帧。", "文件数量可能非常多。", "-i input.mp4 frame_%04d.png", "-start_number", false, "图片", "");
        Add("图片 / 序列帧", "-start_number", "0 / 1 / 100", "图片序列起始编号。", "序列帧输入/输出。", "编号不匹配会找不到文件。", "-start_number 1 -i frame_%04d.png", "-f image2", false, "图片", "");
        Add("图片 / 序列帧", "-q:v", "1~31", "MJPEG/图片质量参数，数值越小质量越高。", "导出 jpg 图片。", "不同编码器含义不同。", "-q:v 2 cover.jpg", "-frames:v", false, "图片", "");
        Add("图片 / 序列帧", "-update", "1", "更新同一个图片文件。", "实时截图刷新。", "普通录屏不用。", "-update 1 frame.jpg", "-frames:v", false, "图片", "");

        // ===== 扩展补充：覆盖更多 FFmpeg 官方组件和进阶参数 =====
        Add("官方文档 / 查询", "ffmpeg -h full", "完整帮助", "输出当前 ffmpeg 可识别的大量选项。", "想在当前机器上确认真实可用参数。", "输出非常长，适合配合搜索。", "ffmpeg -h full", "-h, -version", true, "查询", "full 完整帮助");
        Add("官方文档 / 查询", "ffmpeg -h encoder=xxx", "encoder=libx264 / encoder=aac / encoder=libvpx", "查看某个编码器的专属私有参数。", "想知道 libx264、aac、libvpx、h264_nvenc 支持哪些选项。", "不同 ffmpeg 构建输出会不同。",
            "ffmpeg -h encoder=libx264", "-encoders", true, "查询", "编码器帮助 私有参数");
        Add("官方文档 / 查询", "ffmpeg -h decoder=xxx", "decoder=h264 / decoder=aac", "查看某个解码器的专属参数。", "输入解码异常时排查。", "录屏输出通常更关心 encoder。", "ffmpeg -h decoder=h264", "-decoders", false, "查询", "解码器帮助");
        Add("官方文档 / 查询", "ffmpeg -h muxer=xxx", "muxer=mp4 / muxer=webm / muxer=matroska", "查看某个封装器的参数。", "想查 MP4/WebM/MKV 容器专用选项。", "容器参数和编码器参数不是一回事。", "ffmpeg -h muxer=mp4", "-muxers", true, "查询",
            "muxer 容器帮助");
        Add("官方文档 / 查询", "ffmpeg -h demuxer=xxx", "demuxer=mov / demuxer=matroska / demuxer=image2", "查看某个解封装器的输入参数。", "输入文件读取异常或序列帧读取时。", "录屏采集设备还要看 devices。", "ffmpeg -h demuxer=image2",
            "-demuxers", false, "查询", "demuxer 输入格式帮助");
        Add("官方文档 / 查询", "ffmpeg -h filter=xxx", "filter=scale / filter=overlay / filter=loudnorm", "查看某个滤镜的具体参数。", "滤镜参数很多时最实用。", "滤镜名和参数要与当前 ffmpeg 版本匹配。", "ffmpeg -h filter=scale", "-filters",
            true, "查询", "滤镜帮助");
        Add("官方文档 / 查询", "ffmpeg -h bsf=xxx", "bsf=h264_mp4toannexb / bsf=aac_adtstoasc", "查看某个比特流过滤器参数。", "处理 H.264/H.265/AAC 封装兼容时。", "不是普通转码滤镜，它作用于编码后的码流。", "ffmpeg -h bsf=h264_mp4toannexb",
            "-bsfs", false, "查询", "bitstream filter bsf");

        Add("比特流过滤器", "-bsf:v", "h264_mp4toannexb / hevc_mp4toannexb / dump_extra", "给视频流应用 bitstream filter。", "MP4 H.264 转 TS、HLS、部分流媒体封装。", "它不解码画面，不能改变分辨率/画质。", "-bsf:v h264_mp4toannexb",
            "-c:v copy", false, "进阶", "bsf 视频");
        Add("比特流过滤器", "-bsf:a", "aac_adtstoasc / remove_extra", "给音频流应用 bitstream filter。", "AAC ADTS 与 MP4/M4A 封装转换。", "只处理编码后码流，不是音频滤镜。", "-bsf:a aac_adtstoasc", "-c:a copy", false, "进阶", "bsf 音频");
        Add("比特流过滤器", "h264_mp4toannexb", "H.264 MP4 -> Annex B", "把 MP4 里的 H.264 码流转成 Annex B 格式。", "输出 MPEG-TS、HLS、某些推流场景。", "通常配合 -c:v copy 使用。", "-c:v copy -bsf:v h264_mp4toannexb",
            "hevc_mp4toannexb", false, "常见", "h264 annexb ts hls");
        Add("比特流过滤器", "hevc_mp4toannexb", "HEVC MP4 -> Annex B", "把 MP4 里的 H.265/HEVC 码流转成 Annex B。", "HEVC 输出 TS/HLS。", "目标容器不需要时不要乱加。", "-c:v copy -bsf:v hevc_mp4toannexb", "h264_mp4toannexb",
            false, "常见", "hevc h265 annexb");
        Add("比特流过滤器", "aac_adtstoasc", "AAC ADTS -> MPEG-4 AudioSpecificConfig", "把 ADTS AAC 转成 MP4/M4A 需要的格式。", "把 ADTS AAC 封装进 MP4/M4A。", "ffmpeg 有时会自动加，手动加用于排查。", "-c:a copy -bsf:a aac_adtstoasc",
            "aac", false, "常见", "aac adts mp4");
        Add("比特流过滤器", "extract_extradata", "提取 extradata", "从码流中提取头部额外数据。", "修复或转换部分原始码流。", "普通录屏少用。", "-bsf:v extract_extradata", "dump_extra", false, "进阶", "extradata");
        Add("比特流过滤器", "dump_extra", "插入 extradata", "把 extradata 插入关键帧。", "MPEG-TS 等要求头信息的场景。", "不是所有输出都需要。", "-bsf:v dump_extra", "extract_extradata", false, "进阶", "extradata keyframe");
        Add("比特流过滤器", "filter_units", "remove_types=...", "按 NAL unit 类型过滤码流。", "移除 SEI 等码流单元。", "需要理解编码格式内部结构。", "-bsf:v filter_units=remove_types=6", "h264_metadata", false, "高级", "nal sei");
        Add("比特流过滤器", "h264_metadata", "sample_aspect_ratio / video_full_range_flag / colour_primaries", "修改 H.264 码流元数据。", "修正颜色范围、SAR、显示信息。", "不会重新编码画面，但设置错会导致显示异常。",
            "-bsf:v h264_metadata=video_full_range_flag=1", "hevc_metadata", false, "高级", "h264 metadata 色彩");
        Add("比特流过滤器", "hevc_metadata", "colour_primaries / transfer_characteristics / video_full_range_flag", "修改 HEVC 码流元数据。", "修正 HDR/颜色范围/显示信息。", "需要理解目标播放器如何解释元数据。",
            "-bsf:v hevc_metadata=video_full_range_flag=1", "h264_metadata", false, "高级", "hevc metadata hdr");

        Add("时间戳 / 同步", "-copytb", "0 / 1 / -1", "控制复制流时使用哪个 timebase。", "copy 码流后时间戳异常时。", "普通转码少用。", "-copytb 1 -i input.mp4 -c copy output.mp4", "-copyts", false, "高级", "timebase");
        Add("时间戳 / 同步", "-enc_time_base", "0 / demux / filter / 1:1000", "设置编码器时间基。", "复杂转码或同步问题。", "不懂 timebase 时不要乱设。", "-enc_time_base filter", "-copytb", false, "高级", "timebase encoder");
        Add("时间戳 / 同步", "-muxdelay", "秒数", "设置最大封装延迟。", "低延迟输出/流媒体。", "太低可能影响封装稳定。", "-muxdelay 0.1", "-muxpreload", false, "进阶", "mux delay");
        Add("时间戳 / 同步", "-muxpreload", "秒数", "设置初始封装预载延迟。", "低延迟封装。", "普通文件输出一般不需要。", "-muxpreload 0", "-muxdelay", false, "进阶", "mux preload");
        Add("时间戳 / 同步", "-fflags +genpts", "+genpts", "生成缺失 PTS 时间戳。", "输入流缺少 PTS 或时间轴异常。", "可能改变原时间戳。", "-fflags +genpts -i input.ts output.mp4", "-copyts", false, "排查", "pts 时间戳");
        Add("时间戳 / 同步", "-fflags +discardcorrupt", "+discardcorrupt", "丢弃损坏包。", "处理损坏输入文件。", "可能丢帧/丢音频。", "-fflags +discardcorrupt -i broken.mp4 output.mp4", "-err_detect", false, "排查",
            "损坏 corrupted");
        Add("时间戳 / 同步", "-fflags nobuffer", "nobuffer", "减少输入缓冲以降低延迟。", "低延迟拉流/预览。", "可能增加卡顿或丢包。", "-fflags nobuffer -i rtsp://...", "-flags low_delay", false, "流媒体", "低延迟 buffer");
        Add("时间戳 / 同步", "-flags low_delay", "low_delay", "请求低延迟处理。", "实时流处理。", "具体效果依赖编码器/解码器。", "-flags low_delay", "-fflags nobuffer", false, "流媒体", "低延迟");

        Add("高级视频编码", "-bf", "0 / 2 / 3", "B 帧数量。B 帧可提高压缩效率，但增加延迟。", "离线压缩或普通文件输出。", "低延迟/实时场景常设 0。", "-bf 2", "-g, -b_strategy", false, "进阶", "B帧");
        Add("高级视频编码", "-refs", "1 / 3 / 4 / 8", "参考帧数量。越多压缩潜力越好但更吃内存/解码。", "离线高质量编码。", "兼容性和性能会受影响。", "-refs 3", "-profile:v", false, "进阶", "参考帧");
        Add("高级视频编码", "-b_strategy", "0 / 1 / 2", "B 帧决策策略。", "x264 等编码器压缩优化。", "实时录屏一般不需要手动改。", "-b_strategy 1", "-bf", false, "高级", "B帧策略");
        Add("高级视频编码", "-qmin / -qmax", "整数", "限制量化参数范围。", "精细码率/质量控制。", "普通录屏不建议手调。", "-qmin 10 -qmax 40", "-qp", false, "高级", "量化 qp");
        Add("高级视频编码", "-qp", "0~51", "固定量化参数。越小质量越高。", "测试或某些硬件编码。", "固定 QP 不如 CRF/VBR 自适应。", "-qp 23", "-crf, -cq", false, "进阶", "qp");
        Add("高级视频编码", "-qscale:v / -q:v", "1~31", "某些编码器的质量参数。数值含义因编码器而异。", "MJPEG、MPEG-4 等老编码器。", "不要和 CRF 混淆。", "-q:v 2", "-crf", false, "进阶", "qscale");
        Add("高级视频编码", "-pass", "1 / 2", "两遍编码第几遍。", "目标文件大小/码率精确控制。", "实时录屏不适用。", "-pass 1 ... && -pass 2 ...", "-passlogfile", false, "离线", "二遍编码 two pass");
        Add("高级视频编码", "-passlogfile", "路径", "两遍编码日志文件路径。", "管理二遍编码统计文件。", "第二遍需要读取第一遍日志。", "-passlogfile stats.log", "-pass", false, "离线", "二遍编码 日志");
        Add("高级视频编码", "-x264-params", "key=value:key=value", "传递 libx264 私有参数。", "需要精细控制 x264。", "参数错会编码失败。", "-x264-params keyint=60:min-keyint=60:scenecut=0", "-preset, -crf", false, "高级",
            "x264 私有参数");
        Add("高级视频编码", "-x265-params", "key=value:key=value", "传递 libx265 私有参数。", "需要精细控制 x265。", "参数错会编码失败。", "-x265-params crf=26:preset=fast", "-crf", false, "高级", "x265 私有参数");

        Add("颜色 / HDR", "-colorspace", "bt709 / bt2020nc / smpte170m", "设置颜色矩阵。", "修正播放器颜色解释。", "设置错可能偏色。", "-colorspace bt709", "-color_primaries, -color_trc", false, "颜色", "colorspace");
        Add("颜色 / HDR", "-color_primaries", "bt709 / bt2020 / smpte170m", "设置色彩原色。", "HD/4K/HDR 元数据。", "普通 SDR 录屏通常 bt709。", "-color_primaries bt709", "-colorspace", false, "颜色", "primaries");
        Add("颜色 / HDR", "-color_trc", "bt709 / smpte2084 / arib-std-b67", "设置传输特性。PQ/HLG 属于 HDR。", "HDR 或修正色彩元数据。", "普通 SDR 不要乱设 HDR。", "-color_trc bt709", "-color_primaries", false, "颜色",
            "transfer hdr pq hlg");
        Add("颜色 / HDR", "-color_range", "tv / pc / mpeg / jpeg", "设置有限/全范围颜色。", "修正发灰或黑位不对的问题。", "播放器解释不一致时要测试。", "-color_range pc", "-pix_fmt", false, "颜色", "full limited range");
        Add("颜色 / HDR", "zscale", "matrix / transfer / primaries / range", "高质量颜色空间/范围转换滤镜。", "HDR/SDR 转换、颜色矩阵转换。", "需要 libzimg 支持，参数复杂。", "-vf zscale=matrix=bt709:range=tv", "tonemap", false, "高级",
            "zscale hdr");
        Add("颜色 / HDR", "tonemap", "hable / reinhard / mobius", "HDR 到 SDR 的色调映射滤镜。", "HDR 视频转 SDR。", "普通录屏不需要。", "-vf zscale=t=linear,tonemap=hable,zscale=t=bt709", "zscale", false, "高级",
            "hdr sdr tone map");

        Add("常用滤镜补充", "deinterlace / yadif", "yadif", "反交错滤镜。", "处理隔行扫描素材。", "屏幕录制通常是逐行，不需要。", "-vf yadif", "bwdif", false, "滤镜", "反交错");
        Add("常用滤镜补充", "bwdif", "bwdif", "更现代的反交错滤镜。", "隔行素材转逐行。", "录屏通常不用。", "-vf bwdif", "yadif", false, "滤镜", "反交错");
        Add("常用滤镜补充", "hqdn3d", "hqdn3d=luma:chroma", "视频降噪滤镜。", "脏素材降噪。", "会变慢且可能抹细节。", "-vf hqdn3d", "nlmeans", false, "滤镜", "降噪");
        Add("常用滤镜补充", "unsharp", "unsharp=...", "锐化滤镜。", "缩放后略微锐化。", "过度会产生边缘噪声。", "-vf unsharp=5:5:0.8:3:3:0.4", "scale", false, "滤镜", "锐化");
        Add("常用滤镜补充", "eq", "brightness / contrast / saturation", "调整亮度、对比度、饱和度。", "简单画面校正。", "不适合严肃调色。", "-vf eq=brightness=0.05:contrast=1.1", "curves", false, "滤镜", "亮度 对比度");
        Add("常用滤镜补充", "fade", "in / out", "视频淡入淡出。", "简单片头片尾。", "会重新编码。", "-vf fade=t=in:st=0:d=1", "afade", false, "滤镜", "淡入淡出");
        Add("常用滤镜补充", "afade", "in / out", "音频淡入淡出。", "片头片尾声音处理。", "会重新编码音频。", "-af afade=t=out:st=9:d=1", "fade", false, "滤镜", "音频淡入淡出");
        Add("常用滤镜补充", "silenceremove", "参数较多", "移除音频静音段。", "语音处理。", "可能误删轻声部分。", "-af silenceremove=start_periods=1:start_threshold=-50dB", "volume", false, "滤镜", "静音 移除");
        Add("常用滤镜补充", "highpass / lowpass", "frequency=...", "高通/低通音频滤镜。", "去低频噪声或高频噪声。", "设置不当会改变音色。", "-af highpass=f=80", "afftdn", false, "滤镜", "降噪 音频");
        Add("常用滤镜补充", "afftdn", "nr=...", "FFT 音频降噪滤镜。", "降低背景噪声。", "可能产生水声/失真。", "-af afftdn", "highpass", false, "滤镜", "音频降噪");

        Add("设备 / 平台", "-f avfoundation", "macOS 采集", "macOS 下采集屏幕、摄像头、麦克风的输入格式。", "macOS 录制设备。", "设备编号需要查询。", "-f avfoundation -list_devices true -i \"\"", "-list_devices", false, "设备",
            "macos avfoundation");
        Add("设备 / 平台", "-f kmsgrab", "Linux DRM/KMS 采集", "Linux 下从 KMS/DRM 抓屏。", "无 X11/特殊环境录屏。", "配置复杂，通常需要权限和硬件路径。", "-f kmsgrab -i -", "hwmap", false, "设备", "linux kmsgrab");
        Add("设备 / 平台", "-f fbdev", "/dev/fb0", "Linux framebuffer 采集。", "嵌入式或无桌面环境。", "现代桌面不常用。", "-f fbdev -i /dev/fb0", "x11grab", false, "设备", "framebuffer");
        Add("设备 / 平台", "-f alsa", "default / hw:0", "Linux ALSA 音频采集。", "直接从 ALSA 设备录音。", "系统混音/monitor 通常 PulseAudio/PipeWire 更方便。", "-f alsa -i default", "-f pulse", false, "设备", "alsa 音频");
        Add("设备 / 平台", "-f v4l2", "/dev/video0", "Linux 摄像头采集。", "USB 摄像头输入。", "格式、分辨率、帧率需要设备支持。", "-f v4l2 -framerate 30 -video_size 1280x720 -i /dev/video0", "-list_formats", false, "设备",
            "v4l2 camera");
        Add("设备 / 平台", "-list_formats", "all", "列出设备支持的格式。", "v4l2/dshow 等设备排查。", "不是所有输入设备支持。", "-f v4l2 -list_formats all -i /dev/video0", "-f v4l2", false, "设备", "格式列表");
        Add("设备 / 平台", "-f lavfi", "lavfi 输入", "使用滤镜图作为输入源。", "生成测试图、测试音。", "不是采集真实屏幕。", "-f lavfi -i testsrc=size=1280x720:rate=30", "testsrc, sine", false, "调试", "lavfi 测试源");
        Add("设备 / 平台", "testsrc / testsrc2", "视频测试图", "生成测试视频源。", "测试编码参数、UI 预览。", "不是真实录屏。", "-f lavfi -i testsrc2=size=1280x720:rate=30", "lavfi", false, "调试", "测试图");
        Add("设备 / 平台", "sine", "音频测试正弦波", "生成测试音频。", "测试音频编码/合并。", "声音刺耳，注意音量。", "-f lavfi -i sine=frequency=1000:sample_rate=48000", "lavfi", false, "调试", "测试音");

        Add("协议 / 网络补充", "file:", "本地文件协议", "读取本地文件。", "普通输入输出。", "路径转义要注意。", "file:input.mp4", "-i", false, "协议", "file protocol");
        Add("协议 / 网络补充", "pipe:", "pipe:0 / pipe:1", "通过标准输入/输出传输媒体数据。", "Unity 子进程管道、内存流。", "处理不当容易阻塞。", "-i pipe:0 -f mp4 pipe:1", "-progress pipe:1", false, "协议", "pipe 管道");
        Add("协议 / 网络补充", "tcp://", "tcp://host:port", "TCP 网络输入/输出。", "自定义网络流。", "要处理连接和防火墙。", "tcp://127.0.0.1:1234", "udp://", false, "协议", "tcp");
        Add("协议 / 网络补充", "udp://", "udp://host:port", "UDP 网络输入/输出。", "低延迟流。", "可能丢包。", "udp://127.0.0.1:1234", "tcp://", false, "协议", "udp");
        Add("协议 / 网络补充", "http:// / https://", "URL", "读取 HTTP/HTTPS 媒体资源。", "下载/转码网络文件。", "部分网站需要 headers/user_agent。", "-i https://example.com/video.mp4", "-headers", false, "协议", "http");
        Add("协议 / 网络补充", "rtmp://", "RTMP URL", "RTMP 推流/拉流。", "直播推流。", "现代平台也可能使用其他协议。", "-f flv rtmp://server/live/key", "-f flv", false, "流媒体", "rtmp");
        Add("协议 / 网络补充", "rtsp://", "RTSP URL", "RTSP 摄像头/流媒体输入。", "摄像头拉流。", "常需要 -rtsp_transport tcp。", "-rtsp_transport tcp -i rtsp://...", "-rtsp_transport", false, "流媒体", "rtsp");
        Add("协议 / 网络补充", "srt://", "SRT URL", "SRT 可靠低延迟传输协议。", "直播/远程传输。", "需要 ffmpeg 支持 libsrt。", "srt://host:port?mode=caller", "-protocols", false, "流媒体", "srt");

        Add("图像 / 像素补充", "-s", "1280x720", "设置输出大小的旧式简写。", "快速改分辨率。", "复杂缩放建议用 scale 滤镜。", "-s 1280x720", "-vf scale", false, "常用", "尺寸 分辨率");
        Add("图像 / 像素补充", "-aspect", "16:9 / 4:3", "设置显示宽高比。", "修正播放比例。", "不会真正缩放像素，和分辨率不同。", "-aspect 16:9", "setdar", false, "进阶", "宽高比");
        Add("图像 / 像素补充", "-sws_flags", "bilinear / bicubic / lanczos / neighbor", "设置缩放算法。", "控制 scale 滤镜缩放质量/速度。", "高质量算法更慢。", "-sws_flags lanczos -vf scale=1280:720", "scale", false, "进阶", "缩放算法");
        Add("图像 / 像素补充", "-vf scale_cuda", "CUDA 缩放", "GPU CUDA 缩放滤镜。", "NVIDIA GPU 加速缩放。", "需要 CUDA 支持和正确硬件链路。", "-vf scale_cuda=1280:720", "hwupload_cuda", false, "硬件", "cuda scale");
        Add("图像 / 像素补充", "hwupload / hwdownload / hwmap", "硬件帧上传/下载/映射", "在 CPU/GPU 滤镜之间转换帧。", "硬件加速滤镜链。", "配置复杂，错误容易失败。", "-vf hwupload,scale_vaapi=w=1280:h=720", "-hwaccel", false, "硬件",
            "hardware frames");
    }

    private void Add(
        string category,
        string parameter,
        string values,
        string meaning,
        string whenToUse,
        string warning,
        string example,
        string related,
        bool recommended,
        string level,
        string keywords)
    {
        _items.Add(new ParamInfo
        {
            category  = category,
            parameter = parameter,
            values    = values,
            meaning   = meaning,
            whenToUse = whenToUse,
            warning   = warning,
            example   = example,
            related   = related,
            recorderRelated = recommended || category.Contains("录屏") || category.Contains("视频编码") || category.Contains("音频") || category.Contains("WebM") || category.Contains("容器") ||
                              category.Contains("同步"),
            recommended = recommended,
            level       = level,
            keywords    = keywords
        });
    }

    private sealed class ParamInfo
    {
        public string category;
        public string parameter;
        public string values;
        public string meaning;
        public string whenToUse;
        public string warning;
        public string example;
        public string related;
        public string keywords;
        public bool recorderRelated;
        public bool recommended;
        public string level;
    }
}
#endif