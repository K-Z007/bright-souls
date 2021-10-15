﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Patterns.Observer;

namespace BrightSouls
{
    public enum WeaponType
    {
        Unarmed = 0,
        Sword = 1,
        Shield = 2,
        TwoHandedSword = 3
    }

    public class Weapon : MonoBehaviour, IHitboxOwner, IObserver
    {
        public WeaponType weaponType = WeaponType.Sword;

        public EquipmentSlotType slots = EquipmentSlotType.LeftHand | EquipmentSlotType.RightHand;

        private Attack[] attacks;
        private Attack currentAttack;

        private ICombatCharacter wielder;
        private Hitbox hitbox;

        public void OnNotification(object sender, Message msg, params object[] args)
        {
            var senderAsObj = (UnityEngine.Object)sender;
            var senderIsSelf = senderAsObj == this || sender == wielder || senderAsObj == wielder.transform.GetComponent<Animator>();
            if (!senderIsSelf)
                return;

            if (msg == Message.Combat_Hit)
            {
                var target = (IHittable)args[0];
                OnHit(target);
            }
            else if (msg == Message.Combat_WeaponActivateHitbox)
            {
                hitbox.IsActive = true;
            }
            else if (msg == Message.Combat_WeaponDeactivateHitbox)
            {
                hitbox.IsActive = false;
            }
        }

        public void OnAttack(int id)
        {
            if (id >= 0 && id < attacks.Length)
            {
                currentAttack = attacks[id];
                currentAttack.Activate(wielder);
            }
            else
                throw new System.ArgumentOutOfRangeException("id", "Attack with ID " + id.ToString() + " does not exist on weapon " + this.ToString());
        }

        public override string ToString()
        {
            return this.name + " (" + wielder + ")";
        }

        internal virtual void OnHit(IHittable target)
        {
            if ((target as ICharacter) != wielder)
            {
                target.OnGetHit(currentAttack);
            }
        }

        private void Start()
        {
            this.Observe(Message.Combat_Hit);
            this.Observe(Message.Combat_WeaponActivateHitbox);
            this.Observe(Message.Combat_WeaponDeactivateHitbox);
            AssignComponents();
        }

        private void AssignComponents()
        {
            wielder = GetComponentInParent<ICombatCharacter>();
            hitbox = GetComponentInChildren<Hitbox>();
            attacks = GetComponents<Attack>();

            if (wielder == null)
            {
                Debug.LogError("ERROR: Weapon has no wielder!");
                enabled = false;
            }
            else if (hitbox == null)
            {
                Debug.LogError("ERROR: Weapon has no hitbox!");
                enabled = false;
            }
        }
    }
}