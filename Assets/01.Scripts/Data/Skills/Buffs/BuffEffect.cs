using System;
using UnityEngine;

public class BuffEffect
{
    private Enums.BuffStat.Stat buffStat;
    private float duration;
    private float boostValue;

    public Enums.BuffStat.Stat _BuffStat => buffStat;
    public float _Duration => duration;
    public float _BoostValue => boostValue;

    public BuffEffect(Enums.BuffStat.Stat buffStat, float duration, float boostValue)
    {
        this.buffStat = buffStat;
        this.duration = Mathf.Max(0f, duration);
        this.boostValue = boostValue;
    }
}
