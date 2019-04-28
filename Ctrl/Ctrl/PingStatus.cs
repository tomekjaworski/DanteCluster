using System.Net;
using System.Net.NetworkInformation;

namespace Ctrl
{
    public class PingStatus {
        private int responsesInARow;
        private int failsInARow;
        private int totalPings;
        private IPStatus icmpStatus;
        private PingReplyStatus currentReplyStatus;


        public IPAddress IP { get; private set; }

        public PingStatus(IPAddress ip)
        {
            this.IP = ip;
        }

        public void SetICMPStatus(IPStatus replyStatus)
        {
            lock (this)
            {
                // ogólne statystyki
                this.totalPings++;
                if (replyStatus == IPStatus.Success)
                {
                    this.responsesInARow++;
                    this.failsInARow = 0;
                }
                else
                {
                    this.responsesInARow = 0;
                    this.failsInARow++;
                }

                // status odpowiedzi (ostatniej)
                this.icmpStatus = replyStatus;
                if (replyStatus == IPStatus.Success)
                    this.currentReplyStatus = PingReplyStatus.Success;
                if (replyStatus == IPStatus.TimedOut || replyStatus == IPStatus.TimeExceeded)
                    this.currentReplyStatus = PingReplyStatus.TimeOut;
                if (replyStatus == IPStatus.BadRoute)
                    this.currentReplyStatus = PingReplyStatus.UnknownHost;


            }
        }
    }
}