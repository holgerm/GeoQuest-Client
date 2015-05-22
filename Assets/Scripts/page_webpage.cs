﻿using UnityEngine;
using System.Collections;

public class page_webpage : MonoBehaviour {
	
	public questdatabase questdb;
	public actions actioncontroller;
	public Quest quest;
	public QuestPage webpage;



	public GameObject nextButtonObject;
	//Just let it compile on platforms beside of iOS and Android
	//If you are just targeting for iOS and Android, you can ignore this
	#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
	
	//1. First of all, we need a reference to hold an instance of UniWebView
	private UniWebView _webView;
	
	private string _errorMessage;
	private GameObject _cube;
	private Vector3 _moveVector;
	
#endif

	public void backButton ()
	{
		
		
		
		QuestPage show = questdb.currentquest.previouspages [questdb.currentquest.previouspages.Count - 1];
		questdb.currentquest.previouspages.Remove (questdb.currentquest.previouspages [questdb.currentquest.previouspages.Count - 1]);
		questdb.changePage (show.id);
		
		
		
	}
	public void nextButton ()
	{
		
		
		onEnd ();
		
		
		
	}
	// Use this for initialization
	void Start () {


		if (GameObject.Find ("QuestDatabase") != null) {
			questdb = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ();
			actioncontroller = GameObject.Find ("QuestDatabase").GetComponent<actions> ();
			quest = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ().currentquest;
			webpage = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ().currentquest.currentpage;
		} else {
			Application.LoadLevel(0);

		}


		if (questdb.currentquest.previouspages.Count == 0) {

			Destroy(nextButtonObject);
		}


		if(webpage.onStart != null){
			
			webpage.onStart.Invoke();
		}
		
		
		#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8

	
		_webView = GetComponent<UniWebView>();
		if (_webView == null) {
			_webView = gameObject.AddComponent<UniWebView>();
			_webView.OnReceivedMessage += OnReceivedMessage;
			_webView.OnLoadComplete += OnLoadComplete;
			_webView.OnWebViewShouldClose += OnWebViewShouldClose;
			_webView.OnEvalJavaScriptFinished += OnEvalJavaScriptFinished;
			_webView.InsetsForScreenOreitation += InsetsForScreenOreitation;

		}
		



		if(webpage.getAttribute ("url") != null && webpage.getAttribute("url") != ""){
			_webView.url = webpage.getAttribute ("url");
		_webView.Load();
		
		_errorMessage = null;
		} else {

			onEnd();

		}
#else


		onEnd();


#endif
		
	}

	
	public void onEnd(){
		
			webpage.state = "succeeded";
		
		
		if (webpage.onEnd != null) {
			
			webpage.onEnd.Invoke ();
		} else {
			
			GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ().endQuest();
			
		}
		
		
	}
	
	
	#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8

	
	
	//5. When the webView complete loading the url sucessfully, you can show it.
	//   You can also set the autoShowWhenLoadComplete of UniWebView to show it automatically when it loads finished.
	void OnLoadComplete(UniWebView webView, bool success, string errorMessage) {
		if (success) {
			webView.Show();
		} else {
			Debug.Log("Something wrong in webview loading: " + errorMessage);
			_errorMessage = errorMessage;
		}
	}
	
	//6. The webview can talk to Unity by a url with scheme of "uniwebview". See the webpage for more
	//   Every time a url with this scheme clicked, OnReceivedMessage of webview event get raised.
	void OnReceivedMessage(UniWebView webView, UniWebViewMessage message) {
		Debug.Log("Received a message from native");
		Debug.Log(message.rawMessage);
		//7. You can get the information out from the url path and query in the UniWebViewMessage
		//For example, a url of "uniwebview://move?direction=up&distance=1" in the web page will 
		//be parsed to a UniWebViewMessage object with:
		//				message.scheme => "uniwebview"
		//              message.path => "move"
		//              message.args["direction"] => "up"
		//              message.args["distance"] => "1"
		// "uniwebview" scheme is sending message to Unity by default.
		// If you want to use your customized url schemes and make them sending message to UniWebView,
		// use webView.AddUrlScheme("your_scheme") and webView.RemoveUrlScheme("your_scheme")
//		if (string.Equals(message.path, "close")) {
//			//8. When you done your work with the webview, 
//			//you can hide it, destory it and do some clean work.
//			webView.Hide();
//			Destroy(webView);
//			webView.OnReceivedMessage -= OnReceivedMessage;
//			webView.OnLoadComplete -= OnLoadComplete;
//			webView.OnWebViewShouldClose -= OnWebViewShouldClose;
//			webView.OnEvalJavaScriptFinished -= OnEvalJavaScriptFinished;
//			webView.InsetsForScreenOreitation -= InsetsForScreenOreitation;
//			_webView = null;
//		}
	}
	
	//9. By using EvaluatingJavaScript method, you can talk to webview from Unity.
	//It can evel a javascript or run a js method in the web page.
	//(In the demo, it will be called when the cube hits the sphere)
	public void ShowAlertInWebview(float time, bool first) {
		_moveVector = Vector3.zero;
		if (first) {
			//Eval the js and wait for the OnEvalJavaScriptFinished event to be raised.
			//The sample(float time) is written in the js in webpage, in which we pop 
			//up an alert and return a demo string.
			//When the js excute finished, OnEvalJavaScriptFinished will be raised.
			_webView.EvaluatingJavaScript("sample(" + time +")");
		}
	}
	
	//In this demo, we set the text to the return value from js.
	void OnEvalJavaScriptFinished(UniWebView webView, string result) {
		Debug.Log("js result: " + result);
	}
	
	//10. If the user close the webview by tap back button (Android) or toolbar Done button (iOS), 
	//    we should set your reference to null to release it. 
	//    Then we can return true here to tell the webview to dismiss.
	bool OnWebViewShouldClose(UniWebView webView) {
		if (webView == _webView) {
			_webView = null;
			return true;
		}
		return false;
	}
	
	// This method will be called when the screen orientation changed. Here we returned UniWebViewEdgeInsets(5,5,bottomInset,5)
	// for both situation. Although they seem to be the same, screenHeight was changed, leading a difference between the result.
	// eg. on iPhone 5, bottomInset is 284 (568 * 0.5) in portrait mode while it is 160 (320 * 0.5) in landscape.
	UniWebViewEdgeInsets InsetsForScreenOreitation(UniWebView webView, UniWebViewOrientation orientation) {
		int bottomInset = (int)(UniWebViewHelper.screenHeight * 0.18f);

		if (orientation == UniWebViewOrientation.Portrait) {
			return new UniWebViewEdgeInsets(0,0,bottomInset,0);
		} else {
			return new UniWebViewEdgeInsets(0,0,bottomInset,0);
		}
	}



	#endif
	

}
