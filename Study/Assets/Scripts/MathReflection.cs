using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathReflection
{
    public static Vector3 Reflect(Vector3 hitNormal, Vector3 hitPos, Vector3 bouncePos)
    {
        Vector3 objToHit = hitPos - bouncePos;
        //Find the reflection
        //1. find the projection of the ray direction vector onto the normal of the hitPoint
        Vector3 projOnN = Vector3.Dot(objToHit, hitNormal) * hitNormal;
        //2. Subtract two of the projection vector from the ray vector to get to the other side of the original object
        //based on the hitNormal to get the reflection vector
        Vector3 reflectionVector = objToHit - (2 * projOnN);
        //3. add the pos of the hit to displace the vector onto the hitPos
        Vector3 reflectAtHit = reflectionVector + hitPos;
        //TADA
        return reflectionVector;
    }
}
