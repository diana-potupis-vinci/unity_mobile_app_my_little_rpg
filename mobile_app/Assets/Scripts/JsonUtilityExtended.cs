using System.Collections.Generic;
using UnityEngine;

public static class JsonUtilityExtended
{
    public static List<T> FromJsonList<T>(string json)
    {
        string newJson = "{ \"list\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.list ?? new List<T>();
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> list;
    }
}
