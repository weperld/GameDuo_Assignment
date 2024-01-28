using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModes
{
    public enum Mode
    {
        CHALLENGE,
        REPEAT,
    }

    private Mode currentMode = Mode.CHALLENGE;

    public Mode _CurrentMode => currentMode;
    public void ChangeGameMode(Mode mode)
    {
        currentMode = mode;
    }
}
