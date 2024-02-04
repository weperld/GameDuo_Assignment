using System;
using Enums;

[Serializable]
public class ActiveSkill : Skill
{
    public ActiveSkill(Archer owner) : base(owner) { }
    public ActiveSkill(Archer owner, float cd, SkillEffects eff, SkillTargetType targetType, object[] values) : base(owner, cd, eff, targetType, values) { }

    protected override void ApplyEffectToTargets()
    {
        SkillFunction();
    }
    protected virtual void SkillFunction() { }

    protected override void ExpireAppliedEffectOfTargets()
    {

    }
}
