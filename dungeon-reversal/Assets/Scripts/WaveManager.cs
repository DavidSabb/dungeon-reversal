using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    public GameObject heroPrefab;
    public Transform[] spawnPoints;
    public int totalWaves = 3;
    public int baseHeroesPerWave = 2;
    public int heroesPerWaveIncrease = 1;
    public float timeBetweenWaves = 5f;
    public float timeBetweenSpawns = 1f;
    public float startDelay = 2f;

    public float autoSpawnRadius = 18f;
    public float autoSpawnMinDist = 12f;

    public int scorePerKill = 100;
    public float difficultyMultiplier = 1f;

    public int CurrentWave { get; private set; }
    public int HeroesAlive { get; private set; }
    public bool AllWavesDone { get; private set; }
    public int Score { get; private set; }

    public System.Action<int> OnWaveStart;
    public System.Action<int> OnWaveEnd;
    public System.Action OnAllWavesComplete;

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

            if (OnWaveStart != null) OnWaveStart(wave);
            yield return StartCoroutine(SpawnWave(wave, count));
            yield return new WaitUntil(() => HeroesAlive <= 0);
            if (OnWaveEnd != null) OnWaveEnd(wave);

            if (wave < totalWaves) yield return new WaitForSeconds(timeBetweenWaves);
        }

        AllWavesDone = true;
        if (OnAllWavesComplete != null) OnAllWavesComplete();
        if (GameManager.Instance != null) GameManager.Instance.HeroesRetreated();
    }

    IEnumerator SpawnWave(int waveNumber, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos;
            Quaternion rot;

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform pt = spawnPoints[Random.Range(0, spawnPoints.Length)];
                pos = pt.position;
                rot = pt.rotation;
            }
            else
            {
                pos = GetAutoSpawnPosition();
                rot = Quaternion.LookRotation(player != null ? (player.position - pos).normalized : Vector3.forward);
            }

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

        NavMeshHit centerHit;
        if (NavMesh.SamplePosition(center, out centerHit, 20f, NavMesh.AllAreas))
            center = centerHit.position;

        for (int i = 0; i < 20; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(autoSpawnMinDist, autoSpawnRadius);
            Vector3 candidate = center + new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(candidate, out hit, 3f, NavMesh.AllAreas)) continue;

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(hit.position, center, NavMesh.AllAreas, path)
                && path.status == NavMeshPathStatus.PathComplete)
                return hit.position;
        }

        return center;
    }

    public void HeroKilled()
    {
        HeroesAlive = Mathf.Max(0, HeroesAlive - 1);
        Score += scorePerKill * Mathf.Max(1, CurrentWave);
    }
}
