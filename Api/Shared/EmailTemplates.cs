using System.Reflection.Metadata.Ecma335;
using Api.Models;

namespace Api.Shared
{


    public static class AccountCreationEmailTemplate
    {

        public static string Subject = "Welcome to Galaxy Bank!";

        public static string Message(string username, string accountNumber)
        {

            var message = $@"
                            <html>
                            <head>
                            <style>
                                body {{
                                font-family: Arial, sans-serif;
                                background-color: #f4f4f4;
                                padding: 20px;
                                color: #333;
                                }}
                                .container {{
                                max-width: 600px;
                                margin: auto;
                                background-color: #ffffff;
                                padding: 30px;
                                border-radius: 10px;
                                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                                }}
                                .header {{
                                text-align: center;
                                color: #4A90E2;
                                }}
                                .content {{
                                margin-top: 20px;
                                line-height: 1.6;
                                }}
                                .footer {{
                                margin-top: 30px;
                                font-size: 0.9em;
                                color: #777;
                                text-align: center;
                                }}
                            </style>
                            </head>
                            <body>
                            <div class='container'>
                                <h2 class='header'>Welcome to Galaxy Bank, {username}!</h2>
                                <div class='content'>
                                <p>Thank you for signing up with Galaxy Bank. We're excited to have you on board!</p>
                                <p>Your new account has been created successfully.</p>
                                <p><strong>Account Number:</strong> {accountNumber}</p>
                                <p>If you have any questions or need help, feel free to contact our support team anytime.</p>
                                </div>
                                <div class='footer'>
                                &copy; {DateTime.Now.Year} Galaxy Bank. All rights reserved.
                                </div>
                            </div>
                            </body>
                            </html>
                            ";


            return message;
        }

    }

    public static class DepositEmailTemplate
    {
        public static string Subject = "Deposit Confirmation";

        public static string Message(string username, string accountNumber, string amount)
        {
            var message = $@"
                            <html>
                            <head>
                            <style>
                                body {{
                                font-family: Arial, sans-serif;
                                background-color: #f4f4f4;
                                padding: 20px;
                                color: #333;
                                }}
                                .container {{
                                max-width: 600px;
                                margin: auto;
                                background-color: #ffffff;
                                padding: 30px;
                                border-radius: 10px;
                                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                                }}
                                .header {{
                                text-align: center;
                                color: #4A90E2;
                                }}
                                .content {{
                                margin-top: 20px;
                                line-height: 1.6;
                                }}
                                .footer {{
                                margin-top: 30px;
                                font-size: 0.9em;
                                color: #777;
                                text-align: center;
                                }}
                            </style>
                            </head>
                            <body>
                            <div class='container'>
                                <h2 class='header'>Deposit Confirmation</h2>
                                <div class='content'>
                                    <p>Dear {username},</p>
                                    <p>We are pleased to inform you that a deposit of <span style={"color:green;"}>Q {amount}</span> has been successfully credited to your account.</p>
                                    <p><strong>Account Number:</strong> *******{accountNumber[^4..]}</p>
                                    <p>If you have any questions or need assistance, please contact our support team.</p>
                                </div>
                                <div class='footer'>
                                &copy; {DateTime.Now.Year} Galaxy Bank. All rights reserved.
                                </div>
                            </div>
                            </body>
                            </html>
                            ";

            return message;
        }
    }

    public static class WithdrawEmailTemplate
    {
        public static string Subject = "Withdrawal Confirmation";

        public static string Message(string username, string accountNumber, string amount)
        {
            var message = $@"
                            <html>
                            <head>
                            <style>
                                body {{
                                font-family: Arial, sans-serif;
                                background-color: #f4f4f4;
                                padding: 20px;
                                color: #333;
                                }}
                                .container {{
                                max-width: 600px;
                                margin: auto;
                                background-color: #ffffff;
                                padding: 30px;
                                border-radius: 10px;
                                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                                }}
                                .header {{
                                text-align: center;
                                color: #4A90E2;
                                }}
                                .content {{
                                margin-top: 20px;
                                line-height: 1.6;
                                }}
                                .footer {{
                                margin-top: 30px;
                                font-size: 0.9em;
                                color: #777;
                                text-align: center;
                                }}
                            </style>
                            </head>
                            <body>
                            <div class='container'>
                                <h2 class='header'>Withdrawal Confirmation</h2>
                                <div class='content'>
                                    <p>Dear {username},</p>
                                    <p>We are writing to confirm that a withdrawal of <span style={"color:red;"}>-Q {amount}</span> has been successfully processed from your account.</p>
                                    <p><strong>Account Number:</strong> *****{accountNumber[^4..]}</p>
                                    <p>If you have any questions or need assistance, please contact our support team.</p>
                                </div>
                                <div class='footer'>
                                &copy; {DateTime.Now.Year} Galaxy Bank. All rights reserved.
                                </div>
                            </div>
                            </body>
                            </html>
                            ";

            return message;
        }
    }

