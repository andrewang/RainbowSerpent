using UnityEngine;

public class ScaleDimensions : MonoBehaviour
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
		
		// If this is a sprite, resize it in case it's sliced, instead of setting the scale.
		UIWidget widget = this.GetComponent<UIWidget>();
		UISprite sprite = widget as UISprite;
		if (sprite != null)
		{
			sprite.width = (int) ( (float)sprite.width * this.widthScale );
			sprite.height = (int) ( (float)sprite.height * this.heightScale );
			return;	
		}
		UILabel label = widget as UILabel;
		if (label != null)
		{
			// We always want to keep the text proportionate, so increase scale by the smaller of the two.
			// But if the proportions aren't the same, increase the label width/height by the difference.
			float smaller = Mathf.Min(this.widthScale, this.heightScale);	
			float larger = Mathf.Max(this.widthScale, this.heightScale);
			float ratio = larger/smaller;
			
			label.width = (int) ( (float)label.width * ratio );
			label.height = (int) ( (float)label.height * ratio );
			
			this.widthScale = smaller;
			this.heightScale = smaller;
		}
		
		Vector3 scale = this.transform.localScale;
		if (this.applyToHorizontal)		
		{
			scale.x *= this.widthScale;
		}
		if (this.applyToVertical)
		{
			scale.y *= this.heightScale;
		}
		
		this.transform.localScale = scale;
	}
	
}


