﻿using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.Healing;
using Gameplay.GameControllers.Entities;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace Blasphemous.LostDreams.Effects;

/// <summary>
/// Lose it when getting hit, regain it when dead or prie dieu
/// </summary>
internal class DamageRemoval : IToggleEffect
{
    private bool _alreadyUsed = false;

    public bool IsActive => !_alreadyUsed && Main.LostDreams.ItemHandler.IsEquipped("RB503");

    public DamageRemoval()
    {
        Main.LostDreams.EventHandler.OnPlayerKilled += RegainDamageRemoval;
        Main.LostDreams.EventHandler.OnUsePrieDieu += RegainDamageRemoval;
        Main.LostDreams.EventHandler.OnExitGame += RegainDamageRemoval;
        Main.LostDreams.EventHandler.OnPlayerDamaged += PlayerTakeDamage;
    }

    private void RegainDamageRemoval()
    {
        _alreadyUsed = false;
    }

    private void PlayerTakeDamage(ref Hit hit)
    {
        if (!Main.LostDreams.DamageRemoval.IsActive)
            return;

        Main.LostDreams.Log("RB503: Preventing damage");
        hit.DamageAmount = 0;
        _alreadyUsed = true;

        Healing_Start_Patch.HealingFlag = true;
        Object.FindObjectOfType<HealingAura>()?.StartAura(Core.Logic.Penitent.Status.Orientation);
        Core.Logic.Penitent.Audio.PrayerInvincibility();
    }
}

// Show blue aura when damage is prevented
[HarmonyPatch(typeof(HealingAura), "StartAura")]
class Healing_Start_Patch
{
    public static void Postfix(HealingAura __instance, Animator ____auraAnimator, SpriteRenderer ____auraRenderer)
    {
        if (HealingFlag)
        {
            HealingFlag = false;
            ____auraRenderer.color = new Color(0.139f, 0.459f, 0.557f);
            ____auraAnimator.Play(0, 0, 0.28f);
            __instance.StartCoroutine(TurnOffHealing());
        }

        IEnumerator TurnOffHealing()
        {
            yield return new WaitForSeconds(1.8f);
            __instance.StopAura();
            yield return new WaitForEndOfFrame();
            ____auraRenderer.color = Color.white;
        }
    }

    public static bool HealingFlag { get; set; }
}