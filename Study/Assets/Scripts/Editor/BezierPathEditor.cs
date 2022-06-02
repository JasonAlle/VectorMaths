using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(BezierPathCreator))]
public class BezierPathEditor : Editor
{
    BezierPath path;
    BezierPathCreator creator;

    const float segmentSelectDistanceThreshold = 0.1f;
    int selectedSegmentIndex = -1;

    private void OnEnable()
    {
        creator = (BezierPathCreator)target;
        if (creator.path == null)
        {
            creator.CreatePath();
        }
        path = creator.path;
    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (GUILayout.Button("Create New Path"))
        {
            Undo.RecordObject(creator, "Create New Path");
            creator.CreatePath();
            path = creator.path;
        }
        bool isClosed = GUILayout.Toggle(path.IsClosed, "Toggle Closed");
        if (isClosed != path.IsClosed)
        {
            Undo.RecordObject(creator, "Toggle Closed");
            path.IsClosed = isClosed;
        }
        bool autoSetControlPoints = GUILayout.Toggle(path.AutoSetControlPoints, "Auto Set Control Points");
        if (autoSetControlPoints != path.AutoSetControlPoints)
        {
            Undo.RecordObject(creator, "Toggle auto set control points");
            path.AutoSetControlPoints = autoSetControlPoints;
        }
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }
    private void OnSceneGUI()
    {
        DrawCurve();
        Input();
        Event guiEvent = Event.current;
        if (guiEvent.type == EventType.Repaint)
        {
            FindObjectOfType<PathPointPlacer>().UpdateSpheres();
        }
    }
    private void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            if (selectedSegmentIndex != -1)
            {
                Undo.RecordObject(creator, "Split Segment");
                path.SplitSegment(mousePos, selectedSegmentIndex);
            }
            else if (!path.IsClosed)
            {
                Undo.RecordObject(creator, "Added Segment");
                path.AddSegment(mousePos);
            }
        }
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDistanceToAnchor = creator.anchorDiameter * 0.5f;
            int closestAnchorIndex = -1;
            for (int i = 0; i < path.NumPoints; i += 3)
            {
                float dist = Vector2.Distance(mousePos, path[i]);
                if (dist <= minDistanceToAnchor)
                {
                    minDistanceToAnchor = dist;
                    closestAnchorIndex = i;
                }
                if (closestAnchorIndex != -1)
                {
                    Undo.RecordObject(creator, "Delete Segment");
                    path.DeleteSegment(closestAnchorIndex);
                }
            }
        }
        if (guiEvent.type == EventType.MouseMove)
        {
            float minDistanceToSegment = segmentSelectDistanceThreshold;
            int newSelectedSegmentIndex = -1;
            for (int i = 0; i < path.NumSegments; i++)
            {
                Vector2[] points = path.GetPointsInSegment(i);
                float dst = HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);
                if (dst < minDistanceToSegment)
                {
                    minDistanceToSegment = dst;
                    newSelectedSegmentIndex = i;
                }
            }
            if (newSelectedSegmentIndex != selectedSegmentIndex)
            {
                selectedSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }
    }
    private void DrawCurve()
    {
        Handles.color = Color.black;
        for (int i = 0; i < path.NumSegments; i++)
        {
            Vector2[] points = path.GetPointsInSegment(i);
            if (creator.displayControlPoints)
            {
                Handles.DrawLine(points[1], points[0], 2f);
                Handles.DrawLine(points[2], points[3], 2f);
            }
            Color segmentColor = (i == selectedSegmentIndex && Event.current.shift) ? creator.selectedSegmentColor : creator.segmentColor;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentColor, null, 3.0f);
        }
        for (int i = 0; i < path.NumPoints; i++)
        {
            if (i % 3 == 0 || creator.displayControlPoints)
            {
                Handles.color = i % 3 == 0 ? creator.anchorColor : creator.controlColor;
                float diameter = i % 3 == 0 ? creator.anchorDiameter : creator.controlDiameter;
                //Create/Draw the handles as a Vector2 (because our bezier is 2d for now 
                Vector2 newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity, diameter, Vector3.zero, Handles.CylinderHandleCap);
                if (path[i] != newPos)
                {
                    //Place this on the undo stack of the Unity Editor
                    Undo.RecordObject(creator, "MovePoint");
                    //Move the point to the new position
                    path.MovePoint(i, newPos);
                }
            }
        }
    }
}
