public interface ISkill
{
    public void SetTargets(params Character[] characters);

    public void ApplyEffectToTargets();
    public void ExpireAppliedEffectOfTargets();
}
