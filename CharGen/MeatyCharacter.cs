using System;
using System.Collections.Generic;

namespace CharGen
{
    public class MeatyCharacter
    {
        public string Name;
        public string Description;
        public Version Version;
        public string SignatureWeaponId;
        public string SignatureMeleeId;
        /// <summary>
        /// Amount of mags you start with, also the amount of the magazines that will be dropped after a Replicant will be defeated.
        /// </summary>
        public int StartingMagsAmount;
        /// <summary>
        /// List of supported magazines Ids.
        /// Important node: Giving high capacity magazines will break balance of this character, use with care.
        /// </summary>
        public List<string> MagazineIds = new List<string>();
        /// <summary>
        /// Five ammo tiers, describing the progression of ammo.
        /// If less than 5 is defined, last one will be repeated till end.
        /// If more than 5, it will throw an error on create.
        /// </summary>
        public List<string> AmmoIdTiers= new List<string>();
        /// <summary>
        /// Multiplies amount of ammo you get from shops cheap and premium ammo pools.
        /// </summary>
        public float AmmoShopMultiplier;
        /// <summary>
        /// Multiplies amount of hp a sosig will have, only works for combat oriented sosigs
        /// </summary>
        public float SosigHpModifier;
        /// <summary>
        /// Multiplies amount walking and rnning speed of each sosig
        /// </summary>
        public float SosigSpeedModifier;
        /// <summary>
        /// Multiplies amount of encryptions to spawn per wave
        /// </summary>
        public float EncryptionsModifier;
        /// <summary>
        /// This additionally replaces optional places in the template with desired values.
        /// </summary>
        public Dictionary<string, string> AdditionalOptions= new Dictionary<string, string>();

        /// <summary>
        /// Points to the directory with additional files to be included
        /// </summary>
        public string AdditionalContentPath;
    }
}