using System;
using UnityEngine;

public class SerpentUtils
{
	public static T SerpentInstantiate<T>( GameObject go, Transform parentTransform = null ) where T : MonoBehaviour
	{
		GameObject newObj = (GameObject) UnityEngine.GameObject.Instantiate( go, new Vector3(0,0,0), Quaternion.identity );
		
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

