//using UnityEngine;
//using GoogleMobileAds.Api;
//using System;

//public class AdsInitializer : MonoBehaviour
//{
  //  private InterstitialAd interstitial;

    //void Start()
    //{
      //  MobileAds.Initialize(initStatus => {
        //    Debug.Log("AdMob inicializado");
          //  RequestInterstitial();
        //});
    //}

    //private void RequestInterstitial()
    //{
      //  string adUnitId = "ca-app-pub-9324562557935244/5370834419";

        //AdRequest adRequest = new AdRequest();

        //InterstitialAd.Load(adUnitId, adRequest,
          //  (InterstitialAd ad, LoadAdError error) =>
            //{
              //  if (error != null || ad == null)
                //{
                  //  Debug.LogError("Fallo al cargar anuncio: " + error);
                    //return;
                //}

                //interstitial = ad;
                //Debug.Log("Anuncio cargado con éxito");

                //if (interstitial.CanShowAd())
                //{
                  //  interstitial.Show();
                //}
            //});
        //}
//}