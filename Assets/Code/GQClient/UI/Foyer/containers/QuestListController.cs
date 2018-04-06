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
using QM.Util;

namespace GQ.Client.UI.Foyer
{

	/// <summary>
	/// Shows all Quest Info objects, e.g. in a scrollable list within the foyer. Drives a dialog while refreshing its content.
	/// </summary>
	public class QuestListController : QuestContainerController
	{
		public Transform InfoList;


		#region React on Events

		public override void OnQuestInfoChanged (object sender, QuestInfoChangedEvent e)
		{
			QuestInfoUIC qiCtrl;
			switch (e.ChangeType) {
			case ChangeType.AddedInfo:
				qiCtrl = 
					QuestInfoUICListElement.Create (
					root: InfoList.gameObject,
					qInfo: e.NewQuestInfo,
					containerController: this
				).GetComponent<QuestInfoUICListElement> ();
				QuestInfoControllers.Add (e.NewQuestInfo.Id, qiCtrl);
				qiCtrl.Show ();
				sortView ();
				break;
			case ChangeType.ChangedInfo:
//				if (e.OldQuestInfo == null || !QuestInfoControllers.TryGetValue (e.OldQuestInfo.Id, out qiCtrl)) {
//					Log.SignalErrorToDeveloper (
//						"Quest Info Controller for quest id {0} not found when a Change event occurred.",
//						e.OldQuestInfo.Id
//					);
//					break;
//				}
				if (e.NewQuestInfo == null || !QuestInfoControllers.TryGetValue (e.NewQuestInfo.Id, out qiCtrl)) {
					Log.SignalErrorToDeveloper (
						"Quest Info Controller for quest id {0} not found when a Change event occurred.",
						e.NewQuestInfo.Id
					);
					break;
				}
//				if (e.OldQuestInfo.Id != e.NewQuestInfo.Id) {
//					Log.SignalErrorToDeveloper (
//						"Quest Info Controller for quest id {0} got an update that changed the id to {1} which is not allowed and will be ignored.",
//						e.NewQuestInfo.Id, e.NewQuestInfo.Id
//					);
//					break;
//				}
				qiCtrl.PerformUpdate (e.NewQuestInfo);
				qiCtrl.Show ();
				sortView ();
				break;
			case ChangeType.RemovedInfo:
				if (!QuestInfoControllers.TryGetValue (e.OldQuestInfo.Id, out qiCtrl)) {
					Log.SignalErrorToDeveloper (
						"Quest Info Controller for quest id {0} not found when a Remove event occurred.",
						e.OldQuestInfo.Id
					);
					break;
				}
				qiCtrl.Hide ();
				QuestInfoControllers.Remove (e.OldQuestInfo.Id);
				break;							
			case ChangeType.ListChanged:
				UpdateView ();
				break;							
			case ChangeType.FilterChanged:
				UpdateViewAfterFilterChanged ();
				break;							
			}
		}

		/// <summary>
		/// Sorts the list. Takes the current sorter into account to move the gameobjects in the right order.
		/// </summary>
		private void sortView ()
		{
			List<QuestInfoUIC> qcList = new List<QuestInfoUIC> (QuestInfoControllers.Values);
			qcList.Sort ();
			for (int i = 0; i < qcList.Count; i++) {
				qcList [i].transform.SetSiblingIndex (i);
			}
		}

		public override void UpdateView ()
		{
			if (this == null) {
				return;
			}
			if (InfoList == null) {
				return;
			}

			// hide and delete all list elements:
			foreach (KeyValuePair<int, QuestInfoUIC> kvp in QuestInfoControllers) {
				kvp.Value.Hide ();
				kvp.Value.Destroy ();
			}

			QuestInfoControllers.Clear();

			foreach (QuestInfo info in QuestInfoManager.Instance.GetFilteredQuestInfos()) {
				// create new list elements
				QuestInfoUICListElement qiCtrl = 
					QuestInfoUICListElement.Create (
						root: InfoList.gameObject,
						qInfo: info,
						containerController: this
					).GetComponent<QuestInfoUICListElement> ();
				QuestInfoControllers[info.Id] = qiCtrl;
				qiCtrl.Show ();
			}

			sortView ();
		}

		public void UpdateViewAfterFilterChanged ()
		{
			if (this == null) {
				return;
			}
			if (InfoList == null) {
				return;
			}

			// we make a separate list of ids of all old quest infos:
			List<int> rememberedOldIDs = new List<int>(QuestInfoControllers.Keys);

			// we create new qi elements and keep those we can reuse. We remove those from our helper list.
			foreach (QuestInfo info in QuestInfoManager.Instance.GetFilteredQuestInfos()) {
				QuestInfoUIC qiCtrl;
				if (QuestInfoControllers.TryGetValue(info.Id, out qiCtrl)) {
					qiCtrl.Show (); // why do we need to show them here again? Aren't they still shown? Why?
					// this new element was already there, hence we keep it:
					rememberedOldIDs.Remove(info.Id);
				}
				else {
					QuestInfoControllers [info.Id].Show ();
				}
			}

			// now in the helper list only the old unused elements are left. Hence we delete them:
			foreach (int oldID in rememberedOldIDs) {
				QuestInfoControllers [oldID].Hide ();
			}
		}

		#endregion

	}
}