﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Reflection;

namespace QM.Util
{
	
	public class WATCH
	{

		static Dictionary<string, WATCH> watches = new Dictionary<string, WATCH> ();

		private Stopwatch stopwatch;
		private string name;
		private long lastTimeStamp;

		public WATCH () : this (new StackFrame (1).GetMethod ().DeclaringType.Name + "." + new StackFrame (1).GetMethod ().Name)
		{
		}

		public WATCH (string name)
		{
			stopwatch = new Stopwatch ();
			this.name = name;
			nameOfLastStarted = name;
			this.lastTimeStamp = 0L;
			watches [name] = this;
		}

		public static WATCH Get (string name)
		{
			WATCH watch;
			if (!watches.TryGetValue (name, out watch)) {
				return null;
			}
			return watch;
		}

		public void Start ()
		{
			lastTimeStamp = 0L;
			nameOfLastStarted = 
				this.name == null ? 
				new StackFrame (1).GetType ().Name + "." + new StackFrame (1).GetMethod ().Name : 
				this.name;
			stopwatch.Start ();
		}

		public void StopAndShow ()
		{
			stopwatch.Stop ();
			UnityEngine.Debug.Log (
				string.Format ("WATCH {0} stopped after {1} ms ({2} delta)", 
					name, 
					stopwatch.ElapsedMilliseconds, 
					stopwatch.ElapsedMilliseconds - lastTimeStamp
				)
			);
		}

		public static void Lap (string name)
		{
			WATCH w = Get (name);
			if (w == null) {
				UnityEngine.Debug.Log (string.Format ("WATCH {0} not available.", name));
				return;
			}
			w.Lap ();
		}

		public void Lap ()
		{
			lastTimeStamp = 0L;
		}

		public static void Show (string name, string pointName)
		{
			WATCH w = Get (name);
			if (w == null) {
				UnityEngine.Debug.Log (string.Format ("WATCH {0} not available at {1}.", name, pointName));
				return;
			}
			w.Show (pointName);
		}

		public void Show (string pointName)
		{
			stopwatch.Stop ();
			UnityEngine.Debug.Log (
				string.Format ("WATCH {0} at {1} took {2} ms ({3} delta)", 
					name, 
					pointName, 
					stopwatch.ElapsedMilliseconds, 
					stopwatch.ElapsedMilliseconds - lastTimeStamp
				)
			);
			lastTimeStamp = stopwatch.ElapsedMilliseconds;
			stopwatch.Start ();
		}

		public static long Milliseconds (string name)
		{
			WATCH w = Get (name);
			if (w == null)
				return 0L;
			return w.Milliseconds ();
		}

		public long Milliseconds ()
		{
			return stopwatch.ElapsedMilliseconds;
		}

		#region Static Quick Access Methods

		private static string nameOfLastStarted;
		private static int nrOfShows = 0;

		public static void _Start ()
		{
			_Start (new StackFrame (1).GetMethod ().DeclaringType.Name + "." + new StackFrame (1).GetMethod ().Name);
		}

		public static void _Start (string name)
		{
			nameOfLastStarted = name;
			WATCH w = new WATCH (name);
			w.Start ();
		}

		public static void _Show ()
		{
			WATCH w = Get (nameOfLastStarted);
			if (w == null)
				return;

			w.Show ("" + nrOfShows++);
		}

		public static void _StopAndShow ()
		{
			_StopAndShow (nameOfLastStarted);
		}

		public static void _StopAndShow (string name)
		{
			WATCH w = Get (name);
			if (w == null) {
				UnityEngine.Debug.Log (string.Format ("WATCH {0} not available.", name));
				return;
			}
			w.StopAndShow ();
		}

		#endregion

	}
}
