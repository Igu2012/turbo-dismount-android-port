using System;
using System.Collections.Generic;
using System.IO;
using Prime31;
using UnityEngine;

public class EtceteraAndroidManager : AbstractManager
{
	public static event Action<string> alertButtonClickedEvent;

	public static event Action alertCancelledEvent;

	public static event Action<string> promptFinishedWithTextEvent;

	public static event Action promptCancelledEvent;

	public static event Action<string, string> twoFieldPromptFinishedWithTextEvent;

	public static event Action twoFieldPromptCancelledEvent;

	public static event Action webViewCancelledEvent;

	public static event Action albumChooserCancelledEvent;

	public static event Action<string> albumChooserSucceededEvent;

	public static event Action photoChooserCancelledEvent;

	public static event Action<string> photoChooserSucceededEvent;

	public static event Action<string> videoRecordingSucceededEvent;

	public static event Action videoRecordingCancelledEvent;

	public static event Action ttsInitializedEvent;

	public static event Action ttsFailedToInitializeEvent;

	public static event Action askForReviewWillOpenMarketEvent;

	public static event Action askForReviewRemindMeLaterEvent;

	public static event Action askForReviewDontAskAgainEvent;

	public static event Action<string> inlineWebViewJSCallbackEvent;

	public static event Action<string> notificationReceivedEvent;

	public static event Action<List<EtceteraAndroid.Contact>> contactsLoadedEvent;

	static EtceteraAndroidManager()
	{
		AbstractManager.initialize(typeof(EtceteraAndroidManager));
	}

	public void alertButtonClicked(string positiveButton)
	{
		if (alertButtonClickedEvent != null)
		{
			alertButtonClickedEvent(positiveButton);
		}
	}

	public void alertCancelled(string empty)
	{
		if (alertCancelledEvent != null)
		{
			alertCancelledEvent();
		}
	}

	public void promptFinishedWithText(string text)
	{
		string[] array = text.Split(new string[1] { "|||" }, StringSplitOptions.None);
		if (array.Length == 1 && promptFinishedWithTextEvent != null)
		{
			promptFinishedWithTextEvent(array[0]);
		}
		if (array.Length == 2 && twoFieldPromptFinishedWithTextEvent != null)
		{
			twoFieldPromptFinishedWithTextEvent(array[0], array[1]);
		}
	}

	public void promptCancelled(string empty)
	{
		if (promptCancelledEvent != null)
		{
			promptCancelledEvent();
		}
	}

	public void twoFieldPromptCancelled(string empty)
	{
		if (twoFieldPromptCancelledEvent != null)
		{
			twoFieldPromptCancelledEvent();
		}
	}

	public void webViewCancelled(string empty)
	{
		if (webViewCancelledEvent != null)
		{
			webViewCancelledEvent();
		}
	}

	public void albumChooserCancelled(string empty)
	{
		if (albumChooserCancelledEvent != null)
		{
			albumChooserCancelledEvent();
		}
	}

	public void albumChooserSucceeded(string path)
	{
		if (albumChooserSucceededEvent != null)
		{
			if (File.Exists(path))
			{
				albumChooserSucceededEvent(path);
			}
			else if (albumChooserCancelledEvent != null)
			{
				albumChooserCancelledEvent();
			}
		}
	}

	public void photoChooserCancelled(string empty)
	{
		if (photoChooserCancelledEvent != null)
		{
			photoChooserCancelledEvent();
		}
	}

	public void photoChooserSucceeded(string path)
	{
		if (photoChooserSucceededEvent != null)
		{
			if (File.Exists(path))
			{
				photoChooserSucceededEvent(path);
			}
			else if (photoChooserCancelledEvent != null)
			{
				photoChooserCancelledEvent();
			}
		}
	}

	public void videoRecordingSucceeded(string path)
	{
		if (videoRecordingSucceededEvent != null)
		{
			videoRecordingSucceededEvent(path);
		}
	}

	public void videoRecordingCancelled(string empty)
	{
		if (videoRecordingCancelledEvent != null)
		{
			videoRecordingCancelledEvent();
		}
	}

	public void ttsInitialized(string result)
	{
		bool flag = result == "1";
		if (flag && ttsInitializedEvent != null)
		{
			ttsInitializedEvent();
		}
		if (!flag && ttsFailedToInitializeEvent != null)
		{
			ttsFailedToInitializeEvent();
		}
	}

	public void ttsUtteranceCompleted(string utteranceId)
	{
		Debug.Log("utterance completed: " + utteranceId);
	}

	public void askForReviewWillOpenMarket(string empty)
	{
		if (askForReviewWillOpenMarketEvent != null)
		{
			askForReviewWillOpenMarketEvent();
		}
	}

	public void askForReviewRemindMeLater(string empty)
	{
		if (askForReviewRemindMeLaterEvent != null)
		{
			askForReviewRemindMeLaterEvent();
		}
	}

	public void askForReviewDontAskAgain(string empty)
	{
		if (askForReviewDontAskAgainEvent != null)
		{
			askForReviewDontAskAgainEvent();
		}
	}

	public void inlineWebViewJSCallback(string message)
	{
		inlineWebViewJSCallbackEvent.fire(message);
	}

	public void notificationReceived(string extraData)
	{
		notificationReceivedEvent.fire(extraData);
	}

	private void contactsLoaded(string json)
	{
		if (contactsLoadedEvent != null)
		{
			List<EtceteraAndroid.Contact> obj = Json.decode<List<EtceteraAndroid.Contact>>(json);
			contactsLoadedEvent(obj);
		}
	}
}
