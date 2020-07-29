using System;
using System.Collections.Generic;

[Serializable]
public class Project
{
    public string author, name;
    public int mins, secs;
    public string source, creatorNick;

    public bool hasImage;
    public enum ImageExtension { Jpeg, Png }
    public ImageExtension imageExtension;
    public byte[] image;

    public enum AudioExtension { Ogg, Mp3 }
    public AudioExtension audioExtension;
    public byte[] audioFile;

    /// <summary>
    /// Deprecated. Use difficulties instead.
    /// </summary>
    public List<BeatCubeClass> beatCubeList = new List<BeatCubeClass>();
    public List<Bookmark> bookmarks = new List<Bookmark>();

    public string difficultName;
    public int difficultStars;
    
    public int lastGivenDifficultyId = -1;
    public List<Difficulty> difficulties = new List<Difficulty>();


    public void CheckDefaults()
    {
        if (beatCubeList == null) beatCubeList = new List<BeatCubeClass>();
        if (bookmarks == null) bookmarks = new List<Bookmark>();
    }

    public static string ToString(ImageExtension value)
    {
        return value == ImageExtension.Jpeg ? ".jpg" : ".png";
    }
    public static string ToString(AudioExtension value)
    {
        return value == AudioExtension.Mp3 ? ".mp3" : ".ogg";
    }
}
[Serializable]
public class Difficulty
{
    public string name;
    public int stars;
    public int id = -1;
    public float speed = 1;

    public List<BeatCubeClass> beatCubeList = new List<BeatCubeClass>();

    public Difficulty Clone()
    {
        return (Difficulty)this.MemberwiseClone();
    }
}

[Serializable]
public class BeatCubeClass
{
    public float time;

    public int road; // 0-3 include
    public int level; // 0-1 include
    public float speed = 1; // 1 - default

    public enum Type { Point, Dir, Line, Bomb }
    public Type type;

    public enum SubType { Down, DownRight, Right, UpRight, Up, UpLeft, Left, DownLeft, Random }
    public SubType subType;

    public int saberType;

    // Used only for Line (position of start and end of line)
    public float lineLenght = 0; // Time in seconds
    public int lineEndRoad = 0; // Second point road (If linePoints.Count != 0, then use legacy way - linePoints)
    public int lineEndLevel = 0;
    public List<SerializableVector3> linePoints;

    public BeatCubeClass() { }

    public BeatCubeClass(float time, int road, Type type)
    {
        this.time = time;
        this.road = road;
        this.type = type;
    }
}

[Serializable]
public class Bookmark
{
    public float time;
    public int type;
    public SerializableColor color;

    public Bookmark(float time, int type, SerializableColor color)
    {
        this.time = time;
        this.type = type;
        this.color = color;
    }

    public Bookmark() { }
}

[System.Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rX"></param>
    /// <param name="rY"></param>
    /// <param name="rZ"></param>
    public SerializableVector3(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }

    /// <summary>
    /// Returns a string representation of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}]", x, y, z);
    }

    /// <summary>
    /// Automatic conversion from SerializableVector3 to Vector3
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    //public static implicit operator Vector3(SerializableVector3 rValue)
    //{
    //    return new Vector3(rValue.x, rValue.y, rValue.z);
    //}

    /// <summary>
    /// Automatic conversion from Vector3 to SerializableVector3
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    //public static implicit operator SerializableVector3(Vector3 rValue)
    //{
    //    return new SerializableVector3(rValue.x, rValue.y, rValue.z);
    //}
}

[Serializable]
public struct SerializableColor
{
    public float r, g, b;
    public SerializableColor(float r, float g, float b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }
    //public static implicit operator Color(SerializableColor color)
    //{
    //    return new Color(color.r, color.g, color.b);
    //}

    //public static implicit operator SerializableColor(Color color)
    //{
    //    return new SerializableColor(color.r, color.g, color.b);
    //}
}