using UnityEngine;

namespace BoneThrone.Combat
{
    /// <summary>
    /// Central D20 roller for basic combat resolution.
    /// Future host-authoritative networking should keep dice generation routed here.
    /// </summary>
    public sealed class D20Roller : MonoBehaviour
    {
        [SerializeField] private bool useDebugOverride;
        [SerializeField] private int debugRoll = 10;

        public int RollD20()
        {
            if (useDebugOverride)
            {
                return Mathf.Clamp(debugRoll, 1, 20);
            }

            return Random.Range(1, 21);
        }
    }
}
