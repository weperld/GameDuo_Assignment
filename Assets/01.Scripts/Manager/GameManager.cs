using System.Collections.Generic;
using UnityEngine;

public class GameManager : Manager<GameManager>
{
    private Transform tf_RootGeneratePosition;
    public Transform _RootOfGeneratePosition => tf_RootGeneratePosition;

    #region User's Character Variables
    /// <summary>
    /// 게임이 시작되는 등 유저의 캐릭터가 생성될 때 각 캐릭터의 정보를 매니저에 등록하여야 함
    /// </summary>
    private HashSet<Character> set_ActivatedArchers = new HashSet<Character>();

    private bool archerGenPosIsRegisted = false;
    private Vector3[] archerGenPositions;
    #endregion

    #region Game Mode Variables
    private GameModes gameModeData = new GameModes();

    private ActionTemplate<GameModes.Mode, GameModes.Mode> actionOnChangeMode = new ActionTemplate<GameModes.Mode, GameModes.Mode>();
    /// <summary>
    /// (previous mode, current mode)
    /// </summary>
    public ActionTemplate<GameModes.Mode, GameModes.Mode> _ActionOnChangeMode => actionOnChangeMode;

    public GameModes.Mode _GameMode
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

    public Character[] GetActivatedArchers() => (new List<Character>(set_ActivatedArchers)).ToArray();
    public void RegistArcherGenPositions(params Vector3[] archerGenPositions)
    {
        if (archerGenPositions == null || archerGenPositions.Length == 0) return;

        archerGenPosIsRegisted = true;
        this.archerGenPositions = archerGenPositions;
    }
    public void CreateArcher(int archerNumber = 1)
    {
        archerNumber = Mathf.Clamp(archerNumber, 1, 2);

        string rscPath = archerNumber switch
        {
            1 => PathOfResources.Prefabs.Archer01,
            2 => PathOfResources.Prefabs.Archer02,
            _ => PathOfResources.Prefabs.Archer01,
        };

        if (SpawnManager.IsDestroying) return;
        var archer = SpawnManager.Instance.Spawn<Archer>(rscPath, _RootOfGeneratePosition);

        if (archer == null || !archerGenPosIsRegisted) return;
        int randomPosIndex = UnityEngine.Random.Range(0, archerGenPositions.Length);
        var randomPos = archerGenPositions[randomPosIndex];
        archer.transform.position = randomPos;
        archer.SetResourcePath(rscPath);

        AddActivatedArcherToSet(archer);
    }
    private void AddActivatedArcherToSet(Character archer)
    {
        if (archer == null || set_ActivatedArchers.Contains(archer)) return;
        set_ActivatedArchers.Add(archer);
        archer._ActionOnCharacterIsDead.RegistAction(RemoveArcherOfSet);
    }
    private void RemoveArcherOfSet(Character archer)
    {
        if (archer == null || !set_ActivatedArchers.Contains(archer)) return;
        set_ActivatedArchers.Remove(archer);
        archer._ActionOnCharacterIsDead.RemoveAction(RemoveArcherOfSet);
    }

    public void ChangeGameMode()
    {
        var currentMode = _GameMode;
        var nextMode = currentMode switch
        {
            GameModes.Mode.REPEAT => GameModes.Mode.CHALLENGE,
            GameModes.Mode.CHALLENGE => GameModes.Mode.REPEAT,
        };
        _GameMode = nextMode;
    }
    public void ChangeGameMode(GameModes.Mode mode)
    {
        if (_GameMode == mode) return;
        _GameMode = mode;
    }

    public void SetRootGeneratePosition(Transform root)
    {
        tf_RootGeneratePosition = root;
    }
}
