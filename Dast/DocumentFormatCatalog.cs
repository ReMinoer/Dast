using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dast
{
    public class DocumentFormatCatalog<TFormat> : ICollection<TFormat>
        where TFormat : class, IDocumentFormat
    {
        private readonly ICollection<TFormat> _collectionImplementation = new List<TFormat>();
        private readonly Dictionary<string, ICollection<TFormat>> _extensionDictionary = new Dictionary<string, ICollection<TFormat>>();

        public int Count => _collectionImplementation.Count;
        public bool IsReadOnly => _collectionImplementation.IsReadOnly;

        public IReadOnlyCollection<TFormat> this[string extension] => new ReadOnlyCollection<TFormat>(_extensionDictionary[extension].ToArray());

        public TFormat BestMatch(string extension)
        {
            if (!_extensionDictionary.TryGetValue(extension, out ICollection<TFormat> extensionFormats))
                return null;

            return extensionFormats.FirstOrDefault(x => x.FileExtension.MatchMain(extension)) ?? extensionFormats.First();
        }

        public void Add(TFormat item)
        {
            _collectionImplementation.Add(item);

            AddFormatExtension(item.FileExtension.Main, item);
            foreach (string other in item.FileExtension.Others)
                AddFormatExtension(other, item);
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
            if (!_collectionImplementation.Remove(item))
                return false;

            RemoveFormatExtension(item.FileExtension.Main, item);
            foreach (string other in item.FileExtension.Others)
                RemoveFormatExtension(other, item);

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
            _collectionImplementation.Clear();
            _extensionDictionary.Clear();
        }

        public bool Contains(TFormat item)
        {
            return _collectionImplementation.Contains(item);
        }

        public void CopyTo(TFormat[] array, int arrayIndex)
        {
            _collectionImplementation.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TFormat> GetEnumerator() => _collectionImplementation.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_collectionImplementation).GetEnumerator();
    }
}