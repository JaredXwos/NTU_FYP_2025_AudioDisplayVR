using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollidingComponent : CoreComponent
{
    private static Mesh cubeMesh;
    [SerializeField] protected Material cubeMaterial;

    private readonly Volatile<Matrix4x4[][]> body = new();
    protected HashSet<Vector3Int> targetBody = new();

    #region Monobehaviour
    protected override void Awake()
    {
        base.Awake();
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubeMesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Destroy(temp);
    }
    #endregion

    #region CoreComponent
    protected override (string name, Func<object> binding)[] Bindings => new (string name, Func<object> binding)[0];
    #endregion

    protected bool AttemptUpdate()
    {
        if (World.CheckCollision(targetBody)) return false;

        World.TryDeregister(targetBody, this);
        World.TryRegister(targetBody, this);

        Matrix4x4[] allMatrices = targetBody
            .Select(pos => Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one))
            .ToArray();

        int total = allMatrices.Length;
        int batchCount = (total + 1022) / 1023;

        Matrix4x4[][] batches = new Matrix4x4[batchCount][];
        for (int i = 0; i < batchCount; i++)
        {
            int length = Math.Min(1023, total - i * 1023);
            batches[i] = new Matrix4x4[length];
            Array.Copy(allMatrices, i * 1023, batches[i], 0, length);
        }

        body.Value = batches;
        return true;
    }

    protected void Render()
    {
        foreach(Matrix4x4[] batch in body.Value)
            Graphics.DrawMeshInstanced(cubeMesh, 0, cubeMaterial, batch);
    }
    

    public static Vector3Int RotateX90CW(Vector3Int v) => new(v.x, -v.z, v.y);
    public static Vector3Int RotateX180(Vector3Int v) => new(v.x, -v.y, -v.z);
    public static Vector3Int RotateX90CCW(Vector3Int v) => new(v.x, v.z, -v.y);
    public static Vector3Int RotateY90CW(Vector3Int v) => new(-v.z, v.y, v.x);
    public static Vector3Int RotateY180(Vector3Int v) => new(-v.x, v.y, -v.z);
    public static Vector3Int RotateY90CCW(Vector3Int v) => new(v.z, v.y, -v.x);
    public static Vector3Int RotateZ90CW(Vector3Int v) => new(v.y, -v.x, v.z);
    public static Vector3Int RotateZ180(Vector3Int v) => new(-v.x, -v.y, v.z);
    public static Vector3Int RotateZ90CCW(Vector3Int v) => new(-v.y, v.x, v.z);

}