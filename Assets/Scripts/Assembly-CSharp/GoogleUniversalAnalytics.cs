#pragma warning disable 0618,0619
using System.Text;
using UnityEngine;

public sealed class GoogleUniversalAnalytics
{
	public enum HitType
	{
		Pageview = 0,
		Appview = 1,
		Event = 2,
		Transaction = 3,
		Item = 4,
		Social = 5,
		Exception = 6,
		Timing = 7,
		None = 8
	}

	private delegate string Delegate_escapeString(string s);

	private static readonly string[] hitTypeNames = new string[9] { "pageview", "appview", "event", "transaction", "item", "social", "exception", "timing", "none" };

	private static readonly string httpCollectUrl = "http://www.google-analytics.com/collect";

	private static readonly string httpsCollectUrl = "https://ssl.google-analytics.com/collect";

	private static readonly string guaVersionData = "v=1";

	private string defaultHitData;

	private StringBuilder sb = new StringBuilder(256, 8192);

	private HitType hitType = HitType.None;

	private Delegate_escapeString escapeString = WWW.EscapeURL;

	private static readonly GoogleUniversalAnalytics instance = new GoogleUniversalAnalytics();

	public bool useHTTPS;

	public string trackingID;

	public string clientID;

	public string appName;

	public string appVersion;

	public static GoogleUniversalAnalytics Instance
	{
		get
		{
			return instance;
		}
	}

	private static string returnStringAsIs(string s)
	{
		return s;
	}

	public void initialize(string trackingID, string anonymousClientID, string appName = "", string appVersion = "", bool useHTTPS = false)
	{
		this.trackingID = escapeString(trackingID);
		this.useHTTPS = useHTTPS;
		clientID = WWW.EscapeURL(anonymousClientID);
		this.appName = ((appName == null || appName.Length <= 0) ? null : WWW.EscapeURL(appName));
		this.appVersion = ((appVersion == null || appVersion.Length <= 0) ? null : WWW.EscapeURL(appVersion));
		sb.Length = 0;
		sb.Append(guaVersionData);
		sb.Append("&tid=");
		sb.Append(this.trackingID);
		sb.Append("&cid=");
		sb.Append(clientID);
		if (this.appName != null && this.appName.Length > 0)
		{
			sb.Append("&an=");
			sb.Append(this.appName);
		}
		defaultHitData = sb.ToString();
		sb.Length = 0;
	}

	public void setStringEscaping(bool useStringEscaping)
	{
		if (useStringEscaping)
		{
			escapeString = WWW.EscapeURL;
		}
		else
		{
			escapeString = returnStringAsIs;
		}
	}

