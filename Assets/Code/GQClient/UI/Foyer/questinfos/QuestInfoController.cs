﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GQ.Client.Model;
using UnityEngine.UI;
using GQ.Client.Err;
using System;

namespace GQ.Client.UI.Foyer
{
	/// <summary>
	/// Abstact class for all kinds of view controllers presenting quest infos. E.g. list elements in the foyer list, or markers on the foyer map.
	/// </summary>
	public abstract class QuestInfoController : PrefabController, IComparable<QuestInfoController> {

		protected QuestInfo data;
		public Text Name;

		/// <summary>
		/// Returns a value greater than zero in case this object is considered greater than the given other. 
		/// A return value of 0 signals that both objects are equal and 
		/// a value less than zero means that this object is less than the given other one.
		/// </summary>
		/// <param name="otherCtrl">Other ctrl.</param>
		public int CompareTo (QuestInfoController otherCtrl)
		{
			return data.CompareTo (otherCtrl.data);
		}



		abstract public void UpdateView ();

		public override void Destroy ()
		{
			base.Destroy ();
		}

		void OnDestroy ()
		{
			if (this == null) {
				Log.SignalErrorToDeveloper ("QICtrl: this == null in OnDestroy()".Red ());
			} else {
				if (data != null)
					data.OnChanged -= UpdateView;
			}
		}
			
	}

}
