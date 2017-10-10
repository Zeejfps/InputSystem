namespace ISX
{
    // An axis that has a trigger point beyond which it is considered to be pressed.
    // By default stored as a single bit. In that format, buttons will only yield 0
    // and 1 as values.
    //
    // NOTE: While it may seem unnatural to derive ButtonControl from AxisControl, doing
    //       so brings many benefits through allowing code to flexibly target buttons
    //       and axes the same way.
    public class ButtonControl : AxisControl
    {
        public float pressPoint;

        public ButtonControl()
        {
            m_StateBlock.format = InputStateBlock.kTypeBit;
        }

        protected bool IsValueConsideredPresses(float value)
        {
            var point = pressPoint;
            if (pressPoint <= 0.0f)
                point = InputConfiguration.ButtonPressPoint;
            return value >= point;
        }

        public bool isPressed => IsValueConsideredPresses(value);
        public bool wasPressedThisFrame => IsValueConsideredPresses(value) && !IsValueConsideredPresses(previous);
        public bool wasReleasedThisFrame => !IsValueConsideredPresses(value) && IsValueConsideredPresses(previous);
    }
}
