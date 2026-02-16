using System;
using System.Collections.Generic;
using Assets.Scripts.Audio;
using Assets.Scripts.Entities;
using FMODUnity;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Projectiles
{
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile")]
        [Tooltip("Whether the projectile will be destroyed upon contacting a wall or an enemy")]
        public bool DestroyOnHit = true;
        [Tooltip("Whether the projectile will be destroyed upon contacting a wall or an enemy")]
        public bool Bounces = false;
        [ShowIf(nameof(Bounces))] public bool BouncesAffectDurability = false;
        [Tooltip("Which layers the projectile will bounce off of")]
        [ShowIf(nameof(Bounces))] public LayerMask BounceLayers;
        [ShowIf(nameof(Bounces)), Range(0f, 1f)] public float BounceEnergyRetention = 1f;

        [Tooltip("How long the projectile lasts before expiring, in seconds"), Unit("secs")]
        [Range(0.1f, 100f)] public float Lifetime = 5f;
        [Tooltip("How many hits until the projectile is destroyed")]
        [ShowIf(nameof(DestroyOnHit)), Range(1, 999)] public int Durability = 1;

        [Header("Physics")]
        [Tooltip("Enable continuous collision detection for fast projectiles")]
        public bool UseContinuousCollision = true;
        public bool IgnoreSelf = true;

        [Header("Events")]
        public UnityEvent<Projectile, Collision> OnHit = new UnityEvent<Projectile, Collision>();
        public UnityEvent<Projectile, Collision> OnBounce = new UnityEvent<Projectile, Collision>();
        public UnityEvent<Projectile> OnExpire = new UnityEvent<Projectile>();
        public UnityEvent OnDestroyed = new UnityEvent();

        [Header("Audio")]
        [Tooltip("FMOD EventReference for when the projectile hits something")]
        public EventReference ProjectileHit;
        [Tooltip("FMOD EventReference for when the projectile is initially fired")]
        public EventReference ProjectileFired;
        [Tooltip("FMOD EventReference for when the projectile reaches the end of its lifetime")]
        public EventReference ProjectileExpire;
        [Tooltip("FMOD EventReference for when the projectile is destroyed")]
        public EventReference ProjectileDestroyed;
        [Tooltip("FMOD EventReference for when the projectile bounces")]
        public EventReference ProjectileBounce;

        private ProjectileEmitter owner;
        private GameObject source;
        private Action<Projectile> returnToPool;

        private new Rigidbody rigidbody;
        private new Collider collider;

        private Vector3 direction;
        private float speed;
        private float acceleration;
        private float maxSpeed;
        private float minSpeed;
        private int currentDurability;
        private Vector3 initialLocalScale;

        private List<Collider> ignoredColliders;

        // NEW: damage stored on projectile
        private int damage;

        #region Unity
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                Debug.LogError("Projectile requires a Rigidbody!", this);
            }

            collider = GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogError("Projectile requires a Collider!", this);
            }

            initialLocalScale = transform.localScale;
        }

        private void FixedUpdate()
        {
            UpdateSpeed();
        }

        private void OnCollisionEnter(Collision collision)
        {
            bool isBounceLayer = Bounces && ((BounceLayers.value & (1 << collision.gameObject.layer)) != 0);

            if (isBounceLayer)
            {
                // If the bounce would kill the projectile, handle that first
                if (BouncesAffectDurability && DestroyOnHit)
                {
                    if (!HandleHit())
                    {
                        return;
                    }
                }

                ApplyBounce(collision);
                OnBounce?.Invoke(this, collision);
                AudioManager.PlayOneShot(ProjectileBounce, nameof(ProjectileBounce));
                return;
            }

            Entity hitEntity = collision.collider.GetComponentInParent<Entity>();
            if (hitEntity != null)
            {
                if (!IsSourceOrChildOf(hitEntity.gameObject, source))
                {
                    hitEntity.HurtEntity(damage);
                }
            }

            OnHit?.Invoke(this, collision);

            if (DestroyOnHit)
            {
                HandleHit();
            }
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(HandleExpire));
            if (rigidbody != null)
            {
                rigidbody.linearVelocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }
        }
        #endregion

        #region Public Members
        public void Initialize(Vector3 direction, float speed, float sizeMultiplier, float acceleration, float maxSpeed, float minSpeed, int damage, ProjectileEmitter owner, GameObject source = null, Quaternion? spawnRotation = null)
        {
            transform.localScale = initialLocalScale * sizeMultiplier;

            if (spawnRotation.HasValue)
            {
                transform.rotation = spawnRotation.Value;
            }

            this.direction = direction.normalized;
            this.acceleration = acceleration;
            this.maxSpeed = maxSpeed;
            this.minSpeed = minSpeed;
            this.speed = speed;
            this.owner = owner;
            this.source = source;
            this.damage = Math.Max(0, damage);

            currentDurability = Durability;

            CancelInvoke(nameof(HandleExpire));
            Invoke(nameof(HandleExpire), Lifetime);

            if (rigidbody != null)
            {
                rigidbody.collisionDetectionMode = UseContinuousCollision ? CollisionDetectionMode.ContinuousDynamic : CollisionDetectionMode.Discrete;
                rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                rigidbody.linearVelocity = direction * speed;
            }

            IgnoreSourceCollisions();
            gameObject.SetActive(true);

            AudioManager.PlayOneShot(ProjectileFired, nameof(ProjectileFired));
        }

        public void SetPoolReturnAction(Action<Projectile> returnAction) => returnToPool = returnAction;
        #endregion

        #region Private Members
        /// <summary>
        /// Make the projectile ignore colliding with the source object
        /// </summary>
        private void IgnoreSourceCollisions()
        {
            if (!IgnoreSelf || source == null || collider == null)
            {
                return;
            }

            if (ignoredColliders == null)
            {
                ignoredColliders = new List<Collider>(4);
            }
            else
            {
                ignoredColliders.Clear();
            }

            if (source.TryGetComponent(out Collider sourceCollider))
            {
                ignoredColliders.Add(sourceCollider);
            }

            Collider[] children = source.GetComponentsInChildren<Collider>();
            if (children != null && children.Length > 0)
            {
                ignoredColliders.AddRange(children);
            }

            foreach (Collider ignored in ignoredColliders)
            {
                if (ignored != null)
                {
                    Physics.IgnoreCollision(collider, ignored);
                }
            }
        }

        private void ClearIgnoredCollisions()
        {
            if (ignoredColliders == null || collider == null)
            {
                return;
            }

            foreach (Collider ignored in ignoredColliders)
            {
                if (ignored == null)
                {
                    continue;
                }

                Physics.IgnoreCollision(collider, ignored, false);
            }

            ignoredColliders.Clear();
        }

        /// <summary>
        /// Updates the position of the gameobject given direction and speed, optionally updates speed with acceleration too
        /// </summary>
        private void UpdateSpeed(bool increaseAcceleration = true)
        {
            if (increaseAcceleration && acceleration != 0f)
            {
                speed += acceleration * Time.fixedDeltaTime;
                speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
            }

            if (rigidbody != null)
            {
                rigidbody.linearVelocity = direction * speed;
            }
            else
            {
                transform.position += speed * Time.fixedDeltaTime * direction;
            }
        }

        /// <summary>
        /// Called when a projectile hits something
        /// </summary>
        private bool HandleHit()
        {
            AudioManager.PlayOneShot(ProjectileHit, nameof(ProjectileHit));

            currentDurability = Mathf.Max(0, currentDurability - 1);
            if (currentDurability <= 0)
            {
                DestroySelf();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called when a projectile reaches the end of its lifespan
        /// </summary>
        private void HandleExpire()
        {
            OnExpire?.Invoke(this);
            AudioManager.PlayOneShot(ProjectileExpire, nameof(ProjectileExpire));
            DestroySelf();
        }

        /// <summary>
        /// Either returns the projectile to the pool it came from, or destroys it (if no return action was specified)
        /// </summary>
        private void DestroySelf()
        {
            OnDestroyed?.Invoke();
            AudioManager.PlayOneShot(ProjectileDestroyed, nameof(ProjectileDestroyed));

            if (returnToPool != null)
            {
                CancelInvoke(nameof(HandleExpire));
                ClearIgnoredCollisions();
                returnToPool(this);
            }
            else
            {
                ClearIgnoredCollisions();
                Destroy(gameObject);
            }
        }

        private void ApplyBounce(Collision collision)
        {
            Vector3 normal = collision.contacts.Length > 0 ? collision.contacts[0].normal : -collision.transform.forward;
            direction = Vector3.Reflect(direction, normal).normalized;
            speed *= BounceEnergyRetention;
            UpdateSpeed(false);
        }

        /// <summary>
        /// Return true if candidate is the source or a child of the source
        /// </summary>
        private bool IsSourceOrChildOf(GameObject candidate, GameObject possibleSource)
        {
            if (possibleSource == null || candidate == null)
            {
                return false;
            }

            if (candidate == possibleSource)
            {
                return true;
            }

            if (candidate.transform.IsChildOf(possibleSource.transform))
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}