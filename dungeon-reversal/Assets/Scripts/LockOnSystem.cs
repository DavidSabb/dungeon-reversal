using UnityEngine;

public class LockOnSystem : MonoBehaviour
{
    public float lockOnRange = 20f;
    public LayerMask enemyLayer;
    public string enemyTag = "Hero";
    public GameObject lockOnIndicatorPrefab;

    public bool HasTarget => currentTarget != null;
    public Transform CurrentTarget => currentTarget;

    Transform currentTarget;
    GameObject indicator;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (HasTarget) ClearTarget();
            else FindTarget();
        }

        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) > lockOnRange * 1.5f)
            ClearTarget();
    }

    void FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, lockOnRange, enemyLayer);
        Transform best = null;
        float bestDist = Mathf.Infinity;

        foreach (Collider col in hits)
        {
            if (!col.CompareTag(enemyTag)) continue;
            float d = Vector3.Distance(transform.position, col.transform.position);
            if (d < bestDist) { bestDist = d; best = col.transform; }
        }

        if (best == null) return;
        currentTarget = best;

        if (lockOnIndicatorPrefab != null)
        {
            indicator = Instantiate(lockOnIndicatorPrefab, currentTarget.position + Vector3.up * 2.5f, Quaternion.identity);
            indicator.transform.SetParent(currentTarget);
        }
    }

    public void ClearTarget()
    {
        currentTarget = null;
        if (indicator != null) Destroy(indicator);
    }
}
