using System.Collections.Generic;
using UnityEngine;

public static class Cutter
{
    private static bool isBusy;
    private static Mesh originalMesh;
    private static Vector3[] originalVertices;
    private static Vector3[] originalNormals;
    private static Vector2[] originalUVs;

    public static GameObject Cut(GameObject originalGameObject, Vector3 contactPoint, Vector3 cutNormal)
    {
        if (isBusy)
        {
            return null;
        }

        isBusy = true;

        try
        {
            var originalFilter = originalGameObject.GetComponent<MeshFilter>();
            var originalRenderer = originalGameObject.GetComponent<MeshRenderer>();
            if (originalFilter == null || originalRenderer == null)
            {
                Debug.LogWarning($"{originalGameObject.name} has no MeshFilter/MeshRenderer to cut", originalGameObject);
                return null;
            }

            Vector3 localNormal = originalGameObject.transform.localToWorldMatrix.transpose
                .MultiplyVector(-cutNormal).normalized;

            Plane cutPlane = new Plane(localNormal,
                originalGameObject.transform.InverseTransformPoint(contactPoint));
            originalMesh = originalFilter.mesh;

            if (originalMesh == null)
            {
                Debug.Log("No Mesh to cut");
                return null;
            }

            originalVertices = originalMesh.vertices;
            originalNormals = originalMesh.normals;
            originalUVs = originalMesh.uv;

            List<Vector3> addedVertices = new List<Vector3>();
            GeneratedMesh leftMesh = new GeneratedMesh();
            GeneratedMesh rightMesh = new GeneratedMesh();

            SeparateMeshes(leftMesh, rightMesh, cutPlane, addedVertices);

            FillCut(addedVertices, cutPlane, leftMesh, rightMesh);

            if (leftMesh.Vertices.Count == 0 || rightMesh.Vertices.Count == 0)
            {
                return null;
            }

            Mesh finishedLeftMesh = leftMesh.GetGeneratedMesh();
            Mesh finishedRightMesh = rightMesh.GetGeneratedMesh();

            var originalCols = originalGameObject.GetComponents<Collider>();
            foreach (var col in originalCols)
            {
                Object.Destroy(col);
            }

            originalFilter.mesh = finishedLeftMesh;
            var collider = originalGameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = finishedLeftMesh;
            collider.convex = true;

            var mat = originalRenderer.material;
            Material[] mats = new Material[finishedLeftMesh.subMeshCount];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = mat;
            }

            originalRenderer.materials = mats;
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

            Vector3[] vertices = { _vertices[i], _vertices[(i + 1) % _vertices.Count], centerPosition };
            Vector2[] uvs = { uv1, uv2, new Vector2(.5f, .5f) };

            AddOriented(_leftMesh, vertices,
                new[] { -_plane.normal, -_plane.normal, -_plane.normal }, uvs, originalMesh.subMeshCount);

            AddOriented(_rightMesh, vertices,
                new[] { _plane.normal, _plane.normal, _plane.normal }, uvs, originalMesh.subMeshCount);
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

                bool triangleALeftSide = plane.GetSide(originalVertices[triangleIndexA]);
                bool triangleBLeftSide = plane.GetSide(originalVertices[triangleIndexB]);
                bool triangleCLeftSide = plane.GetSide(originalVertices[triangleIndexC]);

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
        bool[] leftSide = { triangleALeftSide, triangleBLeftSide, triangleCLeftSide };

        var left = new MeshTriangle(new Vector3[2], new Vector3[2], new Vector2[2], triangle.SubmeshIndex);
        var right = new MeshTriangle(new Vector3[2], new Vector3[2], new Vector2[2], triangle.SubmeshIndex);

        bool hasLeft = false;
        bool hasRight = false;

        for (int i = 0; i < 3; i++)
        {
            if (leftSide[i])
                Collect(left, triangle, i, ref hasLeft);
            else
                Collect(right, triangle, i, ref hasRight);
        }

        Vector3 vertLeft = Split(plane, left, right, 0, out Vector3 normalLeft, out Vector2 uvLeft);
        addedVertices.Add(vertLeft);

        Vector3 vertRight = Split(plane, left, right, 1, out Vector3 normalRight, out Vector2 uvRight);
        addedVertices.Add(vertRight);

        Emit(leftMesh, triangle.SubmeshIndex,
            left.Vertices[0], vertLeft, vertRight,
            left.Normals[0], normalLeft, normalRight,
            left.UVs[0], uvLeft, uvRight);

        Emit(leftMesh, triangle.SubmeshIndex,
            left.Vertices[0], left.Vertices[1], vertRight,
            left.Normals[0], left.Normals[1], normalRight,
            left.UVs[0], left.UVs[1], uvRight);

        Emit(rightMesh, triangle.SubmeshIndex,
            right.Vertices[0], vertLeft, vertRight,
            right.Normals[0], normalLeft, normalRight,
            right.UVs[0], uvLeft, uvRight);

        Emit(rightMesh, triangle.SubmeshIndex,
            right.Vertices[0], right.Vertices[1], vertRight,
            right.Normals[0], right.Normals[1], normalRight,
            right.UVs[0], right.UVs[1], uvRight);
    }

