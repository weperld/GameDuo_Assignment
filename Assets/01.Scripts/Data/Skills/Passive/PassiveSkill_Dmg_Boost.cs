using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveSkill_Dmg_Boost : PassiveSkill
{
    protected override string _SpritePath => PathOfResources.SpriteAtlas.GDR_Icons_Bright;
    protected override string _SpriteName => "Bow_Bright";

    public PassiveSkill_Dmg_Boost(Archer owner) : base(owner, 10f, Enums.PassiveActiveCondition.HIT_BASIC_ATK, 0f, Enums.SkillEffects.DMG_BOOST, Enums.SkillTargetType.TARGET, new object[] { 100f }) { }
    public PassiveSkill_Dmg_Boost(Archer owner, float dmgMultValue) : base(owner, 10f, Enums.PassiveActiveCondition.HIT_BASIC_ATK, 0f, Enums.SkillEffects.DMG_BOOST, Enums.SkillTargetType.TARGET, new object[] { dmgMultValue }) { }
    public PassiveSkill_Dmg_Boost(Archer owner, float cd, float dmgMultValue) : base(owner, 10f, Enums.PassiveActiveCondition.HIT_BASIC_ATK, cd, Enums.SkillEffects.DMG_BOOST, Enums.SkillTargetType.TARGET, new object[] { dmgMultValue }) { }

    protected override void SkillFunction()
    {
        if (list_Targets == null || list_Targets.Count == 0) return;

        var triggerAttackInfo = (AttackInformation)customCoefficients[0];
        float boostedDmg = (float)_SkillValues[0] * triggerAttackInfo.dmg;
        foreach (var target in list_Targets)
        {
            var atkInfo = new AttackInformation(_SkillOwner, target, Enums.AttackType.SKILL, boostedDmg, Vector2.up * 30f, this);
            atkInfo.SetDmgPosTransform(triggerAttackInfo.tf_DmgPos);
            target.Damage(atkInfo);
        }
    }
}
