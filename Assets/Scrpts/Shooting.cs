using UnityEngine;

public enum WeaponType
{
    Pistol,
    MachineGun,
    Shotgun,
    Katana
}

public class Shooting : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public LayerMask hitLayers;
    public CameraEffects cameraEffects;
    
    [Header("Current Weapon")]
    public WeaponType currentWeapon = WeaponType.Pistol;
    
    [Header("Pistol Settings")]
    public float pistolDamage = 25f;
    public float pistolRange = 100f;
    public float pistolFireRate = 4f;        // Shots per second
    public AudioClip pistolSound;
    [Range(0f, 1f)] public float pistolVolume = 0.7f;
    
    [Header("Machine Gun Settings")]
    public float mgDamage = 10f;
    public float mgRange = 80f;
    public float mgFireRate = 12f;           // Shots per second
    public float mgSpread = 3f;              // Spread angle
    public AudioClip mgSound;
    [Range(0f, 1f)] public float mgVolume = 0.5f;
    
    [Header("Shotgun Settings")]
    public float shotgunDamage = 15f;        // Per pellet
    public float shotgunRange = 30f;
    public float shotgunFireRate = 1.2f;     // Shots per second
    public int shotgunPellets = 8;
    public float shotgunSpread = 10f;
    public AudioClip shotgunSound;
    [Range(0f, 1f)] public float shotgunVolume = 0.8f;
    
    [Header("Katana Settings")]
    public float katanaDamage = 50f;
    public float katanaRange = 3f;           // Melee range
    public float katanaFireRate = 2f;        // Slashes per second
    public float katanaArc = 60f;            // Slash arc angle
    public AudioClip katanaSound;
    [Range(0f, 1f)] public float katanaVolume = 0.6f;
    
    [Header("Effects")]
    public GameObject hitEffectPrefab;
    public GameObject slashEffectPrefab;
    
    private float nextFireTime = 0f;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }
    
    void Update()
    {
        HandleWeaponSwitch();
        HandleShooting();
    }
    
    void HandleWeaponSwitch()
    {
        // Number keys to switch weapons
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentWeapon = WeaponType.Pistol;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentWeapon = WeaponType.MachineGun;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentWeapon = WeaponType.Shotgun;
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentWeapon = WeaponType.Katana;
    }
    
    void HandleShooting()
    {
        if (Time.time < nextFireTime) return;
        
        bool shouldFire = false;
        
        // Machine gun is full auto, others are semi-auto
        if (currentWeapon == WeaponType.MachineGun)
        {
            shouldFire = Input.GetButton("Fire1");
        }
        else
        {
            shouldFire = Input.GetButtonDown("Fire1");
        }
        
        if (shouldFire)
        {
            Fire();
        }
    }
    
    void Fire()
    {
        switch (currentWeapon)
        {
            case WeaponType.Pistol:
                FirePistol();
                break;
            case WeaponType.MachineGun:
                FireMachineGun();
                break;
            case WeaponType.Shotgun:
                FireShotgun();
                break;
            case WeaponType.Katana:
                SlashKatana();
                break;
        }
    }
    
    // ==================== PISTOL ====================
    void FirePistol()
    {
        nextFireTime = Time.time + (1f / pistolFireRate);
        
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;
        
        ShootRay(origin, direction, pistolRange, pistolDamage);
        
        PlaySound(pistolSound, pistolVolume);
        ApplyRecoil(0.05f);
    }
    
    // ==================== MACHINE GUN ====================
    void FireMachineGun()
    {
        nextFireTime = Time.time + (1f / mgFireRate);
        
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = ApplySpread(playerCamera.transform.forward, mgSpread);
        
        ShootRay(origin, direction, mgRange, mgDamage);
        
        PlaySound(mgSound, mgVolume);
        ApplyRecoil(0.02f);
    }
    
    // ==================== SHOTGUN ====================
    void FireShotgun()
    {
        nextFireTime = Time.time + (1f / shotgunFireRate);
        
        Vector3 origin = playerCamera.transform.position;
        
        // Fire multiple pellets
        for (int i = 0; i < shotgunPellets; i++)
        {
            Vector3 direction = ApplySpread(playerCamera.transform.forward, shotgunSpread);
            ShootRay(origin, direction, shotgunRange, shotgunDamage);
        }
        
        PlaySound(shotgunSound, shotgunVolume);
        ApplyRecoil(0.15f);
    }
    
    // ==================== KATANA ====================
    void SlashKatana()
    {
        nextFireTime = Time.time + (1f / katanaFireRate);
        
        Vector3 origin = playerCamera.transform.position;
        
        // Spawn slash effect
        if (slashEffectPrefab != null)
        {
            GameObject slash = Instantiate(
                slashEffectPrefab,
                origin + playerCamera.transform.forward * 1f,
                playerCamera.transform.rotation
            );
            Destroy(slash, 0.5f);
        }
        
        // SphereCast for melee hit detection
        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            1f,                              // Radius
            playerCamera.transform.forward,
            katanaRange,
            hitLayers
        );
        
        foreach (RaycastHit hit in hits)
        {
            // Check if within slash arc
            Vector3 toTarget = (hit.point - origin).normalized;
            float angle = Vector3.Angle(playerCamera.transform.forward, toTarget);
            
            if (angle <= katanaArc / 2f)
            {
                OnHit(hit, katanaDamage);
            }
        }
        
        PlaySound(katanaSound, katanaVolume);
        ApplyRecoil(0.03f);
    }
    
    // ==================== SHARED FUNCTIONS ====================
    void ShootRay(Vector3 origin, Vector3 direction, float range, float damage)
    {
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, range, hitLayers))
        {
            OnHit(hit, damage);
        }
    }
    
    void OnHit(RaycastHit hit, float damage)
    {
        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                hitEffectPrefab,
                hit.point,
                Quaternion.LookRotation(hit.normal)
            );
            Destroy(effect, 2f);
        }
        
        // Deal damage to enemies
        if (hit.collider.CompareTag("enemy"))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
    
    Vector3 ApplySpread(Vector3 direction, float spreadAngle)
    {
        float spreadX = Random.Range(-spreadAngle, spreadAngle);
        float spreadY = Random.Range(-spreadAngle, spreadAngle);
        
        Quaternion spreadRotation = Quaternion.Euler(spreadY, spreadX, 0f);
        return spreadRotation * direction;
    }
    
    void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
    
    void ApplyRecoil(float intensity)
    {
        if (cameraEffects != null)
        {
            cameraEffects.ApplyCameraShake(intensity, 0.1f);
        }
    }
    
    // ==================== PUBLIC METHODS ====================
    public void SetWeapon(WeaponType weapon)
    {
        currentWeapon = weapon;
    }
    
    public WeaponType GetCurrentWeapon()
    {
        return currentWeapon;
    }
}
