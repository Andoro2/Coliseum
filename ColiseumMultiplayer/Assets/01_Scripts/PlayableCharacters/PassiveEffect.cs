using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase base abstracta para todas las pasivas del juego.
/// NO se puede crear directamente desde el menú de Unity porque es abstracta.
/// Para crear una pasiva nueva, crea un script que herede de esta clase
/// y sobreescribe Apply().
/// 
/// Ejemplo:
///     public class StatModifierPassive : PassiveEffect { ... }
///     public class ResistancePassive : PassiveEffect { ... }
/// </summary>
public abstract class PassiveEffect : ScriptableObject
{
    [Header("Info")]
    public string passiveName;
    [TextArea] public string description;

    /// <summary>
    /// Se llama cuando el jugador desbloquea esta pasiva al alcanzar el nivel requerido.
    /// Aquí va la lógica de aplicar el efecto al jugador.
    /// </summary>
    public abstract void Apply(PlayerController player);

    /// <summary>
    /// Se llama si la pasiva necesita ser revertida, por ejemplo al cambiar de personaje.
    /// No todas las pasivas necesitan implementarlo, por eso no es abstracto.
    /// </summary>
    public virtual void Remove(PlayerController player) { }

    /// <summary>
    /// Se llama cada frame desde PassiveManager para pasivas que necesitan
    /// comprobar condiciones continuamente.
    /// Ejemplo: pasiva de Gauthak que ajusta velocidad de ataque según HP.
    /// Las pasivas que no lo necesiten no tienen que sobreescribirlo.
    /// </summary>
    public virtual void Tick(PlayerController player) { }

    /// <summary>
    /// Aplica la pasiva durante una duración determinada y la revierte al terminar.
    /// Útil para efectos temporales activados por otras habilidades o pasivas.
    /// </summary>
    public virtual void ApplyTemporary(PlayerController player, float duration)
    {
        Apply(player);
        player.StartCoroutine(RemoveAfterDelay(player, duration));
    }

    private System.Collections.IEnumerator RemoveAfterDelay(PlayerController player, float duration)
    {
        yield return new UnityEngine.WaitForSeconds(duration);
        Remove(player);
    }
}