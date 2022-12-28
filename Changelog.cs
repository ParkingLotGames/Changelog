#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DevTools.Editor
{
    /// <summary>
    /// Class used to define the behavior of the Changelog editor window.
    /// </summary>
    public class Changelog : EditorWindow
    {
        #region Variables
        /// <summary>
        /// The editor window used to show the Changelog UI.
        /// </summary>
        static Changelog ChangelogWindow;

        string currentVersion;

        /// <summary>
        /// The ChangelogContent asset that contains the entries and settings.
        /// </summary>
        ChangelogContent changelogContent;

        /// <summary>
        /// The ChangelogContent asset that contains the entries and settings.
        /// </summary>
        ChangelogSettings changelogSettings;

        /// <summary>
        /// The directory where the changelogContent asset is saved.
        /// </summary>
        string changelogAssetDirectory = "Assets/Changelog/Content/";

        /// <summary>
        /// The path to the changelogContent asset.
        /// </summary>
        string changelogContentAssetPath = "Assets/Changelog/Content/ChangelogContent.asset";

        /// <summary>
        /// The path to the changelogContent asset.
        /// </summary>
        string changelogSettingsAssetPath = "Assets/Changelog/Content/ChangelogSettings.asset";

        /// <summary>
        /// The path to the ChangelogWindow icon.
        /// </summary>
        static string changelogIconPath = "Assets/Changelog/Changelog-Icon.png";

        /// <summary>
        /// Integer used to define the entryType of each Changelog entry.
        /// </summary>
        int currentEntryTypeFilterIndex = 0;

        /// <summary>
        /// Integer used to initialize the entryType of each Changelog entry.
        /// </summary>
        int newEntryContentEntryTypeIndex = 0;

        /// <summary>
        /// String used to define the title of each new entry.
        /// </summary>
        string newEntryTitle;

        /// <summary>
        /// String used to define the content of each new entry.
        /// </summary>
        string newEntryContent;

        /// <summary>
        /// Boolean used to define a Changelog entry as ready to save to the changelogContent asset.
        /// </summary>
        bool saveable;

        /// <summary>
        /// Vector used to define the current scroll position in the Changelog ChangelogWindow.
        /// </summary>
        Vector2 scrollPosition = Vector2.zero;

        /// <summary>
        /// Boolean used to show or hide the "Create New Entry" Interface.
        /// </summary>
        bool showCreateNewEntryInterface = false;

        static GUIContent titleGUIContent = new GUIContent();

        private bool[] entryFoldouts;

        [MenuItem("Window/Changelog %&c")]
        public static void Init()
        {
            //Define the Title of the window
            titleGUIContent.text = "Changelog";
            titleGUIContent.image = (Texture)AssetDatabase.LoadAssetAtPath(changelogIconPath, typeof(Texture));
            // Get existing open Changelog Window or if it doesn't exist, create one.
            ChangelogWindow = (Changelog)GetWindow(typeof(Changelog));

            //Changelog: Replace with titleContent (GUIContent).
            ChangelogWindow.titleContent = titleGUIContent;
            ChangelogWindow.autoRepaintOnSceneChange = false;
        }

        #endregion

        #region Methods

        private ChangelogContent GetContentForCurrentVersion()
        {
            string[] guids = AssetDatabase.FindAssets("t:ChangelogContent");
            List<ChangelogContent> changelogContentList = new List<ChangelogContent>();
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ChangelogContent changelogContent = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ChangelogContent)) as ChangelogContent;
                changelogContentList.Add(changelogContent);
            }
            changelogContentList.Sort((x, y) => x.version.CompareTo(y.version));
            ChangelogContent contentForCurrentVersion = null;
            for (int i = 0; i < changelogContentList.Count; i++)
            {
                if (changelogContentList[i].version == changelogContent.version)
                {
                    contentForCurrentVersion = changelogContentList[i];
                    break;
                }
            }
            if (contentForCurrentVersion == null)
            {
                contentForCurrentVersion = changelogContent;
                AssetDatabase.CreateAsset(contentForCurrentVersion, "Assets/Changelog/Content/ChangelogContent" + contentForCurrentVersion.version + ".asset");
                GUI.changed = true;
            }
            return contentForCurrentVersion;
        }



        /// <summary>
        /// Draws the Changelog editor Window.
        /// </summary>
        public void OnGUI()
        {
            // Fetch our settings if we haven't.
            if (changelogSettings == null)
            {
                // Fetch our settings if the asset can be found.
                changelogSettings = AssetDatabase.LoadAssetAtPath(changelogSettingsAssetPath, typeof(ChangelogSettings)) as ChangelogSettings;
                if (changelogSettings == null)
                {
                    // If the asset doesn't exist, create it
                    changelogSettings = ScriptableObject.CreateInstance(typeof(ChangelogSettings)) as ChangelogSettings;
                    // Create the directory to store the settings asset.
                    System.IO.Directory.CreateDirectory(Application.dataPath + changelogAssetDirectory);
                    // Save the data asset to disk.
                    AssetDatabase.CreateAsset(changelogSettings, changelogSettingsAssetPath);
                    // Mark the GUI as Changed.
                    GUI.changed = true;
                }
            }

            // Fetch our data if we haven't.
            if (changelogContent == null)
            {
                // Fetch our data if the asset can be found.
                changelogContent = AssetDatabase.LoadAssetAtPath(changelogContentAssetPath, typeof(ChangelogContent)) as ChangelogContent;
                // Create our data asset if it can't be found.
                if (changelogContent == null)
                {
                    // Create an instance of the data asset.
                    changelogContent = CreateInstance(typeof(ChangelogContent)) as ChangelogContent;
                    
                    // Set the version of the changelog content
                    //changelogContent.version = 0.1f;

                    // Set the path + a version.
                    changelogContentAssetPath = "Assets/Changelog/Content/ChangelogContent" + changelogContent.version + ".asset";

                    // Create the directory to store the data asset.
                    System.IO.Directory.CreateDirectory(Application.dataPath + changelogAssetDirectory);
                    // Save the data asset to disk.
                    AssetDatabase.CreateAsset(changelogContent, changelogContentAssetPath);
                    // Mark the GUI as Changed.
                    GUI.changed = true;
                }
            }

            // Find all ChangelogContent assets in the project
            string[] guids = AssetDatabase.FindAssets("t:ChangelogContent");

            // Create a list to store the ChangelogContent objects
            List<ChangelogContent> changelogContentList = new List<ChangelogContent>();

            // Iterate through the GUIDs and load the ChangelogContent objects
            foreach (string guid in guids)
            {
                // Convert the GUID to the asset path
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                // Load the ChangelogContent asset at the asset path
                ChangelogContent changelogContent = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ChangelogContent)) as ChangelogContent;

                // Add the ChangelogContent object to the list
                changelogContentList.Add(changelogContent);
            }

            // Sort the list of ChangelogContent objects by their version
            changelogContentList.Sort((x, y) => x.version.CompareTo(y.version));


            // Display the entry type filter fields.
            string[] entryTypes = new string[changelogSettings.entryTypes.Count + 1];

            // Define the number of entry types defined.
            string[] entryTypesToSelect = new string[changelogSettings.entryTypes.Count];

            // Set index 0 of the entry types selector to show all types.
            entryTypes[0] = "All types";

            // Loop through all available entry types.
            for (int i = 0; i < changelogSettings.entryTypes.Count; i++)
            {
                // Retrieve the name of each entry type and save it as a string.
                entryTypes[i + 1] = changelogSettings.entryTypes[i].name;
                // Save the name of each entry type to the entryTypesToSelect string array.
                entryTypesToSelect[i] = changelogSettings.entryTypes[i].name;
            }

            // Show the entry type Filter label and selector in a single row.
            EditorGUILayout.BeginHorizontal();

            // Create the Filter Label.
            EditorGUILayout.LabelField("Filter:", EditorStyles.boldLabel);

            // Show the entry type selector.
            currentEntryTypeFilterIndex = EditorGUILayout.Popup(currentEntryTypeFilterIndex, entryTypes);

            // End the row
            EditorGUILayout.EndHorizontal();
            // Iterate through the sorted list of ChangelogContent objects and display their contents
            foreach (ChangelogContent changelogContent in changelogContentList)
            {

                // Loop through all available entry types.
                for (int i = 0; i < changelogSettings.entryTypes.Count; i++)
                {
                    // Retrieve the name of each entry type and save it as a string.
                    entryTypes[i + 1] = changelogSettings.entryTypes[i].name;
                    // Save the name of each entry type to the entryTypesToSelect string array.
                    entryTypesToSelect[i] = changelogSettings.entryTypes[i].name;
                }

                // Define an integer to keep track of the number of entries and set it to 0.

                EditorGUILayout.LabelField("v" + changelogContent.version + " Patch Notes", EditorStyles.boldLabel, new GUILayoutOption[0]);

                int displayCount = 0;
                // Show the list of pending entries.

                // Create a style to use for each entry.
                GUIStyle itemStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel);

                // Center our text on each entry.
                itemStyle.alignment = TextAnchor.UpperCenter;

                // Begin the scrollable area that will contain all entries.
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                // Loop through each item in the list.
                for (int i = 0; i < changelogContent.items.Count; i++)
                {
                    // Define the current entry instance being looped over.
                    ChangelogEntry item = changelogContent.items[i];

                    // Retrieve and define the entry type.
                    ChangelogEntryType entryType = item.entryType;

                    // Code to execute if the filter is set to "All".
                    if (currentEntryTypeFilterIndex == 0)
                    {
                        //Changelog: Move this code to a static container or something so it reuses the textures.

                        // Create a 1x1 single entryColor texture using the entryType entryColor.
                        Texture2D colorTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);

                        // Set the entryColor to the entry type entryColor.
                        colorTexture.SetPixel(0, 0, entryType.entryColor);

                        // Save the changes to the texture.
                        colorTexture.Apply();

                        // Set the item background to the corresponding texture.
                        itemStyle.normal.background = colorTexture;

                        // Set the text entryColor based on the entry type for correct visualization.
                        itemStyle.normal.textColor = Color.white;


                        // Add 1 to the display item count.
                        displayCount++;

                        //Show a toggle, a TextArea with the entry contents and an entry type selector in a single row.
                        EditorGUILayout.BeginHorizontal();

                        // Define the text area that displays the entry contents.
                        item.entryContent = EditorGUILayout.TextArea(item.entryContent, itemStyle);

                        // Show an entry type selector
                        int entry = EditorGUILayout.Popup(entryType.index, entryTypesToSelect, GUILayout.Width(60));

                        // Handle the change of entry type to a value different than the one currently used
                        if (entry != entryType.index)
                        {
                            // Replace the index used for the new on
                            item.entryType = changelogSettings.entryTypes[entry];

                            // Set the item to the one currently looped over.
                            // Why? No fucking idea but this was in the original code, I'm just commenting here
                            changelogContent.items[i] = item;
                        }

                        // End row
                        EditorGUILayout.EndHorizontal();

                    }

                    // Code to execute if the filter is set to whatever besides "All".
                    else
                    {
                        // Adjust the index by subtracting the 1 this guy added way earlier in the code
                        // My guess is that this prevents some error with "All" at index 0 but your guess is as good as mine
                        int adjustedIndex = currentEntryTypeFilterIndex - 1;

                        // Set the entry type to the one that corresponds to the adjusted index
                        entryType = changelogSettings.entryTypes[adjustedIndex];

                        // Check if the entry has been correctly set up
                        if (entryType.name == item.entryType.name)
                        {
                            //Changelog: Move this code to a static container or something so it reuses the textures.

                            // Create a 1x1 single entryColor texture using the entryType entryColor.
                            Texture2D colorTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);

                            // Set the entryColor to the entry type entryColor.
                            colorTexture.SetPixel(0, 0, entryType.entryColor);

                            // Save the changes to the texture.
                            colorTexture.Apply();

                            // Set the item background to the corresponding texture.
                            itemStyle.normal.background = colorTexture;

                            // Set the text entryColor based on the entry type for correct visualization.
                            itemStyle.normal.textColor = Color.white;


                            // Add 1 to the display item count.
                            displayCount++;

                            //Show a toggle, a TextArea with the entry contents and an entry type selector in a single row.
                            EditorGUILayout.BeginHorizontal();


                            // Define the text area that displays the entry contents.
                            item.entryContent = EditorGUILayout.TextArea(item.entryContent, itemStyle);

                            // Show an entry type selector
                            int entry = EditorGUILayout.Popup(entryType.index, entryTypesToSelect, GUILayout.Width(60));

                            // Handle the change of entry type to a value different than the one currently used
                            if (entry != entryType.index)
                            {
                                // Replace the index used for the new on
                                item.entryType = changelogSettings.entryTypes[entry];

                                // Set the item to the one currently looped over.
                                // Why? No fucking idea but this was in the original code, I'm just commenting here
                                changelogContent.items[i] = item;
                            }

                            // End row
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                // Code to execute if there are no unfinished entries
                if (displayCount == 0)
                {
                    // Print an informative label
                    EditorGUILayout.LabelField("No entries", EditorStyles.boldLabel);
                }

                EditorGUILayout.EndScrollView();

                // Define the entry or changelogContent asset (not sure which tbh) as saveable if the hotcontrol is not 0
                // (?) must be some kind of detection if you're modifying or not the field as it used to save after each gui change
                // but some dude in the asset store provided this fix.
                if (GUIUtility.hotControl != 0) saveable = true;

                // Code to execute once hot control is 0 and it keeps the saveable attribute (see? I assume 0 is no input).
                if (GUIUtility.hotControl == 0 && saveable)
                {
                    // Set the attribute to false
                    //Changelog: Check if this affects the beavior of saving but I assume not, everything seems to run and save fine.
                    saveable = false;

                    // Set the changelogContent asset as dirty.
                    EditorUtility.SetDirty(changelogContent);

                    // Save the changelogContent Asset to disk.
                    AssetDatabase.SaveAssets();
                }

            }

            if (!showCreateNewEntryInterface)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Create New Entry"))
                {
                    showCreateNewEntryInterface = true;
                }
                if (GUILayout.Button("Create new changelog"))
                {
                    // Create a new ChangelogContent object
                    ChangelogContent newChangelogContent = CreateInstance(typeof(ChangelogContent)) as ChangelogContent;
                    // Set the version of the changelog content to the next version
                    newChangelogContent.version = changelogContent.version + 0.1f;

                    // Save the changelog content asset to a directory of your choosing
                    string newChangelogContentAssetPath = "Assets/Changelog/Content/ChangelogContent" + newChangelogContent.version + ".asset";
                    System.IO.Directory.CreateDirectory(Application.dataPath + "/Changelog/Content/");
                    AssetDatabase.CreateAsset(newChangelogContent, newChangelogContentAssetPath);
                }
                EditorGUILayout.EndHorizontal();

            }
            else
            {

                // Show a title label.
                EditorGUILayout.LabelField("New entry:", EditorStyles.boldLabel);

                // Show a TextArea to input the contents of the new Changelog entry.
                newEntryContent = EditorGUILayout.TextArea(newEntryContent, GUILayout.Height(40));

                EditorGUILayout.BeginHorizontal();

                // Show a dropdown selector with the entry types.
                newEntryContentEntryTypeIndex = EditorGUILayout.Popup(newEntryContentEntryTypeIndex, entryTypesToSelect, GUILayout.Width(60));

                // Button to create the new Changelog entry once some text has been input.
                if ((GUILayout.Button("Create Entry") && newEntryContent != ""))
                {
                    // Define new Changelog entry
                    ChangelogEntryType newEntryType = changelogSettings.entryTypes[newEntryContentEntryTypeIndex];
                    // Add new entry to the Changelog entry list.
                    changelogContent.AddEntry(newEntryType, newEntryTitle, newEntryContent);
                    // Reset the contents of the new entry TextArea once the entry has been added to the list.
                    newEntryContent = "";
                    // Remove focus control from the GUI?
                    showCreateNewEntryInterface = false;
                    // Honestly not sure what this does but my bet is it deselects anything (which would have been the button or TextArea I guess.
                    GUI.FocusControl(null);
                }

                if (GUILayout.Button("Cancel"))
                {
                    showCreateNewEntryInterface = false;
                }

                EditorGUILayout.EndHorizontal();

            }


        }

        /// <summary>
        /// Handles the Changelog behavior when the ChangelogWindow is closed.
        /// </summary>
        void OnDestroy()
        {
            // Set the changelogContent asset as dirty.
            EditorUtility.SetDirty(changelogContent);

            // Save the changelogContent Asset to disk.
            AssetDatabase.SaveAssets();
        }

        #endregion

    }

    /// <summary>
    /// A Scriptable Object used to store the settings for the Changelog.
    /// </summary>
    public class ChangelogSettings : ScriptableObject
    {
        /// <summary>
        /// The list of entry types.
        /// </summary>
        public List<ChangelogEntryType> entryTypes = new List<ChangelogEntryType>();

        public Color bugfixColor = new Color(0.4f, 0.4f, 0.1f);
        public Color qolImprovementColor = new Color(0.15f, 0.3f, 0.15f);
        public Color optmizationColor = new Color(0.1f, 0.15f, 0.4f);
        public Color newFeatureColor = new Color(0.4f, 0.05f, 0.5f);
        public Color regressionColor = new Color(0.3f, 0, 0);


        /// <summary>
        /// Constructor for the ChangelogContent class.
        /// </summary>
        public ChangelogSettings()
        {
            // Define all our entry types and their colors    
            entryTypes.Add(new ChangelogEntryType("Bugfix", bugfixColor, 0));
            entryTypes.Add(new ChangelogEntryType("QOL Improvement", qolImprovementColor, 1));
            entryTypes.Add(new ChangelogEntryType("Optmization", optmizationColor, 2));
            entryTypes.Add(new ChangelogEntryType("New Feature", newFeatureColor, 3));
            entryTypes.Add(new ChangelogEntryType("Regression", regressionColor, 4));

        }

    }

    /// <summary>
    /// A Scriptable Object used to store the entries for a version's Changelog.
    /// </summary>
    public class ChangelogContent : ScriptableObject
    {
        /// <summary>
        /// The list of items.
        /// </summary>
        public List<ChangelogEntry> items = new List<ChangelogEntry>();

        /// <summary>
        /// The build version this changelog asset represents.
        /// </summary>
        public string version;

        /// <summary>
        /// Add a new entry to the list of items.
        /// </summary>
        /// <param name="entryType">The entry type of the new entry.</param>
        /// <param name="entryTitle">The title of the new entry.</param>
        /// <param name="entryContent">The description of the new entry.</param>
        public void AddEntry(ChangelogEntryType entryType, string entryTitle, string entryContent)
        {
            ChangelogEntry item = new ChangelogEntry(entryType, entryTitle, entryContent);
            items.Add(item);
        }
    }

    /// <summary>
    /// A class used to define the contents of each Changelog entry.
    /// </summary>
    [Serializable]
    public class ChangelogEntry
    {
        /// <summary>
        /// The type of the entry.
        /// </summary>
        public ChangelogEntryType entryType;

        /// <summary>
        /// The title of the entry.
        /// </summary>
        public string entryTitle;

        /// <summary>
        /// The description of the entry.
        /// </summary>
        public string entryContent;

        /// <summary>
        /// Constructor for the ChangelogEntry class.
        /// </summary>
        /// <param name="entryType">The Entry type of the entryContent.</param>
        /// <param name="entryContent">The description of the entryContent.</param>
        public ChangelogEntry(ChangelogEntryType entryType, string entryTitle, string entryContent)
        {
            this.entryType = entryType;
            this.entryTitle = entryTitle;
            this.entryContent = entryContent;

        }
    }

    /// <summary>
    /// A class used to define the properties of each Changelog entry type.
    /// </summary>
    [Serializable]
    public class ChangelogEntryType
    {
        /// <summary>
        /// The name of the entry type.
        /// </summary>
        public string name;

        /// <summary>
        /// The entryColor used to represent the entry type.
        /// </summary>
        public Color entryColor;


        /// <summary>
        /// The index of the entry type in the list of entry types.
        /// </summary>
        public int index;

        /// <summary>
        /// Constructor for the ChangelogEntryType class.
        /// </summary>
        /// <param name="name">The name of the entry type.</param>
        /// <param name="entryColor">The entryColor used to represent the entry type.</param>
        /// <param name="entryFinishedColor">The entryColor used to represent the entry type once marked as finished.</param>
        /// <param name="index">The index of the entry type in the list of entry types.</param>
        public ChangelogEntryType(string name, Color entryColor, int index)
        {
            this.name = name;
            this.entryColor = entryColor;
            this.index = index;
        }
    }
}
#endif
