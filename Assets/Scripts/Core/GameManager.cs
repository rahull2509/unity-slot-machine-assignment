using UnityEngine;

namespace SlotMachineGame.Core
{
    /// <summary>
    /// Root application bootstrap ensuring target framerate, persistent configuration,
    /// and clean WebGL lifecycle management.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Runtime Performance")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool runInBackground = false;

        [Header("Controller")]
        [SerializeField] private SlotMachineController slotMachineController;

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Application.targetFrameRate = targetFrameRate;
            Application.runInBackground = runInBackground;
        }

        private void Start()
        {
            if (slotMachineController == null)
                slotMachineController = FindFirstObjectByType<SlotMachineController>();
        }
    }
}
