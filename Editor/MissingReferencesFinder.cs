using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MissingReferencesFinder : EditorWindow {

    private List<string> prefabPathsWithMissingLinks = new List<string>();
    private List<string> allPrefabPaths = new List<string>();
    private int currentPrefabIndex = 0;

    private List<string> scriptableObjectPathsWithMissingLinks = new List<string>();
    private List<string> allScriptableObjectPaths = new List<string>();
    private int currentSOIndex = 0;

    private bool isScanning = false;
    private enum ScanPhase { None, Prefabs, ScriptableObjects }
    private ScanPhase currentPhase = ScanPhase.None;

    [MenuItem("Tools/Find Missing References")]
    public static void ShowWindow() {
        GetWindow<MissingReferencesFinder>("Missing References Finder");
    }

    private void OnGUI() {
        
        GUILayout.Label("Missing References Finder", EditorStyles.boldLabel);

        if (!this.isScanning) {
            
            if (GUILayout.Button("Scan Prefabs")) {
                this.StartAsyncPrefabScan();
            }

            if (GUILayout.Button("Scan ScriptableObjects")) {
                this.StartAsyncScriptableObjectScan();
            }

            if (this.prefabPathsWithMissingLinks.Count > 0) {
                
                if (GUILayout.Button("Delete Missing References in Prefabs")) {
                    this.DeleteMissingReferencesInPrefabs();
                }
                
            }
            
        } else {
            
            GUILayout.Label($"Scanning {this.currentPhase}...");
            
        }

        GUILayout.Space(10);

        this.DrawResultsSection("Prefabs with Missing References", this.prefabPathsWithMissingLinks);
        this.DrawResultsSection("ScriptableObjects with Missing References", this.scriptableObjectPathsWithMissingLinks);
    }

    private void DrawResultsSection(string title, List<string> paths) {
        
        GUILayout.Label($"{title}: {paths.Count}", EditorStyles.boldLabel);
        foreach (var path in paths) {
            
            if (GUILayout.Button(path, EditorStyles.miniButton)) {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                Selection.activeObject = asset;
            }
            
        }
        
    }

    private void StartAsyncPrefabScan() {
        
        this.prefabPathsWithMissingLinks.Clear();

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        
        this.allPrefabPaths = new List<string>();
        foreach (var guid in prefabGuids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            this.allPrefabPaths.Add(path);
        }
        this.currentPrefabIndex = 0;

        this.currentPhase = ScanPhase.Prefabs;
        this.isScanning = true;
        EditorApplication.update += this.AsyncScanStep;
    }

    private void StartAsyncScriptableObjectScan() {
        
        this.scriptableObjectPathsWithMissingLinks.Clear();

        string[] soGuids = AssetDatabase.FindAssets("t:ScriptableObject");
        
        this.allScriptableObjectPaths = new List<string>();
        foreach (var guid in soGuids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            this.allScriptableObjectPaths.Add(path);
        }
        this.currentSOIndex = 0;

        this.currentPhase = ScanPhase.ScriptableObjects;
        this.isScanning = true;
        EditorApplication.update += this.AsyncScanStep;
    }

    private void AsyncScanStep() {
        
        if (this.currentPhase == ScanPhase.Prefabs) {
            
            if (this.currentPrefabIndex >= this.allPrefabPaths.Count) {
                this.FinishScan();
                return;
            }

            string path = this.allPrefabPaths[this.currentPrefabIndex];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            bool hasMissing = false;

            if (prefab != null) {
                
                Component[] components = prefab.GetComponentsInChildren<Component>(true);
                foreach (var c in components) {
                    
                    if (c == null) {
                        
                        hasMissing = true;
                        Debug.LogWarning($"[MissingReferencesFinder] Missing script in prefab: {path}");
                        break;
                        
                    }
                    
                }

                if (this.CheckMissingReferencesInPrefab(prefab, path)) {
                    hasMissing = true;
                }
                
            }

            if (hasMissing && !this.prefabPathsWithMissingLinks.Contains(path)) {
                this.prefabPathsWithMissingLinks.Add(path);
            }

            this.currentPrefabIndex++;
            if (this.currentPrefabIndex % 10 == 0 || this.currentPrefabIndex == this.allPrefabPaths.Count) {
                EditorUtility.DisplayProgressBar("Scanning Prefabs", path, (float)this.currentPrefabIndex / this.allPrefabPaths.Count);
            }

            if (this.currentPrefabIndex == this.allPrefabPaths.Count) {
                EditorUtility.ClearProgressBar();
            }
            
        }
        else if (this.currentPhase == ScanPhase.ScriptableObjects) {
            
            if (this.currentSOIndex >= this.allScriptableObjectPaths.Count) {
                this.FinishScan();
                return;
            }

            string path = this.allScriptableObjectPaths[this.currentSOIndex];
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            bool hasMissing = false;

            if (asset == null) {
                
                hasMissing = true;
                Debug.LogWarning($"[MissingReferencesFinder] Missing ScriptableObject at path: {path}");
                
            } else {
                
                if (this.CheckMissingReferencesInScriptableObject(asset, path)) {
                    hasMissing = true;
                }
                
            }

            if (hasMissing && !this.scriptableObjectPathsWithMissingLinks.Contains(path)) {
                this.scriptableObjectPathsWithMissingLinks.Add(path);
            }

            this.currentSOIndex++;
            if (this.currentSOIndex % 10 == 0 || this.currentSOIndex == this.allScriptableObjectPaths.Count) {
                EditorUtility.DisplayProgressBar("Scanning ScriptableObjects", path, (float)this.currentSOIndex / this.allScriptableObjectPaths.Count);
            }

            if (this.currentSOIndex == this.allScriptableObjectPaths.Count) {
                EditorUtility.ClearProgressBar();
            }
            
        }
        
    }

    private bool CheckMissingReferencesInPrefab(GameObject prefabRoot, string prefabPath) {
        
        bool hasMissing = false;
        var allComponents = prefabRoot.GetComponentsInChildren<Component>(true);

        foreach (var c in allComponents) {
            
            if (c == null) {
                
                Debug.LogWarning($"[MissingReferencesFinder] Missing script found in Prefab '{prefabPath}'");
                hasMissing = true;
                continue;
                
            }

            SerializedObject so = new SerializedObject(c);
            SerializedProperty sp = so.GetIterator();

            while (sp.NextVisible(true)) {
                
                if (sp.propertyType == SerializedPropertyType.ObjectReference) {
                    
                    if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0) {
                        
                        Debug.LogWarning($"[MissingReferencesFinder] Missing reference in Prefab '{prefabPath}' on component '{c.GetType().Name}', field '{sp.displayName}'");
                        hasMissing = true;
                    }
                    
                }
                
            }
            
        }

        return hasMissing;
    }

    private bool CheckMissingReferencesInScriptableObject(Object scriptableObject, string assetPath) {
        
        bool hasMissing = false;

        SerializedObject so = new SerializedObject(scriptableObject);
        SerializedProperty sp = so.GetIterator();

        while (sp.NextVisible(true)) {
            
            if (sp.propertyType == SerializedPropertyType.ObjectReference) {
                
                if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0) {
                    
                    Debug.LogWarning($"[MissingReferencesFinder] Missing reference in ScriptableObject '{assetPath}', field '{sp.displayName}'");
                    hasMissing = true;
                }
                
            }
            
        }

        return hasMissing;
    }

    private void DeleteMissingReferencesInPrefabs() {
        
        int total = this.prefabPathsWithMissingLinks.Count;
        
        for (int i = 0; i < total; i++) {
            
            string path = this.prefabPathsWithMissingLinks[i];

            EditorUtility.DisplayProgressBar("Deleting missing references in Prefabs", path, (float)i / total);

            GameObject prefab = PrefabUtility.LoadPrefabContents(path);
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);
            PrefabUtility.SaveAsPrefabAsset(prefab, path);
            PrefabUtility.UnloadPrefabContents(prefab);
            
        }

        EditorUtility.ClearProgressBar();
        Debug.Log("[MissingReferencesFinder] Deleted missing references in all Prefabs.");

        this.prefabPathsWithMissingLinks.Clear();
    }

    private void FinishScan() {
        
        EditorApplication.update -= this.AsyncScanStep;
        
        this.isScanning = false;
        this.currentPhase = ScanPhase.None;
        
        EditorUtility.ClearProgressBar();
        Debug.Log($"[MissingReferencesFinder] Scan completed.");
    }
    
}
