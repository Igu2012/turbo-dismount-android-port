#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	[RequireComponent(typeof(AudioSource))]
	public class AudioInfo : MonoBehaviour
	{
		private float startTime;

		public float StartTime
		{
			get
			{
				return startTime;
			}
			set
			{
				startTime = value;
			}
		}
	}
}
