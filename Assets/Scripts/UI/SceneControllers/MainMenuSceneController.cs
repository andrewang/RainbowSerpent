using UnityEngine;
using System.Collections;

public class MainMenuSceneController : RSSceneController 
{
	[SerializeField] private UILabel versionLabel;
	
	/*
	[SerializeField] Texture mapTexture;
	[SerializeField] Texture maskTexture;
	[SerializeField] GameObject maskDisplayObject;
	[SerializeField] Material maskingMaterial;
	*/
	
	public override void Start()
	{
		base.Start();
		
		if (this.versionLabel == null) { return; }
		this.versionLabel.text = SerpentConsts.Version;
		
		/*
		UITexture texture = maskDisplayObject.AddComponent<UITexture>();
		texture.material = maskingMaterial;
		maskingMaterial.SetTexture("_MainTex", mapTexture);
		maskingMaterial.SetTexture("_Mask", maskTexture);
		*/
	}

	public void OnStartPressed()
	{
		// begin the game by switching to the game scene		
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Game);
	}
	
	public void OnHelpPressed()
	{
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Help);
	}
	
	public void OnCreditsPressed()
	{
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Credits);
	}

	public void OnOptionsPressed()
	{
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Options);
	}
	
}
