using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// WaveManager.cs
/// Dungeon Reversal - Manages hero wave spawning with escalating difficulty.
/// Assign spawn points and the Crusader hero prefab in the Inspector.
/// Each wave heroes are faster, hit harder, and react quicker (GDD: heroes learn).
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Setup")]
    public GameObject heroPrefab;             // Crusader prefab with HeroAI + HeroHealth
    public Transform[] spawnPoints;           // Spawn locations around arena
    public int totalWaves = 5;

    [Header("Wave Config")]
    public int   baseHeroesPerWave  = 3;
    public int   heroesPerWaveIncrease = 1;   // +N heroes each wave
    public float timeBetweenWaves   = 5f;
    public float timeBetweenSpawns  = 0.8f;

    // Public state — no [Header] on properties (causes CS0592)
    public int  CurrentWave   { get; private set; }
    public int  HeroesAlive   { get; private set; }
    public bool WaveActive    { get; private set; }
    public bool AllWavesDone  { get; private set; }

    // Events
    public System.Action<int> OnWaveStart;  // wave number
    public System.Action<int> OnWaveEnd;    // wave number
    public System.Action OnAllWavesComplete;

    private List<GameObject> _activeHeroes = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(2f); // brief pause before first wave

        for (int wave = 1; wave <= totalWaves; wave++)
        {
            CurrentWave = wave;
            int heroCount = baseHeroesPerWave + (wave - 1) * heroesPerWaveIncrease;

            OnWaveStart?.Invoke(wave);
            WaveActive = true;

            yield return StartCoroutine(SpawnWave(wave, heroCount));

            // Wait until all heroes are dead
            yield return new WaitUntil(() => HeroesAlive <= 0);

            WaveActive = false;
            OnWaveEnd?.Invoke(wave);

            if (wave < totalWaves)
                yield return new WaitForSeconds(timeBetweenWaves);
        }

        AllWavesDone = true;
        OnAllWavesComplete?.Invoke();
        GameManager.Instance?.HeroesRetreated();
    }

    private IEnumerator SpawnWave(int waveNumber, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (spawnPoints.Length == 0) break;
            Transform spawnPt = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject hero = Instantiate(heroPrefab, spawnPt.position, spawnPt.rotation);
            _activeHeroes.Add(hero);
            HeroesAlive++;

            // Apply wave scaling so heroes react faster each wave
            HeroAI ai = hero.GetComponent<HeroAI>();
            if (ai != null) ai.SetWaveScaling(waveNumber);

            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    /// <summary>Called by HeroHealth when a hero dies.</summary>
    public void HeroKilled()
    {
        HeroesAlive = Mathf.Max(0, HeroesAlive - 1);
    }
}