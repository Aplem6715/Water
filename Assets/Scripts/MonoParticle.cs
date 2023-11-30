using System.Collections;
using System.Collections.Generic;
using SPH;
using UnityEngine;

public class MonoParticle : MonoBehaviour
{
    private Particle _linkedParticle;

    public void Link(Particle particle) { _linkedParticle = particle; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3((float)_linkedParticle.Pos.x, (float)_linkedParticle.Pos.y);
    }
}
