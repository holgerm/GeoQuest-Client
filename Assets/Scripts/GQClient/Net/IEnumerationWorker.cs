using UnityEngine;
using UnityEngine.Networking;

using System.Collections;
using System.Collections.Generic;
using System.Text;
using LitJson;
using System.IO;
using System;
using GQ.Client.Net;
using GQ.Util;

namespace GQ.Util {
	
	public interface IEnumerationWorker {

		void enumerate (IEnumerator enumerator);

	}


}