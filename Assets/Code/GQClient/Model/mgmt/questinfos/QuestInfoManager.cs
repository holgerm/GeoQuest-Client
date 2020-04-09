﻿//#define DEBUG_LOG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.GQClient.Conf;
using Code.GQClient.Err;
using Code.GQClient.Model.mgmt.quests;
using Code.GQClient.UI.Foyer.containers;
using Code.GQClient.UI.menu.categories;
using Code.GQClient.UI.menu.viewToggle;
using Code.GQClient.Util;
using Code.GQClient.Util.http;
using Code.GQClient.Util.tasks;
using Code.QM.Util;
using GQClient.Model;
using UnityEngine;

namespace GQClient.Model
{

    /// <summary>
    /// Manages the meta data for all quests available: locally on the device as well as remotely on the server.
    /// </summary>
    public class QuestInfoManager
    {

        #region store & access data

        public static string LocalQuestsPath
        {
            get
            {
                if (!Directory.Exists(Device.GetPersistentDatapath() + "/quests/"))
                {
                    Directory.CreateDirectory(Device.GetPersistentDatapath() + "/quests/");
                }
                return Device.GetPersistentDatapath() + "/quests/";
            }
        }

        public static string LocalQuestInfoJsonPath => LocalQuestsPath + "infos.json";

        public Dictionary<int, QuestInfo> QuestDict
        {
            get;
            set;
        }

        public List<QuestInfo> GetListOfQuestInfos()
        {
            return QuestDict.Values.ToList<QuestInfo>();
        }

        public IEnumerable<QuestInfo> GetFilteredQuestInfos()
        {
            return QuestDict.Values.Where(x => Filter.Accept(x)).ToList();
        }

        public bool ContainsQuestInfo(int id)
        {
            return QuestDict.ContainsKey(id);
        }

        public int Count
        {
            get
            {
                return QuestDict.Count;
            }
        }

        public QuestInfo GetQuestInfo(int id)
        {
            QuestInfo questInfo;
            return (QuestDict.TryGetValue(id, out questInfo) ? questInfo : null);
        }

        #endregion


        #region Filter

        private QuestInfoFilter _filter;

        public QuestInfoFilter Filter
        {
            get
            {
                return _filter;
            }
            protected set
            {
                if (_filter != value)
                {
                    _filter = value;
                    // we register with later changes of the filter:
                    _filter.filterChange += FilterChanged;
                    // we use the new filter instantly:
                    FilterChanged();
                }
            }
        }

        public QuestInfoFilter.CategoryFilter CategoryFilter;

        public Dictionary<string, QuestInfoFilter.CategoryFilter> CategoryFilters;

        /// <summary>
        /// Adds the given andFilter in conjunction to the current filter(s).
        /// </summary>
        /// <param name="andFilter">And filter.</param>
        public void FilterAnd(QuestInfoFilter andFilter)
        {
            Filter = new QuestInfoFilter.And(Filter, andFilter);
        }

        public event ChangeCallback OnFilterChange;

        public void FilterChanged()
        {
            if (OnFilterChange != null)
            {
                OnFilterChange(
                    this,
                    new QuestInfoChangedEvent(
                        "Filter changed ...",
                        ChangeType.FilterChanged,
                        newQuestInfo: null,
                        oldQuestInfo: null
                    )
                );
            }
        }

        #endregion


        #region Quest Info Changes
        public void AddInfo(QuestInfo newInfo)
        {
            QuestDict.Add(newInfo.Id, newInfo);

            if (Filter.Accept(newInfo))
            {
                // Run through filter and raise event if involved:
                onDataChange?.Invoke(
                    this,
                    new QuestInfoChangedEvent(
                        $"Info for quest {newInfo.Name} added.",
                        type: ChangeType.AddedInfo,
                        newQuestInfo: newInfo
                    )
                );
            }
        }

        public void UpdateInfo(QuestInfo changedInfo)
        {
            if (!QuestDict.TryGetValue(changedInfo.Id, out var curInfo))
            {
                Log.SignalErrorToDeveloper("Quest Inf {0} could not be updated because ot was not found locally.",
                    changedInfo.Id);
                return;
            }
            
            curInfo.QuestInfoHasBeenUpdatedTo(changedInfo);

            // React also as container to a change info event
            if (Filter.Accept(curInfo)) // TODO should we also do it, if the new qi passes the filter?
            {
                // Run through filter and raise event if involved

                onDataChange?.Invoke(
                    this,
                    new QuestInfoChangedEvent(
                        message: $"Info for quest {curInfo.Name} changed.",
                        type: ChangeType.ChangedInfo,
                        newQuestInfo: curInfo,
                        oldQuestInfo: curInfo
                    )
                );
            }

        }

