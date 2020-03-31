﻿using System.Collections;
using Code.GQClient.Conf;
using Code.GQClient.Err;
using Code.GQClient.Model.gqml;
using Code.GQClient.Model.pages;
using Code.GQClient.UI.layout;
using Code.GQClient.Util;
using Code.QM.Util;
using TMPro;
using UnityEngine;

namespace Code.GQClient.UI.pages.videoplayer
{
    public static class WebViewExtras
    {
        public static UniWebView uniWebView;

        public static void Initialize(PageVideoPlay myPage, GameObject containerWebPlayer)
        {
            switch (myPage.VideoType)
            {
                case GQML.PAGE_VIDEOPLAY_VIDEOTYPE_YOUTUBE:
                    // USE HTML WEBVIEW FOR VIDEO:                    
                    uniWebView = containerWebPlayer.GetComponent<UniWebView>();
                    if (uniWebView == null)
                    {
                        uniWebView = containerWebPlayer.AddComponent<UniWebView>();
                    }

                    uniWebView.OnPageErrorReceived += (UniWebView webView, int errorCode, string errorMessage) =>
                    {
                        Log.SignalErrorToDeveloper("YOUTUBE PLAYER: OnPageErrorReceived errCode: " + errorCode
                                                                                                   + "\n\terrMessage: " +
                                                                                                   errorMessage);
                    };
                    uniWebView.OnShouldClose += (webView) =>
                    {
                        Debug.Log("YOUTUBE PLAYER: OnShouldClose.");
                        //webView = null;
                        containerWebPlayer.SetActive(false);
                        return true;
                    };

                    UniWebViewLogger.Instance.LogLevel = UniWebViewLogger.Level.Verbose;

                    containerWebPlayer.SetActive(true);

                    myPage.PageCtrl.FooterButtonPanel =
                        ((VideoPlayController) myPage.PageCtrl).webPlayerFooterButtonPanel;
                    Transform backButtonGO = myPage.PageCtrl.FooterButtonPanel.transform.Find("BackButton");
                    backButtonGO.gameObject.SetActive(myPage.Quest.History.CanGoBackToPreviousPage);

                    float headerHeight = LayoutConfig.Units2Pixels(LayoutConfig.HeaderHeightUnits);
                    float footerHeight = LayoutConfig.Units2Pixels(LayoutConfig.FooterHeightUnits);
                    uniWebView.Frame =
                        new Rect(
                            0, headerHeight,
                            Device.width, Device.height - (headerHeight + footerHeight)
                        );

                    //VideoPlayController vpCtrl = (VideoPlayController)myPage.PageCtrl;
                    //uniWebView.ReferenceRectTransform = vpCtrl.webPlayerContent;
                    uniWebView.SetShowSpinnerWhileLoading(true);
                    uniWebView.Show(true);

                    string videoHtml = string.Format(YoutubeHTMLFormatString, myPage.VideoFile);
                    uniWebView.LoadHTMLString(videoHtml, "https://www.youtube.com/");
                    break;
                default:
                    Log.SignalErrorToAuthor("Unknown video type {0} used on page {1}", myPage.VideoType, myPage.Id);
                    break;
            }
        }

        private static string YoutubeHTMLFormatString =
            @"<html>
                <head></head>
                <body style=""margin:0\"">
                    <iframe width = ""100%"" height=""100%"" 
                        src=""https://www.youtube.com/embed/{0}"" frameborder=""0"" 
                        allow=""autoplay; encrypted-media"" allowfullscreen>
                    </iframe>
                </body>
            </html>";

        public static void Initialize(WebPageController pageCtrl, RectTransform webContainer, string url)
        {
            // show the content:
            var webView = webContainer.GetComponent<UniWebView>();
            if (webView == null)
            {
                webView = webContainer.gameObject.AddComponent<UniWebView>();
            }

            if (ShouldCheckToAllowLeavePage(pageCtrl))
            {
                webView.OnPageFinished += (view, statusCode, curUrl) =>
                {
                    if (!CheckUrlToAllowForwardButton(pageCtrl, curUrl))
                        view.GetHTMLContent((html) => { CheckHtmlToAllowForwardButton(pageCtrl, html); });
                };

                webView.OnPageStarted += (view, curUrl) => { CheckUrlToAllowForwardButton(pageCtrl, curUrl); };
            }

            webView.OnPageErrorReceived += (view, error, message) =>
            {
                // TODO show error message also to user.
                Debug.Log("Error: " + message);
            };

            var headerHeight = LayoutConfig.Units2Pixels(LayoutConfig.HeaderHeightUnits);
            var footerHeight = LayoutConfig.Units2Pixels(LayoutConfig.FooterHeightUnits);
            webView.Frame =
                new Rect(
                    0, headerHeight,
                    Device.width, Device.height - (headerHeight + footerHeight)
                );
            webView.SetShowSpinnerWhileLoading(true);
            webView.Show(true);
            webView.Load(url);
        }

         private static bool ShouldCheckToAllowLeavePage(WebPageController pageCtrl)
        {
            var shouldCheck = pageCtrl.myPage.AllowLeaveOnUrlContains != "";
            shouldCheck |= pageCtrl.myPage.AllowLeaveOnUrlDoesNotContain != "";
            shouldCheck |= pageCtrl.myPage.AllowLeaveOnHtmlContains != "";
            shouldCheck |= pageCtrl.myPage.AllowLeaveOnHtmlDoesNotContain != "";
            return shouldCheck;
        }

        private static bool CheckUrlToAllowForwardButton(WebPageController pageCtrl, string myUrl)
        {
            var allowLeave = myUrl.Contains(pageCtrl.myPage.AllowLeaveOnUrlContains) ||
                             !myUrl.Contains(pageCtrl.myPage.AllowLeaveOnUrlDoesNotContain);
            if (allowLeave)
            {
                AllowLeavePage(pageCtrl);
            }

            return allowLeave;
        }

        private static void AllowLeavePage(WebPageController pageCtrl)
        {
            pageCtrl.ForwardButton.interactable = true;
            var forwardButtonText = pageCtrl.ForwardButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            forwardButtonText.text = pageCtrl.myPage.EndButtonText.Decode4TMP(false);
            pageCtrl.BackButton.interactable =
                true; // might be set even if back button is not shown but does not matter
            pageCtrl.BackButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "<";

            if (pageCtrl.myPage.LeaveOnAllow)
            {
                pageCtrl.myPage.End(false);
            }
        }

        private static void CheckHtmlToAllowForwardButton(WebPageController pageCtrl, string html)
        {
            if (html.Contains(pageCtrl.myPage.AllowLeaveOnHtmlContains) ||
                !html.Contains(pageCtrl.myPage.AllowLeaveOnHtmlDoesNotContain))
            {
                AllowLeavePage(pageCtrl);
            }
        }

        public static void CleanUp(GameObject containerWebPlayer)
        {
            if (uniWebView != null)
            {
                uniWebView.Stop();
                uniWebView.Hide(true);
                Object.Destroy(uniWebView);
            }

            if (containerWebPlayer != null)
            {
                containerWebPlayer.SetActive(false);
            }
        }
    }
}