using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class DomeGenerator : MonoBehaviour
{
    [Tooltip("Floor to be moved to the bottom of the dome. Used to receive shadows at floor level.")]
    public GameObject FloorShadow;

    [Tooltip("The source mesh which must be sphere. This sphere is adjusted to be a dome by setting the floor-level at position Max Y")]
    public MeshFilter InputSphere; // from a child game object

    [Tooltip("Range between -0.5 and 0. Marks the cut-off Y-coordinate in the bottom half of the sphere")]
    public float MaxY = -0.03f;

    [Tooltip("Increase to bring items close to the center of the floor more to the center. CorrectionPower of cancels the correction, as it results in a factor 1")]
    public float CorrectionPower = 0f;


    private float? _prevMaxY, _prevCorrectionPower;


    void Start()
    {
        if (InputSphere == null)
        {
            //Debug.LogWarning("DomeGenerator requires a InputSphere child component holding the input mesh");
            // Assume we use sphere on our mesh filter as-is
            return;
        }

        // Hide the source sphere as we will now render the adjusted mesh / dome
        var inputRender = InputSphere.GetComponent<MeshRenderer>();
        if (inputRender != null)
            inputRender.enabled = false;

    }

    void Update()
    {
        if (_prevMaxY == MaxY && _prevCorrectionPower == CorrectionPower)
            return;

        _prevMaxY = MaxY;
        _prevCorrectionPower = CorrectionPower;

        if (InputSphere == null)
        {
            transform.position = new Vector3(0, -MaxY * transform.localScale.y, 0);
        }
        else
        {
            // We expect a sphere child object that we can clone and mould into a dome
            var sphereFilter = InputSphere;
            // The mesh is an instantiated clone of the Sphere asset
            var inputSphere = sphereFilter.mesh;
            if (inputSphere == null)
                return;

            // Clone the sphere mesh https://answers.unity.com/questions/398785/how-do-i-clone-a-sharedmesh.html
            var dome = Instantiate(inputSphere);

            // make it a dome
            // Accessing the vertices clones the full array, see https://docs.unity3d.com/ScriptReference/Mesh.html
            var domeVertices = dome.vertices;

            // First ensure no triangles are partially on floor level
            var domeTriangles = dome.triangles; // returns a clone
            for (var triangleIndex = 0; triangleIndex < domeTriangles.Length; triangleIndex += 3)
            {
                Vector3 v0, v1, v2;
                v0 = domeVertices[domeTriangles[triangleIndex]];
                v1 = domeVertices[domeTriangles[triangleIndex + 1]];
                v2 = domeVertices[domeTriangles[triangleIndex + 2]];
                var isAnyVertexBelowFloorLevel = v0.y < MaxY || v1.y < MaxY || v2.y < MaxY;
                if (isAnyVertexBelowFloorLevel)
                {
                    v0.y = MaxY;
                    v1.y = MaxY;
                    v2.y = MaxY;
                }
            }


            var correctionLog = new StringBuilder();
            for (var vertexIndex = 0; vertexIndex < domeVertices.Length; vertexIndex += 1)
            {
                // push all vertices to be at or above the horizontal plane at Y=0, transforming the input
                // sphere into a dome
                var vertex = domeVertices[vertexIndex];

                // test to prevent flickering at triangles that are truncated at floor level
                var threshold = 0.04f;  // vertices are about 0.1f apart on the unit sphere
                if (vertex.y > MaxY && vertex.y < MaxY + threshold)
                {
                    Debug.Log($"Truncated to floor. y: {vertex.y}");
                    vertex.y = MaxY;
                }

                if (vertex.y <= MaxY)
                {
                    // Below floor level, so move the vertex up to floor level
                    vertex.y = MaxY;

                    if (CorrectionPower > 0)
                    {
                        // scale the vertices that are close the center of the floor even more to the center
                        // to compensate for the fact that the camera is now closer
                        // we use a cosine wave that corrects with max value of 1 at radius 0
                        // and no correction (0) at the edge of the sphere where r=0.5
                        // r = sqrt(x^2 + z^2). 
                        var radius = Mathf.Sqrt(vertex.x * vertex.x + vertex.z * vertex.z);
                        float sphereRadius = 0.5f;
                        // coordinates are from 0 to 0.5, we need them to be from 0 to PI/2 
                        // so that the sine flows from 1 to 0 over the radius of the sphere.
                        float coordinateScaleFactor = (Mathf.PI / 2f) / sphereRadius;
                        var correctionFactor = Mathf.Sin(radius * coordinateScaleFactor);
                        // Widen the sine wave to have extra effect at the center
                        correctionFactor = Mathf.Pow(correctionFactor, CorrectionPower);
                        correctionLog.AppendLine($"x:{vertex.x} z:{vertex.z} r:{radius} correctionFactor: {correctionFactor}");
                        vertex.x *= correctionFactor;
                        vertex.z *= correctionFactor;
                        correctionLog.AppendLine($"x:{vertex.x} z:{vertex.z}");
                    }
                }
                if (correctionLog.ToString() != "")
                    Debug.Log(correctionLog);
                domeVertices[vertexIndex] = vertex;
            }



            // Re-assign the cloned array to the mesh
            dome.vertices = domeVertices;

            dome.RecalculateBounds();
            dome.RecalculateNormals();
            dome.RecalculateTangents();

            // TODO: 
            // the video is rendered on the inside of the dome, not on the outside, as the camera/VR is inside
            // for better perf, invert all normals of the sphere here in the Start logic (so that the outside of the dome becomes the inside)
            // we can/should then remove the normals inversion logic from the shader in the 360PlayerMaterial, which renders on each frame

            var meshFilter = GetComponent<MeshFilter>();
            //meshFilter.sharedMesh = dome;
            meshFilter.mesh = dome;
        }

        // The Dome's Y position is the middle of the source-sphere and is 0 by default
        // we have to move it up the Y axis so that the floor of the dome is at 0.
        // This way gameobjects inside the dome are on the floor when placed at Y = 0
        if (FloorShadow != null)
        {
            // Move the floor that receives shadows to the bottom of the dome
            var floorPosition = FloorShadow.transform.localPosition;
            floorPosition = new Vector3(floorPosition.x, MaxY, floorPosition.z);
            FloorShadow.transform.localPosition = floorPosition;
        }
    }
}


