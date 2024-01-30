using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Enums;

public abstract class Character : MonoBehaviour
{
    #region 기본 능력치 변수
    [Header("Stats")]
    [SerializeField] protected float power = 1f;
    [SerializeField] protected float baseAttackSpeed = 1f;
    [SerializeField] protected float baseMaxHP = 100f;

    protected float attackTimer = 0f;
    protected float currentHP;

    /// <summary>
    /// 공격력
    /// </summary>
    public virtual float _Power
    {
        get
        {
            var ret = power;
            if (dict_AppliedBuffEffects.TryGetValue(BuffStat.Stat.ATK_BOOST, out var v) && !v._IsExpired) ret += v.appliedEffect._BoostValue;
            return ret;
        }
    }
    /// <summary>
    /// 초당 공격 횟수
    /// </summary>
    public virtual float _AttackSpeed
    {
        get
        {
            var ret = baseAttackSpeed;
            if (dict_AppliedBuffEffects.TryGetValue(BuffStat.Stat.ATK_SPD_BOOST, out var v) && !v._IsExpired) ret += v.appliedEffect._BoostValue;
            return ret;
        }
    }
    /// <summary>
    /// 체력
    /// </summary>
    public virtual float _MaxHP => baseMaxHP;

    #endregion

    #region 버프 관련 변수
    protected class AppliedBuffEffect
    {
        public BuffEffect appliedEffect = null;
        public float appliedDuration;
        public bool _IsExpired => appliedEffect == null || appliedEffect._Duration <= appliedDuration;

        private Action<AppliedBuffEffect> onExpire;

        public AppliedBuffEffect(BuffEffect effect, Action<AppliedBuffEffect> onExpire)
        {
            appliedEffect = effect;
            appliedDuration = 0f;
            this.onExpire = onExpire;
        }

        public void ExpireBuff()
        {
            if (onExpire != null) onExpire(this);
        }
    }

    protected Dictionary<BuffStat.Stat, AppliedBuffEffect> dict_AppliedBuffEffects = new Dictionary<BuffStat.Stat, AppliedBuffEffect>();
    private IEnumerator expiredBuffCoroutine = null;
    #endregion

    public bool _IsDead => currentHP <= 0f;
    protected virtual bool _IsAttackable => !_IsDead && attackTimer <= 0f;

    private void OnEnable()
    {
        currentHP = _MaxHP;
    }

    private void Update()
    {
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;
    }

    /// <summary>
    /// 기본 공격 시전
    /// </summary>
    protected void Attack(Character targetCharacter)
    {
        if (!_IsAttackable) return;
        ActionOnAttack(targetCharacter);
        Debug.Log($"{gameObject.name}({gameObject.GetInstanceID()}) ATTACK!!");
    }
    protected abstract void ActionOnAttack(Character targetMonster);

    /// <summary>
    /// 나에게 공격이 닿았을 경우 공격을 시전한 캐릭터에게 알리기 위한 함수
    /// </summary>
    /// <param name="killed"></param>
    /// <param name="attackCharacter"></param>
    public void NoticeToAttackCharacterWhenHit(Character attackCharacter)
    {
        if (attackCharacter == null) return;
        attackCharacter.Hit(this);
    }
    /// <summary>
    /// 공격 적중
    /// </summary>
    /// <param name="damagedCharacter"></param>

    public void Hit(Character damagedCharacter)
    {
        ActionOnHit(_Power, damagedCharacter);
        Debug.Log($"{gameObject.name}({gameObject.GetInstanceID()}) Hit!!");
    }
    protected abstract void ActionOnHit(float dmg, Character damagedCharacter);

    /// <summary>
    /// 캐릭터가 피해를 입을 경우
    /// </summary>
    /// <param name="dmg"></param>
    /// <param name="attackCharacter"></param>
    public void Damage(float dmg, Character attackCharacter)
    {
        currentHP = Mathf.Max(0f, currentHP - dmg);
        DamageEffect(dmg);
        if (_IsDead) { Death(); NoticeDeathToAttackCharacter(attackCharacter); }
        else ActionOnDamage(dmg, attackCharacter);
    }
    private void DamageEffect(float dmg)
    {
        UIManager.Instance.SpawnDamageEffectUI(dmg, transform.position);
    }
    protected abstract void ActionOnDamage(float dmg, Character attackCharacter);

