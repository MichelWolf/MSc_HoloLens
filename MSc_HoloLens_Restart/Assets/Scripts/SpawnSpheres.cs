using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using DataStructures.ViliWonka.KDTree;
using Photon.Pun;

public class SpawnSpheres : MonoBehaviour, IPunObservable
{
    public double maxObjects = 1700000000;
    public double currentSpawnedObjects = 0;
    public float spawnTime = 0.5f;
    public float spawnRate = 100;
    public GameObject spherePrefab;
    public GameObject quadPrefab;
    public GameObject tetraPrefab;
    public GameObject count;

    public GameObject queryPos;
    public GameObject debugCube;
    
    bool firstFrame = true;

    bool spawning = false;
    bool paused = false;

    public ParticleSystem particleSystemM;
    public ParticleSystem particleSystemK;
    public ParticleSystem particleSystemG;
    public ParticleSystem particleSystemF;
    public ParticleSystem particleSystemA;

    public Color spectralColorM;
    public Color spectralColorK;
    public Color spectralColorG;
    public Color spectralColorF;
    public Color spectralColorA;

    public float particleSize = 1;
    PhotonView photonView;

    Reader reader;
    
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();

        reader = FindObjectOfType<Reader>();
    }

    // Update is called once per frame
    void Update()
    {
        if (firstFrame)
        {
            // Get the first Mesh Observer available, generally we have only one registered
            var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

            // Set to not visible
            if (observer != null)
            {
                observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
                // Set to visible and the Occlusion material
                observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Occlusion;
            }
            firstFrame = false;
        }
    }

    public void StartSpawnSpheres()
    {
        DeleteSpawns();
        spawning = true;
        StartCoroutine(startSpawningSpheres());
    }

    public IEnumerator startSpawningSpheres()
    {
        while (currentSpawnedObjects < maxObjects && spawning)
        {
            if (!paused)
            {
                for (int i = 0; i < spawnRate; i++)
                {
                    Instantiate(spherePrefab, this.transform.position + new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)), Quaternion.identity, this.transform);
                    currentSpawnedObjects++;
                }
                count.GetComponent<TextMeshPro>().text = "Count: " + currentSpawnedObjects.ToString();
                yield return new WaitForSeconds(spawnTime);
            }
            else
            {
                yield return new WaitForSeconds(spawnTime);
            }
        }
    }

    public void StartSpawnQuads()
    {
        DeleteSpawns();
        spawning = true;
        StartCoroutine(startSpawningQuads());
    }

    public IEnumerator startSpawningQuads()
    {
        while (currentSpawnedObjects < maxObjects && spawning)
        {
            if (!paused)
            {
                for (int i = 0; i < spawnRate; i++)
                {
                    Instantiate(quadPrefab, this.transform.position + new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)), Quaternion.identity, this.transform);
                    currentSpawnedObjects++;
                }
                count.GetComponent<TextMeshPro>().text = "Count: " + currentSpawnedObjects.ToString();
                yield return new WaitForSeconds(spawnTime);
            }
            else
            {
                yield return new WaitForSeconds(spawnTime);
            }
        }
    }

    public void StartSpawnTetra()
    {
        DeleteSpawns();
        spawning = true;
        StartCoroutine(startSpawningTetra());
    }

    public IEnumerator startSpawningTetra()
    {
        while (currentSpawnedObjects < maxObjects && spawning)
        {
            if (!paused)
            {
                for (int i = 0; i < spawnRate; i++)
                {
                    Instantiate(tetraPrefab, this.transform.position + new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)), Quaternion.identity, this.transform);
                    currentSpawnedObjects++;
                }
                count.GetComponent<TextMeshPro>().text = "Count: " + currentSpawnedObjects.ToString();
                yield return new WaitForSeconds(spawnTime);
            }
            else
            {
                yield return new WaitForSeconds(spawnTime);
            }
        }
    }

    public void DeleteSpawns()
    {
        spawning = false;
        currentSpawnedObjects = 0;
        foreach(GameObject go in GameObject.FindGameObjectsWithTag("SpawnedObject"))
        {
            Destroy(go);
        }
    }

    public void Spawn(float x, float y, float z)
    {
        Instantiate(tetraPrefab, this.transform.position + new Vector3(x, y, z), Quaternion.identity, this.transform);
    }

    

    public void ApplyToParticleSystem(Vector3[] positions)
    {
        //var ps = GetComponent<ParticleSystem>();
        //if (ps == null)
        //    return;
        List<ParticleSystem.Particle> spectralM = new List<ParticleSystem.Particle>();
        List<ParticleSystem.Particle> spectralK = new List<ParticleSystem.Particle>();
        List<ParticleSystem.Particle> spectralG = new List<ParticleSystem.Particle>();
        List<ParticleSystem.Particle> spectralF = new List<ParticleSystem.Particle>();
        List<ParticleSystem.Particle> spectralA = new List<ParticleSystem.Particle>();

        var particles = new ParticleSystem.Particle[positions.Length];
        Debug.Log(positions.Length);
        for (int i = 0; i < particles.Length; ++i)
        {
            particles[i].position = positions[i];
            particles[i].startSize = particleSize;
            //particles[i].startColor = spectralColorM;
            //if(positions[i].temperature <= 3700)
            //{
            //    spectralM.Add(new ParticleSystem.Particle(positions[i].position));
            //}
            //else if (positions[i].temperature > 3700 && positions[i].temperature <= 5200)
            //{
            //    spectralK.Add(positions[i].position);
            //}
            //else if (positions[i].temperature > 5200 && positions[i].temperature <= 6000)
            //{
            //    spectralG.Add(positions[i].position);
            //}
            //else if (positions[i].temperature > 6000 && positions[i].temperature <= 7500)
            //{
            //    spectralF.Add(positions[i].position);
            //}
            //else if (positions[i].temperature > 7500 && positions[i].temperature <= 10000)
            //{
            //    spectralA.Add(positions[i].position);
            //}
        }

        particleSystemM.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorM;
        particleSystemK.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorK;
        particleSystemG.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorG;
        particleSystemF.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorF;
        particleSystemA.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorA;

        
        particleSystemM.Pause();
    }

    public void ApplyToParticleSystem(List<Vector3> positions)
    {
        //var ps = GetComponent<ParticleSystem>();
        //if (ps == null)
        //    return;

        var particles = new ParticleSystem.Particle[positions.Count];

        for (int i = 0; i < particles.Length; ++i)
        {
            particles[i].position = positions[i];
            particles[i].startSize = particleSize;
            particles[i].startColor = spectralColorM;
        }
        particleSystemA.SetParticles(particles);
        particleSystemA.Pause();
    }

    public void ApplyToParticleSystem(char spectralClass, List<int> indices)
    {
        var particles = new ParticleSystem.Particle[indices.Count];

        for (int i = 0; i < particles.Length; ++i)
        {
            particles[i].position = reader.celestialBodyCloud[indices[i]].position;
            particles[i].startSize = particleSize;
            //particles[i].startColor = spectralColorM;
        }
        switch (spectralClass)
        {
            case 'M':
                particleSystemM.SetParticles(particles);
                particleSystemM.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorM;
                particleSystemM.Pause();
                break;
            case 'K':
                particleSystemK.SetParticles(particles);
                particleSystemK.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorK;
                particleSystemK.Pause();
                break;
            case 'G':
                particleSystemG.SetParticles(particles);
                particleSystemG.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorG;
                particleSystemG.Pause();
                break;
            case 'F':
                particleSystemF.SetParticles(particles);
                particleSystemF.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorF;
                particleSystemF.Pause();
                break;
            case 'A':
                particleSystemA.SetParticles(particles);
                particleSystemA.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorA;
                particleSystemA.Pause();
                break;
            default:
                break;
        }
    }

    public void ApplyToParticleSystem(char spectralClass, List<Vector3> points)
    {
        var particles = new ParticleSystem.Particle[points.Count];

        for (int i = 0; i < particles.Length; ++i)
        {
            particles[i].position = points[i];
            particles[i].startSize = particleSize;
            //particles[i].startColor = spectralColorM;
        }
        switch (spectralClass)
        {
            case 'M':
                if (particleSystemM != null)
                {
                    particleSystemM.SetParticles(particles);
                    particleSystemM.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorM;
                    particleSystemM.Pause();
                }
                break;
            case 'K':
                if (particleSystemK != null)
                {
                    particleSystemK.SetParticles(particles);
                    particleSystemK.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorK;
                    particleSystemK.Pause();
                }
                break;
            case 'G':
                if (particleSystemG != null)
                {
                    particleSystemG.SetParticles(particles);
                    particleSystemG.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorG;
                    particleSystemG.Pause();
                }
                break;
            case 'F':
                if (particleSystemF != null)
                {
                    particleSystemF.SetParticles(particles);
                    particleSystemF.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorF;
                    particleSystemF.Pause();
                }
                break;
            case 'A':
                if (particleSystemA != null)
                {
                    particleSystemA.SetParticles(particles);
                    particleSystemA.transform.gameObject.GetComponent<ParticleSystemRenderer>().material.color = spectralColorA;
                    particleSystemA.Pause();
                }
                break;
            default:
                break;
        }
    }

    public void PauseSpawn()
    {
        paused = !paused;
    }

    public void Query()
    {
        Vector3 pos = queryPos.transform.localPosition;

        QueryTree(pos);
    }

    public void QueryTree(Vector3 position)
    {
        //Query.KDQuery query = new Query.KDQuery();
        KDQuery query = new KDQuery();

        List<int> results = new List<int>();

        // spherical query
        //query.Radius(tree, position, radius, results);

        // returns k nearest points         
        //query.KNearest(tree, position, k, results);

        // bounds query
        //query.Interval(tree, min, max, results);

        // closest point query
        //query.ClosestPoint(FindObjectOfType<Reader>().tree, position, results);

        int index = 0;
        for (int i = 0; i < results.Count; i++)
        {
            index = results[i];
            //Vector3 p = FindObjectOfType<Reader>().pointCloud[results[i]];
            //query.DrawLastQuery();
            //debugCube.transform.localPosition = p;
            //Debug.Log(p);
            //Instantiate(tetraPrefab, this.transform.position + p, Quaternion.identity, this.transform);
        }

        //FindObjectOfType<NetworkManager>().SendQueryResult(index);
    }

    public void TakeOwnershipOfView()
    {
        if (!photonView.IsMine)
        {
            photonView.RequestOwnership();
        }
    }

    public void GiveOwnershipToMasterOfView()
    {
        photonView.TransferOwnership(PhotonNetwork.MasterClient);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //if (stream.IsWriting == true)
        //{
            
        //    stream.SendNext((Vector3)this.transform.position);
        //    stream.SendNext((Quaternion)this.transform.rotation);
        //}
        //else
        //{
        //    this.transform.position = (Vector3)stream.ReceiveNext();
        //    this.transform.rotation = (Quaternion)stream.ReceiveNext();
        //}
    }
}
