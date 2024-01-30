using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : Manager<GameManager>
{
    #region User's Character Variables
    /// <summary>
    /// 게임이 시작되는 등 유저의 캐릭터가 생성될 때 각 캐릭터의 정보를 매니저에 등록하여야 함
    /// </summary>
    private HashSet<Character> set_ActivatedCharacters = new HashSet<Character>();
    #endregion

    #region Game Mode Variables
    private GameModes gameModeData = new GameModes();

    private ActionTemplate<GameModes.Mode, GameModes.Mode> actionOnChangeMode = new ActionTemplate<GameModes.Mode, GameModes.Mode>();
    public ActionTemplate<GameModes.Mode, GameModes.Mode> _ActionOnChangeMode => actionOnChangeMode;

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

    public Character[] GetActivatedCharacterSet() => (new List<Character>(set_ActivatedCharacters)).ToArray();
    public bool AddActivatedCharacterToSet(Character character)
    {
        if (set_ActivatedCharacters.Contains(character)) return false;
        set_ActivatedCharacters.Add(character);
        return true;
    }
    public bool RemoveCharacterOfSet(Character character)
    {
        if (!set_ActivatedCharacters.Contains(character)) return false;
        set_ActivatedCharacters.Remove(character);
        return true;
    }
}
