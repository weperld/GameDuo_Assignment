using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : Manager<WaveManager>
{
    #region Wave Variables
    /// <summary>
    /// ù ���̺�� 1���� �����ϵ��� ����
    /// </summary>
    private int totalWaveCount = 0;
    public int _StageCount => (totalWaveCount - 1) / 5 + 1;
    public int _WaveCount => (totalWaveCount - 1) % 5 + 1;

    /// <summary>
    /// �������� �� ���̺갡 ���۵Ǵ� ������ ���͸� �������ָ鼭 ������ ������ ������ ����<para/>
    /// ����� ���� �� ü���� �� �Ͽ� �״� ��� ���� ���Ͱ� ������� ��Ȳ�� �߻��� ��� ����� ������ �������־�� ��
    /// </summary>
    private HashSet<Character> set_GeneratedMonsters = new HashSet<Character>();
    #endregion

    #region Actions
    private ActionTemplate<int, int> actionOnClearWave = new ActionTemplate<int, int>();
    private ActionTemplate<int, int> actionOnChangeWave = new ActionTemplate<int, int>();

    /// <summary>
    /// ���̺� Ŭ���� �� ���� �׼�(Ŭ���� ������ �������� ��ȣ, Ŭ���� ������ ���̺� ��ȣ)
    /// </summary>
    public ActionTemplate<int, int> _ActionOnClearWave => actionOnClearWave;
    /// <summary>
    /// ���̺� ���� Ȥ�� ���� �� ���� �׼�(���� ������ �������� ��ȣ, ���� ������ ���̺� ��ȣ)
    /// </summary>
    public ActionTemplate<int, int> _ActionOnChangeWave => actionOnChangeWave;
    #endregion

    private void Start()
    {
        StartNextWave();
    }

    /// <summary>
    /// ���� ���̺� ����
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