        public void RemoveInfo(int oldInfoID)
        {
#if DEBUG_LOG
            Debug.Log("RemoveInfo(" + oldInfoID + ")");
#endif

            QuestInfo oldInfo = null;
            if (!QuestDict.TryGetValue(oldInfoID, out oldInfo))
            {
                Log.SignalErrorToDeveloper(
                    "Trying to remove quest info with ID {0} but it deos not exist in QuestInfoManager.",
                    oldInfoID
                );
                return;
            }

            oldInfo.Dispose();
            QuestDict.Remove(oldInfoID);
            
            if (Filter.Accept(oldInfo))
            {
                // Run through filter and raise event if involved

                onDataChange?.Invoke(
                    this,
                    new QuestInfoChangedEvent(
                        $"Info for quest {oldInfo.Name} removed.",
                        type: ChangeType.RemovedInfo,
                        oldQuestInfo: oldInfo
                    )
                );
            }
        }

        /// <summary>
        /// Updates the quest infos from the server and intergrates the gathered data into the local data. 
        /// 
        /// Should be called in cases like the list is shown again (or first time), 
        /// the server connection is gained back, the last update is long ago or the user demands an update.
        /// </summary>
        public void UpdateQuestInfos()
        {
            ImportQuestInfos importLocal =
                new ImportLocalQuestInfos();
            var unused = Base.Instance.GetSimpleBehaviour(
                importLocal,
                string.Format("Aktualisiere {0}", ConfigurationManager.Current.nameForQuestsPl),
                string.Format("Lese lokale {0}", ConfigurationManager.Current.nameForQuestSg)
            );

            var downloader =
                new Downloader(
                    url: ConfigurationManager.UrlPublicQuestsJSON,
                    timeout: ConfigurationManager.Current.timeoutMS,
                    maxIdleTime: ConfigurationManager.Current.maxIdleTimeMS
                );
            var unused2 = Base.Instance.GetDownloadBehaviour(
                downloader,
                string.Format("Aktualisiere {0}", ConfigurationManager.Current.nameForQuestsPl)
            );

            ImportQuestInfos importFromServer =
                new ImportServerQuestInfos();
            var unused3 = Base.Instance.GetSimpleBehaviour(
                importFromServer,
                string.Format("Aktualisiere {0}", ConfigurationManager.Current.nameForQuestsPl),
                string.Format("Neue {0} werden lokal gespeichert", ConfigurationManager.Current.nameForQuestsPl)
            );

            ExportQuestInfosToJSON exporter =
                new ExportQuestInfosToJSON();
            var unused4 = Base.Instance.GetSimpleBehaviour(
                exporter,
                string.Format("Aktualisiere {0}", ConfigurationManager.Current.nameForQuestsPl),
                string.Format("{0}-Daten werden gespeichert", ConfigurationManager.Current.nameForQuestSg)
            );

            var autoLoader =
                new AutoLoadAndUpdate();

            var t = 
                new TaskSequence(
                    importLocal, 
                    downloader, 
                    importFromServer, 
                    exporter, 
                    autoLoader);
            t.OnTaskCompleted += OnQuestInfosUpdateSucceeded;
            t.Start();
        }

        /// <summary>
        /// Updates quest infos based on the local infos only. No server connection needed.
        /// </summary>
        public void UpdateLocalQuestInfosOnly()
        {
            ImportQuestInfos importLocal =
                new ImportLocalQuestInfos();

            importLocal.Start();
        }

        /// <summary>
        /// Here one can register listeners that will be called each time when the quest infos are successfully updated.
        /// </summary>
        public static event Task.TaskCallback OnQuestInfosUpdateSucceeded;

        public delegate void ChangeCallback(object sender, QuestInfoChangedEvent e);

        private event ChangeCallback onDataChange;

        public event ChangeCallback OnDataChange
        {
            add
            {
                onDataChange += value;
                value(
                    this,
                    new QuestInfoChangedEvent(
                        "Initializing listener ...",
                        ChangeType.ListChanged,
                        newQuestInfo: null,
                        oldQuestInfo: null
                    )
                );
            }
            remove
            {
                onDataChange -= value;
            }
        }

