﻿
using UnityEngine;

using System;

using UnitySlippyMap;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.Converters.WellKnownText;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class page_map : MonoBehaviour
{
	public Map		map;
	
	
	public questdatabase questdb;
	public Quest quest;
	public QuestPage mappage;
	public actions questactions;
	public GPSPosition gpsdata;


	public Texture	LocationTexture;
	public Texture	MarkerTexture;
	
	private float	guiXScale;
	private float	guiYScale;
	private Rect	guiRect;
	
	private bool 	isPerspectiveView = false;
	private float	perspectiveAngle = 30.0f;
	private float	destinationAngle = 0.0f;
	private float	currentAngle = 0.0f;
	private float	animationDuration = 0.5f;
	private float	animationStartTime = 0.0f;
	
	private List<Layer> layers;
	private int     currentLayerIndex = 0;
	
	public LocationMarker location;


	public Transform radiusprefab;
	private bool zoomin = false;
	private bool zoomout = false;

	public bool fixedonposition = true;
	private
		
		void
			
			Start()
	{




		questdb = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ();
		gpsdata = questdb.GetComponent<GPSPosition> ();
		quest = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ().currentquest;
		mappage = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ().currentquest.currentpage;
		questactions = GameObject.Find ("QuestDatabase").GetComponent<actions> ();

		
		string pre = "file: /";
		
		
		
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
			
			pre = "file:";
		}
		
		
		
		
		// setup the gui scale according to the screen resolution
		guiXScale = (Screen.orientation == ScreenOrientation.Landscape ? Screen.width : Screen.height) / 480.0f;
		guiYScale = (Screen.orientation == ScreenOrientation.Landscape ? Screen.height : Screen.width) / 640.0f;
		// setup the gui area
		guiRect = new Rect(16.0f * guiXScale, 4.0f * guiXScale, Screen.width / guiXScale - 32.0f * guiXScale, 32.0f * guiYScale);
		
		// create the map singleton
		map = Map.Instance;
		map.CurrentCamera = Camera.main;
		map.InputDelegate += UnitySlippyMap.Input.MapInput.BasicTouchAndKeyboard;
	




		if (questdb.getActiveHotspots ().Count > 0) {

						QuestRuntimeHotspot first = questdb.getActiveHotspots () [0];
			map.CurrentZoom = 18.0f;

			map.CenterWGS84 = new double[2] {(double)first.lat,(double)first.lon };


				} else {


			map.CurrentZoom = 18.0f;

						map.CenterWGS84 = new double[2] { 51	, 8 };
				}

		map.UseLocation = true;
		map.UpdateCenterWithLocation = true;
	
		map.UseOrientation = true;
		map.CameraFollowsOrientation = true;
		map.CenterOnLocation ();


		map.MaxZoom = 22.0f;
		map.MinZoom = 13.0f;



		map.InputsEnabled = true;
		map.ShowGUIControls = true;
		
		//map.GUIDelegate += Toolbar;
		
		layers = new List<Layer>();
		
		// create an OSM tile layer
		OSMTileLayer osmLayer = map.CreateLayer<OSMTileLayer>("OSM");
		osmLayer.BaseURL = "http://a.tile.openstreetmap.org/";
		
		layers.Add(osmLayer);
		






		foreach(QuestRuntimeHotspot qrh in questdb.hotspots){



		
			WWW www = null;
		
		
			
			if (qrh.hotspot.getAttribute ("img").StartsWith ("@_")) {
				
				
				www = new WWW (pre + "" + questactions.getVariable (qrh.hotspot.getAttribute ("img")).string_value [0]);
				
				
			} else {
				
				
				
				
				string url = qrh.hotspot.getAttribute ("img");
				if(!url.StartsWith("http:") && !url.StartsWith("https:")){
					url = pre + "" +  qrh.hotspot.getAttribute ("img");
				}
				
//				Debug.Log(url);
				
				
				if(url.StartsWith("http:") || url.StartsWith("https:")) {
					//Debug.Log("webimage");
					
					www = new WWW (url);
					StartCoroutine (createMarkerAfterImageLoaded(www,qrh));
					
					
				} else if(File.Exists (qrh.hotspot.getAttribute ("img"))){
					www = new WWW (url);
					StartCoroutine (createMarkerAfterImageLoaded(www,qrh));
				}
				
			}



		}




		double a = 0;
		double b = 0;
		if (gpsdata.CoordinatesWGS84.Length > 1) {


			a = gpsdata.CoordinatesWGS84[0];
			b = gpsdata.CoordinatesWGS84[1];



				} else {


						QuestRuntimeHotspot minhotspot = null;
						double[] minhotspotposition = null;

						foreach (QuestRuntimeHotspot qrh in questdb.getActiveHotspots()) {

								if (minhotspot == null) {

										minhotspot = qrh;
				
										double[] xy = new double[] {qrh.lon, qrh.lat };
										double lon = (0 / 20037508.34) * 180;                        
										double lat = (((-1) * ((double.Parse (qrh.hotspot.getAttribute ("radius")) * 2d) + 5d)) / 20037508.34) * 180;
										lat = 180 / Math.PI * (2 * Math.Atan (Math.Exp (lat * Math.PI / 180)) - Math.PI / 2);                        
										double[] xy1 = new double[] { xy [0] + lon, xy [1] + lat };
										minhotspotposition = xy1;

				
								} else {

										// Lon/Lat offset by (x// 20037508.34) * 180;
										// i only use lat right now.

										double[] xy = new double[] {qrh.lon, qrh.lat };
										double lon = (0 / 20037508.34) * 180;                        
										double lat = (((-1) * ((double.Parse (qrh.hotspot.getAttribute ("radius")) * 2d) + 5d)) / 20037508.34) * 180;

										lat = 180 / Math.PI * (2 * Math.Atan (Math.Exp (lat * Math.PI / 180)) - Math.PI / 2);                        
										double[] xy1 = new double[] { xy [0] + lon, xy [1] + lat };


				
				
										if (xy1 [1] < minhotspotposition [1]) {
												minhotspot = qrh;
												minhotspotposition = xy1;
										}


								}

						}


//						Debug.Log ("LONLAT:" + minhotspotposition [0] + "," + minhotspotposition [1]);
		
						 a = minhotspotposition [1];
						 b = minhotspotposition [0];


				}
		
		map.CenterWGS84 = new double[2] { a	, b };


		// create the location marker
		var posi = Tile.CreateTileTemplate ().gameObject;
		posi.renderer.material.mainTexture = LocationTexture;
		posi.renderer.material.renderQueue = 4000;
		posi.transform.localScale /= 8.0f;
		
		GameObject markerPosi = Instantiate(posi) as GameObject;
		location = map.SetLocationMarker<LocationMarker>(markerPosi,a,b);
		location.OrientationMarker = location.transform;
		location.GetComponentInChildren<MeshRenderer> ().material.color = Color.blue;


		location.gameObject.AddComponent ("locationcontrol");
		locationcontrol lc = location.GetComponent<locationcontrol> ();
		lc.mapcontroller = this;
		lc.map = map;
		lc.location = location;

		DestroyImmediate(posi);






		if(mappage.onStart != null){
			
			mappage.onStart.Invoke();
		}



	}

	private double deg2rad(double deg) {
		
		return (deg * Math.PI / 180.0);
		
	}
	private double rad2deg(double rad) {
		
		return (rad / Math.PI * 180.0);
		
	}
	
	
	public double distance(double lat1, double lon1, double lat2, double lon2, char unit) {
		
		double theta = lon1 - lon2;
		
		double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
		
		dist = Math.Acos(dist);
		
		dist = rad2deg(dist);
		
		dist = dist * 60 * 1.1515;
		
		if (unit == 'K') {
			
			dist = dist * 1.609344;
			
		} else if (unit == 'N') {
			
			dist = dist * 0.8684;
			
		} else if (unit == 'M') {
			
			dist = dist * 1609;
			
		}
		
		return (dist);
		
	}





	IEnumerator createMarkerAfterImageLoaded(WWW www,QuestRuntimeHotspot qrh){
		
		yield return www;
		
		if (www.error == null) {


			// Prefab
			GameObject go = Tile.CreateTileTemplate(Tile.AnchorPoint.BottomCenter).gameObject;
			
		
			go.renderer.material.mainTexture = www.texture;
			go.renderer.material.renderQueue = 4001;



			int height = go.renderer.material.mainTexture.height;
			int width = go.renderer.material.mainTexture.width;
			

			if(height > width){
				
				//Debug.Log(width+"/"+height+"="+width/height);
				go.transform.localScale = new Vector3(((float)width)/((float)height), 1.0f, 1.0f);
				
			} else {
				
				go.transform.localScale = new Vector3(1.0f, height/width, 1.0f);
				
			}

			go.transform.localScale /= 7.0f;

		
			go.AddComponent("onTapMarker");
			go.GetComponent<onTapMarker>().hotspot = qrh;



			go.AddComponent("circletests");
	
			if(qrh.hotspot.hasAttribute("radius")){
				go.GetComponent<circletests>().radius = int.Parse(qrh.hotspot.getAttribute("radius"));
			}
			go.GetComponent<BoxCollider>().center = new Vector3(0f,0f,0.5f);
			go.GetComponent<BoxCollider>().size = new Vector3(1f,0.1f,1f);

			go.AddComponent<CameraFacingBillboard>().Axis = Vector3.up;
			
			
			// Instantiate
			GameObject markerGO;
			markerGO = Instantiate(go) as GameObject;



			qrh.renderer = markerGO.GetComponent<MeshRenderer>();






			// CreateMarker(Name,longlat,prefab)
			Marker m = map.CreateMarker<Marker>(qrh.hotspot.getAttribute("name"), new double[2] {qrh.lat,qrh.lon }, markerGO);
		




			// Destroy Prefab
			DestroyImmediate(go);


			if(!qrh.visible){
				
				qrh.renderer.enabled = false;

				
			}



		} else {
			Debug.Log(www.error);
			
		}
		
	}






	public void initiateZoomIn(){

		zoomin = true;

	}
	public void initiateZoomOut(){
		
		zoomout = true;
		
	}
	public void endZoomIn(){
		
		zoomin = false;
		
	}
	public void endZoomOut(){
		
		zoomout = false;
		
	}


	public void setFixedPosition(bool b){

		fixedonposition = b;

		}

	void OnApplicationQuit()
	{
		map = null;
	}
	
	void Update()
	{


if (fixedonposition) {

			map.CenterWGS84 = gpsdata.CoordinatesWGS84;



				}



		if (zoomin) {

//			Debug.Log(map.CurrentZoom);
			map.Zoom(1.0f);

			
		} else if (zoomout) {

			map.Zoom(-1.0f);

			
		}


		if (destinationAngle != 0.0f)
		{
			Vector3 cameraLeft = Quaternion.AngleAxis(-90.0f, Camera.main.transform.up) * Camera.main.transform.forward;
			if ((Time.time - animationStartTime) < animationDuration)
			{
				float angle = Mathf.LerpAngle(0.0f, destinationAngle, (Time.time - animationStartTime) / animationDuration);
				Camera.main.transform.RotateAround(Vector3.zero, cameraLeft, angle - currentAngle);
				currentAngle = angle;
			}
			else
			{
				Camera.main.transform.RotateAround(Vector3.zero, cameraLeft, destinationAngle - currentAngle);
				destinationAngle = 0.0f;
				currentAngle = 0.0f;
				map.IsDirty = true;
			}
			
			map.HasMoved = true;
		}
	}
	
	#if DEBUG_PROFILE
	void LateUpdate()
	{
		Debug.Log("PROFILE:\n" + UnitySlippyMap.Profiler.Dump());
		UnitySlippyMap.Profiler.Reset();
	}
	#endif
}

