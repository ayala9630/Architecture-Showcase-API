namespace ChineseSaleApi.Dto
{
    public class EmailSettingsDto
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Password { get; set; }
    }
    public class EmailRequestDto
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

}
