using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

public class GoogleAds : Singleton<GoogleAds> {
    BannerView bannerView;
    RewardedAd rewardedAd;
    InterstitialAd interstitialAd;

    void Start() {
        MobileAds.Initialize((InitializationStatus status) => {
            Dictionary<string, AdapterStatus> map = status.getAdapterStatusMap();

            bool allReady = true;

            foreach (KeyValuePair<string, AdapterStatus> entry in map) {
                string adapterClass = entry.Key;
                AdapterStatus adapterStatus = entry.Value;

                Debug.Log($"[AdMob Init] Adapter: {adapterClass} | Status: {adapterStatus.InitializationState}");

                if (adapterStatus.InitializationState != AdapterState.Ready) {
                    allReady = false;
                    Debug.LogWarning($"{entry.Key} not ready: {adapterStatus.Description}");
                }
            }

            if (allReady) {
                LoadInterstitialAd();
                LoadRewardAd();
            }
        });
    }

    public void ShowBanner() {
        if (bannerView == null) {
            CreateBannerView();
        }
        var adRequest = new AdRequest();
        bannerView.LoadAd(adRequest);
    }

    public void ShowInterstitial(Action success = null, Action error = null) {
        if (interstitialAd != null && interstitialAd.CanShowAd()) {
            interstitialAd.OnAdFullScreenContentClosed += () => {
                success?.Invoke();
                LoadInterstitialAd();
            };
            interstitialAd.Show();
        }
        else {
            error?.Invoke();
        }
    }

    public void ShowReward(Action success = null, Action error = null) {
        if (rewardedAd != null && rewardedAd.CanShowAd()) {
            rewardedAd.Show((Reward reward) => {
                success?.Invoke();
                LoadRewardAd();
            });
        }
        else {
            error?.Invoke();
        }
    }


    void LoadInterstitialAd(int retry = 0) {
        if (interstitialAd != null) {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
        var adRequest = new AdRequest();
        InterstitialAd.Load(Configs.Env.INTERSTITIAL_ID, adRequest,
            (InterstitialAd ad, LoadAdError error) => {
                if (error != null || ad == null) {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    if (retry <= 3) {
                        void Retry() {
                            LoadInterstitialAd(retry + 1);
                        }
                        Invoke(nameof(Retry), 2);
                    }
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());
                interstitialAd = ad;
            });
    }


    void LoadRewardAd(int retry = 0) {
        if (rewardedAd != null) {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        var adRequest = new AdRequest();

        RewardedAd.Load(Configs.Env.REWARD_ID, adRequest,
            (RewardedAd ad, LoadAdError error) => {
                if (error != null || ad == null) {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    if (retry <= 3) {
                        void Retry() {
                            LoadRewardAd(retry + 1);
                        }
                        Invoke(nameof(Retry), 2);
                    }
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());
                rewardedAd = ad;
            });
    }


    void CreateBannerView() {
        if (bannerView != null) {
            bannerView.Destroy();
            bannerView = null;
        }
        AdSize size = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(Screen.width);
        bannerView = new BannerView(Configs.Env.BANNER_ID, size, AdPosition.Bottom);
    }
}
