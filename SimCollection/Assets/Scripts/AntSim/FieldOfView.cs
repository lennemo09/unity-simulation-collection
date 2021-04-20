using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius;

    [Range(0,360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();

    public void Start()
    {
        viewRadius = 10.0f;
        viewAngle = 90.0f;

        StartCoroutine("GetsTargetsWithDelay", 0.2f);
    }

    // Scan for targets in FOV every `delay` interval
    IEnumerator GetsTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            GetTargetsInRadius();
        }
    }

    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        // Handles local angle and converts to global angle
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    void GetTargetsInRadius()
    {
        // Wipes targets array for every search
        visibleTargets.Clear();

        // Gets all target in the 'target' layer within the radius
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        // For each target
        for (int i = 0; i < targetsInRadius.Length; i++)
        {
            // Get target's Transform
            Transform target = targetsInRadius[i].transform;
            // Get direction to target
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            // If target within radius and our FOV angle
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle/2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                // Raycast from position in directionToTarget for distanceToTarget, if didn't hit an object in 'obstacle' layer:
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }
}
