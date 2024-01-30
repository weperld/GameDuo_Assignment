public class PassiveSkill_Skip_Wave : PassiveSkill
{
    public PassiveSkill_Skip_Wave(Character owner) : base(owner, 0f, Enums.SkillEffects.SKIP_WAVE, Enums.SkillTargetType.NONE, new object[] { 0.1f }) { }
    public PassiveSkill_Skip_Wave(Character owner, float cd) : base(owner, cd, Enums.SkillEffects.SKIP_WAVE, Enums.SkillTargetType.NONE, new object[] { 0.1f }) { }
    public PassiveSkill_Skip_Wave(Character owner, object[] values) : base(owner, 0f, Enums.SkillEffects.SKIP_WAVE, Enums.SkillTargetType.NONE, values) { }
    public PassiveSkill_Skip_Wave(Character owner, float cd, object[] values) : base(owner, cd, Enums.SkillEffects.SKIP_WAVE, Enums.SkillTargetType.NONE, values) { }

    protected override void SkillFunction()
    {
        WaveManager.Instance.SkipWave(2);
    }
}
