﻿// #define DEBUG_LOG

using System;
using System.Collections;
using Code.GQClient.Conf;
using Code.GQClient.Err;
using Code.GQClient.Model.mgmt.quests;
using Code.GQClient.Model.pages;
using Code.GQClient.UI.layout;
using Code.GQClient.Util;
using Code.GQClient.Util.http;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.GQClient.UI.pages.npctalk
{
    public class NPCTalkController : PageController
    {
        #region Inspector Fields

        public RawImage image;
        public GameObject imagePanel;
        public GameObject contentPanel;
        public Transform dialogItemContainer;

        #endregion

        #region Runtime API

        protected PageNPCTalk npcPage;

        /// <summary>
        /// Is called during Start() of the base class, which is a MonoBehaviour.
        /// </summary>
        public override void InitPage_TypeSpecific()
        {
            try
            {
                npcPage = (PageNPCTalk) page;
            }
            catch (InvalidCastException)
            {
                Debug.Log(("InvalidCastException: NPCTalk Page foun a page of type " + page.GetType()));
            }

            // show the content:
            ShowImage();
            ClearText();
            AddCurrentText();
            UpdateForwardButton();
        }

        public override void OnForward()
        {
            if (npcPage.HasMoreDialogItems())
            {
                npcPage.Next();
                // update the content:
                AddCurrentText();
                UpdateForwardButton();
            }
            else
            {
                npcPage.End();
            }
        }

        public override void CleanUp()
        {
            Destroy(image.texture);
#if DEBUG_LOG
            Debug.Log("NPCTalkController cleaned up image texture.".Red());
#endif
        }

        #endregion

        #region View Update Methods

        /// <summary>
        /// Shows top margin when no image is shown, i.e. text follows directly below header:
        /// </summary>
        public override bool ShowsTopMargin
        {
            get
            {
                if (npcPage == null)
                {
                    return false;
                }
                else
                {
                    return string.IsNullOrEmpty(npcPage.ImageUrl);
                }
            }
        }

        void ShowImage()
        {
            // allow for variables inside the image url:
            var rtImageUrl = npcPage.ImageUrl.MakeReplacements();

            // show (or hide completely) image:
            if (rtImageUrl == "")
            {
                imagePanel.SetActive(false);
                layout.TopMargin.SetActive(true);
                return;
            }
            else
            {
                layout.TopMargin.SetActive(false);
            }

            AbstractDownloader loader;
            if (npcPage.Parent.MediaStore.ContainsKey(rtImageUrl))
            {
                MediaInfo mediaInfo;
                npcPage.Parent.MediaStore.TryGetValue(rtImageUrl, out mediaInfo);
                loader = new LocalFileLoader(mediaInfo.LocalPath);
            }
            else
            {
                loader = new Downloader(
                    url: rtImageUrl,
                    timeout: ConfigurationManager.Current.timeoutMS,
                    maxIdleTime: ConfigurationManager.Current.maxIdleTimeMS
                );
            }

            loader.OnSuccess += (AbstractDownloader d, DownloadEvent e) =>
            {
                var imageAreaHeight = image == null ? 0f : fitInAndShowImage(d.Www.texture);

                imagePanel.GetComponent<LayoutElement>().flexibleHeight = LayoutConfig.Units2Pixels(imageAreaHeight);
                contentPanel.GetComponent<LayoutElement>().flexibleHeight = CalculateMainAreaHeight(imageAreaHeight);

                // Dispose www including it s Texture and take some logs for preformace surveillance:
                d.Www.Dispose();
            };
            loader.Start();
        }

        float fitInAndShowImage(Texture2D texture)
        {
            var fitter = image.GetComponent<AspectRatioFitter>();
            var imageRatio = (float) texture.width / (float) texture.height;
            var imageAreaHeight =
                ContentWidthUnits / imageRatio; // if image fits, so we use its height (adjusted to the area):

            if (imageRatio < ImageRatioMinimum)
            {
                // image too high to fit:
                imageAreaHeight = ConfigurationManager.Current.imageAreaHeightMaxUnits;
            }

            if (ImageRatioMaximum < imageRatio)
            {
                // image too wide to fit:
                imageAreaHeight = ConfigurationManager.Current.imageAreaHeightMinUnits;
            }

            //imagePanel.GetComponent<LayoutElement> ().flexibleHeight = LayoutConfig.Units2Pixels (imageAreaHeight);
            //contentPanel.GetComponent<LayoutElement> ().flexibleHeight = CalculateMainAreaHeight (imageAreaHeight);

            fitter.aspectRatio = imageRatio; // i.e. the adjusted image area aspect ratio
            fitter.aspectMode =
                ConfigurationManager.Current.fitExceedingImagesIntoArea
                    ? AspectRatioFitter.AspectMode.FitInParent
                    : AspectRatioFitter.AspectMode.EnvelopeParent;

            image.texture = texture;
            imagePanel.SetActive(true);

            return imageAreaHeight;
        }

        void ClearText()
        {
            foreach (Transform dialogItem in dialogItemContainer)
            {
                GameObject.Destroy(dialogItem.gameObject);
            }

            var bgImg = contentPanel.GetComponent<Image>();
            if (bgImg != null)
            {
                bgImg.color = ConfigurationManager.Current.mainBgColor;
            }
        }

        void AddCurrentText()
        {
            // decode text for HyperText Component:
            var currentText = npcPage.CurrentDialogItem.Text.Decode4TMP();

            // create dialog item GO from prefab:
            TextElementCtrl.Create(dialogItemContainer, currentText);

            // play audio if specified:
            var duration = 0f;
            if (npcPage.CurrentDialogItem.AudioURL != null && npcPage.CurrentDialogItem.AudioURL != "")
                duration = Audio.PlayFromMediaStore(npcPage.CurrentDialogItem.AudioURL);

            if (ConfigurationManager.Current.autoScrollNewText)
            {
                if (Math.Abs(duration) < 0.01)
                    duration = currentText.Length / 14f;
                // ca. 130 Worten à 6,5 Buchstaben pro Minute siehe https://de.wikipedia.org/wiki/Lesegeschwindigkeit

                // scroll to bottom:
                Base.Instance.StartCoroutine(adjustScrollRect(duration));
            }
        }

        void UpdateForwardButton()
        {
            // update forward button text:
            var forwardButtonText = forwardButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            forwardButtonText.text = npcPage.HasMoreDialogItems()
                ? npcPage.NextDialogButtonText.Decode4TMP(false)
                : npcPage.EndButtonText.Decode4TMP(false);
        }

        private IEnumerator adjustScrollRect(float timespan)
        {
            yield return new WaitForEndOfFrame();

            var usedTime = 0f;
            var startPosition = contentPanel.GetComponent<ScrollRect>().verticalNormalizedPosition;
            float newPos;

            do
            {
                usedTime += Time.deltaTime;
                var share = timespan <= usedTime ? 1f : usedTime / timespan;
                newPos = Mathf.Lerp(startPosition, 0f, share);
                contentPanel.GetComponent<ScrollRect>().verticalNormalizedPosition = newPos;

                yield return null;
                if (contentPanel == null)
                    // if page already left:
                    yield break;
            } while (newPos > 0.0001 && Input.touchCount == 0 && !Input.GetMouseButtonDown(0));

            // when it was not touched scroll to the perfect button:
            if (Input.touchCount == 0 && !Input.GetMouseButtonDown(0))
                contentPanel.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
        }

        #endregion
    }
}