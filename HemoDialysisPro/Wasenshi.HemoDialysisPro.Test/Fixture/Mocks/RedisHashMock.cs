using NuGet.Packaging;
using ServiceStack.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Wasenshi.HemoDialysisPro.Test.Fixture.Mocks
{
    public class RedisHashMock : IRedisHash
    {
        private readonly string _key;
        private Dictionary<string, string> _Data = new Dictionary<string, string>();

        public RedisHashMock(string key)
        {
            _key = key;
        }

        public string this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICollection<string> Keys => _Data.Keys;

        public ICollection<string> Values => _Data.Values;

        public int Count => _Data.Count;

        public bool IsReadOnly => false;

        public string Id => _key;

        public void Add(string key, string value)
        {
            _Data.Add(key, value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            _Data.Add(item.Key, item.Value);
        }

        public bool AddIfNotExists(KeyValuePair<string, string> item)
        {
            if (_Data.TryAdd(item.Key, item.Value))
            {
                return true;
            }
            return false;
        }

        public void AddRange(IEnumerable<KeyValuePair<string, string>> items)
        {
            _Data.AddRange(items);
        }

        public void Clear()
        {
            _Data.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return _Data.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _Data.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            return;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _Data.GetEnumerator();
        }

        public long IncrementValue(string key, int incrementBy)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            return _Data.Remove(key);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return _Data.Remove(item.Key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        {
            return _Data.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
