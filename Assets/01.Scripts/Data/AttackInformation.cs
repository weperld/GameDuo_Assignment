using UnityEngine;

public class AttackInformation
{
    /// <summary>
    /// ���� ������
    /// </summary>
    public Character attacker;
    /// <summary>
    /// ���� �����
    /// </summary>
    public Character target;
    /// <summary>
    /// �⺻ �����̶�� true, ��ų�� ���� �����̶�� false
    /// </summary>
    public Enums.AttackType attackType;
    public float dmg;
    /// <summary>
    /// ��ų�� ���� ������ �� ������ ��ų�� ����
    /// </summary>
    public Skill skillData;
    public Vector2 dmgFontOffset;
    public Transform tf_DmgPos;

    public AttackInformation(Character attacker, Character target, Enums.AttackType attackType, float dmg, Skill skillData = null)
    {
        this.attacker = attacker;
        this.target = target;
        this.attackType = attackType;
        this.dmg = dmg;
        this.skillData = skillData;
        dmgFontOffset = Vector2.zero;
        tf_DmgPos = target.transform;
    }

    public AttackInformation(Character attacker, Character target, Enums.AttackType attackType, float dmg, Vector2 dmgFontOffset, Skill skillData = null) :
        this(attacker, target, attackType, dmg, skillData)
    {
        this.dmgFontOffset = dmgFontOffset;
    }

    public void SetDmgPosTransform(Transform tf_DmgPos)
    {
        this.tf_DmgPos = tf_DmgPos;
    }
}
