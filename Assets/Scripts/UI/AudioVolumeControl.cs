using UnityEngine;
using System.Collections;

public class AudioVolumeControl : MonoBehaviour 
{
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private bool music;

	// Use this for initialization
	void Start () 
	{
		VerifySerializeFields();
		
		if (this.audioSource == null) { return; }
		
		if (music)
		{
			this.audioSource.volume = Managers.SettingsManager.MusicVolume;
		}
		else
		{
			this.audioSource.volume = Managers.SettingsManager.SoundVolume;
		}
		
		if (this.audioSource.volume > 0)
		{
			this.audioSource.Play();
		}
	}
	
	void VerifySerializeFields()
	{
		if (this.audioSource == null) 
		{
			Debug.LogError("Error: AudioVolumeControl has no audio source");
		}
	}
	
}
