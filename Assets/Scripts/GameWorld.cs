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
    private static UnitCharacter PlayerStatic;

    private GameObject enemyPrefab;
    private GameObject weaponPrefab;
    
    private Entity enemyEntity;
    private Entity weaponEntity;
    private Entity currentWeapon;
    private Entity defenceEntity;

    private World thisWorld;
    private EntityManager entityManager;
    private GameObjectConversionSettings settings;

    private List<Entity> entitiesList = new List<Entity>();
    private BlobAssetReference<AnimationBlobAsset> animationBlob;

    private GameInfo gameInfo;

    public void Init(GameInfo info)
    {
        thisWorld = World.DefaultGameObjectInjectionWorld;
        entityManager = thisWorld.EntityManager;
        settings = GameObjectConversionSettings.FromWorld(thisWorld, null);

        gameInfo = info;

        enemyPrefab = Resources.Load<GameObject>("Prefabs/Enemy 1");
        weaponPrefab = Resources.Load<GameObject>("Prefabs/Sword");

        enemyEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(enemyPrefab, settings);
        weaponEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(weaponPrefab, settings);

        Player.Init();

        InitBlobAsset();
        for (int i = 0; i < info.enemiesCount; i++)
        {
            CreateEnemyEntity();
        }
        currentWeapon = CreateWeaponEntity();
        defenceEntity = CreateDefenceEntity();

        if (info.enableAnimandUI)
        {
            PlayerStatic = Player;
            InitUI();
        }
    }

    /// <summary>
    /// 启用需要的系统
    /// </summary>
    public void CreateWorldSystems()
    {
        thisWorld.GetOrCreateSystem<WeaponUpdateSystem>();//武器位置同步系统
        thisWorld.GetOrCreateSystem<EnemyMoveSystem>();
        thisWorld.GetOrCreateSystem<EnemyRotateSystem>();
        thisWorld.GetOrCreateSystem<CollisionDetectSystem>();
        thisWorld.GetOrCreateSystem<EntityHealthSystem>();
        thisWorld.GetOrCreateSystem<VFXFlyForwardSystem>();
        thisWorld.GetOrCreateSystem<LifeDistanceSystem>();
    }

    private void InitUI()
    {
        var defence = entityManager.GetComponentData<EntityHealth>(defenceEntity);
        Debug.Log(defence.Value);
        UIManager.Instance.InitPlayerHPSlider(defence.Value);
    }

    /// <summary>
    /// 创建一个武器实体 并为其绑定WeaponTag
    /// </summary>
    private Entity CreateWeaponEntity()
    {
        Entity weapon = entityManager.Instantiate(weaponEntity);
        //entityManager.SetName(weapon, "PlayerWeapon");
        entityManager.SetComponentData(weapon, new Translation { Value = Player.weaponPos.position });
        WeaponState weaponState = new WeaponState() { isAttacking = false };
        entityManager.AddComponentData(weapon, weaponState);

        return weapon;
    }

    /// <summary>
    /// 创建一个防御检测用的实体 代表Player
    /// </summary>
    private Entity CreateDefenceEntity()
    {
        Entity defence = entityManager.CreateEntity();
        //entityManager.SetName(defence, "PlayerDefence");
        entityManager.AddComponent(defence, typeof(DefenceTag));
        entityManager.AddComponent(defence, typeof(Translation));
        entityManager.AddComponentData(defence, new EntityCollision { Radius = 1 });
        entityManager.AddComponentData(defence, new EntityHealth { Value = 500 });
        entityManager.AddComponentData(defence, new DefencePreData { preValue = 0});

        return defence;
    }

    public Entity CreateEnemyEntity()
    {
        float randX = UnityEngine.Random.Range(0, gameInfo.rangeLength);
        float randZ = UnityEngine.Random.Range(0, gameInfo.rangeLength);
        Vector3 randomPos = new Vector3(randX, 0, randZ);

        Entity enemy = entityManager.Instantiate(enemyEntity);
        entityManager.SetComponentData(enemy, new Translation() { Value = randomPos });
        entityManager.SetComponentData(enemy, new Rotation() { Value = quaternion.LookRotation(-randomPos, math.up()) }); ;

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

    public Entity CreateSkillVfxEntity(string goPrefabName, Vector3 startPos, Quaternion startQuaterion)
    {
        GameObject vfxPrefab = Resources.Load<GameObject>("Prefabs/" + goPrefabName);
        if (vfxPrefab == null)
        {
            return Entity.Null;
        }

        var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(vfxPrefab, settings);
        var skillVfx = entityManager.Instantiate(entity);
        //entityManager.SetName(skillVfx, goPrefabName);
        entityManager.SetComponentData(skillVfx, new Translation() { Value = startPos });
        entityManager.SetComponentData(skillVfx, new Rotation() { Value = startQuaterion });

        return skillVfx;
    }

    public void CreateSkillVfxEntities(int count, string goPrefabName, Vector3 startPos, Quaternion startQuarerion)
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

    private Entity enemyBullet;
    public Entity EnemyBullet
    {
        get
        {
            if (enemyBullet == null)
            {
                enemyBullet = GetEnemyBulletEntity();
            }

            return enemyBullet;
        }
    }
    public Entity GetEnemyBulletEntity()
    {
        GameObject bulletRes = Resources.Load<GameObject>("Prefabs/EnemyBullet");
        if (bulletRes == null)
        {
            return Entity.Null;
        }

        var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(bulletRes, settings);
        //var bullet = entityManager.Instantiate(entity);

        return entity;
    }

    public string GetEntityName(Entity entity)
    {
        return entity == null ? "EntityNull" : "DefaultEntity";// entityManager.GetName(entity);
    }

    public Vector3 GetCurrentWeaponPos()
    {
        if (currentWeapon == null)
        {
            return Player.transform.position;
        }

        Translation translation = entityManager.GetComponentData<Translation>(currentWeapon);
        return translation.Value;
    }

    public void SetCurrentWeaponAttackState(bool isAttaking)
    {
        if (currentWeapon == null)
        {
            return;
        }

        WeaponState weaponState = entityManager.GetComponentData<WeaponState>(currentWeapon);
        weaponState.isAttacking = isAttaking;
        entityManager.SetComponentData<WeaponState>(currentWeapon, weaponState);
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

    public static void OnPlayerHPUpdate(float value)
    {
        /*if (PlayerStatic == null)
        {
            return;
        }

        if (value <= 0)
        {
            PlayerStatic.Death();
        }
        else
        {
            PlayerStatic.GetHurt();
        }

        UIManager.Instance.UpdatePlayerHPSlider(value);*/
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
