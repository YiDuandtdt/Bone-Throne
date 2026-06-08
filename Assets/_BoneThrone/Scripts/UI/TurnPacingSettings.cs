using UnityEngine;

namespace BoneThrone.UI
{
    /// <summary>
    /// Shared timing profile for turn transition popups and sequential enemy action pacing.
    /// </summary>
    [CreateAssetMenu(fileName = "TurnPacingSettings", menuName = "BoneThrone/UI/Turn Pacing Settings")]
    public sealed class TurnPacingSettings : ScriptableObject
    {
        [Header("Enemy Round Intro")]
        [SerializeField] [Min(0f)] private float beforeEnemyTurnBannerDelay = 0.25f;
        [SerializeField] [Min(0f)] private float enemyTurnBannerHoldDuration = 0.85f;
        [SerializeField] [Min(0f)] private float afterEnemyTurnBannerDelay = 0.2f;

        [Header("Enemy Round Actions")]
        [SerializeField] [Min(0f)] private float enemyActionInterval = 0.5f;
        [SerializeField] [Min(0f)] private float afterEnemyRoundDelay = 0.35f;

        [Header("Player Round Intro")]
        [SerializeField] [Min(0f)] private float beforePlayerTurnBannerDelay = 0.2f;
        [SerializeField] [Min(0f)] private float playerTurnBannerHoldDuration = 0.85f;
        [SerializeField] [Min(0f)] private float afterPlayerTurnBannerDelay = 0.15f;

        [Header("Popup Motion")]
        [SerializeField] [Min(0f)] private float popupFadeInDuration = 0.14f;
        [SerializeField] [Min(0f)] private float popupFadeOutDuration = 0.18f;
        [SerializeField] [Min(0f)] private float popupDriftDistance = 22f;

        public float BeforeEnemyTurnBannerDelay => beforeEnemyTurnBannerDelay;
        public float EnemyTurnBannerHoldDuration => enemyTurnBannerHoldDuration;
        public float AfterEnemyTurnBannerDelay => afterEnemyTurnBannerDelay;
        public float EnemyActionInterval => enemyActionInterval;
        public float AfterEnemyRoundDelay => afterEnemyRoundDelay;
        public float BeforePlayerTurnBannerDelay => beforePlayerTurnBannerDelay;
        public float PlayerTurnBannerHoldDuration => playerTurnBannerHoldDuration;
        public float AfterPlayerTurnBannerDelay => afterPlayerTurnBannerDelay;
        public float PopupFadeInDuration => popupFadeInDuration;
        public float PopupFadeOutDuration => popupFadeOutDuration;
        public float PopupDriftDistance => popupDriftDistance;
    }
}
