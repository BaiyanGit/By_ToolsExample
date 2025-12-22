using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

public class RecodeScreen : MonoBehaviour
{
    [Serializable]
    public class GameState
    {
        public Vector3 playerPos;
        public Vector3 playerRot;
    }

    [Header("播放速度"), Range(1, 5)] public float playSpeed = 1;
    [Header("是否记录")] public bool isRecode;
    [Header("是否回放")] public bool isBackPlay;
    [Header("回放步骤")] public int playStep;
    [Header("保存间隔"), Tooltip("单位：秒")] public float saveInterval = 0.1f; //保存间隔 单位：秒
    [Header("存档列表")] public List<GameState> _gameStates = new();
    private float _interval; //记录间隔
    private RecodeMove _recodeMove;

    private void Start()
    {
        _recodeMove = GetComponent<RecodeMove>();
    }

    /// <summary>
    /// 加载记录文件
    /// </summary>
    private void LoadSimulate()
    {
        var startSimulate = StartSimulate();
        StartCoroutine(startSimulate);
    }

    /// <summary>
    /// 开始模拟回放
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartSimulate()
    {
        while (true)
        {
            Simulate(playStep);
            if (playStep >= _gameStates.Count)
            {
                playStep = 0;
                Debug.Log("Play Reset");
            }

            playStep++;
            yield return new WaitForSeconds(0.1f / playSpeed);
        }
    }

    /// <summary>
    /// 开始模拟
    /// </summary>
    private void Simulate(int stepIndex)
    {
        if (stepIndex > _gameStates.Count - 1)
        {
            Debug.Log("Play End");
            return;
        }

        var gameState = _gameStates[stepIndex];
        var transform1 = transform;
        transform1.position = gameState.playerPos;
        transform1.eulerAngles = gameState.playerRot;
        // Debug.Log($"ReadData  Pos:{gameState.playerPos} Rot:{gameState.playerRot}");
    }

    /// <summary>
    /// 保存游戏状态
    /// </summary>
    private void SaveGameState()
    {
        var transform1 = transform;
        var gameState = new GameState
        {
            playerPos = transform1.position,
            playerRot = transform1.eulerAngles
        };
        _gameStates.Add(gameState);
        Debug.LogWarning($"SaveData  Pos:{gameState.playerPos} Rot:{gameState.playerRot}");
    }

    private void Update()
    {
        if (isBackPlay)
        {
            isRecode = false;
            _recodeMove.enabled = false;
            isBackPlay = false;
            LoadSimulate();
        }

        if (!isRecode) return;
        isBackPlay = false;
        _recodeMove.enabled = true;
        _interval += Time.deltaTime;

        if (!(_interval >= saveInterval)) return;

        _interval = 0;
        SaveGameState();
    }
}