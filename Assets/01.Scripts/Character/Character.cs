using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    #region �⺻ �ɷ�ġ ����
    [Header("Stats")]
    [SerializeField] protected float power = 1f;
    [SerializeField] protected float baseAttackSpeed = 1f;
    [SerializeField] protected float baseHP = 100f;

    protected float attackTimer = 0f;

    /// <summary>
    /// ���ݷ�
    /// </summary>
    public virtual float _Power => power;
    /// <summary>
    /// �ʴ� ���� Ƚ��
    /// </summary>
    public virtual float _AttackSpeed => baseAttackSpeed;
    /// <summary>
    /// ü��
    /// </summary>
    public virtual float _HP => baseHP;
    #endregion

    protected bool _IsAttackable => attackTimer <= 0f;



    private void Update()
    {
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;
    }

    protected void Attack()
    {
        if (!_IsAttackable) return;
        ActionOnAttack();
    }
    protected abstract void ActionOnAttack();

    protected void Death()
    {

    }
}
