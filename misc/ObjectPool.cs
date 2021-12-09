using System.Collections.Generic;
using UnityEngine;

namespace com.samwalz.unity_ui.misc
{
    public static class ListPool<T>
    {
        public static List<T> Get() => ObjectPool<List<T>>.Get();
        public static void Return(List<T> list)
        {
            list.Clear();
            ObjectPool<List<T>>.Return(list);
        }
    }
    public static class ObjectPool<T> where T : new()
    {
        private static readonly Queue<T> AvailableItems = new Queue<T>();
        public static T Get() => AvailableItems.Count != 0 ? AvailableItems.Dequeue() : new T();
        public static void Return(T item) => AvailableItems.Enqueue(item);
    }
    public static class ObjectPool {
        private const HideFlags PoolVisibility = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
        public static bool Debug = false;
        private static readonly Dictionary<string, Pool> Pools = new Dictionary<string, Pool>();
        private static Transform _rootTransform;

        private static Transform RootTransform {
    		get {
                if (_rootTransform) return _rootTransform;
                var go = new GameObject();
                _rootTransform = go.transform;
                go.name = "ObjectPool_root";
                go.hideFlags = PoolVisibility;
                return _rootTransform;
    		}
    	}

        private static Pool GetPool (GameObject gameObject) {
            Pools.TryGetValue(gameObject.name, out var pool);
            if (pool != null) return pool;
            pool = CreatePool(gameObject);
            return pool;
    	}

        private static Pool GetPool (string key) {
            Pools.TryGetValue(key, out var pool);
			return pool;
		}

        private static Pool CreatePool (GameObject gameObject, int minSize = 10) {
            var poolID = gameObject.name;
            if (Pools.ContainsKey(poolID)) return Pools[poolID];
    		var pool = new Pool(gameObject, minSize);
            Pools.Add(poolID, pool);
    		return pool;
    	}

    	public static GameObject InstantiateSustainable (GameObject original)
        {
    		return GetPool(original).InstantiateSustainable();
    	}
        public static GameObject InstantiateSustainable (GameObject original, Transform parent)
        {
            return GetPool(original).InstantiateSustainable(parent);
        }
        public static T InstantiateSustainable<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : MonoBehaviour
        {
            return GetPool(prefab.gameObject).InstantiateSustainable(position, rotation, parent).GetComponent<T>();
        }
        public static GameObject InstantiateSustainable (GameObject original,  Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return GetPool(original).InstantiateSustainable(position, rotation, parent);
    	}

        public static void Recycle<T>(T component) where T: MonoBehaviour
        {
            GetPool(component.gameObject).Recycle(component.gameObject);
        }
    	public static void Recycle (GameObject gameObject) 
        {
    		GetPool(gameObject).Recycle(gameObject);
    	}
    	public static void Recycle (ObjectPoolObject opo) 
        {
    		GetPool(opo.gameObject).Recycle(opo);
    	}
		public static int RecycleAll (GameObject gameObject) 
        {
			return GetPool(gameObject).RecycleAll();
		}

        public class Pool
        {
            private ObjectPoolObject[] _gameObjects = new ObjectPoolObject[0];

            private int _firstInactivePosition;
            private readonly GameObject _original;
            private int _id = -1;
            private Transform _poolRoot;

            public Pool(GameObject original, int minSize)
            {
                _original = original;
                InitPoolRoot("pool_" + original.name);
                GrowPool(minSize);
            }

            private void InitPoolRoot(string name)
            {
                var go = new GameObject();
                _poolRoot = go.transform;
                _poolRoot.SetParent(RootTransform);
                go.name = name;
            }

            public GameObject InstantiateSustainable()
            {
                return InstantiateSustainable(Vector3.zero, Quaternion.identity);
            }

            public GameObject InstantiateSustainable(Transform parent)
            {
                return InstantiateSustainable(parent.position, parent.rotation, parent);
            }

            public GameObject InstantiateSustainable(Vector3 position, Quaternion rotation, Transform parent = null)
            {
                var opo = GetNextInactiveGameObject();
                var gameObject = opo.gameObject;
                gameObject.transform.SetParent(parent);
                gameObject.transform.position = position;
                gameObject.transform.rotation = rotation;
                if (SamWalz.Unity.ObjectPool.debug)
                {
                    UnityEngine.Debug.Log("instantiate at " + position);
                }
                gameObject.hideFlags = HideFlags.None;

                gameObject.SetActive(true);
                return gameObject;
            }

