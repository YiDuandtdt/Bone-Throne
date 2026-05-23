using BoneThrone.Core;
using UnityEngine;

namespace BoneThrone.Units
{
    /// <summary>
    /// Definition asset type for future player and enemy unit templates.
    /// This phase defines the type only and does not require creating assets.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitData", menuName = "Bone Throne/Units/Unit Data")]
    public sealed class UnitData : ScriptableObject
    {
        [SerializeField] private int unitId;
        [SerializeField] private string displayName;
        [SerializeField] private RoleId roleId = RoleId.None;
        [SerializeField] private UnitFaction faction = UnitFaction.None;
        [SerializeField] private UnitStats stats = new UnitStats();

        public int UnitId
        {
            get { return unitId; }
        }

        public string DisplayName
        {
            get { return displayName; }
        }

        public RoleId RoleId
        {
            get { return roleId; }
        }

        public UnitFaction Faction
        {
            get { return faction; }
        }

        public UnitStats Stats
        {
            get { return stats; }
        }
    }
}
