using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptCache<T> : MonoBehaviour where T : MonoBehaviour
{
	[SerializeField] private int initialSize;
	[SerializeField] private GameObject prefab;
	
	private List<T> objects = new List<T>();
	
	// Use this for initialization
	void Start () 
	{
		// NOTE: the objects created in this cache are set to be children of the game object to which the
		// cache script is attached.  This may affect scaling.
		for (int i = 0; i < this.initialSize; ++i)
		{
			T t = SerpentUtils.Instantiate<T>(this.prefab, this.transform);
			this.objects.Add(t);
		}
	}
	
	public T GetObject()
	{
		if (this.objects.Count == 0)
		{
			return null;
		}
		T t = this.objects[ this.objects.Count - 1 ];
		this.objects.RemoveAt( this.objects.Count - 1 );
		return t;
	}
	
	public void AddObject(T t)
	{
		this.objects.Add(t);
	}
}