    private static void Collect(MeshTriangle target, MeshTriangle source, int index, ref bool seen)
    {
        if (!seen)
        {
            seen = true;
            target.Vertices[0] = target.Vertices[1] = source.Vertices[index];
            target.Normals[0] = target.Normals[1] = source.Normals[index];
            target.UVs[0] = target.UVs[1] = source.UVs[index];
            return;
        }

        target.Vertices[1] = source.Vertices[index];
        target.Normals[1] = source.Normals[index];
        target.UVs[1] = source.UVs[index];
    }

    private static Vector3 Split(Plane plane, MeshTriangle left, MeshTriangle right, int index,
        out Vector3 normal, out Vector2 uv)
    {
        Vector3 from = left.Vertices[index];
        Vector3 to = right.Vertices[index];

        plane.Raycast(new Ray(from, (to - from).normalized), out float distance);
        float t = distance / (to - from).magnitude;

        normal = Vector3.Lerp(left.Normals[index], right.Normals[index], t);
        uv = Vector2.Lerp(left.UVs[index], right.UVs[index], t);

        return Vector3.Lerp(from, to, t);
    }

    private static void Emit(GeneratedMesh mesh, int submeshIndex,
        Vector3 v0, Vector3 v1, Vector3 v2,
        Vector3 n0, Vector3 n1, Vector3 n2,
        Vector2 uv0, Vector2 uv1, Vector2 uv2)
    {
        if (v0 == v1 || v0 == v2)
            return;

        AddOriented(mesh, new[] { v0, v1, v2 }, new[] { n0, n1, n2 }, new[] { uv0, uv1, uv2 }, submeshIndex);
    }

    private static void AddOriented(GeneratedMesh mesh, Vector3[] vertices, Vector3[] normals,
        Vector2[] uvs, int submeshIndex)
    {
        var triangle = new MeshTriangle(vertices, normals, uvs, submeshIndex);

        if (Vector3.Dot(Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]), normals[0]) < 0)
            FlipTriangle(triangle);

        mesh.AddTriangle(triangle);
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
            originalVertices[triangleIndexA],
            originalVertices[triangleIndexB],
            originalVertices[triangleIndexC]
        };

        Vector3[] normalsToAdd =
        {
            originalNormals[triangleIndexA],
            originalNormals[triangleIndexB],
            originalNormals[triangleIndexC]
        };

        Vector2[] uvsToAdd =
        {
            originalUVs[triangleIndexA],
            originalUVs[triangleIndexB],
            originalUVs[triangleIndexC]
        };

        return new MeshTriangle(verticesToAdd, normalsToAdd, uvsToAdd, submeshIndex);
    }
}
