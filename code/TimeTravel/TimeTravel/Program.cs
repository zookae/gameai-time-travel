using System;

namespace TimeTravel
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (TimeTravelGame game = new TimeTravelGame())
            {
                game.Run();
            }
        }
    }
#endif
}

