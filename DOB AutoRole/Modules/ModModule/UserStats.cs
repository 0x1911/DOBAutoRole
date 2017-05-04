using System;

namespace DOB_AutoRole.Modules.ModModule
{
    public class UserStats
    {
        public ulong UserId { get; set; }
        public int IdlePoints { get; set; }
        
        public DateTime WarningPointsExpirationDate { get; set; }
        public int WarningPoints { get; set; }
        
        public int KickedCount { get; set; }

        public int BannedCount { get; set; }
        public bool IsPermaBan { get; set; }
        public DateTime BanExpirationDate { get; set; }
    }
}
