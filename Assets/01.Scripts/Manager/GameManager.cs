using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    #region Wave Variables
    /// <summary>
    /// 첫 웨이브는 1부터 시작하도록 구현
    /// </summary>
    private int totalWaveCount = 0;
    public int _StageCount => (totalWaveCount - 1) / 5 + 1;
    public int _WaveCount => (totalWaveCount - 1) % 5 + 1;

    private ActionTemplates<int, int> actionOnChangeWave = new ActionTemplates<int, int>();
    public ActionTemplates<int, int> _ActionOnChangeWave => actionOnChangeWave;
    #endregion

    #region Game Mode Variables
    private GameModes gameModeData = new GameModes();

    private ActionTemplates<GameModes.Mode, GameModes.Mode> actionOnChangeMode = new ActionTemplates<GameModes.Mode, GameModes.Mode>();
    public ActionTemplates<GameModes.Mode, GameModes.Mode> _ActionOnChangeMode => actionOnChangeMode;

    public GameModes.Mode _CurrentGameMode
    {
        get => gameModeData._CurrentMode;
        set
        {
            var prevMode = gameModeData._CurrentMode;
            gameModeData.ChangeGameMode(value);
            _ActionOnChangeMode.Action(prevMode, value);
        }
    }
    #endregion

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        else { instance = this; DontDestroyOnLoad(gameObject); }
    }

    void Start()
    {
        StartNextWave();
    }

    /// <summary>
    /// 다음 웨이브 시작
    /// </summary>
    void StartNextWave()
    {
        totalWaveCount++;
        SpawnEnemies(5, _WaveCount == 5);
        _ActionOnChangeWave.Action(_StageCount, _WaveCount);
    }

    void SpawnEnemies(int spawnCount, bool spawnBoss)
    {
        // Enemy spawning logic
    }

    void HandleWaveClear()
    {
        // Reward logic
    }

    void HandleBossWave()
    {
        // Boss wave logic
    }
}
