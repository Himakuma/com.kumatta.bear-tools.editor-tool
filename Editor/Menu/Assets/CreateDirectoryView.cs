using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.IO;
using static Kumatta.BearTools.Editor.DirectoryDefinition;

namespace Kumatta.BearTools.Editor
{
    public class CreateDirectoryMenu
    {
        const string PACKAGE_NAME = "com.kumatta.bear-tools.editor-tool";

        [InitializeOnLoadMethod]
        private static void ProjectWindow()
        {
            EditorApplication.projectChanged += OnProjectWindowChanged;
        }

        /// <summary>
        /// メニュー用のスクリプトを生成（標準で、既存コンテキストメニューに動的に追加する方法が無い）
        /// </summary>
        private static void OnProjectWindowChanged()
        {
            string[] assetFiles = FileUtil.FindAssetDirectory("*-DirectoryDefinition.json");
            string[] fileFullPaths = FileUtil.FindPackagesDirectory(PACKAGE_NAME, "*-DirectoryDefinition.json");

            GenerationMenuScript(assetFiles.Concat(fileFullPaths).ToArray());
        }



        /// <summary>
        /// JSON定義単位にメニュー作成する為に、メニュー用のクラスを作成
        /// </summary>
        /// <param name="fileFullPaths"></param>
        private static void GenerationMenuScript(string[] fileFullPaths)
        {
            string classTemp = @"
using System.Linq;
using System.IO;
using UnityEditor;
using Kumatta.BearTools.Editor;


namespace Kumatta.BearTools
{
    public static class GenerationMenu
    {
        @@METHODS@@
    }
}
";

            string methodTemp = @"

        [MenuItem(""Assets/Create/Directory Template/@@MenuName@@"", false, 0)]
        public static void GenerationMenu@@MethodNumber@@()
        {
            CreateDirectoryMenu.Create(""@@TempFileFullPath@@"", ""@@MenuName@@"");
        }

";

            string validMethodTemp = @"

        [MenuItem(""Assets/Create/Directory Template/@@MenuName@@"", validate = true)]
        public static bool IsValid_@@MethodNumber@@()
        {
            var selectAssetPaths = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets).Select(asset => AssetDatabase.GetAssetPath(asset));
            var selectDirs = selectAssetPaths.Select(assetPath => AssetDatabase.IsValidFolder(assetPath) ? assetPath : Path.GetDirectoryName(assetPath));
            if (selectDirs.Count() == 0)
            {
                return false;
            }
            return true;
        }
";

            int methodNumber = 0;
            string methodsText = "";
            foreach (string fileFullPath in fileFullPaths)
            {
                foreach (var directoryDefinition in FileUtil.JsonToObject<DirectoryDefinition[]>(fileFullPath))
                {
                    methodNumber++;
                    string menuName = directoryDefinition.Name;
                    methodsText += methodTemp.Replace("@@MenuName@@", menuName).Replace("@@MethodNumber@@", methodNumber.ToString()).Replace("@@TempFileFullPath@@", fileFullPath.Replace("\\", "/"));

                    if (directoryDefinition.RootDirectory == "[Current-Directory]")
                    {
                        methodsText += validMethodTemp.Replace("@@MenuName@@", menuName).Replace("@@MethodNumber@@", methodNumber.ToString());
                    }
                }
            }

            string classText = classTemp.Replace("@@METHODS@@", methodsText);

            string generationDirPath = Path.Combine(Application.dataPath, $"Editor/{PACKAGE_NAME}/Generation");
            Directory.CreateDirectory(generationDirPath);

            string generationMenuClassFilePath = Path.Combine(generationDirPath, "GenerationMenu.cs");
            File.WriteAllText(generationMenuClassFilePath, classText);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 自動生成したスクリプトから呼ばれる、共通
        /// </summary>
        /// <param name="templateFileFullPath"></param>
        /// <param name="name"></param>
        public static void Create(string templateFileFullPath, string name)
        {
            foreach (var directoryDefinition in FileUtil.JsonToObject<DirectoryDefinition[]>(templateFileFullPath))
            {
                if (directoryDefinition.Name == name)
                {
                    var inputView = EditorWindow.GetWindow<CreateDirectoryView>(true, "入力してください", true);
                    inputView.CreateInputField(directoryDefinition);
                    return;
                }
            }
        }




        public class CreateDirectoryView : EditorWindow
        {
            private DirectoryDefinition directoryDefinition;

            private VisualElement mainView;

            public Dictionary<string, CusutomTextField> inputFields = new Dictionary<string, CusutomTextField>();

            public Button okButton;


            public void CreateGUI()
            {
                var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Packages/{PACKAGE_NAME}/Editor/Resources/View/CreateDirectoryView.uxml");
                mainView = visualTree.Instantiate();
                rootVisualElement.Add(mainView);

                okButton = mainView.FindChildByName<Button>("Create");
                okButton.clicked += OnClick_Create;
                okButton.SetEnabled(false);


                var cancelButton = mainView.FindChildByName<Button>("Cancel");
                cancelButton.clicked += () =>
                {
                    Close();
                };
            }



