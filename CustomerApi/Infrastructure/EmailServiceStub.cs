namespace CustomerApi.Infrastructure
{
    public class EmailServiceStub : IEmailService
    {
        public void SendEmail(string to, string subject, string body)
        {
            Console.WriteLine($"Sending email to {to} with subject {subject} and body:");
            Console.WriteLine(body);
        }
    }

}

