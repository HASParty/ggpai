using UnityEngine;
using System.Collections;

public class AsteroidSpawner : MonoBehaviour {

    //prefabs with asteroid script attached here, add in inspector
    public Asteroid[] AsteroidPrefabs;

    [Tooltip("Spawn every [frequency] seconds")]
    public float SpawnMinFrequency = 2;
    public float SpawnMaxFrequency = 8;
    public float SpawnRadius = 5f;
    public int Asteroids = 200;
    public float MaxSize = 50;
    public float MinSize = 1;
    public float SizeSTD = 5;
    public float SizeMean = 25;

    private float spawnTimer;
    private float spawnTime;
	void Start () {
        ResetTimer();
        for (int i = 0; i < Asteroids; i++){
            Spawn();
        }
	}

    void ResetTimer() {
        spawnTimer = 0f;
        spawnTime = Random.Range(SpawnMinFrequency, SpawnMaxFrequency);
    }

    void Spawn() {
        int pick = Random.Range(0, AsteroidPrefabs.Length - 1);
        GameObject go = Instantiate(AsteroidPrefabs[pick].gameObject);
        //sort all asteroids in the hierarchy under the spawner
        go.transform.SetParent(transform);
        //set its initial location to within the spawn radius
        float size = Random.value * SizeSTD + SizeMean;
        if (size > 10){
            Debug.Log(size);
        }
        size = Mathf.Max(size, MinSize);
        size = Mathf.Min(size, MaxSize);
        
        
        go.transform.localScale += new Vector3(size, size, size);
        go.transform.localPosition = Random.insideUnitSphere*SpawnRadius;
    }

    //Visualises the spawn radius in scene view
    void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position, SpawnRadius);
    }

	// Update is called once per frame
	void Update () {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnTime) {
            ResetTimer();
            Spawn();
        }
	}
}
