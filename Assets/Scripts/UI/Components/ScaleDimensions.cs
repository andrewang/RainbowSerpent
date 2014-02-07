using UnityEngine;

public class ScaleClipping : MonoBehaviour
{
	#region Serialize Fields
	
	[SerializeField] private bool horizontal;
	[SerializeField] private bool vertical;
	
	[SerializeField] private bool maintainProportions;
	[SerializeField] private bool scaleByWidth;
	[SerializeField] private bool scaleByHeight;
	
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
		if (this.horizontal == false && this.vertical == false)
		{
			Debug.LogError("ScaleDimensions applies neither to horizontal or vertical");
		}
		if (this.maintainProportions)
		{
			if (this.scaleByWidth == false && this.scaleByHeight == false)
			{
				Debug.LogError("ScaleDimensions has maintain propertions set, but neither scale by width or scale by height is set");
			}
			else if (this.scaleByWidth && this.scaleByHeight)    
			{
				Debug.LogError("ScaleDimensions has maintain propertions set, and BOTH scale by width and scale by height are set");
			}
		}
	}
	
	private void DetermineScales()
	{
		this.widthScale = Screen.width / this.baseWidth ;
		this.heightScale = Screen.height / this.baseHeight ;
		
		if (this.maintainProportions)
		{
			if (this.scaleByWidth)
			{
				this.heightScale = this.widthScale;
			}
			else if (this.scaleByHeight)
			{
				this.widthScale = this.heightScale;
			}
		}
		
		UIPanel panel = this.GetComponentInChildren<UIPanel>();
		if (panel == null) { return; }
		
		Vector4 clipRange = panel.clipRange;
		// remember, size of clip range is ZW.
		
		if (this.horizontal)		
		{
			clipRange.z *= this.widthScale;
		}
		if (this.vertical)
		{
			clipRange.y *= this.heightScale;
		}
		
		panel.clipRange = clipRange;
	}
	
}


