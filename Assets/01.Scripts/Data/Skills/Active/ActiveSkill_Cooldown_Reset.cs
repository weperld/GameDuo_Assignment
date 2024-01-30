using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveSkill_Cooldown_Reset : ActiveSkill
{
    public ActiveSkill_Cooldown_Reset(Character owner) : base(owner, 30f, SkillEffects.CD_RESET, SkillTargetType.ALL_ALLY_EXCEPT_SELF, null) { }
    public ActiveSkill_Cooldown_Reset(Character owner, float cd) : base(owner, cd, SkillEffects.CD_RESET, SkillTargetType.ALL_ALLY_EXCEPT_SELF, null) { }
    public ActiveSkill_Cooldown_Reset(Character owner, float cd, SkillTargetType targetType) : base(owner, cd, SkillEffects.CD_RESET, targetType, null) { }

    protected override void SkillFunction()
    {
        if (list_Targets == null || list_Targets.Count == 0) return;

        foreach (var target in list_Targets)
        {
            if (target is Archer archer)
                archer.ReductActiveSkillCooldown(false, 1f);
        }
    }
}
