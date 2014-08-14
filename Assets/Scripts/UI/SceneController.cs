using UnityEngine;
using System.Collections;

public class SceneController : MonoBehaviour 
{
	[SerializeField] GameObject scalingRoot;

	// Use this for initialization
	virtual public void Start() 
	{
		Managers.SceneManager.RegisterSceneController(this);
		VerifySerializeFields();
		
		// Change scale based on the scale of the screen manager
		if (this.scalingRoot == null) { return; }
		if (Managers.ScreenManager == null) { return; }
		
		float scale = Managers.ScreenManager.Scale;
		Vector3 scaleVector = this.scalingRoot.transform.localScale;
		scaleVector /= scale;
		this.scalingRoot.transform.localScale = scaleVector;
	}

	void Initialize()
	{
	}

	public virtual void VerifySerializeFields()
	{
	}

	/// <summary>
	/// This method is called after the scene is loaded.
	/// </summary>
	public virtual void OnLoad()
	{
		if (Managers.ScreenManager.ScreenRotated)
		{
			Camera camera = this.GetComponentInChildren<Camera>();			
			UIAnchor anchor = camera.GetComponentInChildren<UIAnchor>();
			anchor.transform.eulerAngles = new Vector3(0,0,90);
		}
	}

	/// <summary>
	/// This scene is called when the scene is going to be unloaded
	/// </summary>
	public virtual void OnUnload()
	{
	}
}
