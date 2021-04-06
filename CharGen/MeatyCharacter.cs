using System;
using System.Collections.Generic;

namespace CharGen
{
    public class MeatyCharacter
    {
        [ReplaceValue()]
        public string Name;
        [ReplaceValue()]
        public string NameId;
        [ReplaceValue()]
        public string Description;
        [ReplaceValue()]
        public Version Version;
        [ReplaceValue()]
        public string SignatureWeaponId;
        [ReplaceValue()]
        public string SignatureMeleeId;
        /// <summary>
        /// Amount of mags you start with, also the amount of the magazines that will be dropped after a Replicant will be defeated.
        /// </summary>
        [ReplaceValue()]
        public int StartingMagsAmount;

        [ReplaceValue()]
        public int MagCost;
        
        [ReplaceValue()]
        public float AmmoDropAmountMultiplier;
        
        [ReplaceValue(allowFloat:true)]
        public float AmmoDropChanceMultiplier;
        /// <summary>
        /// List of supported magazines Ids.
        /// Important note: Giving high capacity magazines will break balance of this character, use with care.
        /// </summary>
        [ReplaceValue()]
        public List<string> MagazineIds = new List<string>();
        /// <summary>
        /// Five ammo tiers, describing the progression of ammo.
        /// If less than 5 is defined, last one will be repeated till end.
        /// If more than 5, it will throw an error on generation.
        /// </summary>
        [ReplaceValue(asNumbered:true)]
        public List<string> AmmoIdTiers= new List<string>();
        /// <summary>
        /// Multiplies amount of ammo you get from shops cheap and premium ammo pools.
        /// </summary>
        [ReplaceValue()]
        public float AmmoShopMultiplier;
        /// <summary>
        /// Multiplies amount of hp a sosig will have, only works for combat oriented sosigs
        /// </summary>
        [ReplaceValue(allowFloat:true)]
        public float SosigToughnessModifier;
        /// <summary>
        /// Multiplies amount walking and running speed of each sosig
        /// </summary>
        [ReplaceValue(allowFloat:true)]
        public float SosigSpeedModifier;
        /// <summary>
        /// Multiplies amount of encryptions to spawn per wave
        /// </summary>
        [ReplaceValue()]
        public float EncryptionsModifier;
        /// <summary>
        /// This additionally replaces optional places in the template with desired values.
        /// </summary>
        [ReplaceAdditions()]
        public Dictionary<string, string> AdditionalOptions= new Dictionary<string, string>();

        /// <summary>
        /// Points to the directory with additional files to be included
        /// </summary>
        public string AdditionalContentPath;
    }
}