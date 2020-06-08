﻿using System;
using Code.GQClient.Conf;
using Code.GQClient.Err;
using GQClient.Model;
using Code.QM.Util;
using Code.UnitySlippyMap.Map;
using UnityEngine;

namespace Code.GQClient.UI.map
{

	/// <summary>
	/// Shows all Quest Info objects, on a map within the foyer. Refreshing its content silently (no dialogs shown etc.).
	/// </summary>
	public class FoyerMapController : MapController
	{
		#region Initialize

		private QuestInfoManager _qim;

		protected override void Start ()
		{
			// set up the inherited Map features:
			base.Start ();

			// at last we register for changes on quest infos with the quest info manager:
			_qim = QuestInfoManager.Instance;
			_qim.OnDataChange += OnMarkerChanged;
			_qim.OnFilterChange += OnMarkerChanged;
		}

		#endregion

		#region React on Events

		private void OnMarkerChanged (object sender, QuestInfoChangedEvent e)
		{
			Marker m;
			switch (e.ChangeType) {
			case ChangeType.AddedInfo:
				UpdateView ();
				break;
			case ChangeType.ChangedInfo:
				if (!Markers.TryGetValue (e.OldQuestInfo.Id, out m)) {
					Log.SignalErrorToDeveloper (
						"Quest Info Controller for quest id {0} not found when a Change event occurred.",
						e.OldQuestInfo.Id
					);
					break;
				}
				m.UpdateMarker();
				m.Show ();
				break;
			case ChangeType.RemovedInfo:
				if (!Markers.TryGetValue (e.OldQuestInfo.Id, out m)) {
					Log.SignalErrorToDeveloper (
						"Quest Info Controller for quest id {0} not found when a Remove event occurred.",
						e.OldQuestInfo.Id
					);
					break;
				}
				m.Hide ();
				e.OldQuestInfo.OnChanged -= m.UpdateView;
				Markers.Remove (e.OldQuestInfo.Id);
				break;							
			case ChangeType.ListChanged:
				UpdateView ();
				break;							
			case ChangeType.FilterChanged:
				UpdateView ();
				break;
			case ChangeType.SorterChanged:
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Map & Markers

		protected override void populateMarkers ()
		{
			foreach (var info in QuestInfoManager.Instance.GetFilteredQuestInfos()) {
				// create new list elements
				CreateMarker (info);
			}
		}

		private void CreateMarker (QuestInfo info)
		{
			if (info.MarkerHotspot.Equals (HotspotInfo.NULL)) {
				return;
			}

			var markerGo = TileBehaviour.CreateTileTemplate (TileBehaviour.AnchorPoint.BottomCenter).gameObject;

			var newMarker = map.CreateMarker<QuestMarker> (
				                        info.Name, 
				                        new[]
				                        {
					                        info.MarkerHotspot.Longitude, 
					                        info.MarkerHotspot.Latitude
				                        }, 
				                        markerGo
			                        );
			if (newMarker == null)
				return;
			
			newMarker.Data = info;
			markerGo.name = "Marker tile (" + info.Name + ")";

			calculateMarkerDetails (newMarker.Texture, markerGo);

			Markers.Add (info.Id, newMarker);
			info.OnChanged += newMarker.UpdateView;
		}

		public Texture markerSymbolTexture;

		protected override void locateAtStart ()
		{
			switch (ConfigurationManager.Current.mapStartPositionType) {
			case MapStartPositionType.CenterOfMarkers:
				// calculate center of markers / quests:
				double sumLong = 0f;
				double sumLat = 0f;
				var counter = 0;
				foreach (var qi in QuestInfoManager.Instance.GetListOfQuestInfos()) {
					var hi = qi.MarkerHotspot;
					if (hi == HotspotInfo.NULL)
						continue;

					sumLong += hi.Longitude;
					sumLat += hi.Latitude;
					counter++;
				}
				if (counter == 0) {
					LocateAtFixedConfiguredPosition ();
				}
				else {
					map.CenterWGS84 = new[] {
						sumLong / counter,
						sumLat / counter
					};
				}
				break;
			case MapStartPositionType.FixedPosition:
				LocateAtFixedConfiguredPosition();
				break;
			case MapStartPositionType.PlayerPosition:
				if (Device.location.isEnabledByUser &&
					Device.location.status != LocationServiceStatus.Running) {
					map.CenterOnLocation ();
				} else {
					LocateAtFixedConfiguredPosition();
				}
				break;
			}
		}

		private void LocateAtFixedConfiguredPosition() {
			map.CenterWGS84 = new[] {
				ConfigurationManager.Current.mapStartAtLongitude,
				ConfigurationManager.Current.mapStartAtLatitude
			};
		}



		#endregion
	}
}