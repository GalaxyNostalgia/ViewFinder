using System.Collections.Generic;
using UnityEngine;

public class MeshTriangle
{
    public List<Vector3> Vertices { get; } = new List<Vector3>();
    public List<Vector3> Normals { get; } = new List<Vector3>();
    public List<Vector2> UVs { get; } = new List<Vector2>();
    public int SubmeshIndex { get; }

    public MeshTriangle(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int submeshIndex)
    {
        Vertices.AddRange(vertices);
        Normals.AddRange(normals);
        UVs.AddRange(uvs);
        SubmeshIndex = submeshIndex;
    }
}
