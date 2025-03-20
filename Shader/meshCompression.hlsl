#ifndef MESH_COMPRESSION
#define MESH_COMPRESSION

#define multiplier 64.0

//upack qTangent
void toTangentFrame(half4 q, out half3 n, out half3 t, out half3 b)
{
    n = half3(0.0, 0.0, 1.0) +
        half3(2.0, -2.0, -2.0) * q.x * q.zwx +
        half3(2.0, 2.0, -2.0) * q.y * q.wzy;


    t = half3(1.0, 0.0, 0.0) +
        half3(-2.0, 2.0, -2.0) * q.y * q.yxw +
        half3(-2.0, 2.0, 2.0) * q.z * q.zwx;

    float s = sign(q.w);
    b = cross(n, t) * s;
}

//unpack SNorm16 position
void unpackPositionSNorm16(float4 packedPosition, out float3 position)
{
    position = packedPosition.xyz * (packedPosition.w * multiplier);
}

#endif
