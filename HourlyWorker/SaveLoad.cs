using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HourlyWorker
{

    /// <summary>
    /// This class handles saving ang loading.
    /// </summary>
    public static class SaveLoad
    {

        /// <summary>
        /// This class contains all needed data for the saving.
        /// </summary>
        [Serializable]
        public class SaveData
        {

            /// <summary>
            /// Current model version.
            /// </summary>
            [NonSerialized]
            public const int GlobalVersion = 2;

            /// <summary>
            /// Save data model version.
            /// </summary>
            public int Version { get; set; }

            /// <summary>
            /// <see cref="MainWindow.NightMode"/>
            /// </summary>
            public bool NightMode { get; set; }

            public bool LookForCursor { get; set; }

            /// <summary>
            /// <see cref="MainWindow.ShowNotifications"/>
            /// </summary>
            public bool ShowNotifications { get; set; }
            /// <summary>
            /// <see cref="MainWindow.WorkProjects"/>
            /// </summary>
            public List<WorkProject> WorkProjects { get; set; }

        }

        /// <summary>
        /// Directory to the save file.
        /// </summary>
        private readonly static string DirPath = "save" + Path.DirectorySeparatorChar;
        /// <summary>
        /// Full path of the save file.
        /// </summary>
        private readonly static string FilePath = DirPath + "data.json";

        /// <summary>
        /// Check if the <see cref="DirPath"/> is exists. Will create if not.
        /// </summary>
        private static void CheckDirectory()
        {
            if (!Directory.Exists(DirPath))
                Directory.CreateDirectory(DirPath);
        }

        /// <summary>
        /// Load saved data.
        /// </summary>
        public static SaveData Load()
        {
            CheckDirectory();

            if (!File.Exists(FilePath))
                return null;

            string input = File.ReadAllText(FilePath);
            SaveData data = JsonSerializer.Deserialize<SaveData>(input);
            if (data.Version != SaveData.GlobalVersion)
            {
                try
                {
                    data.Version = SaveData.GlobalVersion;
                    //v1
                    data.NightMode = false;
                    //v2
                    data.LookForCursor = true;
                }
                catch(Exception e)
                {
                    MessageBox.Show(string.Format("Failed to resolve save data version conflict. Error: {0}", e.Message));
                }
            }

            return data;
        }

        /// <summary>
        /// Save application data.
        /// </summary>
        public static void Save(List<WorkProject> workProjects, bool showNotifications, bool lookForCursor)
        {
            CheckDirectory();

            SaveData saveData = new SaveData
            {
                Version = SaveData.GlobalVersion,
                ShowNotifications = showNotifications,
                WorkProjects = workProjects,
                LookForCursor = lookForCursor
            };

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string output = JsonSerializer.Serialize(saveData, options);
            File.WriteAllText(FilePath, output);
        }

    }
}
