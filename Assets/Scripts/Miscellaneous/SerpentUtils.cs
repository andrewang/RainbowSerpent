using System;
using UnityEngine;

public class SerpentUtils
{
	public static GameObject Instantiate( GameObject prefab, Transform parentTransform = null )
	{
		GameObject newObj = (GameObject) UnityEngine.GameObject.Instantiate( prefab, new Vector3(0,0,0), Quaternion.identity );
		
		if (parentTransform != null)
		{
			newObj.transform.parent = parentTransform;
			newObj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);		
			newObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		}
		
		return newObj;
	}

	/// <summary>
	/// Instantiate a GameObject based on a prefab and parent transform.  Returns a component of the specified type.
	/// which is attached to the GameObject.
	/// </summary>
	/// <param name="go">Go.</param>
	/// <param name="parentTransform">Parent transform.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T Instantiate<T>( GameObject prefab, Transform parentTransform = null ) where T : MonoBehaviour
	{
		GameObject newObj = (GameObject) UnityEngine.GameObject.Instantiate( prefab, new Vector3(0,0,0), Quaternion.identity );
		
		if (parentTransform != null)
		{
			newObj.transform.parent = parentTransform;
			newObj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);		
			newObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		}
				
		Type type = typeof(T);
		T returnValue = (T) newObj.GetComponent(type.FullName);
		return returnValue;
	}
}

