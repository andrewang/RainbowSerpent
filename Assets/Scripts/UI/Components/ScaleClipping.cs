using UnityEngine;

public class ScaleClipping : MonoBehaviour
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
		this.widthScale = Managers.ScreenManager.ScreenWidth / this.baseWidth ;
		this.heightScale = Managers.ScreenManager.ScreenHeight / this.baseHeight ;
		
		if (this.maintainProportionsScaledByWidth)
		{
			this.heightScale = this.widthScale;
		}
		else if (this.maintainProportionsScaledByHeight)
		{
			this.widthScale = this.heightScale;
		}
		
		UIPanel panel = this.GetComponentInChildren<UIPanel>();
		if (panel == null) { return; }
		
		Vector4 clipRange = panel.clipRange;
		// remember, size of clip range is ZW.
		
		if (this.applyToHorizontal)		
		{
			clipRange.z *= this.widthScale;
		}
		if (this.applyToVertical)
		{
			clipRange.y *= this.heightScale;
		}
		
		panel.clipRange = clipRange;
	}
	
}


