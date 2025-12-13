# Stripe Payment Integration

## Overview
Complete Stripe payment integration with payment intent creation, confirmation, refunds, and webhook support for the E-Commerce API.

## Architecture

### Service Layer
- **IPaymentService** (Application/Common/Interfaces)
  - Interface for payment processing operations
  - Abstraction over payment gateway implementation

- **StripePaymentService** (Infrastructure/Services)
  - Concrete implementation using Stripe.NET SDK
  - Handles payment intents, refunds, and webhook verification

### Configuration
```json
{
  "StripeSettings": {
    "SecretKey": "sk_test_your-stripe-secret-key",
    "PublishableKey": "pk_test_your-stripe-publishable-key",
    "WebhookSecret": "whsec_your-stripe-webhook-secret",
    "Currency": "usd"
  }
}
```

## Features

### 1. Payment Intent Creation
Creates a Stripe payment intent when an order is placed.

**Endpoint**: `POST /api/payments/create-payment-intent`

**Request Body**:
```json
{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 99.99,
  "currency": "usd",
  "paymentMethod": "Card"
}
```

**Response**:
```json
{
  "paymentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "paymentIntentId": "pi_3MtwBwLkdIwHu7ix28a3tqPa",
  "clientSecret": "pi_3MtwBwLkdIwHu7ix28a3tqPa_secret_..."
}
```

**Flow**:
1. Validates order exists and retrieves customer ID
2. Creates Stripe payment intent with amount in cents
3. Stores payment record with PENDING status
4. Returns payment intent ID and client secret for frontend confirmation

### 2. Payment Confirmation
Confirms a payment on the server side (can also be done client-side).

**Endpoint**: `POST /api/payments/confirm-payment`

**Request Body**:
```json
{
  "paymentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "paymentIntentId": "pi_3MtwBwLkdIwHu7ix28a3tqPa"
}
```

**Response**:
```json
{
  "message": "Payment confirmed successfully"
}
```

**Flow**:
1. Confirms payment with Stripe
2. Marks payment as COMPLETED in database
3. Sends real-time notification to customer via SignalR

### 3. Payment Refunds
Processes full or partial refunds for completed payments.

**Endpoint**: `POST /api/payments/{id}/refund`

**Authorization**: Admin, Manager roles only

**Flow**:
1. Validates payment is in COMPLETED status
2. Creates refund in Stripe
3. Marks payment as REFUNDED in database
4. Sends refund notification to customer

### 4. Webhook Integration
Handles asynchronous Stripe events for payment status updates.

**Endpoint**: `POST /api/payments/webhook`

**Supported Events**:
- `payment_intent.succeeded` - Payment completed successfully
- `payment_intent.payment_failed` - Payment failed
- `charge.refunded` - Charge refunded

**Security**:
- Verifies Stripe webhook signature using webhook secret
- Returns 400 Bad Request if signature is invalid

**Flow**:
1. Receives webhook event from Stripe
2. Verifies signature for security
3. Processes event based on type
4. Updates payment status in database
5. Sends appropriate notifications

## Service Methods

### IPaymentService Interface

```csharp
// Create a payment intent
Task<string> CreatePaymentIntentAsync(
    decimal amount, 
    string currency, 
    string customerId, 
    Dictionary<string, string>? metadata = null, 
    CancellationToken cancellationToken = default
)

// Confirm a payment intent
Task<bool> ConfirmPaymentAsync(
    string paymentIntentId, 
    CancellationToken cancellationToken = default
)

// Cancel a payment intent
Task<bool> CancelPaymentAsync(
    string paymentIntentId, 
    CancellationToken cancellationToken = default
)

// Create a refund
Task<string> CreateRefundAsync(
    string paymentIntentId, 
    decimal? amount = null, 
    CancellationToken cancellationToken = default
)

// Get payment details
Task<PaymentDetails?> GetPaymentDetailsAsync(
    string paymentIntentId, 
    CancellationToken cancellationToken = default
)

// Verify webhook signature
bool VerifyWebhookSignature(
    string payload, 
    string signature
)
```

## Database Schema

**Payment Entity**:
- `Id` (Guid) - Primary key
- `OrderId` (Guid) - Foreign key to Order
- `Amount` (Money) - Payment amount with currency
- `PaymentDate` (DateTime) - UTC timestamp
- `Status` (PaymentStatus) - PENDING, COMPLETED, FAILED, REFUNDED, CANCELLED
- `Method` (PaymentMethod) - Card, PayPal, etc.
- `TransactionId` (string?) - Stripe payment intent ID
- `PaymentGateway` (string?) - "Stripe"

## Integration Steps

### 1. Get Stripe API Keys
1. Create a Stripe account at https://stripe.com
2. Go to Developers > API keys
3. Copy your Test Secret Key (sk_test_...)
4. Copy your Test Publishable Key (pk_test_...)

### 2. Configure Webhook
1. Go to Developers > Webhooks
2. Click "Add endpoint"
3. Enter URL: `https://your-domain.com/api/payments/webhook`
4. Select events:
   - `payment_intent.succeeded`
   - `payment_intent.payment_failed`
   - `charge.refunded`
5. Copy the Webhook Signing Secret (whsec_...)

### 3. Update Configuration
Add keys to `appsettings.json` or environment variables:
```json
{
  "StripeSettings": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_...",
    "Currency": "usd"
  }
}
```

### 4. Frontend Integration Example (React)

Install Stripe.js:
```bash
npm install @stripe/stripe-js @stripe/react-stripe-js
```

