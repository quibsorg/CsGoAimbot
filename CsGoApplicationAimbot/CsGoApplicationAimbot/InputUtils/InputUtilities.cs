namespace CsGoApplicationAimbot.InputUtils
{
    public class InputUtilities
    {
        public KeyUtils Keys;
        public MouseHook Mouse;

        /// <summary>
        ///     If true mouse changed since last update
        /// </summary>
        public bool MouseChanged;

        public InputUtilities()
        {
            Init();
        }

        private void Init()
        {
            Keys = new KeyUtils();
            Mouse = new MouseHook();
            Mouse.InstallHook();
        }

        /// <summary>
        ///     Updates keys and mouse
        /// </summary>
        public void Update()
        {
            Keys.Update();
            MouseChanged = Mouse.Update();
        }
    }
}