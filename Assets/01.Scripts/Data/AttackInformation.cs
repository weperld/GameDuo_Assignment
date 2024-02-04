using UnityEngine;

public class AttackInformation
{
    /// <summary>
    /// 공격 시전자
    /// </summary>
    public Character attacker;
    /// <summary>
    /// 공격 대상자
    /// </summary>
    public Character target;
    /// <summary>
    /// 기본 공격이라면 true, 스킬에 의한 공격이라면 false
    /// </summary>
    public Enums.AttackType attackType;
    public float dmg;
    /// <summary>
    /// 스킬에 의한 공격일 때 시전한 스킬의 정보
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
