using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class EntityHealthSystem : JobComponentSystem
{
    //1 ComponentSystem方式管理生命值 70fps左右
    /*protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref EntityHealth health) =>
        {
            if (health.Value <= 0)
            {
                if (EntityManager.HasComponent(entity, typeof(EnemyTag)))
                {
                    PostUpdateCommands.DestroyEntity(entity);
                }
            }
        });
    }*/

    private BeginInitializationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        bufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    [BurstCompile(CompileSynchronously = true)]
    struct DestoryEntityJob : IJobForEachWithEntity<EntityHealth>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, ref EntityHealth health)
        {
            if (health.Value <= 0)
            {
                CommandBuffer.DestroyEntity(index, entity);
            }
        }
    }

    /// <summary>
    /// 2 JobComponentSystem 实现生命管理 fps翻了一倍
    /// </summary>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        DestoryEntityJob job = new DestoryEntityJob
        {
            CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent(),
        };

        job.Schedule(this, inputDeps).Complete();

        return default;
    }
}
