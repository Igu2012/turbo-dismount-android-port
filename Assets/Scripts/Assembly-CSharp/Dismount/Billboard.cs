#pragma warning disable 0618,0619
using Dismount.LevelEditor;
using UnityEngine;

namespace Dismount
{
	[RequireComponent(typeof(BillboardData))]
	public class Billboard : MonoBehaviour
	{
		private BillboardData data;

		public Texture[] posterTextures;

		public MeshRenderer posterMesh;

		private void Awake()
		{
			data = GetComponent<BillboardData>();
			UpdatePoster();
		}

		private void UpdatePoster()
		{
			int num = 0;
			int max = 7;
			num = ((data.poster == BillboardData.Poster.Random) ? Random.Range(0, max) : ((data.poster == BillboardData.Poster.RandomMoreSecretExit) ? ((Random.Range(0, 3) >= 2) ? Random.Range(3, max) : Random.Range(0, 3)) : ((data.poster == BillboardData.Poster.RandomSecretExit) ? Random.Range(0, 3) : ((data.poster != BillboardData.Poster.RandomFriend) ? ((int)data.poster) : Random.Range(3, max)))));
			num = Mathf.Clamp(num, 0, posterTextures.Length - 1);
			posterMesh.material.mainTexture = posterTextures[num];
		}
	}
}
