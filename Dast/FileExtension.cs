using System;
using System.Linq;

namespace Dast
{
    public struct FileExtension
    {
        static public FileExtension Unknown => new FileExtension("", "");

        public readonly string Name;
        public readonly string Main;
        public readonly string[] Others;

        public FileExtension(string name, string main, params string[] others)
        {
            Name = name;
            Main = main;
            Others = others;
        }

        public bool Match(string extension)
        {
            return MatchMain(extension) || MatchOthers(extension);
        }

        public bool MatchMain(string extension)
        {
            return Main.Equals(extension, StringComparison.OrdinalIgnoreCase);
        }

        public bool MatchOthers(string extension)
        {
            return Others.Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }

        public bool Equals(FileExtension other)
        {
            return string.Equals(Name, other.Name) && string.Equals(Main, other.Main) && Equals(Others, other.Others);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is FileExtension extension && Equals(extension);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Name != null ? Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Main != null ? Main.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Others != null ? Others.GetHashCode() : 0);
                return hashCode;
            }
        }

        static public bool operator==(FileExtension a, FileExtension b)
        {
            return a.Equals(b);
        }

        static public bool operator!=(FileExtension a, FileExtension b)
        {
            return !(a == b);
        }
    }
}