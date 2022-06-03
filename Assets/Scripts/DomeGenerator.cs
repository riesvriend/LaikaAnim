using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class DomeGenerator : MonoBehaviour
{
    Mesh inputMesh;

    [UnityEngine.Tooltip("Range between -0.5 and 0. Marks the cut-off Y-coordinate in the bottom half of the sphere")]
    [SerializeField] public float MaxY = 0.0f;

    void Start()
    {
        // We expect a sphere child object that we can clone and mould into a dome
        var sphereFilter = transform.GetChild(0).GetComponentInChildren<MeshFilter>();
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

        var meshFilter = GetComponent<MeshFilter>();
        //meshFilter.sharedMesh = dome;
        meshFilter.mesh = dome;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
