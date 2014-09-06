using UnityEngine;

public abstract class Scale : MonoBehaviour
{
	#region Serialize Fields
	
	[SerializeField] protected bool applyToHorizontal;
	[SerializeField] protected bool applyToVertical;
	
	[SerializeField] private bool maintainProportionsScaledByWidth;
	[SerializeField] private bool maintainProportionsScaledByHeight;
	[SerializeField] private bool maintainProportionsScaledBySmaller;
	
	[SerializeField] protected float baseWidth = 480;
	[SerializeField] protected float baseHeight = 800;
	
	protected float widthScale;
	protected float heightScale;
	
	#endregion Serialize Fields
	
	void Start()
	{
		VerifySerializeFields();
		DetermineScales();
		ApplyScale();
	}
	
	private void VerifySerializeFields()
	{
		if (this.applyToHorizontal == false && this.applyToVertical == false)
		{
			Debug.LogError("ScalePosition applies neither to horizontal or vertical");
		}
		
		if (this.maintainProportionsScaledByWidth && this.maintainProportionsScaledByHeight)    
		{
			Debug.LogError("ScalePosition has maintain propertions set, and BOTH scale by width and scale by height are set");
		}
	}
	
	private void DetermineScales()
	{
		this.widthScale = Managers.ScreenManager.Width / this.baseWidth;
		this.heightScale = Managers.ScreenManager.Height / this.baseHeight;
		
		if (this.maintainProportionsScaledByWidth)
		{
			this.heightScale = this.widthScale;
		}
		else if (this.maintainProportionsScaledByHeight)
		{
			this.widthScale = this.heightScale;
		}
		else if (this.maintainProportionsScaledBySmaller)
		{
			float smaller = Mathf.Min(this.widthScale, this.heightScale);
			this.widthScale = smaller;
			this.heightScale = smaller;
		}
		
		// Override with 1s for axes that should not be affected.
		if (this.applyToVertical == false)	
		{
			this.heightScale = 1.0f;
		}
		if (this.applyToHorizontal == false)
		{
			this.widthScale = 1.0f;
		}
		
	}
	
	abstract protected void ApplyScale();
	

	
}


