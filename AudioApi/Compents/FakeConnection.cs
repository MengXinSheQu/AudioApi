
using Mirror;
using System;

namespace AudioApi.Compents
{
    public class FakeConnection(int networkConnectionId) : NetworkConnectionToClient(networkConnectionId)
    {
        public override string address => "localhost";

        public override void Send(ArraySegment<byte> segment, int channelId = 0)
        {
        }
    }
}
