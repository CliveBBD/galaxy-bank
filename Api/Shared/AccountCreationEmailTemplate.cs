using System.Reflection.Metadata.Ecma335;

namespace Api.Shared
{


    public static class AccountCreationEmailTemplate
    {

        public static string Subject = "Welcome to Galaxy Bank!";

        public static string Message(string username, int accountId)
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
                                <p><strong>Account Number:</strong> {accountId}</p>
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
}
