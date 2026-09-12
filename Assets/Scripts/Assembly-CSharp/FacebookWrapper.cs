#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using Prime31;
using UnityEngine;

public class FacebookWrapper : MonoBehaviour
{
	public struct FacebookUser
	{
		public string name;

		public string id;
	}

	private class FacebookFriendsResult
	{
	}

	private class FacebookMeResult
	{
	}

	public static void Init()
	{
	}

	public static bool IsLoggedIn()
	{
		return false;
	}

	private static void sessionOpenedEvent()
	{
	}

	private static void loginFailedEvent(P31Error error)
	{
	}

	public static void Login(Action<bool> completeAction)
	{
	}

	private static void GetFriendsComplete(string s, FacebookFriendsResult result)
	{
	}

	private static void GetMeComplete(string s, FacebookMeResult result)
	{
	}

	public static void GetFriends(Action<List<FacebookUser>> completeAction)
	{
	}

	public static void GetUserPicture(string id, Action<Texture2D> completeAction)
	{
	}
}
