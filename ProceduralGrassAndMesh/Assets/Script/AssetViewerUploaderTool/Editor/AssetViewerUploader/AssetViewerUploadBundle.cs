using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Script.Editor.AssetViewerUploader
{
    public class AssetViewerUploadBundle : UnityEditor.Editor
    {
        private static int packageIndex = 0;
        private static int packageCount;
        private static UnityWebRequest www;
        private static float _uploadProgress;
        private static bool _isUploadComplete = false;
        
        public struct UploadDataInfo
        {
            public string project;
            public string category;
            public string userName;
            public string mainAsset;
            public int unityVersion;
        }
        
        public static void SendPackageToServer(string[] packages, UploadDataInfo dataInfo)
        {
            _isUploadComplete = false;
            
            packageCount = packages.Length;
        
            foreach (var package in packages)
            {
                Request(package, dataInfo); 
            }
       
            EditorApplication.update += EditorUpdate;
        }
        
        static void Request(string packagePath, UploadDataInfo dataInfo)
        {
            string BundleName = System.IO.Path.GetFileName(packagePath);
            
            if (!String.IsNullOrEmpty(BundleName))
            {
                BundleName = BundleName.ToLower();
            }
            
            string path = packagePath;
            byte[] package = System.IO.File.ReadAllBytes(path);
            
            //----Upload file
            string getData = "?project=" + dataInfo.project;
            getData += "&category=" + dataInfo.category; 
            getData += "&user=" + dataInfo.userName; 
            getData += "&asset=" + dataInfo.mainAsset; 
            getData += "&unityVersion=" + dataInfo.unityVersion; 
            getData += "&bundleName=" + BundleName; 

            www = UnityWebRequest.Put(Script.AssetViewerUploaderTool.Config.AssetViewerConfig.SERVER_UPLOAD + getData, package);
            www.SendWebRequest();
        
            Debug.Log("Uploading: " + BundleName);
        }
        
        private static void EditorUpdate()
        {
            if (!www.isDone)
            {
                _uploadProgress = www.uploadProgress;
                return;
            }
            
            if (www.isNetworkError)
            {
                Debug.Log("Error");
            }
            else
            {
                string response = www.downloadHandler.text;
                packageIndex++;
            }

            if (packageIndex > packageCount)
            {
                www.Dispose();
                EditorApplication.update -= EditorUpdate;
                UploadComplete();
            }
        }

        private static void UploadComplete()
        {
            Debug.Log("Upload Complete");
            _isUploadComplete = true;
        }

        public static bool GetUploadStatus()
        {
            return _isUploadComplete;
        }

        public static float GetUploadProgress()
        {
            return _uploadProgress;
        }
    }
}
