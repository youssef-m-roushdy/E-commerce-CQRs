# SignalR Real-Time Notifications

## Overview
The E-Commerce API uses SignalR for real-time notifications. Clients can connect to the hub to receive instant updates about orders, payments, stock alerts, and more.

## Hub Endpoint
**URL:** `http://localhost:5272/hubs/notifications`

## Authentication
The SignalR hub requires JWT authentication. Include your Bearer token when establishing the connection.

## Notification Types

### 1. **ReceiveNotification** (General)
```json
{
  "Type": "Info",
  "Message": "Your order has been created successfully",
  "Data": {
    "OrderId": "guid",
    "TotalAmount": 99.99
  },
  "Timestamp": "2025-12-13T10:30:00Z"
}
```

### 2. **OrderStatusUpdate**
```json
{
  "Type": "OrderUpdate",
  "Message": "Your order status has been updated to: Shipped",
  "Data": {
    "OrderId": "guid",
    "Status": "Shipped"
  },
  "Timestamp": "2025-12-13T10:30:00Z"
}
```

### 3. **PaymentNotification**
```json
{
  "Type": "Payment",
  "Message": "Payment of $99.99 completed successfully",
  "Data": {
    "PaymentId": "guid",
    "Status": "Completed",
    "Amount": 99.99
  },
  "Timestamp": "2025-12-13T10:30:00Z"
}
```

### 4. **StockAlert** (Admin/Manager only)
```json
{
  "Type": "StockAlert",
  "Message": "Low stock alert: Product Name (Stock: 5)",
  "Data": {
    "ProductId": "guid",
    "ProductName": "Product Name",
    "CurrentStock": 5
  },
  "Timestamp": "2025-12-13T10:30:00Z"
}
```

### 5. **NewOrder** (Admin/Manager/Support only)
```json
{
  "Type": "NewOrder",
  "Message": "New order from John Doe - Total: $199.99",
  "Data": {
    "OrderId": "guid",
    "CustomerName": "John Doe",
    "TotalAmount": 199.99
  },
  "Timestamp": "2025-12-13T10:30:00Z"
}
```

## Client Implementation Examples

### JavaScript/TypeScript (SignalR Client)

#### Installation
```bash
npm install @microsoft/signalr
```

#### Connection Code
```typescript
import * as signalR from "@microsoft/signalr";

// Get JWT token from your auth system
const token = "your-jwt-token-here";

// Create connection
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5272/hubs/notifications", {
    accessTokenFactory: () => token
  })
  .withAutomaticReconnect()
  .configureLogging(signalR.LogLevel.Information)
  .build();

// Handle connection events
connection.onreconnecting((error) => {
  console.log("Reconnecting...", error);
});

connection.onreconnected((connectionId) => {
  console.log("Reconnected:", connectionId);
});

connection.onclose((error) => {
  console.log("Connection closed:", error);
});

// Subscribe to notifications
connection.on("ReceiveNotification", (notification) => {
  console.log("General Notification:", notification);
  // Update UI
});

connection.on("OrderStatusUpdate", (notification) => {
  console.log("Order Status Update:", notification);
  // Update order status in UI
});

connection.on("PaymentNotification", (notification) => {
  console.log("Payment Notification:", notification);
  // Show payment success/failure
});

connection.on("StockAlert", (notification) => {
  console.log("Stock Alert:", notification);
  // Show stock alert to admin/manager
});

connection.on("NewOrder", (notification) => {
  console.log("New Order:", notification);
  // Notify staff of new order
});

// Start connection
async function start() {
  try {
    await connection.start();
    console.log("SignalR Connected");
  } catch (err) {
    console.error("Error connecting:", err);
    // Retry after 5 seconds
    setTimeout(start, 5000);
  }
}

start();

// Optional: Join specific groups
async function joinGroup(groupName: string) {
  try {
    await connection.invoke("JoinGroup", groupName);
    console.log(`Joined group: ${groupName}`);
  } catch (err) {
    console.error("Error joining group:", err);
  }
}

// Optional: Leave specific groups
async function leaveGroup(groupName: string) {
  try {
    await connection.invoke("LeaveGroup", groupName);
    console.log(`Left group: ${groupName}`);
  } catch (err) {
    console.error("Error leaving group:", err);
  }
}
```

