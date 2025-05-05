#pragma warning disable 0414

using System;
using UnityEngine;
using System.Collections.Generic;

namespace Watermelon
{
    /// <summary>
    /// Class that manages all pool operations.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        private static PoolManager instance;

        [SerializeField]
        private Transform objectsContainer;
        /// <summary>
        /// Empty object to store all pooled objects at scene (used only in Editor).
        /// </summary>
        public static Transform ObjectsContainerTransform
        {
            get
            {
#if UNITY_EDITOR
                if (instance.objectsContainer != null)
                    return instance.objectsContainer;

                return null;
#else
            return null;
#endif
            }
        }

        /// <summary>
        /// List of all existing pools.
        /// </summary>
        [SerializeField]
        private List<Pool> poolsList = new List<Pool>();

        // [CACHE IS CURRENTLY DISABLED]
        //private bool useCache = false;
        ///// <summary>
        ///// When enabled PoolManager will automaticaly setup amount of spawned by default objects using cashed data.
        ///// </summary>
        //public static bool UseCache
        //{
        //    get { return instance.useCache; }
        //}

        /// <summary>
        /// Dictionary which allows to acces Pool by name.
        /// </summary>
        private Dictionary<string, Pool> poolsDictionary;

        /// <summary>
        /// True when PoolManager is already initialized.
        /// </summary>
        private bool isInited;

        private int spawnedObjectAmount = 0;
        /// <summary>
        /// Amount of spawned objects.
        /// </summary>
        public static int SpawnedObjectsAmount
        {
            get { return instance.spawnedObjectAmount; }
        }


        /// <summary>
        /// Cache data file name.
        /// </summary>
        private const string CACHE_FILE_NAME = "PoolManagerCache";

        /// <summary>
        /// Id of cache for current scene.
        /// </summary>
        private string currentCacheId = string.Empty;



        /// <summary>
        /// Initialize a single instance of PoolManager.
        /// </summary>
        private static void InitSingletone()
        {
            PoolManager poolManager = FindObjectOfType<PoolManager>();

            if (poolManager != null)
            {
                poolManager.Initialise();

                instance = poolManager;
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError("Please, add PoolManager behaviour at scene.");
            }
#endif
        }

        /// <summary>
        /// Initialization of PoolManager.
        /// </summary>
        public void Initialise()
        {
            if (instance != null)
                return;

            instance = this;

#if UNITY_EDITOR
            if (objectsContainer == null)
            {
                objectsContainer = new GameObject("[PooledObjects]").transform;
            }

            // [CACHE IS CURRENTLY DISABLED]
            //if (useCache)
            //{
            //    LoadCache();
            //}
#endif

            poolsDictionary = new Dictionary<string, Pool>();

            foreach (Pool pool in poolsList)
            {
                poolsDictionary.Add(pool.Name, pool);
            }

            foreach (Pool pool in poolsList)
            {
                pool.Initialize();
            }
        }

        public static void Unload()
        {
            PoolManager poolManager = instance; 
            if(poolManager != null)
            {
                for(int i = 0; i < poolManager.poolsList.Count; i++)
                {
                    poolManager.poolsList[i].ReturnToPoolEverything(true);
                }
            }
        }

        public static GameObject SpawnObject(GameObject prefab, Transform parrent)
        {
#if UNITY_EDITOR
            if (parrent == null)
            {
                parrent = ObjectsContainerTransform;
            }
#endif

            instance.spawnedObjectAmount++;
            return Instantiate(prefab, parrent);
        }

        /// <summary>
        /// Returns reference to Pool by it's name.
        /// </summary>
        /// <param name="poolName">Name of Pool which should be returned.</param>
        /// <returns>Reference to Pool.</returns>
        public static Pool GetPoolByName(string poolName)
        {
            if (instance == null)
            {
                InitSingletone();
            }

            if (instance.poolsDictionary.ContainsKey(poolName))
            {
                return instance.poolsDictionary[poolName];
            }
            else
            {
                return null;
            }
        }

        public static PoolGeneric<T> GetGenericPool<T>(string poolName) where T : Component
        {
            var pool = GetPoolByName(poolName);

            if (pool == null)
                return null;
            try
            {
                var poolGeneric = pool as PoolGeneric<T>;

                return poolGeneric;
            }
            catch (Exception)
            {
                Debug.Log($"[PoolManager] Could not convert pool with name {poolName} to {typeof(PoolGeneric<T>)}");

                return null;
            }
        }

        /// <summary>
        /// Adds new pool at runtime.
        /// </summary>
        /// <param name="poolBuilder">Pool builder settings.</param>
        /// <returns>Newly created pool.</returns>
        public static Pool AddPool(PoolSettings poolBuilder)
        {
            if (instance.poolsDictionary.ContainsKey(poolBuilder.name))
            {
                Debug.LogError("[Pool manager] Adding a new pool failed. Name \"" + poolBuilder.name + "\" already exists.");
                return GetPoolByName(poolBuilder.name);
            }

            Pool newPool = new Pool(poolBuilder);
            instance.poolsDictionary.Add(newPool.Name, newPool);
            instance.poolsList.Add(newPool);

            newPool.Initialize();

            return newPool;
        }

