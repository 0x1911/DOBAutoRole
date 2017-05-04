using System;

namespace DOB_AutoRole.Modules.ModModule
{
    public class UserStats
    {
        public ulong UserId { get; set; }        
        public DateTime ExpirationDate { get; set; }
        public int WarningPoints { get; set; }
    }
}
