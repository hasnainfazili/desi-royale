using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class LineOfSight : MonoBehaviour
{
    [FormerlySerializedAs("_viewingRadius")] public float viewingRadius;
    [FormerlySerializedAs("_meshResolution")] public float meshResolution;
    [FormerlySerializedAs("_fieldOfView")] [Range(0,360)]
    public float fieldOfView;
    [FormerlySerializedAs("inViewingRange")] [FormerlySerializedAs("InViewingRange")] [HideInInspector] public List<Transform> playersInView = new List<Transform>();
    Actor _actor;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask targetMask;

    private void Awake()
    {
        _actor = GetComponent<Actor>();
    }
    void Update()
    {
        FindVisibleTargets();
    }

    public void FindVisibleTargets()
    {
        playersInView.Clear();
        var results = Physics.OverlapSphere(transform.position, viewingRadius, targetMask);
        for (int i = 0; i < results.Length; i++)
        {
            Transform target = results[i].transform;
            Vector3 direction = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, direction) <= fieldOfView / 2)
            {   
                
                var distance = Vector3.Distance(transform.position, target.position);
                if(!Physics.Raycast(transform.position, direction, distance, obstacleMask))
                        playersInView.Add(target);
            }
        }
    }

    public IEnumerator PlayerIsVisible()
    {
         if (playersInView.Count <= 0) yield break;
        while (!playersInView[0].CompareTag("Player"))
        {
            _actor.playerInFOV = false;
            yield return null;
        }

        yield return new WaitUntil(() => playersInView[0].CompareTag("Player"));
        _actor.playerInFOV = true;
    }
    private void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(fieldOfView * meshResolution);
        float stepAngleSize = fieldOfView / stepCount;

        for(int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - fieldOfView / 2 + stepAngleSize * i;
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

}
