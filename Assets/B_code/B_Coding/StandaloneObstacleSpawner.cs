using UnityEngine;
using System.Collections.Generic;

public class StandaloneObstacleSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> obstaclePrefabs;
    [SerializeField] private float scrollSpeed = 8f;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float destroyX = -25f;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private float timer;

    private void Update()
    {
        // 1. นับเวลาเสกอุปสรรคใหม่
        timer += Time.deltaTime;
        if (timer >= spawnInterval && obstaclePrefabs != null && obstaclePrefabs.Count > 0)
        {
            timer = 0f;
            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity);
                spawnedObjects.Add(obj);
            }
        }

        // 2. เลื่อนอุปสรรคที่เกิดแล้วไปทางซ้าย และลบเมื่อพ้นจอ
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
}