namespace Enums
{
    public enum SkillTargetType
    {
        NONE,
        SELF,

        ALL,
        ALL_EXCEPT_SELF,

        ALL_ALLY,
        ALL_ALLY_EXCEPT_SELF,

        ONE_ALLY,   // Randomly
        ONE_ALLY_EXCEPT_SELF,   // Randomly

        ALL_ENEMY,
        ONE_ENEMY,  // Randomly

        TARGET,  // 공격 및 스킬의 시전 대상이나 피격 대상, 혹은 나에게 피해를 입힌 공격의 시전자
    }
}