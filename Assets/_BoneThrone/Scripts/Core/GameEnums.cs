using BoneThrone.Skills;
using BoneThrone.Units;

namespace BoneThrone.Core
{
    /// <summary>
    /// Identifies the session mode without tying gameplay code to a transport layer.
    /// </summary>
    public enum GameMode
    {
        SinglePlayer = 0,
        LANHost = 1,
        LANClient = 2,
        OnlineHost = 3,
        OnlineClient = 4
    }

    /// <summary>
    /// Identifies fixed player roles and non-player turn ownership placeholders.
    /// </summary>
    public enum RoleId
    {
        None = 0,
        Fighter = 1,
        Ranger = 2,
        Mage = 3,
        Barbarian = 4,
        Enemy = 5
    }

    /// <summary>
    /// Describes the intent of a command without implementing any gameplay behavior.
    /// </summary>
    public enum ActionCommandType
    {
        None = 0,
        Move = 1,
        Attack = 2,
        Skill = 3,
        UseItem = 4,
        Defend = 5,
        EndTurn = 6,
        Interact = 7
    }

    /// <summary>
    /// Represents high-level session lifecycle state for local or future network sessions.
    /// </summary>
    public enum GameSessionState
    {
        None = 0,
        NotStarted = 1,
        Starting = 2,
        Running = 3,
        Paused = 4,
        Completed = 5,
        Failed = 6
    }

    public static class BoneThroneTextUtility
    {
        private const string FighterColor = "#D9EAF7";
        private const string RangerColor = "#8FE388";
        private const string MageColor = "#8CC8FF";
        private const string BarbarianColor = "#FF9A7A";
        private const string EnemyColor = "#D8C08A";
        private const string BossColor = "#FF6B6B";

        public static string GetRoleDisplayName(RoleId roleId)
        {
            switch (roleId)
            {
                case RoleId.Fighter:
                    return "战士";
                case RoleId.Ranger:
                    return "游侠";
                case RoleId.Mage:
                    return "法师";
                case RoleId.Barbarian:
                    return "野蛮人";
                case RoleId.Enemy:
                    return "敌方";
                default:
                    return "角色";
            }
        }

        public static string GetUnitDisplayName(Unit unit)
        {
            if (unit == null)
            {
                return "未知单位";
            }

            if (unit.Faction == UnitFaction.Player)
            {
                return GetRoleDisplayName(unit.RoleId);
            }

            return GetEnemyDisplayName(unit);
        }

        public static string GetEnemyDisplayName(Unit unit)
        {
            if (unit == null)
            {
                return "敌人";
            }

            string key = NormalizeKey(unit.DisplayName);
            if (string.IsNullOrEmpty(key))
            {
                key = NormalizeKey(unit.name);
            }

            switch (key)
            {
                case "skeletongolemboss":
                case "skeletongolem":
                    return "骸骨巨像";
                case "skeletonwarrior":
                    return "骸骨战士";
                case "skeletonrogue":
                    return "骸骨游荡者";
                case "skeletonnecromancer":
                case "skeletonmage":
                    return "骸骨法师";
                case "skeletonminion":
                    return "骸骨喽啰";
                default:
                    return string.IsNullOrWhiteSpace(unit.DisplayName) || unit.DisplayName == "Enemy" || unit.DisplayName == "Skeleton"
                        ? "骸骨守卫"
                        : unit.DisplayName;
            }
        }

        public static string GetSkillDisplayName(SkillData skill)
        {
            return skill == null ? "未知技能" : GetSkillDisplayName(skill.DisplayName);
        }

        public static string GetSkillDisplayName(string rawName)
        {
            string key = NormalizeKey(rawName);
            switch (key)
            {
                case "fightershieldbash":
                    return "战士·盾击";
                case "fighterguardstrike":
                    return "战士·守备突刺";
                case "fightercrushingchallenge":
                    return "战士·重压嘲讽";
                case "rangerprecisionshot":
                    return "游侠·精准射击";
                case "rangerquickshot":
                    return "游侠·速射";
                case "rangerpiercingarrow":
                    return "游侠·穿透箭";
                case "magefireball":
                    return "法师·火球术";
                case "magefrostbolt":
                    return "法师·寒霜箭";
                case "magearcaneburst":
                    return "法师·奥术爆发";
                case "barbarianheavycleave":
                    return "野蛮人·重击斩";
                case "barbarianragestrike":
                    return "野蛮人·狂怒打击";
                case "barbarianbloodfuryslash":
                    return "野蛮人·血怒斩击";
                default:
                    return string.IsNullOrWhiteSpace(rawName) ? "未知技能" : rawName;
            }
        }

        public static string ColorizeUnitName(Unit unit)
        {
            return WrapColor(GetUnitDisplayName(unit), GetUnitColor(unit));
        }

        public static string ColorizeSkillName(Unit caster, SkillData skill)
        {
            return WrapColor(GetSkillDisplayName(skill), GetSkillColor(caster, skill));
        }

        public static string WrapColor(string text, string colorHex)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return "<color=" + colorHex + ">" + text + "</color>";
        }

        public static string GetUnitColor(Unit unit)
        {
            if (unit == null)
            {
                return EnemyColor;
            }

            if (unit.Faction == UnitFaction.Player)
            {
                switch (unit.RoleId)
                {
                    case RoleId.Fighter:
                        return FighterColor;
                    case RoleId.Ranger:
                        return RangerColor;
                    case RoleId.Mage:
                        return MageColor;
                    case RoleId.Barbarian:
                        return BarbarianColor;
                    default:
                        return FighterColor;
                }
            }

            return IsBossLikeUnit(unit) ? BossColor : EnemyColor;
        }

        public static string GetSkillColor(Unit caster, SkillData skill)
        {
            return GetUnitColor(caster);
        }

        public static bool IsBossLikeUnit(Unit unit)
        {
            if (unit == null)
            {
                return false;
            }

            string key = NormalizeKey(unit.DisplayName);
            if (string.IsNullOrEmpty(key))
            {
                key = NormalizeKey(unit.name);
            }

            return key.Contains("boss") || key.Contains("golem");
        }

        public static string NormalizeKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            char[] buffer = new char[value.Length];
            int count = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (char.IsLetterOrDigit(c))
                {
                    buffer[count] = char.ToLowerInvariant(c);
                    count++;
                }
            }

            return new string(buffer, 0, count);
        }
    }
}
