using BepInEx;
using RoR2;
using RoR2.UI;
using RoR2.UI.LogBook;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

// I well and truly have no idea what I'm doing - I barely know any C# and my programming abilities are novice at best
// Relying on a lot of outside help here, llms, w3schools, geeksforgeeks

namespace Unlockify
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Main : BaseUnityPlugin
  {
    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "Nuxlar+Lucifish+Internet+Caffeine";
    public const string PluginName = "UnlockifySkinsOnlyTest";
    public const string PluginVersion = "1.0";

    internal static Main Instance { get; private set; }
    public static string PluginDirectory { get; private set; }

    /*
    I'm crossing my fingers that defining the collection of what is a skin outside of the awake function is safe and rational
    I don't actually have enough knowledge of class/instance management, or file structure in general, so. YOLO I guess.
    */
    
    public static readonly HashSet<UnlockableDef> SkinUnlocksHash = new HashSet<UnlockableDef>();
    // I don't know what the UnlockableDef type is or where it's coming from. I will let the wizard guide me and just hope it works out
    public static readonly HashSet<string> SkinSpecificUnlocksHash = new HashSet<string>();
    // I know there are other mods that add skins so I'm going to make this public and hope it doesn't explode

    public void Awake()
    {
      Instance = this;
      // I don't know what this; means and it scares me

      Stopwatch stopwatch = Stopwatch.StartNew();

      Log.Init(Logger);

      On.RoR2.Run.IsUnlockableUnlocked_UnlockableDef += TrueifyRun;
      On.RoR2.Run.IsUnlockableUnlocked_string += TrueifyRun2;
      On.RoR2.Run.DoesEveryoneHaveThisUnlockableUnlocked_UnlockableDef += TrueifyRunAll;
      On.RoR2.Run.DoesEveryoneHaveThisUnlockableUnlocked_string += TrueifyRunAll2;

      On.RoR2.UnlockableCatalog.GetUnlockableDef_string += PreventAchievement1;
      On.RoR2.UnlockableCatalog.GetUnlockableDef_UnlockableIndex += PreventAchievement2;

      On.RoR2.UserProfile.HasUnlockable_UnlockableDef += TrueifyProfile;
      On.RoR2.UserProfile.HasUnlockable_string += TrueifyProfile2;
      On.RoR2.UserProfile.HasDiscoveredPickup += TrueifyPickup;
      On.RoR2.UserProfile.HasSurvivorUnlocked += TrueifySurvivor;
      On.RoR2.UserProfile.HasAchievement += TrueifyAchievement;
      On.RoR2.UserProfile.CanSeeAchievement += TrueifyAchievement2;

      On.RoR2.Stats.StatSheet.HasUnlockable += TrueifyStatSheet;

      On.RoR2.PreGameController.AnyUserHasUnlockable += TrueifyPreGameController;

      On.RoR2.AchievementSystemSteam.AddAchievement += DontGrantSteamAchievement;
      On.RoR2.AchievementSystemEOS.AddAchievement += DontGrantEGSAchievement;

      On.RoR2.EclipseRun.GetLocalUserSurvivorCompletedEclipseLevel += GiveMaxEclipseLevel;
      On.RoR2.EclipseRun.GetNetworkUserSurvivorCompletedEclipseLevel += GiveMaxEclipseLevel2;

      stopwatch.Stop();
      Log.Info_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
    }

    private void FindBuildUnlockableSkins() {
      //my dumb ass would just wing it but the wizard machine is suggesting I do this so I will
      try {
        var listOfSkins = SkinCatalog.allSkinDefs;
        if (listOfSkins != null) { //no code explosions here no siree not today
          foreach (var skinLoopElem in listOfSkins) {
            var g = skinLoopElem?.unlockableDef;
            if (g != null)
            { //please don't explode
              SkinUnlocksHash.Add(g);
              if (!string.IsNullOrEmpty(g.cachedName)) SkinSpecificUnlocksHash.Add(g.cachedName);
              if (!string.IsNullOrEmpty(g.name)) SkinSpecificUnlocksHash.Add(g.name);
            }
          }
        }
      }
      catch { //I think this is just the fallback behavior if it doesn't work? I'm just going to use this to troll I guess
        Log.Info_NoCallerPrefix($"Luci you can't code dipshit moron your code doesn't work womp womp dumbass")
      }
    }

    private bool IsTheSkinUnlockable(UnlockableDef def) {
      return def != null
      && (SkinUnlocksHash.Contains(def) || SkinSpecificUnlocksHash.Contains(def.name) || SkinSpecificUnlocksHash.Contains(def.cachedName));
    }
    private bool IsTheSkinUnlockable(string name) {//We're well past the point where I don't know what's going on. Maybe it'll work maybe it wont
      if (string.IsNullOrEmpty(name)) return false;
      if (SkinSpecificUnlocksHash.Contains(name)) return true;
      var def = UnlockableCatalog.GetUnlockableDef(name);
      return IsTheSkinUnlockable(def);
    }
    private void DontGrantSteamAchievement(On.RoR2.AchievementSystemSteam.orig_AddAchievement orig, AchievementSystemSteam self, string achievementName)
    {

    }

    private void DontGrantEGSAchievement(On.RoR2.AchievementSystemEOS.orig_AddAchievement orig, AchievementSystemEOS self, string achievementName)
    {

    }

    private int GiveMaxEclipseLevel(On.RoR2.EclipseRun.orig_GetLocalUserSurvivorCompletedEclipseLevel orig, LocalUser localUser, SurvivorDef survivorDef)
    {
      return EclipseRun.maxEclipseLevel;
    }

    private int GiveMaxEclipseLevel2(On.RoR2.EclipseRun.orig_GetNetworkUserSurvivorCompletedEclipseLevel orig, NetworkUser networkUser, SurvivorDef survivorDef)
    {
      return EclipseRun.maxEclipseLevel;
    }

    private bool TrueifyStatSheet(On.RoR2.Stats.StatSheet.orig_HasUnlockable orig, RoR2.Stats.StatSheet self, UnlockableDef def)
    {
      return true;
    }

    private UnlockableDef PreventAchievement1(On.RoR2.UnlockableCatalog.orig_GetUnlockableDef_string orig, string defName)
    {
      return null;
    }

    private UnlockableDef PreventAchievement2(On.RoR2.UnlockableCatalog.orig_GetUnlockableDef_UnlockableIndex orig, UnlockableIndex idx)
    {
      return null;
    }

    private bool TrueifyRun(On.RoR2.Run.orig_IsUnlockableUnlocked_UnlockableDef orig, Run self, UnlockableDef unlockableDef)
    {
      if (NetworkServer.active)
        return true;
      Log.Warning("[Server] function 'Unlockify.TrueifyRun' called on client");
      return false;
    }

    private bool TrueifyRun2(On.RoR2.Run.orig_IsUnlockableUnlocked_string orig, Run self, string unlockableName)
    {
      if (NetworkServer.active)
        return true;
      Log.Warning("[Server] function 'Unlockify.TrueifyRun2' called on client");
      return false;
    }

    private bool TrueifyPreGameController(On.RoR2.PreGameController.orig_AnyUserHasUnlockable orig, UnlockableDef unlockableDef)
    {
      if (NetworkServer.active)
        return true;
      Log.Warning("[Server] function 'Unlockify.TrueifyPreGameController' called on client");
      return false;
    }

    private bool TrueifyRunAll(On.RoR2.Run.orig_DoesEveryoneHaveThisUnlockableUnlocked_UnlockableDef orig, Run self, UnlockableDef unlockableDef)
    {
      if (NetworkServer.active)
        return true;
      Log.Warning("[Server] function 'Unlockify.Trueify' called on client");
      return false;
    }

    private bool TrueifyRunAll2(On.RoR2.Run.orig_DoesEveryoneHaveThisUnlockableUnlocked_string orig, Run self, string unlockableName)
    {
      if (NetworkServer.active)
        return true;
      Log.Warning("[Server] function 'Unlockify.Trueify' called on client");
      return false;
    }

    private bool TrueifyProfile(On.RoR2.UserProfile.orig_HasUnlockable_UnlockableDef orig, UserProfile self, UnlockableDef unlockableDef)
    {
      return true;
    }

    private bool TrueifyProfile2(On.RoR2.UserProfile.orig_HasUnlockable_string orig, UserProfile self, string unlockableName)
    {
      return true;
    }

    private bool TrueifyAchievement(On.RoR2.UserProfile.orig_HasAchievement orig, UserProfile self, string achievementName)
    {
      return true;
    }

    private bool TrueifyAchievement2(On.RoR2.UserProfile.orig_CanSeeAchievement orig, UserProfile self, string achievementName)
    {
      return true;
    }

    private bool TrueifySurvivor(On.RoR2.UserProfile.orig_HasSurvivorUnlocked orig, UserProfile self, SurvivorIndex survivorIndex)
    {
      return true;
    }

    private bool TrueifyPickup(On.RoR2.UserProfile.orig_HasDiscoveredPickup orig, UserProfile self, PickupIndex pickupIndex)
    {
      return true;
    }
  }
}