            private void OnGUI()
            {
                mainView.style.width = rootVisualElement.contentRect.width;
                mainView.style.minWidth = rootVisualElement.contentRect.width;
                mainView.style.maxWidth = rootVisualElement.contentRect.width;

                mainView.style.height = rootVisualElement.contentRect.height;
                mainView.style.minHeight = rootVisualElement.contentRect.height;
                mainView.style.maxHeight = rootVisualElement.contentRect.height;

                foreach (var input in inputFields.Values)
                {
                    if (!input.IsValid())
                    {
                        okButton.SetEnabled(false);
                        return;
                    }
                }
                okButton.SetEnabled(true);
            }

            private void OnClick_Create()
            {
                CreateDirectoryDefinition();
                Close();
            }

            /// <summary>
            /// 入力項目の作成
            /// </summary>
            /// <param name="directoryDefinition"></param>
            public void CreateInputField(DirectoryDefinition directoryDefinition)
            {
                this.directoryDefinition = directoryDefinition;

                var inputAreaScroll = mainView.FindChildByName<ScrollView>("InputAreaScroll");
                inputFields = new Dictionary<string, CusutomTextField>();

                if (directoryDefinition.ReplaceInputs == null || directoryDefinition.ReplaceInputs.Length == 0)
                {
                    OnClick_Create();
                    return;
                }

                foreach (var inputInfo in directoryDefinition.ReplaceInputs)
                {
                    var testFiled = new CusutomTextField(inputInfo.Label);
                    testFiled.AddToClassList("input-field");
                    testFiled.Required = inputInfo.Required;

                    Enum.TryParse(inputInfo.InputTypeStr, true, out CusutomTextField.TextType result);
                    testFiled.FieldType = result;
                    testFiled.Check();

                    inputAreaScroll.contentViewport.Add(testFiled);
                    inputFields[inputInfo.Label] = testFiled;
                }

            }

            /// <summary>
            /// 文字列の置換
            /// </summary>
            /// <param name="val"></param>
            /// <returns></returns>
            private string InputReplaceAll(string val)
            {
                if (inputFields.Count == 0)
                {
                    return val;
                }

                string result = val;
                foreach (var replaceInputs in directoryDefinition.ReplaceInputs)
                {
                    string replaceText = inputFields[replaceInputs.Label].text;
                    result = result.Replace(replaceInputs.ReplaceString, replaceText);
                }
                return result;
            }

            /// <summary>
            /// ディレクトリ階層の作成
            /// </summary>
            public void CreateDirectoryDefinition()
            {
                string rootPath = Application.dataPath;
                if (!string.IsNullOrEmpty(directoryDefinition.RootDirectory))
                {
                    if (directoryDefinition.RootDirectory == "[Current-Directory]")
                    {
                        var selectAssetPaths = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets).Select(asset => AssetDatabase.GetAssetPath(asset));
                        var selectDirs = selectAssetPaths.Select(assetPath => AssetDatabase.IsValidFolder(assetPath) ? assetPath : Path.GetDirectoryName(assetPath));
                        if (selectDirs.Count() == 0)
                        {
                            EditorUtility.DisplayDialog("エラー", "フォルダを選択してください。", "OK");
                            return;
                        }
                        rootPath = Path.GetFullPath(selectDirs.First());
                    }
                    else
                    {
                        rootPath = Path.Combine(rootPath, directoryDefinition.RootDirectory);
                    }
                }

                foreach (var hirectoryDefinition in directoryDefinition.HirectoryDefinitions)
                {
                    CreateDirectory(rootPath, hirectoryDefinition);
                }
                AssetDatabase.Refresh();
            }

            /// <summary>
            /// ディレクトリの作成
            /// </summary>
            /// <param name="rootPath"></param>
            /// <param name="hierarchicalDefinition"></param>
            private void CreateDirectory(string rootPath, HierarchicalDefinition hierarchicalDefinition)
            {
                string fileName = InputReplaceAll(hierarchicalDefinition.Name);
                switch (hierarchicalDefinition.Type)
                {
                    case HierarchicalDefinitionType.Directory:
                        string workPath = Path.Combine(rootPath, fileName);
                        Directory.CreateDirectory(workPath);
                        if (hierarchicalDefinition.Children != null)
                        {
                            foreach (var child in hierarchicalDefinition.Children)
                            {
                                CreateDirectory(workPath, child);
                            }
                        }
                        break;

                    case HierarchicalDefinitionType.Asmdef:
                    case HierarchicalDefinitionType.Text:
                    case HierarchicalDefinitionType.Json:
                    case HierarchicalDefinitionType.Markdown:
                        string temFilePath = FileUtil.GetFileFullPath(PACKAGE_NAME, hierarchicalDefinition.TmpPath);
                        string fileText = string.IsNullOrEmpty(hierarchicalDefinition.TmpPath) ? "" : File.ReadAllText(temFilePath);
                        fileText = InputReplaceAll(fileText);
                        File.WriteAllText(Path.Combine(rootPath, fileName), fileText);
                        break;

                    case HierarchicalDefinitionType.Binary:
                        string temBinaryFilePath = FileUtil.GetFileFullPath(PACKAGE_NAME, hierarchicalDefinition.TmpPath);
                        File.WriteAllBytes(Path.Combine(rootPath, fileName), File.ReadAllBytes(temBinaryFilePath));
                        break;
                }

            }

        }





    }



}