        ///// <summary>
        ///// Destroys all spawned objects. Note, this method is performance heavy.
        ///// </summary>
        //public static void Destroy(GameObject objectToDestroy)
        //{
        //    Destroy(objectTsoDestroy);
        //}

        public static bool PoolExists(string name)
        {
            if (instance == null)
            {
                return false;
            }
            else
            {
                return instance.poolsDictionary.ContainsKey(name);
            }
        }

        public static void RemoveAllNullObjects()
        {
            foreach(var poolKeyValue in instance.poolsDictionary)
            {
                poolKeyValue.Value.DeleteAllNullRefsInSpawnedObjects();
            }
        }

        // editor methods

        private bool IsAllPrefabsAssignedAtPool(int poolIndex)
        {
            if (poolsList != null && poolIndex < poolsList.Count)
            {
                return poolsList[poolIndex].IsAllPrefabsAssigned();
            }
            else
            {
                return true;
            }
        }

        private void RecalculateWeightsAtPool(int poolIndex)
        {
            poolsList[poolIndex].RecalculateWeights();
        }

#if UNITY_EDITOR
        #region Cache Management

        // //////////////////////////////////////////////////////////////////////////////////
        // New Chace system

        /// <summary>
        /// Loads cache from disc and initializing current scene's cache id.
        /// </summary>
        private void LoadCache()
        {
            // [CACHE IS CURRENTLY DISABLED]
            // cache system is currently disabled

            //cache = Serializer.DeserializeFromPDP<PoolManagerCache>(CACHE_FILE_NAME, logIfFileNotExists: false);

            //string sceneMetaFile = Serializer.LoadTextFileAtPath(SceneManager.GetActiveScene().path + ".meta");

            //int startIndex = sceneMetaFile.IndexOf("guid: ") + "guid: ".Length;
            //int finalIndex = sceneMetaFile.LastIndexOf("timeCreated");

            //currentCacheId = sceneMetaFile.Substring(startIndex, finalIndex - startIndex);
        }

        /// <summary>
        /// Updates cache after exit from play mode or creation of new pool.
        /// </summary>
        private void UpdateCache()
        {
            // [CACHE IS CURRENTLY DISABLED]
            // cache system is currently disabled

            //if (currentCacheId != "")
            //{
            //    // true if there is no saved cache
            //    bool init = !cache.ContainsLevel(currentCacheId);

            //    List<PoolCache> newCache = new List<PoolCache>();

            //    for (int i = 0; i < pools.Count; i++)
            //    {
            //        // if we initializing cache we simple adding current pool info
            //        if (init)
            //        {
            //            newCache.Add(new PoolCache(pools[i].Name, pools[i].Size));
            //        }
            //        // if there is a cache, let's update this stuff
            //        else
            //        {
            //            int index = cache.poolsCache[currentCacheId].FindIndex(x => x.poolName == pools[i].Name);
            //            if (index != -1)
            //            {
            //                // do not consider new data if it's (probably pool just was not used at all)
            //                if (pools[i].maxItemsUsedInOneTime != 0)
            //                {
            //                    cache.poolsCache[currentCacheId][index].UpdateSize(pools[i].maxItemsUsedInOneTime);
            //                }

            //                newCache.Add(cache.poolsCache[currentCacheId][index]);
            //            }
            //            else
            //            {
            //                newCache.Add(new PoolCache(pools[i].Name, pools[i].Size));
            //            }
            //        }
            //    }

            //    if (init)
            //    {
            //        cache.poolsCache.Add(currentCacheId, newCache);
            //    }
            //    else
            //    {
            //        cache.UpdateCache(currentCacheId, newCache);
            //    }

            //    Serializer.SerializeToPDP(cache, CACHE_FILE_NAME);
            //}
            //else
            //{
            //    Debug.LogError("[PoolManager] Cache could not be updated. This level was not initialized.");
            //}

        }

        private void OnDisable()
        {
            // [CACHE IS CURRENTLY DISABLED]
            //if (useCache)
            //{
            //    UpdateCache();
            //}
        }

        /// <summary>
        /// Updates and saves cache.
        /// </summary>
        public void SaveCache()
        {
            LoadCache();
            UpdateCache();
        }

        #endregion
#endif
    }
}

// -----------------
// IAP Manager v 1.6.4
// -----------------

// Changelog
// v 1.6.4
// • Added pro theme support
// v 1.6 
// • Added runtime pool creation
// • Added extended functions for multi pool
// • Added new pool constructor and GetPooledObject overrides
// • Generic pool upgate
// • Added clear method to pool
// v 1.5.1 
// • Added Multi objects pool type
// • Added drag n drop support
// v 1.4.5  
// • Added editor changes save
// • Updated cache system
// • Added ability to ignore cache for required pools
// • Fixed created object's names
// • Core refactoring
// • Editor UX improvements
// v 1.3.1  
// • Added RandomPools system
// • Added objectsContainer access property
// v 1.2.1 
// • Added cache system
// • Fixed errors on build
// v 1.1.0 
// • Added PoolManager editor
// v 1.0.0 
// • Basic version of pool
