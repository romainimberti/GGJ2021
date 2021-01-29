using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using com.romainimberti.ggj2021.utilities;
using com.romainimberti.ggj2021;

public class AdManager : SingletonBehaviour<AdManager>
{
    //private string appID = ""; //Mine
    private string appID = "ca-app-pub-3940256099942544~3347511713"; //Google

    private BannerView bannerView;
    //private string bannerID = ""; //Mine
    private string bannerID = "ca-app-pub-3940256099942544/6300978111"; //Google

    private InterstitialAd interstitial;
    //private string interstitialVideoID = ""; //Mine
    private string interstitialVideoID = "ca-app-pub-3940256099942544/1033173712"; //Google

    private RewardedAd rewardedAd;
    //private string rewardedAdID = ""; //Mine
    private string rewardedAdID = "ca-app-pub-3940256099942544/5224354917"; //Google

    private Action interstitialVideoAdCallback;
    private Action rewardedAdCallback;

    private void Start()
    {
        /*MobileAds.Initialize(appID);
        if (!Consts.AdsRemoved())
        {
            ShowBanner();
            RequestInterstitial();
        }
        RequestRewardedVideo();*/
    }

    public void RemoveAds()
    {
        Consts.RemoveAds();
        if (bannerView != null)
            HideBanner();
    }

    public void ShowBanner()
    {
        bannerView = new BannerView(bannerID, AdSize.Banner, AdPosition.Top);

        AdRequest request = new AdRequest.Builder().Build();

        bannerView.LoadAd(request);

        bannerView.Show();
    }
    public void HideBanner()
    {
        bannerView.Hide();
    }

    public void RequestRewardedVideo()
    {
        this.rewardedAd = new RewardedAd(rewardedAdID);
        AdRequest request = new AdRequest.Builder().Build();
        this.rewardedAd.LoadAd(request);
        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;
    }

    public void ShowRewardedVideo(Action callback)
    {
        this.rewardedAdCallback = callback;
        if(this.rewardedAd.IsLoaded())
        {
            this.rewardedAd.Show();
        }
        else
        {
            RequestRewardedVideo();
        }
#if UNITY_EDITOR
        HandleUserEarnedReward(null, null);
#endif
    }

    private void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        RequestRewardedVideo();
    }

    private void HandleUserEarnedReward(object sender, Reward args)
    {
        rewardedAdCallback?.Invoke();
    }

    public void RequestInterstitial()
    {
        this.interstitial = new InterstitialAd(interstitialVideoID);
        // Called when an ad request failed to load.
        this.interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when the ad is closed.
        this.interstitial.OnAdClosed += HandleOnAdClosed;
        AdRequest request = new AdRequest.Builder().Build();
        this.interstitial.LoadAd(request);
    }
    public void ShowInterstitial(Action callback=null)
    {
        if (Consts.AdsRemoved() || this.interstitial == null)
        {
            callback?.Invoke();
            return;
        }
        this.interstitialVideoAdCallback = callback;
        if(this.interstitial.IsLoaded())
        {
            this.interstitial.Show();
        }
        else
        {
            RequestInterstitial();
        }
#if UNITY_EDITOR
        HandleOnAdClosed(null, null);
#endif

        RequestInterstitial();
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        this.interstitialVideoAdCallback?.Invoke();
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        this.interstitialVideoAdCallback?.Invoke();
    }

}
