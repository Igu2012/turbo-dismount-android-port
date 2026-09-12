public class AdColonyAd
{
	public bool shown;

	public bool iapEnabled;

	public string productID;

	public IAPEngagementType iapEngagementType;

	public AdColonyAd(string[] split_args)
	{
		shown = split_args[0].Equals("true");
		iapEnabled = split_args[1].Equals("true");
		switch (split_args[2])
		{
		case "END_CARD":
			iapEngagementType = IAPEngagementType.END_CARD;
			break;
		case "OVERLAY":
			iapEngagementType = IAPEngagementType.OVERLAY;
			break;
		case "AUTOMATIC":
			iapEngagementType = IAPEngagementType.AUTOMATIC;
			break;
		default:
			iapEngagementType = IAPEngagementType.NONE;
			break;
		}
		productID = split_args[3];
	}

	public string toString()
	{
		return "AdColonyAdInfo- Shown:" + shown + ", IAPEnabled: " + iapEnabled + ", productID:" + productID + ", IAPEngagementType: " + iapEngagementType;
	}
}