    /// <summary>
    /// 캐릭터 사망
    /// </summary>
    protected void Death()
    {
        ActionOnDeath();
        Debug.Log($"{gameObject.name}({gameObject.GetInstanceID()}) DEAD..");
    }
    protected abstract void ActionOnDeath();
    /// <summary>
    /// 이 개체를 죽인 캐릭터에게 죽였다는 정보 알림
    /// </summary>
    /// <param name="attackCharacter"></param>
    protected void NoticeDeathToAttackCharacter(Character attackCharacter)
    {
        if (attackCharacter == null) return;

        attackCharacter.NoticedDeathFromTarget(this);
    }
    /// <summary>
    /// 이 개체의 공격으로 대상 캐릭터가 죽으면 받는 알림
    /// </summary>
    /// <param name="targetCharacter"></param>
    public abstract void NoticedDeathFromTarget(Character targetCharacter);

    /// <summary>
    /// 버프 효과 적용
    /// 같은 종류의 효과가 이미 적용되었다면 효과 갱신
    /// </summary>
    /// <param name="buffEffects"></param>
    public virtual void ApplyBuffEffects(params BuffEffect[] buffEffects)
    {
        if (buffEffects == null || buffEffects.Length == 0) return;

        foreach (var buffEffect in buffEffects)
        {
            if (buffEffect == null) continue;

            var key = buffEffect._BuffStat;
            var appliedEffectInfo = new AppliedBuffEffect(buffEffect, b =>
            {
                // 버프 시간 만료 시 작동 코드 작성(ex: 비주얼 효과 제거)
                if (dict_AppliedBuffEffects[key] == b) dict_AppliedBuffEffects[key] = null;
            });
            if (dict_AppliedBuffEffects.ContainsKey(key)) dict_AppliedBuffEffects[key] = appliedEffectInfo;
            else dict_AppliedBuffEffects.Add(key, appliedEffectInfo);
        }

        if (expiredBuffCoroutine != null) return;
        expiredBuffCoroutine = RemoveExpiredBuffs();
        StartCoroutine(expiredBuffCoroutine);
    }
    private IEnumerator RemoveExpiredBuffs()
    {
        int applyingEffCount;
        do
        {
            yield return null;

            applyingEffCount = 0;
            foreach (var kvp in dict_AppliedBuffEffects)
            {
                kvp.Value.appliedDuration += Time.deltaTime;
                if (kvp.Value.appliedDuration < kvp.Value.appliedEffect._Duration) applyingEffCount++;
                else dict_AppliedBuffEffects[kvp.Key].ExpireBuff();
            }
        } while (applyingEffCount > 0);

        expiredBuffCoroutine = null;
    }

    public static Character[] GetTargetsByTargetType(SkillTargetType targetType, Character self)
    {
        var all_Ally = GameManager.Instance.GetActivatedCharacterSet();
        var all_Ally_Except_This = all_Ally.Except(new Character[] { self }).ToArray();
        var all_Enemy = WaveManager.Instance.GetCurrentActiveMonsters();

        int randVal = UnityEngine.Random.Range(0, 10000);

        return targetType switch
        {
            SkillTargetType.SELF => new Character[] { self },
            SkillTargetType.ALL => all_Ally.Concat(all_Enemy).ToArray(),
            SkillTargetType.ALL_EXCEPT_SELF => all_Ally.Concat(all_Enemy).Except(new Character[] { self }).ToArray(),
            SkillTargetType.ALL_ALLY => all_Ally,
            SkillTargetType.ALL_ALLY_EXCEPT_SELF => all_Ally_Except_This,
            SkillTargetType.ONE_ALLY => all_Ally.Where(w => randVal % all_Ally.Length == Array.IndexOf(all_Ally, w)).ToArray(),
            SkillTargetType.ONE_ALLY_EXCEPT_SELF => all_Ally_Except_This.Where(w => randVal % all_Ally_Except_This.Length == Array.IndexOf(all_Ally_Except_This, w)).ToArray(),
            SkillTargetType.ALL_ENEMY => all_Enemy,
            SkillTargetType.ONE_ENEMY => all_Enemy.Where(w => randVal % all_Enemy.Length == Array.IndexOf(all_Enemy, w)).ToArray(),
            _ => null,
        };
    }
}
