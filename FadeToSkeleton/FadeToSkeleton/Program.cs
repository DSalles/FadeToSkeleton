using System;

namespace PullHeadOff
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (PullHeadOff game = new PullHeadOff())
            {
                game.Run();
            }
        }
    }
#endif
}

