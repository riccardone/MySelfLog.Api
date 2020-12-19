using Finbuckle.MultiTenant;

namespace MySelfLog.Admin.Model
{
    public class MySelfLogTenantInfo : TenantInfo
    {
        public string MessageBusLink { get; set; }
        public string CryptoKey { get; set; }
        public string ApiKey { get; set; }
        public string AdminEmail { get; set; }
        public string Domain { get; set; }
    }
}
