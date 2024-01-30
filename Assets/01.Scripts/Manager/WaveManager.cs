using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : Manager<WaveManager>
{
    #region Wave Variables
    /// <summary>
    /// 첫 웨이브는 1부터 시작하도록 구현
    /// </summary>
    private int totalWaveCount = 0;
    public int _StageCount => (totalWaveCount - 1) / 5 + 1;
    public int _WaveCount => (totalWaveCount - 1) % 5 + 1;

    /// <summary>
    /// 스테이지 및 웨이브가 시작되는 시점에 몬스터를 생성해주면서 생성된 몬스터의 정보를 저장<para/>
    /// 저장된 몬스터 중 체력이 다 하여 죽는 경우 등의 몬스터가 사라지는 상황이 발생할 경우 저장된 정보를 제거해주어야 함
    /// </summary>
    private HashSet<Character> set_GeneratedMonsters = new HashSet<Character>();
    #endregion

    #region Actions
    private ActionTemplate<int, int> actionOnClearWave = new ActionTemplate<int, int>();
    private ActionTemplate<int, int> actionOnChangeWave = new ActionTemplate<int, int>();

    /// <summary>
    /// 웨이브 클리어 시 수행 액션(클리어 시점의 스테이지 번호, 클리어 시점의 웨이브 번호)
    /// </summary>
    public ActionTemplate<int, int> _ActionOnClearWave => actionOnClearWave;
    /// <summary>
    /// 웨이브 변경 혹은 시작 시 수행 액션(시작 시점의 스테이지 번호, 시작 시점의 웨이브 번호)
    /// </summary>
    public ActionTemplate<int, int> _ActionOnChangeWave => actionOnChangeWave;
    #endregion

    private void Start()
    {
        StartNextWave();
    }

    /// <summary>
    /// 다음 웨이브 시작
    /// </summary>
    private void StartNextWave(int nextStep = 1)
    {
        totalWaveCount += Mathf.Max(1, nextStep);
        SpawnEnemies(5, _WaveCount == 5);
        _ActionOnChangeWave.Action(_StageCount, _WaveCount);
        UIManager.Instance.ActivateWaveUI(_StageCount, _WaveCount);
    }
    public void SkipWave(int skipWaveStep = 1) => StartNextWave(skipWaveStep);

    private void SpawnEnemies(int spawnCount, bool spawnBoss)
    {
        // Enemy spawn logic
    }

    private void HandleWaveClear()
    {
        _ActionOnClearWave.Action(_StageCount, _WaveCount);
    }

    private void HandleBossWave()
    {
        // Boss wave logic
    }

    public Character[] GetCurrentActiveMonsters()
    {
        var list = new List<Character>(set_GeneratedMonsters);
        int si = list.Count - 1;
        for (int i = si; i >= 0; i--)
        {
            var character = list[i];
            if (character != null && !character._IsDead) continue;

            list.RemoveAt(i);
        }

        return list.ToArray();
    }
}
