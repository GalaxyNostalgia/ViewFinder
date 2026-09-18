using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;


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
            Plane cutPlane = new Plane(originalGameObject.transform.InverseTransformDirection(-cutNormal),
                originalGameObject.transform.InverseTransformPoint(contactPoint));
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

            FillCut(addedVertices, cutPlane, leftMesh, rightMesh);

            Mesh finishedLeftMesh = leftMesh.GetGeneratedMesh();
            Mesh finishedRightMesh = rightMesh.GetGeneratedMesh();

            var originalCols = originalGameObject.GetComponents<Collider>();
            foreach (var col in originalCols)
            {
                Destroy(col);
            }

            originalGameObject.GetComponent<MeshFilter>().mesh = finishedLeftMesh;
            var collider = originalGameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = finishedLeftMesh;
            collider.convex = true;

            var mat = originalGameObject.GetComponent<MeshRenderer>().material;
            Material[] mats = new Material[finishedLeftMesh.subMeshCount];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = mat;
            }

            originalGameObject.GetComponent<MeshRenderer>().materials = mats;
            GameObject right = new GameObject();
            right.transform.position = originalGameObject.transform.position;
            right.transform.rotation = originalGameObject.transform.rotation;
            right.transform.localScale = originalGameObject.transform.localScale;
            right.AddComponent<MeshRenderer>();

            mats = new Material[finishedRightMesh.subMeshCount];
            for (int i = 0; i < finishedRightMesh.subMeshCount; i++)
            {
                mats[i] = mat;
            }

            right.GetComponent<MeshRenderer>().materials = mats;
            right.AddComponent<MeshFilter>().mesh = finishedRightMesh;
            right.AddComponent<MeshCollider>().sharedMesh = finishedRightMesh;

            var cols = right.GetComponents<MeshCollider>();
            foreach (var col in cols)
            {
                col.convex = true;
            }

            var rightRigidBody = right.AddComponent<Rigidbody>();
            rightRigidBody.isKinematic = true;

            right.name = originalGameObject.name + Random.Range(0, 9999);
            right.layer = LayerMask.NameToLayer("Cuttable");
            return right;

        }
        finally
        {
            isBusy = false;
        }
        
    }

    private static void FillCut(List<Vector3> _addedVertices, Plane _plane, GeneratedMesh _leftMesh, GeneratedMesh _rightMesh)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> polygon = new List<Vector3>();

        for (int i = 0; i < _addedVertices.Count - 1; i++)
        {
            if(!vertices.Contains(_addedVertices[i]))
            {
                polygon.Clear();
                polygon.Add(_addedVertices[i]);
                polygon.Add(_addedVertices[i + 1]);
                
                vertices.Add(_addedVertices[i]);
                vertices.Add(_addedVertices[i + 1]);

                EvaluatePairs(_addedVertices, vertices, polygon);
                Fill(polygon, _plane, _leftMesh, _rightMesh);
            }
        }
    }

    private static void Fill(List<Vector3> _vertices, Plane _plane, GeneratedMesh _leftMesh, GeneratedMesh _rightMesh)
    {
        Vector3 centerPosition = Vector3.zero;
        for (int i = 0; i < _vertices.Count; i++)
        {
            centerPosition += _vertices[i];
        }
        centerPosition /= _vertices.Count;
        
        Vector3 up = new Vector3(_plane.normal.x, _plane.normal.y, _plane.normal.z);
        Vector3 referenceAxis = Mathf.Abs(Vector3.Dot(up, Vector3.up)) > 0.99f ? Vector3.right : Vector3.up;
        Vector3 left = Vector3.Cross(up, referenceAxis).normalized;

        Vector3 displacement = Vector3.zero;
        Vector2 uv1 = Vector2.zero;
        Vector2 uv2 = Vector2.zero;

        for (int i = 0; i < _vertices.Count; i++)
        {
            displacement = _vertices[i] - centerPosition;
            uv1 = new Vector2()
            {
                x = .5f + Vector3.Dot(displacement, left),
                y = .5f + Vector3.Dot(displacement, up),
            };
            
            displacement = _vertices[(i+1) % _vertices.Count] - centerPosition;
            uv2 = new Vector2()
            {
                x = .5f + Vector3.Dot(displacement, left),
                y = .5f + Vector3.Dot(displacement, up),
            };
            
            Vector3[] vertices = {_vertices[i], _vertices[(i+1) % _vertices.Count], centerPosition};
            Vector3[] normals = {-_plane.normal, -_plane.normal, -_plane.normal};
            Vector2[] uvs = {uv1, uv2, new Vector2(.5f, .5f)};
            
            MeshTriangle currentTriangle = new MeshTriangle(vertices, normals, uvs, originalMesh.subMeshCount + 1);

            if (Vector3.Dot(Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]), normals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            _leftMesh.AddTriangle(currentTriangle);
            
            normals = new[] { _plane.normal, _plane.normal, _plane.normal };
            currentTriangle = new MeshTriangle(vertices, normals, uvs, originalMesh.subMeshCount + 1);

            if (Vector3.Dot(Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]), normals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            _rightMesh.AddTriangle(currentTriangle);
        }
    }

    private static void EvaluatePairs(List<Vector3> _addedVertices, List<Vector3> _vertices, List<Vector3> _polygon)
    {
        bool isDone = false;
        while (!isDone)
        {
            isDone = true;
            for (int i = 0; i < _addedVertices.Count; i+=2)
            {
                if (_addedVertices[i] == _polygon[_polygon.Count - 1] && !_vertices.Contains(_addedVertices[i + 1]))
                {
                    isDone = false;
                    _polygon.Add(_addedVertices[i + 1]);
                    _vertices.Add(_addedVertices[i + 1]);
                }
                else if (_addedVertices[i + 1] == _polygon[_polygon.Count - 1] && !_vertices.Contains(_addedVertices[i]))
                {
                    isDone = false;
                    _polygon.Add(_addedVertices[i]);
                    _vertices.Add(_addedVertices[i]);
                }
            }
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

    private static void CutTriangle(Plane plane, MeshTriangle triangle, bool triangleALeftSide, bool triangleBLeftSide,
        bool triangleCLeftSide, GeneratedMesh leftMesh, GeneratedMesh rightMesh, List<Vector3> addedVertices)
    {
        List<bool> leftSide = new List<bool>();
        leftSide.Add(triangleALeftSide);
        leftSide.Add(triangleBLeftSide);
        leftSide.Add(triangleCLeftSide);

        MeshTriangle leftMeshTriangle =
            new MeshTriangle(new Vector3[2], new Vector3[2], new Vector2[2], triangle.SubmeshIndex);
        MeshTriangle rightMeshTriangle =
            new MeshTriangle(new Vector3[2], new Vector3[2], new Vector2[2], triangle.SubmeshIndex);

        bool left = false;
        bool right = false;

        for (int i = 0; i < 3; i++)
        {
            if (leftSide[i])
            {
                if (!left)
                {
                    left = true;

                    leftMeshTriangle.Vertices[0] = triangle.Vertices[i];
                    leftMeshTriangle.Vertices[1] = leftMeshTriangle.Vertices[0];

                    leftMeshTriangle.UVs[0] = triangle.UVs[i];
                    leftMeshTriangle.UVs[1] = leftMeshTriangle.UVs[0];

                    leftMeshTriangle.Normals[0] = triangle.Normals[i];
                    leftMeshTriangle.Normals[1] = leftMeshTriangle.Normals[0];
                }
                else
                {
                    leftMeshTriangle.Vertices[1] = triangle.Vertices[i];
                    leftMeshTriangle.Normals[1] = triangle.Normals[i];
                    leftMeshTriangle.UVs[1] = triangle.UVs[i];
                }
            }
            else
            {
                if (!right)
                {
                    right = true;

                    rightMeshTriangle.Vertices[0] = triangle.Vertices[i];
                    rightMeshTriangle.Vertices[1] = rightMeshTriangle.Vertices[0];

                    rightMeshTriangle.UVs[0] = triangle.UVs[i];
                    rightMeshTriangle.UVs[1] = rightMeshTriangle.UVs[0];

                    rightMeshTriangle.Normals[0] = triangle.Normals[i];
                    rightMeshTriangle.Normals[1] = rightMeshTriangle.Normals[0];
                }
                else
                {
                    rightMeshTriangle.Vertices[1] = triangle.Vertices[i];
                    rightMeshTriangle.Normals[1] = triangle.Normals[i];
                    rightMeshTriangle.UVs[1] = triangle.UVs[i];
                }
            }
        }

        float normalizedDistance;
        float distance;
        
        plane.Raycast(new Ray(leftMeshTriangle.Vertices[0], (rightMeshTriangle.Vertices[0] - leftMeshTriangle.Vertices[0]).normalized), out distance);
        normalizedDistance = distance / (rightMeshTriangle.Vertices[0] - leftMeshTriangle.Vertices[0]).magnitude;
        Vector3 vertLeft = Vector3.Lerp(leftMeshTriangle.Vertices[0], rightMeshTriangle.Vertices[0], normalizedDistance);
        addedVertices.Add(vertLeft);
        
        Vector3 normalLeft = Vector3.Lerp(leftMeshTriangle.Normals[0], rightMeshTriangle.Normals[0], normalizedDistance);
        Vector2 uvLeft = Vector2.Lerp(leftMeshTriangle.UVs[0], rightMeshTriangle.UVs[0], normalizedDistance);
        
        plane.Raycast(new Ray(leftMeshTriangle.Vertices[1], (rightMeshTriangle.Vertices[1] - leftMeshTriangle.Vertices[1]).normalized), out distance);
        normalizedDistance = distance / (rightMeshTriangle.Vertices[1] - leftMeshTriangle.Vertices[1]).magnitude;
        Vector3 vertRight = Vector3.Lerp(leftMeshTriangle.Vertices[1], rightMeshTriangle.Vertices[1], normalizedDistance);
        addedVertices.Add(vertRight);
        
        Vector3 normalRight = Vector3.Lerp(leftMeshTriangle.Normals[1], rightMeshTriangle.Normals[1], normalizedDistance);
        Vector2 uvRight = Vector2.Lerp(leftMeshTriangle.UVs[1], rightMeshTriangle.UVs[1], normalizedDistance);

        MeshTriangle currentTriangle;
        Vector3[] updatedVertices = { leftMeshTriangle.Vertices[0], vertLeft, vertRight };
        Vector3[] updatedNormals = { leftMeshTriangle.Normals[0], normalLeft, normalRight };
        Vector2[] updatedUVs = { leftMeshTriangle.UVs[0], uvLeft, uvRight };

        currentTriangle = new MeshTriangle(updatedVertices, updatedNormals, updatedUVs, triangle.SubmeshIndex);
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if(Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0], updatedVertices[2] - updatedVertices[0]), updatedNormals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            } 
            
            leftMesh.AddTriangle(currentTriangle);
            
        }
        
        updatedVertices = new Vector3[] { leftMeshTriangle.Vertices[0], leftMeshTriangle.Vertices[1], vertRight }; 
        updatedNormals = new Vector3[] { leftMeshTriangle.Normals[0], leftMeshTriangle.Normals[1], normalRight };
        updatedUVs = new Vector2[] { leftMeshTriangle.UVs[0], leftMeshTriangle.UVs[1], uvRight };
        
        currentTriangle = new MeshTriangle(updatedVertices, updatedNormals, updatedUVs, triangle.SubmeshIndex);
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if(Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0], updatedVertices[2] - updatedVertices[0]), updatedNormals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            } 
            
            leftMesh.AddTriangle(currentTriangle);
            
        }
        
        updatedVertices = new Vector3[] { rightMeshTriangle.Vertices[0], vertLeft, vertRight }; 
        updatedNormals = new Vector3[] { rightMeshTriangle.Normals[0], normalLeft, normalRight };
        updatedUVs = new Vector2[] { rightMeshTriangle.UVs[0], uvLeft, uvRight };
        
        currentTriangle = new MeshTriangle(updatedVertices, updatedNormals, updatedUVs, triangle.SubmeshIndex);
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if(Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0], updatedVertices[2] - updatedVertices[0]), updatedNormals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            } 
            
            rightMesh.AddTriangle(currentTriangle);
            
        }
        
        updatedVertices = new Vector3[] { rightMeshTriangle.Vertices[0], rightMeshTriangle.Vertices[1], vertRight }; 
        updatedNormals = new Vector3[] { rightMeshTriangle.Normals[0], rightMeshTriangle.Normals[1], normalRight };
        updatedUVs = new Vector2[] { rightMeshTriangle.UVs[0], rightMeshTriangle.UVs[1], uvRight };
        
        currentTriangle = new MeshTriangle(updatedVertices, updatedNormals, updatedUVs, triangle.SubmeshIndex);
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if(Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0], updatedVertices[2] - updatedVertices[0]), updatedNormals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            } 
            
            rightMesh.AddTriangle(currentTriangle);
            
        }
        
    }

    private static void FlipTriangle(MeshTriangle _triangle)
    {
        Vector3 temp = _triangle.Vertices[2];
        _triangle.Vertices[2] = _triangle.Vertices[0];
        _triangle.Vertices[0] = temp;
        
        temp = _triangle.Normals[2];
        _triangle.Normals[2] = _triangle.Normals[0];
        _triangle.Normals[0] = temp;
        
        (_triangle.UVs[2], _triangle.UVs[0]) = (_triangle.UVs[0], _triangle.UVs[2]);
        
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