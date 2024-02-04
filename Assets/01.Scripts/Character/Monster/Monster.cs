using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Character
{
    private AttackInformation basicAttackInfo;

    public override void AttackOnHitFrameOfAnimation()
    {
        basicAttackInfo.target.Damage(basicAttackInfo);
    }

    public override void BeNoticedDeathOfTarget(Character targetCharacter)
    {
        
    }

    protected override void ActionOnBasicAttack(AttackInformation atkInfo)
    {
        basicAttackInfo = atkInfo;
        basicAttackInfo.SetDmgPosTransform(basicAttackInfo.target.FindClosestDmgPosition(transform.position));
    }

    protected override void ActionOnDamage(AttackInformation attackInfo)
    {
        
    }

    protected override void ActionOnDeath(AttackInformation attackInfo)
    {
        
    }

    protected override void ActionOnHit(AttackInformation attackInfo)
    {
        
    }
}
