using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DestroyHelperParticle : MonoBehaviour {

    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            if (!GetComponent<ParticleSystem>().IsAlive(true))
            {
                Destroy(gameObject);
                break;
            }
        }

        yield return null;
    }
}
