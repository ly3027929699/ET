using System;
using Steamworks;
using Steamworks.Data;

namespace ET
{
    public static class SteamUtility
    {
        public static bool CanReceive(out P2Packet packet, int channel)
        {
            if (SteamNetworking.IsP2PPacketAvailable(channel))
            {
               var  _packect= SteamNetworking.ReadP2PPacket(channel);

                if (_packect != null)
                {
                    packet = _packect.Value;
                    return true;
                }
            }

            packet = default;
            return false;
        }
        public static async ETTask ReceiveInternalData(int channel)
        {
            try
            {
                ETTask<P2Packet> task = ETTask<P2Packet>.Create();
                while (CanReceive(out P2Packet packet,channel))
                {
                    if (packet.Data.Length == 1)
                    {
                        // action.Invoke(packet);
                        task.SetResult(packet);
                        return;
                    }
                    else
                    {
                        Log.Info("Incorrect package length on internal channel.");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return;
        }

    }
}