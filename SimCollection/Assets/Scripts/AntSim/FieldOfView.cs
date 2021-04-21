using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius;

    [Range(0,360)]
    public float viewAngle;
    public float getTargetsDelay;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();

    public float fovResolution; // Number of rays to cast per degree
    public MeshFilter viewMeshFilter;
    private Mesh viewMesh;

    public int edgeResolveIterations;
    public float edgeDistanceThreshold;

    public float turnToTargetSpeed;

    public void Start()
    {
        viewRadius = 10.0f;
        viewAngle = 90.0f;
        getTargetsDelay = 0.2f;
        turnToTargetSpeed = 3.5f;

        edgeDistanceThreshold = 0.5f;

        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        StartCoroutine("GetsTargetsWithDelay", getTargetsDelay);
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

    private void LateUpdate()
    {
        // Using LateUpdate so that the FieldOfView is only drawn after character has finished rotating
        // to avoid jittery FOV.
        DrawFieldOfView();
        if (visibleTargets.Count > 0)
        {
            turnsToward(visibleTargets[0]);
        }
    }

    private void DrawFieldOfView()
    {
        int rayCount = Mathf.RoundToInt(viewAngle * fovResolution);
        float rayAngleSize = viewAngle / rayCount;

        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= rayCount; i++)
        {
            // angle = current angle from left FOV border + current ray angle
            float angle = transform.eulerAngles.y - viewAngle/2 + rayAngleSize*i;

            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                // Threshold to determine if oldViewCast and newViewCast has hit the same object or not
                bool edgeDistanceThresholdExceeded = Mathf.Abs(oldViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;

                // Checks if oldViewCast hits and obstacle and the new one didn't
                // or if the old one didn't and the new one did
                // or if both hit something but different objects
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistanceThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount-2)*3]; // Indices of vertices to form a triangle in vertices array

        // ViewMesh is child of character object -> position of the vertices will be local -> (0,0,0) starts from root of character
        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++) // vertexCount - 1 because we already set vertex[0]
        {
            // Translates global point to local point
            Vector3 localPoint = transform.InverseTransformPoint(viewPoints[i]); 
            vertices[i + 1] = localPoint;

            if (i < vertexCount - 2)
            {
                // triangles stores the index of the vertex in the vertices array
                triangles[i * 3] = 0; // First vertex
                triangles[i * 3 + 1] = i + 1; // Second vertex
                triangles[i * 3 + 2] = i + 2; // First vertex
            }            
        }

        // Draws the triangles using viewMesh
        viewMesh.Clear(); // Resets viewMesh
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDistanceThresholdExceeded = Mathf.Abs(minViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDistanceThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
    }

    private ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 direction = DirectionFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, direction, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + direction * viewRadius, viewRadius, globalAngle);
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

    private void GetTargetsInRadius()
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

    private void turnsToward(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * turnToTargetSpeed);
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _distance, float _angle)
        {
            hit = _hit;
            point = _point;
            distance = _distance;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}
