using UnityEngine;
using System.Collections.Generic;

public class StandaloneObstacleSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> obstaclePrefabs;
    [SerializeField] private float scrollSpeed = 8f;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float destroyX = -25f;
    [SerializeField] private bool preventImmediateRepeat = true;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private float timer;
    private int lastIndex = -1;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval && obstaclePrefabs != null && obstaclePrefabs.Count > 0)
        {
            timer = 0f;
            int index = PickRandomIndex();
            if (index >= 0 && obstaclePrefabs[index] != null)
            {
                GameObject obj = Instantiate(obstaclePrefabs[index], transform.position, Quaternion.identity);
                spawnedObjects.Add(obj);
            }
        }

        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = spawnedObjects[i];
            if (obj == null)
            {
                spawnedObjects.RemoveAt(i);
                continue;
            }

            obj.transform.position += Vector3.left * (scrollSpeed * Time.deltaTime);

            if (obj.transform.position.x <= destroyX)
            {
                Destroy(obj);
                spawnedObjects.RemoveAt(i);
            }
        }
    }

    private int PickRandomIndex()
    {
        int count = obstaclePrefabs.Count;
        if (count == 0) return -1;

        int chosen = Random.Range(0, count);
        if (preventImmediateRepeat && count > 1 && chosen == lastIndex)
        {
            chosen = (chosen + Random.Range(1, count)) % count;
        }

        lastIndex = chosen;
        return chosen;
    }
}