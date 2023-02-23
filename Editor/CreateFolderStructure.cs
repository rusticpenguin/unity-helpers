using UnityEditor;
using UnityEngine;
using System.IO;
using System;

public class CreateFolderStructure: EditorWindow {
    private static string[] folders = new string[] {
        "Art",
        "Audio",
        "Prefabs",
        "Scenes",
        "Scripts",
        "Textures"
    };

    static float scaleTarget = 1920f;
    static int baseVerticalOffset = 300;
    static int baseWidth = 600;


    [MenuItem("Window/Create Folder Structure")]
    public static void ShowWindow() {;
        var screenWidth = Screen.currentResolution.width;
        var screenHeight = Screen.currentResolution.height;
        float scaleFactor = screenWidth / scaleTarget;
        float verticalOffset = baseVerticalOffset * scaleFactor;

        float rowHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        int numRows = folders.Length - 5;
        float newHeight = numRows * rowHeight + verticalOffset;

        var min = new Vector2(baseWidth * scaleFactor, newHeight);

        var defaultScreenLocation = new Vector2((screenWidth - min.x) / 2f, (screenHeight - verticalOffset - min.y) / 2f);
        int fontSize = (int)(12 * scaleFactor);

        GUIStyle style = new GUIStyle();
        style.fontSize = fontSize;

        var window = GetWindow<CreateFolderStructure>();
        window.position = new Rect(defaultScreenLocation.x, defaultScreenLocation.y, min.x, min.y);
        window.minSize = min;
        window.maxSize = min;
        window.titleContent = new GUIContent("Create Folder Structure");
    }

    void OnGUI() {
        EditorGUILayout.BeginVertical();
        GUILayout.Label("Folders", EditorStyles.boldLabel);
        foreach (string folder in folders) {
            FolderRowComponent.Render(folder, (f) => RemoveFolder(f));
        }

        AddFolderComponent.Render((newFolder) => AddNewFolder(newFolder));
    

        GUILayout.FlexibleSpace();
        CreateFoldersComponent.Render((useProjectName) => Create(useProjectName));

        EditorGUILayout.EndVertical();
    }

    private bool IsDuplicateFolder(string folder) {
        foreach (string f in folders) {
            if (f == folder) {
                return true;
            }
        }
        return false;
    }

    private void AddNewFolder(string newFolder) {
        if (IsDuplicateFolder(newFolder)) {
            EditorUtility.DisplayDialog("Duplicate Folder", "Folder already exists.", "OK");
            return;
        }
        if (string.IsNullOrEmpty(newFolder)) {
            EditorUtility.DisplayDialog("Empty Folder Name", "Folder name cannot be empty.", "OK");
            return;
        }

        string[] newFolders = new string[folders.Length + 1];
        int i = 0;

        foreach (string folder in folders) {
            newFolders[i] = folder;
            i++;
        }
        newFolders[i] = newFolder;
        folders = newFolders;

        RecalculateWindowSize();
    }

    protected void RemoveFolder(string folder) {
        string[] newFolders = new string[folders.Length - 1];
        int i = 0;

        foreach (string f in folders) {
            if (f != folder) {
                newFolders[i] = f;
                i++;
            }
        }
        folders = newFolders;

        RecalculateWindowSize();
    }

    private void Create(bool useProjectName) {
        string path = Application.dataPath;
        if (useProjectName == true) {
            path +=  "/" + PlayerSettings.productName;
        }    
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }

        foreach (string folder in folders) {
            string folderPath = path + "/" + folder;
            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }
        }

        AssetDatabase.Refresh();
        this.Close();
    }

    private void RecalculateWindowSize() {
        float scaleFactor = Screen.currentResolution.width / scaleTarget;
        float verticalOffset = baseVerticalOffset * scaleFactor;
        float rowHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        int numRows = folders.Length - 5;
        float newHeight = numRows * rowHeight + verticalOffset;
        Vector2 min = new Vector2(minSize.x, newHeight);

        position = new Rect(position.x, position.y, min.x, min.y);
        minSize = min;
        maxSize = min;
    }
}

class FolderRowComponent {
    public static void Render(string folder, Action<string> removeFolder) {
        GUILayout.Space(1);
        GUILayout.BeginHorizontal();

        EditorGUI.indentLevel = 2;
        EditorGUILayout.LabelField(folder);
        EditorGUI.indentLevel = 0;

        if (GUILayout.Button("Remove", GUILayout.Width(60))) {
            removeFolder(folder);
        }

        GUILayout.EndHorizontal();
    }
}

class AddFolderComponent {
    private static string newFolderName = "";

    public static void Render(Action<string> addNewFolder) {
        GUILayout.Space(10);

        GUILayout.Label("Add Folder", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUI.SetNextControlName("NewFolderNameTextField");
        newFolderName = EditorGUILayout.TextField(newFolderName);

        if (Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "NewFolderNameTextField") {
            addNewFolder(newFolderName);
            newFolderName = "";

            GUI.FocusControl("NewFolderNameTextField");
        }

        if (GUILayout.Button("Add", GUILayout.Width(60))) {
            addNewFolder(newFolderName);
            newFolderName = "";

            GUI.FocusControl("NewFolderNameTextField");
        }

        GUILayout.EndHorizontal();
    }
}

class CreateFoldersComponent {
    private static bool UseProjectName = true;

    public static void Render(Action<bool> Create) {
        GUILayout.Label("Create Folders", EditorStyles.boldLabel);
        GUILayout.Label("This will non-destructively create the folder structure above the Assets folder.");

        EditorGUILayout.Space(8);

        UseProjectName = EditorGUILayout.Toggle("Create parent folder", UseProjectName);

        if (GUILayout.Button("Create")) {
            Create(UseProjectName);
        }
        GUILayout.Space(10);
    }
}