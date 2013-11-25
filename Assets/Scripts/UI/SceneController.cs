using UnityEngine;
using System.Collections;

public class SceneController : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{
		Managers.SceneManager.RegisterSceneController(this);
		VerifySerializeFields();
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
	}

	/// <summary>
	/// This scene is called when the scene is going to be unloaded
	/// </summary>
	public virtual void OnUnload()
	{
	}
}
