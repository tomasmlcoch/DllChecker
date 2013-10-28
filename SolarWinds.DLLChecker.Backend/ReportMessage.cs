namespace SolarWinds.DLLChecker.Backend
{
    public class ReportMessage
    {
        public enum ReportType
        {
            Message,Success,Failure
        } 

        public ReportType Type { get; set; }
        public string Title { get; set; }
        public string ShortExplanation { get; set; }
        public string Message { get; set; }
     
    }
}
