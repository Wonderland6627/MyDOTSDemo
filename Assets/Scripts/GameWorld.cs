using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public class GameWorld : MonoBehaviour
{
    #region Instance

    private static GameWorld _instance = null;
    public static GameWorld GetInstance()
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType(typeof(GameWorld)) as GameWorld;
            if (_instance == null)
            {
                var go = new GameObject("GameWorld");
                _instance = go.AddComponent<GameWorld>();
            }

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(_instance.gameObject);
            }
        }

        return _instance;
    }

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
    #endregion

    private UnitCharacter player;
    public UnitCharacter Player
    {
        get
        {
            if (player == null)
            {
                var character = FindObjectOfType<UnitCharacter>();
                if (character)
                {
                    player = character;
                }
            }

            return player;
        }
    }
    public Entity CurrentWeapon;

    private GameObject enemyPrefab;
    private GameObject weaponPrefab;

    private Entity enemyEntity;
    private Entity weaponEntity;

    private EntityManager entityManager;
    private GameObjectConversionSettings settings;

    private List<Entity> entitiesList = new List<Entity>();
    private BlobAssetReference<AnimationBlobAsset> animationBlob;

    public void Init()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);

        enemyPrefab = Resources.Load<GameObject>("Prefabs/Enemy 1");
        weaponPrefab = Resources.Load<GameObject>("Prefabs/Sword");

        enemyEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(enemyPrefab, settings);
        weaponEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(weaponPrefab, settings);

        Player.Init();

        InitBlobAsset();
        for (int i = 0; i < 3000; i++)
        {
            CreateEnemyEntity();
        }
        CurrentWeapon = CreateWeaponEntity();
    }

    /// <summary>
    /// 创建一个武器实体 并为其绑定WeaponTag
    /// </summary>
    private Entity CreateWeaponEntity()
    {
        Entity weapon = entityManager.Instantiate(weaponEntity);
        entityManager.SetName(weapon, "PlayerWeapon");
        entityManager.SetComponentData(weapon, new Translation { Value = Player.weaponPos.position });
        WeaponState weaponState = new WeaponState() { isAttacking = false };
        entityManager.AddComponentData(weapon, weaponState);

        return weapon;
    }

    public Entity CreateEnemyEntity()
    {
        float randX = UnityEngine.Random.Range(0, 1024f);
        float randZ = UnityEngine.Random.Range(0, 1024f);
        Vector3 randomPos = new Vector3(randX, 0, randZ);

        Entity enemy = entityManager.Instantiate(enemyEntity);
        entityManager.SetComponentData(enemy, new Translation() { Value = randomPos });
        entityManager.SetComponentData(enemy, new Rotation() { Value = quaternion.LookRotation(-randomPos,math.up()) }); ;

        EnemyState state = new EnemyState()
        {
            Duration = UnityEngine.Random.Range(1.8f, 2.2f),
            stateTime = UnityEngine.Random.Range(1.9f, 2.1f),
            moveWaitTime = 0,
            moveStartTime = UnityEngine.Random.Range(2f, 10f),
            BehaviourState = EnemyBehaviourState.Idle,
        };
        entityManager.AddComponentData(enemy, state);

        EnemyAnimation animation = new EnemyAnimation()
        {
            animationBlobRef = animationBlob,
            timer = 0,
            frame = 0,
        };
        entityManager.AddComponentData(enemy, animation);

        return enemy;
    }

    public Entity CreateSkillVfxEntity(string goPrefabName, Transform startPos, Quaternion startQuaterion)
    {
        GameObject vfxPrefab = Resources.Load<GameObject>("Prefabs/" + goPrefabName);
        if (vfxPrefab == null)
        {
            return Entity.Null;
        }

        var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(vfxPrefab, settings);
        var skillVfx = entityManager.Instantiate(entity);
        entityManager.SetName(skillVfx, goPrefabName);
        entityManager.SetComponentData(skillVfx, new Translation() { Value = startPos.position });
        entityManager.SetComponentData(skillVfx, new Rotation() { Value = startQuaterion });

        return skillVfx;
    }

    public void CreateSkillVfxEntities(int count, string goPrefabName, Transform startPos, Quaternion startQuarerion)
    {
        NativeArray<Entity> vfxArray = new NativeArray<Entity>(count, Allocator.TempJob);
        entityManager.Instantiate(CreateSkillVfxEntity(goPrefabName, startPos, startQuarerion), vfxArray);

        int max = count / 2;
        int min = -max;
        int index = 0;
        Vector3 rotation = startQuarerion.eulerAngles;
        for (int i = min; i < max; i++)
        {
            rotation.y = (rotation.y + 3f * i) % 360f;
            entityManager.SetComponentData(vfxArray[index], new Rotation { Value = Quaternion.Euler(rotation) });

            index++;
        }

        vfxArray.Dispose();
    }

    public string GetEntityName(Entity entity)
    {
        return entity == null ? "EntityNull" : entityManager.GetName(entity);
    }

    public Vector3 GetCurrentWeaponPos()
    {
        if (CurrentWeapon == null)
        {
            return Player.transform.position;
        }

        Translation translation = entityManager.GetComponentData<Translation>(CurrentWeapon);
        return translation.Value;
    }

    public void SetCurrentWeaponAttackState(bool isAttaking)
    {
        if (CurrentWeapon == null)
        {
            return;
        }

        WeaponState weaponState = entityManager.GetComponentData<WeaponState>(CurrentWeapon);
        weaponState.isAttacking = isAttaking;
        entityManager.SetComponentData<WeaponState>(CurrentWeapon, weaponState);
    }

    /// <summary>
    /// 初始化BlobAsset
    /// </summary>
    private void InitBlobAsset()
    {
        AnimationData data = Resources.Load<AnimationData>("AnimationDatas/Test2") as AnimationData;
        using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
        {
            ref AnimationBlobAsset asset = ref blobBuilder.ConstructRoot<AnimationBlobAsset>();
            BlobBuilderArray<float3> positions = blobBuilder.Allocate(ref asset.positions, data.frameCount);
            asset.frameDelta = data.frameDelta;
            asset.frameCount = data.frameCount;

            for (int i = 0; i < data.frameCount; i++)
            {
                positions[i] = new float3(data.positions[i]);
            }

            animationBlob = blobBuilder.CreateBlobAssetReference<AnimationBlobAsset>(Allocator.Persistent);
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(20);

        NativeArray<Entity> allEntities = entityManager.GetAllEntities();
        GUILayout.Label("Entities Count: " + allEntities.Length);

        /*if (GUILayout.Button("Idle"))
        {
            for (int i = 0; i < allEntities.Length; i++)
            {
                if (entityManager.HasComponent<EnemyState>(allEntities[i]))
                {
                    var state = entityManager.GetComponentData<EnemyState>(allEntities[i]);
                    state.BehaviourState = EnemyBehaviourState.Idle;
                    entityManager.SetComponentData<EnemyState>(allEntities[i], state);
                }
            }
        }

        if (GUILayout.Button("Move"))
        {
            for (int i = 0; i < allEntities.Length; i++)
            {
                if (entityManager.HasComponent<EnemyState>(allEntities[i]))
                {
                    var state = entityManager.GetComponentData<EnemyState>(allEntities[i]);
                    state.BehaviourState = EnemyBehaviourState.Move;
                    entityManager.SetComponentData<EnemyState>(allEntities[i], state);
                }
            }
        }

        if (GUILayout.Button("Attack"))
        {
            for (int i = 0; i < allEntities.Length; i++)
            {
                if (entityManager.HasComponent<EnemyState>(allEntities[i]))
                {
                    var state = entityManager.GetComponentData<EnemyState>(allEntities[i]);
                    state.BehaviourState = EnemyBehaviourState.Attack;
                    entityManager.SetComponentData<EnemyState>(allEntities[i], state);
                }
            }
        }*/
    }

    public void Clear()
    {

    }
}
