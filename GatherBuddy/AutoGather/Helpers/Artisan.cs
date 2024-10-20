using Dalamud.Plugin.Ipc.Exceptions;
using ECommons;
using System;
using ECommons.Throttlers;
using GatherBuddy.Plugin;

namespace GatherBuddy.AutoGather.Helpers
{
    /**
     * <summary>
     * Helper class to handle interaction with Artisan
     * </summary>
     */
    internal class Artisan
    {
        internal static bool WasPaused = false;

        /**
         * 
         * <summary>
         * Try to pause artisan. Returns true if successfully paused an ongoing craft
         * </summary>
         */
        internal static bool TryPause()
        {
            try
            {
                if (IsCurrentlyOperating() && !WasPaused)
                {
                    WasPaused = true;
                    GatherBuddy.Log.Information("Paused Artisan");
                    Artisan_IPCSubscriber.SetStopRequest(true);
                    return true;
                }
            }
            catch (IpcNotReadyError) { }
            catch (Exception ex)
            {
                {
                    ex.Log();
                }
            }
            return false;
        }

        /**
         * <summary>
         * Restart Artisan
         * </summary>
         */
        internal static void Restart()
        {
            if(!IsCurrentlyOperating() && WasPaused)
            {
                GatherBuddy.Log.Information("Artisan is previously paused...Attempting to restart");
                if (GenericHelpers.IsOccupied())
                {
                    EzThrottler.Throttle("ArtisanCanReenableOccupied", 2500, true);
                }
                if (EzThrottler.Check("ArtisanCanReenableOccupied"))
                {
                    GatherBuddy.Log.Information("Successfully restarted Artisan");
                    WasPaused = false;
                    Artisan_IPCSubscriber.SetStopRequest(false);
                }
            }
        }

        internal static bool IsCurrentlyOperating()
        {
            try
            {
                return Artisan_IPCSubscriber.IsListRunning() || Artisan_IPCSubscriber.GetEnduranceStatus();
            }
            catch (IpcNotReadyError) { }
            catch (Exception ex)
            {
                ex.LogWarning();
            }
            return false;
        }
    }
}
