using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierPathCreator : MonoBehaviour
{
    [HideInInspector]
    public BezierPath path;
    public Color anchorColor = Color.red;
    public Color controlColor = Color.white;
    public Color segmentColor = Color.green;
    public Color selectedSegmentColor = Color.yellow;
    public float anchorDiameter = 0.1f;
    public float controlDiameter = .075f;
    public bool displayControlPoints = true;

    public void CreatePath()
    {
        path = new BezierPath(transform.position);
    }
}
