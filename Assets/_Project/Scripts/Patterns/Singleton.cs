using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Patterns
{
    /// <summary>
    /// Abstract singleton class for Unity. Updated with possible errors: 
    /// - Doesn't create a new singleton if it is in scene. 
    /// - Singleton has an initialize method which is called on creation, finding one from scene, or awake. Which makes sure initialization always happen before using it. 
    /// - It doesn't create a new single if you try to access it OnDestroy (e.g. game quit). Returns null instead.
    /// </summary>
    /// <typeparam name="TSingleton">Type of singleton. (Same as class inheriting from it.)</typeparam>
    public abstract class Singleton<TSingleton> : MonoBehaviour
        where TSingleton : Singleton<TSingleton>
    {
        /// <summary>
        /// A flag which is set to true when Application.quitting event is fired.
        /// Used to stop singleton from spawning if it is used OnDestroy.
        /// </summary>
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// Has initialization code been called?
        /// </summary>
        private bool _initialized = false;

        /// <summary>
        /// Backing field of the instance.
        /// </summary>
        private static TSingleton _instance;

        /// <summary>
        /// Static property to access singleton. Creates a new one if it doesn't exist.
        /// </summary>
        public static TSingleton Instance
        {
            get
            {
                if (_applicationIsQuitting) return null;
                if (_instance != null) return _instance;
                // Try to find a singleton in scene.
                var foundSingletons = Resources.FindObjectsOfTypeAll<TSingleton>().ToList();
                if (foundSingletons.Count != 0)  // is not (null or empty)
                {
                    // choose one that has already initialized.
                    _instance = foundSingletons.FirstOrDefault(singleton => singleton._initialized);
                    // if none have initialised then choose first one.
                    if (_instance == null) _instance = foundSingletons[0];
                    // destroy th rest.
                    foundSingletons.Remove(_instance);
                    if (foundSingletons.Count > 0)
                    {
                        Debug.LogWarning($"Found more than one singleton of type: {_instance.GetType()} when searching in scene. Make sure scene only has one instance. Removing them.");
                        for (var idx = foundSingletons.Count - 1; idx >= 1; idx--) Destroy(foundSingletons[idx]);
                    }
                }
                else
                {
                    // otherwise create one.
                    var singleton = new GameObject("Singleton").AddComponent<TSingleton>();
                    _instance = singleton.GetComponent<TSingleton>();
                    Debug.Log($"Singleton of type: {nameof(TSingleton)} was created on the fly.");
                }

                // initialize singleton if it hasn't been initialized.
                if (_instance._initialized) return _instance;
                _instance.Initialize();
                _instance._initialized = true;

                return _instance;
            }
        }

        /// <summary>
        /// Should we mark this singleton object as don't destroy on load.
        /// <remarks>It is best to keep a singleton on one game object as it may mark the game object as don't destroy on load (Unity can be weird).</remarks>
        /// </summary>
        public bool dontDestroyOnLoad = false;
        public bool renamePrefab = false;

        /// <summary>
        /// Sets up the instance field. And names the game object.
        /// </summary>
        protected virtual void Awake()
        {
            if (_initialized == false)
            {
                Initialize();
                _initialized = true;
            }
        }

        /// <summary>
        /// Needs to be overwritten by child class. Give it a descriptive name with "Singleton" suffix.
        /// Return null or empty string to not overwrite name in scene. 
        /// </summary>
        /// <returns>Singleton game object name.</returns>
        protected virtual string GetSingletonName()
        {
            return typeof(TSingleton).Name;
        }

        /// <summary>
        /// Initialize is called in Awake and on creation. You should put any code that should run before this singleton is usable.
        /// Since it can create an instance on the fly, it's is not guaranteed that Awake will run before you access it.
        /// But this will run just after it is created on fly or on Awake.
        /// <remarks>Call base on override.</remarks>
        /// </summary>
        protected virtual void Initialize()
        {
            if (_instance == null) _instance = this as TSingleton;
            else if (_instance != this)
            {
                Debug.LogWarning($"Singleton already exits of type {GetType().Name}. Destroying.");
                Destroy(this.gameObject);
                return;
            }

            if (dontDestroyOnLoad) DontDestroyOnLoad(this);
            
            if (renamePrefab) gameObject.name = GetSingletonName();

            _initialized = true;
        }

        /// <summary>
        /// We need this method because we were getting following error:
        /// 'Some objects were not cleaned up when closing the scene. (Did you spawn new GameObjects from OnDestroy?)'
        /// Which is caused because objects try to access destroyed singleton OnDisable (which is called last on quit) and since it doesn't exists, it create a new instance.
        /// </summary>
        private void OnApplicationQuit() => _applicationIsQuitting = true;

    }
}