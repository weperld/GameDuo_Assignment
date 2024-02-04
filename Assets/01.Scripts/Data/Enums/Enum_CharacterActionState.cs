namespace Enums
{
    public enum CharacterActionState
    {
        WAIT,           // 명령 대기(OrderOnEndCharacterAction 값에 관계 없이 입력 시간이 지나면 제거)
        IDLE,           // 기본 상태
        WALK,           // 이동
        ATTACK,         // 기본 공격
        SKILL,          // 스킬 사용(스킬 사용 애니메이션이 없어 사용 보류)
        DAMAGE,         // 피격(현재로서는 피격 사용 X)
        DEATH,          // 사망
    }

    public enum OrderOnEndCharacterAction
    {
        REMOVE_ACTION,
        REPEAT,
    }
}