using UnityEngine;
using System.Collections;

public class SplashSceneController : RSSceneController 
{
	[SerializeField] private GameObject ManagerHolder;
	
	void Start()
	{
		StartCoroutine("LoadMainMenu");
	}	
	
	IEnumerator LoadMainMenu()
	{
		yield return new WaitForEndOfFrame();
		
		DontDestroyOnLoad(ManagerHolder);
		
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Main);
	}
}
