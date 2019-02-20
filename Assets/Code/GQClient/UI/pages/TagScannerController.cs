﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GQ.Client.Model;
using GQ.Client.Util;
using GQ.Client.Conf;
using System.Threading;
using ZXing;
using GQ.Client.Err;

namespace GQ.Client.UI
{
    public class TagScannerController : PageController
    {

        #region Inspector Features

        public Text prompt;
        public Text scannedText;
        public Text forwardButtonText;

        WebCamTexture camTexture;

        public RawImage camQRImage;

        private Thread qrThread;
        private Color32[] c;

        #endregion


        #region Runtime API

        protected PageTagScanner myPage;

        /// <summary>
        /// Is called during Start() of the base class, which is a MonoBehaviour.
        /// </summary>
        public override void Initialize()
        {
            Debug.Log(("PageTagScanner starting, page has type: " + page.GetType().Name).Yellow());
            myPage = (PageTagScanner)page;

            // show the content:
            prompt.color = ConfigurationManager.Current.mainFgColor;
            prompt.fontSize = ConfigurationManager.Current.mainFontSize;
            prompt.text = myPage.Prompt.Decode4HyperText();
            forwardButtonText.text = "Ok";

            CoroutineStarter.Run(InitQRCamera());
        }

        private int W, H;

        private IEnumerator InitQRCamera()
        {
            string deviceName = null;
            foreach (WebCamDevice wcd in WebCamTexture.devices)
            {
                if (!wcd.isFrontFacing)
                {
                    deviceName = wcd.name;
                    break;
                }
            }

            camTexture = new WebCamTexture(deviceName)
            {
                // request a resolution that is enough to scan qr codes reliably:
                requestedHeight = 480,
                requestedWidth = 640
            };

            camTexture.Play();

            // wait for web cam to be ready which is guaranteed after first image update:
            while (!camTexture.didUpdateThisFrame)
                yield return null;

            // scale height according to camera aspect ratio:
            float xScale = 1F;
            float yScale = ((float)camTexture.height / (float)camTexture.width) * (camTexture.videoVerticallyMirrored ? -1F : 1F);

            // scale to fill:
            float fillScale = 1;
            float minHeight = ((RectTransform)camQRImage.transform.parent).rect.height;
            float minWidth = ((RectTransform)camQRImage.transform.parent).rect.width;
            float isHeight = camQRImage.rectTransform.rect.height * yScale;
            float isWidth = camQRImage.rectTransform.rect.width;
            if (minHeight > isHeight)
                fillScale = Mathf.Max(minHeight / isHeight, fillScale);
            if (minWidth > isWidth)
                fillScale = Mathf.Max(minWidth / isWidth, fillScale);
            xScale *= fillScale;
            yScale *= fillScale;

            // correct shown texture according to webcam details:
            camQRImage.transform.rotation *= Quaternion.AngleAxis(camTexture.videoRotationAngle, Vector3.back);
            camQRImage.transform.localScale = new Vector3(xScale, yScale, 1F);

            camQRImage.texture = camTexture;
            W = camTexture.width;
            H = camTexture.height;
            c = new Color32[W * H];

            qrThread = new Thread(DecodeQR);
            qrThread.Start();
        }

        private bool decoderRunning = false;
        private string qrContent = null;
        private bool pixelsShouldBeDecoded = false;
        private bool scannedTextShouldBeChecked = false;

        void DecodeQR()
        {
            // create a reader with a custom luminance source

            var barcodeReader = new BarcodeReader
            {
                AutoRotate = false,
                TryHarder = false
            };

            decoderRunning = true;

            while (decoderRunning)
            {
                try
                {
                    // decode the current frame
                    if (pixelsShouldBeDecoded)
                    {
                        qrContent = barcodeReader.Decode(c, W, H).Text;
                        scannedTextShouldBeChecked = true;
                        pixelsShouldBeDecoded = false;
                    }

                    // Sleep a little bit and set the signal to get the next frame
                    Thread.Sleep(200);
                }
                catch
                {
                    continue;
                }
            }
        }

        private string lastQrResult = "";

        void Update()
        {
            if (scannedTextShouldBeChecked)
            {
                checkResult(qrContent);
                scannedTextShouldBeChecked = false;
            }

            if (camTexture.didUpdateThisFrame)
            {
                camTexture.GetPixels32(c);
                pixelsShouldBeDecoded = true;
            }
        }


        private void checkResult(string result)
        {
            Debug.Log("QR CODE gescannt:" + result);

            myPage.Result = result;

            if (myPage.ShowTagContent)
            {
                scannedText.text = result;
                //ergebnis_text.enabled = true;
                //ergebnis_textbg.enabled = true;
            }

            if (myPage.AnswerCorrect(result))
            {
                myPage.Succeed();
            }
            else
            {
                myPage.Fail();
            }
        }


        public override void OnForward()
        {
            if (myPage.AnswerCorrect(scannedText.text))
            {
                myPage.Succeed();
            }
            else
            {
                myPage.Fail();
            }
        }

        void OnDisable()
        {
            Debug.Log("OnDisable()");
            if (camTexture != null)
                camTexture.Pause();
        }

        void OnDestroy()
        {
            Debug.Log("OnDestroy()");

            if (qrThread != null)
            {
                decoderRunning = false;
            }

            if (camTexture != null)
                camTexture.Stop();
        }

        #endregion
    }
}
