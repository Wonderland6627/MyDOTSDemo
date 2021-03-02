using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

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

    private EntityManager entityManager;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Player.Init();
        CreateWeaponEntity();
    }

    /// <summary>
    /// 创建一个武器实体 并为其绑定WeaponTag
    /// </summary>
    private void CreateWeaponEntity()
    {
        GameObject swordRes = Resources.Load<GameObject>("Prefabs/Sword");
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        Entity swordEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(swordRes, settings);
        Entity sword = entityManager.Instantiate(swordEntity);
        entityManager.AddComponent<WeaponTag>(sword);
        entityManager.SetComponentData(sword, new Translation { Value = Player.weaponPos.position });
    }

    public void Clear()
    {

    }
}
