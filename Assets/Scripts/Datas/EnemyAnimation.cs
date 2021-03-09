using Unity.Entities;
using Unity.Mathematics;

public struct EnemyAnimation : IComponentData
{
    public BlobAssetReference<AnimationBlobAsset> animationBlobRef;
    public float timer;
    public int frame;
    public float3 localPosition;
}
