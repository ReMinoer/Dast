using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dast.Catalogs
{
    public class FormatCatalog<TFormat> : ICollection<TFormat>
        where TFormat : IFormat
    {
        private readonly ICollection<TFormat> _collection = new List<TFormat>();
        private readonly Dictionary<string, ICollection<TFormat>> _extensionDictionary = new Dictionary<string, ICollection<TFormat>>();

        public int Count => _collection.Count;
        public bool IsReadOnly => _collection.IsReadOnly;

        public IReadOnlyCollection<TFormat> this[string extension] => new ReadOnlyCollection<TFormat>(_extensionDictionary[extension].ToArray());

        public TFormat BestMatch(string extension)
        {
            if (!_extensionDictionary.TryGetValue(extension, out ICollection<TFormat> extensionFormats))
                return default(TFormat);

            foreach (TFormat format in extensionFormats)
                if (format.FileExtensions.Any(e => e.MatchMain(extension)))
                    return format;

            return extensionFormats.FirstOrDefault();
        }

        public void Add(TFormat item)
        {
            _collection.Add(item);

            foreach (FileExtension fileExtension in item.FileExtensions)
            {
                AddFormatExtension(fileExtension.Main, item);
                foreach (string other in fileExtension.Others)
                    AddFormatExtension(other, item);
            }
        }

        public void AddRange(IEnumerable<TFormat> enumerable)
        {
            foreach (TFormat format in enumerable)
                Add(format);
        }

        private void AddFormatExtension(string extension, TFormat documentFormat)
        {
            if (!_extensionDictionary.TryGetValue(extension, out ICollection<TFormat> extensionFormats))
                extensionFormats = _extensionDictionary[extension] = new List<TFormat>();
            extensionFormats.Add(documentFormat);
        }

        public bool Remove(TFormat item)
        {
            if (!_collection.Remove(item))
                return false;

            foreach (FileExtension fileExtension in item.FileExtensions)
            {
                RemoveFormatExtension(fileExtension.Main, item);
                foreach (string other in fileExtension.Others)
                    RemoveFormatExtension(other, item);
            }

            return true;
        }

        private void RemoveFormatExtension(string extension, TFormat documentFormat)
        {
            ICollection<TFormat> extensionFormats = _extensionDictionary[extension];
            extensionFormats.Remove(documentFormat);

            if (extensionFormats.Count == 0)
                _extensionDictionary.Remove(extension);
        }

        public void Clear()
        {
            _collection.Clear();
            _extensionDictionary.Clear();
        }

        public bool Contains(TFormat item)
        {
            return _collection.Contains(item);
        }

        public void CopyTo(TFormat[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TFormat> GetEnumerator() => _collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_collection).GetEnumerator();
    }
}