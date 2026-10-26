using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniGM
{
    /// <summary>
    /// 编辑器下把 GM 命令历史记在 <see cref="Application.persistentDataPath"/> 的二进制文件里。
    /// </summary>
    internal static class GmCommandHistoryFile
    {
        internal const int Capacity = 50;

        private const int Version = 1;

        public static void Read(List<string> history)
        {
            if (!Application.isEditor || history == null)
                return;

            try
            {
                string path = FilePath();
                if (!File.Exists(path))
                    return;

                using (FileStream stream = File.OpenRead(path))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    if (reader.ReadInt32() != Version)
                        return;

                    int count = reader.ReadInt32();
                    if (count < 0 || count > Capacity)
                        return;

                    history.Clear();
                    for (int i = 0; i < count; i++)
                    {
                        string line = reader.ReadString();
                        if (!string.IsNullOrWhiteSpace(line))
                            history.Add(line);
                    }
                }
            }
            catch (Exception exception)
            {
                history.Clear();
                Debug.LogWarning("GM 命令历史读取失败：" + exception.Message);
            }
        }

        public static void Write(List<string> history)
        {
            if (!Application.isEditor || history == null)
                return;

            try
            {
                string path = FilePath();
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                string temp = path + ".tmp";
                using (FileStream stream = File.Create(temp))
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Version);
                    int count = history.Count;
                    if (count > Capacity)
                        count = Capacity;
                    int start = history.Count - count;
                    writer.Write(count);
                    for (int i = start; i < history.Count; i++)
                        writer.Write(history[i] ?? string.Empty);
                }

                if (File.Exists(path))
                    File.Replace(temp, path, null);
                else
                    File.Move(temp, path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("GM 命令历史写入失败：" + exception.Message);
            }
        }

        private static string FilePath()
        {
            return Path.Combine(Application.persistentDataPath, "UniGM", "command-history.bin");
        }
    }
}
