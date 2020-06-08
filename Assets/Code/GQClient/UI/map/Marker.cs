﻿using System.Collections;
using Code.GQClient.Util;
using Code.UnitySlippyMap.Markers;
using GQClient.Model;
using UnityEngine;

namespace Code.GQClient.UI.map
{

	/// <summary>
	/// Abstract super calss for all kinds of map markers, e.g. quest info markers, hotspot markers.
	/// </summary>
	public abstract class Marker : MarkerBehaviour {
		
		protected const string MARKER_ALPHA_BG_PATH = "defaults/readable/defaultMarkerBG";
		public const string DEFAULT_MARKER_PATH = "defaults/readable/defaultMarker";

		public virtual void UpdateView(QuestInfo questInfo)
		{
			CoordinatesWGS84 = new double[]
			{
				questInfo.Hotspots[0].Longitude,
				questInfo.Hotspots[0].Latitude
			};
			Update();
		}

		/// <summary>
		/// Shows the prefab for at least one frame duration.
		/// </summary>
		public void Show ()
		{
			Base.Instance.StartCoroutine (showAsCoroutine (true));
		}

		/// <summary>
		/// Hides the prefab for at least one frame duration.
		/// </summary>
		public void Hide ()
		{
			Base.Instance.StartCoroutine (showAsCoroutine (false));
		}

		private IEnumerator showAsCoroutine (bool show)
		{
			yield return new WaitForEndOfFrame ();
			if (this != null && gameObject != null)
				gameObject.SetActive (show);
		}

		public abstract Texture Texture { get; }

		/// <summary>
		/// Deletes the prefab from the hierarchy.
		/// </summary>
		public virtual void Destroy ()
		{
			Base.Instance.StartCoroutine (destroyAsCoroutine ());
		}

		private IEnumerator destroyAsCoroutine ()
		{
			yield return new WaitForEndOfFrame ();
			yield return new WaitForEndOfFrame ();

			if (this != null && gameObject != null) {
				gameObject.SetActive (false);
				Destroy (gameObject);
			}
		}

		#region Interaction
		public abstract void OnTouch ();
		#endregion


	}


}
