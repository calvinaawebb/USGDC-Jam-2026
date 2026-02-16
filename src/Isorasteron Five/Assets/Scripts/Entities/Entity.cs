using System;
using Assets.Scripts.Audio;
using FMODUnity;
using UnityEngine;

namespace Assets.Scripts.Entities
{
    public class Entity : MonoBehaviour
    {
        public int Health;
        public float MovementSpeed = 1f;

        [Header("Audio")]
        public EventReference OnHurt;
        public EventReference OnKill;

        private new Collider collider;

        private void Awake()
        {
            collider = GetComponent<Collider>();
        }

        public void HurtEntity(int amount)
        {
            amount = Math.Max(0, amount);
            int netHealth = Health - amount;
            if (netHealth <= 0)
            {
                Health = 0;
                KillEntity();
            }
            else
            {
                Health = netHealth;
                AudioManager.PlayOneShot(OnHurt, nameof(OnHurt));
            }
        }

        public void KillEntity()
        {
            AudioManager.PlayOneShot(OnKill, nameof(OnKill));
            // todo
        }
    }
}
