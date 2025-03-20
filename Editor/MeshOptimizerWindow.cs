using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshOptimizerWindow : EditorWindow
{
    private Mesh selectedMesh;

    [MenuItem("Tools/Mesh Optimizer")]
    public static void ShowWindow()
    {
        GetWindow<MeshOptimizerWindow>("Mesh Optimizer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Optimize Mesh", EditorStyles.boldLabel);
        selectedMesh = (Mesh)EditorGUILayout.ObjectField("Mesh to Optimize", selectedMesh, typeof(Mesh), false);

        if (selectedMesh == null)
        {
            EditorGUILayout.HelpBox("Select a Mesh asset to optimize.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Optimize & Save"))
        {
            Mesh optimizedMesh = OptimizeMesh(selectedMesh);
            SaveOptimizedMesh(selectedMesh, optimizedMesh);
        }
    }

  
    private Mesh OptimizeMesh(Mesh originalMesh)
    {
        if (originalMesh.vertexCount == 0 || originalMesh.tangents.Length == 0)
        {
            Debug.LogError("Mesh is missing required data! Can't optimize.");
            return null;
        }

        int vertexCount = originalMesh.vertexCount;
        int subMeshCount = originalMesh.subMeshCount;

        var positions = originalMesh.vertices;
        var normals = originalMesh.normals;
        var tangents = originalMesh.tangents;

        var packedPositions = PackPositions(positions, vertexCount);
        var stream0Data = PackStream0Data(normals, tangents, originalMesh.uv, vertexCount);

        Mesh optimizedMesh = new Mesh
        {
            name = originalMesh.name + "_Optimized",
            indexFormat = originalMesh.indexFormat,
            subMeshCount = subMeshCount
        };

        var vertexLayout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.SNorm16, 4, 1),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.SNorm16, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2)
        };

        optimizedMesh.SetVertexBufferParams(vertexCount, vertexLayout);
        optimizedMesh.SetVertexBufferData(packedPositions, 0, 0, vertexCount, 1, MeshUpdateFlags.DontRecalculateBounds);
        optimizedMesh.SetVertexBufferData(stream0Data, 0, 0, vertexCount, 0, MeshUpdateFlags.DontRecalculateBounds);

        var allIndices = originalMesh.triangles.Select(i => (ushort)i).ToArray();
        optimizedMesh.SetIndexBufferParams(allIndices.Length, originalMesh.indexFormat);
        optimizedMesh.SetIndexBufferData(allIndices, 0, 0, allIndices.Length, MeshUpdateFlags.DontRecalculateBounds);

        for (int i = 0; i < subMeshCount; i++)
        {
            optimizedMesh.SetSubMesh(i, originalMesh.GetSubMesh(i));
        }

        optimizedMesh.bounds = originalMesh.bounds;

        packedPositions.Dispose();
        stream0Data.Dispose();

        return optimizedMesh;
    }

    
    private void SaveOptimizedMesh(Mesh originalMesh, Mesh optimizedMesh)
    {
        string originalPath = AssetDatabase.GetAssetPath(originalMesh);
        if (string.IsNullOrEmpty(originalPath))
        {
            Debug.LogError("Original mesh is not an asset! Cannot determine save path.");
            return;
        }

        string directory = Path.GetDirectoryName(originalPath);
        string optimizedPath = Path.Combine(directory, optimizedMesh.name + ".asset");

        AssetDatabase.CreateAsset(optimizedMesh, optimizedPath);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Success", "Optimized mesh saved to Assets!", "OK");
    }

   
    private NativeArray<SNorm16Vector> PackPositions(Vector3[] positions, int vertexCount)
    {
        const float multiplier = 64.0f;
        var packedPositions = new NativeArray<SNorm16Vector>(vertexCount, Allocator.Temp);

        for (int i = 0; i < vertexCount; i++)
        {
            Vector3 pos = positions[i];
            float magnitude = pos.magnitude;

            float smallestDivision = Mathf.Max(Mathf.Floor(magnitude / multiplier) * multiplier, multiplier);
            float highestDivision = Mathf.Max(Mathf.Ceil(magnitude / multiplier) * multiplier, multiplier);

            Vector3 pos1 = pos / smallestDivision;
            Vector3 pos2 = pos / highestDivision;

            bool usePos1 = (pos2 - pos).sqrMagnitude > (pos1 - pos).sqrMagnitude;
            Vector3 newPos = usePos1 ? pos1 : pos2;
            short bestDivider = VertexCompressionUtils.FloatToSnorm16((usePos1 ? smallestDivision : highestDivision) / multiplier);

            packedPositions[i] = new SNorm16Vector(
                VertexCompressionUtils.FloatToSnorm16(newPos.x),
                VertexCompressionUtils.FloatToSnorm16(newPos.y),
                VertexCompressionUtils.FloatToSnorm16(newPos.z),
                bestDivider
            );
        }

        return packedPositions;
    }

  
    private NativeArray<MeshStream0DataQTangent> PackStream0Data(Vector3[] normals, Vector4[] tangents, Vector2[] uvs, int vertexCount)
    {
        var stream0Data = new NativeArray<MeshStream0DataQTangent>(vertexCount, Allocator.Temp);

        for (var i = 0; i < vertexCount; i++)
        {
            var packedQuaternion = VertexCompressionUtils.PackTangentFrame(normals[i], tangents[i]);

            var packedTangent = new short4(
                VertexCompressionUtils.FloatToSnorm16(packedQuaternion.x),
                VertexCompressionUtils.FloatToSnorm16(packedQuaternion.y),
                VertexCompressionUtils.FloatToSnorm16(packedQuaternion.z),
                VertexCompressionUtils.FloatToSnorm16(packedQuaternion.w)
            );

            var packedUV = new half2((half)uvs[i].x, (half)uvs[i].y);
            stream0Data[i] = new MeshStream0DataQTangent { qTangent = packedTangent, uv = packedUV };
        }
        return stream0Data;
    }
}
