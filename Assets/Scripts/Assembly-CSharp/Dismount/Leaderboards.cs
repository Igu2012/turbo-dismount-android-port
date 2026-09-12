#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class Leaderboards : MonoBehaviour
	{
		public struct LeaderboardEntry
		{
			public string name;

			public int score;

			public int rank;
		}
	}
}
