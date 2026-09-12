#pragma warning disable 0618,0619
using UnityEngine;

[ExecuteInEditMode]
public class UIFilledSprite : UISprite
{
	public override Type type
	{
		get
		{
			return Type.Filled;
		}
	}
}
