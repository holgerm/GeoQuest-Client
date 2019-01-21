﻿using UnityEngine;
using System.Collections;
using GQ.Client.Model;
using System.Xml.Serialization;
using GQ.Client.Err;
using System.Xml;
using GQ.Client.Util;

namespace GQ.Client.Model
{

	public class ActionPlayAudio : ActionAbstract
	{
		#region State

		public string AudioUrl { get; set; }

		public bool Loop { get; set; }

		public bool StopOthers { get; set; }

		#endregion


		#region Structure

		protected override void ReadAttributes (XmlReader reader)
		{
			Loop = GQML.GetOptionalBoolAttribute (GQML.ACTION_PLAYAUDIO_LOOP, reader, false);
			StopOthers = GQML.GetOptionalBoolAttribute (GQML.ACTION_PLAYAUDIO_STOPOTHERS, reader, true);

			AudioUrl = GQML.GetStringAttribute (GQML.ACTION_PLAYAUDIO_FILE, reader);
			QuestManager.CurrentlyParsingQuest.AddMedia (AudioUrl, "PlayAudio." + GQML.ACTION_PLAYAUDIO_FILE);
		}

		#endregion


		#region Functions

		public override void Execute ()
		{
            Audio.PlayFromMediaStore(AudioUrl, Loop, StopOthers);
		}

		#endregion
	}
}
