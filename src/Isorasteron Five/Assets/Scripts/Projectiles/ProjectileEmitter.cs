using System.Collections;
using Assets.Scripts.Audio;
using FMODUnity;
using TriInspector;
using UnityEngine;

namespace Assets.Scripts.Projectiles
{
    public class ProjectileEmitter : MonoBehaviour
    {
        [Header("References")]
        [AssetsOnly] public Projectile ProjectilePrefab;
        [SceneObjectsOnly] public Transform ProjectileOrigin;

        [Header("Emission")]
        [Tooltip("Base firing direction angle around the Y axis"), Unit("degrees")]
        public float Angle = 0f;
        [Unit("/sec")]
        [Range(0.001f, 10f)] public float FireRate = 0.2f;

        [Tooltip("Whether holding down attack continues firing or not")]
        public bool Automatic = true;

        [Tooltip("Percent modifier to fire rate each time you fire (for wind-ups / wind-downs)")]
        [ShowIf(nameof(Automatic))] public float FireRateAcceleration = 0f;
        [ShowIf(nameof(Automatic))] public float MaxFireRate = 1f;
        [ShowIf(nameof(Automatic))] public float MinFireRate = 0.2f;

        [Tooltip("Whether firing triggers multiple \"rounds\" of firing")]
        public bool Burst = false;
        [ShowIf(nameof(Burst)), Range(0, 10)] public float BurstAmount = 0f;
        [Tooltip("Delay between bursts")]
        [ShowIf(nameof(Burst)), Range(0, 1), Unit("secs")] public float BurstDelay = 0f;

        [Tooltip("How many extra projectiles are spawned, if multishot isn't a whole number then the remainder will add")]
        public float Multishot = 0f;
        [Tooltip("What angle in degrees the extra projectiles are spawned at")]
        public float MultishotAngle = 5f;

        [Header("Projectile")]
        [Range(0.1f, 10f)] public float ProjectileSizeMultiplier = 1f;
        [Unit("m/s")] public float ProjectileSpeed = 1f;
        [Unit("m/s²")] public float ProjectileAcceleration = 0f;
        [Unit("m/s")] public float MaxProjectileSpeed = 1f;
        [Unit("m/s")] public float MinProjectileSpeed = 1f;

        [Header("Sound")]
        [Tooltip("FMOD EventReference for when the emitter initially fires")]
        public EventReference OnEmitterFire;

        [Header("Runtime")]
        public int PoolInitialSize = 10;

        private bool heldFire;
        private float nextFireTime;
        private float currentFireRate;
        private float accumulatedMultishotRemander = 0f;
        private float accumulatedBurstRemander = 0f;
        private bool isBursting = false;

        private LazyPool<Projectile> pool;
        private GameObject parent;

        private void Awake()
        {
            if (ProjectilePrefab == null)
            {
                Debug.LogError("Projectile Emitter requires a projectile prefab", this);
                enabled = false;
                return;
            }

            if (MaxFireRate < MinFireRate)
            {
                Debug.LogWarning("Projectile Emitter MaxFireRate is less than MinFireRate!", this);
            }

            if (MaxProjectileSpeed < MinProjectileSpeed)
            {
                Debug.LogWarning("Projectile Emitter MaxProjectileSpeed is less than MinProjectileSpeed!", this);
            }

            currentFireRate = Mathf.Max(0.001f, FireRate);
            pool = new(ProjectilePrefab, PoolInitialSize, transform);
        }

        private void Update()
        {
            if (Automatic && heldFire)
            {
                TryFire();
            }
        }

        #region Public Methods
        [Button("Trigger Fire")]
        public void TriggerFire()
        {
            TryFire();
        }

        [Button("Toggle Fire")]
        public void ToggleFire() => ToggleFire(!heldFire);

        public void ToggleFire(bool state)
        {
            heldFire = state;
            if (!heldFire)
            {
                currentFireRate = FireRate;
            }
        }

        public void SetParent(GameObject parent)
        {
            this.parent = parent;
        }
        #endregion

        #region Private Methods
        private void TryFire()
        {
            if (Time.time < nextFireTime)
            {
                return;
            }

            nextFireTime = Time.time + (1f / currentFireRate);

            if (Burst)
            {
                if (!isBursting)
                {
                    StartCoroutine(FireBurst());
                }
            }
            else
            {
                Fire();
            }

            currentFireRate = Mathf.Clamp(currentFireRate + FireRateAcceleration, MinFireRate, MaxFireRate);
        }

        private void Fire()
        {
            float extraShots = Multishot + accumulatedMultishotRemander; // Multishot + Remainder = Extra shots
            accumulatedMultishotRemander = extraShots - Mathf.Floor(extraShots); // Accumulated is the non-whole remainder
            int totalShots = Mathf.FloorToInt(extraShots) + 1; // Total shots = extra + 1 base, all shots are emitted in an arc

            AudioManager.PlayOneShot(OnEmitterFire, nameof(OnEmitterFire));
            for (int i = 0; i < totalShots; i++)
            {
                float offsetAngle = Angle + ((i - ((totalShots - 1) / 2f)) * MultishotAngle);
                SpawnProjectile(offsetAngle);
            }           
        }

        private void SpawnProjectile(float spawnAngle)
        {
            Quaternion rotation = Quaternion.Euler(0, spawnAngle, 0);
            Vector3 direction = rotation * Vector3.forward;

            Projectile projectile;
            if (pool != null)
            {
                projectile = pool.Get();
                projectile.transform.SetPositionAndRotation(ProjectileOrigin != null ? ProjectileOrigin.position : transform.position, rotation);
                projectile.SetPoolReturnAction(projectile => pool.Return(projectile));
            }
            else
            {
                projectile = Instantiate(ProjectilePrefab, ProjectileOrigin != null ? ProjectileOrigin.position : transform.position, rotation);
                projectile.SetPoolReturnAction(null);
            }

            projectile.Initialize(direction, ProjectileSpeed, ProjectileSizeMultiplier, ProjectileAcceleration, MaxProjectileSpeed, MinProjectileSpeed, this, parent);
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                currentFireRate = FireRate;
            }

            int totalShots = Mathf.Max(1, 1 + Mathf.FloorToInt(Multishot));
            float half = (totalShots - 1) / 2f;
            for (int i = 0; i < totalShots; i++)
            {
                float offsetAngle = Angle + ((i - half) * MultishotAngle);
                Quaternion rotation = Quaternion.Euler(0f, offsetAngle, 0f);
                Vector3 direction = rotation * Vector3.forward;
                Gizmos.color = Color.yellow;
                Vector3 origin = ProjectileOrigin != null ? ProjectileOrigin.position : transform.position;
                Gizmos.DrawRay(origin, direction * 2f);
            }
        }

        private IEnumerator FireBurst()
        {
            isBursting = true;

            float extraShots = BurstAmount + accumulatedBurstRemander;
            accumulatedBurstRemander = extraShots - Mathf.Floor(extraShots);
            int totalShots = 1 + Mathf.FloorToInt(extraShots);

            for (int i = 0; i < totalShots; i++)
            {
                Fire();
                if (i < totalShots - 1)
                {
                    yield return new WaitForSeconds(BurstDelay);
                }
            }

            isBursting = false;
        }
        #endregion
    }
}
