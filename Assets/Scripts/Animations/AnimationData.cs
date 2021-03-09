using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StreamingAssets/AnimationData")]
public class AnimationData : ScriptableObject
{
    public float frameDelta;
    public int frameCount;

    public List<Vector3> positions;
    public List<Vector3> rotations;
    public List<Vector3> scales;
}
