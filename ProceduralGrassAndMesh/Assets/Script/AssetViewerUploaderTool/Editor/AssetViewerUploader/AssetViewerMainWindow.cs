using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Script.Editor.AssetViewerUploader
{


    public struct Users
    {
        public string[] userName;
    }

    public struct Projects
    {
        public string[] projectName;
    }
    
    public class AssetViewerMainWindow : EditorWindow
    {
        //Local Test
        public TextAsset usersData;
        public TextAsset projectData;
        
        private  Script.AssetViewerUploaderTool.Config.AssetViewerCategories.CATEGORY _categories;
        
        //Check Internet Connection
        private bool _pingSuccess = false;
        private bool _pingError = false;
        
        //User List
        private bool _userListLoaded = false;
        private Users _userList;
        private int _userIndex;
        
        //Project List
        private bool _projectListLoaded = false;
        private Projects _projectList;
        private int _projectIndex;
        
        //Asset Info
        private string _assetPath;
        private string _assetName;

        //UI
        private static Texture2D _bgImg;
        private static Texture2D _bgImgWhite;
        
        //Build
        private bool _startBuildAndUpload = false;
        private bool _androidModule = false;
        private bool _iosModule = false;
        private bool _createBuild = false;
        private bool _uploadBundle = false;
        private bool _uploadBundleComplete = false;

        public void Init(string path, string name)
        {
            this.maxSize = new Vector2(400f, 630f);
            this.minSize = this.maxSize;
            
            //Create BG Textures
            GenerateTexture();
            
            //Set Asset Path and Name
            SetAssetPath(path, name);
            
            //Ping Server
            PingServer(Script.AssetViewerUploaderTool.Config.AssetViewerConfig.SERVER_URL, InjectData);

            //Start Windows
            Show();
        }

        private void InjectData()
        {
            //Load Info from Server
            InjectDataProject();
            InjectDataUser();
        }
        
        private void GenerateTexture()
        {
            _bgImg = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            Color32 bgColor = new Color32(84, 142, 166, 255);
            _bgImg.SetPixel(0, 0, bgColor);
            _bgImg.Apply();
            
            _bgImgWhite = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            bgColor = Color.white;
            _bgImgWhite.SetPixel(0, 0, bgColor);
            _bgImgWhite.Apply();
        }

        private GUIStyle TextStyle(TextAnchor anchor,int fontSize, Color color, float height, RectOffset padding)
        {
            GUIStyle centerText = new GUIStyle();
            centerText.alignment = anchor;
            centerText.fontSize = fontSize;
            centerText.normal.textColor = color;
            centerText.fixedHeight = height;
            centerText.padding = padding;
            return centerText;
        }

        private void OnGUI()
        {
            if (!_pingSuccess || !_projectListLoaded || !_userListLoaded)
            {
                ShowInternetScreen();
                return;
            }

            //Background
            float topBar = 90;
            GUI.DrawTexture(new Rect(0, 0, maxSize.x, topBar), _bgImg, ScaleMode.StretchToFill);
            float middleBar = 200;
            GUI.DrawTexture(new Rect(0, topBar, maxSize.x, middleBar), _bgImgWhite, ScaleMode.StretchToFill);
            float endBar = 400;
            GUI.DrawTexture(new Rect(0, middleBar + topBar, maxSize.x, endBar), _bgImg, ScaleMode.StretchToFill);
            //
            
            //Labels
            EditorGUILayout.BeginVertical(GUILayout.Height(topBar));
            EditorGUILayout.LabelField("Upload Asset", TextStyle(TextAnchor.MiddleCenter, 28, Color.white, 90, new RectOffset(0,0,0,0)));
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(GUILayout.Height(middleBar));
            //Project
            EditorGUILayout.LabelField("Project", TextStyle(TextAnchor.LowerLeft, 14, Color.black, 0, new RectOffset(0,0,0,0)));
            _projectIndex = EditorGUILayout.Popup(_projectIndex, _projectList.projectName);
            //Category
            EditorGUILayout.LabelField("Asset Category", TextStyle(TextAnchor.LowerLeft, 14, Color.black, 0, new RectOffset(0,0,0,0)));
            _categories = (Script.AssetViewerUploaderTool.Config.AssetViewerCategories.CATEGORY)EditorGUILayout.EnumPopup(_categories);
            //User
            EditorGUILayout.LabelField("User", TextStyle(TextAnchor.LowerLeft, 14, Color.black, 0, new RectOffset(0,0,0,0)));
            _userIndex = EditorGUILayout.Popup(_userIndex, _userList.userName);
            EditorGUILayout.EndVertical();

            //Upload Button
            Texture2D uploadButton = LoadTexture("Upload");
            Vector2 uploadButtonSize = new Vector2(128,128);
            Rect uploadRect = new Rect((maxSize.x /2f) - (uploadButtonSize.x/2f), (topBar + middleBar ) - (uploadButtonSize.y/2f), uploadButtonSize.x, uploadButtonSize.y);

            if (uploadRect.Contains(Event.current.mousePosition))
            {
                uploadButton = LoadTexture("UploadUP");
            }
            else
            {
                uploadButton = LoadTexture("Upload");
            }
            
            GUI.DrawTexture(uploadRect, uploadButton);

            if (!_startBuildAndUpload)
            {
                if (Event.current.type == EventType.MouseUp && uploadRect.Contains(Event.current.mousePosition))
                {
                    StartBuildProcess();
                }
            }

            //Draw Flow Icons
            EditorGUILayout.Space(80);
            DrawFlowLabels("ANDROIDICON", "Android Module" ,40, 47,47, _androidModule);
            DrawFlowLabels("IOSICON", "iOS Module", 40, 47,47,_iosModule);
            DrawFlowLabels("ABICON", "Asset Bundle Start", 40, 47,47, _createBuild);
            DrawFlowLabels("UPLOADICON", "Upload Bundle",  40, 47,47, _uploadBundle);
 
            
            //Check Upload Status
            if (_uploadBundle && !_uploadBundleComplete)
            { 
                //Upload Screen
                UploadingScreen(topBar);
                
                if (AssetViewerUploadBundle.GetUploadStatus())
                {
                    _uploadBundleComplete = true;
                    //Delete Asset Bundles
                }
            }
        }

        private void ShowInternetScreen()
        {
             //Create BG
            Rect infoR = new Rect(0, 0, maxSize.x, maxSize.y);
            GUI.DrawTexture(infoR, _bgImgWhite);
            Vector2 internetImgSize = new Vector2(165, 154);
            Texture2D internetImg;
            Rect internetRect;
            
            if (!_pingError)
            {
                internetImg = LoadTexture("Internet");
            }
            else
            {
                internetImg = LoadTexture("NoInternet");
            }
            
            internetRect = new Rect((maxSize.x/2f) - (internetImgSize.x/2f), (maxSize.y/2f) - (internetImgSize.y/2f), internetImgSize.x, internetImgSize.y);
            GUI.DrawTexture(internetRect, internetImg);
            
            Rect labelRect = new Rect(0,  (maxSize.y/2f) + (internetImgSize.y/2f), maxSize.x, 90);
            EditorGUI.LabelField(labelRect, "Checking Internet", TextStyle(TextAnchor.MiddleCenter, 24, Color.black, 90, new RectOffset(0,0,0,0)));
        }

        private void UploadingScreen(float yOffset)
        {
            float infoH = 280;
            Rect infoR = new Rect(0, yOffset, maxSize.x, infoH);
            GUI.DrawTexture(infoR, _bgImgWhite);
            float uploadPerc = AssetViewerUploadBundle.GetUploadProgress() * 100f;
            EditorGUI.LabelField(infoR, "Uploading: " + uploadPerc + "%", TextStyle(TextAnchor.MiddleCenter, 32, Color.black, infoH, new RectOffset(0,0,0,0)));
        }
        
        private void StartBuildProcess()
        {
            _startBuildAndUpload = true;
            _androidModule = AssetViewerCreateBundle.CheckForModules(BuildTarget.Android);
            _iosModule = AssetViewerCreateBundle.CheckForModules(BuildTarget.iOS);

            if (!_androidModule && !_iosModule)
            {
                _createBuild = false;
            }
            else
            {
                _createBuild = true;
            }
            
            AssetViewerCreateBundle.BuildGranularAssetBundle(AssetViewerCreateBundle.CollectAssetDependencies(_assetPath), _assetName, _iosModule, _androidModule,  UploadBundle);
        }

        private void UploadBundle(string[]packages)
        {
            AssetViewerUploadBundle.UploadDataInfo dataInfo;
            dataInfo.project = _projectList.projectName[_projectIndex];
            dataInfo.category = _categories.ToString();
            dataInfo.userName = _userList.userName[_userIndex];
            dataInfo.mainAsset = _assetName;
            dataInfo.unityVersion = Common.GetUnityCurrentVersion();
            
            _uploadBundle = true;
            AssetViewerUploadBundle.SendPackageToServer(packages, dataInfo);
        }

        private void DrawFlowLabels(string iconName, string label, int leftOffset, int iconW, int iconH, bool success)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(label, TextStyle(TextAnchor.MiddleLeft, 18, Color.white, iconH, new RectOffset(100,0,0,0)));
            EditorGUILayout.EndVertical();
            Rect lastRect = GUILayoutUtility.GetLastRect();
            DrawIcon(iconName, leftOffset, lastRect.y, iconW, iconH);
            DrawIcon(success ? "success" : "error", 290, lastRect.y + 10, 65, 30);
            
            EditorGUILayout.Space(35);
        }

        private void DrawIcon(string name, float posx, float posy, int w, int h)
        {
            Texture2D texture = LoadTexture(name);
            Vector2 textureSize = new Vector2(w, h);
            Rect textureRect = new Rect(posx,posy,w,h);
            GUI.DrawTexture(textureRect, texture);
        }

        private void Update()
        {
            Repaint();
        }
        
        public void SetAssetPath(string path, string name)
        {
            _assetPath = path;
            _assetName = name;
        }

        //Move to Other Class
        private void PingServer(string url, Action complete)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.SendWebRequest();

            EditorApplication.CallbackFunction update = null;
            update = () =>
            {
                if (www.isDone)
                {
                    if (www.isHttpError || www.isNetworkError || !string.IsNullOrEmpty(www.error))
                    {
                        _pingSuccess = false;
                        _pingError = true;
                    }
                    else
                    {
                        _pingSuccess = true;
                        _pingError = false;
                        complete();
                    }
                    
                    EditorApplication.update -= update;
                    www.Dispose();
                }
            };
            
            EditorApplication.update += update;
        }
        
        private void LoadJsonTextAsset<T>(ref T value, String jsonData, ref bool boolValue)
        {
            value = JsonUtility.FromJson<T>(jsonData);
            boolValue = true;
        }

        private void InjectDataProject()
        {
            LoadDataFromServer(Script.AssetViewerUploaderTool.Config.AssetViewerConfig.SERVER_PROJECT, (string json) =>
            {
                LoadJsonTextAsset(ref _projectList, json, ref _projectListLoaded);
            });
        }
        
        private void InjectDataUser()
        {
            LoadDataFromServer(Script.AssetViewerUploaderTool.Config.AssetViewerConfig.SERVER_USER, (string json) =>
            {
                LoadJsonTextAsset(ref _userList, json, ref _userListLoaded);
            });
        }

        private void LoadDataFromServer(string url, Action<String> Complete)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.SendWebRequest();

            EditorApplication.CallbackFunction update = null;
            update = () =>
            {
                if (www.isDone)
                {
                    if (www.isHttpError || www.isNetworkError || !string.IsNullOrEmpty(www.error)) {
                        Debug.Log("Error");
                    }
                    else
                    {
                        string json = www.downloadHandler.text;
                        Complete(json);
                    }
                    
                    EditorApplication.update -= update;
                    www.Dispose();
                }
            };
            
            EditorApplication.update += update;
        }


        private static Texture2D LoadTexture(string texture)
        {
            string path = Script.AssetViewerUploaderTool.Config.AssetViewerConfig.TEXTURES_PATH + texture + ".png";
            
            
            Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            return image;
        }
    }
}
