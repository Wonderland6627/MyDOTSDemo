using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class RotateSystem : JobComponentSystem
{
    private EntityQuery entityQuery;

    protected override void OnCreate()
    {
        entityQuery = GetEntityQuery(typeof(Rotation), ComponentType.ReadOnly<RotateSpeed>());
    }

    [BurstCompile]
    struct RotateJob : IJobChunk
    {
        public float dt;
        public ArchetypeChunkComponentType<Rotation> rotationType;
        [ReadOnly] public ArchetypeChunkComponentType<RotateSpeed> rotateSpeedType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var rotationArray = chunk.GetNativeArray(rotationType);
            var rotationSpeedArray = chunk.GetNativeArray(rotateSpeedType);

            for (int i = 0; i < chunk.Count; i++)
            {
                rotationArray[i] = new Rotation
                {
                    Value = math.mul(rotationArray[i].Value, quaternion.AxisAngle(math.up(), rotationSpeedArray[i].Value * dt))
                };
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        RotateJob job = new RotateJob
        {
            dt = Time.DeltaTime,
            rotationType = GetArchetypeChunkComponentType<Rotation>(),
            rotateSpeedType = GetArchetypeChunkComponentType<RotateSpeed>()
        };

        return job.Schedule(entityQuery, inputDeps);

        /*Entities
            .WithoutBurst()
            .ForEach((ref RotateSpeed rotationSpeed, ref Rotation rotation) =>
        {
            var deltaTime = Time.DeltaTime;

            //旋转Cube，代码逻辑不用管，这里可以是其他任何逻辑
            rotation.Value = math.mul(math.normalize(rotation.Value),
                quaternion.AxisAngle(math.up(), rotationSpeed.Value * deltaTime));
        }).Run();

        return default;*/
    }
}
