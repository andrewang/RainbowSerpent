using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptCache: MonoBehaviour
{
	[SerializeField] private int initialSize;
	[SerializeField] private GameObject prefab;
	
	private List<GameObject> objects = new List<GameObject>();
	
	// Use this for initialization
	void Start () 
	{
		DontDestroyOnLoad(this);
	
		// NOTE: the objects created in this cache are set to be children of the game object to which the
		// cache script is attached.  This may affect scaling.
		for (int i = 0; i < this.initialSize; ++i)
		{
			GameObject o = SerpentUtils.Instantiate(this.prefab, this.transform);
			DontDestroyOnLoad(o);
			o.SetActive(false);
			this.objects.Add(o);
		}
	}
	
	public T GetObject<T>() where T : MonoBehaviour
	{
		if (this.objects.Count == 0)
		{
			return default(T);
		}
		GameObject o = this.objects[ this.objects.Count - 1 ];
		this.objects.RemoveAt( this.objects.Count - 1 );
		o.SetActive(true);		
		o.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		return o.GetComponent<T>();
	}
	
	public void AddObject(GameObject o)
	{
		o.SetActive(false);
		this.objects.Add(o);
	}
}
