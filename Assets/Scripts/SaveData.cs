using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{

    public int    saveSlot;
    public string saveDateTime;
    public int    dayNumber;
    public float  timeOfDay;
    public int    playTimeSeconds;

    public SerializableVector3 playerPosition;
    public float               playerYRotation;
    public float               currentWeight;

    public List<SavedHerbStack> packContents = new List<SavedHerbStack>();

    public List<SavedHerbStack> storageContents = new List<SavedHerbStack>();

    public List<SavedRequest> activeRequests    = new List<SavedRequest>();
    public List<SavedRequest> completedRequests = new List<SavedRequest>();

    public bool punishmentFiredToday;
}

[Serializable]
public class SavedHerbStack
{
    public string herbAssetName;
    public int    quantity;
}

[Serializable]
public class SavedRequest
{
    public string requestAssetName;
    public int    quantityFulfilled;
}

[Serializable]
public class SerializableVector3
{
    public float x, y, z;

    public SerializableVector3() { }

    public SerializableVector3(UnityEngine.Vector3 v)
    {
        x = v.x; y = v.y; z = v.z;
    }

    public UnityEngine.Vector3 ToVector3() =>
        new UnityEngine.Vector3(x, y, z);
}
