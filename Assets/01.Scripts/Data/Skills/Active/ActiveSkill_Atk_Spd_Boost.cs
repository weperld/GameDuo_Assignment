public class ActiveSkill_Atk_Spd_Boost : ActiveSkill
{
    public ActiveSkill_Atk_Spd_Boost(Character owner) : base(owner, 15f, Enums.SkillEffects.ATK_SPD_BOOST, Enums.SkillTargetType.ALL_ALLY, new object[] { 5f, 1f }) { }
    public ActiveSkill_Atk_Spd_Boost(Character owner, float cd) : base(owner, cd, Enums.SkillEffects.ATK_SPD_BOOST, Enums.SkillTargetType.ALL_ALLY, new object[] { 5f, 1f }) { }
    public ActiveSkill_Atk_Spd_Boost(Character owner, object[] values) : base(owner, 15f, Enums.SkillEffects.ATK_SPD_BOOST, Enums.SkillTargetType.ALL_ALLY, values) { }
    public ActiveSkill_Atk_Spd_Boost(Character owner, float cd, object[] values) : base(owner, cd, Enums.SkillEffects.ATK_SPD_BOOST, Enums.SkillTargetType.ALL_ALLY, values) { }

    protected override void SkillFunction()
    {
        if (list_Targets == null || list_Targets.Count == 0) return;

        var buffData = new BuffEffect(Enums.BuffStat.Stat.ATK_SPD_BOOST, (float)_SkillValues[0], (float)_SkillValues[1]);
        foreach (var target in list_Targets)
            target.ApplyBuffEffects(buffData);
    }
}
