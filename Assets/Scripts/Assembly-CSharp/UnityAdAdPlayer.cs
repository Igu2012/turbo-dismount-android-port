#pragma warning disable 0618,0619
using UnityEngine;

public class UnityAdAdPlayer : AdPlayer
{
    public UnityAdAdPlayer(int priority, int prioritizedAdCount, string appId, bool testMode = false)
        : base(priority, prioritizedAdCount) { }

    public override void ShowAd()
    {
        if (OnVideoFinished != null) OnVideoFinished(false);
    }
}
