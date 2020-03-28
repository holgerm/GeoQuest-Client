﻿using System;
using System.IO;
using System.Linq;
using Code.GQClient.Err;
using UnityEngine;

namespace GQClient.Model
{

	/// <summary>
	/// Imports quest infos from JSON files. Either form the servers listing of all quest infos that are available, 
	/// or form the local json file which keeps track of the latest state of local and remote quest infos.
	/// 
	/// In order to import the server info, you need to use a downloader task before and 
	/// simply call the constructor of this class with 'true'). 
	/// 
	/// To load the local json file use 'false' as paraneter of the constructor. 
	/// In this case no download task is needed and if exitent its result will be ignored.
	/// </summary>
	public class ImportLocalQuestInfos : ImportQuestInfos
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="GQ.Client.Model.ImportQuestInfosFromJSON"/> class.
		/// </summary>
		public ImportLocalQuestInfos () : base ()
		{ 
			// import from local quest json file:
			if (File.Exists (QuestInfoManager.LocalQuestInfoJSONPath)) {
				try {
					InputJson = File.ReadAllText (QuestInfoManager.LocalQuestInfoJSONPath);
				} catch (Exception e) {
					Log.SignalErrorToDeveloper ("Error while trying to import local quest info json file: " + e.Message);
					InputJson = "[]";
				}
			}
		}

        protected override void ReadInput(object input = null)
        {
            // we read directly from local file cf. constructor.
        }

        protected override void updateQuestInfoManager (QuestInfo[] newQuests)
        {
	        var oldQIString = qim.QuestDict.ContainsKey(12902) ? qim.QuestDict[12902].ToString() : "12902 not found";
	        var qiX = newQuests.FirstOrDefault(qi => qi.Id == 12902);
	        var newQIString = qiX != null ? qiX.ToString() : "no questInfos";
	        Debug.Log($"ImportLocalQuestInfos.updateQuestInfoManager() Test QI old: {oldQIString} \n new: {newQIString}");
	        
			foreach (var q in newQuests) {
                if (q.Id <= 0 || qim.QuestDict.ContainsKey(q.Id))
					continue;

				qim.AddInfo (q);
			}
			Debug.Log($"ImportLocalQuestInfos.updateQuestInfoManager() END Test QI old: {oldQIString} \n new: {newQIString}");
   		}

	}
}
