using System;

namespace src;

public class FileType : IEquatable<FileType>
{
    public int Id { get; private set; }

    public static FileType Any = new FileType(0);
    public static FileType Idf = new FileType(1);
    public static FileType Doe2 = new FileType(2);
    public FileType(int id)
    {
        Id = id;
    }

    public bool Equals(FileType other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((FileType)obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }

    public static bool operator ==(FileType left, FileType right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(FileType left, FileType right)
    {
        return !Equals(left, right);
    }
}