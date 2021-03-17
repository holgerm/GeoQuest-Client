using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Code.GQClient.Err;
using Code.QM.Util;
using Newtonsoft.Json;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

#if UNITY_EDITOR

#endif

namespace Code.GQClient.Conf
{
    /// <summary>
    /// Config class specifies textual parameters of a product. It is used both at runtime to initilize the app's branding details from and 
    /// at editor time to back the product editor view and store the parameters while we use the editor.
    /// </summary>
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public class RTConfig
    {
        #region Parse Helper

        public static bool __JSON_Currently_Parsing = false;

        /// <summary>
        /// Used during deserialization of json file. Shows whether the json text used in-app resources (true) or
        /// application persistent files (false). Used to put that information e.g. into ImagePath objects.
        /// </summary>
        public static LoadsFrom CurrentLoadingMode;

        public enum LoadsFrom
        {
            Resource,
            LocalFile,
            RemoteFile
        }

        public static RTConfig _doDeserialize(string configText, LoadsFrom loadMode)
        {
            __JSON_Currently_Parsing = true;
            CurrentLoadingMode = loadMode;
            RTConfig rtConfig = JsonConvert.DeserializeObject<RTConfig>(configText);
            rtConfig.RefreshCategoryDictionary();
            __JSON_Currently_Parsing = false;
            return rtConfig;
        }

        #endregion

        //////////////////////////////////////////
        // THE ACTUAL PRODUCT RUNTIME CONFIG DATA:

        public bool foldableCategoryFilters { get; set; }

        public bool categoryFiltersStartFolded { get; set; }

        public bool categoryFoldersStartFolded { get; set; }

        /// <summary>
        /// Used as characterization of the quest infos, e.g. to determine the shown symbols in the foyer list.
        /// </summary>
        /// <value>The main category set.</value>
        public string mainCategorySet { get; set; }
        
        /// <summary>
        /// Shows quests also in the case that at least one category class has no user selections.
        /// We interpret the complete deselection of a category class as "does not matter", so we show all.
        /// 
        /// For apps with only one class of categories this will probably be most often "false", while
        /// for apps with multiple category classes, "true" will probably be the best value.
        /// </summary>
        public bool showAllIfNoCatSelectedInFilter { get; set; } 

        /// <summary>
        /// If a quest has no category stated or at least no correctly spelled category, this option decides
        /// whether we show it anyway or not.
        ///
        /// For apps with no special editors so that you have to specify the categories in metadata pages
        /// "true" is best bet, since specifying categories will often be overseen. For apps where the categories
        /// are highlighted during the editing process "false" will be the standard.
        /// In the future we will support error free editors that insist of a valid category definition
        /// before a quest can be published. Hence this option is only needed fot the time being. 
        /// </summary>
        public bool showIfNoCatDefined { get; set; } 

        public List<CategorySet> CategorySets
        {
            get
            {
                if (_categorySets == null)
                {
                    _categorySets = new List<CategorySet>();
                    return _categorySets;
                }

                return _categorySets;
            }
            set { _categorySets = value; }
        }

        internal void RefreshCategoryDictionary()
        {
            if (null == _categorySets) return;
            _categoryDict = new Dictionary<string, Category>();

            foreach (CategorySet cs in _categorySets)
            {
                foreach (Category c in cs.categories)
                {
                    _categoryDict[c.id] = c;
                }
            }

            // CategoriesChanged?.Invoke();
        }

        public string defaultCategory { get; set; }

        [JsonIgnore] private List<CategorySet> _categorySets;

        public static event VoidToVoid CategoriesChanged;


        public Category GetCategory(string catId)
        {
            if (!categoryDict.ContainsKey(catId))
            {
                return null;
            }

            return categoryDict[catId];
        }

        [JsonIgnore] internal Dictionary<string, Category> _categoryDict;


        [JsonIgnore]
        internal Dictionary<string, Category> categoryDict
        {
            get
            {
                if (_categoryDict == null)
                {
                    RefreshCategoryDictionary();
                }

                return _categoryDict;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RTConfig"/> class and initializes it with generic default values.
        /// 
        /// This constructor is used by the ProductManager (explicit) as well as the JSON.Net deserialize method (via reflection).
        /// </summary>
        public RTConfig()
        {
            foldableCategoryFilters = true;
            categoryFiltersStartFolded = true;
            categoryFoldersStartFolded = true;
            showAllIfNoCatSelectedInFilter = false;
            showIfNoCatDefined = false;
        }
    }
}