**Checkout Component**:
```typescript
import { loadStripe } from '@stripe/stripe-js';
import { Elements, CardElement, useStripe, useElements } from '@stripe/react-stripe-js';

const stripePromise = loadStripe('pk_test_your_publishable_key');

function CheckoutForm({ orderId, amount }) {
  const stripe = useStripe();
  const elements = useElements();
  const [error, setError] = useState(null);
  const [processing, setProcessing] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setProcessing(true);

    // 1. Create payment intent on backend
    const response = await fetch('/api/payments/create-payment-intent', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        orderId,
        amount,
        currency: 'usd',
        paymentMethod: 'Card'
      })
    });

    const { paymentId, clientSecret } = await response.json();

    // 2. Confirm payment with Stripe
    const { error: stripeError, paymentIntent } = await stripe.confirmCardPayment(
      clientSecret,
      {
        payment_method: {
          card: elements.getElement(CardElement)
        }
      }
    );

    if (stripeError) {
      setError(stripeError.message);
      setProcessing(false);
      return;
    }

    // 3. Confirm payment on backend (optional - webhook will also do this)
    await fetch('/api/payments/confirm-payment', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        paymentId,
        paymentIntentId: paymentIntent.id
      })
    });

    setProcessing(false);
    // Redirect to success page
  };

  return (
    <form onSubmit={handleSubmit}>
      <CardElement />
      <button type="submit" disabled={!stripe || processing}>
        {processing ? 'Processing...' : `Pay $${amount}`}
      </button>
      {error && <div className="error">{error}</div>}
    </form>
  );
}

function Checkout({ orderId, amount }) {
  return (
    <Elements stripe={stripePromise}>
      <CheckoutForm orderId={orderId} amount={amount} />
    </Elements>
  );
}

export default Checkout;
```

## Testing

### Test Card Numbers
Stripe provides test cards for different scenarios:

**Success**:
- `4242 4242 4242 4242` - Visa
- `5555 5555 5555 4444` - Mastercard

**Failure**:
- `4000 0000 0000 0002` - Card declined

**3D Secure**:
- `4000 0025 0000 3155` - Requires authentication

Use any future expiry date and any 3-digit CVC.

### Manual Testing Flow

1. **Create Payment Intent**:
```bash
curl -X POST http://localhost:5272/api/payments/create-payment-intent \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "guid-here",
    "amount": 99.99,
    "currency": "usd"
  }'
```

2. **Check Stripe Dashboard**:
   - Go to https://dashboard.stripe.com/test/payments
   - Verify payment intent was created

3. **Test Webhook Locally** (using Stripe CLI):
```bash
# Install Stripe CLI
brew install stripe/stripe-cli/stripe

# Login
stripe login

# Forward webhooks to local endpoint
stripe listen --forward-to http://localhost:5272/api/payments/webhook

# Trigger test event
stripe trigger payment_intent.succeeded
```

## Security Considerations

1. **Never expose Secret Key**: Only use in backend, never in frontend
2. **Verify Webhook Signatures**: Always validate webhook signatures
3. **Use HTTPS**: Stripe requires HTTPS for production webhooks
4. **Idempotency**: Stripe API supports idempotency keys for safe retries
5. **PCI Compliance**: Using Stripe.js keeps card data off your servers

## Error Handling

The service includes comprehensive error handling:
- Invalid payment intent IDs return `false` or `null`
- Stripe API errors are caught and returned as operation failures
- Webhook signature verification failures return 400 Bad Request
- Payment status validation prevents invalid state transitions

## SignalR Notifications

Payment events trigger real-time notifications:
- **Payment Success**: Customer receives instant confirmation
- **Payment Failed**: Customer notified of failure
- **Refund Processed**: Customer notified when refund is issued

Notifications sent via `NotificationHub` to user-specific groups.

## Dependencies

- **Stripe.net** v50.0.0 - Official Stripe .NET SDK
- **System.Configuration.ConfigurationManager** v8.0.0
- **Microsoft.AspNetCore.SignalR.Core** (for notifications)

## API Endpoints Summary

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/payments/{id}` | GET | User | Get payment by ID |
| `/api/payments/order/{orderId}` | GET | User | Get payment by order |
| `/api/payments/create-payment-intent` | POST | User | Create Stripe payment intent |
| `/api/payments/confirm-payment` | POST | User | Confirm payment |
| `/api/payments/{id}/complete` | POST | Admin, Manager | Mark payment complete (manual) |
| `/api/payments/{id}/refund` | POST | Admin, Manager | Refund payment |
| `/api/payments/webhook` | POST | None (Signature) | Stripe webhook endpoint |

## Production Checklist

- [ ] Replace test API keys with live keys
- [ ] Configure webhook endpoint with HTTPS
- [ ] Update webhook secret for production
- [ ] Test all payment flows end-to-end
- [ ] Set up monitoring for failed payments
- [ ] Configure email notifications for admins
- [ ] Review and test refund process
- [ ] Verify webhook signature validation
- [ ] Set up Stripe Dashboard alerts
- [ ] Document customer support procedures

## Troubleshooting

**Payment Intent Creation Fails**:
- Check Stripe API key is correct
- Verify amount is positive and not too large
- Check order exists and is valid

**Webhook Not Receiving Events**:
- Verify webhook URL is correct and accessible
- Check webhook secret is configured correctly
- Use Stripe CLI to test locally
- Check server logs for webhook errors

**Payment Confirmation Fails**:
- Verify payment intent ID is correct
- Check payment intent is in correct state
- Ensure payment method is attached to intent

## Support

For Stripe-specific issues, refer to:
- Stripe Documentation: https://stripe.com/docs
- Stripe API Reference: https://stripe.com/docs/api
- Stripe .NET SDK: https://github.com/stripe/stripe-dotnet
