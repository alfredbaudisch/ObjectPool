using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public sealed class ObjectPool : MonoBehaviour
{
	static ObjectPool _instance;

	Dictionary<Component, List<Component>> objectLookup = new Dictionary<Component, List<Component>>();
	Dictionary<Component, Component> prefabLookup = new Dictionary<Component, Component>();
	
	public static void Clear()
	{
		instance.objectLookup.Clear();
		instance.prefabLookup.Clear();
	}

	public static void CreatePool<T>(T prefab) where T : Component
	{
		CreatePool(prefab, 0);
	}

	public static void CreatePool<T>(T prefab, int initialAmount) where T : Component
	{
		if (!instance.objectLookup.ContainsKey(prefab))
			instance.objectLookup.Add(prefab, new List<Component>());

		if(initialAmount > 0)
			for(int i = 0; i < initialAmount; i++)
				SpawnInactive(prefab);
	}
	
	public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		if (instance.objectLookup.ContainsKey(prefab))
		{
			T obj = null;
			var list = instance.objectLookup[prefab];
			if (list.Count > 0)
			{
				while (obj == null && list.Count > 0)
				{
					obj = list[0] as T;
					list.RemoveAt(0);
				}
				if (obj != null)
				{
					obj.transform.parent = null;
					obj.transform.localPosition = position;
					obj.transform.localRotation = rotation;
					obj.gameObject.SetActive(true);
					instance.prefabLookup.Add(obj, prefab);
					return (T)obj;
				}
			}

			obj = (T)Object.Instantiate(prefab, position, rotation);
			instance.prefabLookup.Add(obj, prefab);
			return (T)obj;
		}
		else
			return (T)Object.Instantiate(prefab, position, rotation);
	}
	public static T Spawn<T>(T prefab, Vector3 position) where T : Component
	{
		return Spawn(prefab, position, Quaternion.identity);
	}
	public static T Spawn<T>(T prefab) where T : Component
	{
		return Spawn(prefab, Vector3.zero, Quaternion.identity);
	}

	public static T SpawnInactive<T>(T prefab) where T : Component
	{
		if (!instance.objectLookup.ContainsKey(prefab))
			instance.objectLookup.Add(prefab, new List<Component>());

		var obj = (T)Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
		obj.transform.parent = instance.transform;
		obj.gameObject.SetActive(false);

		instance.objectLookup[prefab].Add(obj);

		return (T)obj;
	} 

	public static void Recycle<T>(T obj) where T : Component
	{
		if(instance.prefabLookup.ContainsKey(obj))
		{
			instance.objectLookup[instance.prefabLookup[obj]].Add(obj);
			instance.prefabLookup.Remove(obj);
			obj.transform.parent = instance.transform;
			obj.gameObject.SetActive(false);
		}
		else
			Object.Destroy(obj.gameObject);
	}

	public static void RecycleAll<T>(T obj) where T : Component
	{
		var active = instance.prefabLookup.Keys.Where(p => p.GetType() == typeof(T)).ToList();

		if(active.Count > 0)
			for(int i = 0; i < active.Count; i++)
				Recycle(active[i]);
	}

	public static List<T> GetAllOfType<T>() where T : Component
	{
		var keys = instance.objectLookup.Keys.Where(p => p.GetType() == typeof(T)).ToList();

		List<T> objects = new List<T>();

		if(keys.Count > 0)			
			foreach(var key in keys)
				if(instance.objectLookup[key].Count > 0)
					foreach(var obj in instance.objectLookup[key]) 
						objects.Add(obj as T);


		return objects;
	}

	public static int Count<T>(T prefab) where T : Component
	{
		if (instance.objectLookup.ContainsKey(prefab))
			return instance.objectLookup[prefab].Count;
		else
			return 0;
	}

	public static ObjectPool instance
	{
		get
		{
			if (_instance != null)
				return _instance;
			var obj = new GameObject("_ObjectPool");
			obj.transform.localPosition = Vector3.zero;
			_instance = obj.AddComponent<ObjectPool>();
			return _instance;
		}
	}
}

public static class ObjectPoolExtensions
{
	public static void CreatePool<T>(this T prefab) where T : Component
	{
		ObjectPool.CreatePool(prefab);
	}

	public static void CreatePool<T>(this T prefab, int initialAmount) where T : Component
	{
		ObjectPool.CreatePool(prefab, initialAmount);
	}
	
	public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return ObjectPool.Spawn(prefab, position, rotation);
	}
	public static T Spawn<T>(this T prefab, Vector3 position) where T : Component
	{
		return ObjectPool.Spawn(prefab, position, Quaternion.identity);
	}
	public static T Spawn<T>(this T prefab) where T : Component
	{
		return ObjectPool.Spawn(prefab, Vector3.zero, Quaternion.identity);
	}

	public static T SpawnInactive<T>(this T prefab) where T : Component
	{
		return ObjectPool.SpawnInactive(prefab);
	}
	
	public static void Recycle<T>(this T obj) where T : Component
	{
		ObjectPool.Recycle(obj);
	}

	public static void RecycleAll<T>(this T obj) where T : Component
	{
		ObjectPool.RecycleAll(obj);
	}

	public static List<T> GetAllOfType<T>(this T prefab) where T : Component
	{
		return ObjectPool.GetAllOfType<T>();
	}

	public static int Count<T>(T prefab) where T : Component
	{
		return ObjectPool.Count(prefab);
	}
}