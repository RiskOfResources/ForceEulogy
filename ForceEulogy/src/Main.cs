using RoR2;
using R2API;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using UnityEngine.AddressableAssets;
using RiskOfOptions;
using RiskOfOptions.Options;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace PodRacing;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]

public class ForceEulogy : BaseUnityPlugin
{
  public const string PluginGUID = PluginAuthor + "." + PluginName;
  public const string PluginAuthor = "RiskOfResources";
  public const string PluginName = "ForceEulogy";
  public const string PluginVersion = "1.1.1";

  public static ConfigEntry<int> amount { get; set; }
  public static ConfigEntry<BooleanChoice> removeFromPool { get; set; }

  public static ArtifactDef podRacing = ScriptableObject.CreateInstance<ArtifactDef>();
  public static bool podRacingEnabled = false;
  public static AssetBundle assets;
  public static Sprite artifactOnIcon;
  public static Sprite artifactOffIcon;
  private static ItemIndex[] cleansableItems = Array.Empty<ItemIndex>();
  public ItemDef beads = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/LunarTrinket/LunarTrinket.asset").WaitForCompletion();
  public ItemDef eulogy = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/RandomlyLunar/RandomlyLunar.asset").WaitForCompletion();    

  public void Awake()
  {
    InitConfig();
    LoadSprites();
    DefineArtifact();
    eulogy.tier = ItemTier.NoTier;
    #pragma warning disable 0612, 0618
    eulogy.deprecatedTier = ItemTier.NoTier;
    #pragma warning restore 0612, 0618
    if (Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
    {
      ModSettingsManager.AddOption(new IntSliderOption(amount));
      ModSettingsManager.AddOption(new ChoiceOption(removeFromPool));
    }
    
    On.RoR2.Run.Start += (orig, self) =>
    {
      orig(self);
      podRacingEnabled = RunArtifactManager.instance.IsArtifactEnabled(podRacing);
      if (podRacingEnabled)
      {
        eulogy.tier = ItemTier.NoTier;
        #pragma warning disable 0612, 0618
        eulogy.deprecatedTier = ItemTier.NoTier;
        #pragma warning restore 0612, 0618
        PlayerCharacterMasterController._instances[0].master.inventory.GiveItem(eulogy.itemIndex, amount.Value);
      }
      ItemDef eulogyItemDef = ItemCatalog.GetItemDef(eulogy.itemIndex);
    };

    On.RoR2.Run.RefreshLunarCombinedDropList += ( orig, self ) =>
    {
      orig(self);
      podRacingEnabled = RunArtifactManager.instance.IsArtifactEnabled(podRacing);
      if (podRacingEnabled)
      {
        if (removeFromPool.Value == BooleanChoice.True)
        {
          PickupIndex eulogyPickup = PickupCatalog.FindPickupIndex(eulogy.itemIndex);
          PickupIndex beadsPickup = PickupCatalog.FindPickupIndex(beads.itemIndex);
          self.availableLunarItemDropList.Remove(eulogyPickup);
          self.availableLunarItemDropList.Remove(beadsPickup);
          self.availableLunarCombinedDropList.Remove(eulogyPickup);
          self.availableLunarCombinedDropList.Remove(beadsPickup);
        }
      }
    };

    On.RoR2.Run.OnDestroy += (orig, self) =>
    {
      orig(self);
      podRacingEnabled = false;
    };
  }

  public void InitConfig()
  {
    amount = Config.Bind(
      "General"
    , "Amount"
    , 2
    , "The amount of Eulogy Zero to insert into the player's inventory on run start."
    );
    
    removeFromPool = Config.Bind(
      "General"
    , "Remove Eulogy"
    , BooleanChoice.True
    , "Set to true to remove Eulogy Zero from the list of available lunars."
    );
  }

  public void LoadSprites()
  {
    assets = AssetBundle.LoadFromFile(System.IO.Path.Combine(Paths.PluginPath, "RiskOfResources-ForceEulogy/artifactofpower"));
    artifactOnIcon = assets.LoadAsset<Sprite>("artifactOn.png");
    artifactOffIcon = assets.LoadAsset<Sprite>("artifactOff.png");
  }

  public void DefineArtifact()
  {
    podRacing.cachedName = "Artifact of Power";
    podRacing.nameToken = "Artifact of Power";
    podRacing.descriptionToken = "Converts a percentage of items in a run to lunar items.";
    podRacing.smallIconSelectedSprite = artifactOnIcon;
    podRacing.smallIconDeselectedSprite = artifactOffIcon;
    podRacing.pickupModelPrefab = LegacyResourcesAPI.Load<ArtifactDef>("artifactdefs/MixEnemy").pickupModelPrefab;
    ContentAddition.AddArtifactDef(podRacing);
  }

  public enum BooleanChoice
  {
    True,
    False
  }
}