using System;
using System.Collections.Generic;
using UnityEngine;


public class Cutter : MonoBehaviour
{
    private static bool isBusy;
    private static Mesh originalMesh;

    public static GameObject Cut(GameObject originalGameObject, Vector3 contactPoint, Vector3 cutNormal)
    {
        if (isBusy)
        {
            return null;
        }

        isBusy = true;
        
        try
        {
            Plane cutPlane = new Plane(originalGameObject.transform.InverseTransformDirection(-cutNormal), originalGameObject.transform.InverseTransformPoint(contactPoint));
            originalMesh = originalGameObject.GetComponent<MeshFilter>().mesh;

            if (originalMesh == null)
            {
                Debug.Log("No Mesh to cut");
                return null;
            }

            List<Vector3> addedVertices = new List<Vector3>();
            GeneratedMesh leftMesh = new GeneratedMesh();
            GeneratedMesh rightMesh = new GeneratedMesh();
            
            SeparateMeshes(leftMesh, rightMesh, cutPlane, addedVertices);
        }
        
    }

    private static void SeparateMeshes(GeneratedMesh leftMesh, GeneratedMesh rightMesh, Plane plane, List<Vector3> addedVertices)
    {
        for (int i = 0; i < originalMesh.subMeshCount; i++)
        {
            var subMeshIndices = originalMesh.GetTriangles(i);
            for (int j = 0; j < subMeshIndices.Length; j += 3)
            {
                var triangleIndexA = subMeshIndices[j];
                var triangleIndexB = subMeshIndices[j + 1];
                var triangleIndexC = subMeshIndices[j + 2];
                
                MeshTriangle currentTriangle = GetTriangle(triangleIndexA, triangleIndexB, triangleIndexC, i);
                
                bool triangleALeftSide = plane.GetSide(originalMesh.vertices[triangleIndexA]);
                bool triangleBLeftSide = plane.GetSide(originalMesh.vertices[triangleIndexB]);
                bool triangleCLeftSide = plane.GetSide(originalMesh.vertices[triangleIndexC]);

                switch (triangleALeftSide)
                {
                    case true when triangleBLeftSide && triangleCLeftSide:
                        leftMesh.AddTriangle(currentTriangle);
                        break;
                    case false when !triangleBLeftSide && !triangleCLeftSide:
                        rightMesh.AddTriangle(currentTriangle);
                        break;
                    default:
                        CutTriangle(plane, currentTriangle, triangleALeftSide, triangleBLeftSide, triangleCLeftSide, leftMesh, rightMesh, addedVertices);
                        break;
                }
            }
        }
    }

    private static void CutTriangle(Plane plane, MeshTriangle currentTriangle, bool triangleALeftSide, bool triangleBLeftSide, bool triangleCLeftSide, GeneratedMesh leftMesh, GeneratedMesh rightMesh, List<Vector3> addedVertices)
    {
        throw new NotImplementedException();
    }

    private static MeshTriangle GetTriangle(int triangleIndexA, int triangleIndexB, int triangleIndexC, int submeshIndex)
    {
        Vector3[] verticesToAdd =
        {
            originalMesh.vertices[triangleIndexA],
            originalMesh.vertices[triangleIndexB],
            originalMesh.vertices[triangleIndexC]
        };
        
        Vector3[] normalsToAdd =
        {
            originalMesh.normals[triangleIndexA],
            originalMesh.normals[triangleIndexB],
            originalMesh.normals[triangleIndexC]
        };
        
        Vector2[] uvsToAdd =
        {
            originalMesh.uv[triangleIndexA],
            originalMesh.uv[triangleIndexB],
            originalMesh.uv[triangleIndexC]
        };
        
        return new MeshTriangle(verticesToAdd, normalsToAdd, uvsToAdd, submeshIndex);
    }
}