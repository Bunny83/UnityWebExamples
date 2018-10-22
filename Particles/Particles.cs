using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Fragment
{
    public Vector3 home;
    public Color col;
}

public class Particles : MonoBehaviour
{
    public Texture2D inputTex;
    ParticleSystem ps;
    List<Fragment> fragments;
    ParticleSystem.Particle[] particles;
    float radius = 3;
    void Start ()
    {
        ps = GetComponent<ParticleSystem>();
        CreateParticles();
    }

    void CreateParticles()
    {
        fragments = new List<Fragment>();

        var cols = inputTex.GetPixels();
        var width = inputTex.width;
        var height = inputTex.height;
        var offset = new Vector3(-width,-height) * 0.5f;
        for(int x = 0; x < width; x+=2)
        {
            for(int y = 0; y < height; y+=2)
            {
                var col = cols[x + y * width];
                if (col.a >0)
                {
                    var f = new Fragment();
                    f.home = (offset + new Vector3(x, y, 0))*0.05f;
                    f.col = col;
                    fragments.Add(f);
                }
            }
        }
        particles = new ParticleSystem.Particle[fragments.Count];
        for(int i = 0; i < fragments.Count; i++)
        {
            var p = new ParticleSystem.Particle();
            p.position = fragments[i].home;
            p.color = fragments[i].col;
            p.color = new Color32(p.color.r, p.color.g, p.color.b, 255);
            p.lifetime = float.PositiveInfinity;
            p.size = 0.4f;
            p.startLifetime = float.PositiveInfinity;
            particles[i] = p;
        }
        ps.SetParticles(particles, particles.Length);
        Debug.Log("Count: " + fragments.Count);
    }

	
    void Update ()
    {
        ps.GetParticles(particles);
        var plane = new Plane(-transform.forward,transform.position);
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float dist = 0;
        plane.Raycast(ray, out dist);
        var pos = ray.GetPoint(dist);

        float v = Input.GetAxis("Mouse ScrollWheel");
        if (v != 0)
            radius = Mathf.Clamp(radius + v * 1f, 1f, 300);


        int count = particles.Length;
        for(int i = 0; i < count; i++)
        {
            var p = particles[i];
            var f = fragments[i];
            p.velocity *= 0.9f;
            p.velocity += (f.home - p.position);

            // apply random position offset and velocity to each particle
            if (Input.GetKey(KeyCode.A))
            {
                p.velocity += new Vector3(Random.Range(-10, 10), Random.Range(-10, 10));
                p.position += new Vector3(Random.Range(-10, 10), Random.Range(-10, 10));
            }
            // apply offset and repelling velocity according to the mouse position.
            if (Input.GetMouseButton(0) || v!=0)
            {
                var d = p.position - pos;
                if (d.sqrMagnitude < radius * radius) // if particle is inside our circle
                {
                    d = d.normalized;
                    p.position = pos + d * radius; // offset the particle to the border of the circle
                    p.velocity += d; // additionally add a repelling force
                }
            }
            particles[i] = p;
        }
        ps.SetParticles(particles, count);
    }
    void OnGUI()
    {
        GUILayout.Label("Particle Count: " + particles.Length + "\nRadius: " + radius.ToString("f2"));
    }
}
