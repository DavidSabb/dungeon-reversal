using UnityEngine;
using System.Collections.Generic;
 
/// <summary>
/// LockOnSystem.cs
/// Dungeon Reversal - Right-click to lock onto the nearest enemy hero.
/// Attach to the Cave Troll root alongside PlayerCombat.
/// </summary>
public class LockOnSystem : MonoBehaviour
{
    [Header("Settings")]
    public float lockOnRange = 20f;
    public LayerMask enemyLayer;
    public string enemyTag = "Hero";
 
    [Header("Visual")]
    public GameObject lockOnIndicatorPrefab; // optional UI indicator shown on target
 
    public bool HasTarget      => _currentTarget != null;
    public Transform CurrentTarget => _currentTarget;
 
    private Transform _currentTarget;
    private GameObject _indicator;
 
    private void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Right click
        {
            if (HasTarget)
                ClearTarget();
            else
                FindTarget();
        }
 
        // If target dies or moves out of range, release
        if (_currentTarget != null)
        {
            if (Vector3.Distance(transform.position, _currentTarget.position) > lockOnRange * 1.5f)
                ClearTarget();
        }
    }
 
    private void FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, lockOnRange, enemyLayer);
        if (hits.Length == 0) return;
 
        Transform best = null;
        float bestDist = Mathf.Infinity;
 
        foreach (Collider col in hits)
        {
            if (!col.CompareTag(enemyTag)) continue;
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = col.transform;
            }
        }
 
        if (best == null) return;
 
        _currentTarget = best;
 
        // Spawn indicator above target
        if (lockOnIndicatorPrefab != null)
        {
            _indicator = Instantiate(lockOnIndicatorPrefab,
                _currentTarget.position + Vector3.up * 2.5f, Quaternion.identity);
            _indicator.transform.SetParent(_currentTarget);
        }
    }
 
    public void ClearTarget()
    {
        _currentTarget = null;
        if (_indicator != null) Destroy(_indicator);
    }
 
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lockOnRange);
    }
}