using Unity.Entities;
using Unity.Mathematics;

public struct AnimationBlobAsset
{
    public float frameDelta;
    public float frameCount;

    public BlobArray<float3> positions;
    public BlobArray<float3> rotations;
    public BlobArray<float3> scales;
};
