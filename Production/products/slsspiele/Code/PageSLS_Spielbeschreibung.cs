﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System;
using System.Xml;

namespace GQ.Client.Model
{

	/// <summary>
	/// SLS Quests have a main page that relies on the MetaData page. Hence, its keys and values are stored in the globally accessible dictionary 
	/// QuestManager.CurrentlyParsingQuest.metadata<string, string>.
	/// 
	/// Thus, the model here is pretty simple.
	/// 
	/// The UI Controller does the whiole work using the global metadata dictionary.
	/// </summary>
	[XmlRoot (GQML.PAGE)]
	public class PageSLS_Spielbeschreibung : PageMetaData
	{
		#region State

		public string KeyWords { get; set; }
		public string Necessities { get; set; }
		public string ImageUrl { get; set; }

		#endregion


		#region XML Serialization

		protected override void ReadContent (XmlReader reader, XmlRootAttribute xmlRootAttr)
		{
			switch (reader.LocalName) {
			case GQML.PAGE_METADATA_STRINGMETA:
				xmlRootAttr.ElementName = GQML.PAGE_METADATA_STRINGMETA;
				XmlSerializer serializer = new XmlSerializer (typeof(StringMetaData), xmlRootAttr);
				StringMetaData smde = (StringMetaData)serializer.Deserialize (reader);
				if (smde.Key == null)
					break;
				if (!QuestManager.CurrentlyParsingQuest.metadata.ContainsKey (smde.Key)) {
					switch (smde.Key) {
					case "Bild":
						ImageUrl = smde.Value;
						QuestManager.CurrentlyParsingQuest.AddMedia (ImageUrl);
						break;
					case "Stichworte":
						KeyWords = smde.Value;
						break;
					case "Material":
						Necessities = smde.Value;
						break;
					default:
						QuestManager.CurrentlyParsingQuest.metadata.Add (smde.Key, smde.Value);
						break;
					}
				}
				break;
			default:
				base.ReadContent (reader, xmlRootAttr);
				break;
			}
		}

		#endregion


		#region Runtime API

		public override bool CanStart ()
		{
			return true;
		}

		#endregion


	}
		
}