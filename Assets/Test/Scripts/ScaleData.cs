using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

[GenerateAuthoringComponent]
public struct ScaleData : IComponentData
{
    public float Value;
}
