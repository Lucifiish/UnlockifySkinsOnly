using BepInEx;
using RoR2;
using RoR2.UI;
using RoR2.UI.LogBook;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

namespace Unlockify
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Main : BaseUnityPlugin
  {
    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "Nuxlar";
    public const string PluginName = "Unlockify";
    public const string PluginVersion = "1.0.0";

    internal static Main Instance { get; private set; }
    public static string PluginDirectory { get; private set; }

    public void Awake()
    {
      Instance = this;

      Stopwatch stopwatch = Stopwatch.StartNew();

      Log.Init(Logger);

      On.RoR2.Run.IsUnlockableUnlocked_UnlockableDef += TrueifyRun;
      On.RoR2.Run.DoesEveryoneHaveThisUnlockableUnlocked_UnlockableDef += TrueifyRunAll;
      On.RoR2.UserProfile.HasUnlockable_UnlockableDef += TrueifyProfile;
      On.RoR2.UserProfile.HasDiscoveredPickup += TrueifyPickup;
      On.RoR2.UserProfile.HasSurvivorUnlocked += TrueifySurvivor;

      stopwatch.Stop();
      Log.Info_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
    }

    private bool TrueifyRun(On.RoR2.Run.orig_IsUnlockableUnlocked_UnlockableDef orig, Run self, UnlockableDef unlockableDef)
    {
      if (NetworkServer.active)
        return true;
      Log.Warning("[Server] function 'Unlockify.Trueify' called on client");
      return false;
    }

    private bool TrueifyRunAll(On.RoR2.Run.orig_DoesEveryoneHaveThisUnlockableUnlocked_UnlockableDef orig, Run self, UnlockableDef unlockableDef)
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

    private bool TrueifySurvivor(On.RoR2.UserProfile.orig_HasSurvivorUnlocked orig, UserProfile self, SurvivorIndex survivorIndex)
    {
      SurvivorDef survivorDef = SurvivorCatalog.GetSurvivorDef(survivorIndex);
      if (survivorDef)
        return true;

      return false;
    }

    private bool TrueifyPickup(On.RoR2.UserProfile.orig_HasDiscoveredPickup orig, UserProfile self, PickupIndex pickupIndex)
    {
      if (pickupIndex.isValid)
        return true;
      return false;
    }
  }
}