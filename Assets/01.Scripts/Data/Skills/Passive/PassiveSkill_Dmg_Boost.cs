using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveSkill_Dmg_Boost : PassiveSkill
{
    public PassiveSkill_Dmg_Boost(Character owner) : base(owner, 0f, Enums.SkillEffects.DMG_BOOST, Enums.SkillTargetType.TARGET, new object[] { 100f }) { }
    public PassiveSkill_Dmg_Boost(Character owner, float dmgMultValue) : base(owner, 0f, Enums.SkillEffects.DMG_BOOST, Enums.SkillTargetType.TARGET, new object[] { dmgMultValue }) { }
    public PassiveSkill_Dmg_Boost(Character owner, float cd, float dmgMultValue) : base(owner, cd, Enums.SkillEffects.DMG_BOOST, Enums.SkillTargetType.TARGET, new object[] { dmgMultValue }) { }

    protected override void SkillFunction()
    {
        if (list_Targets == null || list_Targets.Count == 0) return;

        float boostedDmg = (float)_SkillValues[0] * (float)customCoefficients[0];
        foreach (var target in list_Targets)
        {
            target.Damage(boostedDmg, _SkillOwner);
        }
    }
}
