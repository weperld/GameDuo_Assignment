public class PassiveSkill_Skip_Wave : PassiveSkill
{
    public PassiveSkill_Skip_Wave(Character owner) : base(owner, BasePercent, Enums.PassiveActiveCondition.CLEAR_WAVE, 0f, Enums.SkillEffects.SKIP_WAVE, Enums.SkillTargetType.NONE, new object[] { 0.1f }) { }
    public PassiveSkill_Skip_Wave(Character owner, float cd) : base(owner, BasePercent, Enums.PassiveActiveCondition.CLEAR_WAVE, cd, Enums.SkillEffects.SKIP_WAVE, Enums.SkillTargetType.NONE, new object[] { 0.1f }) { }
    public PassiveSkill_Skip_Wave(Character owner, object[] values) : base(owner, BasePercent, Enums.PassiveActiveCondition.CLEAR_WAVE, 0f, Enums.SkillEffects.SKIP_WAVE, Enums.SkillTargetType.NONE, values) { }
    public PassiveSkill_Skip_Wave(Character owner, float cd, object[] values) : base(owner, BasePercent, Enums.PassiveActiveCondition.CLEAR_WAVE, cd, Enums.SkillEffects.SKIP_WAVE, Enums.SkillTargetType.NONE, values) { }

    protected override void SkillFunction()
    {
        if (!WaveManager.IsDestroying) WaveManager.Instance.SkipWave(2);
    }
}
