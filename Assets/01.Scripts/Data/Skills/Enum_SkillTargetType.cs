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

        TARGET,  // ���� �� ��ų�� ���� ����̳� �ǰ� ���, Ȥ�� ������ ���ظ� ���� ������ ������
    }
}