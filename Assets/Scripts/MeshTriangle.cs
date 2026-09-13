using UnityEngine;
using System.Collections.Generic;


public class MeshTriangle
{
    private int submeshIndex;

    public List<Vector3> Vertices { get; set; } = new();
    public List<Vector3> Normals { get; set; } = new();
    public List<Vector2> UVs { get; set; } = new();

    public int SubmeshIndex
    {
        get => submeshIndex;
        private set => SubmeshIndex = value;
    }
    
    public MeshTriangle(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int submeshIndex)
    {
        Clear();
        Vertices.AddRange(vertices);
        Normals.AddRange(normals);
        UVs.AddRange(uvs);
        this.submeshIndex = submeshIndex;
    }

    private void Clear()
    {
        Vertices.Clear();
        Normals.Clear();
        UVs.Clear();
        
        submeshIndex = 0;
    }
}
