namespace Enums
{
    public enum PassiveActiveCondition
    {
        BASIC_ATK,      // 기본 공격 시전 시
        ACTIVATE_SKILL, // 스킬 발동 시
        HIT,            // 공격 적중 시
        DAMAGE,         // 피해를 입을 시
        DEATH,          // 사망 시
        KILL,           // 적을 죽일 경우
        CLEAR_WAVE,     // 웨이브 클리어 시
    }
}