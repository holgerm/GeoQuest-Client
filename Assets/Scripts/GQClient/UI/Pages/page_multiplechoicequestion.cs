﻿using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using GQ.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class page_multiplechoicequestion : MonoBehaviour {




	
	public Quest quest;
	public QuestPage multiplechoicequestion;
	private WWW www;

	public Text questiontext;
	public VerticalLayoutGroup list;
	public multiplechoiceanswerbutton answerbuttonprefab;

	public questdatabase questdb;
	public actions questactions;
	public Image image;

	// Use this for initialization
	void Start () {


		if ( GameObject.Find("QuestDatabase") == null ) {
			
			SceneManager.LoadScene("questlist");
		}
		else {


			questdb = GameObject.Find("QuestDatabase").GetComponent<questdatabase>();
			quest = GameObject.Find("QuestDatabase").GetComponent<questdatabase>().currentquest;
			multiplechoicequestion = GameObject.Find("QuestDatabase").GetComponent<questdatabase>().currentquest.currentpage;
			questactions = GameObject.Find("QuestDatabase").GetComponent<actions>();


			if ( multiplechoicequestion.onStart != null ) {
			
				multiplechoicequestion.onStart.Invoke();
			}




			questiontext.text = questdb.GetComponent<actions>().formatString(multiplechoicequestion.getAttribute("question"));


			List<QuestContent> answers = multiplechoicequestion.contents_answers;

			if ( multiplechoicequestion.hasAttribute("shuffle") && multiplechoicequestion.getAttribute("shuffle") == "true" ) {


				int n = answers.Count;
				System.Random rnd = new System.Random();
				while ( n > 1 ) {
					int k = (rnd.Next(0, n) % n);
					n--;
					QuestContent value = answers[k];
					answers[k] = answers[n];
					answers[n] = value;
				}
				
				
				
			}
			
			
			
			foreach ( QuestContent qc in multiplechoicequestion.contents_answers ) {


				multiplechoiceanswerbutton btn = (multiplechoiceanswerbutton)Instantiate(answerbuttonprefab, transform.position, Quaternion.identity);
				btn.transform.SetParent(list.transform);
				btn.transform.localScale = new Vector3(1f, 1f, 1f);
				btn.setText(qc.content);

				if ( qc.getAttribute("correct") == "1" ) {
					btn.correct = true; 
				}
				else {
					btn.correct = false;

				}
			}




			







			if ( multiplechoicequestion.getAttribute("bg") != "" ) {





				if (
					multiplechoicequestion.getAttribute("bg").StartsWith("http://") ||
					multiplechoicequestion.getAttribute("bg").StartsWith("https://") ) {
					
					www = new WWW(multiplechoicequestion.getAttribute("bg"));
					
					StartCoroutine(waitforImage());
					
					
				}
				else
				if ( multiplechoicequestion.getAttribute("bg").StartsWith("@_") ) {
				
				
				
				
					foreach ( QuestRuntimeAsset qra in questactions.photos ) {
					
						//Debug.Log("KEY:"+qra.key);
					
						if ( qra.key == multiplechoicequestion.getAttribute("bg") ) {
						
						
						
						
							Sprite s = Sprite.Create(qra.texture, new Rect(0, 0, qra.texture.width, qra.texture.height), new Vector2(0.5f, 0.5f));
						

							
							image.sprite = s;
							image.enabled = true;
							
							

						
						}
					}
					//Debug.Log ("donewithforeach");
				
				
				
				
				}
				else {
				
				
				
				
					foreach ( SpriteConverter sc in questdb.convertedSprites ) {
					
					
					
						if ( sc.filename == multiplechoicequestion.getAttribute("bg") ) {
						
							if ( sc.isDone ) {
								if ( sc.sprite != null ) {
						
									image.sprite = sc.sprite;
									image.enabled = true;
									
									

								
								
								
								
								
								}
								else {
								
									Debug.Log("Sprite was null");
								}
							}
							else {
							
								Debug.Log("SpriteConverter was not done.");
							
							}
						}
					}
				
				}
			
			}


		}
	}

	public void onEnd () {


		bool doit = true;



		if ( multiplechoicequestion.hasAttribute("loopUntilSuccess")
		     && multiplechoicequestion.getAttribute("loopUntilSuccess") == "true"
		     && multiplechoicequestion.state.Equals("failed") ) {


			doit = false;

		}



		if ( doit ) {



			if ( multiplechoicequestion.state != "failed" ) {
				multiplechoicequestion.state = "succeeded";
			
			}
		
			if ( multiplechoicequestion.onEnd != null ) {
			
				multiplechoicequestion.onEnd.Invoke();
			}
			else
			if ( (multiplechoicequestion.onSuccess == null && multiplechoicequestion.onFailure == null)
			            ||
			            (multiplechoicequestion.onSuccess != null && !multiplechoicequestion.onSuccess.hasMissionAction() && multiplechoicequestion.onFailure != null && !multiplechoicequestion.onFailure.hasMissionAction()) ) {
			
				GameObject.Find("QuestDatabase").GetComponent<questdatabase>().endQuest();
			
			}

		}
		
		
	}




	IEnumerator waitforImage () {
		
		DateTime startWWW = DateTime.Now;
		
		yield return www;
		
		if ( www.error == null ) {
			
			DateTime start = DateTime.Now;
			
			Sprite s = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));

				
			image.sprite = s;
			image.enabled = true;
				
				
			
			
			
		}
		else {
			
			
			
			Debug.Log(www.error);
			
			image.enabled = false;
		}
		
		
		yield return 0;
		
	}



	public void onSuccess () {
		
		multiplechoicequestion.state = "succeeded";

		
		if ( multiplechoicequestion.onSuccess != null ) {
			
			multiplechoicequestion.onSuccess.Invoke();
		} 
		
		
	}

	public void onFailure () {
		
		multiplechoicequestion.state = "failed";

		
		if ( multiplechoicequestion.onFailure != null ) {
			
			multiplechoicequestion.onFailure.Invoke();
		} 
		
		
	}
}