        #endregion


        #region singleton

        private static QuestInfoManager _instance = null;

        public static QuestInfoManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new QuestInfoManager();
                    _instance.initViews();
                    _instance.initFilters();
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        public static void Reset()
        {
            _instance = null;
        }

        public QuestInfoManager()
        {
            // init quest info store:
            QuestDict = new Dictionary<int, QuestInfo>();
        }

        void initViews()
        {
            if (ConfigurationManager.Current.questInfoViews == null || ConfigurationManager.Current.questInfoViews.Length == 0)
            {
                Log.SignalErrorToDeveloper("No quest info views defined for this app. Fix that!");
                return;
            }

            var startView = ConfigurationManager.Current.questInfoViews[0];
            Base.Instance.ListCanvas.gameObject.SetActive(startView == QuestInfoView.List.ToString());
            Base.Instance.TopicTreeCanvas.gameObject.SetActive(startView == QuestInfoView.TopicTree.ToString());
            Base.Instance.MapCanvas.gameObject.SetActive(startView == QuestInfoView.Map.ToString());
            Base.Instance.MapHolder.gameObject.SetActive(startView == QuestInfoView.Map.ToString());

            // check whether we have alternative views to offer:
            if (ConfigurationManager.Current.questInfoViews.Length <= 1)
                return;

            // Create the multitoggle View for the view alternatives currently not displayed, i.e. 2 to n:
            var menuContent = Base.Instance.MenuTopLeftContent;
            ViewToggleController.Create(menuContent);
        }

        /// <summary>
        /// Initializes the quest info filters, e.g. called at start when the QuestInfoManager is initialized.
        /// </summary>
		void initFilters()
        {
            // init filters
            Filter = new QuestInfoFilter.All();

            // init hidden quests filter:
            FilterAnd(QuestInfoFilter.HiddenQuestsFilter.Instance);

            // init local quests filter:
            FilterAnd(QuestInfoFilter.LocalQuestInfosFilter.Instance);
            
            // init TopicFilter:
            FilterAnd(TopicFilter.Instance);

            // init category filters:
            CategoryFilters = new Dictionary<string, QuestInfoFilter.CategoryFilter>();
            var catSets = ConfigurationManager.Current.CategorySets;
            foreach (var catSet in catSets)
            {
                CategoryFilters[catSet.name] = new QuestInfoFilter.CategoryFilter(catSet);
                FilterAnd(CategoryFilters[catSet.name]);
            }

            // create UI for Category Filters:
            var menuContent = Base.Instance.MenuTopLeftContent;
            foreach (var catSet in ConfigurationManager.Current.CategorySets)
            {
                CategoryTreeCtrl.Create(
                    root: menuContent, 
                    catFilter: CategoryFilters[catSet.name], 
                    categories: catSet.categories);
            }
        }

        #endregion

        /// <summary>
        /// Remove listeners of QuestContainerController instance if possible:
        /// </summary>
        /// <param name="qcc"></param>
        public static void DoDestroy(QuestContainerController qcc)
        {
            if (_instance != null)
            {
                Instance.OnDataChange -= qcc.OnQuestInfoChanged;
                Instance.OnFilterChange -= qcc.OnQuestInfoChanged;
            }
        }
    }

    public class QuestInfoChangedEvent : EventArgs
    {
        public string Message { get; protected set; }

        public ChangeType ChangeType { get; protected set; }

        public QuestInfo NewQuestInfo { get; protected set; }

        public QuestInfo OldQuestInfo { get; protected set; }

        public QuestInfoChangedEvent(
            string message = "",
            ChangeType type = ChangeType.ChangedInfo,
            QuestInfo newQuestInfo = null,
            QuestInfo oldQuestInfo = null
        )
        {
            Message = message;
            ChangeType = type;
            NewQuestInfo = newQuestInfo;
            OldQuestInfo = oldQuestInfo;
        }

        public override string ToString()
        {
            var message = $"{ChangeType}: {Message}";
            return message;
        }
    }

    public enum ChangeType
    {
        AddedInfo,
        RemovedInfo,
        ChangedInfo,
        ListChanged,
        FilterChanged,
        SorterChanged
    }


}
