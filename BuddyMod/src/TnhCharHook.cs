using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx.Logging;
using Deli.Setup;
using FistVR;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.SceneManagement;
using AutoMeaterHitZone = On.FistVR.AutoMeaterHitZone;
using Damage = FistVR.Damage;
using FVRPhysicalObject = FistVR.FVRPhysicalObject;
using FVRViveHand = FistVR.FVRViveHand;
using PyroSplodeyPack = On.FistVR.PyroSplodeyPack;
using TNH_Manager = On.FistVR.TNH_Manager;

namespace Osa.MeatyceiverBuddyMod
{
    public class TnhCharHook
    {
        private readonly ManualLogSource _manualLogSource;

        private bool _enabled = false;

        private readonly string _idFilter;
        private readonly DeliBehaviour _behaviour;

        public TnhCharHook(ManualLogSource manualLogSource, string idFilter, DeliBehaviour behaviour)
        {
            _manualLogSource = manualLogSource;
            _idFilter = idFilter;
            _behaviour = behaviour;
            Hook();
        }

        public void Dispose()
        {
            Unhook();
        }

        public void Hook()
        {
            On.FistVR.TNH_Manager.SetPhase += OnTnh_ManagerOnSetPhase;
        }

        private void OnTnh_ManagerOnSetPhase(TNH_Manager.orig_SetPhase orig, FistVR.TNH_Manager self, TNH_Phase phase)
        {
            if (phase == TNH_Phase.Dead || phase == TNH_Phase.Completed)
            {
                UnhookChanges();
            }
            else
            {
                if (self.Phase == TNH_Phase.StartUp)
                {
                    if (self.C.TableID.Contains(_idFilter))
                    {
                        HookChanges();
                    }
                }
            }

            orig(self, phase);
        }

        public void Unhook()
        {
        }


        public void UnhookChanges()
        {
            if (!_enabled)
                return;
            
            _manualLogSource.LogWarning("UnHooking changes!");
            On.FistVR.PyroSplodeyPack.Damage -= OnPyroSplodeyPackOnDamage;
            On.FistVR.AutoMeaterHitZone.Damage -= OnAutoMeaterHitZoneOnDamage;
            _enabled = false;
        }

        public void HookChanges()
        {
            if (_enabled)
                return;

            _manualLogSource.LogWarning("Hooking changes!");
            On.FistVR.PyroSplodeyPack.Damage += OnPyroSplodeyPackOnDamage;
            On.FistVR.AutoMeaterHitZone.Damage += OnAutoMeaterHitZoneOnDamage;
            _enabled = true;
        }

        private void OnAutoMeaterHitZoneOnDamage(AutoMeaterHitZone.orig_Damage orig, FistVR.AutoMeaterHitZone self,
            Damage damage)
        {
            //Turrets weakpoints are slightly to resistant
            damage.Dam_TotalKinetic *= 2;
            _manualLogSource.LogWarning("Yup, multiplied dmg to turret");
            orig(self, damage);
        }

        private void OnPyroSplodeyPackOnDamage(PyroSplodeyPack.orig_Damage orig, FistVR.PyroSplodeyPack self,
            Damage damage)
        {
            //Pyros tank is too hard to destroy
            damage.Dam_TotalKinetic *= 3;
            _manualLogSource.LogWarning("Yup, multiplied dmg to tank");
            orig(self, damage);
        }
    }
}