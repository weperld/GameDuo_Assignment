using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveManager : Manager<WaveManager>
{
    #region Wave Variables
    private const float WaveStartDelay = 3f;
    public Enums.WaveSequence _WaveSequence { get; private set; } = Enums.WaveSequence.NONE;

    /// <summary>
    /// ù ���̺�� 1���� �����ϵ��� ����
    /// </summary>
    private int totalWaveCount = 0;
    public int _StageCount => (totalWaveCount - 1) / 5 + 1;
    public int _WaveCount => (totalWaveCount - 1) % 5 + 1;
    public bool _IsBossWave => _WaveCount == 5;

    /// <summary>
    /// ���̺갡 ���۵Ǵ� ������ ���͸� �������ָ鼭 ������ ������ ������ ����
    /// </summary>
    private Monster[] generatedMonsters;
    private Vector3[] monsterGenPositions;
    #endregion

    #region Barricade Variables
    private bool barricadeGenPosIsRegisted = false;
    private bool barricadeIsGenerated = false;
    private Vector3 barricadeWorldPos;
    public Barricade _Barricade { get; private set; }
    #endregion

    #region Actions
    /// <summary>
    /// ���̺� Ŭ���� �� ���� �׼�(Ŭ���� ������ �������� ��ȣ, Ŭ���� ������ ���̺� ��ȣ)
    /// </summary>
    public ActionTemplate<int, int> _ActionOnClearWave { get; } = new ActionTemplate<int, int>();
    /// <summary>
    /// ���̺� ���� Ȥ�� ���� �� ���� �׼�(���� ������ �������� ��ȣ, ���� ������ ���̺� ��ȣ)
    /// </summary>
    public ActionTemplate<int, int> _ActionOnStartWave { get; } = new ActionTemplate<int, int>();
    #endregion

    #region ���̺� ����
    /// <summary>
    /// ���� ���̺� ����<para/>
    /// �ݺ� ����� ��� ������ ���̺�� �ٽ� ����
    /// </summary>
    public void StartNextWave(int nextStep = 1)
    {
        if (_WaveSequence != Enums.WaveSequence.CLEAR && _WaveSequence != Enums.WaveSequence.NONE) return;

        if (GameManager.IsDestroying) return;

        totalWaveCount += GameManager.Instance._GameMode == GameModes.Mode.CHALLENGE ? Mathf.Max(1, nextStep) : 0;
        StartCoroutine(WaitOfWaveStart());
        if (!barricadeIsGenerated) CreateBarricade();
        SpawnEnemies(5, _IsBossWave);
    }
    private IEnumerator WaitOfWaveStart()
    {
        _WaveSequence = Enums.WaveSequence.WAIT;
        yield return new WaitForSeconds(WaveStartDelay);
        _WaveSequence = Enums.WaveSequence.START;
        _ActionOnStartWave.Action(_StageCount, _WaveCount);
    }

    /// <summary>
    /// ���̺� ��ŵ<para/>
    /// �ݺ� ����� ��� ������ ���̺�� �ٽ� ����
    /// </summary>
    /// <param name="skipWaveStep"></param>
    public void SkipWave(int skipWaveStep = 1)
    {
        Debug.Log($"Skip Wave {skipWaveStep}lv");
        StartNextWave(skipWaveStep);
    }

    private void HandleWaveClear()
    {
        _WaveSequence = Enums.WaveSequence.CLEAR;
        generatedMonsters = null;
        _ActionOnClearWave.Action(_StageCount, _WaveCount);
        StartNextWave();
    }
    private void HandleBossWave()
    {
        // Boss wave logic
    }
    #endregion

    #region ���� ���� �Լ�
    public void RegistMonsterGenPositions(params Vector3[] positions)
    {
        monsterGenPositions = positions;
    }
    private void SpawnEnemies(int spawnCount, bool spawnBoss)
    {
        if (SpawnManager.IsDestroying) return;

        string spawnRsc = _StageCount switch
        {
            1 => PathOfResources.Prefabs.Zombie01,
            2 => PathOfResources.Prefabs.Zombie02,
            3 => PathOfResources.Prefabs.Zombie03,
            4 => PathOfResources.Prefabs.Zombie04,
            _ => PathOfResources.Prefabs.Zombie04,
        };

        generatedMonsters = SpawnManager.Instance.SpawnMany<Monster>(spawnCount, spawnRsc, GameManager.IsDestroying ? null : GameManager.Instance._RootOfGeneratePosition);
        if (generatedMonsters == null || generatedMonsters.Length == 0) return;

        for (int i = 0; i < generatedMonsters.Length; i++)
        {
            var monster = generatedMonsters[i];
            if (monster == null) continue;

            var genPos = monsterGenPositions == null || monsterGenPositions.Length == 0
                ? new Vector3(11f, -8f, 0f)
                : monsterGenPositions[Random.Range(0, generatedMonsters.Length)];
            monster.transform.position = genPos;
            monster.SetResourcePath(spawnRsc);
            monster._ActionOnCharacterIsDead.RegistAction(OnDeadMonster);
            monster.AddNewOrderActions(
                new Params.CharacterActionParam(Enums.CharacterActionState.WAIT, Enums.OrderOnEndCharacterAction.REMOVE_ACTION, i * 0.5f + Random.Range(-0.2f, 0.2f)),
                new Params.CharacterActionParam(Enums.CharacterActionState.WALK, Enums.OrderOnEndCharacterAction.REMOVE_ACTION, _Barricade),
                new Params.CharacterActionParam(Enums.CharacterActionState.ATTACK, Enums.OrderOnEndCharacterAction.REPEAT, _Barricade));
        }
    }
    private void OnDeadMonster(Character monster)
    {
        monster._ActionOnCharacterIsDead.RemoveAction(OnDeadMonster);

        int deadCount = generatedMonsters.Where(w => w._IsDead).Count();
        if (deadCount < generatedMonsters.Length) return;

        HandleWaveClear();
    }

    public Monster[] GetActivatedMonsters()
    {
        return generatedMonsters?.Where(s => s != null && !s._IsDead).ToArray();
    }
    #endregion

    #region �ٸ����̵� ���� �Լ�
    public void RegistBarricadePosition(Vector3 pos)
    {
        barricadeGenPosIsRegisted = true;
        barricadeWorldPos = pos;
    }
    public void CreateBarricade()
    {
        if (!barricadeGenPosIsRegisted) return;
        if (SpawnManager.IsDestroying) return;

        var barricade = SpawnManager.Instance.Spawn<Barricade>(PathOfResources.Prefabs.Barricade, GameManager.IsDestroying ? null : GameManager.Instance._RootOfGeneratePosition);
        if (barricade == null) return;

        barricadeIsGenerated = true;
        _Barricade = barricade;
        barricade.transform.position = barricadeWorldPos;
        barricade.SetResourcePath(PathOfResources.Prefabs.Barricade);
        barricade._ActionOnCharacterIsDead.RegistAction(OnDestroyBarricade);
    }
    private void OnDestroyBarricade(Character barricade)
    {
        if (!GameManager.IsDestroying) GameManager.Instance.ChangeGameMode(GameModes.Mode.REPEAT);

        barricadeIsGenerated = false;
        barricade._ActionOnCharacterIsDead.RemoveAction(OnDestroyBarricade);
        _Barricade = null;
    }
    #endregion
}