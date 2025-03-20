using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public struct short4
{
    public short x, y, z, w;
    public short4(short x, short y, short z, short w) => (this.x, this.y, this.z, this.w) = (x, y, z, w);
}

[StructLayout(LayoutKind.Sequential)]
public struct SNorm16Vector
{
    public short x, y, z, w;
    public SNorm16Vector(short x, short y, short z, short w) => (this.x, this.y, this.z, this.w) = (x, y, z, w);
}

[StructLayout(LayoutKind.Sequential)]
public struct UNorm16Vector
{
    public ushort x, y, z, w;
    public UNorm16Vector(ushort x, ushort y, ushort z, ushort w) => (this.x, this.y, this.z, this.w) = (x, y, z, w);
}

[StructLayout(LayoutKind.Sequential)]
public struct MeshStream0DataQTangent
{
    public short4 qTangent;
    public half2 uv;
}

// Structure representing the compressed vertex
struct CompressedVertex
{
    public SNorm16Vector pos;     // Position (SNorm16 x4)
    public SNorm16Vector qTangent;  // Normal (SNorm16 x4)
    public Vector2 uv;            // UV0 (Float32 x2)
}

public static class VertexCompressionUtils
{
    //QTangent
    public static Quaternion PackTangentFrame(Vector3 normal, Vector4 tangent)
    {
        Vector3 bitangent = Vector3.Cross(normal, tangent);
        
        //base
        Quaternion quaternion = Quaternion.LookRotation(normal, bitangent);
        quaternion.Normalize();
        quaternion = PositiveNormalize(quaternion);
        
        //calc bias: get max value for 16bits - 32767 and divide to 1 - this is min w
        //but q need to be normalized -  sum of quads = 1-bias^2
        //and then xyz mul by this factor
        const float bias = 1.0f / 32767.0f; //SNORM16
        if (quaternion.w < bias)
        {
            quaternion.w = bias;
            float factor = Mathf.Sqrt(1.0f - bias * bias);
            quaternion.x *= factor;
            quaternion.y *= factor;
            quaternion.z *= factor;
        }

        // b vec by sign of tangent w
        //because we dont know btangent
        Vector3 b = (tangent.w > 0) ? Vector3.Cross(tangent, normal) : Vector3.Cross(normal, tangent);
        
        //??? reflect* why?
        Vector4 c = Vector3.Cross(tangent, normal);
        if (Vector3.Dot(c, b) < 0)
        {
            quaternion = new Quaternion(-quaternion.x, -quaternion.y, -quaternion.z, -quaternion.w);
        }
        return (quaternion);
    }

   
    ///<summary>
    /// Normalise quaternion with positive w only
    /// </summary>
    private static Quaternion PositiveNormalize(Quaternion q)
    {
        if (q.w < 0)
        {
            q.x = -q.x;
            q.y = -q.y;
            q.z = -q.z;
            q.w = -q.w;
        }
        return q;
    }
    
    
    public static short FloatToSnorm16(float v)
    {
        float scaled = Mathf.Clamp(v, -1.0f, 1.0f) * 32767.0f;
        return (short)(Mathf.RoundToInt(scaled));
    }
    
    public static ushort FloatToUnorm16(float v)
    {
        float scaled = Mathf.Clamp(v, 0.0f, 1.0f) * 65535.0f;
        return (ushort)(Mathf.RoundToInt(scaled));
    }
    
    
}
