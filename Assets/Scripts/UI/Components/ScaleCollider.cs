using UnityEngine;

public class ScaleCollider : MonoBehaviour
{
	#region Serialize Fields
	
	[SerializeField] private bool applyToHorizontal;
	[SerializeField] private bool applyToVertical;
	
	[SerializeField] private bool maintainProportionsScaledByWidth;
	[SerializeField] private bool maintainProportionsScaledByHeight;
	[SerializeField] private bool maintainProportionsScaledBySmaller;
	
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

		// Set (box) collider size
		BoxCollider collider = this.collider as BoxCollider;
		if (collider == null) { return; }
				
		Vector3 centre = collider.center;
		Vector3 size = collider.size;
		centre.x *= this.widthScale;
		centre.y *= this.heightScale;
		size.x *= this.widthScale;
		size.y *= this.heightScale;
		
		collider.center = centre;
		collider.size = size;
		
		UISlider slider = this.GetComponent<UISlider>();
		if (slider != null)			
		{
			slider.fullSize = size;
		}
		
	}
	
}


