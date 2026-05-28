//=====================================================
// 文件名称: VideoEncodingAllInOneGuideWindow
// 描    述:
//  Unity Editor 下的视频编码参数大词典 / 搜索查看器。
//  面向录屏、视频编码、音频编码、WebM/MP4、硬件编码、颜色格式、性能与兼容配置。
//=====================================================

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 视频编码参数大词典。
///
/// 菜单路径：
/// ByTools / 视频编码参数大词典
///
/// 说明：
/// 1. 这是面向录屏工具配置的视频编码参数查看器。
/// 2. 覆盖你脚本里的字段，也扩展到常见编码器、码率、CRF、预设、像素格式、颜色空间、WebM/MP4、硬件编码等配置。
/// 3. 不会修改任何录屏组件，只用于查看、搜索、复制配置片段。
/// </summary>
public sealed class VideoEncodingAllInOneGuideWindow : EditorWindow
{
    private const string WINDOW_TITLE = "视频编码参数大词典";
    private readonly VideoEncodingGuideContent _content = new();

    public static void ShowWindow()
    {
        var window = GetWindow<VideoEncodingAllInOneGuideWindow>(WINDOW_TITLE);
        window.minSize = new Vector2(1160, 720);
        window.Show();
    }

    private void OnEnable()
    {
        _content.Initialize();
    }

    private void OnGUI()
    {
        _content.Draw(position.width, position.height);
    }
}
public sealed class VideoEncodingGuideContent
{
    private const string WINDOW_TITLE = "视频编码参数大词典";

    private const string OFFICIAL_DOCUMENT_URL = "https://ffmpeg.org/ffmpeg-codecs.html";
    private const string OFFICIAL_DOCUMENT_BUTTON_TEXT = "打开编码官方文档";

    private const float MIN_LEFT_WIDTH = 240f;
    private const float MAX_LEFT_WIDTH = 340f;
    private const float CARD_MAX_TEXT_WIDTH_PADDING = 360f;
    private const float TOP_HEADER_HEIGHT = 74f;
    private const float QUICK_BAR_HEIGHT = 58f;
    private const float SEARCH_BAR_HEIGHT = 30f;


    private readonly List<EncodingItem> _items = new();
    private readonly List<string> _categories = new();
    private readonly List<EncodingItem> _cachedFilteredItems = new();
    private readonly Dictionary<string, List<EncodingItem>> _cachedFilteredByCategory = new();

    private Vector2 _leftScroll;
    private Vector2 _rightScroll;
    private string _searchText = string.Empty;
    private int _selectedCategoryIndex;
    private float _viewWidth = 1160f;
    private bool _filterCacheDirty = true;
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

    public void Initialize()
    {
        BuildData();
        RebuildCategories();
    }

    public void Draw(float viewWidth, float viewHeight)
    {
        _viewWidth = viewWidth;
        EnsureStyles();

        var fullRect = new Rect(8, 8, viewWidth - 16, viewHeight - 16);
        GUILayout.BeginArea(fullRect);
        DrawContent();
        GUILayout.EndArea();
    }

    public void DrawEmbedded(float viewWidth, float viewHeight)
    {
        _viewWidth = viewWidth;
        EnsureStyles();
        DrawContent();
    }

    private void DrawContent()
    {
        DrawHeader();
        DrawQuickButtons();
        DrawSearchToolbar();

        EditorGUILayout.Space(6);

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        DrawCategoryList();
        GUILayout.Space(8);
        DrawItemList();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical(_panelStyle, GUILayout.Height(TOP_HEADER_HEIGHT));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("视频编码参数大词典", _titleStyle);
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

    private void DrawQuickButtons()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("常用推荐配置", _fieldStyle);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("复制：MP4 通用", GUILayout.Height(26)))
        {
            CopyText(
                "outputAsWebM=false; videoCodec=\"libx264\"; videoPreset=\"veryfast\"; videoCrf=23; pixelFormat=\"yuv420p\"; audioCodec=\"aac\"; audioBitrate=\"192k\"; audioSampleRate=48000; audioChannels=2; captureFrameRate=30;");
        }

        if (GUILayout.Button("复制：MP4 高质量", GUILayout.Height(26)))
        {
            CopyText("outputAsWebM=false; videoCodec=\"libx264\"; videoPreset=\"fast\"; videoCrf=18; pixelFormat=\"yuv420p\"; audioCodec=\"aac\"; audioBitrate=\"256k\"; captureFrameRate=60;");
        }

        if (GUILayout.Button("复制：低配置机器", GUILayout.Height(26)))
        {
            CopyText("outputAsWebM=false; videoCodec=\"libx264\"; videoPreset=\"ultrafast\"; videoCrf=26; outputScale=0.5f; audioBitrate=\"128k\"; captureFrameRate=30;");
        }

        if (GUILayout.Button("复制：WebM Unity", GUILayout.Height(26)))
        {
            CopyText(
                "outputAsWebM=true; webmVideoCodec=\"libvpx\"; webmAudioCodec=\"libvorbis\"; webmVideoBitrate=\"3M\"; webmDeadline=\"realtime\"; webmCpuUsed=8; audioBitrate=\"192k\"; captureFrameRate=30;");
        }

        if (GUILayout.Button("复制：游戏录屏", GUILayout.Height(26)))
        {
            CopyText("captureFrameRate=60; outputScale=1f; videoCodec=\"libx264\"; videoPreset=\"veryfast\"; videoCrf=20; audioCodec=\"aac\"; audioBitrate=\"256k\";");
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
            _filterCacheDirty = true;
        }

        if (GUILayout.Button("清空", EditorStyles.toolbarButton, GUILayout.Width(52), GUILayout.Height(22)))
        {
            _searchText  = string.Empty;
            _rightScroll = Vector2.zero;
            _filterCacheDirty = true;
        }

        GUILayout.Space(8);
        bool nextOnlyRecommended = GUILayout.Toggle(_onlyRecommended, "只看推荐", EditorStyles.toolbarButton, GUILayout.Width(78), GUILayout.Height(22));
        if (nextOnlyRecommended != _onlyRecommended)
        {
            _onlyRecommended = nextOnlyRecommended;
            _filterCacheDirty = true;
        }

        _showExamples    = GUILayout.Toggle(_showExamples, "显示示例", EditorStyles.toolbarButton, GUILayout.Width(78), GUILayout.Height(22));
        bool nextSearchAllWords = GUILayout.Toggle(_searchAllWords, "多词全部匹配", EditorStyles.toolbarButton, GUILayout.Width(104), GUILayout.Height(22));
        if (nextSearchAllWords != _searchAllWords)
        {
            _searchAllWords = nextSearchAllWords;
            _filterCacheDirty = true;
        }

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
        float leftWidth = Mathf.Clamp(_viewWidth * 0.24f, MIN_LEFT_WIDTH, MAX_LEFT_WIDTH);

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

    private void DrawItemList()
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

