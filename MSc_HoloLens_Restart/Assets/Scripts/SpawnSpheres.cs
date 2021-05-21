using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using DataStructures.ViliWonka.KDTree;
using System;

public class SpawnSpheres : MonoBehaviour
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

    Reader reader;
    
    // Start is called before the first frame update
    void Start()
    {
        

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


    public void ApplyToParticleSystem(char spectralClass, List<Tuple<Vector3, float>> points)
    {
        
        var particles = new ParticleSystem.Particle[points.Count];

        for (int i = 0; i < particles.Length; ++i)
        {
            particles[i].position = points[i].Item1;
            particles[i].startSize = particleSize;
            particles[i].startSize = points[i].Item2;
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

}
