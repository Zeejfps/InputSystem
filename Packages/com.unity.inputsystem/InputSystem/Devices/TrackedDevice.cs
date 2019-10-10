using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

namespace UnityEngine.InputSystem
{
    /// <summary>
    /// An input device that has its orientation and position in space tracked.
    /// </summary>
    [InputControlLayout(displayName = "Tracked Device", isGenericTypeOfDevice = true)]
    public class TrackedDevice : InputDevice
    {
        [InputControl(noisy = true)]
        [Preserve]
        public IntegerControl trackingState { get; private set; }
        [InputControl(noisy = true)]
        [Preserve]
        public ButtonControl isTracked { get; private set; }
        [InputControl(noisy = true)]
        public Vector3Control devicePosition { get; private set; }
        [InputControl(noisy = true)]
        [Preserve]
        public QuaternionControl deviceRotation { get; private set; }

        protected override void FinishSetup()
        {
            base.FinishSetup();

            trackingState = GetChildControl<IntegerControl>("trackingState");
            isTracked = GetChildControl<ButtonControl>("isTracked");
            devicePosition = GetChildControl<Vector3Control>("devicePosition");
            deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");
        }
    }
}
