#pragma warning disable 0618,0619
using Dismount;
using UnityEngine;

public class FacebookPicContainer : MonoBehaviour
{
	public TextureFetcher textureFetcher;

	public string id;

	private UITexture uiTexture;

	private Texture defaultTexture;

	private bool visible;

	private void Awake()
	{
		uiTexture = GetComponent<UITexture>();
		defaultTexture = uiTexture.mainTexture;
	}

	private void TextureFetchComplete(Texture result)
	{
		if ((bool)result)
		{
			if (uiTexture.mainTexture != result)
			{
				uiTexture.mainTexture = result;
			}
		}
		else
		{
			Debug.Log("failed to retrieve image!");
		}
	}

	private void Update()
	{
		if (uiTexture.panel.IsVisible(uiTexture))
		{
			if (!visible)
			{
				visible = true;
				FetchTexture();
			}
		}
		else if (visible)
		{
			visible = false;
			FreeTexture();
		}
	}

	private void FetchTexture()
	{
		if (!textureFetcher)
		{
			textureFetcher = DismountGame.textureFetcher;
		}
		if ((bool)textureFetcher)
		{
			textureFetcher.RequestTexture(id, TextureFetchComplete);
		}
	}

	private void FreeTexture()
	{
		if ((bool)textureFetcher)
		{
			textureFetcher.FreeTexture(id, this);
		}
	}

	public void TextureReleasedFromCache()
	{
		if (uiTexture.mainTexture != defaultTexture)
		{
			uiTexture.mainTexture = defaultTexture;
		}
	}
}
