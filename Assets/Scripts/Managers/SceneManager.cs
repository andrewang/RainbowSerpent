using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour 
{
	SceneController currentController = null;

	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	// There shouldn't be much to this.
	public void LoadScene(string sceneName)
	{
		StartCoroutine(LoadSceneCoroutine(sceneName));
	}

	private IEnumerator LoadSceneCoroutine(string sceneName)
	{
		if (this.currentController != null)
		{
			this.currentController.OnUnload();
			Destroy(this.currentController.gameObject);
			this.currentController = null;
		}

		// Load loading scene first?
		//Application.LoadLevel(SerpentConsts.SceneNames.Loading);

		yield return new WaitForEndOfFrame();

		Application.LoadLevel(sceneName);
	
		yield break;
	}

	public void RegisterSceneController(SceneController controller)
	{
		this.currentController = controller;
		// Handle this in some other way?  TODO FIX.
		controller.OnLoad();
	}

}
