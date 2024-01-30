using System;
using UnityEngine;
using Enums;

[Serializable]
public class PassiveSkill : Skill
{
    [SerializeField] protected PassiveActiveCondition activeCond;
    [SerializeField, Range(0f, 100f)] protected float activateChancePercentage;

    #region 프로퍼티
    public PassiveActiveCondition _ActiveCond => activeCond;
    public float _ActivateChancePercentage => activateChancePercentage;
    #endregion

    public PassiveSkill(Character owner) : base(owner) { }
    public PassiveSkill(Character owner, float cd, SkillEffects eff, SkillTargetType targetType, object[] values) : base(owner, cd, eff, targetType, values) { }

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
