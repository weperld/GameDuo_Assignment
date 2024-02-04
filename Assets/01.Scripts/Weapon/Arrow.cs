using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Transforms")]
    [SerializeField] private Transform tf_HeightAdjuster;
    [SerializeField] private Transform tf_RotateAdjuster;

    [Header("Animation Curves")]
    [SerializeField] private AnimationCurve heightCurve;
    [SerializeField] private AnimationCurve zAxisRotateCurve;

    [Header("Variables")]
    [SerializeField] private float speed;
    [SerializeField] private float heightCurveCoefficient;

    private AttackInformation attackInfo;
    private Vector3 firePos;

    public void SetAttackInformationAndFire(Vector3 genPos, AttackInformation attackInformation)
    {
        attackInfo = attackInformation;
        transform.position = firePos = genPos;
        if (tf_HeightAdjuster != null) tf_HeightAdjuster.position = Vector3.zero;
        if (tf_RotateAdjuster != null) tf_RotateAdjuster.rotation = Quaternion.Euler(0f, (attackInfo.target.transform.position - firePos).x >= 0f ? 0f : 180f, 0f);
        attackInfo.SetDmgPosTransform(attackInfo.target.FindClosestDmgPosition(genPos));
        StartCoroutine(Fire());
    }

    private IEnumerator Fire()
    {
        while (!IsArrivedAtTarget())
        {
            yield return null;

            var currentPos = transform.position;
            if (speed < 0f) speed *= -1f;
            var nextPos = Vector3.MoveTowards(currentPos, attackInfo.tf_DmgPos.position, speed * Time.deltaTime);
            transform.position = nextPos;

            float totalDist = (firePos - attackInfo.tf_DmgPos.position).magnitude;
            float fireDist = (firePos - currentPos).magnitude;
            float fireDistRate = fireDist / totalDist;

            if (tf_HeightAdjuster != null)
                tf_HeightAdjuster.localPosition = new Vector3(0f, heightCurveCoefficient * heightCurve.Evaluate(fireDistRate), 0f);
            if (tf_RotateAdjuster != null)
                tf_RotateAdjuster.localRotation = Quaternion.Euler(0f, tf_RotateAdjuster.localRotation.eulerAngles.y, zAxisRotateCurve.Evaluate(fireDistRate));
        }

        attackInfo.target.Damage(attackInfo);

        if (SpawnManager.IsDestroying) yield break;
        SpawnManager.Instance.Despawn(PathOfResources.Prefabs.Arrow, gameObject);
    }
    private bool IsArrivedAtTarget(float margin = 0.1f)
    {
        var dist = (attackInfo.tf_DmgPos.position - transform.position).magnitude;
        if (margin < 0f) margin *= -1f;
        return dist <= margin;
    }
}
