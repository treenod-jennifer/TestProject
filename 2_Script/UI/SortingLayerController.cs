using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortingLayerController : MonoBehaviour
{
    [SerializeField] private ParticleSystemRenderer[] particleObjects;

    private Dictionary<string, ParticleSystemRenderer> particles = null;

    private void InitParticles()
    {
        particles = new Dictionary<string, ParticleSystemRenderer>();

        foreach (var particle in particleObjects)
        {
            particles.Add(particle.name, particle);
        }
    }

    public void SetParticleSortingLayer(string key, int sortingLayer)
    {
        if(particles == null) InitParticles();

        particles[key].sortingOrder = sortingLayer;
    }

    public void SetParticleSortingLayer(int sortingLayer)
    {
        if (particles == null) InitParticles();

        foreach(var particle in particles)
        {
            particle.Value.sortingOrder = sortingLayer;
        }
    }
}
