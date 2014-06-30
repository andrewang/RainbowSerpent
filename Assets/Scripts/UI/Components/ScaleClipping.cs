using UnityEngine;

public class ScaleDimensions : MonoBehaviour
{
	#region Serialize Fields
	
	[SerializeField] private bool applyToHorizontal;
	[SerializeField] private bool applyToVertical;
	
	[SerializeField] private bool maintainProportionsScaledByWidth;
	[SerializeField] private bool maintainProportionsScaledByHeight;
	
	[SerializeField] private float baseWidth = 480;
	[SerializeField] private float baseHeight = 800;
	
	private float widthScale;
	private float heightScale;
	
	#endregion Serialize Fields
	
	void Start()
	{
		VerifySerializeFields();
		DetermineScales();
	}
	
	private void VerifySerializeFields()
	{
		if (this.applyToHorizontal == false && this.applyToVertical == false)
		{
			Debug.LogError("ScaleDimensions applies neither to horizontal or vertical");
		}
			
		if (this.maintainProportionsScaledByWidth && this.maintainProportionsScaledByHeight)    
		{
			Debug.LogError("ScaleDimensions has maintain propertions set, and BOTH scale by width and scale by height are set");
		}
	}
	
	private void DetermineScales()
	{
		this.widthScale = Screen.width / this.baseWidth;
		this.heightScale = Screen.height / this.baseHeight;
		
		if (this.maintainProportionsScaledByWidth)
		{
			this.heightScale = this.widthScale;
		}
		else if (this.maintainProportionsScaledByHeight)
		{
			this.widthScale = this.heightScale;
		}
		
		Vector3 scale = this.transform.localScale;
		if (this.applyToHorizontal)		
		{
			scale.x = this.widthScale;
		}
		if (this.applyToVertical)
		{
			scale.y = this.heightScale;
		}
		Debug.Log ("Scale clipping " + this.widthScale + " " + this.heightScale);
		Debug.Log (scale.x + " " + scale.y);
		
		this.transform.localScale = scale;
	}
	
}


