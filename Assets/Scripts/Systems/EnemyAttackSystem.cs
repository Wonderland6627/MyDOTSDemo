using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;

public class EnemyAttackSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        bufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    struct AnimationJob : IJobForEach<EnemyAnimation, NonUniformScale>
    {
        public float deltaTime;

        public void Execute(ref EnemyAnimation animation, ref NonUniformScale scale)
        {
            ref AnimationBlobAsset blob = ref animation.animationBlobRef.Value;

            animation.timer += deltaTime;
            if (animation.timer < blob.frameDelta)
            {
                return;
            }

            while (animation.timer > blob.frameDelta)
            {
                animation.timer -= blob.frameDelta;
                animation.frame = (animation.frame + 1) % (int)blob.frameCount;
            }

            animation.localPosition = blob.positions[animation.frame];
            scale.Value = blob.scales[animation.frame];
        }
    }

    [BurstCompile]
    struct AttackJob : IJobForEachWithEntity<EnemyState, EnemyBulletSpawnerData, Translation, Rotation>
    {
        public EntityCommandBuffer.Concurrent concurrent;
        public float deltaTime;
        public Entity bulletEntity;

        public void Execute(Entity entity, int index, ref EnemyState state, ref EnemyBulletSpawnerData data, ref Translation pos, ref Rotation rot)
        {
            if (state.BehaviourState != EnemyBehaviourState.Attack)
            {
                return;
            }

            state.aimTime += deltaTime;
            if (state.aimTime >= EnemyStateTime.AttackDurationValue)
            {
                state.aimTime = 0;
                //todo Attack
                var instance = concurrent.Instantiate(index, data.bulletPrefab);
                concurrent.DestroyEntity(index, entity);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        /*AnimationJob job = new AnimationJob()
        {
            deltaTime = UnityEngine.Time.deltaTime,
        };

        return job.Schedule(this, inputDeps);*/

        /*AttackJob attackJob = new AttackJob()
        {
            concurrent = bufferSystem.CreateCommandBuffer().ToConcurrent(),
            deltaTime = Time.DeltaTime,
            bulletEntity = GameWorld.GetInstance().EnemyBullet,
        };*/

        var commandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent();

        var job = Entities.ForEach((Entity entity, int entityInQueryIndex, ref EnemyState state, in Translation pos, in Rotation rot, in EnemyBulletSpawnerData data) =>
            {
                if (state.BehaviourState == EnemyBehaviourState.Attack)
                {
                    state.aimTime += 0.016f;
                    if (state.aimTime >= EnemyStateTime.AttackDurationValue)
                    {
                        state.aimTime = 0;
                        //todo Attack
                        var instance = commandBuffer.Instantiate(entityInQueryIndex, data.bulletPrefab);//通过BufferSystem在Job中添加一个任务，下次执行InitSystem时候检查是否存在任务，存在就执行
                        commandBuffer.SetComponent(entityInQueryIndex, instance, new Translation() { Value = pos.Value + new float3(0,1,0) });
                        commandBuffer.SetComponent(entityInQueryIndex, instance, new Rotation() { Value = rot.Value });
                    }
                }              
            })
            .Schedule(inputDeps);

        bufferSystem.AddJobHandleForProducer(job);

        return job;
    }
}
