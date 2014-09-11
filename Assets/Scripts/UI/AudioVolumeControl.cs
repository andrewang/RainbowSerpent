using UnityEngine;
using System.Collections;

public class AudioVolumeControl : MonoBehaviour 
{
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private float volumeMultiplier = 1.0f;
	[SerializeField] private bool music;

	// Use this for initialization
	void Start () 
	{		
		if (this.audioSource == null) 
		{
			this.audioSource = this.gameObject.GetComponent<AudioSource>();
			if (this.audioSource == null)
			{
				return;
			}
		}
		
		if (music)
		{
			this.audioSource.volume = Managers.SettingsManager.MusicVolume * this.volumeMultiplier;
		}
		else
		{
			this.audioSource.volume = Managers.SettingsManager.SoundVolume * this.volumeMultiplier;
		}
		
		if (this.audioSource.volume > 0)
		{
			this.audioSource.Play();
		}
	}
	
}
