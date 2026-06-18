using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MimeKit;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class KafkaConsumerService : BackgroundService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private IConsumer<Ignore, string>? _consumer;

    // Email settings - update these or load from config
    private const string SmtpServer = "smtp.gmail.com";
    private const int SmtpPort = 587;
    private const string SenderEmail = "your-email@gmail.com";
    private const string SenderPassword = "your-app-password";
    private const string SenderName = "ChineseSaleApi";

    public KafkaConsumerService(ILogger<KafkaConsumerService> logger)
    {
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "transaction-events-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        _consumer.Subscribe(new[] { "transaction-events", "email-events" });
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = _consumer!.Consume(stoppingToken);
                        if (cr != null)
                        {
                            _logger.LogInformation("Message received from topic: {Topic}\n\n{Message}\n", cr.Topic, cr.Message.Value);

                            // Handle email events
                            if (cr.Topic == "email-events")
                            {
                                try
                                {
                                    var emailEvent = JsonSerializer.Deserialize<EmailEvent>(cr.Message.Value!);
                                    if (emailEvent != null)
                                    {
                                        _logger.LogInformation("Sending email to {To} with subject: {Subject}", emailEvent.To, emailEvent.Subject);
                                        SendEmailViaSMTP(emailEvent);
                                        _logger.LogInformation("Email sent successfully to {To}", emailEvent.To);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Failed to send email.");
                                }
                            }
                            // Handle transaction events
                            else if (cr.Topic == "transaction-events")
                            {
                                try
                                {
                                    var doc = JsonSerializer.Deserialize<JsonElement>(cr.Message.Value!);
                                    _logger.LogInformation("Transaction received\n\nTransactionId: {Id}\nCustomerEmail: {Email}\nAmount: {Amount}\nCreatedAt: {CreatedAt}\nType: {Type}",
                                        doc.GetProperty("TransactionId").GetString(),
                                        doc.TryGetProperty("CustomerEmail", out var e) ? e.GetString() : "N/A",
                                        doc.TryGetProperty("Amount", out var a) ? a.GetDecimal() : 0m,
                                        doc.TryGetProperty("CreatedAt", out var c) ? c.GetDateTime() : DateTime.MinValue,
                                        doc.TryGetProperty("TransactionType", out var t) ? t.GetString() : "N/A");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Failed to parse transaction message payload.");
                                }
                            }
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Kafka consume error");
                    }
                }
            }
            finally
            {
                _consumer?.Close();
            }
        }, stoppingToken);
    }

    private void SendEmailViaSMTP(EmailEvent emailEvent)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(SenderName, SenderEmail));
            message.To.Add(new MailboxAddress("", emailEvent.To));
            message.Subject = emailEvent.Subject;
            message.Body = new TextPart("html") { Text = emailEvent.Body };

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    if (sslPolicyErrors == SslPolicyErrors.None)
                        return true;

                    if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0 && chain != null)
                    {
                        var statuses = chain.ChainStatus ?? Array.Empty<X509ChainStatus>();
                        var onlyRevocationIssues = statuses.Length > 0 &&
                                                   statuses.All(s =>
                                                       s.Status == X509ChainStatusFlags.RevocationStatusUnknown ||
                                                       s.Status == X509ChainStatusFlags.OfflineRevocation);

                        if (onlyRevocationIssues)
                            return true;
                    }

                    return false;
                };

                client.Connect(SmtpServer, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate(SenderEmail, SenderPassword);
                client.Send(message);
                client.Disconnect(true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}.", emailEvent?.To);
            throw;
        }
    }
}

// Email event DTO for deserialization
public class EmailEvent
{
    public Guid EventId { get; set; }
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

