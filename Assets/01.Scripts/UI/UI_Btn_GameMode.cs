using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Btn_GameMode : MonoBehaviour
{
    public TextMeshProUGUI tmpu_Btn;

    private void Awake()
    {
        if (!GameManager.IsDestroying)
        {
            GameManager.Instance._ActionOnChangeMode.RegistAction(OnChangeGameMode);
            OnChangeGameMode(GameManager.Instance._GameMode, GameManager.Instance._GameMode);
        }
    }

    private void OnChangeGameMode(GameModes.Mode prevMode, GameModes.Mode currentMode)
    {
        if (tmpu_Btn == null) return;

        tmpu_Btn.text = currentMode switch
        {
            GameModes.Mode.CHALLENGE => "도전 모드",
            GameModes.Mode.REPEAT => "반복 모드",
            _ => "모드 값 오류"
        };
    }
    public void OnClickButton()
    {
        if (GameManager.IsDestroying) return;
        GameManager.Instance.ChangeGameMode();
    }
}
