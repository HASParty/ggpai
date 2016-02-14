using UnityEngine;
using System.Collections;

public class AsteroidSpawner : MonoBehaviour {
    //prefabs with asteroid script attached here, add in inspector
    private float spawnTimer;
    private float spawnTime;
    public Asteroid[] AsteroidPrefabs;

    [Tooltip("Spawn every [frequency] seconds")]
    public float SpawnMean = 500f;
    public float SpawnSTD = 5000f;
    public int Asteroids = 1000;
    public float MaxSize = 150;
    public float MinSize = 1;
    public float SizeSTD = 80;
    public float SizeMean = 5;

	void Start () {
        for (int i = 0; i < Asteroids; i++){
            Spawn();
        }
	}


    void Spawn() {
        int pick = Random.Range(1, AsteroidPrefabs.Length);
        GameObject go = Instantiate(AsteroidPrefabs[2].gameObject);

        //sort all asteroids in the hierarchy under the spawner
        go.transform.SetParent(transform);

        //set its initial location to within the spawn radius
        float size = Random.value * SizeSTD + SizeMean;
        float spawnRadius = Random.value * SpawnSTD + SpawnMean;
        size = Mathf.Max(size, MinSize);
        size = Mathf.Min(size, MaxSize);
        go.transform.localScale += new Vector3(size * (1 + Random.value * 0.8f),
                                               size * (1 + Random.value * 0.8f),
                                               size * (1 + Random.value * 0.8f));
        go.transform.localPosition = Random.onUnitSphere*spawnRadius;
    }

    //Visualises the spawn radius in scene view
    void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position, SpawnSTD);
    }

	// Update is called once per frame
	void Update () {
	}
}
