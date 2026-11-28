using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// สุ่มต่อท่อนฉากไปเรื่อย ๆ  ใช้ 1 อันต่อ 1 ฝั่ง (บน = มิโกะ, ล่าง = ผู้ชาย)
/// วาง Spawner ที่ความสูง (Y) ของช่องนั้น
/// </summary>
public class ChunkSpawner : MonoBehaviour
{
    public Chunk[] chunkPrefabs;
    [Tooltip("ท่อนแรกที่วางไว้ในฉากแล้ว (ควรเป็นพื้นเรียบไม่มีอุปสรรค)")]
    public Chunk startChunk;
    [Tooltip("สร้างท่อนใหม่ล่วงหน้าจนขอบขวาเลยพิกัด X นี้")]
    public float spawnAheadX = 30f;

    Chunk last;
    readonly List<Chunk> pool = new List<Chunk>();

    void Start()
    {
        last = startChunk;
    }

    void Update()
    {
        if (chunkPrefabs == null || chunkPrefabs.Length == 0) return;

        int guard = 0;
        while ((last == null || last.RightEdge < spawnAheadX) && guard++ < 10)
            SpawnNext();
    }

    void SpawnNext()
    {
        float dist = GameManager.Instance != null ? GameManager.Instance.Distance : 0f;

        pool.Clear();
        foreach (var c in chunkPrefabs)
            if (c != null && c.minDistance <= dist && c.width > 0.1f) pool.Add(c);
        if (pool.Count == 0) return;

        var prefab = pool[Random.Range(0, pool.Count)];
        float x = last != null ? last.RightEdge : transform.position.x;
        last = Instantiate(prefab, new Vector3(x, transform.position.y, 0f), Quaternion.identity, transform);
    }
}