    public static class TransferSenderEmailTemplate
    {
        public static string Subject = "Transfer Confirmation";

        public static string Message(string senderName, string receiverName, string amount, string fromAccountNumber, string toAccountNumber)
        {
            var message = $@"
                            <html>
                            <head>
                            <style>
                                body {{
                                font-family: Arial, sans-serif;
                                background-color: #f4f4f4;
                                padding: 20px;
                                color: #333;
                                }}
                                .container {{
                                max-width: 600px;
                                margin: auto;
                                background-color: #ffffff;
                                padding: 30px;
                                border-radius: 10px;
                                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                                }}
                                .header {{
                                text-align: center;
                                color: #4A90E2;
                                }}
                                .content {{
                                margin-top: 20px;
                                line-height: 1.6;
                                }}
                                .footer {{
                                margin-top: 30px;
                                font-size: 0.9em;
                                color: #777;
                                text-align: center;
                                }}
                            </style>
                            </head>
                            <body>
                            <div class='container'>
                                <h2 class='header'>Transfer Confirmation</h2>
                                <div class='content'>
                                    <p>Dear {senderName},</p>
                                    <p>Your transfer of <span style={"color:red;"}>-Q {amount}</span> to {receiverName} has been successfully processed.</p>
                                    <p><strong>From Account Number:</strong> *****{fromAccountNumber[^4..]}</p>
                                    <p><strong>To Account Number:</strong> *****{toAccountNumber[^4..]}</p>
                                    <p>If you have any questions or need assistance, please contact our support team.</p>
                                    <p style={"color: red;"}> If this was not you, please dispute the transfer via the CLI immediately.</p>
                                </div>
                                <div class='footer'>
                                &copy; {DateTime.Now.Year} Galaxy Bank. All rights reserved.
                                </div>
                            </div>
                            </body>
                            </html>
                            ";

            return message;
        }
    }

    public static class TransferReceiverEmailTemplate
    {
        public static string Subject = "You've Received Money!";

        public static string Message(string receiverName, string senderName, string amount, string fromAccountNumber, string toAccountNumber)
        {
            var message = $@"
                            <html>
                            <head>
                            <style>
                                body {{
                                font-family: Arial, sans-serif;
                                background-color: #f4f4f4;
                                padding: 20px;
                                color: #333;
                                }}
                                .container {{
                                max-width: 600px;
                                margin: auto;
                                background-color: #ffffff;
                                padding: 30px;
                                border-radius: 10px;
                                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                                }}
                                .header {{
                                text-align: center;
                                color: #4A90E2;
                                }}
                                .content {{
                                margin-top: 20px;
                                line-height: 1.6;
                                }}
                                .footer {{
                                margin-top: 30px;
                                font-size: 0.9em;
                                color: #777;
                                text-align: center;
                                }}
                            </style>
                            </head>
                            <body>
                            <div class='container'>
                                <h2 class='header'>You've Received Money!</h2>
                                <div class='content'>
                                    <p>Dear {receiverName},</p>
                                    <p>You have received <span style={"color:green;"}>{amount}</span> from {senderName}.</p>
                                    <p><strong>To Account Number:</strong> ******{toAccountNumber[^4..]}</p>
                                    <p><strong>From Account Number:</strong> ******{fromAccountNumber[^4..]}</p>
                                    <p>If you have any questions or need assistance, please contact our support team.</p>
                                </div>
                                <div class='footer'>
                                &copy; {DateTime.Now.Year} Galaxy Bank. All rights reserved.
                                </div>
                            </div>
                            </body>
                            </html>
                            ";

            return message;
        }
    }
}
