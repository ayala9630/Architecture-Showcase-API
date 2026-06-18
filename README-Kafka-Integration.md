# Kafka Integration - ChineseSaleApi

## Overview

This implementation uses Kafka as the event bus for all notifications:
- **Email delivery** is fully asynchronous via Kafka (not direct SMTP)
- **Transaction events** are published for audit and business intelligence
- **Consumer service** runs independently and processes events from Kafka

## Local setup

1. Ensure Docker is installed and running.
2. From the `ChineseSaleApi` folder, start the stack:

```powershell
docker compose up -d
```

This will start:
- `zookeeper` on 2181
- `kafka` on 9092 and 29092
- `kafka-ui` on 8080
- `redis` and `api`

## Kafka UI

Open http://localhost:8080 to access Kafka UI. Cluster `local` connects to `kafka:9092`.

### Expected Topics

- `transaction-events` - Contains UserRegistered, UserLogin, LotteryWin events
- `email-events` - Contains EmailEvent messages to be processed by the consumer

## Architecture Changes

### Producer (Main API)

Services no longer call `_emailService.SendEmail()` directly. Instead:

1. **UserService.AddUser()** → publishes `EmailEvent` to `email-events` topic
2. **UserService.AuthenticateAsync()** → publishes `EmailEvent` to `email-events` topic
3. **LotteryService.UpdateWin()** → publishes `TransactionCreatedEvent` to `transaction-events` topic

### Consumer (Worker Service)

The consumer at `Consumers/KafkaConsumer` processes two types of events:

1. **email-events** - Deserializes and sends actual emails via SMTP
   - Configured with SMTP details (Gmail, corporate email, etc.)
   - Non-blocking; failures are logged

2. **transaction-events** - Logs business events for audit/analytics
   - Can be extended for webhooks, webhooks, databases, etc.

## Consumer Configuration

### Email Settings

Edit `Consumers/KafkaConsumer/KafkaConsumerService.cs` and update:

```csharp
private const string SmtpServer = "smtp.gmail.com";
private const int SmtpPort = 587;
private const string SenderEmail = "your-email@gmail.com";
private const string SenderPassword = "your-app-password";
private const string SenderName = "ChineseSaleApi";
```

For Gmail:
- Use [App Passwords](https://myaccount.google.com/apppasswords) instead of your actual password
- Enable 2FA on your Google account first

## Running the Application

1. **Start Docker stack:**
   ```powershell
   docker compose up -d
   ```

2. **Start the main API:**
   ```powershell
   dotnet run
   ```

3. **Start the consumer (in a separate terminal):**
   ```powershell
   cd Consumers/KafkaConsumer
   dotnet run
   ```

## Testing Scenarios

### User Registration Flow

1. Call POST `/api/user/register` with a user
2. Check the **main API logs** - should publish `EmailEvent` and `TransactionCreatedEvent`
3. Check Kafka UI topics - both events should appear
4. Check **consumer logs** - should show email sent and transaction logged

### User Login Flow

1. Call POST `/api/user/login`
2. API publishes login email and transaction event
3. Consumer sends the notification email
4. Both events visible in Kafka UI

### Lottery Execution

1. Call lottery execution endpoint
2. API publishes lottery win event
3. Consumer logs the transaction
4. Event visible in Kafka UI

## Kafka UI Verification

### Topics Tab
- `transaction-events` - shows all business events
- `email-events` - shows pending emails

### Messages Tab
- Click each topic to view message payloads
- Inspect `TransactionId`, `CreatedAt`, email details, etc.

### Consumer Groups Tab
- `transaction-events-consumer-group` - group subscribed to both topics
- View lag and offsets

## Troubleshooting

### Consumer not sending emails
- Verify SMTP credentials in `KafkaConsumerService.cs`
- Check consumer logs for "Failed to send email" errors
- Ensure port 587 (TLS) is accessible to your SMTP server

### Messages not appearing in Kafka UI
- Verify Kafka broker is running: `docker logs chinesesale_kafka`
- Check API logs for Kafka connection errors
- Consumer group might be lagging - wait a moment and refresh

### API can'ct to Kafka
- Verify `BootstrapServers` in `appsettings.json` is set to `kafka:9092`
- Check Docker network: `docker network inspect chinesesale_network`
- Check Kafka broker logs: `docker logs chinesesale_kafka`

## Notes

- All email sending is now fully asynchronous
- API completes requests without waiting for email delivery
- Emails are eventually consistent (processed by consumer at its own pace)
- Failed emails are logged; consider implementing a retry mechanism if needed


