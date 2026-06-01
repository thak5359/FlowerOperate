    using System.IO;                                                                                                    
    using System.Reflection;                                                                                            
    using UnityEditor;                                                                                                  
    using UnityEngine;                                                                                                  
    using UnityEngine.UIElements;                                                                                       
                                                                                                                        
    public class USSMissingAssetFinder : EditorWindow                                                                   
    {                                                                                                                   
        [MenuItem("Tools/USS Missing Asset Finder")]                                                                    
        public static void ShowWindow()                                                                                 
        {                                                                                                               
            GetWindow<USSMissingAssetFinder>("USS Missing Asset Finder");                                               
        }                                                                                                               
                                                                                                                        
        private void OnGUI()                                                                                            
        {                                                                                                               
            EditorGUILayout.HelpBox(                                                                                    
                "USS 스타일시트 내부의 에셋 참조(m_Assets) 중 MissingReferenceAsset(깨진 이미지/폰트 등) 상태인 항목을 찾아냅니다.",                                                                                                         
                MessageType.Info);                                                                                      
                                                                                                                        
            if (GUILayout.Button("USS 깨진 에셋 스캔", GUILayout.Height(40)))                                           
            {                                                                                                           
                ScanStyleSheets();                                                                                      
            }                                                                                                           
        }                                                                                                               
                                                                                                                        
        private static void ScanStyleSheets()                                                                           
        {                                                                                                               
            // 1. 프로젝트 내 모든 StyleSheet (.uss) 에셋 로드                                                          
            string[] guids = AssetDatabase.FindAssets("t:StyleSheet");                                                  
            int errorCount = 0;                                                                                         
                                                                                                                        
            Debug.Log("[USS Finder] 스타일시트 에셋 스캔을 시작합니다...");                                             
                                                                                                                        
            foreach (string guid in guids)                                                                              
            {                                                                                                           
                string path = AssetDatabase.GUIDToAssetPath(guid);                                                      
                StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);                                
                                                                                                                        
                if (styleSheet == null) continue;                                                                       
                                                                                                                        
                // 2. StyleSheet의 내부 직렬화 목록인 m_Assets 필드에 접근 (Reflection)                                 
                FieldInfo assetsField = typeof(StyleSheet).GetField("m_Assets", BindingFlags.NonPublic | BindingFlags.Instance)                                                                                                             
                                     ?? typeof(StyleSheet).GetField("assets", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);                                                                                      
                                                                                                                        
                if (assetsField != null)                                                                                
                {                                                                                                       
                    var assetsList = assetsField.GetValue(styleSheet) as System.Collections.IList;                      
                    if (assetsList != null)                                                                             
                    {                                                                                                   
                        for (int i = 0; i < assetsList.Count; i++)                                                      
                        {                                                                                               
                            Object obj = assetsList[i] as Object;                                                       
                            bool isMissing = false;                                                                     
                            string assetInfo = "Unknown Asset";

                        // Case A: 객체가 아예 로드되지 않아 null 상태이지만 인스턴스 ID 정보만 남아있는 경우(Missing)
                            if (obj == null)                                                                            
                            {                                                                                           
                                SerializedObject so = new SerializedObject(styleSheet);                                 
                                SerializedProperty assetsProp = so.FindProperty("m_Assets");                            
                                if (assetsProp != null && assetsProp.isArray && i < assetsProp.arraySize)               
                                {                                                                                       
                                    SerializedProperty element = assetsProp.GetArrayElementAtIndex(i);                  
                                    if (element.objectReferenceValue == null && element.objectReferenceInstanceIDValue != 0)                                                                                                                 
                                    {                                                                                   
                                        isMissing = true;                                                               
                                        assetInfo = $"[Serialized Instance ID: {element.objectReferenceInstanceIDValue}]";                                                                                    
                                    }                                                                                   
                                }                                                                                       
                            }                                                                                           
                            // Case B: Unity가 깨진 참조 대신 'MissingReferenceAsset' 타입의 임시 객체를 할당해둔 경우  
                            else if (obj.GetType().Name == "MissingReferenceAsset" || obj.name == "MissingReferenceAsset" || obj.ToString().Contains("MissingReferenceAsset"))                                          
                            {                                                                                           
                                isMissing = true;                                                                       
                                assetInfo = $"Type: {obj.GetType().Name} (Name: {obj.name})";                           
                            }                                                                                           
                                                                                                                        
                            if (isMissing)                                                                              
                            {                                                                                           
                                Debug.LogError(                                                                         
                                    $"<color=red><b>[USS Missing Asset]</b></color> 깨진 참조 발견!\n" +                
                                    $"<b>스타일시트 파일:</b> <a href=\"{path}\">{path}</a>\n" +                        
                                    $"<b>내부 에셋 인덱스:</b> [{i}]\n" +                                               
                                    $"<b>에셋 상세 정보:</b> {assetInfo}\n" +                                           
                                    $"<i>해당 스타일시트를 UI Builder로 열어 깨진 폰트/이미지 속성을 재지정하거나 USS 텍스트에서 유효하지 않은 url()을 제거해 주세요.</i>"                                                                  
                                );                                                                                      
                                errorCount++;                                                                           
                            }                                                                                           
                        }                                                                                               
                    }                                                                                                   
                }                                                                                                       
            }                                                                                                           
                                                                                                                        
            if (errorCount == 0)                                                                                        
            {                                                                                                           
                Debug.Log("<color=green>[USS Finder] 스캔 완료: 깨진 USS 에셋 참조가 없습니다!</color>");               
            }                                                                                                           
            else                                                                                                        
            {                                                                                                           
                Debug.LogWarning($"[USS Finder] 스캔 완료: 총 {errorCount}개의 깨진 USS 에셋 참조를 검출했습니다.콘솔창의 에러 로그를 확인해 주세요.");                                                                                
            }                                                                                                           
        }                                                                                                               
    }                            