using UnityEngine;

public class ZombiePerception : MonoBehaviour
{
    [SerializeField] private float detectRadius = 15f;
    [SerializeField] private LayerMask targetMask;

    public void SetDetectRadius(float radius)
    {
        detectRadius = radius;
    }

    public Transform AcquireTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position,
                                                detectRadius,
                                                targetMask,
                                                QueryTriggerInteraction.Ignore);
        if (hits.Length == 0) return null;

        Transform best = null;
        float bestSqr = float.PositiveInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            float sqr = (hits[i].transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = hits[i].transform;
            }
        }

        return best;
    }

    public bool IsTargetInRange(Transform t, float range)
    {
        if (t == null) return false;
        return (t.position - transform.position).sqrMagnitude <= range * range;
    }
}
