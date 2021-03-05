using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;

[UpdateAfter(typeof(VFXFlyForwardSystem))]
public class LifeDistanceSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    struct DistanceJob : IJobForEachWithEntity<MoveSpeed, DistanceRecord>
    {
        public EntityCommandBuffer.Concurrent concurrent;
        public float deltaTime;

        public void Execute(Entity entity, int index, ref MoveSpeed moveSpeed, ref DistanceRecord record)
        {
            record.Time += deltaTime;
            record.Distance = moveSpeed.Value * record.Time;
            if (record.Distance >= 200f)
            {
                Debug.Log(string.Format("移除,飞行距离 {0}: ", record.Distance));
                concurrent.DestroyEntity(index, entity); 
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        DistanceJob job = new DistanceJob()
        {
            concurrent = bufferSystem.CreateCommandBuffer().ToConcurrent(),
            deltaTime = Time.DeltaTime,
        };

        JobHandle handle = job.Schedule(this, inputDeps);
        bufferSystem.AddJobHandleForProducer(handle);

        return handle;
    }
}
