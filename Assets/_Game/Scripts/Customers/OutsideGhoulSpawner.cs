using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Ouiki.Restaurant;

public class OutsideGhoulSpawner : MonoBehaviour
{
    [Header("Ghoul Spawning (Prefab, Area, etc)")]
    public List<OutsideGhoul> ghoulPrefabs; // Assign OutsideGhoul prefabs directly
    public int numberOfGhouls = 6;
    public Transform[] ghoulSpawnPoints; // Optional fixed spawn points outside, else random
    public Vector3 areaMin = new Vector3(-10, 0, -10); // For random spawn
    public Vector3 areaMax = new Vector3(-5, 0, 5);
    public Vector3 areaOffset = Vector3.zero;
    public float morphDelay = 0.7f;
    public float screamOffset = 0.22f;

    public void SpawnKillers()
    {
        StartCoroutine(SpawnMorphScreamChaseRoutine());
    }

    private IEnumerator SpawnMorphScreamChaseRoutine()
    {
        List<OutsideGhoul> spawnedGhouls = new List<OutsideGhoul>();

        for (int i = 0; i < numberOfGhouls; i++)
        {
            // Pick spawn position
            Vector3 spawnPos;
            if (ghoulSpawnPoints != null && ghoulSpawnPoints.Length > 0)
                spawnPos = ghoulSpawnPoints[i % ghoulSpawnPoints.Length].position;
            else
            {
                float x = Random.Range(areaMin.x, areaMax.x);
                float y = Random.Range(areaMin.y, areaMax.y);
                float z = Random.Range(areaMin.z, areaMax.z);
                Vector3 localRandom = new Vector3(x, y, z) + areaOffset;
                spawnPos = transform.TransformPoint(localRandom);
            }

            // Pick prefab
            if (ghoulPrefabs == null || ghoulPrefabs.Count == 0)
            {
                Debug.LogWarning("[OutsideGhoulSpawner] No ghoul prefabs assigned.");
                yield break;
            }
            var prefab = ghoulPrefabs[Random.Range(0, ghoulPrefabs.Count)];
            var ghoul = Instantiate(prefab, spawnPos, Quaternion.identity);

            spawnedGhouls.Add(ghoul);
        }

        // Morph delay (visual effect, optional for dramatic spawn)
        yield return new WaitForSeconds(morphDelay);

        // Scream in sequence with offset
        for (int i = 0; i < spawnedGhouls.Count; i++)
        {
            spawnedGhouls[i].PlayScream();
            yield return new WaitForSeconds(screamOffset);
        }

        // Set all to chase mode
        foreach (var ghoul in spawnedGhouls)
        {
            ghoul.BeginChase(); // Immediate pursuit logic
        }
    }
}