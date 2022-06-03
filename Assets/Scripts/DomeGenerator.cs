using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class DomeGenerator : MonoBehaviour
{
    [Tooltip("Floor to be moved to the bottom of the dome. Used to receive shadows at floor level.")]
    public GameObject FloorShadow;

    [Tooltip("The source mesh which must be sphere. This sphere is adjusted to be a dome by setting the floor-level at position Max Y")]
    public MeshFilter InputSphere;

    [Tooltip("Range between -0.5 and 0. Marks the cut-off Y-coordinate in the bottom half of the sphere")]
    public float MaxY = 0.0f;

    void Start()
    {
        if (InputSphere == null)
        {
            Debug.LogWarning("DomeGenerator requires a InputSphere child component holding the input mesh");
            return;
        }

        // Hide the source sphere as we will now render the adjusted mesh / dome
        var inputRender = InputSphere.GetComponent<MeshRenderer>();
        if (inputRender != null)
            inputRender.enabled = false;

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
        for (var vertexIndex = 0; vertexIndex < domeVertices.Length; vertexIndex += 1)
        {
            // push all vertices to be at or above the horizontal plane at Y=0, transforming the input
            // sphere into a dome
            var vertex = domeVertices[vertexIndex];
            vertex.y = Mathf.Max(MaxY, vertex.y);
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

        // The Dome's Y position is the middle of the source-sphere and is 0 by default
        // we have to move it up the Y axis so that the floor of the dome is at 0.
        // This way gameobjects inside the dome are on the floor when placed at Y = 0
        var domeY = transform.localScale.y * -MaxY;
        transform.localPosition = new Vector3(transform.localPosition.x, domeY, transform.localPosition.z);    

        if (FloorShadow != null)
        {
            // Move the floor that receives shadows to the bottom of the dome
            var floorPosition = FloorShadow.transform.localPosition;
            floorPosition = new Vector3(floorPosition.x, MaxY, floorPosition.z);
            FloorShadow.transform.localPosition = floorPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
