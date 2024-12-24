# DotNetChatApp

.NET Chat Application
https://github.com/gasalaza/DotNetChatApp

Table of Contents
Overview
Features
Architecture
Technologies Used
Prerequisites
Installation
Configuration
Running the Application
1. Start RabbitMQ
2. Run the API
3. Run the ChatBot
4. Run the ChatClient
Testing
Usage
Troubleshooting
Contributing
License
Acknowledgements
Overview
The .NET Chat Application is a real-time chat system built with .NET 8, leveraging SignalR for real-time communication, Entity Framework Core for data persistence, and RabbitMQ for handling asynchronous message commands via a bot. The application supports user authentication with JWT, multiple chat rooms, message ordering, and limiting to ensure efficient and organized conversations. Additionally, a simple console-based client allows users to interact with the chat system, and a bot console application processes specific commands like stock queries.

Features
User Authentication: Secure registration and login using JWT tokens.
Real-Time Communication: Instant messaging within chat rooms using SignalR.
Multiple Chat Rooms: Users can join and leave different chat rooms.
Message Commands: Detect and process commands (e.g., /stock=APPL) within messages.
Bot Integration: A bot listens for commands via RabbitMQ and responds appropriately.
Message Ordering & Limiting: Ensures messages are ordered by timestamp and only the latest 50 messages are retained in each chat room.
Console Client: A simple console application to simulate multiple users for testing purposes.




ChatApp.Api: The main Web API that handles user authentication, SignalR hubs, and API endpoints.
ChatApp.Infrastructure: Handles data access using Entity Framework Core, including database migrations and models.
ChatApp.Core: Contains core business logic and shared models.
ChatBot: A console application that acts as a bot, listening to RabbitMQ and interacting with the chat via SignalR.
ChatClient: A console-based SignalR client for testing and simulating multiple users.
ChatApp.Tests: Contains unit tests to ensure the reliability of critical components.
Technologies Used
.NET 8: Framework for building the application.
ASP.NET Core Web API: Backend API development.
SignalR: Real-time communication between server and clients.
Entity Framework Core 8: ORM for data access.
RabbitMQ: Message broker for handling asynchronous tasks.
xUnit & Moq: Testing frameworks for unit tests.
JWT (JSON Web Tokens): For secure user authentication.
FluentValidation: For robust model validation.
AutoMapper: For object-to-object mapping.
Prerequisites
Before setting up the project, ensure you have the following installed on your machine:

.NET 8 SDK: Download .NET 8
SQL Server Express: Download SQL Server Express
RabbitMQ: Download RabbitMQ
Visual Studio 2022 or Visual Studio Code: Download Visual Studio
Git: Download Git
Installation
Clone the Repository:

bash
Copy code
git clone https://github.com/gasalaza/DotNetChatApp
cd your-repo/DotNetChatApp
Restore Dependencies:

Restore NuGet packages for all projects.

bash
Copy code
dotnet restore
Build the Solution:

bash
Copy code
dotnet build
Configuration
1. SQL Server Setup
Ensure that SQL Server Express is installed and running on your machine. By default, the connection string is set to use localhost\SQLEXPRESS. Modify it if your SQL Server instance has a different name.

2. RabbitMQ Setup
Install RabbitMQ:

Download and install RabbitMQ from the official website.

Start RabbitMQ Server:

Ensure that the RabbitMQ service is running. You can access the management console at http://localhost:15672/ using the default credentials (guest / guest).

3. JWT Configuration
Set JWT Secrets:

For development, the JWT secrets are stored in appsettings.json or using User Secrets. Ensure that the Secret, ValidIssuer, and ValidAudience fields are properly set.

Example appsettings.json:

json
Copy code
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=ChatAppDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JWT": {
    "ValidIssuer": "ChatApp",
    "ValidAudience": "ChatAppUser",
    "Secret": "YourSuperSecretKey12345" // Replace with a strong secret key
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
Running the Application
To run the application, you need to start several components in the correct order.

1. Start RabbitMQ
Ensure that RabbitMQ is running on your machine. You can verify this by accessing the management console at http://localhost:15672/.

2. Run the API (ChatApp.Api)
Navigate to the API Project Directory:

bash
Copy code
cd C:\Users\gasalaza\Desktop\jobsifyproj\DotNetChatApp\ChatApp.Api
Apply Migrations and Update the Database:

bash
Copy code
dotnet ef database update --project ChatApp.Infrastructure/ChatApp.Infrastructure.csproj --startup-project ChatApp.Api/ChatApp.Api.csproj
Start the API:

bash
Copy code
dotnet run
The API will typically run on https://localhost:5001 and http://localhost:5000.

3. Run the Bot (ChatBot)
Navigate to the Bot Project Directory:

bash
Copy code
cd C:\Users\gasalaza\Desktop\jobsifyproj\DotNetChatApp\ChatBot
Configure appsettings.json:

Ensure that the appsettings.json contains the correct SignalR Hub URL and a valid JWT token.

