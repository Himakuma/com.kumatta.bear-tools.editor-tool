using System.IO;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Kumatta.BearTools.Editor
{
    public class ScriptConversion : AssetModificationProcessor
    {

        #region Auto Convert Setting

        const string MENU_AUTO_UTF8_PATH = "Edit/BearTools/Script Auto/UTF-8";
        const string MENU_AUTO_UTF8_KEY = "com.kumatta.bear-tools.editor-tool.script-auto-utf-8";


        const string MENU_AUTO_LF_PATH = "Edit/BearTools/Script Auto/Line Char LF";
        const string MENU_AUTO_LF_KEY = "com.kumatta.bear-tools.editor-tool.script-auto-line-lf";

        const string MENU_AUTO_CRLF_PATH = "Edit/BearTools/Script Auto/Line Char CRLF";
        const string MENU_AUTO_CRLF_KEY = "com.kumatta.bear-tools.editor-tool.script-auto-line-cr;f";


        private static void OnWillCreateAsset(string assetName)
        {
            if (assetName.EndsWith(".cs.meta"))
            {
                string lineChar = GetLineChar();
                if (EditorPrefs.GetBool(MENU_AUTO_UTF8_KEY, false))
                {
                    Task.Run(() =>
                    {
                        string filePath = GetCSFilePath(assetName);
                        ScriptAutoUtf8(filePath, lineChar);
                    });

                }
                else if (!string.IsNullOrEmpty(lineChar))
                {
                    Task.Run(() =>
                    {
                        string filePath = GetCSFilePath(assetName);
                        string text = LineCharConvert(File.ReadAllText(filePath), lineChar);
                        File.WriteAllText(filePath, text);
                    });
                }
            }
        }


        private static string GetLineChar()
        {
            if (EditorPrefs.GetBool(MENU_AUTO_LF_KEY, false))
            {
                return "\n";
            }
            else if (EditorPrefs.GetBool(MENU_AUTO_CRLF_KEY, false))
            {
                return "\r\n";
            }
            return string.Empty;
        }




        [MenuItem(MENU_AUTO_UTF8_PATH, priority = 20)]
        public static void MenuScriptAutoUtf8()
        {
            bool menuChecked = !EditorPrefs.GetBool(MENU_AUTO_UTF8_KEY, false);
            EditorPrefs.SetBool(MENU_AUTO_UTF8_KEY, menuChecked);
            Menu.SetChecked(MENU_AUTO_UTF8_PATH, menuChecked);
        }

        [MenuItem(MENU_AUTO_UTF8_PATH, true)]
        public static bool MenuCheckScriptAutoUtf8()
        {
            Menu.SetChecked(MENU_AUTO_UTF8_PATH, EditorPrefs.GetBool(MENU_AUTO_UTF8_KEY, false));
            return true;
        }



        [MenuItem(MENU_AUTO_LF_PATH, priority = 31)]
        public static void MenuScriptAutoLF()
        {
            bool menuChecked = !EditorPrefs.GetBool(MENU_AUTO_LF_KEY, false);
            EditorPrefs.SetBool(MENU_AUTO_LF_KEY, menuChecked);
            Menu.SetChecked(MENU_AUTO_LF_PATH, menuChecked);

            if (menuChecked)
            {
                EditorPrefs.SetBool(MENU_AUTO_CRLF_KEY, false);
                Menu.SetChecked(MENU_AUTO_CRLF_PATH, false);
            }
        }

        [MenuItem(MENU_AUTO_LF_PATH, true)]
        public static bool MenuCheckScriptAutoLF()
        {
            Menu.SetChecked(MENU_AUTO_LF_PATH, EditorPrefs.GetBool(MENU_AUTO_LF_KEY, false));
            return true;
        }


        [MenuItem(MENU_AUTO_CRLF_PATH, priority = 32)]
        public static void MenuScriptAutoCRLF()
        {
            bool menuChecked = !EditorPrefs.GetBool(MENU_AUTO_CRLF_KEY, false);
            EditorPrefs.SetBool(MENU_AUTO_CRLF_KEY, menuChecked);
            Menu.SetChecked(MENU_AUTO_CRLF_PATH, menuChecked);

            if (menuChecked)
            {
                EditorPrefs.SetBool(MENU_AUTO_LF_KEY, false);
                Menu.SetChecked(MENU_AUTO_LF_PATH, false);
            }
        }


        [MenuItem(MENU_AUTO_CRLF_PATH, true)]
        public static bool MenuCheckScriptAutoCRLF()
        {
            Menu.SetChecked(MENU_AUTO_CRLF_PATH, EditorPrefs.GetBool(MENU_AUTO_CRLF_KEY, false));
            return true;
        }


        private static string GetCSFilePath(string assetName)
        {
            return Path.GetFullPath(assetName.Substring(0, assetName.Length - 5));
        }



        private static void ScriptAutoUtf8(string filePath, string lineChar)
        {
            // BOMがあるので、UTF-8
            byte[] fileData = File.ReadAllBytes(filePath);
            string utf8Text = "";
            byte[] bom = null;
            if ((fileData[0] == 0xef) && (fileData[1] == 0xbb) && (fileData[2] == 0xbf))
            {
                utf8Text = Encoding.UTF8.GetString(fileData);
            }
            else
            {
                bom = Encoding.UTF8.GetPreamble();
                var encodingSJIS = Encoding.GetEncoding("Shift_JIS");
                string sjisText = encodingSJIS.GetString(fileData);
                utf8Text = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(sjisText));
            }

            if (!string.IsNullOrEmpty(lineChar))
            {
                utf8Text = LineCharConvert(utf8Text, lineChar);
            }

            byte[] utf8Bytes = Encoding.UTF8.GetBytes(utf8Text);
            if (bom == null)
            {
                File.WriteAllBytes(filePath, utf8Bytes);
            }
            else
            {
                File.WriteAllBytes(filePath, bom.Concat(utf8Bytes).ToArray());
            }

        }


        private static string LineCharConvert(string text, string lineChar)
        {
            return text.Replace("\r\n", "\n").Replace("\n", lineChar);
        }



        #endregion



        [MenuItem("Assets/Apply Auto Script Convert", priority = -100)]
        public static void MenuApplyAutoScriptConvert()
        {
            var selectAssetPaths = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets).Select(asset => AssetDatabase.GetAssetPath(asset));

            var fileList = new List<string>();
            foreach (string path in selectAssetPaths)
            {
                string fullpath = Path.GetFullPath(path);
                if (Directory.Exists(path))
                {
                    fileList.AddRange(Directory.EnumerateFiles(fullpath, "*.cs", SearchOption.AllDirectories));
                }
                else
                {
                    fileList.Add(fullpath);
                }
            }

            string lineChar = GetLineChar();
            if (EditorPrefs.GetBool(MENU_AUTO_UTF8_KEY, false))
            {
                Task.Run(() =>
                {
                    foreach (string scriptFullPath in fileList)
                    {
                        ScriptAutoUtf8(scriptFullPath, lineChar);
                    }
                });
            }
            else if (!string.IsNullOrEmpty(lineChar))
            {
                Task.Run(() =>
                {
                    foreach (string scriptFullPath in fileList)
                    {
                        string text = LineCharConvert(File.ReadAllText(scriptFullPath), lineChar);
                        File.WriteAllText(scriptFullPath, text);
                    }
                });
            }



        }

        [MenuItem("Assets/Apply Auto Script Convert", validate = true)]
        public static bool MenuValidApplyAutoScriptConvert()
        {
            var selectAssetPaths = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets).Select(asset => AssetDatabase.GetAssetPath(asset));
            if (selectAssetPaths.Count() == 0)
            {
                return false;
            }
            return true;
        }


    }
}