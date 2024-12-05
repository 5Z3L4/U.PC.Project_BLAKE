using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Fog : MonoBehaviour
{
    [SerializeField]
    private int fogWidth = 5;
    [SerializeField]
    private int fogDepth = 5;
    [SerializeField]
    private float particleSpacing = 1;
    [SerializeField]
    private GameObject particlePrefab;
    [SerializeField]
    private float particlesYOffset = 0f;
    [SerializeField]
    private LayerMask objectsToHide;
    [SerializeField]
    private bool turnedOn = false;
    [SerializeField]
    private GameObject LODFog;
    private bool disabled = false;

    private List<FogParticle> particles = new List<FogParticle>();
    private Dictionary<Renderer, bool> hiddenObjects = new Dictionary<Renderer, bool>();

    private void Awake()
    {
        GetComponentInParent<Room>().OnResetEnemies += hiddenObjects.Clear;
        for (int i = 0; i < fogWidth; i++)
        {
            for (int j = 0; j < fogDepth; j++)
            {
                Vector3 local = new Vector3(i * particleSpacing, transform.position.y + particlesYOffset, j* particleSpacing);
                Vector3 world = transform.TransformPoint(local);
                particles.Add(new FogParticle(i, j, Instantiate(particlePrefab, world, Quaternion.identity, this.transform)));
            }
        }
    }

    private void Update()
    {
        if (disabled) return;
        List<Renderer> toHide = new List<Renderer>();
        foreach(FogParticle particle in particles)
        {
            Vector3 localPosition = new Vector3(particle.x * particleSpacing, 0, particle.y * particleSpacing);
            Vector3 worldPosition = transform.TransformPoint(localPosition);
            Collider[] hits = Physics.OverlapCapsule(worldPosition - Vector3.up, worldPosition + Vector3.up, 0.2f, objectsToHide);
            bool blocking = false;
            foreach (Collider hit in hits)
            {
                if (hit.gameObject.layer == LayerMask.NameToLayer("BlockFog"))
                {
                    blocking = true;
                    break;
                }
            }
            foreach (Collider hit in hits)
            {
                Renderer[] renderers = hit.GetComponentsInChildren<Renderer>(); // This gets both MeshRenderer and SkinnedMeshRenderer
                if (!turnedOn || (turnedOn && !blocking)){
                    foreach (Renderer renderer in renderers)
                    {
                        if (hiddenObjects.ContainsKey(renderer))
                        {
                            if (!hiddenObjects[renderer])
                            {
                                toHide.Add(renderer);
                                renderer.enabled = false;
                            }
                            continue;
                        }
                        hiddenObjects.Add(renderer, true);
                        renderer.enabled = false;
                    }
                }
            }
            if (blocking || !turnedOn)
            {
                particle.TurnOff();
                continue;
            }
            else
            {
                particle.TurnOn();
            }
        }

        List<Renderer> keys = new List<Renderer>(hiddenObjects.Keys);
        foreach(Renderer key in keys)
        {
            if (toHide.Contains(key)) continue;
            key.enabled = true;
            hiddenObjects[key] = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (particles.Count == 0)
        {
            for (int i = 0; i < fogWidth; i++)
            {
                for (int j = 0; j < fogDepth; j++)
                {
                    Vector3 localPosition = new Vector3(i * particleSpacing, 0, j * particleSpacing);
                    Vector3 worldPosition = transform.TransformPoint(localPosition);
                    Gizmos.DrawWireSphere(worldPosition - Vector3.up, 0.2f);
                    Gizmos.DrawWireSphere(worldPosition + Vector3.up, 0.2f);
                }
            }
        } else
        {
            foreach(var particle in particles)
            {
                Vector3 localPosition = new Vector3(particle.x * particleSpacing, 0, particle.y * particleSpacing);
                Vector3 worldPosition = transform.TransformPoint(localPosition);
                Gizmos.DrawWireSphere(worldPosition - Vector3.up, 0.2f);
                Gizmos.DrawWireSphere(worldPosition + Vector3.up, 0.2f);
            }
        }
    }

    public void TurnOffFog()
    {
        foreach(var mr in hiddenObjects.Keys)
        {
            mr.enabled = true;
        }
        hiddenObjects.Clear();
        turnedOn = false;
        if (LODFog != null)
        {
            LODFog.SetActive(true);
        }

    }

    private void OnDisable()
    {
        disabled = true;
        TurnOffFog();
    }

    private void OnEnable()
    {
        if (LODFog != null)
        {
            LODFog.SetActive(true);
        }
        TurnOffFog();
        disabled = false;
    }

    public void Peek()
    {
        turnedOn = true;
        if (LODFog != null)
        {
            LODFog.SetActive(false);
        }
    }

}

public class FogParticle
{
    public int x;
    public int y;
    public GameObject particlePrefab;
    private bool turnedOn = true;
    private ParticleSystem particleSystem;

    public FogParticle(int x, int y, GameObject particlePrefab)
    {
        this.x = x;
        this.y = y;
        this.particlePrefab = particlePrefab;
        particleSystem = particlePrefab.GetComponent<ParticleSystem>();
    }

    public bool IsTurnedOn()
    {
        return turnedOn;
    }

    public void TurnOff()
    {
        if (!turnedOn) return;
        turnedOn = false;
        particleSystem.Stop();
        particleSystem.Clear();
        particleSystem.gameObject.SetActive(false);
    }

    public void TurnOn()
    {
        if (turnedOn) return;
        turnedOn = true;
        particleSystem.gameObject.SetActive(true);
        particleSystem.Play();
    }
}
