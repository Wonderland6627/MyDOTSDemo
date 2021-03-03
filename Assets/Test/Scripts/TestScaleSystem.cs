using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

[DisableAutoCreation]
public class TestScaleSystem : JobComponentSystem
{
    /// <summary>
    /// 筛选 查找所有带有设定的Entity 所以一开始Scale不好用就是因为压根没有Scale属性
    /// </summary>
    private EntityQuery entityQuery;

    protected override void OnCreate()
    {
        //entityQuery = GetEntityQuery(typeof(Scale), ComponentType.ReadOnly<ScaleData>());
        entityQuery = GetEntityQuery(typeof(ScalePivot),ComponentType.ReadOnly<ScaleData>());
    }

    [BurstCompile]
    struct ScaleJob : IJobChunk
    {
        public float dt;
        
        public ArchetypeChunkComponentType<CompositeScale> scaleType;
        [ReadOnly] public ArchetypeChunkComponentType<ScaleData> scaleDataType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var scaleArray = chunk.GetNativeArray(scaleType);
            var scaleDataArray = chunk.GetNativeArray(scaleDataType);

            for (int i = 0; i < chunk.Count; i++)
            {
                scaleArray[i] = new CompositeScale()
                {
                    Value = scaleArray[i].Value * scaleDataArray[i].Value
                };
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Debug.Log("111");
        ScaleJob scaleJob = new ScaleJob()
        {
            dt = Time.DeltaTime,
            scaleType = GetArchetypeChunkComponentType<CompositeScale>(),
            scaleDataType = GetArchetypeChunkComponentType<ScaleData>(true),
        };

        Debug.Log(scaleJob);

        return scaleJob.Schedule(entityQuery, inputDeps);

        return default;
    }
}
