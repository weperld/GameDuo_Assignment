using System;
using Enums;

[Serializable]
public class ActiveSkill : Skill
{
    public ActiveSkill(Character owner) : base(owner) { }
    public ActiveSkill(Character owner, float cd, SkillEffects eff, SkillTargetType targetType, object[] values) : base(owner, cd, eff, targetType, values) { }

    public override void ApplyEffectToTargets()
    {
        SkillFunction();
    }
    protected virtual void SkillFunction() { }

    public override void ExpireAppliedEffectOfTargets()
    {

    }
}
