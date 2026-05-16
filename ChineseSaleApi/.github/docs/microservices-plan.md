# Microservices Architecture Planning Document

**ChineseSaleApi — Future Microservices Migration Guide**

*Last Updated: May 13, 2026*  
*Current State: Monolithic ASP.NET Core application*  
*Purpose: Strategic planning for potential future decomposition into microservices*

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current Monolithic Architecture](#current-monolithic-architecture)
3. [Candidate Domains for Microservices](#candidate-domains-for-microservices)
4. [Communication Strategies Between Services](#communication-strategies-between-services)
5. [Shared Infrastructure Considerations](#shared-infrastructure-considerations)
6. [Database Separation Strategy](#database-separation-strategy)
7. [Deployment Considerations](#deployment-considerations)
8. [Implementation Roadmap](#implementation-roadmap)
9. [Risk Assessment](#risk-assessment)

---

## Executive Summary

The ChineseSaleApi is a **mid-scale lottery and gift management platform** currently implemented as a monolithic ASP.NET Core application. This document outlines a strategic framework for potential future decomposition into microservices, focusing on business domain boundaries, data isolation, and operational considerations.

**Key Objectives:**
- Identify independent business domains suitable for service separation
- Plan communication patterns that maintain system cohesion
- Define data ownership and isolation boundaries
- Establish deployment and scaling strategies
- Preserve existing functionality while enabling future growth

**Scope:** This is **planning and strategy only**. No implementation, code refactoring, or architectural changes are made in the current codebase.

---

## Current Monolithic Architecture

### System Components

The current monolithic application consists of:

- **11 API Controllers** handling business operations
- **13 Services** implementing domain logic
- **10 Repository Interfaces** abstracting data access
- **13 DTOs** maintaining API contracts
- **10 Core Domain Models** representing business entities
- **Single Relational Database (SQL Server)** via Entity Framework Core
- **Shared Infrastructure:**
  - JWT Authentication service
  - Error handling middleware
  - Logging (Serilog)
  - Dependency injection container

### Core Business Domains

| Domain | Purpose | Models | Controllers |
|--------|---------|--------|-------------|
| **User Management** | Authentication, authorization, user profiles | User, Address | UserController |
| **Lottery Management** | Campaign lifecycle, winner selection, reporting | Lottery, Card | LotteryController, CardController |
| **Gift Management** | Catalog, donation tracking | Gift, Category | GiftController, CategoryController |
| **Donor Management** | Corporate/individual donors, gift provisioning | Donor | DonorController |
| **Shopping & Carts** | Purchase workflow, cart management | CardCart, PackageCart | CardCartController, PackageCartController |
| **Package Management** | Pre-configured card bundles | Package | PackageController |
| **Address Management** | Shipping/company addresses | Address | AddressController |
| **File Operations** | Reporting, export functionality | (N/A) | FilesController |

### Current Data Dependencies

```
User ←→ Address (1:1)
    ↓
    CardCart ←→ Gift
    ↓
    PackageCart ←→ Package ←→ Lottery

Lottery ←→ Card ←→ Gift ←→ Category
     ↓
     Donor ←→ Address

User ←→ Card ←→ Gift ←→ Lottery
```

---

## Candidate Domains for Microservices

### Tier 1: High-Priority Candidates (Low Coupling, Clear Boundaries)

#### **1. User & Identity Service**
**Current Components:**
- UserController, UserService, UserRepository
- Models: User, Address
- Authentication/Authorization logic
- JWT TokenService

**Rationale for Separation:**
- ✅ Low direct coupling with other domains (users don't own other entities)
- ✅ Clear authentication boundary (can be isolated completely)
- ✅ High scalability demands (every API call touches user auth)
- ✅ Potential for external identity provider integration (e.g., Azure AD, Auth0)

**Responsibilities in Microservice:**
- User registration, login, profile management
- JWT token generation and validation
- Address management for users
- Admin role assignment and validation
- User pagination and search

**Data Ownership:**
- User table (complete ownership)
- Address table (for user addresses only)

**External Dependencies:**
- None (can be completely isolated)

**Scalability Benefit:** Authentication typically becomes a bottleneck; isolating it enables independent scaling.

---

#### **2. Donor & Donor Address Service**
**Current Components:**
- DonorController, DonorService, DonorRepository
- Models: Donor, Address (CompanyAddressId)

**Rationale for Separation:**
- ✅ Minimal coupling (donors are referenced by Gifts, not core to purchase flow)
- ✅ Admin-only operations (isolated access pattern)
- ✅ Separate business entity with independent lifecycle
- ✅ Rarely changes during runtime, suitable for caching

**Responsibilities in Microservice:**
- Donor CRUD operations
- Company address management
- Donor pagination and filtering
- Donor analytics (gift contributions)

**Data Ownership:**
- Donor table (complete ownership)
- Address table (company addresses only)

**External Dependencies:**
- Gift Service (read-only: which gifts belong to which donor)

**Scalability Benefit:** Low-traffic admin service; can run on minimal resources.

---

#### **3. Category & Taxonomy Service**
**Current Components:**
- CategoryController, CategoryService, CategoryRepository
- Models: Category

**Rationale for Separation:**
- ✅ Single table, no complex relationships
- ✅ Admin-only CRUD operations
- ✅ High read volume, low write volume (excellent caching candidate)
- ✅ Completely independent from other domains

**Responsibilities in Microservice:**
- Category CRUD operations
- Category listing and filtering
- Category caching and distribution

**Data Ownership:**
- Category table (complete ownership)

**External Dependencies:**
- None (write-only dependency from Gift Service)

**Scalability Benefit:** Lightweight service; excellent performance characteristics for hierarchical content.

---

### Tier 2: Medium-Priority Candidates (Moderate Coupling, Manageable Boundaries)

#### **4. Gift Catalog Service**
**Current Components:**
- GiftController, GiftService, GiftRepository
- Models: Gift
- Related: Category (read), Donor (read), Lottery (read)

**Rationale for Separation:**
- ✅ Core business entity with defined lifecycle
- ✅ Read-heavy (customers browse frequently)
- ✅ External dependencies exist but are mostly read-only
- ✅ Independent pricing and inventory logic

**Responsibilities in Microservice:**
- Gift CRUD operations
- Gift catalog browsing and search
- Gift availability checking
- Gift pricing and inventory management
- Image URL management
- IsPackageAble flag (used by Package Service)

**Data Ownership:**
- Gift table (complete ownership)

**External Dependencies (Read-Only):**
- Category Service (category lookup)
- Donor Service (donor lookup)
- Lottery Service (lottery context)

**Scalability Benefit:** Can cache extensively; enable read replicas for catalog browsing.

---

#### **5. Package Management Service**
**Current Components:**
- PackageController, PackageService, PackageRepository
- Models: Package
- Related: Lottery (read)

**Rationale for Separation:**
- ✅ Related domain (manages bundles of cards)
- ✅ Independent CRUD lifecycle
- ✅ Primarily admin-managed (low write frequency)
- ✅ Dependency on Lottery is one-directional

**Responsibilities in Microservice:**
- Package CRUD operations
- Package-by-lottery filtering
- NumOfCards configuration and validation
- Package pricing

**Data Ownership:**
- Package table (complete ownership)

**External Dependencies (Read-Only):**
- Lottery Service (valid lottery validation)

**Scalability Benefit:** Moderate traffic service; suitable for standard deployment.

---

### Tier 3: High-Coupling Candidates (Complex, Deferred Separation)

#### **6. Lottery & Lottery Operations Service**
**Current Components:**
- LotteryController, LotteryService, LotteryRepository
- CardController, CardService, CardRepository
- Models: Lottery, Card
- Related: Package (read), Gift (read), Donor (read)

**Rationale for Keeping Coupled (for now):**
- ❌ High internal coupling (Cards belong to Lotteries)
- ❌ Complex state machine (Lottery.StartDate, Lottery.EndDate, Lottery.IsDone)
- ❌ Winner selection and Card.IsWin flag modifications are atomic
- ⚠️ Can be separated **later** with eventual consistency patterns

**Responsibilities if Separated (Future Consideration):**
- Lottery lifecycle management (create, update, completion)
- Card generation and distribution
- Winner selection and tracking
- Lottery reporting and analytics
- Card pagination and sorting

**Data Ownership:**
- Lottery table (complete ownership)
- Card table (complete ownership)

**External Dependencies (Read-Only):**
- Package Service (package-lottery relationships)
- Gift Service (gift availability)
- User Service (owner/creator validation)

**Scalability Consideration:** **Keep in monolith initially**. Reason: Complex transactions and state management require tight coupling. Separate after implementing saga patterns or event sourcing (defer 6+ months).

---

#### **7. Shopping & Cart Service**
**Current Components:**
- CardCartController, CardCartService, CardCartRepository
- PackageCartController, PackageCartService, PackageCartRepository
- Models: CardCart, PackageCart

**Rationale for Keeping Coupled (for now):**
- ❌ Dual cart management (individual gifts vs. bundles)
- ❌ References both Gift and Package services
- ⚠️ High transactional coupling with checkout workflow
- ⚠️ Can be separated **later** with event-driven checkout

**Responsibilities if Separated (Future Consideration):**
- Personal gift cart management (add/remove, quantity)
- Package bundle cart management (add/remove, quantity)
- Cart summary and checkout preparation
- Cart persistence and user-specific isolation

**Data Ownership:**
- CardCart table (complete ownership)
- PackageCart table (complete ownership)

**External Dependencies (Read-Only):**
- Gift Service (gift lookup, pricing)
- Package Service (package lookup, pricing)
- User Service (user validation)

**Scalability Consideration:** **Keep in monolith initially**. Reason: Payment integration and order processing require atomic transactions. Separate after implementing transactional outbox pattern (defer 6+ months).

---

#### **8. File & Reporting Service**
**Current Components:**
- FilesController
- Integrates with LotteryService for report generation

**Rationale for Keeping Coupled (for now):**
- ⚠️ Currently integrated with LotteryService
- ✅ Can be extracted with async job queue (medium-term)
- ✅ Read-only from other services

**Responsibilities if Separated (Future Consideration):**
- Lottery report generation and export
- File storage management
- Async export job scheduling

**Scalability Consideration:** **Keep in monolith initially**. Separate after implementing async job queue (3-4 months). Can leverage blob storage (Azure Blob, S3).

---

### Separation Timeline Recommendation

```
PHASE 1 (Months 1-2): Extract Low-Coupling Services
├─ User & Identity Service ✅ (Low risk, high benefit)
├─ Donor Service ✅ (Low traffic, clear boundary)
└─ Category Service ✅ (Stateless, cache-friendly)

PHASE 2 (Months 3-4): Extract Read-Heavy Services
├─ Gift Catalog Service ✅ (High read volume)
└─ Package Service ✅ (Moderate traffic)

PHASE 3 (Months 5-6): Event-Driven & Transactional Services
├─ Shopping & Cart Service (requires event-driven patterns)
├─ File & Reporting Service (requires async job queue)
└─ Lottery & Operations Service (requires saga pattern)

PHASE 4 (Months 7-9): Optimization & Monitoring
├─ Service mesh implementation
├─ Distributed tracing
├─ Cross-service caching
└─ Performance tuning
```

---

## Communication Strategies Between Services

### Strategy 1: Synchronous REST/HTTP APIs (Recommended for Phases 1-2)

**When to Use:**
- Phase 1 services with low coupling
- Phase 2 read-heavy catalog services
- Real-time query requirements

**Architecture:**
```
Service A → HTTP GET/POST → Service B
   ↓
   Response (immediate)
```

**Implementation Details:**

| Aspect | Recommendation |
|--------|---|
| **HTTP Protocol** | REST with JSON payloads |
| **Client Library** | HttpClient with Polly for resilience (retry, circuit breaker) |
| **Response Format** | Standard DTO format (consistent with current system) |
| **Error Handling** | HTTP status codes + standardized error response body |
| **Timeout Policy** | 10-second default timeout, configurable per endpoint |
| **Retry Strategy** | Exponential backoff (1s, 2s, 4s) for transient failures |
| **Circuit Breaker** | Break after 5 consecutive failures, 60-second cooldown |

**Example Configuration (Pseudo-code):**
```csharp
// In User Service
services.AddHttpClient<IGiftServiceClient>()
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(60)));
```

**Advantages:**
- ✅ Simple to implement and debug
- ✅ Immediate feedback
- ✅ No additional infrastructure (beyond services)

**Disadvantages:**
- ❌ Tight coupling (if Service B is down, Service A fails)
- ❌ Cascading failures
- ❌ High latency with deep call chains

---

### Strategy 2: Asynchronous Message Queue (Recommended for Phases 3+)

**When to Use:**
- Phase 3 services with transactional requirements
- Decoupled event propagation
- Non-real-time operations (reporting, exports)

**Architecture:**
```
Service A → Publish Event → Message Bus (RabbitMQ/Service Bus)
                              ↓
                            Service B consumes
                            Service C consumes (fanout)
   ↓
   Confirmation (eventual)
```

**Message Bus Options:**

| Platform | Recommendation | Cost | Complexity |
|----------|---|---|---|
| **RabbitMQ** | ✅ Recommended (self-hosted) | Low | Medium |
| **Azure Service Bus** | ✅ Recommended (cloud) | Medium | Low |
| **Amazon SQS/SNS** | ⚠️ Alternative (cloud) | Low-Medium | Medium |
| **NServiceBus** | ⚠️ Framework wrapper | Higher | Medium |

**Event Types to Publish:**

| Event | Source | Subscribers | Purpose |
|-------|--------|-------------|---------|
| `UserCreatedEvent` | User Service | Gift Service, Cart Service | Update user contexts |
| `LotteryCompletedEvent` | Lottery Service | Reporting Service, User Service | Trigger notifications, update UI |
| `GiftCatalogUpdatedEvent` | Gift Service | Cart Service, Package Service | Update availability |
| `DonorAddedEvent` | Donor Service | Gift Service | Link new donor to gifts |
| `CategoryUpdatedEvent` | Category Service | Gift Service | Update gift categorization |
| `PackageCreatedEvent` | Package Service | Cart Service | Enable package purchases |

**Example: Gift Service Publishes Event**
```csharp
// Gift Service publishes
public class GiftCatalogUpdatedEvent
{
    public int GiftId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPackageAble { get; set; }
}

// Cart Service subscribes
public class GiftCatalogUpdatedEventHandler : IEventHandler<GiftCatalogUpdatedEvent>
{
    public async Task Handle(GiftCatalogUpdatedEvent @event)
    {
        // Update gift cache or invalidate cart items with old pricing
        await _cache.InvalidateAsync($"gift:{@event.GiftId}");
    }
}
```

**Advantages:**
- ✅ Loose coupling (services don't need to know each other)
- ✅ Handles failures gracefully (retry via message bus)
- ✅ Natural scalability (multiple subscribers, load distribution)

**Disadvantages:**
- ❌ Eventual consistency (no immediate guarantees)
- ❌ More complex debugging and tracing
- ❌ Additional infrastructure (message broker)

---

### Strategy 3: API Gateway Pattern (Recommended for All Phases)

**When to Use:**
- All phases (central entry point)
- Client-to-service routing
- Cross-cutting concerns

**Implementation:**
```
Client → API Gateway (routing, auth, rate limiting)
            ↓
         ├─ User Service
         ├─ Gift Service
         ├─ Lottery Service
         └─ ... (other services)
```

**API Gateway Responsibilities:**
- Route requests to appropriate microservices
- Enforce authentication at gateway level
- Apply rate limiting per user/API key
- Transform responses into consistent format
- Log all requests
- Implement CORS policies

**Technology Options:**

| Technology | Recommendation | Cost | Setup |
|-----------|---|---|---|
| **YARP** (Yet Another Reverse Proxy) | ✅ Recommended (.NET-native) | Free | Low |
| **Ocelot** | ✅ Recommended (.NET) | Free | Low |
| **Kong** | ⚠️ Alternative (language-agnostic) | Free (OSS) | Medium |
| **Azure API Management** | ✅ Cloud option | Medium | Very Low |

**Example YARP Configuration:**
```json
{
  "ReverseProxy": {
    "Routes": {
      "userService": {
        "ClusterId": "users",
        "Match": { "Path": "/api/users/**" }
      },
      "giftService": {
        "ClusterId": "gifts",
        "Match": { "Path": "/api/gifts/**" }
      }
    },
    "Clusters": {
      "users": {
        "Destinations": {
          "user1": { "Address": "https://user-service:5001" }
        }
      },
      "gifts": {
        "Destinations": {
          "gift1": { "Address": "https://gift-service:5002" }
        }
      }
    }
  }
}
```

---

### Strategy 4: Service-to-Service Authentication

**When to Use:**
- All phases (secure inter-service communication)

**Implementation Approaches:**

| Approach | Recommendation | Security | Complexity |
|----------|---|---|---|
| **Mutual TLS (mTLS)** | ✅ Recommended | Highest | High |
| **API Keys** | ⚠️ Development only | Medium | Low |
| **Service Tokens (JWT)** | ⚠️ Alternative | High | Medium |
| **Service Mesh (Istio)** | ⚠️ Advanced | Highest | Very High |

**Recommended: Mutual TLS via Service Mesh (Phase 4)**
```
Service A ─(TLS + certificate)─→ Service B
         ← (verified + authorized) ─
```

**Development Phase: API Keys**
```csharp
// Service B validates incoming requests
services.AddAuthentication("ApiKey")
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        "ApiKey", options => { });

// Service A includes key in requests
client.DefaultRequestHeaders.Add("X-API-Key", _config["Services:GiftService:ApiKey"]);
```

---

### Communication Matrix (Services & Dependencies)

```
SERVICE              →  User | Donor | Category | Gift | Package | Lottery | Cart | File
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
User                 |  —    |  —    |  —       |  —   |  —      |  —      |  ✓   |  —
Donor                |  —    |  —    |  —       |  ✓   |  —      |  —      |  —   |  —
Category             |  —    |  —    |  —       |  ✓   |  —      |  —      |  —   |  —
Gift                 |  —    |  —    |  —       |  —   |  ✓      |  ✓      |  ✓   |  —
Package              |  —    |  —    |  —       |  —   |  —      |  ✓      |  ✓   |  —
Lottery              |  —    |  —    |  —       |  —   |  —      |  —      |  ✓   |  ✓
Cart                 |  ✓    |  —    |  —       |  ✓   |  ✓      |  —      |  —   |  —
File                 |  —    |  —    |  —       |  —   |  —      |  —      |  —   |  —

Legend:
✓ = Dependency (reads from or queries)
— = No dependency

PREFERRED PROTOCOL:
Phases 1-2: REST/HTTP with Polly resilience
Phases 3+: Async messages + REST for queries
Phase 4: Service mesh (mTLS, observability)
```

---

## Shared Infrastructure Considerations

### 1. Authentication & Authorization Service

**Current State:** JWT token generation in User Service

**Microservices State:**
- User Service owns token generation
- All services validate tokens independently
- Services trust JWT signature (asymmetric key distribution)

**Key Management:**
```
User Service (RSA Private Key)
    ↓
    Signs JWT
    ↓
Distribute Public Key to all services (via config or key management service)
    ↓
Each service validates signature locally
```

**Implementation:**
```csharp
// User Service: Token generation
var token = _tokenService.GenerateToken(user);

// Token payload
{
  "userId": "123",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "isAdmin": false,
  "iat": 1715592000,
  "exp": 1715677200  // 90 minutes
}

// All services: Validate token
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://user-service:5001";
        options.Audience = "chineseSaleApi";
    });
```

**Recommended:** Each service caches the public key to reduce lookups.

---

### 2. Centralized Logging & Observability

**Current State:** Serilog logging in Program.cs

**Microservices State:** Distributed logging across services

**Implementation Stack:**

| Component | Technology | Purpose |
|-----------|-----------|---------|
| **Log Aggregation** | ELK Stack (Elasticsearch + Logstash + Kibana) or Azure Application Insights | Centralize logs from all services |
| **Distributed Tracing** | Jaeger or OpenTelemetry | Track requests across services |
| **Metrics** | Prometheus + Grafana | Monitor service health and performance |
| **Structured Logging** | Serilog with JSON output | Machine-readable logs |

**Log Correlation:**
```csharp
// Each request gets unique trace ID
var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

// Include in all log entries and service calls
logger.LogInformation("Processing request", new { TraceId = traceId });

// Pass to downstream services
httpClient.DefaultRequestHeaders.Add("X-Trace-Id", traceId);
```

**Example: Distributed Trace Flow**
```
User Service receives request (trace-id: abc123)
    ↓ logs: "UserCreated" (trace-id: abc123)
    ↓ calls Gift Service with trace-id: abc123
Gift Service processes
    ↓ logs: "CatalogUpdated" (trace-id: abc123)
    ↓ calls Category Service with trace-id: abc123
Category Service processes
    ↓ logs: "CategoryLinked" (trace-id: abc123)

All visible in centralized dashboard with full request journey.
```

---

### 3. Distributed Caching

**Current State:** No distributed cache (in-memory only)

**Microservices State:** Required to avoid cascading reads

**Caching Strategy:**

| Layer | Technology | Use Case |
|-------|-----------|----------|
| **Distributed Cache** | Redis (primary recommendation) | Shared across all services |
| **Local Cache** | MemoryCache + expiration | Service-specific, short-lived data |
| **CDN Cache** | Azure CDN or CloudFlare | Static assets (gift images) |

**Cache Invalidation Pattern:**
```
Gift Service updates gift price
    ↓
    Publishes: GiftPriceUpdatedEvent
    ↓
Redis cache key invalidated: gift:123
    ↓
Cart Service receives event
    ↓
    Invalidates: cart:*:gift:123
```

**Recommended Cache Keys:**
```
gift:{giftId}
category:{categoryId}
package:{packageId}
donor:{donorId}
user:{userId}:profile
lottery:{lotteryId}:report
```

---

### 4. Configuration Management

**Current State:** appsettings.json (environment-specific)

**Microservices State:** Centralized configuration service

**Implementation:**

| Approach | Recommendation | Cost | Complexity |
|----------|---|---|---|
| **Azure App Configuration** | ✅ Recommended (cloud) | Low | Very Low |
| **Consul** | ⚠️ Self-hosted | None | Medium |
| **Spring Cloud Config** | ❌ Java-focused | None | Medium |
| **Environment Variables + Secrets Manager** | ✅ Recommended (simple) | Low | Low |

**Recommended: Environment Variables + Azure Key Vault**
```
Development:  appsettings.Development.json (local)
Staging:      Azure App Configuration + Key Vault
Production:   Azure App Configuration + Key Vault (encrypted)
```

---

### 5. Circuit Breaker & Resilience Patterns

**Technology:** Polly library (already understood by team)

**Policy Application:**
```csharp
// Each service-to-service call uses resilience policies
var policy = Policy
    .Handle<HttpRequestException>()
    .Or<TaskCanceledException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)))
    .WrapAsync(
        Policy.CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(60)));

await policy.ExecuteAsync(() => client.GetGiftAsync(giftId));
```

---

### 6. Data Consistency & Saga Pattern

**Current State:** Single database ensures immediate consistency

**Microservices State:** Distributed transactions required (Phase 3+)

**Saga Pattern (Orchestration):**
```
Cart Service initiates checkout saga
    ↓
    Step 1: Reserve inventory (Gift Service)
    ↓
    Step 2: Process payment (Payment Service)
    ↓
    Step 3: Create order (Order Service)
    ↓
    Step 4: Update lottery stats (Lottery Service)

If any step fails → Compensating transactions (rollback steps)
```

**Recommended Implementation:** MassTransit or NServiceBus (support .NET, saga patterns out-of-box)

---

## Database Separation Strategy

### Current Database Architecture

**Single SQL Server Database:**
- 10 tables (User, Address, Lottery, Card, Gift, Donor, Category, Package, CardCart, PackageCart)
- Foreign key relationships
- Transactional consistency guarantees

---

### Proposed Database Separation

#### **Phase 1: User & Identity Database**
```
USER_DB (SQL Server)
├── dbo.User
├── dbo.Address (user addresses only)
└── Indexes: User.UserName (unique), User.Email (unique)
```

**Migration Path:**
1. Create new SQL Server database: `ChineseSaleApi_Users`
2. Copy User and Address tables (user context only)
3. Add foreign key constraints
4. Update User Service connection string
5. Archive old tables (marked as deprecated)

**Ownership:** User & Identity Service team

---

#### **Phase 2: Donor & Catalog Databases**

**DONOR_DB:**
```
DONOR_DB (SQL Server)
├── dbo.Donor
├── dbo.Address (company addresses only)
└── Indexes: Donor.Name, Address lookup
```

**CATALOG_DB:**
```
CATALOG_DB (SQL Server)
├── dbo.Category
├── dbo.Gift
├── dbo.Package
└── Relationships: Gift → Category, Gift → Donor (FK to DONOR_DB), Package → Lottery (FK - cross-database)
```

**Cross-Database Joins:**
- Use SQL Server's distributed queries or application-level joins
- Example: `Gift.DonorId` → Query DONOR_DB for donor details
- Cache donor data locally to avoid cross-DB calls

**Ownership:** Donor Service team (Donor DB), Gift & Package Service team (Catalog DB)

---

#### **Phase 3: Lottery & Operations Database**

**LOTTERY_DB:**
```
LOTTERY_DB (SQL Server)
├── dbo.Lottery
├── dbo.Card
└── Indexes: Card.LotteryId, Card.UserId, Card.IsWin
```

**Relationships:**
- `Card.GiftId` → Reference CATALOG_DB.Gift (cross-database FK)
- `Card.UserId` → Reference USER_DB.User (cross-database FK)
- `Lottery.PackageId` → Reference CATALOG_DB.Package (cross-database FK)

**Ownership:** Lottery Service team

---

#### **Phase 4: Shopping & Cart Database**

**CART_DB:**
```
CART_DB (SQL Server)
├── dbo.CardCart
├── dbo.PackageCart
└── Relationships: CardCart → USER_DB.User, CardCart → CATALOG_DB.Gift
```

**Ownership:** Cart Service team

---

### Database Migration Strategy

**Step 1: Create New Databases**
```sql
-- Create USER_DB
CREATE DATABASE [ChineseSaleApi_Users];

-- Create DONOR_DB
CREATE DATABASE [ChineseSaleApi_Donors];

-- Create CATALOG_DB
CREATE DATABASE [ChineseSaleApi_Catalog];

-- Create LOTTERY_DB (defer to Phase 3)
CREATE DATABASE [ChineseSaleApi_Lottery];

-- Create CART_DB (defer to Phase 4)
CREATE DATABASE [ChineseSaleApi_Cart];
```

**Step 2: Schema Migration**
```csharp
// In User Service Program.cs
services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer("Server=..;Database=ChineseSaleApi_Users;..."));

// In Donor Service Program.cs
services.AddDbContext<DonorDbContext>(options =>
    options.UseSqlServer("Server=..;Database=ChineseSaleApi_Donors;..."));

// ... etc for each service
```

**Step 3: Data Migration (dual-write pattern for safety)**
```
Phase A (Preparation):
├─ Deploy new services with dual writes
├─ Write to both OLD monolith DB and NEW service DB
├─ Read from OLD DB (maintain consistency)
└─ Monitor data sync

Phase B (Cutover):
├─ Validate data consistency between DBs
├─ Update services to read from NEW DB
├─ Maintain dual-write for 1-2 weeks (rollback safety)
├─ Monitor for issues
└─ Remove old tables after validation period

Phase C (Cleanup):
└─ Archive old monolith database
```

---

### Cross-Database Reference Handling

**Challenge:** Foreign keys cannot span databases in SQL Server

**Solution 1: Application-Level Foreign Keys (Recommended)**
```csharp
// Gift Service queries Donor Service instead of direct DB join
public class GiftService
{
    private readonly IDonorServiceClient _donorClient;
    
    public async Task<GiftWithDonorDto> GetGiftWithDonorAsync(int giftId)
    {
        var gift = await _giftRepository.GetByIdAsync(giftId);
        var donor = await _donorClient.GetDonorAsync(gift.DonorId);
        
        return new GiftWithDonorDto
        {
            Gift = gift,
            Donor = donor
        };
    }
}
```

**Solution 2: Replicated Reference Data (for read-heavy)**
```csharp
// Gift Service maintains local read-only copy of Donor data
// Receives updates via DonorUpdatedEvent message
public class DonorSyncedToGiftService
{
    public int DonorId { get; set; }
    public string Name { get; set; }
    public string CompanyAddress { get; set; }
}

// When Donor Service publishes DonorUpdatedEvent:
public class DonorUpdatedEventHandler
{
    public async Task Handle(DonorUpdatedEvent @event)
    {
        var syncedDonor = new DonorSyncedToGiftService
        {
            DonorId = @event.DonorId,
            Name = @event.Name,
            CompanyAddress = @event.CompanyAddress
        };
        
        await _context.DonorSyncedToGiftService.AddOrUpdateAsync(syncedDonor);
        await _context.SaveChangesAsync();
    }
}
```

**Recommended:** Solution 1 for critical paths (join queries), Solution 2 for high-volume reads (performance).

---

### Data Consistency Guarantees

| Scenario | Guarantee | Implementation |
|----------|-----------|---|
| **User updates profile** | Immediate (same DB) | SQL Server transaction |
| **Gift price changes, Cart reflects update** | Eventual (cross-DB) | Event-driven + Cache invalidation |
| **Lottery completes, Card marked IsWin** | Immediate (same DB) | SQL Server transaction |
| **Lottery report generated, Gift stats updated** | Eventual (async job) | Message queue + job scheduler |

---

## Deployment Considerations

### Deployment Architecture (Target State)

```
┌─────────────────────────────────────────┐
│         Client Applications             │
│    (Web, Mobile, Third-party)           │
└────────────────┬────────────────────────┘
                 │
         ┌───────▼─────────┐
         │  API Gateway    │
         │   (YARP/Ocelot) │
         └───┬─────────────┘
             │
    ┌────────┼────────┐
    │        │        │
    ▼        ▼        ▼
┌────────────────────────────────────────────┐
│       Kubernetes Cluster (AKS)             │
│                                            │
│  Namespace: production                     │
│  ├─ Pod: user-service (3 replicas)        │
│  ├─ Pod: gift-service (3 replicas)        │
│  ├─ Pod: lottery-service (2 replicas)     │
│  ├─ Pod: cart-service (3 replicas)        │
│  ├─ Pod: donor-service (1 replica)        │
│  ├─ Pod: category-service (1 replica)     │
│  ├─ Pod: package-service (2 replicas)     │
│  └─ Pod: file-service (1 replica)         │
│                                            │
│  ├─ Redis (StatefulSet)                   │
│  ├─ RabbitMQ (StatefulSet)                │
│  └─ Ingress Controller                    │
└────────────────────────────────────────────┘
         │         │         │
         ▼         ▼         ▼
┌──────────────────────────────────────────┐
│   Persistent Storage                     │
│  ├─ SQL Server (Azure SQL Database)      │
│  │  ├─ USER_DB                           │
│  │  ├─ CATALOG_DB                        │
│  │  ├─ LOTTERY_DB                        │
│  │  └─ CART_DB                           │
│  │                                        │
│  ├─ Redis (for distributed cache)        │
│  └─ Azure Blob Storage (for files)       │
└──────────────────────────────────────────┘
```

---

### Containerization Strategy

**Dockerfile Per Service:**

```dockerfile
# Dockerfile for each microservice (e.g., user-service)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ChineseSaleApi.csproj", "."]
RUN dotnet restore "ChineseSaleApi.csproj"
COPY . .
RUN dotnet build "ChineseSaleApi.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "ChineseSaleApi.dll"]

EXPOSE 5001
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5001/health || exit 1
```

**Key Points:**
- Multi-stage build (reduce image size)
- Health check endpoint (/health) for Kubernetes liveness probe
- Expose service port

---

### Kubernetes Deployment Manifests

**Example: User Service Deployment**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: user-service
  namespace: production
spec:
  replicas: 3
  selector:
    matchLabels:
      app: user-service
  template:
    metadata:
      labels:
        app: user-service
    spec:
      containers:
      - name: user-service
        image: registry.azurecr.io/chineseSaleApi/user-service:1.0.0
        ports:
        - containerPort: 5001
        env:
        - name: ConnectionStrings__UserDb
          valueFrom:
            secretKeyRef:
              name: user-db-secret
              key: connectionString
        - name: JWT_SECRET
          valueFrom:
            secretKeyRef:
              name: jwt-secret
              key: secret
        livenessProbe:
          httpGet:
            path: /health
            port: 5001
          initialDelaySeconds: 10
          periodSeconds: 30
        readinessProbe:
          httpGet:
            path: /ready
            port: 5001
          initialDelaySeconds: 5
          periodSeconds: 10
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
---
apiVersion: v1
kind: Service
metadata:
  name: user-service
  namespace: production
spec:
  selector:
    app: user-service
  ports:
  - port: 5001
    targetPort: 5001
  type: ClusterIP
```

---

### Service Discovery

**In Kubernetes:**
- Services register automatically in DNS
- Service-to-service calls use DNS name: `http://gift-service:5001/api/gifts`
- Internal DNS resolver handles load balancing

**Configuration:**
```csharp
// User Service calls Gift Service
services.AddHttpClient<IGiftServiceClient>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("http://gift-service:5001");
    });
```

---

### Continuous Integration / Deployment (CI/CD)

**Pipeline Stages:**

```
1. Trigger (on git push to main)
   ↓
2. Build services (parallel)
   ├─ user-service build
   ├─ gift-service build
   ├─ lottery-service build
   └─ ... (others)
   ↓
3. Run unit tests (parallel)
   ├─ user-service tests
   ├─ gift-service tests
   └─ ... (others)
   ↓
4. Run integration tests
   ├─ User ↔ Gift interaction
   ├─ Gift ↔ Lottery interaction
   └─ ... (cross-service tests)
   ↓
5. Security scanning (SAST)
   ├─ Dependency scanning
   ├─ Code quality (SonarQube)
   └─ Container scanning
   ↓
6. Build Docker images (parallel)
   ├─ user-service:hash
   ├─ gift-service:hash
   └─ ... (others)
   ↓
7. Push to container registry (Azure ACR)
   ↓
8. Deploy to staging (AKS)
   ├─ Smoke tests
   ├─ Performance tests
   └─ Manual approval
   ↓
9. Deploy to production (AKS)
   ├─ Blue-green deployment
   ├─ Health checks
   └─ Rollback on failure
```

**Recommended Tools:**
- **CI/CD Platform:** Azure Pipelines (integrated with Azure)
- **Container Registry:** Azure Container Registry (ACR)
- **Kubernetes:** Azure Kubernetes Service (AKS)

---

### Deployment Strategies

#### **Blue-Green Deployment (Recommended)**
```
Blue Environment (Current Production)
├─ user-service v1.0.0
├─ gift-service v1.0.0
└─ ...

Green Environment (New Deployment)
├─ user-service v1.1.0
├─ gift-service v1.1.0
└─ ...

Load Balancer routes:
  100% → Blue (until Green validated)
  50% → Blue, 50% → Green (canary phase)
  100% → Green (cutover complete)
```

#### **Rolling Update (Alternative)**
```
Gradually replace old pods with new:
Iteration 1: Remove 1 old pod, add 1 new pod
Iteration 2: Remove 1 old pod, add 1 new pod
...
Result: Zero downtime, progressive rollout
```

**Kubernetes Rolling Update:**
```yaml
spec:
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
```

---

### Scaling Strategy

#### **Horizontal Scaling (Pod Replicas)**

**High-Traffic Services (scale up):**
- Gift Service: 3-5 replicas (customers browse constantly)
- Cart Service: 3-5 replicas (concurrent checkouts)
- User Service: 3-4 replicas (authentication bottleneck)

**Low-Traffic Services (minimal replicas):**
- Donor Service: 1-2 replicas (admin operations)
- Category Service: 1-2 replicas (static data, highly cacheable)
- File Service: 1-2 replicas (batch job runner)

**Auto-Scaling Policy (Kubernetes HPA):**
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: gift-service-autoscaler
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: gift-service
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

#### **Vertical Scaling (Resource Limits)**

Per service configuration (CPU, Memory):

| Service | Min CPU | Max CPU | Min Memory | Max Memory |
|---------|---------|---------|-----------|-----------|
| User Service | 250m | 500m | 256Mi | 512Mi |
| Gift Service | 250m | 1000m | 512Mi | 1Gi |
| Lottery Service | 500m | 2000m | 512Mi | 2Gi |
| Cart Service | 250m | 750m | 256Mi | 512Mi |
| Donor Service | 100m | 250m | 128Mi | 256Mi |
| Category Service | 100m | 250m | 128Mi | 256Mi |
| File Service | 500m | 2000m | 512Mi | 2Gi |

---

### Monitoring, Logging & Alerting

**Stack:**

| Component | Technology | Purpose |
|-----------|-----------|---------|
| **Metrics** | Prometheus | Service health, CPU, memory, request rates |
| **Logging** | ELK (Elasticsearch, Logstash, Kibana) | Centralized log aggregation |
| **Tracing** | Jaeger or Azure App Insights | Distributed request tracing |
| **Alerting** | AlertManager (Prometheus) or Azure Alerts | Threshold violations, SLA breaches |
| **Dashboards** | Grafana | Real-time service visibility |

**Key Metrics to Monitor:**

```
Per Service:
├─ Request rate (req/sec)
├─ Response time (p50, p95, p99)
├─ Error rate (% of 5xx responses)
├─ CPU usage (%)
├─ Memory usage (%)
└─ Database query time (ms)

Cross-Service:
├─ API Gateway latency
├─ Service-to-service call latency
├─ Message queue depth
├─ Cache hit rate (Redis)
└─ Database connection pool exhaustion
```

**Alert Examples:**
```
- User Service error rate > 5% for 5 minutes → Page on-call
- Gift Service response time p95 > 2 seconds → Investigate
- Redis CPU > 80% for 10 minutes → Scale or optimize queries
- Message queue backlog > 1000 messages → Trigger autoscaling
```

---

### Disaster Recovery & Backup Strategy

#### **Database Backups**

**SQL Server Backup Schedule:**
- Full backup: Daily at 2 AM
- Incremental backup: Every 4 hours
- Transaction log backup: Every 15 minutes

**Retention Policy:**
- Daily backups: 30 days
- Weekly backups: 12 weeks
- Monthly backups: 12 months

**Geo-Replication:**
- Primary region: East US
- Secondary region: West US (read-only replica)
- RPO (Recovery Point Objective): 15 minutes (acceptable)
- RTO (Recovery Time Objective): 1 hour (acceptable)

#### **Container Registry Backup**

**Azure ACR Replication:**
```
Primary Registry (East US)
    ├─ auto-replicate to
    └─ Secondary Registry (West US)
```

#### **Configuration Backup**

**Azure App Configuration Snapshots:**
- Automated snapshots every 6 hours
- Point-in-time restore capability
- 30-day retention

#### **Failover Procedure**

```
Normal State: Primary Datacenter (East US)
    ├─ Kubernetes Cluster (primary)
    ├─ SQL Server (primary, read-write)
    └─ Redis (primary)

Disaster Detected:
    └─ Automatic failover to Secondary Datacenter (West US)
        ├─ Kubernetes Cluster (standby → active)
        ├─ SQL Server (replica → read-write)
        └─ Redis (failover)

DNS Update (Azure Traffic Manager):
    └─ Route traffic from primary to secondary
    └─ RTO: ~5 minutes

Validation:
    ├─ Health checks pass on secondary
    ├─ Data consistency verified
    └─ Alerts to ops team
```

---

## Implementation Roadmap

### Phase 1 (Months 1-2): Extract Low-Coupling Services

**Goals:**
- Deploy 3 independent microservices
- Establish inter-service communication patterns
- Set up monitoring and logging

**Deliverables:**
1. **User & Identity Service**
   - [ ] Create new solution: `ChineseSaleApi.UserService`
   - [ ] Create USER_DB database
   - [ ] Implement JWT token generation
   - [ ] Deploy to Kubernetes

2. **Donor Service**
   - [ ] Create new solution: `ChineseSaleApi.DonorService`
   - [ ] Create DONOR_DB database
   - [ ] Implement donor CRUD endpoints
   - [ ] Deploy to Kubernetes

3. **Category Service**
   - [ ] Create new solution: `ChineseSaleApi.CategoryService`
   - [ ] Create CATEGORY_DB database (or shared CATALOG_DB)
   - [ ] Implement category CRUD endpoints
   - [ ] Deploy to Kubernetes

4. **API Gateway**
   - [ ] Set up YARP routing configuration
   - [ ] Configure authentication middleware
   - [ ] Deploy as central entry point

5. **Infrastructure**
   - [ ] Configure Azure Container Registry
   - [ ] Set up Kubernetes cluster (AKS)
   - [ ] Configure Redis for distributed caching
   - [ ] Set up ELK stack for logging

**Success Criteria:**
- All 3 services independently deployable
- Inter-service HTTP calls with Polly resilience
- Health checks passing
- Logs aggregated in ELK stack
- Zero downtime during deployments

---

### Phase 2 (Months 3-4): Extract Read-Heavy Services

**Goals:**
- Deploy 2 high-traffic catalog services
- Implement distributed caching
- Optimize for read-heavy workloads

**Deliverables:**
1. **Gift Catalog Service**
   - [ ] Create new solution: `ChineseSaleApi.GiftService`
   - [ ] Create CATALOG_DB database
   - [ ] Implement gift CRUD + search + pagination
   - [ ] Integrate with Redis caching
   - [ ] Deploy with 3-5 replicas

2. **Package Service**
   - [ ] Create new solution: `ChineseSaleApi.PackageService`
   - [ ] Use shared CATALOG_DB or separate database
   - [ ] Implement package CRUD endpoints
   - [ ] Deploy with 2 replicas

3. **Event-Driven Communication**
   - [ ] Set up RabbitMQ or Azure Service Bus
   - [ ] Implement domain events (e.g., `GiftCreatedEvent`)
   - [ ] Configure pub/sub handlers across services
   - [ ] Test eventual consistency flows

4. **Distributed Caching Strategy**
   - [ ] Configure Redis clusters
   - [ ] Implement cache-aside pattern
   - [ ] Set up cache invalidation on updates
   - [ ] Monitor cache hit rates

**Success Criteria:**
- Gift and Package services independently deployable
- Asynchronous event propagation working
- Cache hit rate > 80% for read operations
- Response time for gift catalog < 200ms (p95)
- Horizontal scaling verified

---

### Phase 3 (Months 5-6): Event-Driven Transactional Services

**Goals:**
- Implement saga pattern for distributed transactions
- Deploy Shopping Cart and File/Reporting services
- Ensure data consistency across databases

**Deliverables:**
1. **Shopping & Cart Service**
   - [ ] Create new solution: `ChineseSaleApi.CartService`
   - [ ] Create CART_DB database
   - [ ] Implement cart CRUD endpoints
   - [ ] Integrate saga pattern for checkout
   - [ ] Deploy with 3 replicas

2. **File & Reporting Service**
   - [ ] Create new solution: `ChineseSaleApi.FileService`
   - [ ] Implement async export jobs
   - [ ] Integrate with blob storage (Azure Blob)
   - [ ] Deploy with 1 replica

3. **Saga Pattern Implementation**
   - [ ] Define checkout saga steps
   - [ ] Implement compensating transactions
   - [ ] Handle failure scenarios
   - [ ] Add distributed tracing for sagas

4. **Data Consistency Verification**
   - [ ] Implement audit logs for cross-DB transactions
   - [ ] Set up alerts for consistency violations
   - [ ] Perform chaos engineering tests

**Success Criteria:**
- Cart service handles concurrent checkouts
- Saga pattern ensures atomicity across services
- Checkout latency < 2 seconds (p95)
- Zero data inconsistencies in production
- File exports complete within SLA

---

### Phase 4 (Months 7-9): Optimization & Advanced Patterns

**Goals:**
- Deploy Lottery Service with complex state management
- Implement service mesh for advanced observability
- Optimize for scale and resilience

**Deliverables:**
1. **Lottery & Operations Service**
   - [ ] Create new solution: `ChineseSaleApi.LotteryService`
   - [ ] Create LOTTERY_DB database
   - [ ] Implement complex lottery state machine
   - [ ] Implement winner selection algorithm
   - [ ] Deploy with 2 replicas

2. **Service Mesh (Istio)**
   - [ ] Deploy Istio to Kubernetes cluster
   - [ ] Configure mutual TLS (mTLS) for all services
   - [ ] Implement traffic policies and circuit breakers
   - [ ] Enable distributed tracing (Jaeger)

3. **Advanced Monitoring**
   - [ ] Deploy Grafana dashboards for each service
   - [ ] Implement SLI/SLO tracking
   - [ ] Configure AlertManager rules
   - [ ] Set up incident response workflows

4. **Performance Optimization**
   - [ ] Identify bottlenecks using distributed tracing
   - [ ] Optimize database queries
   - [ ] Implement read replicas for heavy queries
   - [ ] Cache warming strategies

**Success Criteria:**
- Lottery service handles high-volume operations
- mTLS enforced across all service-to-service communication
- Distributed tracing shows full request journeys
- SLO: 99.5% availability, < 500ms p99 latency
- Successful incident detection and auto-remediation

---

### Phase 5 (Ongoing): Maintenance & Continuous Improvement

**Goals:**
- Establish operational excellence
- Implement feedback loops
- Continuously optimize

**Ongoing Tasks:**
- Monthly performance reviews
- Quarterly architecture reviews
- Security patching and updates
- Training and documentation
- Cost optimization analysis

---

## Risk Assessment

### Risk 1: Distributed System Complexity
**Probability:** High | **Impact:** High

**Description:** Microservices introduce significant operational complexity (debugging, deployment, monitoring).

**Mitigation:**
- Invest in observability (tracing, logging, metrics)
- Automate deployment via CI/CD
- Start with low-coupling services (Phase 1)
- Maintain detailed runbooks for each service

---

### Risk 2: Data Consistency Issues
**Probability:** Medium | **Impact:** High

**Description:** With multiple databases, ensuring data consistency becomes challenging.

**Mitigation:**
- Use event-driven architecture for eventual consistency
- Implement saga pattern with compensating transactions
- Audit logs for all distributed operations
- Regular data consistency checks (nightly jobs)

---

### Risk 3: Network Latency & Failures
**Probability:** High | **Impact:** Medium

**Description:** Service-to-service calls may fail or become slow.

**Mitigation:**
- Implement Polly resilience policies (retry, circuit breaker)
- Cache frequently-accessed data
- Implement timeouts and async patterns
- Service mesh (Phase 4) for advanced resilience

---

### Risk 4: Authentication Complexity
**Probability:** Medium | **Impact:** Medium

**Description:** Managing JWT tokens across multiple services is error-prone.

**Mitigation:**
- Centralized token validation at API Gateway
- Consistent JWT payload structure across all services
- Key rotation procedures
- Regular security audits

---

### Risk 5: Cost Escalation
**Probability:** Medium | **Impact:** Medium

**Description:** Microservices infrastructure (Kubernetes, multiple databases, monitoring) can be expensive.

**Mitigation:**
- Right-size compute resources
- Use auto-scaling to avoid over-provisioning
- Monitor cloud spending monthly
- Consider reserved instances for predictable workloads
- Cost analysis before each phase

---

### Risk 6: Backwards Compatibility
**Probability:** Medium | **Impact:** Low

**Description:** Breaking API changes between services cause cascading failures.

**Mitigation:**
- API versioning strategy (e.g., /v1, /v2)
- Deprecation warnings (announce breaking changes 2 quarters in advance)
- Contract testing (ensure API contracts don't break)
- Blue-green deployments for safe rollouts

---

### Risk 7: Team Skill Gap
**Probability:** Medium | **Impact:** High

**Description:** Team lacks experience with microservices, Kubernetes, distributed systems.

**Mitigation:**
- Invest in training (Kubernetes, Docker, distributed systems)
- Hire external consultants for phases 1-2
- Document architecture and operational procedures
- Gradual migration (don't rush to full microservices)

---

## Conclusion

This document provides a **strategic roadmap** for evolving the ChineseSaleApi from a monolithic architecture to a microservices-based system. The plan prioritizes:

1. **Early wins** (Phase 1: Low-coupling services) to build confidence and skills
2. **Scalability** (Phase 2: Read-heavy services) to handle growth
3. **Resilience** (Phase 3-4: Transactional & complex services) for reliability
4. **Operational excellence** (Phase 4+: Monitoring, service mesh) for long-term sustainability

The separation strategy respects current business domains, enables independent scaling, and maintains system coherence through well-defined communication patterns and shared infrastructure.

**Next Steps:**
- Stakeholder review and approval
- Team training on microservices patterns
- Phase 1 planning and resource allocation
- Proof-of-concept (User Service extraction)

---

*Document Version: 1.0*  
*Prepared for: ChineseSaleApi Development Team*  
*Status: Planning/Strategy (No Implementation)*
