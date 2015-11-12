using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jil;

namespace SimpleKeyValueStore
{
    public class DirectoryKeyValueStore<TValue> : IKeyValueStore<TValue>
    {
        private readonly object _lockobj = new object();

        public DirectoryKeyValueStore(string baseDirectoryPath)
        {
            DataName = typeof (TValue).FullName;
            DirectoryPath = Path.Combine(baseDirectoryPath, DataName);
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
        }

        public string DirectoryPath { get; }

        public string DataName { get; }


        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, TValue value)
        {
            var filepath = GetFilePath(key);
            lock (_lockobj)
            {
                if (File.Exists(filepath))
                {
                    return;
                }
                using (var stream = new FileStream(filepath, FileMode.CreateNew, FileAccess.Write, FileShare.Write))
                using (var writer = new StreamWriter(stream))
                {
                    JSON.Serialize(value, writer);
                    writer.Flush();
                }
            }
        }

        /// <summary>
        ///     Get AllKey
        /// </summary>
        /// <returns></returns>
        public List<string> AllKey()
        {
            lock (_lockobj)
            {
                return Directory.GetFiles(DirectoryPath)
                    .Select(Path.GetFileNameWithoutExtension).ToList();
            }
        }

        public void Delete()
        {
            lock (_lockobj)
            {
                foreach (var file in Directory.GetFiles(DirectoryPath))
                {
                    File.Delete(file);
                }
            }
        }

        /// <summary>
        ///     Delete
        /// </summary>
        /// <param name="key"></param>
        public void Delete(string key)
        {
            var path = GetFilePath(key);
            lock (_lockobj)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        public Dictionary<string, TValue> Get()
        {
            lock (_lockobj)
            {
                return Directory.GetFiles(DirectoryPath)
                    .Select(x => new FileInfo(x))
                    .OrderBy(x => x.CreationTime)
                    .ThenBy(x => x.Name)
                    .Select(
                        x =>
                            new
                            {
                                value = JSON.Deserialize<TValue>(File.ReadAllText(x.FullName)),
                                key = Path.GetFileNameWithoutExtension(x.Name)
                            })
                    .ToDictionary(x => x.key, x => x.value);
            }
        }

        public Dictionary<string, TValue> Get(IList<string> keys)
        {
            lock (_lockobj)
            {
                return Directory.GetFiles(DirectoryPath).Select(x => new FileInfo(x))
                    .Select(x => new KeyValuePair<string, FileInfo>(Path.GetFileNameWithoutExtension(x.Name), x))
                    .Join(keys, x => x.Key, y => y, (x, y) => x)
                    .OrderBy(x => x.Value.CreationTime)
                    .ThenBy(x => x.Key)
                    .Select(x => new {value = JSON.Deserialize<TValue>(x.Value.FullName), key = x.Key})
                    .ToDictionary(x => x.key, x => x.value);
            }
        }

        public Tuple<string, TValue> Get(string key)
        {
            lock (_lockobj)
            {
                var path = GetFilePath(key);
                return File.Exists(path) ? Tuple.Create(key, JSON.Deserialize<TValue>(File.ReadAllText(path))) : null;
            }
        }

        public void Replace(string key, TValue value)
        {
            var filepath = GetFilePath(key);
            lock (_lockobj)
            {
                if (!File.Exists(filepath)) return;

                using (var stream = new FileStream(filepath, FileMode.Truncate, FileAccess.Write, FileShare.Write))
                using (var writer = new StreamWriter(stream))
                {
                    JSON.Serialize(value, writer);
                    writer.Flush();
                }
            }
        }

        public void Set(string key, TValue value)
        {
            lock (_lockobj)
            {
                if (File.Exists(GetFilePath(key)))
                {
                    Replace(key, value);
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        private string GetFilePath(string key)
        {
            if (key.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("Not User Chars");
            }

            return Path.Combine(DirectoryPath, key + ".txt");
        }
    }
}