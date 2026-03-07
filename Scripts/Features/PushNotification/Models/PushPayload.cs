namespace GameFoundation.Scripts.Features.PushNotification.Models
{
    using System.Collections.Generic;

    public class PushPayload
    {
        public string                     Title          { get; set; }
        public string                     Body           { get; set; }
        public string                     DeepLink       { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }
}
