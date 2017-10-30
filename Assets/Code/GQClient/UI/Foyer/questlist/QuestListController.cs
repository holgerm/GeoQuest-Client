﻿using UnityEngine;
using System.Collections;
using GQ.Client.Model;
using System.Collections.Generic;
using System;
using GQ.Client.UI.Dialogs;
using GQ.Client.Util;
using GQ.Client.Conf;
using UnityEngine.UI;
using GQ.Client.Err;

namespace GQ.Client.UI.Foyer
{

	/// <summary>
	/// Shows all Quest Info objects, e.g. in a scrollable list within the foyer. Drives a dialog while refreshing its content.
	/// </summary>
	public class QuestListController : MonoBehaviour
	{

		#region Fields

		public Transform InfoList;

		protected QuestInfoManager qim;

		protected Dictionary<int, QuestInfoController> questInfoControllers;

		#endregion


		#region Lifecycle API

		static bool _listAlreadyShownBefore = false;

		public static bool ListAlreadyShownBefore {
			get {
				return _listAlreadyShownBefore;
			}
			set {
				_listAlreadyShownBefore = value;
			}
		}

		// Use this for initialization
		void Start ()
		{
			qim = QuestInfoManager.Instance;

			qim.OnChange += OnQuestInfoChanged;

			if (questInfoControllers == null) {
				questInfoControllers = new Dictionary<int, QuestInfoController> ();
			}

			// TODO soll wirklich der ListController hier dem Manager sagen, dass er ein update braucht?
			// sollte doch eher passieren, wenn wieder online, oder user interaktion, oder letztes updates lange her...
			// aber vielleicht eben doch auch hier beim Start des Controllers, d.h. bei neuer Anzeige der Liste.
			if (!ListAlreadyShownBefore) {
				qim.UpdateQuestInfos ();
				ListAlreadyShownBefore = true;
			} else {
				UpdateView ();
			}
		}

		void OnDestroy ()
		{
			qim.OnChange -= OnQuestInfoChanged;
		}

		#endregion


		#region React on Events

		public void OnQuestInfoChanged (object sender, QuestInfoChangedEvent e)
		{
			QuestInfoController qiCtrl;
			switch (e.ChangeType) {
			case ChangeType.AddedInfo:
				qiCtrl = 
					QuestListElementController.Create (
					root: InfoList.gameObject,
					qInfo: e.NewQuestInfo
				).GetComponent<QuestListElementController> ();
				questInfoControllers.Add (e.NewQuestInfo.Id, qiCtrl);
				qiCtrl.Show ();
				sortView ();
				break;
			case ChangeType.ChangedInfo:
				if (!questInfoControllers.TryGetValue (e.OldQuestInfo.Id, out qiCtrl)) {
					Log.SignalErrorToDeveloper (
						"Quest Info Controller for quest id {0} not found when a Change event occurred.",
						e.OldQuestInfo.Id
					);
					break;
				}
				qiCtrl.UpdateView ();
				qiCtrl.Show ();
				sortView ();
				break;
			case ChangeType.RemovedInfo:
				if (!questInfoControllers.TryGetValue (e.OldQuestInfo.Id, out qiCtrl)) {
					Log.SignalErrorToDeveloper (
						"Quest Info Controller for quest id {0} not found when a Remove event occurred.",
						e.OldQuestInfo.Id
					);
					break;
				}
				qiCtrl.Hide ();
				questInfoControllers.Remove (e.OldQuestInfo.Id);
				break;							
			case ChangeType.ListChanged:
				UpdateView ();
				break;							
			}
		}

		/// <summary>
		/// Sorts the list. Takes the current sorter into account to move the gameobjects in the right order.
		/// </summary>
		private void sortView ()
		{
			List<QuestInfoController> qcList = new List<QuestInfoController> (questInfoControllers.Values);
			qcList.Sort ();
			for (int i = 0; i < qcList.Count; i++) {
				qcList [i].transform.SetSiblingIndex (i);
			}
		}

		public void UpdateView ()
		{
			if (this == null) {
				Debug.Log ("QuestListController is null".Red ());
				return;
			}
			if (InfoList == null) {
				Debug.Log ("QuestListController.InfoList is null".Red ());
				return;
			}
			// TODO: Create a common super class for QuestInfoControllers like QuestListInfoController(this one here) and QuestInfoMapController!

			// hide and delete all list elements:
			foreach (KeyValuePair<int, QuestInfoController> kvp in questInfoControllers) {
				kvp.Value.Hide ();
				kvp.Value.Destroy ();
			}
			foreach (QuestInfo info in QuestInfoManager.Instance.GetListOfQuestInfos()) {
				// create new list elements
				if (QuestInfoManager.Instance.Filter.accept (info)) {
					QuestListElementController qiCtrl = 
						QuestListElementController.Create (
							root: InfoList.gameObject,
							qInfo: info
						).GetComponent<QuestListElementController> ();
					questInfoControllers.Add (info.Id, qiCtrl);
					qiCtrl.Show ();
				}
			}
			sortView ();

		}

		#endregion

	}
}