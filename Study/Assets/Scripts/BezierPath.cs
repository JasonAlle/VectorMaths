using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BezierPath
{
    [SerializeField, HideInInspector]
    List<Vector2> points;
    [SerializeField, HideInInspector]
    bool isClosed;
    [SerializeField, HideInInspector]
    bool isAutoSetControlPoints;
    public int NumSegments
    {
        //To get the number of segments we divide the number of counts by 3
        //4 / 3 = 1
        // 7 / 3 = 2
        //... 
        get { return points.Count / 3; }
    }
    public int NumPoints
    {
        get { return points.Count; }
    }
    public Vector2 this[int i]
    {
        get
        {
            return points[i];
        }
    }
    public bool AutoSetControlPoints
    {
        get {
            return isAutoSetControlPoints;
        }
        set
        {
            if (isAutoSetControlPoints != value)
            {
                isAutoSetControlPoints = value;
                if (isAutoSetControlPoints)
                {
                    AutoSetAllControlPoints();
                }
            }
        }
    }
    public bool IsClosed
    {
        get
        {
            return isClosed;
        }
        set
        {
            if (isClosed != value)
            {
                isClosed = !isClosed;
                isClosed = value;
                if (isClosed)
                {
                    points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
                    points.Add(points[0] * 2 - points[1]);
                    if (isAutoSetControlPoints)
                    {
                        AutoSetControlPointsByAnchor(0);
                        AutoSetControlPointsByAnchor(points.Count - 3);

                    }
                }
                else
                {
                    points.RemoveRange(points.Count - 2, 2);
                    if (isAutoSetControlPoints)
                    {
                        AutoSetStartEndControls();

                    }
                }
            }
        }
    }
    public Vector2[] GetPointsInSegment(int i)
    {
        //Points are based on passed in indicies
        // if 0 is passed in then we get the initial 4 points as:
        //0 x 3 is 0
        //0 x 3 + 1 is 1 and so on
        return new Vector2[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[LoopIndex(i * 3 + 3)] };
    }
    public BezierPath(Vector2 centerPoint)
    {
        //Create the initial 4 points (segment) based on a centerPosition
        points = new List<Vector2>(){
            centerPoint + Vector2.left, // Point to the left of the center
            centerPoint + (Vector2.left + Vector2.up) * 0.5f, // Point to the left and up (halfway) of the center
            centerPoint + (Vector2.right + Vector2.down) * 0.5f, // Point to the right and down (halfway) of the center
            centerPoint + Vector2.right // Point to the right of the center
        };
    }
    public void AddSegment(Vector2 anchorPos)
    {
        //Adds a new segment based on the anchorPosition by connecting those points in the point list
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]); // First new point is placed at double the length of the distance between point #2 and point #3
        points.Add((points[points.Count - 1] + anchorPos) * 0.5f); // Second new point is the point just added plus half the distance to the anchor point
        points.Add(anchorPos); // Last point is just the anchorPosition
        //3 new Points creates a segment 
        if (isAutoSetControlPoints)
        {
            AutoSetAffectedControlPoints(points.Count - 1);

        }
    }

    public void MovePoint(int i, Vector2 pos)
    {
        Vector2 dispPos = pos - points[i];
        if (i % 3 == 0 || !isAutoSetControlPoints)
        {
            points[i] = pos;
            if (isAutoSetControlPoints)
            {
                AutoSetAffectedControlPoints(i);
            }
            else
            {
                if (i % 3 == 0)
                {
                    if (i + 1 < points.Count || isClosed)
                    {
                        points[LoopIndex(i + 1)] += dispPos;
                    }
                    if (i - 1 >= 0 || isClosed)
                    {
                        points[LoopIndex(i - 1)] += dispPos;
                    }
                }
                else
                {
                    bool isNextPointAnchor = (i + 1) % 3 == 0;
                    int correspondingControlIndex = (isNextPointAnchor) ? i + 2 : i - 2;
                    int anchorIndex = (isNextPointAnchor) ? i + 1 : i - 1;
                    if (correspondingControlIndex >= 0 && correspondingControlIndex < points.Count || isClosed)
                    {
                        float distanceScalar = (points[LoopIndex(anchorIndex)] - points[LoopIndex(correspondingControlIndex)]).magnitude;
                        Vector2 directionVector = (points[LoopIndex(anchorIndex)] - pos).normalized;
                        points[LoopIndex(correspondingControlIndex)] = points[LoopIndex(anchorIndex)] + directionVector * distanceScalar;
                    }
                }
            }
        }
    }
    public void SplitSegment(Vector2 anchorPos, int segmentIndex)
    {
        points.InsertRange(segmentIndex * 3 + 2, new Vector2[] { Vector2.zero, anchorPos, Vector2.zero });
        if (isAutoSetControlPoints)
        {
            AutoSetAffectedControlPoints(segmentIndex * 3 + 3);
        }
        else
        {
            AutoSetControlPointsByAnchor(segmentIndex * 3 + 3);
        }
    }
    public void DeleteSegment(int anchorIndex)
    {
        if (NumSegments > 2 || (!isClosed && NumSegments > 1))
        {
            if (anchorIndex == 0)
            {
                if (isClosed)
                {
                    points[points.Count - 1] = points[2];
                    return;
                }
                points.RemoveRange(0, 3);
                return;
            }
            else if (anchorIndex == points.Count - 1)
            {
                if (!isClosed)
                {
                    points.RemoveRange(anchorIndex - 2, 3);
                    return;
                }
            }
            points.RemoveRange(anchorIndex - 1, 3);
        }
    }
    private void AutoSetControlPointsByAnchor(int anchorIndex)
    {
        //Get the point at the anchorIndex
        Vector2 anchorPos = points[anchorIndex];
        //Reset its direction
        Vector2 anchorDir = Vector2.zero;
        //We want 2 neighbouring anchors
        float[] neighbourDistances = new float[2];
        if (anchorIndex - 3 >= 0 || isClosed)
        {
            //Get the difference between anchorPos and the anchor point before this one
            Vector2 offSet = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            //Get the direction between the 2 points and add it to the anchors direction
            anchorDir += offSet.normalized;
            //Set the distance to the distance between the 2 anchorPoints
            neighbourDistances[0] = offSet.magnitude;
        }
        if (anchorIndex + 3 >= 0 || isClosed)
        {
            //Same stuff but with the anchor point after the passed in one
            Vector2 offSet = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            //Flip the sign to go the other way
            anchorDir -= offSet.normalized;
            //Same here
            neighbourDistances[1] = -offSet.magnitude;
        }
        //Normalize this direction for good 
        anchorDir.Normalize();
        for (int i = 0; i < neighbourDistances.Length; i++)
        {
            //Get the controlPointIndex by starting at the passed in anchorPoint
            //then multiplying the neighbour point by 2
            //add that together to either 
            // At 0: stay in the same segment
            //At 1: move to the next segment
            //Subtract one to get the point behind either the neighbour (the control point ahead of the passed in anchor point)
            //or get the point behind the passed in anchorPoint (the controlPoint behind the anchorPoint) 
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < points.Count || isClosed)
            {
                //Set the position of the controlPoint to be the direction between the anchorPoints scaled by half of the
                //Distance between them
                points[LoopIndex(controlIndex)] = anchorPos + anchorDir * neighbourDistances[i] * 0.5f;
            }
        }
    }
    public Vector2[] CalcEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(points[0]);
        Vector2 previousPoint = points[0];
        float dstSinceLastEvenPoint = 0f;
        for (int segmentIndex = 0; segmentIndex < NumSegments ; segmentIndex++)
        {
            Vector2[] pointInSegment = GetPointsInSegment(segmentIndex);
            float controlNetLength = Vector2.Distance(pointInSegment[0], pointInSegment[1]) + Vector2.Distance(pointInSegment[1], pointInSegment[2]) + Vector2.Distance(pointInSegment[2], pointInSegment[3]);
            float estimatedCurveLength = Vector2.Distance(pointInSegment[0], pointInSegment[3]) + controlNetLength /2f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10f);
            float t = 0.0f;
            while (t <= 1)
            {
                t += 1f / divisions;
                Vector2 pointOnCurve = BezierCurve.EvaluateCubic(pointInSegment[0], pointInSegment[1], pointInSegment[2], pointInSegment[3], t);
                dstSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);
                while (dstSinceLastEvenPoint >= spacing)
                {
                    float overShotDst = dstSinceLastEvenPoint - spacing;
                    Vector2 newEvenlySpacePoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overShotDst;
                    evenlySpacedPoints.Add(newEvenlySpacePoint);
                    dstSinceLastEvenPoint = overShotDst;
                previousPoint = newEvenlySpacePoint;
                }
                previousPoint = pointOnCurve;
            }
        }
        return evenlySpacedPoints.ToArray();
    }
    private void AutoSetStartEndControls()
    {
        if (!isClosed)
        {
            //Set the point infront of the first anchor point to halfway
            //between the first anchor in the list and the next anchor in the list
            points[1] = (points[0] + points[2]) * 0.5f;
            //Same thing here but going down the list instead of up
            points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * 0.5f;
        }
    }
    private void AutoSetAffectedControlPoints(int anchorIndex)
    {
        for (int i = anchorIndex - 3; i < anchorIndex + 3; i += 3)
        {
            if (i >= 0 && i < points.Count || isClosed)
            {
                AutoSetControlPointsByAnchor(LoopIndex(i));
            }
        }
        AutoSetStartEndControls();
    }
    private void AutoSetAllControlPoints()
    {
        for (int i = 0; i < points.Count; i++)
        {
            AutoSetControlPointsByAnchor(i);
        }
        AutoSetStartEndControls();
    }
    private int LoopIndex(int i)
    {
        //Mod i by points.count to know when to loop back around
        //Add i by points.count to handle negatives
        // if i is even negative (out of bounds) it will always return up to point.count - 1
        return (i + points.Count) % points.Count; 
    }
    
}
