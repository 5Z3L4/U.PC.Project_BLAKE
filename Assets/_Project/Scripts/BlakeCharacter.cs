using System;
using System.Collections;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Interfaces;
using _Project.Scripts.SoundEffects;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class BlakeCharacter : MonoBehaviour, IDamageable
{
    [SerializeField]
    protected int health = 1;

    [SerializeField] 
    protected float timeBetweenDamages = .5f;

    [SerializeField] 
    protected SoundData[] hitSoundData = {};
    
    [SerializeField] 
    protected SoundData[] deathSoundData = {};

    [SerializeField] 
    private SoundData shieldBreakSoundData;
    

    public int Health
    {
        get => health;
        protected set { health = value; }
    }

    public int RespawnsLeft => maxRespawns - respawnCounter;
    public bool IsAlive => !isDead;
    public int DefaultHealth => defaultHealth;

    private bool hasShield = false;

    protected int respawnCounter = 0;
    
    [SerializeField]
    protected int maxRespawns = 3;

    [SerializeField] 
    protected GameObject explosionParticle;

    [SerializeField]
    private GameObject shieldParticle;

    [SerializeField]
    private GameObject shieldExplosionParticle;

    [SerializeField] 
    protected Animator animator;

#if UNITY_EDITOR
    [SerializeField]
    private bool godMode;
#endif

    private GameObject shieldExplosionParticleInstantiated;
    protected GameObject explosionParticleInstantiated;
    protected int defaultHealth;
    protected bool recentlyDamaged = false;
    protected bool isDead = false;
    protected Vector3 respawnPos;

    public delegate void OnDeath(BlakeCharacter blakeCharacter);
    public event OnDeath onDeath;
    public delegate void OnRespawn();
    public event OnRespawn onRespawn;
    public event Action<GameObject> OnDamageTaken;

    public virtual void Die(GameObject killer)
    {
        isDead = true;
        
        if (deathSoundData.Length > 0)
        {
            SoundEffectsManager.Instance.PlaySFX(deathSoundData[Random.Range(0, deathSoundData.Length)], transform.position);
        }
        
        if (respawnCounter >= maxRespawns)
        {
            ReferenceManager.LevelHandler.EndRun(false);
            return;
        }
        
        respawnCounter++;
        onDeath?.Invoke(this);
    }

    public virtual bool TryTakeDamage(GameObject instigator, int damage)
    {
#if UNITY_EDITOR
        if (godMode) return false;
#endif
        if (recentlyDamaged) return false;
        if (health < 1) return false;
        if(!CanTakeDamage(instigator)) return false;
        
        if(hasShield)
        {
            DeactivateShield();
            return false;
        }
        
        //Debug.Log(instigator.name + " dealt " + damage + " damage to " + name);
        Health -= damage;

        if (hitSoundData.Length > 0)
        {
            SoundEffectsManager.Instance.PlaySFX(hitSoundData[Random.Range(0, hitSoundData.Length)], transform.position);
        }
        
        OnDamageTaken?.Invoke(instigator);

        if (health > 0)
        {
            StartCoroutine(StopTakingDamageForPeriod(timeBetweenDamages));
        }
        else if (!isDead)
        {
            Die(instigator);
        }

        return true;
    }

    public virtual bool CanTakeDamage(GameObject instigator)
    {
        if (instigator == null) return false;

        BlakeCharacter other = instigator.GetComponent<BlakeCharacter>();
        if(other != null)
        {
            return GetType() != other.GetType();
        }

        return true;
    }

    public void AddRespawnCounter()
    {
        maxRespawns++;
    }

    public void ActivateShield()
    {
        hasShield = true;
        shieldParticle.gameObject.SetActive(true);

        if (shieldExplosionParticleInstantiated != null)
        {
            Destroy(shieldExplosionParticleInstantiated);
        }
    }

    public void DeactivateShield()
    {
        hasShield = false;
        shieldParticle.gameObject.SetActive(false);
        shieldExplosionParticleInstantiated = Instantiate(shieldExplosionParticle, transform.position, Quaternion.identity);
        SoundEffectsManager.Instance.PlaySFX(shieldBreakSoundData, transform.position);
    }

    public void SetRespawnPosition(Vector3 position)
    {
        respawnPos = position;
    }

    public Vector3 GetRespawnPosition()
    {
        return respawnPos;
    }

    private IEnumerator StopTakingDamageForPeriod(float period)
    {
        recentlyDamaged = true;
        yield return new WaitForSeconds(period);
        recentlyDamaged = false;
    }

    protected void Respawn()
    {
        isDead = false;
        Destroy(explosionParticleInstantiated);
        transform.position = respawnPos;
        gameObject.SetActive(true);
        GetComponent<CapsuleCollider>().enabled = true;
        health = defaultHealth;
        onRespawn?.Invoke();
    }

#if UNITY_EDITOR
    public void SetGodMode(bool isEnabled)
    {
        godMode = isEnabled;
    }
#endif
}
