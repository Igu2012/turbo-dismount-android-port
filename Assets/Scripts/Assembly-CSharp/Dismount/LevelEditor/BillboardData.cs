#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.LevelEditor
{
	public class BillboardData : MonoBehaviour
	{
		public enum Poster
		{
			StairDismount = 0,
			Eyelord = 1,
			ZenBound2 = 2,
			PapersPlease = 3,
			RGBExpress = 4,
			Pako = 5,
			FallMan = 6,
			Random = 7,
			RandomMoreSecretExit = 8,
			RandomSecretExit = 9,
			RandomFriend = 10
		}

		public Poster poster = Poster.Random;
	}
}
