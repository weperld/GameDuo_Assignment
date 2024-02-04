using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barricade : Character
{
    protected override void OnEnable()
    {
        base.OnEnable();
        var sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in sprites)
        {
            var color = sprite.color;
            color.a = 1f;
            sprite.color = color;
        }
    }

    protected override void ActionOnDamage(AttackInformation attackInfo)
    {
        
    }

    protected override void ActionOnDeath(AttackInformation attackInfo)
    {
        StartCoroutine(BarricadeDestroyAnimation());
    }

    private IEnumerator BarricadeDestroyAnimation()
    {
        float animationTime = 1f;
        var sprites = GetComponentsInChildren<SpriteRenderer>();

        while (animationTime > 0f)
        {
            yield return null;
            animationTime -= Time.deltaTime;

            foreach (var sprite in sprites)
            {
                var color = sprite.color;
                color.a = animationTime;
                sprite.color = color;
            }
        }

        OnEndDeathAnimation();
    }


    #region Unuse
    public override void BeNoticedDeathOfTarget(Character targetCharacter) { }
    public override void AttackOnHitFrameOfAnimation() { }
    protected override void ActionOnBasicAttack(AttackInformation atkInfo) { }
    protected override void ActionOnHit(AttackInformation attackInfo) { }
    #endregion
}
