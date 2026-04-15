using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Setup")]
    public GameObject heroPrefab;
    public Transform[] spawnPoints;
    public int totalWaves = 5;

    [Header("Wave Config")]
    public int   baseHeroesPerWave    = 3;
    public int   heroesPerWaveIncrease = 1;
    public float timeBetweenWaves     = 5f;
    public float timeBetweenSpawns    = 0.8f;
    public float startDelay           = 2f;

    [Header("Auto Spawn (used when no spawnPoints assigned)")]
    public float autoSpawnRadius   = 18f;
    public float autoSpawnMinDist  = 12f;

    [Header("Difficulty Scaling (per wave above 1)")]
    public float damageScalePerWave    = 0.20f; // +20% dmg per wave
    public float healthScalePerWave    = 0.25f; // +25% HP per wave
    public float speedScalePerWave     = 0.12f; // +12% speed per wave
    public float reactReductionPerWave = 0.18f; // -18% react delay per wave (compounding)
    public float cooldownReductionPerWave = 0.12f;

    [Header("Scoring")]
    public int scorePerKill = 100;

    public int  CurrentWave   { get; private set; }
    public int  HeroesAlive   { get; private set; }
    public bool WaveActive    { get; private set; }
    public bool AllWavesDone  { get; private set; }
    public int  Score         { get; private set; }

    public System.Action<int> OnWaveStart;
    public System.Action<int> OnWaveEnd;
    public System.Action OnAllWavesComplete;

    private List<GameObject> _activeHeroes = new List<GameObject>();
    private Transform _player;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;

        if (heroPrefab == null)
        {
            Debug.LogWarning("WaveManager: heroPrefab not assigned. Drag the Crusader prefab into the slot.");
            return;
        }

        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(startDelay);

        for (int wave = 1; wave <= totalWaves; wave++)
        {
            CurrentWave = wave;
            int heroCount = baseHeroesPerWave + (wave - 1) * heroesPerWaveIncrease;

            OnWaveStart?.Invoke(wave);
            WaveActive = true;

            yield return StartCoroutine(SpawnWave(wave, heroCount));

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
            Vector3 pos; Quaternion rot;
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform pt = spawnPoints[Random.Range(0, spawnPoints.Length)];
                pos = pt.position;
                rot = pt.rotation;
            }
            else
            {
                pos = GetAutoSpawnPosition();
                rot = Quaternion.LookRotation(_player != null
                    ? (_player.position - pos).normalized
                    : Vector3.forward);
            }

            GameObject hero = Instantiate(heroPrefab, pos, rot);
            _activeHeroes.Add(hero);
            HeroesAlive++;

            HeroAI ai = hero.GetComponent<HeroAI>();
            if (ai != null) ai.SetWaveScaling(waveNumber);

            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    private Vector3 GetAutoSpawnPosition()
    {
        Vector3 center = _player != null ? _player.position : transform.position;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float dist  = Random.Range(autoSpawnMinDist, autoSpawnRadius);
        return center + new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);
    }

    public void HeroKilled()
    {
        HeroesAlive = Mathf.Max(0, HeroesAlive - 1);
        Score += scorePerKill * Mathf.Max(1, CurrentWave);
    }

    public float GetDamageScale(int wave)   => 1f + (wave - 1) * damageScalePerWave;
    public float GetHealthScale(int wave)   => 1f + (wave - 1) * healthScalePerWave;
    public float GetSpeedScale(int wave)    => 1f + (wave - 1) * speedScalePerWave;
    public float GetReactScale(int wave)    => Mathf.Pow(1f - reactReductionPerWave, wave - 1);
    public float GetCooldownScale(int wave) => Mathf.Pow(1f - cooldownReductionPerWave, wave - 1);
}
