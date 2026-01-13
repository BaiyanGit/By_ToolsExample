namespace _3rdBy.ByTools.ReplaceWithPrefabByMesh.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// 选择一个预制件，扫描场景中所有与该预制件匹配的对象（根据用户选择的匹配模式），并预览这些匹配对象。
    /// 行替换操作，将匹配的对象替换为预制件实例，同时可自由选择是否保留原对象的名称、层级、静态标记等属性，并可选择是否在替换后解体预制件实例。
    /// 批量移动场景中的对象，根据对象名称或用户选择的目标父节点移动对象，并可以选择在移动后删除空的父节点。
    /// </summary>
    public class ReplaceWithPrefabByMeshEditorWindow : EditorWindow
    {
        private GameObject _prefab;
        private GameObject _parent;

        // 扫描选项
        private bool _includeInactive = true;
        private bool _matchSkinnedMeshes = true;
        private bool _matchMeshFilters = true;
        private bool _usePrefabStructure;
        private bool _overrideParentPrefabCheck;

        // 替换选项
        private bool _keepOriginalName = true;
        private bool _parentToOriginalParent = true;
        private bool _copyStaticFlags = true;
        private bool _dryRun = true;
        private bool _breakPrefabLink;
        private bool _showReplaceSection;

        private Vector2 _scroll;
        private readonly List<GameObject> _cachedMatches = new();
        private readonly HashSet<Mesh> _prefabMeshes = new();
        private bool _pendingReplaceConfirm;

        [MenuItem("ByTools/🧩 Prefab替换场景对象 | 场景对象批量移动 &%4", false, 1001)]
        private static void OpenWindow()
        {
            var w = GetWindow<ReplaceWithPrefabByMeshEditorWindow>("Replace By Prefab Mesh");
            w.minSize = new Vector2(460, 500);
        }

        private GUIStyle _tipsStyle;

        private void DrawUseTips()
        {
            var steps = new List<string>
            {
                "1. 在 Project 中选择 Prefab",
                "2. 点击“扫描场景”预览匹配对象",
                "3. 取消勾选“干运行”后点“执行替换”",
                "注意：Play 模式下不可用，替换后请保存场景。"
            };

            var stepText = string.Join("\n", steps);

            string modeText;
            if (_usePrefabStructure)
                modeText = "→ 结构模式：按 Prefab 层级匹配，扫描时会检查完整层级是否与 Prefab 一致。\n";
            else
            {
                modeText = "→ 单 Mesh 模式：按单 Mesh 匹配场景对象。\n";
                modeText += _overrideParentPrefabCheck
                                ? "→ 已勾选：允许父 Prefab 下含 Mesh 时也替换子对象。"
                                : "→ 未勾选：父 Prefab 下有 Mesh 时不会替换子对象。";
            }

            EditorGUILayout.HelpBox(stepText + "\n\n模式说明：\n" + modeText, MessageType.Info);
            EditorGUILayout.Space();
        }

        private void OnGUI()
        {
            _tipsStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                normal   = { textColor = Color.gray }
            };

            DrawUseTips();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            _dryRun = EditorGUILayout.Toggle(_dryRun, GUILayout.Width(18));
            EditorGUILayout.LabelField("干运行 (仅预览，不替换)");
            EditorGUILayout.EndHorizontal();

            DrawReplaceSection();
            DrawMoveSection();
        }

        #region Prefab 替换场景对象

        private void DrawReplaceSection()
        {
            EditorGUILayout.Space();
            _showReplaceSection = EditorGUILayout.Foldout(_showReplaceSection, "Prefab 代替 Mesh", true /*, EditorStyles.boldLabel*/);
            if (!_showReplaceSection) return;

            EditorGUILayout.LabelField("选择 Prefab", EditorStyles.boldLabel);
            _prefab = (GameObject)EditorGUILayout.ObjectField(_prefab, typeof(GameObject), false);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("扫描选项", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _includeInactive = EditorGUILayout.Toggle(_includeInactive, GUILayout.Width(18));
            EditorGUILayout.LabelField("包括未激活对象");

            _matchMeshFilters = EditorGUILayout.Toggle(_matchMeshFilters, GUILayout.Width(18));
            EditorGUILayout.LabelField("匹配 MeshFilter");

            _matchSkinnedMeshes = EditorGUILayout.Toggle(_matchSkinnedMeshes, GUILayout.Width(18));
            EditorGUILayout.LabelField("匹配 SkinnedMeshRenderer");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("替换选项", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _usePrefabStructure = EditorGUILayout.Toggle(_usePrefabStructure, GUILayout.Width(18));
            EditorGUILayout.LabelField("按 Prefab 结构匹配");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("→ √：按完整层级匹配；不勾选：按单 Mesh 匹配", _tipsStyle);

            if (!_usePrefabStructure)
            {
                EditorGUILayout.BeginHorizontal();
                _overrideParentPrefabCheck = EditorGUILayout.Toggle(_overrideParentPrefabCheck, GUILayout.Width(18));
                EditorGUILayout.LabelField("允许替换子对象");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("→ √：父 Prefab 下有 Mesh 时替换子对象；X：父 Prefab 下无 Mesh 时才替换子对象", _tipsStyle);
            }

            EditorGUILayout.BeginHorizontal();
            _parentToOriginalParent = EditorGUILayout.Toggle(_parentToOriginalParent, GUILayout.Width(18));
            EditorGUILayout.LabelField("保持原层级 (Parent)");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("[Tips] √：新对象会放回原来的父节点下", _tipsStyle);

            EditorGUILayout.BeginHorizontal();
            _keepOriginalName = EditorGUILayout.Toggle(_keepOriginalName, GUILayout.Width(18));
            EditorGUILayout.LabelField("保持原名称");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("[Tips] √：替换后保留原对象名称，不勾选：使用 Prefab 名称", _tipsStyle);

            EditorGUILayout.BeginHorizontal();
            _copyStaticFlags = EditorGUILayout.Toggle(_copyStaticFlags, GUILayout.Width(18));
            EditorGUILayout.LabelField("继承原对象的静态标记(批处理/导航/光照)");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("[Tips] √：新对象会继承原对象的静态标记，不勾选：新对象不继承原对象的静态标记", _tipsStyle);

            EditorGUILayout.BeginHorizontal();
            _breakPrefabLink = EditorGUILayout.Toggle(_breakPrefabLink, GUILayout.Width(18));
            EditorGUILayout.LabelField("替换后解体");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("[Tips] √：替换后新对象将脱离 Prefab 关联，不勾选：新对象仍然与 Prefab 关联", _tipsStyle);

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(_prefab == null))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("扫描场景"))
                    FindAndListMatches();

                if (!_dryRun && GUILayout.Button("执行替换"))
                    _pendingReplaceConfirm = true;

                EditorGUILayout.EndHorizontal();
            }

            if (_cachedMatches.Count > 0)
            {
                EditorGUILayout.LabelField($"匹配对象列表 ({_cachedMatches.Count})", EditorStyles.boldLabel);
                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(180));
                for (var i = 0; i < _cachedMatches.Count; i++)
                {
                    var go = _cachedMatches[i];
                    if (go == null) continue;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{i + 1}. {GetPath(go.transform)}", GUILayout.MaxHeight(18));
                    if (GUILayout.Button("选中", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = go;
                        EditorGUIUtility.PingObject(go);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }

            if (_pendingReplaceConfirm)
            {
                _pendingReplaceConfirm      =  false;
                EditorApplication.delayCall += TryReplaceWithConfirm;
            }
        }

        // === 核心逻辑 ===
        private void CollectPrefabMeshes()
        {
            _prefabMeshes.Clear();
            if (_prefab == null) return;

            foreach (var mf in _prefab.GetComponentsInChildren<MeshFilter>(true))
                if (mf.sharedMesh != null)
                    _prefabMeshes.Add(mf.sharedMesh);

            foreach (var sk in _prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if (sk.sharedMesh != null)
                    _prefabMeshes.Add(sk.sharedMesh);
        }

        private void FindAndListMatches()
        {
            _cachedMatches.Clear();
            CollectPrefabMeshes();

            if (_prefabMeshes.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "Prefab 中没有 Mesh，无法匹配。", "OK");
                return;
            }

            var allRoots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in allRoots)
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(_includeInactive))
                {
                    var go = t.gameObject;
                    if (_usePrefabStructure)
                    {
                        if (IsStructureMatch(sceneObj: go, prefabObj: _prefab))
                            _cachedMatches.Add(go);
                    }
                    else
                    {
                        if (IsSingleMeshMatch(go))
                        {
                            if (!_overrideParentPrefabCheck && IsParentPrefabWithMesh(go))
                                continue;
                            _cachedMatches.Add(go);
                        }
                    }
                }
            }

            Debug.Log($"[ReplaceByPrefabMesh] 找到 {_cachedMatches.Count} 个匹配对象。");
        }

        private bool IsSingleMeshMatch(GameObject go)
        {
            if (_matchMeshFilters)
            {
                var mf = go.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null && _prefabMeshes.Contains(mf.sharedMesh))
                    return true;
            }

            if (_matchSkinnedMeshes)
            {
                var smr = go.GetComponent<SkinnedMeshRenderer>();
                if (smr != null && smr.sharedMesh != null && _prefabMeshes.Contains(smr.sharedMesh))
                    return true;
            }

            return false;
        }

        private bool IsParentPrefabWithMesh(GameObject go)
        {
            var t = go.transform.parent;
            while (t != null)
            {
                if (PrefabUtility.IsPartOfPrefabAsset(t.gameObject))
                {
                    if (t.GetComponentInChildren<MeshFilter>(true) != null ||
                        t.GetComponentInChildren<SkinnedMeshRenderer>(true) != null)
                        return true;
                }

                t = t.parent;
            }

            return false;
        }

        private static bool IsStructureMatch(GameObject sceneObj, GameObject prefabObj)
        {
            var prefabChildrenDict = new Dictionary<string, GameObject>();
            for (var i = 0; i < prefabObj.transform.childCount; i++)
                prefabChildrenDict[prefabObj.transform.GetChild(i).name] = prefabObj.transform.GetChild(i).gameObject;

            if (sceneObj.transform.childCount != prefabChildrenDict.Count) return false;

            for (var i = 0; i < sceneObj.transform.childCount; i++)
            {
                var sceneChild = sceneObj.transform.GetChild(i).gameObject;
                if (!prefabChildrenDict.TryGetValue(sceneChild.name, out var prefabChild))
                    return false;

                if (!IsStructureMatch(sceneChild, prefabChild))
                    return false;
            }

            return true;
        }

        private void TryReplaceWithConfirm()
        {
            if (_cachedMatches.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先扫描场景，未找到匹配对象。", "OK");
                return;
            }

            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("错误", "Play Mode 下不可替换！", "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("确认替换", $"将替换 {_cachedMatches.Count} 个对象，是否继续？", "Yes", "Cancel"))
                ReplaceAllMatches();
        }

        private void ReplaceAllMatches()
        {
            var total = _cachedMatches.Count;
            for (var i = 0; i < total; i++)
            {
                var orig = _cachedMatches[i];
                if (orig == null) continue;

                EditorUtility.DisplayProgressBar("替换中...", $"{i + 1}/{total} {GetPath(orig.transform)}", (float)i / total);

                var origParent = orig.transform.parent;
                var origLocalPos = orig.transform.localPosition;
                var origLocalRot = orig.transform.localRotation;
                var origLocalScale = orig.transform.localScale;
                var origName = orig.name;
                var origTag = orig.tag;
                var origActive = orig.activeSelf;
                var origStatic = GameObjectUtility.GetStaticEditorFlags(orig);

                var newInstance = (GameObject)PrefabUtility.InstantiatePrefab(_prefab, orig.scene);
                Undo.RegisterCreatedObjectUndo(newInstance, "Replace With Prefab");

                if (_breakPrefabLink)
                    PrefabUtility.UnpackPrefabInstance(newInstance, PrefabUnpackMode.Completely, InteractionMode.UserAction);


                if (_parentToOriginalParent)
                {
                    newInstance.transform.SetParent(origParent, false);
                    newInstance.transform.localPosition = origLocalPos;
                    newInstance.transform.localRotation = origLocalRot;
                    newInstance.transform.localScale    = origLocalScale;
                }
                else
                {
                    newInstance.transform.position   = orig.transform.position;
                    newInstance.transform.rotation   = orig.transform.rotation;
                    newInstance.transform.localScale = origLocalScale;
                }

                if (_keepOriginalName) newInstance.name = origName;
                newInstance.tag = origTag;
                newInstance.SetActive(origActive);
                if (_copyStaticFlags) GameObjectUtility.SetStaticEditorFlags(newInstance, origStatic);

                Undo.DestroyObjectImmediate(orig);
            }

            EditorUtility.ClearProgressBar();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log($"[ReplaceByPrefabMesh] 替换完成，共替换 {total} 个对象。");
        }

        private static string GetPath(Transform t)
        {
            var path = t.name;
            while (t.parent != null)
            {
                t    = t.parent;
                path = t.name + "/" + path;
            }

            return path;
        }

        #endregion

        #region 新增：批量移动模块

        private GameObject _moveTargetParent;
        private string _targetName;
        private readonly List<GameObject> _cachedMoveMatches = new();
        private bool _pendingMoveConfirm;
        private bool _deleteEmptyParent;
        private bool _showObjectMove;

        private void DrawMoveSection()
        {
            _showObjectMove = EditorGUILayout.Foldout(_showObjectMove, "批量移动模块", true /*, EditorStyles.boldLabel*/);
            if (!_showObjectMove) return;

            _moveTargetParent = (GameObject)EditorGUILayout.ObjectField("目标父节点", _moveTargetParent, typeof(GameObject), true);
            EditorGUILayout.LabelField("[Tips] 若没有选择父节点，则移动到场景根节点", _tipsStyle);

            EditorGUILayout.BeginHorizontal();
            _targetName = EditorGUILayout.TextField("对象名称", _targetName);
            if (GUILayout.Button("从选中对象获取", GUILayout.Width(140)))
            {
                if (Selection.activeGameObject != null)
                    _targetName = Selection.activeGameObject.name;
                else
                    EditorUtility.DisplayDialog("提示", "请先在层次视图中选中一个对象。", "OK");
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _deleteEmptyParent = EditorGUILayout.Toggle(_deleteEmptyParent, GUILayout.Width(18));
            EditorGUILayout.LabelField("移动后删除空父节点");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("[Tips] √：若父节点下无其他子物体则会被删除；X：保留父节点", _tipsStyle);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("扫描场景(按名称)"))
                FindAndListMoveMatches();

            if (!_dryRun && GUILayout.Button("执行移动"))
                _pendingMoveConfirm = true;

            EditorGUILayout.EndHorizontal();

            if (_cachedMoveMatches.Count > 0)
            {
                EditorGUILayout.LabelField($"匹配对象列表 ({_cachedMoveMatches.Count})", EditorStyles.boldLabel);
                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(180));
                for (var i = 0; i < _cachedMoveMatches.Count; i++)
                {
                    var go = _cachedMoveMatches[i];
                    if (go == null) continue;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{i + 1}. {GetPath(go.transform)}", GUILayout.MaxHeight(18));
                    if (GUILayout.Button("选中", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = go;
                        EditorGUIUtility.PingObject(go);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }

            if (_pendingMoveConfirm)
            {
                _pendingMoveConfirm         =  false;
                EditorApplication.delayCall += TryMoveWithConfirm;
            }
        }

        private void FindAndListMoveMatches()
        {
            _cachedMoveMatches.Clear();

            if (string.IsNullOrEmpty(_targetName))
            {
                EditorUtility.DisplayDialog("提示", "请输入对象名称或从选中对象获取。", "OK");
                return;
            }

            var allRoots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in allRoots)
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(_includeInactive))
                {
                    if (t.name == _targetName)
                        _cachedMoveMatches.Add(t.gameObject);
                }
            }

            Debug.Log($"[ReplaceByPrefabMesh-Move] 找到 {_cachedMoveMatches.Count} 个匹配对象。");
        }

        private void TryMoveWithConfirm()
        {
            if (_cachedMoveMatches.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先扫描场景，未找到匹配对象。", "OK");
                return;
            }

            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("错误", "Play Mode 下不可移动！", "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("确认移动", $"将移动 {_cachedMoveMatches.Count} 个对象，是否继续？", "Yes", "Cancel"))
                MoveAllMatches();
        }

        private void MoveAllMatches()
        {
            var total = _cachedMoveMatches.Count;
            var oldParents = new HashSet<Transform>();

            try
            {
                for (var i = 0; i < total; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("移动中...", $"{i + 1}/{total} {GetPath(_cachedMoveMatches[i].transform)}", (float)i / total))
                    {
                        Debug.Log("用户取消了移动操作");
                        break;
                    }

                    var go = _cachedMoveMatches[i];
                    if (go == null) continue;

                    if (_deleteEmptyParent && go.transform.parent != null)
                        oldParents.Add(go.transform.parent);

                    Undo.SetTransformParent(go.transform, _moveTargetParent != null ? _moveTargetParent.transform : null, "Move Object");
                }

                if (_deleteEmptyParent)
                {
                    var parentsToRemove = new List<Transform>();
                    foreach (var p in oldParents)
                    {
                        if (p == null) continue;
                        if (p.childCount == 0)
                            parentsToRemove.Add(p);
                    }

                    foreach (var p in parentsToRemove)
                    {
                        Debug.Log($"[删除空父节点] \n {GetPath(p)}");
                        Undo.DestroyObjectImmediate(p.gameObject);
                    }
                }

                Debug.Log($"移动完成，共移动  {total} 个对象。");
            }
            catch (Exception e)
            {
                Debug.LogError($"移动过程中发生错误:\n{e.Message}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }

        #endregion
    }
}