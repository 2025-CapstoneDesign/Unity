using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class MarkerObjectManager : MonoBehaviour
{
    public GameObject markerPrefab;
    private Dictionary<int, GameObject> markerObjects = new Dictionary<int, GameObject>();

    public void UpdateMarkers(string jsonData)
    {
        List<(Vector3 position, float scale)> markerPositions = ParseJsonData(jsonData);

        for (int i = 0; i < markerPositions.Count; i++)
        {
            if (!markerObjects.ContainsKey(i))
            {
                GameObject newMarker = Instantiate(markerPrefab, markerPositions[i].position, Quaternion.identity);
                newMarker.transform.localScale = Vector3.one * markerPositions[i].scale;
                markerObjects[i] = newMarker;
            }
            else
            {
                markerObjects[i].transform.position = Vector3.Lerp(markerObjects[i].transform.position, markerPositions[i].position, 0.1f);
                markerObjects[i].transform.localScale = Vector3.one * Mathf.Lerp(markerObjects[i].transform.localScale.x, markerPositions[i].scale, 0.1f);
            }
        }
    }

    private List<(Vector3 position, float scale)> ParseJsonData(string jsonData)
    {
        List<(Vector3 position, float scale)> markerDataList = new List<(Vector3 position, float scale)>();

        try
        {
            JArray jsonArray = JArray.Parse(jsonData);

            foreach (JObject marker in jsonArray)
            {
                float x = marker["x"].ToObject<float>();
                float y = marker["y"].ToObject<float>();
                float z = marker["z"].ToObject<float>();
                float scale = marker["scale"].ToObject<float>();

                markerDataList.Add((new Vector3(x, y, z), scale));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("JSON 파싱 오류: " + e.Message);
        }

        return markerDataList;
    }
}
