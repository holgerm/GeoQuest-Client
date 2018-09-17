﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GQ.Client.Conf;

namespace GQ.Client.UI
{

	public class QuestListEntryLayout : LayoutConfig
	{

		public override void layout ()
		{
			//// set entry background color:
			//Image image = GetComponent<Image> ();
			//if (image != null) {
			//	image.color = ConfigurationManager.Current.listEntryBgColor;
			//}

			// set heights and colors of text and image:
            FoyerListLayoutConfig.SetListEntryLayout (gameObject, fgColor: ConfigurationManager.Current.listEntryBgColor);
            FoyerListLayoutConfig.SetListEntryLayout (gameObject, "InfoButton", sizeScaleFactor: 0.65f, fgColor: ConfigurationManager.Current.listEntryFgColor);
			FoyerListLayoutConfig.SetListEntryLayout (gameObject, "Name", fgColor: ConfigurationManager.Current.listEntryFgColor);
			FoyerListLayoutConfig.SetListEntryLayout (gameObject, "DownloadButton", fgColor: ConfigurationManager.Current.listEntryFgColor);
			FoyerListLayoutConfig.SetListEntryLayout (gameObject, "StartButton", fgColor: ConfigurationManager.Current.listEntryFgColor);
			FoyerListLayoutConfig.SetListEntryLayout (gameObject, "DeleteButton", fgColor: ConfigurationManager.Current.listEntryFgColor);
			FoyerListLayoutConfig.SetListEntryLayout (gameObject, "UpdateButton", fgColor: ConfigurationManager.Current.listEntryFgColor);

            Debug.Log("COLORS: Layout called.");

        }

    }
}
