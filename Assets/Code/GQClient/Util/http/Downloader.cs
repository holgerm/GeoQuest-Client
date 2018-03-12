using System;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using GQ.Client.Err;
using GQ.Client.UI;
using System.IO;

namespace GQ.Client.Util
{
	public class Downloader : AbstractDownloader
	{
		public string Url { get; set; } 

		public string TargetPath { get; set; }

		WWW _www;


		#region Default Handler

		public static void defaultLogInformationHandler (AbstractDownloader d, DownloadEvent e)
		{
			Log.InformUser (e.Message);
		}

		public static void defaultLogErrorHandler (AbstractDownloader d, DownloadEvent e)
		{
			Log.SignalErrorToUser (e.Message);
		}

		#endregion


		#region Delegation API for Tests

		static Downloader() {
			CoroutineRunner = DownloadAsCoroutine;
		}

		public delegate IEnumerator DownloaderCoroutineMethod(Downloader d);

		public static DownloaderCoroutineMethod CoroutineRunner {
			get;
			set;
		}

		public override IEnumerator RunAsCoroutine ()
		{
			return CoroutineRunner (this);
		}

		static protected IEnumerator DownloadAsCoroutine (Downloader d)
		{
			return d.Download ();
		}

		#endregion


		#region Public API

		/// <summary>
		/// The elapsed time the download is/was active in milliseconds.
		/// </summary>
		/// <value>The elapsed time.</value>
		public long elapsedTime {
			get {
				return stopwatch.ElapsedMilliseconds;
			}
		}

		/// <summary>
		/// Initializes a new Downloader object. 
		/// You can start the download as Coroutine: StartCoroutine(download.startDownload).
		/// All callbacks are intialized with defaults. You can customize the behaviour via method delegates 
		/// onStart, onError, onTimeout, onSuccess, onProgress.
		/// </summary>
		/// <param name="url">URL.</param>
		/// <param name="timeout">Timout in milliseconds (optional).</param>
		/// <param name="timeout">Target path where the downloaded file will be stored (optional).</param>
		public Downloader (
			string url, 
			long timeout = 0,
			string targetPath = null) : base (true)
		{
			Result = "";
			this.Url = url;
			Timeout = timeout;
			TargetPath = targetPath;
			stopwatch = new Stopwatch ();
			OnStart += defaultLogInformationHandler;
			OnError += defaultLogErrorHandler;
			OnTimeout += defaultLogErrorHandler;
			OnSuccess += defaultLogInformationHandler;
			OnProgress += defaultLogInformationHandler;
		}

		protected IEnumerator Download ()
		{
			UnityEngine.Debug.Log ("Downloader #1 from url: " + Url);

			Www = new WWW (Url);
			stopwatch.Start ();

			string msg = String.Format ("Start to download url {0}", Url);
			if (Timeout > 0) {
				msg += String.Format (", timout set to {0} ms.", Timeout);
			}
			Raise (DownloadEventType.Start, new DownloadEvent (message: msg));

			float progress = 0f;
			while (!Www.isDone) {
				if (progress < Www.progress) {
					progress = Www.progress;
					msg = string.Format ("Lade Datei {0}, aktuell: {1:N2}%", Url, progress * 100);
					Raise (DownloadEventType.Progress, new DownloadEvent (progress: progress, message: msg));
				}
				if (Timeout > 0 && stopwatch.ElapsedMilliseconds >= Timeout) {
					stopwatch.Stop ();
					Www.Dispose ();
					msg = string.Format ("Timeout: schon {0} ms vergangen", 
						stopwatch.ElapsedMilliseconds);
					Raise (DownloadEventType.Timeout, new DownloadEvent (elapsedTime: Timeout, message: msg));
					yield break;
				}
				if (Www == null)
					UnityEngine.Debug.Log ("Www is null"); // TODO what to do in this case?
				yield return null;
			} 

			stopwatch.Stop ();

			if (Www.error != null && Www.error != "") {
				Raise (DownloadEventType.Error, new DownloadEvent (message: Www.error));
				UnityEngine.Debug.Log ("Downloader error: " + Www.error);
				RaiseTaskFailed ();
			} else {
				Result = Www.text;
			
				UnityEngine.Debug.Log ("Downloader done text length: " + Www.text.Length);

				msg = string.Format ("Lade Datei {0}, aktuell: {1:N2}%", Url, progress * 100);
				Raise (DownloadEventType.Progress, new DownloadEvent (progress: Www.progress, message: msg));

				yield return null;

				msg = string.Format ("Speichere Datei ...");
				Raise (DownloadEventType.Progress, new DownloadEvent (progress: Www.progress, message: msg));

				if (TargetPath != null) {
					// we have to store the loaded file:
					try {
						string targetDir = Directory.GetParent (TargetPath).FullName;
						if (!Directory.Exists (targetDir))
							Directory.CreateDirectory (targetDir);
						if (File.Exists (TargetPath))
							File.Delete (TargetPath);

						File.WriteAllBytes (TargetPath, Www.bytes);
					} catch (Exception e) {
						Raise (DownloadEventType.Error, new DownloadEvent (message: "Could not save downloaded file: " + e.Message));
						RaiseTaskFailed ();

						Www.Dispose ();
						yield break;
					}
				}

				msg = string.Format ("Download für Datei {0} abgeschlossen", 
					Url);
				Raise (DownloadEventType.Success, new DownloadEvent (message: msg));
				RaiseTaskCompleted (Result);
			}

			Www.Dispose ();
			yield break;
		}

		#endregion

	}

}

