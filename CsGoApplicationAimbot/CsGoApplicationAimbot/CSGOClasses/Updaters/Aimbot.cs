namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    internal class Aimbot
    {
        private void Update()
        {
            if (!Memory.ShouldUpdate())
                return;
        }
    }
}