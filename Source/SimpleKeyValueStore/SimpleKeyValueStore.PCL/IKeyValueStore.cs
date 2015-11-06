using System;
using System.Collections.Generic;

namespace SimpleKeyValueStore
{
    public interface IKeyValueStore<TValue>
    {
        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Add(string key, TValue value);

        /// <summary>
        ///     Replace
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Replace(string key, TValue value);

        /// <summary>
        ///     Add or Replace
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Set(string key, TValue value);

        /// <summary>
        ///     Get
        /// </summary>
        /// <param name="key"></param>
        Tuple<string, TValue> Get(string key);

        /// <summary>
        ///     GetAll
        /// </summary>
        Dictionary<string, TValue> Get();

        /// <summary>
        /// Get Where
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        Dictionary<string, TValue> Get(IList<string> keys); 

        /// <summary>
        ///     Delete
        /// </summary>
        /// <param name="key"></param>
        void Delete(string key);

        /// <summary>
        ///     DeleteAll
        /// </summary>
        void Delete();

        /// <summary>
        /// Get AllKey
        /// </summary>
        /// <returns></returns>
        List<string> AllKey();


    }
}