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
                var _packect = SteamNetworking.ReadP2PPacket(channel);

                if (_packect != null)
                {
                    packet = _packect.Value;
                    return true;
                }
            }

            packet = default;
            return false;
        }

        public static bool CanReceive(out SChannel.MessageInfo messageInfo, int channel)
        {
            if (SteamNetworking.IsP2PPacketAvailable(channel))
            {
                messageInfo = new SChannel.MessageInfo(0);
                return SteamNetworking.ReadP2PPacket(messageInfo.messageInfo.bytes, ref messageInfo.messageSize, ref messageInfo.messageInfo.steamId,
                    channel);
            }

            messageInfo = default;
            return false;
        }
    }
}