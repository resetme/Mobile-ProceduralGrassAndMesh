using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.AssetViewerUploaderTool.Config
{
    public static class AssetViewerConfig
    {
        public const string ASSETBUNDLE_DIRECTORY = "Assets/AssetBundleViewer/Bundles";
        public const string INSTALL_FOLDER = "Assets/Script/AssetViewerUploaderTool/Editor/AssetViewerUploader";
        public const string TEXTURES_PATH = INSTALL_FOLDER + "/Images/";
        public const string SERVER_URL = "http://franfndz.com/AssetBundle/VFXViewer/uploadPackage.php";
        public const string SERVER_BASE = "http://franfndz.com/AssetBundle/AssetViewer/";
        public const string SERVER_PROJECT = SERVER_BASE + "Projects/projectList.php";
        public const string SERVER_UPLOAD = SERVER_BASE + "Projects/uploadPackage.php";
        public const string SERVER_USER = SERVER_BASE + "Users/UsersList.json";
        public const string SERVER_PACKAGELIST = SERVER_BASE + "Projects/packageList.php";
    } 
}

