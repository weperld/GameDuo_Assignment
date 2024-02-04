public interface ISkill
{
    public void SetTargets(params Character[] characters);
    public void SetCustomCoefficients(params object[] values);
    public void UseSkill();
}
