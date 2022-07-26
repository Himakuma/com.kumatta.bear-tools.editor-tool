using System.IO;
using UnityEngine;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Kumatta.BearTools.Editor
{

    public static class FileUtil
    {
        /// <summary>
        /// アセットディレクトリ配下のファイルを検索
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        public static string[] FindAssetDirectory(string searchPattern)
        {
            return Directory.EnumerateFiles(Application.dataPath, searchPattern, SearchOption.AllDirectories).Where(w => !w.EndsWith("meta")).ToArray();
        }

        /// <summary>
        /// パッケージディレクトリ配下のファイルを検索
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        public static string[] FindPackagesDirectory(string packageName, string searchPattern)
        {
            return Directory.EnumerateFiles(GetPackagePath(packageName), searchPattern, SearchOption.AllDirectories).Where(w => !w.EndsWith("meta")).ToArray();
        }

        /// <summary>
        /// パッケージディレクトリ配下のフルパスを取得
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static string GetPackagePath(string packageName, string filePath = "")
        {
            return Path.GetFullPath(Path.Combine($"Packages/{packageName}", filePath));
        }

        /// <summary>
        /// アセットディレクトリまたは、パッケージに存在するファイルのフルパスを取得
        /// ※パッケージにファイルが存在しない場合、アセットのパスを返す
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileFullPath(string packageName, string filePath)
        {
            string fileFullPath = GetPackagePath(packageName, filePath);
            if (File.Exists(fileFullPath))
            {
                return fileFullPath;
            }
            return Path.Combine(Application.dataPath, filePath);
        }

        #region Text File

        /// <summary>
        /// JSONをオブジェクトに変換
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T JsonToObject<T>(string filePath)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(filePath))))
            {
                return (T)serializer.ReadObject(stream);
            }
        }

        /// <summary>
        /// ファイルヘッダにBOMがある場合、true
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool BomExists(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] headBytes = new byte[3];
                if (headBytes.Length == fs.Read(headBytes, 0, 3))
                {
                    if (headBytes[0] == 0xef && headBytes[1] == 0xbb && headBytes[2] == 0xbf)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// ファイルのテキストをSJISで取得
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetStringSjis(string filePath)
        {
            var encoding = Encoding.GetEncoding("Shift_JIS");
            return File.ReadAllText(filePath, encoding);
        }

        #endregion
    }
}
