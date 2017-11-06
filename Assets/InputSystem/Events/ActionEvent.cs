using System;
using System.Runtime.InteropServices;

namespace ISX
{
    // Phase change of an action.
    //
    // Captures the control state initiating the phase shift as well as action-related
    // information when the phase shift happened.
    //
    // Action events are representation-compatible with DeltaStateEvents. The
    // DeltaStateEvent portion of the event captures the control state that
    // triggered the action. The remainder of the action event contains
    // information about which binding triggered the action and such.
    //
    // Variable-size event.
    //
    // NOTE: ActionEvents do not surface on the native event queue (i.e. they do not come in
    //       through native updates received through NativeInputSystem.onUpdate). Instead,
    //       action events are handled separately by the action system.
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = InputEvent.kBaseEventSize + 9)]
    public unsafe struct ActionEvent : IInputEventTypeInfo
    {
        public const int Type = 0x4143544E; // 'ACTN'

        [FieldOffset(0)] public InputEvent baseEvent;
        [FieldOffset(InputEvent.kBaseEventSize)] public FourCC stateFormat;
        [FieldOffset(InputEvent.kBaseEventSize + 4)] public uint stateOffset;
        [FieldOffset(InputEvent.kBaseEventSize + 8)] public fixed byte stateData[1]; // Variable-sized.

        public int stateSizeInBytes => baseEvent.sizeInBytes - (InputEvent.kBaseEventSize + 8);

        public IntPtr state
        {
            get
            {
                fixed(byte* data = stateData)
                {
                    return new IntPtr((void*)data);
                }
            }
        }

        // Action-specific fields are appended *after* the device state in the event.
        // This way we can use ActionEvent everywhere a DeltaStateEvent is expected.

        public int actionIndex => *(int*)(actionData + (int)ActionDataOffset.ActionIndex);
        public int bindingIndex => *(int*)(actionData + (int)ActionDataOffset.BindingIndex);
        public int modifierIndex => *(int*)(actionData + (int)ActionDataOffset.ModifierIndex);
        public double startTime => *(double*)(actionData + (int)ActionDataOffset.StartTime);
        public double endTime => *(double*)(actionData + (int)ActionDataOffset.EndTime);
        public InputAction.Phase phase => (InputAction.Phase)(*(int*)(actionData + (int)ActionDataOffset.Phase));

        ////TODO: give all currently enabled actions indices

        public FourCC GetTypeStatic()
        {
            return Type;
        }

        internal IntPtr actionData => state + stateSizeInBytes;

        internal enum ActionDataOffset
        {
            ActionIndex = 0,
            BindingIndex = 4,
            ModifierIndex = 8,
            StartTime = 12,
            EndTime = 20,
            Phase = 28
        }
    }
}