json
Copy code
{
  "RabbitMQ": {
    "HostName": "localhost",
    "QueueName": "stock_commands"
  },
  "SignalR": {
    "HubUrl": "https://localhost:5001/chathub",
    "AccessToken": "YOUR_JWT_TOKEN_HERE" // Replace with a valid token
  }
}
Start the Bot:

bash
Copy code
dotnet run
The bot will listen to the stock_commands queue and respond to stock commands.

4. Run the ChatClient (ChatClient)
Navigate to the ChatClient Project Directory:

bash
Copy code
cd C:\Users\gasalaza\Desktop\jobsifyproj\DotNetChatApp\ChatClient
Configure appsettings.json:

Ensure that the appsettings.json contains the correct SignalR Hub URL and a valid JWT token for the user.

json
Copy code
{
  "SignalR": {
    "HubUrl": "https://localhost:5001/chathub",
    "AccessToken": "YOUR_JWT_TOKEN_HERE" // Replace with a valid token
  }
}
Note: For multiple clients, use different tokens corresponding to different registered users.

Start the ChatClient:

bash
Copy code
dotnet run
Interaction Flow:
Join a Chat Room: Enter the name of the chat room you wish to join.
Send Messages: Type messages and press Enter to send.
Send Commands: Type commands like /stock=APPL to interact with the bot.
Exit: Type /exit to leave the chat room and disconnect.
Navigate to the Test Project Directory:



Testing the Chat Functionality
Register and Login Users:

Use Swagger UI or tools like Postman to register multiple users via the AuthController.
Login each user to obtain their respective JWT tokens.
Run Multiple ChatClient Instances:

Open multiple terminal windows.
Navigate to each ChatClient project and configure appsettings.json with different JWT tokens.
Start each client to simulate multiple users.
Interact in Chat Rooms:

Join the same chat room from different clients.
Send messages and verify real-time updates across all clients.
Test command functionalities like /stock=APPL and observe bot responses.
Usage
Registering a New User
Access Swagger UI:

Navigate to https://localhost:5001/swagger in your browser.

Use the POST /api/Auth/Register Endpoint:

Click on the POST /api/Auth/Register endpoint.

Click Try it out.

Provide the necessary user details:

json
Copy code
{
  "username": "testuser",
  "email": "testuser@example.com",
  "password": "P@ssw0rd!"
}
Click Execute to register the user.

Logging In
Use the POST /api/Auth/Login Endpoint:
Click on the POST /api/Auth/Login endpoint.

Click Try it out.

Provide the login credentials:

json
Copy code
{
  "username": "testuser",
  "password": "P@ssw0rd!"
}
Click Execute to obtain the JWT token.

Using the ChatClient
Configure appsettings.json:

Insert the obtained JWT token into the AccessToken field.

Run the ChatClient:

Start the application.
Enter the chat room name (e.g., General).
Begin sending messages or commands.
Troubleshooting
Common Issues and Solutions
appsettings.json Not Found:

Error:
arduino
Copy code
System.IO.FileNotFoundException: 'The configuration file 'appsettings.json' was not found and is not optional...'
Solution:
Ensure that appsettings.json exists in the project root.
Verify that it's set to copy to the output directory by checking the .csproj file.
Example in ChatClient.csproj:
xml
Copy code
<ItemGroup>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </None>
</ItemGroup>
JWT Authentication Issues:

Problem: Clients unable to authenticate with the Hub.
Solution:
Ensure that the JWT token is correctly generated and valid.
Verify that the token is correctly inserted into the appsettings.json of the client.
Check that the JWT settings in appsettings.json (issuer, audience, secret) match between the API and clients.
RabbitMQ Connection Errors:

Problem: Bot unable to connect to RabbitMQ.
Solution:
Ensure RabbitMQ is installed and the service is running.
Verify the connection settings in ChatBot/appsettings.json.
Check firewall settings that might block RabbitMQ ports.
SignalR Connection Failures:

Problem: Clients unable to connect to the SignalR Hub.
Solution:
Confirm that the API (ChatApp.Api) is running and accessible.
Verify the Hub URL in the client's appsettings.json.
Check SSL certificate configurations if using HTTPS.
Bot Not Responding to Commands:

Problem: Commands sent from clients are not processed by the bot.
Solution:
Ensure that the bot is running and connected to both RabbitMQ and the SignalR Hub.
Check RabbitMQ queues for incoming messages.
Review logs in both the bot and API for any errors.
Additional Tips
Enable Detailed Logging:

Configure logging in both the API and bot to capture detailed error information.

Use Swagger for API Testing:

Utilize Swagger UI to manually test API endpoints and verify responses.

Check Network Configurations:

Ensure that all services are accessible over the network, especially if running on different machines or containers.



License
This project is licensed under the MIT License. See the LICENSE file for details.

Acknowledgements
Microsoft Docs
SignalR
Entity Framework Core
RabbitMQ
xUnit
Moq
Contact
For any questions or feedback, please contact:

Name: Gabriel Salazar
Email: gabrielsalazar3092@gmail.com
LinkedIn: https://www.linkedin.com/in/gasalazacr/
GitHub: https://github.com/gasalaza# DotNetChatApp
