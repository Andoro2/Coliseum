using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona qué pasivas están activas en el jugador según su nivel actual.
/// Se encarga de aplicar las nuevas pasivas al subir de nivel.
/// Añadir al prefab base del jugador junto a PlayerStats.
/// </summary>
public class PassiveManager : MonoBehaviour
{
    private PlayerController playerController;
    private PlayerStats playerStats;
    //private CharacterData characterData;
    //private HashSet<PassiveEffect> appliedPassives = new HashSet<PassiveEffect>();

    /// <summary>
    /// Llamar desde Start() del PlayerController pasándole el CharacterData del personaje elegido.
    /// </summary>
    /*public void Initialize(CharacterData data)
    {
        characterData = data;
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();

        // Aplica las pasivas del nivel 1 al iniciar
        CheckAndApplyPassives(1);
    }*/

    /// <summary>
    /// PlayerStats lo llama automáticamente al subir de nivel.
    /// </summary>
    public void OnLevelUp(int newLevel)
    {
        CheckAndApplyPassives(newLevel);
    }

    private void CheckAndApplyPassives(int level)
    {
        //if (characterData == null) return;

        // Pasiva propia del personaje (siempre activa desde Nv 1)
        //TryApply(characterData.characterPassive);

        // Desbloqueo de la definitiva a nivel 4
        if (level >= 4)
            UnlockUltimate();

        // Pasivas de raza
        //foreach (PassiveEffect p in characterData.race.GetAllUnlockedPassives(level))
            //TryApply(p);

        // Pasivas de clase
        //foreach (PassiveEffect p in characterData.classData.GetAllUnlockedPassives(level))
            //TryApply(p);
    }

    /*private void TryApply(PassiveEffect passive)
    {
        if (passive == null) return;
        if (appliedPassives.Contains(passive)) return; // ya aplicada, no duplicar

        passive.Apply(playerController);
        appliedPassives.Add(passive);
    }*/

    private void UnlockUltimate()
    {
        // TODO: conectar con AbilityManager cuando esté implementado
        // GetComponent<AbilityManager>()?.UnlockUltimate(characterData.ultimate);
    }

    /// <summary>
    /// Revierte todas las pasivas aplicadas. Útil al cambiar de personaje.
    /// </summary>
    /*public void RemoveAllPassives()
    {
        foreach (PassiveEffect p in appliedPassives)
            p.Remove(playerController);
        appliedPassives.Clear();
    }*/
}