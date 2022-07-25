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
    }
}
