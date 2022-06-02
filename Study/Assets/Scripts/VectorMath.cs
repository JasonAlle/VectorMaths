using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

public class VectorMath : MonoBehaviour
{
    public Transform terrain;
    public float objToTerrainDotLookDir;
    public float angleBetweenVectors;
    public float builtInAnglesBetweenVectors;
    public float signedAngleBetween;
    public float determinant;
    public float rightMag;
    public float FOVLimit;
    public Vector3 projABOnN;
    public float dotABdN;
    void OnDrawGizmos() {
        Vector3 objPos = transform.position;
        Vector3 lookDir = transform.forward;

        //Vector From Point A to B
        Vector3 objToTerrain = (terrain.position - objPos);
        Gizmos.color = Color.yellow;
        //Testing dot Product on normal vector
        dotABdN = Vector3.Dot(objToTerrain, Vector3.up);
        //Testing Projection from dotproduct onto normal vector
        projABOnN = (dotABdN) * Vector3.up;
        ///Gizmos.DrawLine(objPos, terrain.position);
        objToTerrainDotLookDir = Vector3.Dot(objToTerrain, lookDir);
        //Angle between vectors
        angleBetweenVectors = Mathf.Acos(objToTerrainDotLookDir);
        angleBetweenVectors = angleBetweenVectors * Mathf.Rad2Deg;
        builtInAnglesBetweenVectors = Vector3.Angle(objToTerrain, lookDir);
        //If the Angle between the lookdir and the Terrain- objPos vector is less than the limit provided we
        //are within line of sight to the target
        if (builtInAnglesBetweenVectors <= FOVLimit)
            Handles.color = Color.green;
        else
        {
            Handles.color = Color.red;

        }
            Handles.DrawAAPolyLine(objPos, terrain.position);
        //Needs to be * -1 to flip the plane of reference the other way because we are looking from right to left
        signedAngleBetween = Vector3.SignedAngle(objToTerrain, lookDir, Vector3.forward * -1);
        //Determinant or 2D Cross Product
        determinant = Mathf.Sin(builtInAnglesBetweenVectors);

        if (Physics.Raycast(objPos, lookDir, out RaycastHit hitobj))
        {
            Vector3 hitNormal = hitobj.normal;
            Vector3 hitPos = hitobj.point;
            //Display the forward Vector as a raycast in grean if hit
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(objPos, hitPos);
            //Draw the hit position's normal
            Handles.color = Color.cyan;
            Handles.DrawAAPolyLine(hitPos, hitPos + hitNormal);
            //Get the right vector for orientation using the cross product between the other 2
            //available vectors that are interacting with hitPos
            Handles.color = Color.green;
             Vector3 right = Vector3.Cross(hitNormal, lookDir).normalized;
            rightMag = right.magnitude;
            //Draw the right vector
            Handles.DrawAAPolyLine(hitPos, hitPos + right);
            //Finally get the final vector neccissarry for orientation using the 2
            //vectors that are coming out of hitPos "right and the hitNormal/up vector"
            Vector3 facingDirection = Vector3.Cross(right, hitNormal);
            //Draw the final vector
            Handles.color = Color.red;
            Handles.DrawAAPolyLine(hitPos, hitPos + facingDirection);

        }
        else
        {
            //Display the forward Vector as a raycast in red if missed
            Handles.color = Color.red;
            Handles.DrawAAPolyLine(objPos, objPos + lookDir * 12);
        }
        
    }
}
