using Mirror;
using System;

namespace AudioApi.Compents
{
    public class FakeConnection : NetworkConnectionToClient
    {
        public override string address => "localhost";
        public FakeConnection(int networkConnectionId) : base(networkConnectionId)
        {
        }
        public override void Send(ArraySegment<byte> segment, int channelId = 0)
        {
        }
    }
}