        _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll, false, true);

        if (items.Count == 0)
        {
            EditorGUILayout.HelpBox("没有匹配结果。可以尝试搜索：mp4、webm、aac、crf、码率、帧率、像素格式、低配置、游戏、硬件编码、unity。", MessageType.Info);
        }
        else
        {
            for (int i = 0; i < items.Count; i++)
            {
                DrawItemCard(items[i]);
                EditorGUILayout.Space(6);
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawItemCard(EncodingItem item)
    {
        EditorGUILayout.BeginVertical(_cardStyle);

        EditorGUILayout.BeginHorizontal(_cardHeaderStyle, GUILayout.MinHeight(32));

        EditorGUILayout.LabelField(item.field, _cardTitleStyle);

        if (item.recommended)
        {
            GUILayout.Label("推荐", _activePillStyle, GUILayout.Width(48), GUILayout.Height(22));
        }

        if (!string.IsNullOrWhiteSpace(item.level))
        {
            GUILayout.Label(item.level, _pillStyle, GUILayout.Width(60), GUILayout.Height(22));
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("复制字段", GUILayout.Width(72), GUILayout.Height(22)))
        {
            CopyText(item.field);
        }

        if (!string.IsNullOrWhiteSpace(item.example) && GUILayout.Button("复制示例", GUILayout.Width(72), GUILayout.Height(22)))
        {
            CopyText(item.example);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        DrawTextBlock("分类", item.category);
        DrawTextBlock("常见取值", item.values);
        DrawTextBlock("含义", item.meaning);
        DrawTextBlock("适合", item.whenToUse);
        DrawTextBlock("注意", item.warning);
        DrawTextBlock("相关", item.related);

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

    private List<EncodingItem> GetFilteredItems(string category = null)
    {
        RebuildFilterCacheIfNeeded();
        if (string.IsNullOrWhiteSpace(category) || category == "全部") return _cachedFilteredItems;
        return _cachedFilteredByCategory.TryGetValue(category, out var items) ? items : emptyEncodingItemList;
    }

    private static readonly List<EncodingItem> emptyEncodingItemList = new();

    private void RebuildFilterCacheIfNeeded()
    {
        if (!_filterCacheDirty) return;
        _cachedFilteredItems.Clear();
        _cachedFilteredByCategory.Clear();

        IEnumerable<EncodingItem> query = _items;

        if (_onlyRecommended)
        {
            query = query.Where(item => item.recommended);
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

        _cachedFilteredItems.AddRange(query
            .OrderBy(item => item.category)
            .ThenByDescending(item => item.recommended)
            .ThenBy(item => item.field));

        foreach (var item in _cachedFilteredItems)
        {
            if (!_cachedFilteredByCategory.TryGetValue(item.category, out var list))
            {
                list = new List<EncodingItem>();
                _cachedFilteredByCategory[item.category] = list;
            }

            list.Add(item);
        }

        _filterCacheDirty = false;
    }

    private static bool MatchItem(EncodingItem item, string keyword)
    {
        return Contains(item.category, keyword) ||
               Contains(item.field, keyword) ||
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
        Debug.Log("[视频编码参数大词典] 打开官方编码文档：" + OFFICIAL_DOCUMENT_URL);
    }

    private static void CopyText(string text)
    {
        EditorGUIUtility.systemCopyBuffer = text ?? string.Empty;
        Debug.Log("[视频编码参数大词典] 已复制到剪贴板：" + EditorGUIUtility.systemCopyBuffer);
    }

    private static string BuildMarkdown(List<EncodingItem> items)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# 视频编码参数大词典搜索结果");
        builder.AppendLine();

        foreach (var group in items.GroupBy(item => item.category))
        {
            builder.AppendLine("## " + group.Key);
            builder.AppendLine();

            foreach (var item in group)
            {
                builder.AppendLine("### " + item.field + (item.recommended ? "（推荐）" : string.Empty));
                if (!string.IsNullOrWhiteSpace(item.values)) builder.AppendLine("- 常见取值：" + item.values);
                builder.AppendLine("- 含义：" + item.meaning);
                if (!string.IsNullOrWhiteSpace(item.whenToUse)) builder.AppendLine("- 适合：" + item.whenToUse);
                if (!string.IsNullOrWhiteSpace(item.warning)) builder.AppendLine("- 注意：" + item.warning);
                if (!string.IsNullOrWhiteSpace(item.related)) builder.AppendLine("- 相关：" + item.related);
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

        _sectionBoxStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(10, 10, 8, 10),
            margin  = new RectOffset(0, 0, 0, 0)
        };

        _cardStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(10, 10, 8, 10),
            margin  = new RectOffset(0, 0, 0, 2)
        };

        _cardHeaderStyle = new GUIStyle
        {
            padding = new RectOffset(10, 10, 5, 5),
            margin  = new RectOffset(0, 0, 0, 6)
        };

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

        _selectedCategoryAccentStyle = new GUIStyle(EditorStyles.label);

        _categoryButtonStyle = new GUIStyle(EditorStyles.miniButton)
        {
            alignment = TextAnchor.MiddleLeft,
            padding   = new RectOffset(10, 8, 3, 3),
            wordWrap  = false,
            fontSize  = 12,
            fontStyle = FontStyle.Normal
        };
        _categoryButtonStyle.normal.textColor  = textColor;
        _categoryButtonStyle.hover.textColor   = textColor;
        _categoryButtonStyle.active.textColor  = selectedTextColor;

        _selectedCategoryButtonStyle = new GUIStyle(_categoryButtonStyle)
        {
            fontStyle = FontStyle.Bold
        };
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
        _tagStyle.normal.textColor  = Color.white;

        _pillStyle = new GUIStyle(EditorStyles.miniButton)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize  = 10,
            padding   = new RectOffset(4, 4, 1, 1)
        };
        _pillStyle.normal.textColor  = pro ? new Color(0.88f, 0.88f, 0.88f) : new Color(0.12f, 0.12f, 0.12f);

        _activePillStyle                   = new GUIStyle(_pillStyle);
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

    private void BuildData()
    {
        _items.Clear();
        Add("快速配置方案", "MP4 通用录屏",
            "outputAsWebM=false, videoCodec=libx264, videoPreset=veryfast, videoCrf=23, pixelFormat=yuv420p, audioCodec=aac, audioBitrate=192k, audioSampleRate=48000, audioChannels=2, captureFrameRate=30",
            "最稳妥的 MP4 录屏配置。兼容性、文件体积、清晰度和性能比较平衡。", "教程录屏、UI 演示、普通应用录制、交付给别人播放。", "如果机器性能很差，可以把 videoPreset 改成 ultrafast，或者 outputScale 改成 0.5。",
            "outputAsWebM=false; videoCodec=\"libx264\"; videoPreset=\"veryfast\"; videoCrf=23; pixelFormat=\"yuv420p\";", true, "推荐", "mp4 通用 推荐 默认", "");
        Add("快速配置方案", "MP4 高质量录屏", "videoCrf=18, videoPreset=fast, captureFrameRate=60, audioBitrate=256k", "偏高质量配置，适合保留细节。", "代码录屏、UI 细节、小字、游戏高质量录制。", "文件更大，CPU 压力更高。低配置机器可能卡。",
            "videoCrf=18; videoPreset=\"fast\"; captureFrameRate=60; audioBitrate=\"256k\";", true, "推荐", "高质量 清晰", "");
        Add("快速配置方案", "MP4 低配置机器", "videoPreset=ultrafast, videoCrf=26, captureFrameRate=30, outputScale=0.5", "优先降低 CPU 和磁盘压力。", "低配置电脑、临时调试录制。", "画质和分辨率会下降，文字可能不够清晰。",
            "videoPreset=\"ultrafast\"; videoCrf=26; outputScale=0.5f;", true, "推荐", "低配置 性能", "");
        Add("快速配置方案", "WebM Unity 播放优先", "outputAsWebM=true, webmVideoCodec=libvpx, webmAudioCodec=libvorbis, webmVideoBitrate=3M, webmDeadline=realtime, webmCpuUsed=8", "偏向 Unity 内播放兼容和实时录制性能。",
            "需要把录制结果放回 Unity 里播放。", "WebM 在系统播放器、剪辑软件里的通用性通常不如 MP4。", "outputAsWebM=true; webmVideoCodec=\"libvpx\"; webmAudioCodec=\"libvorbis\"; webmVideoBitrate=\"3M\";", true, "推荐",
            "webm unity 国产机", "");
        Add("快速配置方案", "WebM 小体积", "webmVideoCodec=libvpx-vp9, webmVideoBitrate=2M, webmAudioCodec=libopus, audioBitrate=128k", "更注重文件体积。", "分享预览、网络传输、低码率场景。", "VP9 编码更吃 CPU，实时录屏要测试。",
            "webmVideoCodec=\"libvpx-vp9\"; webmVideoBitrate=\"2M\"; webmAudioCodec=\"libopus\";", false, "进阶", "小文件 vp9 opus", "");
        Add("快速配置方案", "静态 UI 录屏", "captureFrameRate=30, videoCrf=20~23, outputScale=1, audioBitrate=128k~192k", "适合画面变化不大、主要看文字和 UI 的录制。", "编辑器教程、软件操作、菜单说明。", "如果字体很小，CRF 降到 18~20。",
            "captureFrameRate=30; videoCrf=20;", true, "推荐", "UI 教程", "");
        Add("快速配置方案", "游戏录屏", "captureFrameRate=60, videoCrf=18~23 或 webmVideoBitrate=6M~12M, audioBitrate=192k~256k", "适合动态画面和音效较多的录制。", "游戏、动画、快速移动画面。", "60fps 压力较大，必要时降到 30fps。",
            "captureFrameRate=60; videoCrf=20; audioBitrate=\"256k\";", true, "推荐", "游戏 动态", "");
        Add("快速配置方案", "只录语音讲解", "audioCodec=aac, audioBitrate=96k~128k, audioSampleRate=48000, audioChannels=1 或 2", "声音主要是人声时可以降低码率。", "教学讲解、语音注释。", "单声道会失去左右声道，不适合游戏/音乐。",
            "audioBitrate=\"128k\"; audioChannels=1;", false, "常用", "语音 旁白", "");
        Add("快速配置方案", "后期剪辑友好", "outputAsWebM=false, videoCodec=libx264, videoCrf=18~20, pixelFormat=yuv420p, audioCodec=aac, audioBitrate=256k", "适合后面还要丢到剪辑软件里处理。", "剪辑、合成、二次压制。", "文件较大。",
            "videoCrf=18; audioBitrate=\"256k\";", true, "推荐", "剪辑 后期", "");
        Add("快速配置方案", "临时调试录制", "captureFrameRate=15~30, outputScale=0.5, videoCrf=28, audioBitrate=96k", "尽量快速生成小文件，只看大概效果。", "测试录制流程、Debug。", "不适合正式保存。",
            "captureFrameRate=15; outputScale=0.5f; videoCrf=28;", false, "调试", "debug 临时", "");
        Add("输出格式 / 容器", "outputAsWebM", "true / false", "决定最终输出是 WebM 还是 MP4。true 为 WebM，false 为 MP4。", "在 Unity 内播放可尝试 WebM；通用交付优先 MP4。",
            "容器变化会影响可用编码器组合。WebM 通常搭配 VP8/VP9 + Vorbis/Opus；MP4 通常搭配 H.264 + AAC。", "outputAsWebM = false;", true, "核心", "容器 输出格式", "");
        Add("输出格式 / 容器", "MP4", ".mp4", "最通用的视频容器。", "分享、上传、系统播放器、剪辑软件。", "MP4 里不建议塞 VP8/VP9/Vorbis。", "outputAsWebM=false", true, "核心", "mp4 通用", "");
        Add("输出格式 / 容器", "WebM", ".webm", "开源网页视频容器，常搭配 VP8/VP9/AV1 和 Vorbis/Opus。", "浏览器、Unity 内特定播放链路、开源格式。", "系统播放器和剪辑软件兼容性不一定如 MP4。", "outputAsWebM=true", true, "核心", "webm vp8 vp9", "");
        Add("输出格式 / 容器", "MKV", ".mkv", "Matroska 容器，兼容多编码、多音轨、字幕。", "中间文件、测试、多轨保留。", "交付和网页播放不如 MP4 普及。", "container=\"mkv\"", false, "进阶", "matroska", "");
        Add("输出格式 / 容器", "MOV", ".mov", "Apple 系常见容器。", "macOS、Final Cut、ProRes 工作流。", "Windows/网页通用性不如 MP4。", "container=\"mov\"", false, "进阶", "quicktime apple", "");
        Add("输出格式 / 容器", "AVI", ".avi", "较旧的视频容器。", "兼容老系统或老软件。", "不推荐现代录屏使用。", "container=\"avi\"", false, "旧格式", "avi", "");
        Add("输出格式 / 容器", "GIF", ".gif", "动图格式，不是真正的视频编码。", "短循环动图、无音频演示。", "颜色少、文件可能巨大、无音频。", "format=\"gif\"", false, "特殊", "动图", "");
        Add("视频编码器", "videoCodec = libx264", "H.264 软件编码", "兼容性最好的视频编码器之一。", "MP4 通用录屏首选。", "CPU 编码，分辨率高/帧率高时可能吃性能。", "videoCodec=\"libx264\"", true, "推荐", "h264 x264 mp4", "");
        Add("视频编码器", "videoCodec = libx265", "H.265 / HEVC 软件编码", "压缩效率高，同画质体积更小。", "离线压缩、高质量归档。", "编码慢，兼容性不如 H.264，部分播放器/浏览器受限。", "videoCodec=\"libx265\"", false, "进阶", "h265 hevc", "");
        Add("视频编码器", "videoCodec = h264_nvenc", "NVIDIA 硬件 H.264", "使用 NVIDIA 显卡硬件编码，速度快、CPU 占用低。", "有 NVIDIA 显卡的实时录屏。", "需要显卡、驱动、ffmpeg 编译支持。画质/码率表现和 libx264 不完全一样。", "videoCodec=\"h264_nvenc\"",
            false, "硬件", "nvidia nvenc", "");
        Add("视频编码器", "videoCodec = hevc_nvenc", "NVIDIA 硬件 H.265", "NVIDIA 显卡硬件 HEVC 编码。", "需要更小体积且目标支持 HEVC。", "兼容性不如 H.264。", "videoCodec=\"hevc_nvenc\"", false, "硬件", "nvidia hevc", "");
        Add("视频编码器", "videoCodec = h264_qsv", "Intel Quick Sync H.264", "Intel 核显硬件 H.264 编码。", "Intel 平台降低 CPU 压力。", "需要硬件、驱动和 ffmpeg 支持。", "videoCodec=\"h264_qsv\"", false, "硬件", "intel qsv", "");
        Add("视频编码器", "videoCodec = hevc_qsv", "Intel Quick Sync HEVC", "Intel 硬件 HEVC 编码。", "Intel 平台 HEVC 输出。", "兼容性和硬件支持要测试。", "videoCodec=\"hevc_qsv\"", false, "硬件", "intel hevc", "");
        Add("视频编码器", "videoCodec = h264_amf", "AMD AMF H.264", "AMD 显卡硬件 H.264 编码。", "AMD 显卡机器实时录屏。", "需要 AMD 驱动和 ffmpeg 支持。", "videoCodec=\"h264_amf\"", false, "硬件", "amd amf", "");
        Add("视频编码器", "videoCodec = hevc_amf", "AMD AMF HEVC", "AMD 显卡硬件 HEVC 编码。", "AMD 显卡 HEVC 输出。", "兼容性不如 H.264。", "videoCodec=\"hevc_amf\"", false, "硬件", "amd hevc", "");
        Add("视频编码器", "webmVideoCodec = libvpx", "VP8", "WebM 常用视频编码，实时编码相对容易。", "WebM 录屏，Unity 内播放测试。", "压缩效率不如 VP9。", "webmVideoCodec=\"libvpx\"", true, "推荐", "vp8 webm", "");
        Add("视频编码器", "webmVideoCodec = libvpx-vp9", "VP9", "压缩效率比 VP8 好，但编码更吃 CPU。", "WebM 小体积或质量优先。", "实时录屏可能卡，需要测试 cpu-used/deadline。", "webmVideoCodec=\"libvpx-vp9\"", false, "进阶", "vp9 webm", "");
        Add("视频编码器", "videoCodec = libaom-av1", "AV1 软件编码", "新一代高压缩效率编码。", "离线压缩、实验。", "非常慢，不适合普通实时录屏。", "videoCodec=\"libaom-av1\"", false, "实验", "av1", "");
        Add("视频编码器", "videoCodec = copy", "不重新编码", "直接复制视频流。", "换容器、合并音频、快速处理。", "不能改分辨率、画质、CRF、像素格式。", "videoCodec=\"copy\"", false, "常用", "copy 不转码", "");
        Add("音频编码器", "audioCodec = aac", "AAC", "MP4 最常见音频编码。兼容性非常好。", "MP4 录屏、通用播放器、移动端。", "WebM 通常不搭配 AAC。", "audioCodec=\"aac\"", true, "推荐", "aac mp4", "");
        Add("音频编码器", "webmAudioCodec = libopus", "Opus", "现代高效率音频编码，低码率语音也清楚。", "WebM、语音、网络传输。", "老旧播放器可能不支持。", "webmAudioCodec=\"libopus\"", true, "推荐", "opus webm", "");
        Add("音频编码器", "webmAudioCodec = libvorbis", "Vorbis", "WebM 常见音频编码。", "WebM 兼容方案。", "同码率下通常不如 Opus 高效。", "webmAudioCodec=\"libvorbis\"", true, "推荐", "vorbis webm", "");
        Add("音频编码器", "audioCodec = libmp3lame", "MP3", "MP3 编码器，兼容性强。", "需要 MP3 音轨或特殊兼容。", "MP4 视频音轨更推荐 AAC。", "audioCodec=\"libmp3lame\"", false, "常用", "mp3", "");
        Add("音频编码器", "audioCodec = pcm_s16le", "未压缩 PCM 16-bit", "无压缩音频，常用于 wav 临时文件。", "临时录音、调试、无损音频。", "最终文件非常大。", "audioCodec=\"pcm_s16le\"", false, "调试", "pcm wav", "");
        Add("音频编码器", "audioCodec = copy", "复制音频流", "不重新编码音频。", "源音频已经兼容目标容器。", "录屏临时 wav 合并进 MP4/WebM 通常不适合 copy。", "audioCodec=\"copy\"", false, "常用", "copy", "");
        Add("音频编码器", "audioCodec = ac3", "Dolby AC-3", "多声道音频常见格式。", "家庭影院、多声道输出。", "普通录屏不需要，兼容也要测试。", "audioCodec=\"ac3\"", false, "进阶", "ac3 5.1", "");
        Add("音频编码器", "audioCodec = flac", "无损音频 FLAC", "无损压缩音频。", "音频归档。", "视频容器兼容性不如 AAC。", "audioCodec=\"flac\"", false, "进阶", "无损", "");
        Add("帧率 / 时间", "captureFrameRate", "15", "低帧率，文件小，性能压力低。", "静态界面、低配置机器、临时测试。", "鼠标移动、动画、游戏会有明显卡顿感。", "captureFrameRate=15", false, "低性能", "fps 15", "");
        Add("帧率 / 时间", "captureFrameRate", "24", "电影常见帧率。", "画面变化不多的视频。", "屏幕操作录制通常 30 更自然。", "captureFrameRate=24", false, "常用", "fps 24", "");
        Add("帧率 / 时间", "captureFrameRate", "30", "通用推荐帧率。流畅度和性能平衡。", "教程、应用演示、普通录屏。", "高速游戏不如 60 顺滑。", "captureFrameRate=30", true, "推荐", "fps 30 推荐", "");
        Add("帧率 / 时间", "captureFrameRate", "45", "介于 30 和 60 之间。", "想比 30 顺但带不动 60。", "不是常见标准帧率，播放链路要测试。", "captureFrameRate=45", false, "进阶", "fps 45", "");
        Add("帧率 / 时间", "captureFrameRate", "60", "高流畅帧率。", "游戏、动画、高流畅演示。", "性能压力和文件体积明显增加。", "captureFrameRate=60", true, "推荐", "fps 60 游戏", "");
        Add("帧率 / 时间", "outputFrameRate", "与 captureFrameRate 相同", "输出帧率通常应和采集帧率一致。", "避免重复/丢帧。", "输入输出帧率不一致可能造成运动不自然。", "outputFrameRate=30", true, "推荐", "输出帧率", "");
        Add("帧率 / 时间", "variableFrameRate", "true / false", "可变帧率。画面静止时可以少写帧。", "节省体积或特殊录制。", "剪辑软件/播放器可能同步不稳定，录屏更推荐固定帧率。", "variableFrameRate=false", false, "进阶", "vfr cfr", "");
        Add("帧率 / 时间", "constantFrameRate", "true / false", "固定帧率。每秒固定帧数。", "录屏、剪辑、稳定播放。", "文件可能比可变帧率略大。", "constantFrameRate=true", true, "推荐", "cfr 固定帧率", "");
        Add("分辨率 / 缩放", "outputScale", "1", "原始分辨率输出。", "需要保留 UI 文字和细节。", "高分屏下文件大、编码压力高。", "outputScale=1f", true, "推荐", "分辨率 原始", "");
        Add("分辨率 / 缩放", "outputScale", "0.75", "输出宽高为原来的 75%。", "2K/4K 屏幕降一点压力。", "细小文字略有损失。", "outputScale=0.75f", false, "常用", "缩放", "");
        Add("分辨率 / 缩放", "outputScale", "0.5", "宽高各减半，总像素约四分之一。", "低配置机器、小文件、调试。", "文字和细节可能看不清。", "outputScale=0.5f", true, "推荐", "半分辨率", "");
        Add("分辨率 / 缩放", "outputScale", "0.25", "宽高各四分之一。", "流程调试。", "正式录制不推荐。", "outputScale=0.25f", false, "调试", "低清", "");
        Add("分辨率 / 缩放", "outputWidth/outputHeight", "1280x720", "720p 分辨率。", "小文件、网页预览、低配置机器。", "UI 字体小的话可能不清晰。", "outputWidth=1280; outputHeight=720;", true, "常用", "720p", "");
        Add("分辨率 / 缩放", "outputWidth/outputHeight", "1920x1080", "1080p 分辨率。", "通用高清录屏。", "比 720p 文件更大。", "outputWidth=1920; outputHeight=1080;", true, "推荐", "1080p", "");
        Add("分辨率 / 缩放", "outputWidth/outputHeight", "2560x1440", "2K 分辨率。", "高分屏细节保留。", "编码压力明显增加。", "outputWidth=2560; outputHeight=1440;", false, "进阶", "2k 1440p", "");
        Add("分辨率 / 缩放", "outputWidth/outputHeight", "3840x2160", "4K 分辨率。", "超高清录制。", "文件巨大，实时编码压力很高。", "outputWidth=3840; outputHeight=2160;", false, "高压", "4k", "");
        Add("分辨率 / 缩放", "evenSize", "宽高为偶数", "很多编码器要求宽高是偶数，特别是 yuv420p。", "缩放/裁剪后修正尺寸。", "奇数宽高可能导致编码失败。", "scale=-2:720", true, "重要", "偶数 宽高", "");
        Add("质量 / 码率", "videoCrf", "16", "非常高质量，文件很大。", "需要保留极多细节。", "普通录屏一般没必要。", "videoCrf=16", false, "高质量", "crf", "");
        Add("质量 / 码率", "videoCrf", "18", "高质量推荐值。", "代码录屏、UI 小字、高质量录制。", "文件较大。", "videoCrf=18", true, "推荐", "crf 高质量", "");
        Add("质量 / 码率", "videoCrf", "20", "较高质量。", "比 18 省体积，仍比较清晰。", "复杂画面可能略损失。", "videoCrf=20", true, "推荐", "crf", "");
        Add("质量 / 码率", "videoCrf", "23", "x264 常用默认平衡值。", "普通录屏推荐。", "小字不清时降到 20 或 18。", "videoCrf=23", true, "推荐", "crf 默认", "");
        Add("质量 / 码率", "videoCrf", "26", "偏小文件。", "不重要的录屏、分享预览。", "文字和细节可能变糊。", "videoCrf=26", false, "常用", "crf 小文件", "");
        Add("质量 / 码率", "videoCrf", "28", "明显压缩。", "临时调试、小文件。", "画质下降明显。", "videoCrf=28", false, "调试", "crf 低质量", "");
        Add("质量 / 码率", "videoCrf", "30~35", "低质量小体积。", "只看流程的调试视频。", "不适合正式输出。", "videoCrf=32", false, "调试", "crf", "");
        Add("质量 / 码率", "videoBitrate", "1M", "低视频码率。", "720p 或低动态画面。", "1080p 可能很糊。", "videoBitrate=\"1M\"", false, "低码率", "码率", "");
        Add("质量 / 码率", "videoBitrate", "2M", "偏小文件码率。", "720p、半分辨率。", "1080p 动态画面不够。", "videoBitrate=\"2M\"", true, "常用", "码率", "");
        Add("质量 / 码率", "videoBitrate", "3M", "WebM 通用推荐码率。", "普通 1080p WebM 录屏。", "复杂动态画面可能需要更高。", "webmVideoBitrate=\"3M\"", true, "推荐", "webm 码率", "");
        Add("质量 / 码率", "videoBitrate", "4M~6M", "较清晰码率。", "1080p 动态画面。", "文件更大。", "videoBitrate=\"5M\"", true, "推荐", "码率", "");
        Add("质量 / 码率", "videoBitrate", "8M~12M", "高码率。", "游戏、2K/4K、高动态画面。", "文件大，编码压力高。", "videoBitrate=\"8M\"", false, "高质量", "码率", "");
        Add("质量 / 码率", "audioBitrate", "64k", "低音频码率。", "纯语音且文件极小。", "音乐/游戏音效明显损失。", "audioBitrate=\"64k\"", false, "低码率", "音频码率", "");
        Add("质量 / 码率", "audioBitrate", "96k", "偏低音频码率。", "语音讲解。", "音乐和复杂音效一般。", "audioBitrate=\"96k\"", false, "常用", "音频码率", "");
        Add("质量 / 码率", "audioBitrate", "128k", "标准低体积音频码率。", "普通语音和系统声音。", "音乐质量一般。", "audioBitrate=\"128k\"", true, "推荐", "音频码率", "");
        Add("质量 / 码率", "audioBitrate", "192k", "通用推荐音频码率。", "系统声音、游戏音效、普通录屏。", "比 128k 略大。", "audioBitrate=\"192k\"", true, "推荐", "音频码率", "");
        Add("质量 / 码率", "audioBitrate", "256k", "高质量音频码率。", "音乐、游戏、后期剪辑。", "文件更大。", "audioBitrate=\"256k\"", true, "推荐", "音频码率", "");
        Add("质量 / 码率", "audioBitrate", "320k", "很高音频码率。", "极重视声音质量。", "多数录屏没必要。", "audioBitrate=\"320k\"", false, "高质量", "音频码率", "");
        Add("预设 / Profile / GOP", "videoPreset", "ultrafast", "最快 x264 预设，CPU 压力最低，文件较大。", "实时录屏、低配置机器。", "同质量体积偏大。", "videoPreset=\"ultrafast\"", true, "推荐", "preset", "");
        Add("预设 / Profile / GOP", "videoPreset", "superfast", "非常快，比 ultrafast 稍好压缩。", "低配置实时录屏。", "体积仍偏大。", "videoPreset=\"superfast\"", false, "常用", "preset", "");
        Add("预设 / Profile / GOP", "videoPreset", "veryfast", "常用实时录制折中。", "MP4 通用推荐。", "比 fast/medium 文件略大。", "videoPreset=\"veryfast\"", true, "推荐", "preset", "");
        Add("预设 / Profile / GOP", "videoPreset", "faster", "比 veryfast 慢一点，压缩更好。", "机器性能较好。", "实时录制要测试。", "videoPreset=\"faster\"", false, "常用", "preset", "");
        Add("预设 / Profile / GOP", "videoPreset", "fast", "质量/体积更好但更吃 CPU。", "高质量录屏。", "低配机器可能卡。", "videoPreset=\"fast\"", true, "推荐", "preset", "");
        Add("预设 / Profile / GOP", "videoPreset", "medium", "x264 默认平衡预设。", "离线转码或性能足够。", "实时录屏可能压力偏高。", "videoPreset=\"medium\"", false, "进阶", "preset", "");
        Add("预设 / Profile / GOP", "videoPreset", "slow / slower / veryslow", "慢速高压缩效率。", "离线压制。", "不推荐实时录屏。", "videoPreset=\"slow\"", false, "离线", "preset", "");
        Add("预设 / Profile / GOP", "videoTune", "zerolatency", "低延迟调优。", "直播/实时预览。", "可能牺牲压缩效率。", "videoTune=\"zerolatency\"", false, "进阶", "低延迟", "");
        Add("预设 / Profile / GOP", "videoTune", "animation", "动画内容调优。", "动画、卡通画面。", "普通录屏不一定有效。", "videoTune=\"animation\"", false, "进阶", "tune", "");
        Add("预设 / Profile / GOP", "videoTune", "film", "电影内容调优。", "自然视频。", "屏幕录制一般不需要。", "videoTune=\"film\"", false, "进阶", "tune", "");
        Add("预设 / Profile / GOP", "videoProfile", "baseline", "H.264 基础 Profile，兼容老设备。", "老设备兼容。", "压缩能力弱。", "profile=\"baseline\"", false, "兼容", "profile", "");
        Add("预设 / Profile / GOP", "videoProfile", "main", "H.264 Main Profile。", "中等兼容和压缩。", "通常不如 high 常用。", "profile=\"main\"", false, "常用", "profile", "");
        Add("预设 / Profile / GOP", "videoProfile", "high", "H.264 High Profile，常见高质量方案。", "现代播放器/设备。", "极老设备可能不支持。", "profile=\"high\"", true, "推荐", "profile", "");
        Add("预设 / Profile / GOP", "videoLevel", "3.1 / 4.0 / 4.1 / 5.1", "H.264 Level，约束分辨率、帧率、码率。", "针对特定硬件播放器。", "不懂时不要强行设置。", "level=\"4.1\"", false, "进阶", "level", "");
        Add("预设 / Profile / GOP", "gopSize", "30 / 60 / 120", "关键帧间隔。60fps 下 gop=60 表示约 1 秒一个关键帧。", "需要拖动进度条更顺或低延迟。", "关键帧太多文件变大，太少拖动不方便。", "gopSize=60", false, "进阶", "gop keyframe", "");
        Add("预设 / Profile / GOP", "keyframeInterval", "1~5 秒", "关键帧时间间隔。", "平衡拖动和体积。", "录屏一般 1~2 秒即可。", "keyframeInterval=2", false, "进阶", "关键帧", "");
        Add("像素格式 / 颜色", "pixelFormat", "yuv420p", "最通用像素格式。", "MP4/WebM 正式输出。", "色度采样少于 4:4:4，但录屏足够。", "pixelFormat=\"yuv420p\"", true, "推荐", "像素格式 兼容", "");
        Add("像素格式 / 颜色", "pixelFormat", "yuv422p", "比 420 保留更多色彩信息。", "专业视频处理。", "兼容性不如 yuv420p。", "pixelFormat=\"yuv422p\"", false, "进阶", "像素格式", "");
        Add("像素格式 / 颜色", "pixelFormat", "yuv444p", "完整色度采样。", "图像处理、后期。", "体积大，兼容差。", "pixelFormat=\"yuv444p\"", false, "进阶", "像素格式", "");
        Add("像素格式 / 颜色", "pixelFormat", "nv12", "硬件编码常见格式。", "配合硬件编码器。", "普通软件编码不必手动设。", "pixelFormat=\"nv12\"", false, "硬件", "nv12", "");
        Add("像素格式 / 颜色", "pixelFormat", "bgra", "带 Alpha 的原始色彩格式。", "输入/调试。", "不适合最终输出。", "pixelFormat=\"bgra\"", false, "调试", "bgra alpha", "");
        Add("像素格式 / 颜色", "colorRange", "tv / limited", "有限范围颜色，视频标准常见。", "普通视频输出。", "和播放器解释有关。", "colorRange=\"tv\"", true, "常用", "颜色范围", "");
        Add("像素格式 / 颜色", "colorRange", "pc / full", "全范围颜色。", "屏幕录制或特殊需求。", "播放器不正确识别时可能发灰或过曝。", "colorRange=\"pc\"", false, "进阶", "颜色范围 full", "");
        Add("像素格式 / 颜色", "colorPrimaries", "bt709", "HD 视频常用色彩原色。", "720p/1080p 录屏。", "通常无需手动设置。", "colorPrimaries=\"bt709\"", true, "常用", "色彩", "");
        Add("像素格式 / 颜色", "colorPrimaries", "bt2020", "HDR/4K 常见广色域。", "HDR 视频。", "普通 SDR 录屏不要乱用。", "colorPrimaries=\"bt2020\"", false, "HDR", "hdr", "");
        Add("像素格式 / 颜色", "colorMatrix", "bt709", "HD 视频常用颜色矩阵。", "1080p 视频。", "矩阵错误可能颜色不对。", "colorMatrix=\"bt709\"", true, "常用", "颜色矩阵", "");
        Add("像素格式 / 颜色", "colorTransfer", "bt709", "SDR 常用传输特性。", "普通 SDR 视频。", "HDR 不适合。", "colorTransfer=\"bt709\"", true, "常用", "颜色传输", "");
        Add("像素格式 / 颜色", "colorTransfer", "smpte2084", "HDR PQ 传输特性。", "HDR10。", "普通录屏不要用。", "colorTransfer=\"smpte2084\"", false, "HDR", "pq hdr", "");
        Add("音频参数", "audioSampleRate", "44100", "44.1kHz，音乐 CD 常见。", "音频源本身是 44100。", "视频/游戏录屏更常用 48000。", "audioSampleRate=44100", false, "常用", "采样率", "");
        Add("音频参数", "audioSampleRate", "48000", "48kHz，视频/游戏/系统声音常见。", "录屏推荐。", "通常保持这个值。", "audioSampleRate=48000", true, "推荐", "采样率", "");
        Add("音频参数", "audioSampleRate", "96000", "高采样率。", "专业音频测试。", "普通录屏没必要，文件更大。", "audioSampleRate=96000", false, "进阶", "采样率", "");
        Add("音频参数", "audioChannels", "1", "单声道。", "只录语音，文件小。", "会失去左右方向感。", "audioChannels=1", false, "常用", "声道 单声道", "");
        Add("音频参数", "audioChannels", "2", "双声道，最常用。", "普通录屏、游戏、系统声音。", "无明显缺点。", "audioChannels=2", true, "推荐", "声道 stereo", "");
        Add("音频参数", "audioChannels", "6", "5.1 多声道。", "源音频本来就是 5.1。", "录屏通常没必要，兼容和体积更复杂。", "audioChannels=6", false, "进阶", "5.1", "");
        Add("音频参数", "audioLayout", "mono", "单声道布局。", "语音录制。", "丢失左右声道。", "audioLayout=\"mono\"", false, "常用", "mono", "");
        Add("音频参数", "audioLayout", "stereo", "左右双声道布局。", "普通录屏推荐。", "无明显缺点。", "audioLayout=\"stereo\"", true, "推荐", "stereo", "");
        Add("音频参数", "audioNormalize", "true / false", "是否做响度标准化。", "不同片段音量差异很大时。", "可能改变原始声音动态，需要额外处理时间。", "audioNormalize=false", false, "进阶", "响度", "");
        Add("音频参数", "audioVolume", "0.5 / 1 / 1.5 / 2", "音量倍率。1 为原音量。", "声音太小/太大时。", "过大可能爆音。", "audioVolume=1.5f", false, "常用", "音量", "");
        Add("音频参数", "muteAudio", "true / false", "是否不输出音频。", "只需要画面。", "开启后最终视频无声音。", "muteAudio=false", false, "常用", "静音", "");
        Add("音频参数", "recordMicrophone", "true / false", "是否录麦克风。", "旁白讲解。", "麦克风权限和设备选择要处理。", "recordMicrophone=true", false, "录音", "麦克风", "");
        Add("音频参数", "recordSystemAudio", "true / false", "是否录系统声音。", "游戏/软件声音。", "不同平台实现差异大。", "recordSystemAudio=true", true, "推荐", "系统声音", "");
        Add("WebM 参数", "webmVideoBitrate", "1M", "低 WebM 视频码率。", "低分辨率或预览。", "1080p 会糊。", "webmVideoBitrate=\"1M\"", false, "低码率", "webm", "");
        Add("WebM 参数", "webmVideoBitrate", "2M", "偏小文件码率。", "720p 或半分辨率。", "1080p 动态画面可能不够。", "webmVideoBitrate=\"2M\"", true, "常用", "webm", "");
        Add("WebM 参数", "webmVideoBitrate", "3M", "WebM 通用推荐值。", "普通 1080p 录屏。", "复杂游戏画面可能需要 4M+。", "webmVideoBitrate=\"3M\"", true, "推荐", "webm", "");
        Add("WebM 参数", "webmVideoBitrate", "4M~6M", "较清晰 WebM 码率。", "1080p 动态画面。", "文件更大。", "webmVideoBitrate=\"4M\"", true, "推荐", "webm", "");
        Add("WebM 参数", "webmVideoBitrate", "8M~12M", "高 WebM 码率。", "2K/4K 或高动态。", "文件和编码压力都大。", "webmVideoBitrate=\"8M\"", false, "高质量", "webm", "");
        Add("WebM 参数", "webmDeadline", "realtime", "实时编码模式。", "实时录屏推荐。", "同码率质量不如 good/best。", "webmDeadline=\"realtime\"", true, "推荐", "deadline", "");
        Add("WebM 参数", "webmDeadline", "good", "平衡质量和速度。", "机器较强时。", "实时录制要测试。", "webmDeadline=\"good\"", false, "进阶", "deadline", "");
        Add("WebM 参数", "webmDeadline", "best", "最佳质量但很慢。", "离线编码。", "不推荐实时录屏。", "webmDeadline=\"best\"", false, "离线", "deadline", "");
        Add("WebM 参数", "webmCpuUsed", "0", "最慢，质量最好。", "离线编码。", "不适合实时录屏。", "webmCpuUsed=0", false, "离线", "cpu-used", "");
        Add("WebM 参数", "webmCpuUsed", "4", "中间速度/质量。", "性能较好时测试。", "实时不一定稳。", "webmCpuUsed=4", false, "进阶", "cpu-used", "");
        Add("WebM 参数", "webmCpuUsed", "6", "偏速度。", "普通实时录屏。", "画质略降。", "webmCpuUsed=6", true, "推荐", "cpu-used", "");
        Add("WebM 参数", "webmCpuUsed", "8", "最快。", "实时录屏减少卡顿。", "同码率画质最弱，可提高 bitrate 弥补。", "webmCpuUsed=8", true, "推荐", "cpu-used", "");
        Add("WebM 参数", "webmRowMt", "true / false", "VP9 行多线程。", "加速 libvpx-vp9。", "ffmpeg/libvpx 版本要支持。", "webmRowMt=true", false, "进阶", "row-mt vp9", "");
        Add("WebM 参数", "webmLagInFrames", "0 / 16 / 25", "VPx 前瞻帧数。", "实时录屏用 0，离线压缩可提高。", "高前瞻会增加延迟。", "webmLagInFrames=0", false, "进阶", "lag", "");
        Add("硬件编码参数", "hardwareEncoder", "auto", "自动选择硬件编码。", "希望根据机器能力自动切换。", "自动探测容易误判，失败要回退软件编码。", "hardwareEncoder=\"auto\"", false, "进阶", "硬件", "");
        Add("硬件编码参数", "hardwareEncoder", "none", "禁用硬件编码。", "追求稳定和一致质量。", "CPU 压力更高。", "hardwareEncoder=\"none\"", true, "推荐", "软件编码", "");
        Add("硬件编码参数", "nvencPreset", "p1~p7 / fast / medium / slow", "NVIDIA 编码预设。不同版本 FFmpeg 命名不同。", "NVENC 速度/质量控制。", "和 x264 preset 不是一套含义。", "nvencPreset=\"p4\"", false, "硬件", "nvenc preset", "");
        Add("硬件编码参数", "nvencRc", "cbr / vbr / constqp", "NVENC 码率控制模式。", "硬件编码码率策略。", "不同显卡和 ffmpeg 支持不同。", "nvencRc=\"vbr\"", false, "硬件", "nvenc rc", "");
        Add("硬件编码参数", "nvencCq", "18 / 23 / 28", "NVENC 质量参数。", "硬件编码质量控制。", "不要直接等同 x264 CRF。", "nvencCq=23", false, "硬件", "nvenc cq", "");
        Add("硬件编码参数", "qsvPreset", "veryfast / faster / fast / medium / slow", "Intel QSV 预设。", "Intel 硬件编码。", "具体支持看 ffmpeg。", "qsvPreset=\"veryfast\"", false, "硬件", "qsv", "");
        Add("硬件编码参数", "amfUsage", "transcoding / ultralowlatency / lowlatency / webcam", "AMD AMF 用途模式。", "AMD 硬件编码调优。", "ffmpeg 版本支持要测试。", "amfUsage=\"transcoding\"", false, "硬件", "amf", "");
        Add("硬件编码参数", "gpuIndex", "0 / 1 / any", "指定使用哪块 GPU。", "多显卡机器。", "不是所有编码器支持。", "gpuIndex=0", false, "硬件", "gpu", "");
        Add("硬件编码参数", "fallbackToSoftware", "true / false", "硬件编码失败后是否回退软件编码。", "提高兼容性。", "回退后性能和输出参数可能变化。", "fallbackToSoftware=true", true, "推荐", "fallback", "");
        Add("录屏 / 平台参数", "screenSource", "desktop / window / monitor", "录制源类型。", "全屏、窗口、指定显示器录制。", "窗口录制跨平台实现差异大。", "screenSource=\"desktop\"", true, "录屏", "屏幕源", "");
        Add("录屏 / 平台参数", "captureRegion", "x,y,width,height", "录制区域。", "只录屏幕一部分。", "区域坐标要考虑系统缩放和多显示器。", "captureRegion=(0,0,1920,1080)", false, "录屏", "区域", "");
        Add("录屏 / 平台参数", "drawMouse", "true / false", "是否录制鼠标指针。", "教程录屏推荐显示。", "部分平台鼠标捕获方式不同。", "drawMouse=true", true, "推荐", "鼠标", "");
        Add("录屏 / 平台参数", "showRegion", "true / false", "显示录制区域边框。", "调试区域。", "正式录制不要开。", "showRegion=false", false, "调试", "区域边框", "");
        Add("录屏 / 平台参数", "windowsCaptureBackend", "gdigrab / dshow / desktopduplication", "Windows 采集后端。", "根据实现选择。", "不同后端性能和兼容性差异大。", "windowsCaptureBackend=\"gdigrab\"", true, "录屏", "windows", "");
        Add("录屏 / 平台参数", "linuxCaptureBackend", "x11grab / pipewire", "Linux 采集后端。", "X11 用 x11grab，Wayland 可能需要 PipeWire。", "Wayland 下传统 x11grab 可能不可用。", "linuxCaptureBackend=\"x11grab\"", true,
            "录屏", "linux", "");
        Add("录屏 / 平台参数", "linuxSystemAudioSourceName", "留空 / alsa_output.xxx.monitor", "Linux 系统声音源。", "PulseAudio 系统声音录制。", "自动探测失败时要手动填写。", "linuxSystemAudioSourceName=\"\"", true, "录音",
            "pulse monitor", "");
        Add("录屏 / 平台参数", "microphoneDeviceName", "设备名", "麦克风设备。", "录旁白。", "设备名跨平台不同。", "microphoneDeviceName=\"Microphone\"", false, "录音", "麦克风", "");
        Add("录屏 / 平台参数", "tempVideoPath", "临时视频路径", "录制过程中生成的临时视频文件。", "分离录音/录像再合并。", "路径要可写，录制结束后清理。", "tempVideoPath=\"...\"", false, "实现", "临时文件", "");
        Add("录屏 / 平台参数", "tempAudioPath", "临时音频路径", "录制过程中生成的临时音频文件。", "分离录音/录像再合并。", "路径要可写。", "tempAudioPath=\"...\"", false, "实现", "临时文件", "");
        Add("录屏 / 平台参数", "mergeAfterRecord", "true / false", "录完后是否合并音视频。", "分离录制时需要。", "合并耗时和失败处理要做好。", "mergeAfterRecord=true", true, "推荐", "合并", "");
        Add("录屏 / 平台参数", "deleteTempFiles", "true / false", "合并后是否删除临时文件。", "节省空间。", "调试失败时建议先保留。", "deleteTempFiles=true", true, "推荐", "清理", "");
        Add("性能 / 超时 / 日志", "stopVideoTimeoutMs", "15000", "等待视频 ffmpeg 进程停止的时间。", "停止录制时给 ffmpeg 写尾部数据。", "太短可能文件损坏。", "stopVideoTimeoutMs=15000", true, "推荐", "超时", "");
        Add("性能 / 超时 / 日志", "stopAudioTimeoutMs", "15000", "等待音频录制进程停止的时间。", "停止录音。", "太短可能音频文件没写完。", "stopAudioTimeoutMs=15000", true, "推荐", "超时", "");
        Add("性能 / 超时 / 日志", "waitTempFileReadyTimeoutMs", "8000", "等待临时文件可读/释放。", "Windows 文件句柄释放可能延迟。", "太短可能合并失败。", "waitTempFileReadyTimeoutMs=8000", true, "推荐", "临时文件", "");
        Add("性能 / 超时 / 日志", "mergeTimeoutMs", "0", "合并不限时。", "长视频、慢机器。", "ffmpeg 卡死时任务可能一直挂。", "mergeTimeoutMs=0", true, "推荐", "合并超时", "");
        Add("性能 / 超时 / 日志", "mergeTimeoutMs", "60000", "合并最多 60 秒。", "短视频防止卡死。", "长视频可能合并失败。", "mergeTimeoutMs=60000", false, "常用", "合并超时", "");
        Add("性能 / 超时 / 日志", "processPriority", "Normal / BelowNormal / High", "ffmpeg 进程优先级。", "控制录制对系统影响。", "High 可能影响用户操作体验。", "processPriority=BelowNormal", false, "进阶", "进程优先级", "");
        Add("性能 / 超时 / 日志", "threadCount", "0 / 2 / 4 / 8", "编码线程数。0 通常自动。", "限制 CPU 或提高速度。", "不是所有编码器都遵守。", "threadCount=0", false, "进阶", "threads", "");
        Add("性能 / 超时 / 日志", "bufferSize", "128M / 256M / 512M", "实时输入缓冲。", "多输入/设备采集。", "过大占内存。", "bufferSize=\"256M\"", false, "进阶", "缓冲", "");
        Add("性能 / 超时 / 日志", "threadQueueSize", "128 / 512 / 1024", "输入队列大小。", "录屏+录音防止队列阻塞。", "过大占内存，过小可能丢包。", "threadQueueSize=512", true, "推荐", "队列", "");
        Add("性能 / 超时 / 日志", "asyncMerge", "true / false", "是否后台合并。", "避免主线程卡住。", "要处理完成回调和失败状态。", "asyncMerge=true", true, "推荐", "后台", "");
        Add("性能 / 超时 / 日志", "logLevel", "error / warning / info / debug", "ffmpeg 日志级别。", "调试或减少日志。", "debug 日志很多。", "logLevel=\"warning\"", true, "推荐", "日志", "");
        Add("兼容性 / 播放建议", "fastStart", "true / false", "MP4 moov 元数据前置，便于边下载边播放。", "上传网页或网络播放。", "只对 MP4/MOV 有意义。", "fastStart=true", true, "推荐", "faststart", "");
        Add("兼容性 / 播放建议", "playerCompatibility", "high / medium / low", "兼容性策略。高兼容通常意味着 H.264 + AAC + yuv420p。", "给别人播放或跨平台。", "高兼容可能牺牲部分压缩效率。", "playerCompatibility=\"high\"", true, "推荐", "兼容", "");
        Add("兼容性 / 播放建议", "unityVideoPlayer", "WebM / MP4 视平台测试", "Unity VideoPlayer 对格式的支持和平台有关。", "需要在目标平台真实测试。", "编辑器能播不代表目标设备能播。", "outputAsWebM=true", true, "重要", "unity videoplayer", "");
        Add("兼容性 / 播放建议", "mobileCompatibility", "H.264 + AAC + yuv420p", "移动端通用组合。", "Android/iOS 播放。", "过高 profile/level 可能老设备不支持。",
            "videoCodec=\"libx264\"; audioCodec=\"aac\"; pixelFormat=\"yuv420p\";", true, "推荐", "手机", "");
        Add("兼容性 / 播放建议", "browserCompatibility", "MP4 H.264/AAC 或 WebM VP8/VP9/Opus", "浏览器播放常见组合。", "网页播放。", "不同浏览器支持差异。", "container=\"mp4\"", true, "常用", "浏览器", "");
        Add("兼容性 / 播放建议", "editingCompatibility", "MP4 H.264/AAC 或 MOV/ProRes", "剪辑软件友好的格式。", "后期剪辑。", "ProRes 文件巨大。", "videoCodec=\"libx264\"; videoCrf=18;", true, "常用", "剪辑", "");
        Add("兼容性 / 播放建议", "fileSizePriority", "CRF 提高 / Bitrate 降低 / outputScale 降低", "文件小优先。", "网络传输、临时分享。", "画质会下降。", "videoCrf=26; outputScale=0.5f;", true, "常用", "小文件", "");
        Add("兼容性 / 播放建议", "qualityPriority", "CRF 降低 / Bitrate 提高 / outputScale=1", "画质优先。", "正式录制、细节保留。", "文件和性能压力增加。", "videoCrf=18; outputScale=1f;", true, "常用", "画质", "");
        Add("兼容性 / 播放建议", "performancePriority", "preset 更快 / fps 降低 / outputScale 降低 / 硬件编码", "性能优先。", "避免录制卡顿。", "画质和体积可能变差。", "videoPreset=\"ultrafast\"; captureFrameRate=30;", true, "常用", "性能",
            "");
        Add("常见组合", "MP4 + H.264 + AAC", "videoCodec=libx264, audioCodec=aac", "最通用组合。", "绝大多数交付场景。", "不是最小体积，但最稳。", "outputAsWebM=false; videoCodec=\"libx264\"; audioCodec=\"aac\";", true, "推荐",
            "组合", "");
        Add("常见组合", "MP4 + H.265 + AAC", "videoCodec=libx265, audioCodec=aac", "体积更小但兼容稍差。", "较新设备、归档。", "老设备/浏览器可能不支持。", "videoCodec=\"libx265\";", false, "进阶", "组合", "");
        Add("常见组合", "WebM + VP8 + Vorbis", "webmVideoCodec=libvpx, webmAudioCodec=libvorbis", "WebM 传统兼容组合。", "Unity/WebM 测试。", "压缩效率一般。", "webmVideoCodec=\"libvpx\"; webmAudioCodec=\"libvorbis\";",
            true, "推荐", "组合", "");
        Add("常见组合", "WebM + VP9 + Opus", "webmVideoCodec=libvpx-vp9, webmAudioCodec=libopus", "更现代的 WebM 组合。", "小体积/网页。", "编码压力更高。", "webmVideoCodec=\"libvpx-vp9\"; webmAudioCodec=\"libopus\";",
            false, "进阶", "组合", "");
        Add("常见组合", "MKV + 任意编码", "mkv 容器", "MKV 可以容纳很多编码格式。", "中间文件、多轨。", "交付通用性不如 MP4。", "container=\"mkv\"", false, "进阶", "组合", "");
        Add("常见组合", "WAV 临时音频 + MP4 最终 AAC", "pcm_s16le -> aac", "录音临时无压缩，最终合并转 AAC。", "Windows 分离录制音频。", "临时文件大，需要清理。", "audioTempCodec=\"pcm_s16le\"; audioCodec=\"aac\";", true, "推荐", "临时音频", "");

        // ===== 扩展补充：更完整的视频编码配置概念 =====
        Add("编码标准 / 格式", "H.264 / AVC", ".mp4 常用", "目前最通用的视频编码标准之一。", "通用录屏、移动端、网页、剪辑软件。", "压缩效率不如 H.265/AV1，但兼容性最好。", "videoCodec=\"libx264\"", true, "推荐", "avc h264 mp4", "libx264, h264_nvenc");
        Add("编码标准 / 格式", "H.265 / HEVC", "HEVC", "比 H.264 压缩效率更高。", "需要更小体积且目标设备支持。", "兼容性、授权、浏览器支持不如 H.264。", "videoCodec=\"libx265\"", false, "进阶", "hevc h265", "libx265, hevc_nvenc");
        Add("编码标准 / 格式", "VP8", "WebM 常用", "WebM 传统视频编码。", "WebM 实时录屏和兼容测试。", "压缩效率不如 VP9/AV1。", "webmVideoCodec=\"libvpx\"", true, "推荐", "vp8 webm", "libvpx");
        Add("编码标准 / 格式", "VP9", "WebM 常用", "WebM 更高压缩效率视频编码。", "小体积 WebM、网页播放。", "编码比 VP8 更慢。", "webmVideoCodec=\"libvpx-vp9\"", false, "进阶", "vp9 webm", "libvpx-vp9");
        Add("编码标准 / 格式", "AV1", "libaom-av1 / libsvtav1 / librav1e", "压缩效率很高的新一代编码。", "离线压缩、网页新格式、实验。", "软件编码通常很慢，实时录屏不推荐。", "videoCodec=\"libsvtav1\"", false, "实验", "av1", "SVT-AV1, rav1e");
        Add("编码标准 / 格式", "ProRes", "prores_ks / prores_aw", "高质量中间编码，剪辑友好。", "后期剪辑、保留质量。", "文件非常大，不适合普通分享。", "videoCodec=\"prores_ks\"", false, "后期", "prores mov", "MOV");
        Add("编码标准 / 格式", "DNxHD / DNxHR", "dnxhd", "Avid 系中间编码。", "剪辑流程、中间文件。", "文件大，参数要求严格。", "videoCodec=\"dnxhd\"", false, "后期", "dnxhr dnxhd", "MOV/MXF");
        Add("编码标准 / 格式", "MJPEG", "mjpeg", "每帧都是 JPEG 图像。", "简单帧内编码、调试、兼容老工具。", "压缩效率低，文件大。", "videoCodec=\"mjpeg\"", false, "特殊", "mjpeg", "intra");
        Add("编码标准 / 格式", "GIF", "palettegen / paletteuse", "动图格式，不是现代视频编码。", "短循环演示。", "无音频、颜色少、文件可能很大。", "format=\"gif\"", false, "特殊", "gif 动图", "palettegen");
        Add("编码标准 / 格式", "无压缩视频", "rawvideo", "不压缩画面数据。", "极少数调试/专业中间流程。", "文件极大，普通录屏不要用。", "videoCodec=\"rawvideo\"", false, "调试", "rawvideo", "pixelFormat");

        Add("AV1 编码器", "libaom-av1", "软件 AV1", "AOM 官方 AV1 编码器，质量好但慢。", "离线压缩和测试。", "实时录屏基本不推荐。", "videoCodec=\"libaom-av1\"", false, "实验", "av1 aom", "AV1");
        Add("AV1 编码器", "libsvtav1", "SVT-AV1", "更适合多线程的 AV1 编码器。", "离线或强机器压缩。", "仍比 H.264 慢，兼容要测试。", "videoCodec=\"libsvtav1\"", false, "进阶", "svt-av1", "AV1");
        Add("AV1 编码器", "librav1e", "rav1e", "Rust AV1 编码器。", "实验和离线编码。", "可用性取决于 ffmpeg 编译。", "videoCodec=\"librav1e\"", false, "实验", "rav1e", "AV1");
        Add("AV1 编码器", "av1_nvenc", "NVIDIA AV1 硬件编码", "新 NVIDIA 显卡支持的 AV1 硬件编码。", "新显卡实时 AV1。", "需要新硬件、新驱动和 ffmpeg 支持。", "videoCodec=\"av1_nvenc\"", false, "硬件", "nvidia av1", "NVENC");
        Add("AV1 编码器", "av1_qsv", "Intel QSV AV1", "Intel 平台 AV1 硬件编码。", "支持 AV1 QSV 的 Intel 机器。", "兼容和硬件支持要测试。", "videoCodec=\"av1_qsv\"", false, "硬件", "intel av1", "QSV");
        Add("AV1 编码器", "av1_amf", "AMD AMF AV1", "AMD 新显卡 AV1 硬件编码。", "AMD AV1 实时编码。", "需要硬件、驱动、ffmpeg 支持。", "videoCodec=\"av1_amf\"", false, "硬件", "amd av1", "AMF");

        Add("码率控制模式", "CRF", "恒定质量", "以画质为目标，文件大小由内容复杂度决定。", "MP4/x264/x265 录屏最常用。", "不能精确控制最终文件大小。", "videoCrf=23", true, "推荐", "crf 恒定质量", "videoCrf");
        Add("码率控制模式", "CBR", "恒定码率", "尽量保持固定码率。", "直播、网络带宽固定场景。", "复杂画面可能画质下降，简单画面浪费码率。", "rateControl=\"cbr\"", false, "流媒体", "cbr 固定码率", "maxrate, minrate");
        Add("码率控制模式", "VBR", "可变码率", "根据画面复杂度动态分配码率。", "文件录制、质量和体积折中。", "最终大小不完全固定。", "rateControl=\"vbr\"", true, "推荐", "vbr 可变码率", "videoBitrate");
        Add("码率控制模式", "ABR", "平均码率", "以平均码率为目标。", "想大致控制文件大小。", "画质稳定性不如 CRF。", "videoBitrate=\"4M\"", false, "常用", "abr 平均码率", "videoBitrate");
        Add("码率控制模式", "CQP / QP", "固定量化", "使用固定量化参数。", "硬件编码测试或特殊质量控制。", "不同内容画质波动较大。", "qp=23", false, "进阶", "qp cqp 量化", "nvencCq");
        Add("码率控制模式", "CQ", "恒定质量硬件编码", "硬件编码器常见质量模式。", "NVENC/QSV/AMF 等硬件编码。", "和 x264 CRF 不是完全同一概念。", "nvencCq=23", false, "硬件", "cq", "NVENC");
        Add("码率控制模式", "Two-Pass", "二遍编码", "第一遍分析，第二遍编码。", "离线控制目标体积。", "实时录屏不适用。", "twoPass=true", false, "离线", "二遍编码", "pass");
        Add("码率控制模式", "Lossless", "无损编码", "不损失画面信息。", "中间文件、质量验证。", "文件巨大，实时压力高。", "videoCrf=0 或 qp=0", false, "特殊", "无损 lossless", "CRF 0");

        Add("高级画质参数", "B-Frames", "0 / 2 / 3", "B 帧可以提高压缩效率，但增加延迟。", "普通文件输出可开启。", "低延迟录屏/直播常设 0。", "bFrames=2", false, "进阶", "B帧", "GOP");
        Add("高级画质参数", "Reference Frames", "1 / 3 / 4 / 8", "参考帧数量。", "提高压缩效率。", "太多可能影响兼容和解码性能。", "referenceFrames=3", false, "进阶", "参考帧 refs", "Profile");
        Add("高级画质参数", "Lookahead", "0 / 16 / 32", "编码器提前分析后续帧。", "提高码率分配和压缩效率。", "增加延迟和内存，实时录屏慎用。", "lookahead=16", false, "进阶", "前瞻 lookahead", "B-Frames");
        Add("高级画质参数", "Scene Cut", "true / false", "场景变化时自动插入关键帧。", "自然视频压缩。", "固定 GOP/低延迟时可能关闭。", "sceneCut=true", false, "进阶", "场景切换", "GOP");
        Add("高级画质参数", "AQ / Adaptive Quantization", "true / false", "自适应量化，让码率分配更符合视觉感知。", "提高主观质量。", "不同编码器实现不同。", "adaptiveQuantization=true", false, "高级", "aq 自适应量化", "心理视觉");
        Add("高级画质参数", "Psychovisual Tuning", "psy-rd / psy-rdoq", "心理视觉优化。", "让画面看起来更自然。", "过度可能影响客观指标。", "psyRd=1.0", false, "高级", "psy", "x264-params");
        Add("高级画质参数", "Deblock", "去块滤波", "减轻块状压缩痕迹。", "低码率视频。", "过强会变糊。", "deblock=0:0", false, "高级", "去块", "x264");
        Add("高级画质参数", "Trellis", "网格量化优化", "提高压缩效率。", "离线/高质量编码。", "可能增加编码耗时。", "trellis=1", false, "高级", "trellis", "x264");
        Add("高级画质参数", "Entropy Coding", "CABAC / CAVLC", "H.264 熵编码方式。CABAC 压缩更好，CAVLC 兼容更旧。", "现代设备通常用 CABAC。", "Baseline Profile 可能不支持 CABAC。", "cabac=true", false, "进阶", "cabac cavlc", "Profile");

        Add("色彩 / 位深 / HDR", "Bit Depth", "8-bit / 10-bit / 12-bit", "每个颜色通道的位深。", "普通 SDR 用 8-bit；HDR/高质量后期可用 10-bit。", "10-bit 兼容性和文件处理要求更高。", "bitDepth=8", true, "推荐", "位深 bit depth",
            "PixelFormat");
        Add("色彩 / 位深 / HDR", "8-bit SDR", "yuv420p", "普通标准动态范围视频。", "绝大多数录屏。", "不是 HDR。", "pixelFormat=\"yuv420p\"", true, "推荐", "sdr 8bit", "bt709");
        Add("色彩 / 位深 / HDR", "10-bit SDR", "yuv420p10le", "10 位 SDR，渐变更平滑。", "高质量后期。", "兼容性不如 8-bit。", "pixelFormat=\"yuv420p10le\"", false, "进阶", "10bit sdr", "libx265");
        Add("色彩 / 位深 / HDR", "HDR10", "bt2020 + PQ + 10-bit", "HDR10 常见组合。", "HDR 视频处理。", "普通屏幕录制不应该随便输出 HDR。", "colorPrimaries=\"bt2020\"; colorTransfer=\"smpte2084\";", false, "HDR", "hdr10 pq",
            "bt2020");
        Add("色彩 / 位深 / HDR", "HLG", "bt2020 + arib-std-b67", "广播常见 HDR 传输特性。", "HLG HDR 内容。", "普通录屏不用。", "colorTransfer=\"arib-std-b67\"", false, "HDR", "hlg hdr", "bt2020");
        Add("色彩 / 位深 / HDR", "Chroma Subsampling 4:2:0", "yuv420p", "色度分辨率减半，视频最常见。", "兼容性最好。", "彩色细线/文字边缘可能有轻微损失。", "pixelFormat=\"yuv420p\"", true, "推荐", "420 chroma", "PixelFormat");
        Add("色彩 / 位深 / HDR", "Chroma Subsampling 4:2:2", "yuv422p", "色度信息比 4:2:0 更多。", "专业视频流程。", "播放器兼容性较差。", "pixelFormat=\"yuv422p\"", false, "进阶", "422 chroma", "PixelFormat");
        Add("色彩 / 位深 / HDR", "Chroma Subsampling 4:4:4", "yuv444p", "完整色度采样。", "图像处理/后期。", "文件大且兼容差。", "pixelFormat=\"yuv444p\"", false, "进阶", "444 chroma", "PixelFormat");
        Add("色彩 / 位深 / HDR", "Alpha 通道", "yuva420p / prores_ks / qtrle", "透明通道支持。", "需要带透明背景的视频。", "MP4 H.264 通常不支持透明，格式选择要专门设计。", "pixelFormat=\"yuva420p\"", false, "特殊", "alpha 透明",
            "ProRes, WebM VP9 Alpha");

        Add("音频高级概念", "Audio Delay", "毫秒 / 秒", "音频相对视频的延迟。", "修正音画不同步。", "正负方向要测试。", "audioDelay=0.2f", false, "进阶", "音画同步 延迟", "itsoffset");
        Add("音频高级概念", "Audio Normalize", "loudnorm", "响度标准化。", "让不同录制片段音量一致。", "处理较慢，可能改变动态。", "audioNormalize=true", false, "进阶", "响度 loudness", "loudnorm");
        Add("音频高级概念", "Noise Reduction", "afftdn / highpass", "音频降噪。", "麦克风背景噪声。", "过度降噪会产生失真。", "audioDenoise=true", false, "进阶", "降噪", "afftdn");
        Add("音频高级概念", "Limiter", "alimiter", "限制音频峰值，避免爆音。", "声音忽大忽小时。", "设置不当会压扁动态。", "audioLimiter=true", false, "进阶", "限制器 爆音", "volume");
        Add("音频高级概念", "Mix System + Mic", "amix", "混合系统声音和麦克风。", "游戏/软件声音加旁白。", "要处理延迟和音量平衡。", "mixMicAndSystemAudio=true", true, "推荐", "混音", "amix");
        Add("音频高级概念", "Ducking", "旁白时降低系统声音", "人声出现时自动压低背景声音。", "教程讲解。", "需要复杂滤镜或外部处理。", "enableDucking=true", false, "高级", "ducking 自动压低", "sidechaincompress");

        Add("封装 / 交付策略", "Fast Start", "moov 前置", "让 MP4 可以边下载边播放。", "网页播放、上传平台。", "只对 MP4/MOV 有意义。", "fastStart=true", true, "推荐", "faststart", "movflags");
        Add("封装 / 交付策略", "Fragmented MP4", "fMP4", "分片 MP4。", "流媒体、低延迟播放。", "普通文件交付不一定需要。", "fragmentedMp4=true", false, "流媒体", "fmp4", "dash hls");
        Add("封装 / 交付策略", "HLS", "m3u8 + ts/fmp4", "HTTP Live Streaming。", "长视频分段播放、直播。", "会生成多个文件，Unity 普通本地播放不一定适合。", "outputHls=true", false, "流媒体", "hls", "m3u8");
        Add("封装 / 交付策略", "DASH", "mpd + segments", "MPEG-DASH 自适应流媒体。", "网页/流媒体平台。", "配置复杂。", "outputDash=true", false, "流媒体", "dash", "mpd");
        Add("封装 / 交付策略", "Timecode", "时间码", "为视频写入时间码。", "专业后期流程。", "普通录屏不需要。", "timecode=\"00:00:00:00\"", false, "后期", "timecode", "metadata");
        Add("封装 / 交付策略", "Metadata", "title / author / comment", "视频元数据。", "写标题、作者、备注。", "不影响编码质量。", "metadataTitle=\"Recording\"", false, "常用", "metadata 元数据", "title");
        Add("封装 / 交付策略", "Chapters", "章节", "视频章节信息。", "长视频导航。", "普通录屏工具通常不需要。", "writeChapters=true", false, "进阶", "chapters", "metadata");

        Add("错误排查建议", "画面卡顿", "降低 fps / outputScale / preset 更快 / 硬件编码", "录制时画面卡顿通常是编码跟不上。", "低配置机器。", "盲目提高码率不会解决 CPU 编码压力。", "captureFrameRate=30; outputScale=0.5f; videoPreset=\"ultrafast\";",
            true, "排查", "卡顿 性能", "captureFrameRate, outputScale");
        Add("错误排查建议", "文字不清楚", "降低 CRF / 提高码率 / outputScale=1 / 1080p+", "文字糊通常是分辨率或压缩质量不够。", "UI/代码录屏。", "只提高音频码率没用。", "videoCrf=18; outputScale=1f;", true, "排查", "文字 模糊", "videoCrf");
        Add("错误排查建议", "文件太大", "提高 CRF / 降低码率 / 降低 fps / 降低分辨率", "减少视频体积。", "分享和上传。", "体积小一定会牺牲质量或流畅度。", "videoCrf=26; captureFrameRate=30; outputScale=0.75f;", true, "排查", "文件大 小文件", "CRF");
        Add("错误排查建议", "声音不同步", "检查采样率 / thread_queue_size / itsoffset / 合并逻辑", "音画不同步排查方向。", "分离录音录像。", "每个平台设备延迟可能不同。", "audioSampleRate=48000; threadQueueSize=512;", true, "排查", "音画不同步",
            "audioDelay");
        Add("错误排查建议", "播放器打不开", "换 H.264 + AAC + yuv420p + MP4", "最大化播放兼容性。", "给别人播放。", "WebM/HEVC/10-bit 可能不被某些播放器支持。", "videoCodec=\"libx264\"; audioCodec=\"aac\"; pixelFormat=\"yuv420p\";", true,
            "排查", "打不开 兼容", "MP4");
        Add("错误排查建议", "颜色发灰/过曝", "检查 color_range / colorspace / HDR 元数据", "颜色范围或 HDR/SDR 解释错误。", "屏幕录制颜色异常。", "不同播放器解释可能不一致。", "colorRange=\"tv\" 或 \"pc\"", false, "排查", "颜色 发灰", "colorRange");
        Add("错误排查建议", "WebM 编码很慢", "使用 libvpx + realtime + cpu-used=8 / 降低分辨率", "WebM 实时性能优化。", "Unity 内需要 WebM 时。", "画质可能下降，需要提高码率。", "webmDeadline=\"realtime\"; webmCpuUsed=8;", true, "排查",
            "webm 慢", "webmCpuUsed");
        Add("错误排查建议", "硬件编码不可用", "检查 ffmpeg -encoders / 驱动 / 显卡型号", "NVENC/QSV/AMF 失败排查。", "硬件编码。", "有显卡不代表 ffmpeg 支持对应编码器。", "fallbackToSoftware=true", true, "排查", "硬件编码失败", "ffmpeg -encoders");
        Add("错误排查建议", "宽高为奇数导致失败", "输出宽高取偶数", "yuv420p 和很多编码器要求偶数尺寸。", "缩放/裁剪后。", "奇数尺寸会报错或编码失败。", "scale=-2:720", true, "排查", "奇数 宽高", "evenSize");
    }

    private void Add(
        string category,
        string field,
        string values,
        string meaning,
        string whenToUse,
        string warning,
        string example,
        bool   recommended,
        string level,
        string keywords,
        string related)
    {
        _items.Add(new EncodingItem
        {
            category    = category,
            field       = field,
            values      = values,
            meaning     = meaning,
            whenToUse   = whenToUse,
            warning     = warning,
            example     = example,
            recommended = recommended,
            level       = level,
            keywords    = keywords,
            related     = related
        });
    }

    private sealed class EncodingItem
    {
        public string category;
        public string field;
        public string values;
        public string meaning;
        public string whenToUse;
        public string warning;
        public string example;
        public bool recommended;
        public string level;
        public string keywords;
        public string related;
    }
}
#endif

