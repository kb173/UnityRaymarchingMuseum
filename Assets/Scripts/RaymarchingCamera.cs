using System.Collections.Generic;
using UnityEngine;

// Adapted from https://github.com/SebLague/Ray-Marching
[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class RaymarchingCamera : MonoBehaviour
{
    public ComputeShader raymarching;

    RenderTexture target;
    Camera cam;
    Light lightSource;
    List<ComputeBuffer> buffersToDispose;
    List<RaymarchingShape> raymarchingShapes;
    RaymarchingShapeData[] raymarchingShapeData;

    void Init()
    {
        cam = Camera.current;
        lightSource = FindObjectOfType<Light>();

        // Assuming the scene doesn't change after loading!
        raymarchingShapes = GetAllRaymarchingShapes();
        raymarchingShapeData = GetRaymarchingShapeData();
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Init();
        buffersToDispose = new List<ComputeBuffer>();

        InitRenderTexture();
        CreateScene();
        SetParameters();

        raymarching.SetTexture(0, "Source", source);
        raymarching.SetTexture(0, "Destination", target);

        // Work groups take 8x8 pixel blocks
        int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 8.0f);
        raymarching.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(target, destination);

        foreach (var buffer in buffersToDispose)
        {
            buffer.Dispose();
        }
    }

    private List<RaymarchingShape> GetAllRaymarchingShapes()
    {
        // Find all raymarching shapes in the scene
        List<RaymarchingShape> allRaymarchingShapes = new List<RaymarchingShape>(FindObjectsOfType<RaymarchingShape>());
        allRaymarchingShapes.Sort((a, b) => a.operation.CompareTo(b.operation));

        List<RaymarchingShape> orderedRaymarchingShapes = new List<RaymarchingShape>();

        for (int i = 0; i < allRaymarchingShapes.Count; i++)
        {
            // Add top-level shapes (those without a parent)
            if (allRaymarchingShapes[i].transform.parent == null)
            {
                Transform parentRaymarchingShape = allRaymarchingShapes[i].transform;
                orderedRaymarchingShapes.Add(allRaymarchingShapes[i]);
                allRaymarchingShapes[i].numChildren = parentRaymarchingShape.childCount;
                // Add all children of the shape (nested children not supported currently)
                for (int j = 0; j < parentRaymarchingShape.childCount; j++)
                {
                    if (parentRaymarchingShape.GetChild(j).GetComponent<RaymarchingShape>() != null)
                    {
                        orderedRaymarchingShapes.Add(parentRaymarchingShape.GetChild(j).GetComponent<RaymarchingShape>());
                        orderedRaymarchingShapes[orderedRaymarchingShapes.Count - 1].numChildren = 0;
                    }
                }
            }
        }

        return allRaymarchingShapes;
    }

    RaymarchingShapeData[] GetRaymarchingShapeData()
    {
        // Extract the data from the raymarching shapes
        RaymarchingShapeData[] data = new RaymarchingShapeData[raymarchingShapes.Count];
        for (int i = 0; i < raymarchingShapes.Count; i++)
        {
            var s = raymarchingShapes[i];
            Vector3 col = new Vector3(s.colour.r, s.colour.g, s.colour.b);
            data[i] = new RaymarchingShapeData()
            {
                position = s.Position,
                scale = s.Scale,
                colour = col,
                shapeType = (int)s.shapeType,
                operation = (int)s.operation,
                blendStrength = s.blendStrength * 3,
                numChildren = s.numChildren
            };
        }

        return data;
    }

    void CreateScene()
    {
        // Pass the shapes to the raymarching shader as a buffer
        ComputeBuffer shapeBuffer = new ComputeBuffer(raymarchingShapeData.Length, RaymarchingShapeData.GetSize());
        shapeBuffer.SetData(raymarchingShapeData);
        raymarching.SetBuffer(0, "shapes", shapeBuffer);
        raymarching.SetInt("numRaymarchingShapes", raymarchingShapeData.Length);

        buffersToDispose.Add(shapeBuffer);
    }

    void SetParameters()
    {
        // Set light and camera parameters in the shader
        bool lightIsDirectional = lightSource.type == LightType.Directional;

        raymarching.SetMatrix("_CameraToWorld", cam.cameraToWorldMatrix);
        raymarching.SetMatrix("_CameraInverseProjection", cam.projectionMatrix.inverse);
        raymarching.SetVector("_Light", (lightIsDirectional) ? lightSource.transform.forward : lightSource.transform.position);
        raymarching.SetBool("positionLight", !lightIsDirectional);
    }

    void InitRenderTexture()
    {
        if (target == null || target.width != cam.pixelWidth || target.height != cam.pixelHeight)
        {
            if (target != null)
            {
                target.Release();
            }

            target = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };

            target.Create();
        }
    }

    // Data struct for raymarching shapes which can be passed to the shader in a buffer
    struct RaymarchingShapeData
    {
        public Vector3 position;
        public Vector3 scale;
        public Vector3 colour;
        public int shapeType;
        public int operation;
        public float blendStrength;
        public int numChildren;

        public static int GetSize()
        {
            // The class has 3 ints, 1 float and 3 Vector3 (which are 3 floats each)
            return sizeof(float) * 10 + sizeof(int) * 3;
        }
    }
}
