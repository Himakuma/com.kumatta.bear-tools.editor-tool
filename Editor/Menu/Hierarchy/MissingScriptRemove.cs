using log4net.Util;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Transform = UnityEngine.Transform;

namespace Kumatta.BearTools.Editor
{

    public static class MissingScriptRemove
    {

        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod_MissingScriptRemove()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnLoadMethod_MissingScriptRemove;
        }

        private static void OnLoadMethod_MissingScriptRemove(int instanceID, Rect rect)
        {
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null)
            {
                return;
            }

            var missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            if (0 < missingCount)
            {
                var color = new Color(1f, 0f, 0f, 0.2f);
                EditorGUI.DrawRect(rect, color);
            }
        }





        [MenuItem("GameObject/Script/Missing Remove", priority = 2000)]
        private static void ScriptMissingRemove()
        {
            var selectObjects = Selection.gameObjects;
            foreach (var gameObject in selectObjects)
            {
                AllMissingMissingScriptRemove(gameObject);
            }
        }


        [MenuItem("GameObject/Script/Missing Remove", validate = true)]
        private static bool MenuValid_ScriptMissingRemove()
        {
            if (Selection.gameObjects.Length <= 0) return false;

            int missingCount = 0;
            foreach (var gameObject in Selection.gameObjects)
            {
                missingCount += AllMissingMissingScriptCount(gameObject);
            }
            return 0 < missingCount;
        }



        [MenuItem("GameObject/Script/Select Missing", priority = 2000)]
        private static void SelectMissingScript()
        {
            var gameObjects = new List<GameObject>();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++) 
            {
                var scenes = EditorSceneManager.GetSceneAt(i);
                foreach (var gameObject in scenes.GetRootGameObjects()) 
                {
                    var missingObjects = GetGameObjectsMissingScript(gameObject);
                    gameObjects.AddRange(missingObjects);
                }
            }
            Selection.objects = gameObjects.ToArray();
        }


        [MenuItem("GameObject/Script/Select Missing", validate = true)]
        private static bool MenuValid_SelectMissingScript()
        {
            return Selection.gameObjects.Length <= 0;
        }


        private static int AllMissingMissingScriptCount(GameObject root)
        {
            int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root);
            foreach (Transform child in root.transform)
            {
                missingCount += AllMissingMissingScriptCount(child.gameObject);
            }
            return missingCount;
        }


        private static void AllMissingMissingScriptRemove(GameObject root)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
            foreach (Transform child in root.transform)
            {
                AllMissingMissingScriptRemove(child.gameObject);
            }
        }




        private static List<GameObject> GetGameObjectsMissingScript(GameObject root) 
        {
            var gameObjects = new List<GameObject>();
            if (0 < GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root)) 
            {
                gameObjects.Add(root);
            }

            foreach (Transform child in root.transform) 
            {
                var missingObjects = GetGameObjectsMissingScript(child.gameObject);
                gameObjects.AddRange(missingObjects);
            }

            return gameObjects;

        }
    }

}