	public bool beginHit(HitType hitType)
	{
		string value = hitTypeNames[(int)hitType];
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			return false;
		}
		this.hitType = hitType;
		sb.Length = 0;
		sb.Append(defaultHitData);
		sb.Append("&t=");
		sb.Append(value);
		return true;
	}

	public void addAnonymizeIP()
	{
		sb.Append("&aip=1");
	}

	public void addQueueTime(int ms)
	{
		if (ms >= 0)
		{
			sb.Append("&qt=");
			sb.Append(ms);
		}
	}

	public void addSessionControl(bool type)
	{
		sb.Append((!type) ? "&sc=end" : "&sc=start");
	}

	public void addDocumentReferrer(string url, bool allowNonEscaped = false)
	{
		string value = ((!allowNonEscaped) ? WWW.EscapeURL(url) : escapeString(url));
		sb.Append("&dr=");
		sb.Append(value);
	}

	public void addCampaignName(string text)
	{
		string value = escapeString(text);
		sb.Append("&cn=");
		sb.Append(value);
	}

	public void addCampaignSource(string text)
	{
		string value = escapeString(text);
		sb.Append("&cs=");
		sb.Append(value);
	}

	public void addCampaignMedium(string text)
	{
		string value = escapeString(text);
		sb.Append("&cm=");
		sb.Append(value);
	}

	public void addCampaignKeyword(string text, bool allowNonEscaped = false)
	{
		string value = ((!allowNonEscaped) ? WWW.EscapeURL(text) : escapeString(text));
		sb.Append("&ck=");
		sb.Append(value);
	}

	public void addCampaignContent(string text, bool allowNonEscaped = false)
	{
		string value = ((!allowNonEscaped) ? WWW.EscapeURL(text) : escapeString(text));
		sb.Append("&cc=");
		sb.Append(value);
	}

	public void addCampaignID(string text)
	{
		string value = escapeString(text);
		sb.Append("&ci=");
		sb.Append(value);
	}

	public void addGoogleAdWordsID(string text)
	{
		string value = escapeString(text);
		sb.Append("&gclid=");
		sb.Append(value);
	}

	public void addGoogleDisplayAdsID(string text)
	{
		string value = escapeString(text);
		sb.Append("&dclid=");
		sb.Append(value);
	}

	public void addScreenResolution(int width, int height)
	{
		sb.Append("&sr=");
		sb.Append(width);
		sb.Append('x');
		sb.Append(height);
	}

	public void addViewportSize(int width, int height)
	{
		sb.Append("&vp=");
		sb.Append(width);
		sb.Append('x');
		sb.Append(height);
	}

	public void addDocumentEncoding(string text)
	{
		string value = escapeString(text);
		sb.Append("&de=");
		sb.Append(value);
	}

	public void addScreenColors(int depthBits)
	{
		sb.Append("&sd=");
		sb.Append(depthBits);
		sb.Append("-bits");
	}

	public void addUserLanguage(string text)
	{
		string value = escapeString(text);
		sb.Append("&ul=");
		sb.Append(value);
	}

	public void addJavaEnabled(bool enabled)
	{
		sb.Append((!enabled) ? "&je=0" : "&je=1");
	}

	public void addFlashVersion(int major, int minor, int revision)
	{
		sb.Append("&fl=");
		sb.Append(major);
		sb.Append("%20");
		sb.Append(minor);
		sb.Append("%20r");
		sb.Append(revision);
	}

	public void addNonInteractionHit()
	{
		sb.Append("&ni=1");
	}

	public void addDocumentLocationURL(string url, bool allowNonEscaped = false)
	{
		string value = ((!allowNonEscaped) ? WWW.EscapeURL(url) : escapeString(url));
		sb.Append("&dl=");
		sb.Append(value);
	}

	public void addDocumentHostName(string text)
	{
		string value = escapeString(text);
		sb.Append("&dh=");
		sb.Append(value);
	}

	public void addDocumentPath(string text, bool allowNonEscaped = false)
	{
		string value = ((!allowNonEscaped) ? WWW.EscapeURL(text) : escapeString(text));
		sb.Append("&dp=");
		sb.Append(value);
	}

	public void addDocumentTitle(string text)
	{
		string value = escapeString(text);
		sb.Append("&dt=");
		sb.Append(value);
	}

	public void addContentDescription(string text)
	{
		string value = escapeString(text);
		sb.Append("&cd=");
		sb.Append(value);
	}

	public void addApplicationVersion(string text = null)
	{
		string s = ((text != null) ? text : appVersion);
		string value = escapeString(s);
		sb.Append("&av=");
		sb.Append(value);
	}

	public void addEventCategory(string text)
	{
		if (hitType == HitType.Event)
		{
			string value = escapeString(text);
			sb.Append("&ec=");
			sb.Append(value);
		}
	}

	public void addEventAction(string text)
	{
		if (hitType == HitType.Event)
		{
			string value = escapeString(text);
			sb.Append("&ea=");
			sb.Append(value);
		}
	}

	public void addEventLabel(string text)
	{
		if (hitType == HitType.Event)
		{
			string value = escapeString(text);
			sb.Append("&el=");
			sb.Append(value);
		}
	}

	public void addEventValue(int value)
	{
		if (hitType == HitType.Event && value >= 0)
		{
			sb.Append("&ev=");
			sb.Append(value);
		}
	}

	public void addTransactionID(string text)
	{
		if (hitType == HitType.Transaction || hitType == HitType.Item)
		{
			string value = escapeString(text);
			sb.Append("&ti=");
			sb.Append(value);
		}
	}

	public void addTransactionAffiliation(string text)
	{
		if (hitType == HitType.Transaction)
		{
			string value = escapeString(text);
			sb.Append("&ta=");
			sb.Append(value);
		}
	}

	public void addTransactionRevenue(double currency)
	{
		if (hitType == HitType.Transaction)
		{
			sb.Append("&tr=");
			sb.AppendFormat("{0:F6}", currency);
		}
	}

	public void addTransactionShipping(double currency)
	{
		if (hitType == HitType.Transaction)
		{
			sb.Append("&ts=");
			sb.AppendFormat("{0:F6}", currency);
		}
	}

	public void addTransactionTax(double currency)
	{
		if (hitType == HitType.Transaction)
		{
			sb.Append("&tt=");
			sb.AppendFormat("{0:F6}", currency);
		}
	}

	public void addItemName(string text)
	{
		if (hitType == HitType.Item)
		{
			string value = escapeString(text);
			sb.Append("&in=");
			sb.Append(value);
		}
	}

	public void addItemPrice(double currency)
	{
		if (hitType == HitType.Item)
		{
			sb.Append("&ip=");
			sb.AppendFormat("{0:F6}", currency);
		}
	}

	public void addItemQuantity(int value)
	{
		if (hitType == HitType.Item)
		{
			sb.Append("&iq=");
			sb.Append(value);
		}
	}

	public void addItemCode(string text)
	{
		if (hitType == HitType.Item)
		{
			string value = escapeString(text);
			sb.Append("&ic=");
			sb.Append(value);
		}
	}

	public void addItemCategory(string text)
	{
		if (hitType == HitType.Item)
		{
			string value = escapeString(text);
			sb.Append("&iv=");
			sb.Append(value);
		}
	}

	public void addCurrencyCode(string text)
	{
		if (hitType == HitType.Transaction || hitType == HitType.Item)
		{
			string value = escapeString(text);
			sb.Append("&cu=");
			sb.Append(value);
		}
	}

	public void addSocialNetwork(string text)
	{
		if (hitType == HitType.Social)
		{
			string value = escapeString(text);
			sb.Append("&sn=");
			sb.Append(value);
		}
	}

	public void addSocialAction(string text)
	{
		if (hitType == HitType.Social)
		{
			string value = escapeString(text);
			sb.Append("&sa=");
			sb.Append(value);
		}
	}

	public void addSocialActionTarget(string text, bool allowNonEscaped = false)
	{
		if (hitType == HitType.Social)
		{
			string value = ((!allowNonEscaped) ? WWW.EscapeURL(text) : escapeString(text));
			sb.Append("&st=");
			sb.Append(value);
		}
	}

	public void addUserTimingCategory(string text)
	{
		if (hitType == HitType.Timing)
		{
			string value = escapeString(text);
			sb.Append("&utc=");
			sb.Append(value);
		}
	}

	public void addUserTimingVariableName(string text)
	{
		if (hitType == HitType.Timing)
		{
			string value = escapeString(text);
			sb.Append("&utv=");
			sb.Append(value);
		}
	}

	public void addUserTimingTime(int value)
	{
		if (hitType == HitType.Timing)
		{
			sb.Append("&utt=");
			sb.Append(value);
		}
	}

	public void addUserTimingLabel(string text)
	{
		if (hitType == HitType.Timing)
		{
			string value = escapeString(text);
			sb.Append("&utl=");
			sb.Append(value);
		}
	}

	public void addPageLoadTime(int value)
	{
		if (hitType == HitType.Timing)
		{
			sb.Append("&plt=");
			sb.Append(value);
		}
	}

	public void addDNSTime(int value)
	{
		if (hitType == HitType.Timing)
		{
			sb.Append("&dns=");
			sb.Append(value);
		}
	}

	public void addPageDownloadTime(int value)
	{
		if (hitType == HitType.Timing)
		{
			sb.Append("&pdt=");
			sb.Append(value);
		}
	}

	public void addRedirectResponseTime(int value)
	{
		if (hitType == HitType.Timing)
		{
			sb.Append("&rrt=");
			sb.Append(value);
		}
	}

	public void addTCPConnectTime(int value)
	{
		if (hitType == HitType.Timing)
		{
			sb.Append("&tcp=");
			sb.Append(value);
		}
	}

	public void addServerResponseTime(int value)
	{
		if (hitType == HitType.Timing)
		{
			sb.Append("&srt=");
			sb.Append(value);
		}
	}

	public void addExceptionDescription(string text)
	{
		if (hitType == HitType.Exception)
		{
			string value = escapeString(text);
			sb.Append("&exd=");
			sb.Append(value);
		}
	}

	public void addExceptionIsFatal(bool value)
	{
		if (hitType == HitType.Exception)
		{
			sb.Append((!value) ? "&exf=0" : "&exf=1");
		}
	}

	public void addCustomDimension(int index, string text)
	{
		if (index >= 1 && index <= 200)
		{
			string value = escapeString(text);
			sb.Append("&cd");
			sb.Append(index);
			sb.Append('=');
			sb.Append(value);
		}
	}

	public void addCustomMetric(int index, long value)
	{
		if (index >= 1 && index <= 200)
		{
			sb.Append("&cm");
			sb.Append(index);
			sb.Append('=');
			sb.Append(value);
		}
	}

	public void sendPageViewHit(string documentHostName, string documentPath, string documentTitle)
	{
		beginHit(HitType.Pageview);
		addDocumentHostName(documentHostName);
		addDocumentPath(documentPath);
		addDocumentTitle(documentTitle);
		sendHit();
	}

	public void sendEventHit(string eventCategory, string eventAction, string eventLabel = null, int eventValue = -1)
	{
		beginHit(HitType.Event);
		addEventCategory(eventCategory);
		addEventAction(eventAction);
		if (eventLabel != null)
		{
			addEventLabel(eventLabel);
		}
		if (eventValue >= 0)
		{
			addEventValue(eventValue);
		}
		sendHit();
	}

	public void sendTransactionHit(string transactionID, string affiliation, double revenue, double shipping, double tax, string currencyCode)
	{
		beginHit(HitType.Transaction);
		addTransactionID(transactionID);
		if (affiliation != null && affiliation.Length > 0)
		{
			addTransactionAffiliation(affiliation);
		}
		if (revenue != 0.0)
		{
			addTransactionRevenue(revenue);
		}
		if (shipping != 0.0)
		{
			addTransactionShipping(shipping);
		}
		if (tax != 0.0)
		{
			addTransactionTax(tax);
		}
		if (currencyCode != null && currencyCode.Length > 0)
		{
			addCurrencyCode(currencyCode);
		}
		sendHit();
	}

	public void sendItemHit(string transactionID, string itemName, double price, int quantity, string itemCode, string itemCategory, string currencyCode)
	{
		beginHit(HitType.Item);
		addTransactionID(transactionID);
		addItemName(itemName);
		if (price != 0.0)
		{
			addItemPrice(price);
		}
		if (quantity != 0)
		{
			addItemQuantity(quantity);
		}
		if (itemCode != null && itemCode.Length > 0)
		{
			addItemCode(itemCode);
		}
		if (itemCategory != null && itemCategory.Length > 0)
		{
			addItemCategory(itemCategory);
		}
		if (currencyCode != null && currencyCode.Length > 0)
		{
			addCurrencyCode(currencyCode);
		}
		sendHit();
	}

	public void sendSocialHit(string network, string action, string target)
	{
		beginHit(HitType.Social);
		addSocialNetwork(network);
		addSocialAction(action);
		addSocialActionTarget(target);
		sendHit();
	}

	public void sendExceptionHit(string description, bool isFatal)
	{
		beginHit(HitType.Exception);
		if (description != null && description.Length > 0)
		{
			addExceptionDescription(description);
		}
		addExceptionIsFatal(isFatal);
		sendHit();
	}

	public void sendUserTimingHit(string category, string variable, int time, string label)
	{
		beginHit(HitType.Timing);
		if (category != null && category.Length > 0)
		{
			addUserTimingCategory(category);
		}
		if (variable != null && variable.Length > 0)
		{
			addUserTimingVariableName(variable);
		}
		if (time >= 0)
		{
			addUserTimingTime(time);
		}
		if (label != null && label.Length > 0)
		{
			addUserTimingLabel(label);
		}
		sendHit();
	}

	public void sendBrowserTimingHit(int dnsTime, int pageDownloadTime, int redirectTime, int tcpConnectTime, int serverResponseTime)
	{
		beginHit(HitType.Timing);
		if (dnsTime >= 0)
		{
			addDNSTime(dnsTime);
		}
		if (pageDownloadTime >= 0)
		{
			addPageDownloadTime(pageDownloadTime);
		}
		if (redirectTime >= 0)
		{
			addRedirectResponseTime(redirectTime);
		}
		if (tcpConnectTime >= 0)
		{
			addTCPConnectTime(tcpConnectTime);
		}
		if (serverResponseTime >= 0)
		{
			addServerResponseTime(serverResponseTime);
		}
		sendHit();
	}

	public void sendAppScreenHit(string screenName)
	{
		beginHit(HitType.Appview);
		addApplicationVersion();
		addContentDescription(screenName);
		sendHit();
	}

	public bool sendHit()
	{
		if (hitType == HitType.None || Application.internetReachability == NetworkReachability.NotReachable)
		{
			return false;
		}
		sb.Append("&z=");
		sb.Append(Random.Range(0, int.MaxValue) ^ 0x13377AA7);
		string s = sb.ToString();
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		sb.Length = 0;
		hitType = HitType.None;
		new WWW((!useHTTPS) ? httpCollectUrl : httpsCollectUrl, bytes);
		return true;
	}
}
