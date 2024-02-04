namespace Enums
{
    public enum CharacterActionState
    {
        WAIT,           // ��� ���(OrderOnEndCharacterAction ���� ���� ���� �Է� �ð��� ������ ����)
        IDLE,           // �⺻ ����
        WALK,           // �̵�
        ATTACK,         // �⺻ ����
        SKILL,          // ��ų ���(��ų ��� �ִϸ��̼��� ���� ��� ����)
        DAMAGE,         // �ǰ�(����μ��� �ǰ� ��� X)
        DEATH,          // ���
    }

    public enum OrderOnEndCharacterAction
    {
        REMOVE_ACTION,
        REPEAT,
    }
}