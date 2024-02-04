public class PassiveSkill_Skip_Wave : PassiveSkill
{
    protected override string _SpritePath => PathOfResources.SpriteAtlas.GDR_Icons_Bright;
    protected override string _SpriteName => "Skip_Bright";

    public PassiveSkill_Skip_Wave(Archer owner) : base(owner, BasePercent, Enums.PassiveActiveCondition.CLEAR_WAVE, 0f, Enums.SkillEffects.SKIP_WAVE, Enums.SkillTargetType.NONE, new object[] { 0.1f }) { }
    public PassiveSkill_Skip_Wave(Archer owner, float cd) : base(owner, BasePercent, Enums.PassiveActiveCondition.CLEAR_WAVE, cd, Enums.SkillEffects.SKIP_WAVE, Enums.SkillTargetType.NONE, new object[] { 0.1f }) { }
    public PassiveSkill_Skip_Wave(Archer owner, object[] values) : base(owner, BasePercent, Enums.PassiveActiveCondition.CLEAR_WAVE, 0f, Enums.SkillEffects.SKIP_WAVE, Enums.SkillTargetType.NONE, values) { }
    public PassiveSkill_Skip_Wave(Archer owner, float cd, object[] values) : base(owner, BasePercent, Enums.PassiveActiveCondition.CLEAR_WAVE, cd, Enums.SkillEffects.SKIP_WAVE, Enums.SkillTargetType.NONE, values) { }

    protected override void SkillFunction()
    {
        if (!WaveManager.IsDestroying) WaveManager.Instance.SkipWave(2);
    }
}
