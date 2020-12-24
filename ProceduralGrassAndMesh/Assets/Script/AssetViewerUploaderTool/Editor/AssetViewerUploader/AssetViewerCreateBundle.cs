using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Script.Editor.AssetViewerUploader
{
    public class AssetViewerCreateBundle
    {
        public static string[] CollectAssetDependencies(string assetPath)
        {
            string[] dep = AssetDatabase.GetDependencies(assetPath);

            foreach (var d in dep)
            {
                Debug.Log(d);
            }
            return dep;

        }
        
        //This would be inside another class
        public static void BuildGranularAssetBundle(string[] dep, string assetName, bool iosModule, bool androidModule, Action<String[]> Complete)
        {
            List<string> packagesList = new List<string>();
            
            string state = String.Empty;
            
            AssetBundleBuild[]buildMap = new AssetBundleBuild[1];
            buildMap[0].assetNames = dep;
            
            
            if(!Directory.Exists(Script.AssetViewerUploaderTool.Config.AssetViewerConfig.ASSETBUNDLE_DIRECTORY))
            {
                Directory.CreateDirectory(Script.AssetViewerUploaderTool.Config.AssetViewerConfig.ASSETBUNDLE_DIRECTORY);
            }
            
            if (iosModule)
            {
               //Create iOS Build
                //iOS Build
                string buildName = String.Format(assetName + "_{0}", "ios");
                buildMap[0].assetBundleName = buildName;
                BuildPipeline.BuildAssetBundles(Script.AssetViewerUploaderTool.Config.AssetViewerConfig.ASSETBUNDLE_DIRECTORY, buildMap, BuildAssetBundleOptions.None, BuildTarget.iOS);

                packagesList.Add(Script.AssetViewerUploaderTool.Config.AssetViewerConfig.ASSETBUNDLE_DIRECTORY + "/" + buildName);
                state += "Asset Bundle iOS Build \n";
            }
            else
            {
                state += "iOS Module not Installed, <color=red>iOS BUILD FAILED</color> \n";
            }
            
            if (androidModule)
            {
                //Create Android Build
                //Android Build
                string buildName = String.Format(assetName + "_{0}", "android");
                buildMap[0].assetBundleName = buildName;
                BuildPipeline.BuildAssetBundles(Script.AssetViewerUploaderTool.Config.AssetViewerConfig.ASSETBUNDLE_DIRECTORY, buildMap, BuildAssetBundleOptions.None, BuildTarget.Android);
                packagesList.Add(Script.AssetViewerUploaderTool.Config.AssetViewerConfig.ASSETBUNDLE_DIRECTORY + "/" + buildName);
                state += "Asset Bundle Android Build \n";
            }
            else
            {
                state += "Android Module not Installed, <color=red>Android BUILD FAILED</color> \n";
            }

            Debug.Log(state);

            string[] package = packagesList.ToArray(); 
            Complete(package);

        }


        public static bool CheckForModules(BuildTarget buildType)
        {
            var moduleManager = System.Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
            var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
     
            bool checkModule = (bool)isPlatformSupportLoaded.Invoke(null,new object[] {(string)getTargetStringFromBuildTarget.Invoke(null, new object[] {buildType})});

            return checkModule;
        }
    }   
}