### React Hook Example
```typescript
import { useEffect, useState } from 'react';
import * as signalR from "@microsoft/signalr";

export function useSignalR(token: string) {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [notifications, setNotifications] = useState<any[]>([]);

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5272/hubs/notifications", {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);
  }, [token]);

  useEffect(() => {
    if (connection) {
      connection
        .start()
        .then(() => {
          console.log("SignalR Connected");

          // Subscribe to all notification types
          connection.on("ReceiveNotification", (notification) => {
            setNotifications(prev => [...prev, notification]);
          });

          connection.on("OrderStatusUpdate", (notification) => {
            setNotifications(prev => [...prev, notification]);
          });

          connection.on("PaymentNotification", (notification) => {
            setNotifications(prev => [...prev, notification]);
          });

          connection.on("StockAlert", (notification) => {
            setNotifications(prev => [...prev, notification]);
          });

          connection.on("NewOrder", (notification) => {
            setNotifications(prev => [...prev, notification]);
          });
        })
        .catch((err) => console.error("Connection error:", err));
    }

    return () => {
      connection?.stop();
    };
  }, [connection]);

  return { connection, notifications };
}
```

### C# Client Example
```csharp
using Microsoft.AspNetCore.SignalR.Client;

var token = "your-jwt-token-here";

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5272/hubs/notifications", options =>
    {
        options.AccessTokenProvider = () => Task.FromResult(token);
    })
    .WithAutomaticReconnect()
    .Build();

// Subscribe to notifications
connection.On<object>("ReceiveNotification", notification =>
{
    Console.WriteLine($"Notification: {notification}");
});

connection.On<object>("OrderStatusUpdate", notification =>
{
    Console.WriteLine($"Order Status Update: {notification}");
});

connection.On<object>("PaymentNotification", notification =>
{
    Console.WriteLine($"Payment Notification: {notification}");
});

connection.On<object>("StockAlert", notification =>
{
    Console.WriteLine($"Stock Alert: {notification}");
});

connection.On<object>("NewOrder", notification =>
{
    Console.WriteLine($"New Order: {notification}");
});

// Start connection
await connection.StartAsync();
Console.WriteLine("SignalR Connected");

// Join group (optional)
await connection.InvokeAsync("JoinGroup", "custom-group");

// Keep connection alive
Console.ReadLine();

// Clean up
await connection.StopAsync();
```

## Group-Based Notifications

The hub automatically adds users to groups based on:
- **User ID**: `user_{userId}` - For user-specific notifications
- **Role**: `role_{roleName}` - For role-based notifications (Admin, Manager, Support, Customer)

### Custom Groups
You can join/leave custom groups using hub methods:
```typescript
// Join group
await connection.invoke("JoinGroup", "custom-group-name");

// Leave group
await connection.invoke("LeaveGroup", "custom-group-name");
```

## Automatic Notifications

The API automatically sends real-time notifications for:

1. **Order Created** → Customer receives confirmation, Staff receives new order alert
2. **Order Status Updated** → Customer receives status change notification
3. **Payment Completed** → Customer receives payment confirmation
4. **Payment Refunded** → Customer receives refund notification
5. **Low Stock** → Admin/Manager receives stock alert when product stock ≤ 10

## CORS Configuration

The API is configured to accept SignalR connections from:
- `http://localhost:3000` (React default)
- `http://localhost:4200` (Angular default)
- `http://localhost:5173` (Vite default)

To add more origins, update the CORS policy in `Program.cs`.

## Testing with Postman or Similar Tools

SignalR connections can be tested using browser console or dedicated SignalR testing tools. For basic testing:

1. Get a valid JWT token by logging in via `/api/auth/login`
2. Use the token to establish a SignalR connection
3. Trigger events (create order, update order status, etc.) to see real-time notifications

## Troubleshooting

### Connection Fails
- Ensure JWT token is valid and not expired
- Check CORS settings if connecting from browser
- Verify the hub endpoint URL is correct

### Not Receiving Notifications
- Check if user is authenticated properly
- Verify user roles for role-specific notifications
- Check browser console for SignalR errors

### Reconnection Issues
- Use `.withAutomaticReconnect()` for automatic reconnection
- Implement retry logic with exponential backoff
- Handle `onreconnecting` and `onreconnected` events
