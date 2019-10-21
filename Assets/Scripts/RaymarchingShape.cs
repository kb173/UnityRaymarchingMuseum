using UnityEngine;

// Adapted from https://github.com/SebLague/Ray-Marching
public class RaymarchingShape : MonoBehaviour
{

    public enum ShapeType { Sphere, Cube, Torus, Mandelbulb };
    public enum Operation { None, Blend, Cut, Mask }

    public ShapeType shapeType;
    public Operation operation;
    public Color colour = Color.white;

    [Range(0, 1)]
    public float blendStrength;

    [HideInInspector]
    public int numChildren;

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    public Vector3 Scale
    {
        get
        {
            Vector3 parentScale = Vector3.one;

            if (transform.parent != null
                && transform.parent.GetComponent<RaymarchingShape>() != null)
            {
                parentScale = transform.parent.GetComponent<RaymarchingShape>().Scale;
            }

            return Vector3.Scale(transform.localScale, parentScale);
        }
    }
}
