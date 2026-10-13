# Invest Now — Mock Investment REST API Service

A high-performance .NET 10 Web API service engineered to power the **Invest Now — Mobile Investment Experience** Flutter application.

This service serves realistic financial investment products, calculates maturity returns and interest projections, validates investment rules, prevents duplicate submissions (via idempotency keys), and provides testing hooks for mobile UI states (loading, empty, error/retry).

---

## 🚀 Quick Start

### 1. Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### 2. Run the API Service
Run the following command from this directory:

```bash
dotnet run --launch-profile http
```

The service will start listening on all interfaces at port `5140`:
- **Base URL**: `http://localhost:5140`
- **Interactive API Docs (Scalar)**: [http://localhost:5140/scalar/v1](http://localhost:5140/scalar/v1)
- **OpenAPI 3.1 Spec**: [http://localhost:5140/openapi/v1.json](http://localhost:5140/openapi/v1.json)

---

## 📱 Connecting from Flutter

Depending on your Flutter target platform, configure your base URL in your Flutter `ApiClient` or `Dio`/`http` service:

| Platform | Recommended Base URL | Note |
| :--- | :--- | :--- |
| **Android Emulator** | `http://10.0.2.2:5140` | Android emulators map `10.0.2.2` to the host machine. |
| **iOS Simulator** | `http://localhost:5140` | Uses host `localhost` directly. |
| **Flutter Web / Desktop** | `http://localhost:5140` | Permissive CORS is enabled by default. |
| **Physical Phone (Wi-Fi)** | `http://<YOUR_LOCAL_IP>:5140` | The server listens on `0.0.0.0:5140`, accessible via local network IP (e.g. `192.168.1.X`). |

> **Note on HTTPS in Development**: By default, `app.UseHttpsRedirection()` is disabled during Development to avoid SSL handshake failures (`CERTIFICATE_VERIFY_FAILED: self-signed certificate`) on Android emulators and mobile devices. Use plain `http://` for local development.

---

## 📋 API Endpoints Overview

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `GET` | `/api/products` | Retrieve catalog of investment products (supports filtering). |
| `GET` | `/api/products/{id}` | Retrieve full details for a single investment product. |
| `POST` | `/api/investments/simulate` | Preview/calculate projected return, maturity date, and breakdown. |
| `POST` | `/api/investments` | Create and simulate an investment, returning a reference number. |
| `GET` | `/api/investments` | Retrieve list of user's active/past investments. |
| `GET` | `/api/investments/{reference}` | Retrieve details of an investment by its reference number. |
| `POST` | `/api/investments/reset` | Reset in-memory investments back to initial state. |

---

## 🛠️ Detailed Endpoint Documentation & Schemas

### 1. List Investment Products
**`GET /api/products`**

#### Query Parameters (Optional)
- `category` (string) — Filter by category (e.g., `Government Bonds`, `Fixed Income`, `Renewable Infrastructure`).
- `riskLevel` (string) — `Low`, `Medium`, or `High`.
- `search` (string) — Search name, description, category, or highlights.
- `simulateEmpty` (bool) — Pass `true` to receive `[]` to test Flutter **Empty State**.
- `simulateError` (bool) — Pass `true` to receive `500 Internal Server Error` to test Flutter **Error State & Retry**.
- `simulateDelayMs` (int) — Artificial delay (e.g., `2000`) to test Flutter **Loading Shimmers / Spinners**.

#### Sample Response (`200 OK`)
```json
[
  {
    "id": "treasury-yield-30d",
    "name": "Federal Treasury 30-Day Note",
    "category": "Government Bonds",
    "riskLevel": "Low",
    "annualInterestRate": 7.50,
    "returnRateDisplay": "7.5% p.a.",
    "durationDays": 30,
    "durationLabel": "30 Days",
    "minimumAmount": 100.00,
    "maximumAmount": 250000.00,
    "currency": "USD",
    "currencySymbol": "$",
    "shortDescription": "Backed by sovereign treasury assets with guaranteed interest upon maturity.",
    "fullDescription": "The Federal Treasury 30-Day Note is a short-term, sovereign-backed investment vehicle engineered for capital preservation...",
    "maturityTerms": "Principal and accumulated interest will be automatically settled into your primary funding account on the 30th day. Zero penalty applies after maturity.",
    "payoutFrequency": "At Maturity",
    "iconName": "account_balance",
    "highlights": [
      "Zero capital loss guarantee",
      "Daily interest accrual calculation",
      "Automated settlement upon maturity",
      "Ideal for short-term emergency reserve growth"
    ],
    "isActive": true
  }
]
```

---

### 2. Get Product Details
**`GET /api/products/{id}`**

- **URL Example**: `/api/products/fixed-deposit-90d`
- **Response**: `200 OK` with full product object, or `404 Not Found` if product does not exist.

---

### 3. Simulate / Preview Investment (Confirmation Screen)
**`POST /api/investments/simulate`** *(alias: `/api/investments/preview`)*

Use this endpoint on your Flutter confirmation screen to show exact financial projections before the user confirms submission.

#### Request Body
```json
{
  "productId": "treasury-yield-30d",
  "amount": 1000.00
}
```

#### Response (`200 OK`)
```json
{
  "productId": "treasury-yield-30d",
  "productName": "Federal Treasury 30-Day Note",
  "category": "Government Bonds",
  "riskLevel": "Low",
  "investedAmount": 1000.00,
  "annualInterestRate": 7.50,
  "returnRateDisplay": "7.5% p.a.",
  "durationDays": 30,
  "durationLabel": "30 Days",
  "expectedReturnAmount": 6.16,
  "totalPayoutAtMaturity": 1006.16,
  "startDateUtc": "2026-10-13T12:00:00Z",
  "maturityDateUtc": "2026-11-12T12:00:00Z",
  "formattedMaturityDate": "Nov 12, 2026",
  "currency": "USD",
  "currencySymbol": "$",
  "maturityTerms": "Principal and accumulated interest will be automatically settled into your primary funding account...",
  "payoutFrequency": "At Maturity",
  "summaryBreakdown": {
    "principal": 1000.00,
    "estimatedEarnings": 6.16,
    "managementFee": 0.00,
    "netEstimatedPayout": 1006.16
  }
}
```

---

### 4. Create Investment (Simulate Successful Investment)
**`POST /api/investments`**

Creates the investment and returns a simulated unique reference number.

#### Headers
- `Idempotency-Key` *(string, optional)* — A UUID or client transaction ID. Prevents accidental duplicate submissions caused by double tapping or retries.

#### Request Body
```json
{
  "productId": "treasury-yield-30d",
  "amount": 1000.00,
  "investorName": "Jane Doe",
  "fundingSource": "Main Wallet",
  "idempotencyKey": "4f1837f6-b183-4f9e-990c-512cb5d2c884"
}
```

#### Success Response (`201 Created` or `200 OK` on duplicate prevented)
```json
{
  "referenceNumber": "INV-20261013-8A2C4F",
  "status": "Active",
  "productId": "treasury-yield-30d",
  "productName": "Federal Treasury 30-Day Note",
  "category": "Government Bonds",
  "riskLevel": "Low",
  "investedAmount": 1000.00,
  "annualInterestRate": 7.50,
  "returnRateDisplay": "7.5% p.a.",
  "durationDays": 30,
  "durationLabel": "30 Days",
  "expectedReturnAmount": 6.16,
  "totalPayoutAtMaturity": 1006.16,
  "createdAtUtc": "2026-10-13T12:55:04Z",
  "maturityDateUtc": "2026-11-12T12:55:04Z",
  "formattedCreatedAt": "Oct 13, 2026 12:55:04 UTC",
  "formattedMaturityDate": "Nov 12, 2026",
  "currency": "USD",
  "currencySymbol": "$",
  "investorName": "Jane Doe",
  "fundingSource": "Main Wallet",
  "idempotencyKey": "4f1837f6-b183-4f9e-990c-512cb5d2c884",
  "wasDuplicatePrevented": false,
  "message": "Investment created successfully."
}
```

> **Duplicate Prevention**: If the user sends the same `Idempotency-Key` again, the API returns the existing record with `wasDuplicatePrevented: true` and HTTP `200 OK` rather than generating a second investment.

---

### 5. Error Handling & Validation Format
When validation fails (e.g., amount below the product minimum or negative amount), the API returns a structured HTTP `400 Bad Request`:

```json
{
  "statusCode": 400,
  "message": "Investment amount $25.00 is below the required minimum of $100.00 for Federal Treasury 30-Day Note.",
  "errorCode": "VALIDATION_FAILED",
  "validationErrors": {
    "amount": [
      "Investment amount $25.00 is below the required minimum of $100.00 for Federal Treasury 30-Day Note."
    ]
  },
  "timestampUtc": "2026-10-13T12:55:34Z"
}
```

---

## 🧮 Financial Calculation Formula

The expected return uses standard annual interest prorated for tenure:

$$\text{Interest} = \text{Round}\left(\text{Principal} \times \frac{\text{AnnualRate}}{100} \times \frac{\text{DurationDays}}{365},\; 2\right)$$

$$\text{Total Payout} = \text{Principal} + \text{Interest}$$

All calculations are rounded away from zero to 2 decimal places to guarantee financial precision.

---

## 🧪 Testing the API

Run the unit test suite:

```bash
dotnet test
```

The test suite covers:
- Financial calculation accuracy across multiple rates and tenures.
- Minimum investment amount validation.
- Missing / invalid product ID handling.
- Idempotency and duplicate submission prevention.
- Product filtering by category, risk level, and keyword search.
- Controller HTTP responses and status codes.

---

## 💡 Quick Tips for Flutter Implementation
1. **Model Generation**: Use the JSON schemas from Scalar ([http://localhost:5140/scalar/v1](http://localhost:5140/scalar/v1)) or the `.http` file ([investmentSimuoation.http](file:///c:/Users/david/source/repos/investmentSimuoation/investmentSimuoation.http)) to build your Dart models.
2. **State Management**: Test your Flutter `Bloc` / `Riverpod` / `Provider` state transitions by calling `/api/products?simulateDelayMs=1500` (loading) and `/api/products?simulateError=true` (error & retry).
3. **Double-Tap Protection**: When the user taps "Confirm Investment", generate a UUID (`Uuid().v4()`) on the Flutter client and pass it in the `Idempotency-Key` header.