            private ObjectPoolObject GetNextInactiveGameObject()
            {
                ObjectPoolObject gameObject = null;
                // we know where the first inactive object should be
                if (_firstInactivePosition < _gameObjects.Length)
                {
                    gameObject = _gameObjects[_firstInactivePosition];
                }
                // none found? we need more!
                if (gameObject == null)
                {
                    gameObject = GrowPool(_gameObjects.Length > 0 ? _gameObjects.Length : 3);
                }
                // move position - next inactive object is now first in line
                _firstInactivePosition++;
                return gameObject;
            }

            /// <summary>
            /// Grows the pool by amount units.
            /// </summary>
            /// <returns>the first new GameObject</returns>
            /// <param name="amount">wished increase in size</param>
            private ObjectPoolObject GrowPool(int amount)
            {
                var newPool = new ObjectPoolObject[_gameObjects.Length + amount];
                _gameObjects.CopyTo(newPool, 0);
                for (var i = _gameObjects.Length; i < newPool.Length; i++)
                {
                    var opo = CreateNewPoolObject(_original);
                    newPool[i] = opo;
                    opo.transform.SetParent(_poolRoot);
                    opo.Pool = this;
                    _id++;
                    opo.ObjectPoolId = _id;
                }
                _gameObjects = newPool;
                return newPool[newPool.Length - amount];
            }

            private static ObjectPoolObject CreateNewPoolObject(GameObject original)
            {
                var gameObject = (GameObject)Object.Instantiate(original);
                gameObject.name = original.name;
                var opo = gameObject.GetComponent<ObjectPoolObject>();
                if (opo == null)
                {
                    opo = gameObject.AddComponent<ObjectPoolObject>();
                }
                gameObject.hideFlags = PoolVisibility;
                gameObject.SetActive(false);
                return opo;
            }

            public void Recycle(GameObject gameObject)
            {
                var opo = gameObject.GetComponent<ObjectPoolObject>();
                if (opo == null)
                {
                    opo = gameObject.AddComponent<ObjectPoolObject>();
                }
                RecycleGameObject(opo);
            }

            public void Recycle(ObjectPoolObject opo)
            {
                RecycleGameObject(opo);
            }

            private void RecycleGameObject(ObjectPoolObject opo)
            {
                var go = opo.gameObject;
                go.hideFlags = PoolVisibility;
                go.SetActive(false);
                go.transform.SetParent(_poolRoot);

                //TODO: better handling of recycling objects that are not from this pool
                // move game object to first inactive object position
                var pos = 0;
                // find position
                while (pos < _firstInactivePosition
                    && opo != _gameObjects[pos])
                {
                    pos++;
                }
                if (pos == _firstInactivePosition)
                {
                    return;
                }
                // switch positions
                var lastActive = _firstInactivePosition - 1;
                var temp = _gameObjects[lastActive];
                _gameObjects[lastActive] = opo;
                _gameObjects[pos] = temp;
                _firstInactivePosition = lastActive;
            }

            /// <summary>
            /// Recycles all objects of this pool.
            /// </summary>
            public int RecycleAll()
            {
                for (var pos = 0; pos < _firstInactivePosition; pos++)
                {
                    var opo = _gameObjects[pos];
                    var go = opo.gameObject;
                    go.hideFlags = PoolVisibility;
                    go.SetActive(false);
                    go.transform.SetParent(_poolRoot);
                }
                var recycleCount = _firstInactivePosition;
                _firstInactivePosition = 0;
                return recycleCount;
            }


        }
        public sealed class ObjectPoolObject : MonoBehaviour
        {
            private int _objectPoolId = -1;
            private Pool _objectPool;

            public int ObjectPoolId {
                get => _objectPoolId;
                set
                {
                    if (_objectPoolId != -1) return;
                    _objectPoolId = value;
                }
            }

            public Pool Pool {
                get => _objectPool;
                set
                {
                    if (_objectPool != null) return;
                    _objectPool = value;
                }
            }

            private void Recycle () {
                if (Debug) {
                    UnityEngine.Debug.Log(gameObject.name + " recycle!");
                }
                if (_objectPool == null)
                {
                    _objectPool = GetPool(gameObject.name);
                }
                _objectPool.Recycle(this);
            }
        }
    }
}