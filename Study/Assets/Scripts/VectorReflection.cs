using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VectorReflection : MonoBehaviour
{
    public Transform mirror;
    private void OnDrawGizmos()
    {
        Vector3 objPos = transform.position;
        Vector3 lookDir = transform.forward;
        if(Physics.Raycast(objPos, lookDir, out RaycastHit hitObj, 50.0f, LayerMask.GetMask("Mirror")))
        {
            Vector3 hitNormal = hitObj.normal;
            Vector3 hitPos = hitObj.point;
            Handles.color = Color.green;
            //Draw the ray on hit
            Vector3 objToHit = hitPos - objPos;
            Handles.DrawAAPolyLine(objPos, hitPos );
            //Find the reflection
            //1. find the projection of the ray direction vector onto the normal of the hitPoint
            Vector3 projOnN = Vector3.Dot(objToHit, hitNormal) * hitNormal;
            //2. Subtract two of the projection vector from the ray vector to get to the other side of the original object
            //bassed on the hitNormal to get the reflection vector
            Vector3 reflectionVector = objToHit - (2 * projOnN);
            Handles.color = Color.red;
            Handles.DrawAAPolyLine(objPos, objPos + reflectionVector);
            //3. add the pos of the hit to displace the vector onto the hitPos
            Vector3 reflectAtHit = reflectionVector + hitPos;
            Handles.color = Color.cyan;
            Handles.DrawAAPolyLine(hitPos, reflectAtHit);
            //TADA
        }
        else
        {
            Handles.color = Color.red;
            //Draw the look vector (scaled by 10) on miss
            Handles.DrawAAPolyLine(objPos, objPos + lookDir * 10);
        }
    }
}
