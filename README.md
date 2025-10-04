# 🍴 FoodMela
*A food ordering and delivery platform with real-time vendor ↔ customer notifications (ASP.NET Core MVC + SignalR)*  

---

## 🚀 Features  
- 👨‍🍳 **Vendor Dashboard** – Manage products, accept and deliver orders, view analytics.  
- 🛒 **Customer Panel** – Browse products, add to cart, place orders, view order status.  
- 📊 **Admin Panel** – Manage vendors, customers, and products.  
- 🔔 **Real-Time Notifications** – Vendors & Customers receive instant updates (SignalR).  
- 📦 **Order Tracking** – Customers can track order status (Placed → Accepted → Delivered).  
- 🔑 **Authentication & Roles** – Admin, Vendor, Customer with ASP.NET Core Identity.  

---

## 🛠️ Tech Stack  
- **Backend:** ASP.NET Core 7 MVC  
- **Frontend:** Razor Views + Bootstrap 5 + jQuery  
- **Database:** SQL Server (Entity Framework Core)  
- **Real-Time:** SignalR  
- **Authentication:** ASP.NET Core Identity  

---

## 📂 Project Structure  
```
FoodMela/
 ├── Controllers/       # MVC Controllers (Admin, Vendor, Customer, Notifications)
 ├── Models/            # Entity Framework Models (Order, Product, Notification, etc.)
 ├── Views/             # Razor Pages for UI
 ├── Data/              # ApplicationDbContext & Seeders
 ├── Hubs/              # SignalR Hub for notifications
 ├── wwwroot/           # Static files (CSS, JS, images)
 └── Program.cs         # Application entry point
```

---

## ⚡ Getting Started  

### 1️⃣ Clone the repository  
```bash
git clone https://github.com/YOUR-USERNAME/FoodMela.git
cd FoodMela
```

### 2️⃣ Setup Database  
Update your **`appsettings.json`** connection string:  
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=FoodMelaDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Run migrations:  
```bash
dotnet ef database update
```

### 3️⃣ Run the project  
```bash
dotnet run
```
Open in browser: **https://localhost:5001**

---

## 🔔 Notifications Example  
- Vendor accepts an order → Customer receives a real-time notification.  
- Vendor delivers order → Customer gets notified instantly.  
- Notifications are also stored in DB → visible in Notification Center (Bell icon 🔔).  

---

## Test It 
http://foodmela.runasp.net/

---

## 👨‍💻 Author  
- **Syed Ali Jawad** – https://github.com/AliJawad1214  
