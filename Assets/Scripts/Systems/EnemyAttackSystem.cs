using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;

[DisableAutoCreation]
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
    struct AttackJob : IJobForEachWithEntity<EnemyState, Translation, Rotation>
    {
        public float deltaTime;
        public Entity bulletEntity;

        public void Execute(Entity entity, int index, ref EnemyState state, ref Translation pos, ref Rotation rot)
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
            deltaTime = Time.DeltaTime,
            bulletEntity = GameWorld.GetInstance().EnemyBullet,
        };*/

        /*var commandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent();

        var job = Entities.ForEach((Entity entity, int entityInQueryIndex, ref EnemyState state) =>
            {
                if (state.BehaviourState != EnemyBehaviourState.Attack)
                {

                }

                state.aimTime += 0.016f;
                if (state.aimTime >= EnemyStateTime.AttackDurationValue)
                {
                    state.aimTime = 0;
                    //todo Attack
                }
                commandBuffer.Instantiate(entityInQueryIndex, entity);
            }).Schedule(inputDeps);*/

        return default;
    }
}
