using UnityEngine;
using System;

public class Managers : MonoBehaviour
{
	public static SceneManager SceneManager;
	[SerializeField] GameObject sceneManagerPrefab = null;

	void Start()
	{
		GameObject inst = (GameObject) Instantiate(this.sceneManagerPrefab);
		Managers.SceneManager = inst.GetComponent<SceneManager>();
	}


}

