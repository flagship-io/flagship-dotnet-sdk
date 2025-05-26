namespace Flagship.Model
{
    public class SdkInitialData
    {
        public string InstanceId { get; set; }
        public string LastInitializationTimestamp { get; set; }
        public bool UsingCustomHitCache { get; set; }
        public bool UsingCustomVisitorCache { get; set; }
    }
}
