﻿using System.Collections.Generic;
using Sandbox.Definitions;
using VRage.Collections;
using VRage.Game;
using VRage.Utils;

namespace AtmosphericDamage
{
    public static class Config
    {
        /////////////////////CHANGE THESE FOR EACH PLANET////////////////////////////

        public const string PLANET_NAME = "Tau_Verisia_A"; //Target Planet
        public const float LARGE_SHIP_DAMAGE = 2500f; //damage per update
        public const float SMALL_SHIP_DAMAGE = 2500f;
        public const string DAMAGE_STRING = PLANET_NAME + "Atmosphere";
        public const float PLAYER_DAMAGE_AMOUNT = 3f;

        public const double EMITTER_DRAW_DIST = 5000;
        public static bool DAMAGE_PLAYERS = true; //set true to apply atmospheric damage to players
        public static bool ACID_RAIN = true; //set true to apply damage only from above (like acid rain)
        public static bool DRAW_RAIN = true;
        public static bool RADIATION = true; //set true to apply passive radiation damage to all blocks in atmosphere
        public static bool VOXEL_DAMAGE = false; //allow certain voxel types to deal damage

        private static readonly List<VoxelDamageItem> VOXEL_DAMAGE_TYPES = new List<VoxelDamageItem>
                                                                           {
                                                                            //material name, particle effect ID, ship damage multiplier, character damage multiplier
                                                                               new VoxelDamageItem("RavLava", 213, 70, 10),
                                                                           };
        /////////////////////////////////////////////////////////////////////////////

        ///////////////////NEVER EVER CHANGE THESE///////////////////////////////////

        public const int UPDATE_RATE = 10; //damage will apply every 200 frames. MUST ALWAYS BE DIVISIBLE BY 10!
        public const int MAX_QUEUE = UPDATE_RATE * 8; //damage 4 objects per tick

        //message handler IDs
        //I literally just mashed my keyboard to get these
        public const long INIT_ID = 1684445163654733187;
        public const long INIT_INHIBIT_ID = 1684445163654733188;
        public const long DAMAGE_LIST_ID = 1684445163654733189;
        public const long DRAW_LIST_ID = 1684445163654733190;
        public const long PARTICLE_LIST_ID = 1684445163654733191;
        public const ushort NETWORK_ID = 51287;

        /////////////////////////////////////////////////////////////////////////////

        ///////////////////IGNORE THIS///////////////////////////////////////////////
        public static Dictionary<byte, VoxelDamageItem> VOXEL_IDS;

        public static void InitVoxelIDs()
        {
            VOXEL_IDS = new Dictionary<byte, VoxelDamageItem>();
            DictionaryValuesReader<string, MyVoxelMaterialDefinition> mats = MyDefinitionManager.Static.GetVoxelMaterialDefinitions();
            foreach (MyVoxelMaterialDefinition mat in mats)
            {
                foreach (VoxelDamageItem item in VOXEL_DAMAGE_TYPES)
                {
                    if (item.MaterialName == mat.MaterialTypeName)
                    {
                        VOXEL_IDS.Add(mat.Index, item);
                        MyLog.Default.WriteLine($"Added {mat.Index}:{mat.MaterialTypeName}");
                    }
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////
    }

    public class VoxelDamageItem
    {
        public float CharacterMultiplier;
        public float DamageMultiplier;
        public string MaterialName;
        //set null to disable particle effect
        public int? ParticleEffect;

        public VoxelDamageItem(string material, int? particleEffect, float damageMult, float characterMult)
        {
            MaterialName = material;
            ParticleEffect = particleEffect;
            DamageMultiplier = damageMult;
            CharacterMultiplier = characterMult;
        }
    }
}
