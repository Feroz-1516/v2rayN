namespace v2rayN.Models
{
    
    
    
    public class V2rayTcpRequest
    {
        
        
        
        public RequestHeaders headers { get; set; }
    }

    public class RequestHeaders
    {
        
        
        
        public List<string> Host { get; set; }
    }
}