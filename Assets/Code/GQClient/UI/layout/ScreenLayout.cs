﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GQ.Client.Conf;
using System;
using GQ.Client.Err;

namespace GQ.Client.UI
{

    /// <summary>
    /// Makes the layout for all screens, i.e. all pages plus all foyer views and all additional full screen views (imprint, author login etc.).
    /// </summary>
    public class ScreenLayout : LayoutConfig
    {

        public GameObject TopMargin;
        public GameObject ContentArea;
        public GameObject Divider;
        public GameObject BottomMargin;
        public GameObject Footer;

        public override void layout()
        {
            if (ConfigurationManager.Current == null)
                return;

            setMainBackground();

            setContent();
            setTopMargin();
            setDivider();
            setBottomMargin();
            setFooter();
        }

        protected virtual void setMainBackground()
        {
            Image image = GetComponent<Image>();
            if (image == null)
                return;

            image.color = ConfigurationManager.Current.contentBackgroundColor;
        }

        /// <summary>
        /// Sets the height of the content. If the footer is not shown its height will be added to content.
        /// </summary>
        protected virtual void setContent()
        {
            if (ContentArea == null)
            {
                Log.SignalErrorToDeveloper("Content Area is null.");
                return;
            }

            LayoutElement layElem = ContentArea.GetComponent<LayoutElement>();
            if (layElem == null)
            {
                Log.SignalErrorToDeveloper("LayoutElement for Content is null.");
                return;
            }

            if (Footer == null || !Footer.gameObject.activeSelf)
            {
                float extendedHeight = Units2Pixels(ContentHeightUnits + FooterHeightUnits);
                SetLayoutElementHeight(layElem, extendedHeight);
            }
            else
            {
                float height = Units2Pixels(ContentHeightUnits);
                SetLayoutElementHeight(layElem, height);
            }
        }

        protected virtual void setTopMargin()
        {
            if (TopMargin == null)
                return;

            // set background color:
            Image image = TopMargin.GetComponent<Image>();
            if (image != null)
            {
                image.color = ConfigurationManager.Current.contentBackgroundColor;
            }

            LayoutElement layElem = TopMargin.GetComponent<LayoutElement>();
            if (layElem == null)
                return;
            layElem.flexibleHeight = PageController.ContentTopMarginUnits;
            TopMargin.SetActive(PageController.ContentTopMarginUnits > 0f);
        }

        protected virtual void setBottomMargin()
        {
            if (BottomMargin == null)
                return;

            // set background color:
            Image image = BottomMargin.GetComponent<Image>();
            if (image != null)
            {
                image.color = ConfigurationManager.Current.contentBackgroundColor;
            }

            LayoutElement layElem = BottomMargin.GetComponent<LayoutElement>();
            if (layElem == null)
                return;
            layElem.flexibleHeight = PageController.ContentBottomMarginUnits;
            BottomMargin.SetActive(PageController.ContentBottomMarginUnits > 0f);
        }

        protected virtual void setDivider()
        {
            if (Divider == null)
                return;

            // set background color:
            Image image = Divider.GetComponent<Image>();
            if (image != null)
            {
                image.color = ConfigurationManager.Current.contentBackgroundColor;
            }

            LayoutElement layElem = Divider.GetComponent<LayoutElement>();
            if (layElem == null)
                return;

            layElem.flexibleHeight = PageController.ContentDividerUnits;
            Divider.SetActive(PageController.ContentDividerUnits > 0f);
        }

        protected virtual void setFooter()
        {
            if (Footer == null)
            {
                return;
            }

            // set background color:
            Image image = Footer.GetComponent<Image>();
            if (image != null)
            {
                image.color = ConfigurationManager.Current.footerBgColor;
            }

            LayoutElement layElem = Footer.GetComponent<LayoutElement>();
            if (layElem != null)
            {
                layElem.minHeight = LayoutConfig.Units2Pixels(LayoutConfig.FooterHeightUnits);
                layElem.preferredHeight = layElem.minHeight;
            }
        }

        #region Static Helpers

        protected enum ListEntryKind
        {
            QuestInfo,
            Menu
        }

        protected static void SetEntryLayout(float heightUnits, GameObject menuEntry, ListEntryKind listEntryKind, string gameObjectPath = null, float sizeScaleFactor = 1f, Color? fgColor = null)
        {
            Color fgCol = (Color)((fgColor == null) ? ConfigurationManager.Current.mainFgColor : fgColor);

            // set layout height:
            Transform transf = (gameObjectPath == null ? menuEntry.transform : menuEntry.transform.Find(gameObjectPath));
            if (transf != null)
            {
                LayoutElement layElem = transf.GetComponent<LayoutElement>();
                if (layElem != null)
                {
                    layElem.minHeight = Units2Pixels(heightUnits) * sizeScaleFactor;
                    layElem.preferredHeight = layElem.minHeight;
                    float height = Math.Min(100f, layElem.minHeight);
                    layElem.minWidth = height;
                    layElem.preferredWidth = height;
                }

                Image image = transf.GetComponent<Image>();
                // for images we set the width too:
                if (image != null)
                {
                    transf.GetComponent<Image>().color = fgCol;
                }

                // for texts we adapt the font size to be at most two thirds of the container element height:
                Text text = transf.GetComponent<Text>();
                if (text != null)
                {
                    switch (listEntryKind)
                    {
                        case ListEntryKind.Menu:
                            text.fontSize = (int)Math.Floor(layElem.minHeight * 0.66f * sizeScaleFactor);
                            break;
                        case ListEntryKind.QuestInfo:
                            float fontSizeFactor = ConfigurationManager.Current.listEntryUseTwoLines ? 2.7f : 1.53f;
                            // these factors have been determined by some manual test measures on the UI.
                            text.fontSize =
                                Math.Min(
                                    ConfigurationManager.Current.maxFontSize,
                                    (int)Math.Floor(layElem.minHeight / fontSizeFactor));
                            break;
                    }
                    text.color = fgCol;
                }

                // for Buttons' normal color we set the fgColor if there is no image, otherwise we set it to white:
                Button button = transf.GetComponent<Button>();
                if (button != null)
                {
                    ColorBlock newColors = button.colors;
                    newColors.normalColor = (image == null ? fgCol : Color.white);
                    button.colors = newColors;
                }
            }
            else
            {
                Log.SignalErrorToDeveloper("In gameobject {0} path {1} did not lead to another gameobject.", menuEntry.gameObject, gameObjectPath);
            }
        }

        protected static void SetMenuEntryLayout(float heightUnits, GameObject menuEntry, string gameObjectPath = null, float sizeScaleFactor = 1f, Color? fgColor = null)
        {
            SetEntryLayout(heightUnits, menuEntry, ListEntryKind.Menu, gameObjectPath, sizeScaleFactor, fgColor);
        }

        protected static void SetQuestInfoEntryLayout(float heightUnits, GameObject menuEntry, string gameObjectPath = null, float sizeScaleFactor = 1f, Color? fgColor = null)
        {
            SetEntryLayout(heightUnits, menuEntry, ListEntryKind.QuestInfo, gameObjectPath, sizeScaleFactor, fgColor);
        }

        #endregion
    }

}