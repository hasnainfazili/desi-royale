using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LineOfSight))]
public class FieldOfViewEditor : Editor
{
    void OnSceneGUI()
    {
        LineOfSight fov = (LineOfSight)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position,Vector3.up, Vector3.forward, 360, fov.viewingRadius);
        Vector3 viewingAngleA = fov.DirectionFromAngle(-fov.fieldOfView / 2,  false);
        Vector3 viewingAngleB = fov.DirectionFromAngle(fov.fieldOfView / 2,  false);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewingAngleA * fov.viewingRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewingAngleB * fov.viewingRadius);
    }
}
