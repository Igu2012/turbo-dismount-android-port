#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;

public class TextureFetcher : MonoBehaviour
{
	private struct FreedTexture
	{
		public FacebookPicContainer container;

		public float timeStamp;

		public Texture texture;

		public FreedTexture(FacebookPicContainer container, float timeStamp, Texture texture)
		{
			this.container = container;
			this.timeStamp = timeStamp;
			this.texture = texture;
		}
	}

	private int maxTexturesInCache = 50;

	private Dictionary<string, Texture> fetchedTextures = new Dictionary<string, Texture>();

	private Dictionary<string, FreedTexture> freedTextures = new Dictionary<string, FreedTexture>();

	private Dictionary<string, Action<Texture>> completeActions = new Dictionary<string, Action<Texture>>();

	private string currentFetchId = string.Empty;

	private bool fetchInProgress;

	private void TextureFetchComplete(Texture2D texture)
	{
		if (texture == null)
		{
			Debug.Log("Error fetching texture");
		}
		else if (currentFetchId != string.Empty)
		{
			fetchedTextures.Add(currentFetchId, texture);
			completeActions[currentFetchId](texture);
			completeActions.Remove(currentFetchId);
			currentFetchId = string.Empty;
		}
		else
		{
			UnityEngine.Object.Destroy(texture);
		}
		fetchInProgress = false;
	}

	private string FindOldestTexture()
	{
		float num = Time.time;
		string result = string.Empty;
		foreach (KeyValuePair<string, FreedTexture> freedTexture in freedTextures)
		{
			if (freedTexture.Value.timeStamp < num)
			{
				result = freedTexture.Key;
				num = freedTexture.Value.timeStamp;
			}
		}
		return result;
	}

	public void RequestTexture(string id, Action<Texture> completeAction)
	{
		if (completeActions.ContainsKey(id))
		{
			completeActions[id] = completeAction;
		}
		if (freedTextures.ContainsKey(id))
		{
			fetchedTextures.Add(id, freedTextures[id].texture);
			freedTextures.Remove(id);
		}
		if (fetchedTextures.ContainsKey(id))
		{
			completeAction(fetchedTextures[id]);
			if (completeActions.ContainsKey(id))
			{
				completeActions.Remove(id);
			}
			return;
		}
		if (fetchedTextures.Count + freedTextures.Count > maxTexturesInCache)
		{
			string text = FindOldestTexture();
			if (text != string.Empty)
			{
				FreedTexture freedTexture = freedTextures[text];
				Texture texture = freedTexture.texture;
				freedTexture.container.TextureReleasedFromCache();
				freedTextures.Remove(text);
				UnityEngine.Object.Destroy(texture);
			}
		}
		completeActions.Add(id, completeAction);
	}

	public void FreeTexture(string id, FacebookPicContainer container)
	{
		if (fetchedTextures.ContainsKey(id))
		{
			freedTextures.Add(id, new FreedTexture(container, Time.time, fetchedTextures[id]));
			fetchedTextures.Remove(id);
		}
		if (currentFetchId == id)
		{
			currentFetchId = string.Empty;
		}
		if (completeActions.ContainsKey(id))
		{
			completeActions.Remove(id);
		}
	}

	public void Update()
	{
		if (fetchInProgress || completeActions.Count == 0)
		{
			return;
		}
		fetchInProgress = true;
		using (Dictionary<string, Action<Texture>>.Enumerator enumerator = completeActions.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				currentFetchId = enumerator.Current.Key;
			}
		}
		FacebookWrapper.GetUserPicture(currentFetchId, TextureFetchComplete);
	}
}
