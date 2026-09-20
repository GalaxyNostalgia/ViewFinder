using System.Collections.Generic;
using UnityEngine;

public class GeneratedMesh
{
    public List<Vector3> Vertices { get; } = new List<Vector3>();
    public List<Vector3> Normals { get; } = new List<Vector3>();
    public List<Vector2> UVs { get; } = new List<Vector2>();
    public List<List<int>> SubmeshIndices { get; } = new List<List<int>>();

    public void AddTriangle(MeshTriangle triangle)
    {
        int currentVertexCount = Vertices.Count;

        Vertices.AddRange(triangle.Vertices);
        Normals.AddRange(triangle.Normals);
        UVs.AddRange(triangle.UVs);

        while (SubmeshIndices.Count < triangle.SubmeshIndex + 1)
            SubmeshIndices.Add(new List<int>());

        for (int i = 0; i < 3; i++)
            SubmeshIndices[triangle.SubmeshIndex].Add(currentVertexCount + i);
    }

    public Mesh GetGeneratedMesh()
    {
        var mesh = new Mesh();
        mesh.SetVertices(Vertices);
        mesh.SetNormals(Normals);
        mesh.SetUVs(0, UVs);
        mesh.SetUVs(1, UVs);

        mesh.subMeshCount = SubmeshIndices.Count;

        for (int i = 0; i < SubmeshIndices.Count; i++)
            mesh.SetTriangles(SubmeshIndices[i], i);

        return mesh;
    }
}
