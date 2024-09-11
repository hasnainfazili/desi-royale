using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineOfSight : MonoBehaviour
{
    //TODO
    //Shift this code to patrol + hostile/aware states when implementing state machine 
    //The Range of the enemy actors viewing range
    public float _viewingRadius;
    public float _meshResolution;
    //The Angle or the FoV of the enemy actor
    [Range(0,360)]
    public float _fieldOfView;

    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask targetMask;
    public List<Transform> InViewingRange = new List<Transform>();

    void Update()
    {
        FindVisibleTargets();
    }

    private void FindVisibleTargets()
    {
        InViewingRange.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, _viewingRadius, targetMask);

        for(int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform; 
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if(Vector3.Angle(transform.forward, directionToTarget) < _fieldOfView / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if(!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    InViewingRange.Add(target);
                    Debug.Log(target.name);
                }
            }

        }
    }
    private void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(_fieldOfView * _meshResolution);
        float stepAngleSize = _fieldOfView / stepCount;

        for(int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - _fieldOfView / 2 + stepAngleSize * i;
        }
    }
    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if(!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
    //Two  Raycasts, get the dot product and check if the object is within the range.

}
