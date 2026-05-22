using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    public GameObject heroPrefab;
    public int totalWaves = 3;
    public int baseHeroesPerWave = 2;
    public int heroesPerWaveIncrease = 1;
    public float difficultyMultiplier = 1f;

    const float timeBetweenWaves = 5f;
    const float timeBetweenSpawns = 1f;
    const float startDelay = 2f;
    const float autoSpawnRadius = 18f;
    const float autoSpawnMinDist = 12f;
    const int scorePerKill = 100;

    public int CurrentWave { get; private set; }
    public int HeroesAlive { get; private set; }
    public bool AllWavesDone { get; private set; }
    public int Score { get; private set; }

    List<GameObject> activeHeroes = new List<GameObject>();
    Transform player;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (heroPrefab == null)
        {
            Debug.LogError("WaveManager: heroPrefab not assigned.");
            return;
        }

        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(startDelay);

        for (int wave = 1; wave <= totalWaves; wave++)
        {
            CurrentWave = wave;
            int count = baseHeroesPerWave + (wave - 1) * heroesPerWaveIncrease;

            yield return StartCoroutine(SpawnWave(wave, count));

            while (HeroesAlive > 0)
                yield return null;

            if (wave < totalWaves) yield return new WaitForSeconds(timeBetweenWaves);
        }

        AllWavesDone = true;
        if (GameManager.Instance != null) GameManager.Instance.HeroesRetreated();
    }

    IEnumerator SpawnWave(int waveNumber, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = GetAutoSpawnPosition();
            Quaternion rot = Quaternion.LookRotation(player != null ? (player.position - pos).normalized : Vector3.forward);

            GameObject hero = Instantiate(heroPrefab, pos, rot);
            if (!hero.activeSelf) hero.SetActive(true);
            activeHeroes.Add(hero);
            HeroesAlive++;

            HeroAI ai = hero.GetComponent<HeroAI>();
            if (ai != null) ai.SetWaveScaling(waveNumber, difficultyMultiplier);

            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    Vector3 GetAutoSpawnPosition()
    {
        Vector3 center = player != null ? player.position : transform.position;

        float angle = Random.Range(0f, 360f);
        float dist = Random.Range(autoSpawnMinDist, autoSpawnRadius);
        Vector3 candidate = center + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * dist;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidate, out hit, 10f, NavMesh.AllAreas))
            return hit.position;

        return center;
    }

    public void HeroKilled()
    {
        HeroesAlive = Mathf.Max(0, HeroesAlive - 1);
        Score += scorePerKill * Mathf.Max(1, CurrentWave);
    }
}
