using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Script.Editor.AssetViewerUploader
{
    public class AssetViewerPrefabSelect : UnityEditor.Editor
    {
        [MenuItem("Assets/Asset Viewer/Upload Asset")]
        private static void UploadAsset()
        {
            Object prefab = Selection.activeGameObject;
            
            //Check if is a Prefab
            string path = AssetDatabase.GetAssetPath (prefab);

            if (!String.IsNullOrEmpty(path))
            {
                //Get Prefab Root
                GameObject root = Selection.activeGameObject.transform.root.gameObject;
                
                Selection.activeGameObject = root;
                
                //Open Window
                AssetViewerMainWindow assetUpload = EditorWindow.GetWindow<AssetViewerMainWindow>(true, "Asset Uploader", true);
                assetUpload.Init(path, prefab.name);
            }
            else
            {
                //Warning ios not a prefab yet
                EditorUtility.DisplayDialog("Error", "Not a Prefab", "OK");
            }
        }
    }  
}

