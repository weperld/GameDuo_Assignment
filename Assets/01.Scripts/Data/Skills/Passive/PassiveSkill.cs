using System;
using UnityEngine;
using Enums;

[Serializable]
public class PassiveSkill : Skill
{
    protected const float BasePercent = 10f;

    [SerializeField] protected PassiveActiveCondition activeCond;
    [SerializeField, Range(0f, 100f)] protected float activateChancePercentage;

    #region 프로퍼티
    public PassiveActiveCondition _ActiveCond => activeCond;
    public float _ActivateChancePercentage => activateChancePercentage;
    #endregion

    public PassiveSkill(Character owner, PassiveActiveCondition acriveCond) : this(owner, BasePercent, acriveCond) { }
    public PassiveSkill(Character owner, float activePercent, PassiveActiveCondition activeCond) : base(owner) { this.activeCond = activeCond; activateChancePercentage = activePercent; }
    public PassiveSkill(Character owner, float activePercent, PassiveActiveCondition activeCond, float cd, SkillEffects eff, SkillTargetType targetType, object[] values) :
        base(owner, cd, eff, targetType, values)
    {
        this.activeCond = activeCond;
        activateChancePercentage = activePercent;
    }

    public override void ApplyEffectToTargets()
    {
        float chance = UnityEngine.Random.Range(0f, 100f);
        if (chance > _ActivateChancePercentage) return;

        SkillFunction();
    }
    protected virtual void SkillFunction() { }

    public override void ExpireAppliedEffectOfTargets()
    {

    }
    protected virtual void ExpireFunction() { }
}
