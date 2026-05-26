using BoneThrone.Combat;
using BoneThrone.Movement;
using BoneThrone.Turns;
using BoneThrone.Units;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BoneThrone.UI
{
    /// <summary>
    /// Lightweight UI bridge for battle action targeting. It stores UI intent and calls existing gameplay APIs.
    /// </summary>
    public sealed class UIActionModeController : MonoBehaviour
    {
        private enum ActionMode
        {
            None,
            BasicAttackTargeting
        }

        [SerializeField] private SelectionManager selectionManager;
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private PromptView promptView;
        [SerializeField] private Camera inputCamera;
        [SerializeField] private LayerMask targetLayerMask = ~0;
        [SerializeField] private float maxRayDistance = 500f;
        [SerializeField] private PlayerMovementController movementControllerToSuspend;

        private ActionMode currentMode = ActionMode.None;
        private bool movementControllerWasEnabled;
        private bool movementInputSuspended;

        public bool IsTargeting
        {
            get { return currentMode != ActionMode.None; }
        }

        public void Configure(
            SelectionManager selection,
            CombatSystem combat,
            PromptView prompt,
            Camera camera,
            LayerMask layerMask,
            PlayerMovementController movementController)
        {
            selectionManager = selection;
            combatSystem = combat;
            promptView = prompt;
            inputCamera = camera;
            targetLayerMask = layerMask;
            movementControllerToSuspend = movementController;
        }

        private void OnDisable()
        {
            ExitTargetingMode();
        }

        private void Update()
        {
            if (currentMode == ActionMode.None)
            {
                return;
            }

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelTargeting();
                return;
            }

            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            HandleTargetClick();
        }

        public void HandleBasicAttackButtonClicked()
        {
            if (currentMode == ActionMode.BasicAttackTargeting)
            {
                CancelTargeting();
                return;
            }

            Unit selectedUnit = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (selectedUnit == null)
            {
                ShowPrompt("Select a unit first.");
                return;
            }

            if (!selectedUnit.IsAlive)
            {
                ShowPrompt("Selected unit is dead.");
                return;
            }

            UnitTurnState turnState = selectedUnit.GetComponent<UnitTurnState>();
            if (turnState != null && turnState.HasActed)
            {
                ShowPrompt("Selected unit has already acted.");
                return;
            }

            if (combatSystem == null)
            {
                ShowPrompt("Basic attack unavailable: CombatSystem unbound.");
                return;
            }

            EnterBasicAttackTargeting();
        }

        private void EnterBasicAttackTargeting()
        {
            currentMode = ActionMode.BasicAttackTargeting;
            SuspendMovementInput();
            ShowPrompt("Select an enemy target.");
        }

        private void HandleTargetClick()
        {
            Unit target = RaycastUnitUnderCursor();
            if (target == null || target.Faction != UnitFaction.Enemy)
            {
                ShowPrompt("Invalid attack target.");
                return;
            }

            Unit attacker = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (attacker == null)
            {
                ShowPrompt("Select a unit first.");
                ExitTargetingMode();
                return;
            }

            if (combatSystem == null)
            {
                ShowPrompt("Basic attack unavailable: CombatSystem unbound.");
                return;
            }

            bool success = combatSystem.TryBasicAttack(attacker, target);
            if (success)
            {
                ExitTargetingMode();
                ClearPrompt();
                return;
            }

            ShowPrompt("Invalid attack target.");
        }

        private Unit RaycastUnitUnderCursor()
        {
            Camera cameraToUse = inputCamera != null ? inputCamera : Camera.main;
            if (cameraToUse == null)
            {
                ShowPrompt("Basic attack unavailable: Camera unbound.");
                return null;
            }

            Ray ray = cameraToUse.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, maxRayDistance, targetLayerMask, QueryTriggerInteraction.Ignore))
            {
                return null;
            }

            return hit.collider.GetComponentInParent<Unit>();
        }

        private void CancelTargeting()
        {
            ExitTargetingMode();
            ShowPrompt("Basic attack canceled.", 1.5f);
        }

        private void ExitTargetingMode()
        {
            currentMode = ActionMode.None;
            RestoreMovementInput();
        }

        private void SuspendMovementInput()
        {
            if (movementControllerToSuspend == null)
            {
                return;
            }

            movementControllerWasEnabled = movementControllerToSuspend.enabled;
            movementControllerToSuspend.enabled = false;
            movementInputSuspended = true;
        }

        private void RestoreMovementInput()
        {
            if (!movementInputSuspended || movementControllerToSuspend == null)
            {
                return;
            }

            movementControllerToSuspend.enabled = movementControllerWasEnabled;
            movementInputSuspended = false;
        }

        private void ShowPrompt(string message)
        {
            ShowPrompt(message, 0f);
        }

        private void ShowPrompt(string message, float duration)
        {
            if (promptView != null)
            {
                promptView.ShowOverride(message, duration);
            }
        }

        private void ClearPrompt()
        {
            if (promptView != null)
            {
                promptView.ClearOverride();
            }
        }
    }
}
