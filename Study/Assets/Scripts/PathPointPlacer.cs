using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPointPlacer : MonoBehaviour
{
    public float spacing = 0.1f;
    public float resolution = 1f;
    List<GameObject> spheres = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        Vector2[] points = FindObjectOfType<BezierPathCreator>().path.CalcEvenlySpacedPoints(spacing, resolution);
        foreach (Vector2 point in points)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = point;
            sphere.transform.localScale = Vector3.one * spacing * 0.5f;
            spheres.Add(sphere);
        }
    }
    public void UpdateSpheres()
    {
        if (Application.isPlaying)
        {
            Vector2[] points = FindObjectOfType<BezierPathCreator>().path.CalcEvenlySpacedPoints(spacing, resolution);
            if (spheres.Count > points.Length)
            {
                for (int i = points.Length; i < spheres.Count; i++)
                {
                    Destroy(spheres[i]);
                }
                spheres.RemoveRange(points.Length, spheres.Count - points.Length);
            }
            for (int i = 0; i < points.Length; i++)
            {
                if (points.Length > spheres.Count)
                {
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.position = points[i];
                    sphere.transform.localScale = Vector3.one * spacing * 0.5f;
                    spheres.Add(sphere);
                    continue;
                }
                spheres[i].transform.position = points[i];
            }
        }
    }
} 
