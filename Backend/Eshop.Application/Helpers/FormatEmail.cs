namespace Eshop.Application.Helpers
{
    public class FormatEmail
    {
        public static string ConfirmEmail(string code, string userName, string time)
        {
            var HTML = $@"
<!DOCTYPE html>
<html lang=""en"">

<head>
     <meta charset=""UTF-8"" />
     <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
     <meta http-equiv=""X-UA-Compatible"" content=""ie=edge"" />
     <title>Static Template</title>

     <link href=""https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600&display=swap"" rel=""stylesheet"" />
</head>

<body style=""
      margin: 0;
      font-family: 'Poppins', sans-serif;
      background: #ffffff;
      font-size: 14px;
    "">
     <div style=""
        max-width: 680px;
        margin: 0 auto;
        padding: 45px 30px 60px;
        background: #f4f7ff;
        background-image: url(https://i.imgur.com/CeakRdI.png);
        background-repeat: no-repeat;
        background-size: 800px 452px;
        background-position: top center;
        font-size: 14px;
        color: #434343;
      "">
          <header>
               <table style=""width: 100%;"">
                    <tbody>
                         <tr style=""height: 0;"">
                              <td>
                                   <img alt="""" src=""https://i.ibb.co/Q9KvSWD/Tendez-Logo.png"" height=""120px"" />
                              </td>
                              <td style=""text-align: right;"">
                                   <span style=""font-size: 25px; line-height: 30px; font-weight: 500; color: #ffffff;"">{time}</span>
                              </td>
                         </tr>
                    </tbody>
               </table>
          </header>

          <main>
               <div style=""
            margin: 0;
            padding: 92px 30px 115px;
            background: #ffffff;
            border-radius: 30px;
            text-align: center;
          "">
                    <div style=""width: 100%; max-width: 489px; margin: 0 auto;"">
                         <h1 style=""
                margin: 0;
                font-size: 25px;
                font-weight: 500;
                color: #1f1f1f;
              "">
                              Your OTP
                         </h1>
                               <div style=""display: flex;align-items: center;justify-content: center;"">

                              <p style=""
                margin: 0;
                margin-top: 17px;
                font-size: 15px;
                font-weight: 500;
                "">
                                   Hey {{{{{userName}}}}}
                              </p>
                              <p style=""
                margin: 0;
                margin-top: 17px;
                font-weight: 500;
                letter-spacing: 0.56px;
                "">
                                   <img src=""https://png.pngtree.com/png-vector/20220621/ourmid/pngtree-isolated-waving-hand-vector-icon-on-white-backgroundeps-10-vector-png-image_46317074.jpg""
                                        style=""width: 50px;"" alt=""hi image"">
                         </div>
                              Thank you for choosing Eshop Website. Use the following OTP
                              to Verify you Account. OTP is
                              valid for
                              <span style=""font-weight: bold; color: red;"">30 minutes</span>.
                              Do not share this code with others.
                         </p>
                         <p style=""
                margin: 0;
                margin-top: 60px;
                font-size: 40px;
                font-weight: 600;
                letter-spacing: 25px;
                color: #9788ff;
              "">
                              {code}
                         </p>
                    </div>
               </div>

               <p style=""
            max-width: 400px;
            margin: 0 auto;
            margin-top: 90px;
            text-align: center;
            font-weight: 500;
            color: #8c8c8c;
          "">
                    Need help? Ask at
                    <a href=""mailto:offical.Tendez.website@hotmail.com""
                         style=""color: #499fb6; text-decoration: none;"">offical.Tendez.website@hotmail.com</a>
                    or visit our
                    <a href="""" target=""_blank"" style=""color: #499fb6; text-decoration: none;"">Help Center</a>
               </p>
          </main>

          <footer style=""
          width: 100%;
          max-width: 490px;
          margin: 20px auto 0;
          text-align: center;
          border-top: 1px solid #e6ebf1;
        "">


            <div style=""margin: 0; margin-top: 16px;"">
    <a  href=""https://wa.me/97336221722"" target=""_blank"" style=""display: inline-block;"">
        <img width=""36px"" alt=""WhatsApp"" src=""https://upload.wikimedia.org/wikipedia/commons/6/6b/WhatsApp.svg"" />
    </a>
    <a href=""https://www.instagram.com/Eshopbh/profilecard/?igsh=MTR2dnF1c2VwdGRxNg=="" target=""_blank"" style=""display: inline-block; margin-left: 8px;"">
        <img width=""36px"" alt=""Instagram"" src=""https://upload.wikimedia.org/wikipedia/commons/a/a5/Instagram_icon.png"" />
    </a>
</div>
               <p style=""margin: 0; margin-top: 16px; color: #434343;"">
                    Copyright © 2024 Tendez. All rights reserved.
               </p>
          </footer>
<br>
<br>
<br>
     </div>
</body>

</html>
";
            return HTML;
        }

        public static string ForgetPassword(string Token, string userName, string time)
        {
            var HTML = $@"
<!DOCTYPE html>
<html lang=""en"">

<head>
     <meta charset=""UTF-8"" />
     <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
     <meta http-equiv=""X-UA-Compatible"" content=""ie=edge"" />
     <title>Reset Password</title>

     <link href=""https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600&display=swap"" rel=""stylesheet"" />
</head>

<body style=""
      margin: 0;
      font-family: 'Poppins', sans-serif;
      background: #ffffff;
      font-size: 14px;
    "">
     <div style=""
        max-width: 680px;
        margin: 0 auto;
        padding: 45px 30px 60px;
        background: #f4f7ff;
        background-image: url(https://i.imgur.com/CeakRdI.png);
        background-repeat: no-repeat;
        background-size: 800px 452px;
        background-position: top center;
        font-size: 14px;
        color: #434343;
      "">
          <header>
               <table style=""width: 100%;"">
                    <tbody>
                         <tr style=""height: 0;"">
                              <td>
                                   <img alt="""" src=""https://i.ibb.co/Q9KvSWD/Tendez-Logo.png"" height=""120px"" />
                              </td>
                              <td style=""text-align: right;"">
                                   <span style=""font-size: 25px; line-height: 30px; font-weight: 500; color: #ffffff;"">{time}</span>
                              </td>
                         </tr>
                    </tbody>
               </table>
          </header>

          <main>
               <div style=""
            margin: 0;
            padding: 92px 30px 115px;
            background: #ffffff;
            border-radius: 30px;
            text-align: center;
          "">
                    <div style=""width: 100%; max-width: 489px; margin: 0 auto;"">
                         <h1 style=""
                margin: 0;
                font-size: 25px;
                font-weight: 500;
                color: #1f1f1f;
              "">
                              Reset Your Password
                         </h1>
                         <p style=""
                margin: 0;
                margin-top: 17px;
                font-size: 15px;
                font-weight: 500;
                "">
                                   Hello {userName},
                              </p>
                              <p style=""
                margin: 0;
                margin-top: 17px;
                font-weight: 500;
                letter-spacing: 0.56px;
                "">
                                   Thank you for choosing Eshop Website. Please click the button below to reset your password. This link is valid for <span style=""font-weight: bold; color: red;"">30 days</span>. Do not share this link with others.
                              </p>
                              <a target=""_blank"" href=""http://localhost:4200/create-new-password?token={Token}"" style=""
                              display: inline-block;
                              background-color: #9788ff;
                              color: white;
                              padding: 14px 25px;
                              text-align: center;
                              text-decoration: none;
                              font-size: 16px;
                              margin-top: 20px;
                              border-radius: 8px;
                            "">
                              Reset Password
                         </a>
                    </div>
               </div>

               <p style=""
            max-width: 400px;
            margin: 0 auto;
            margin-top: 90px;
            text-align: center;
            font-weight: 500;
            color: #8c8c8c;
          "">
                    Need help? Ask at
                    <a href=""mailto:offical.Tendez.website@hotmail.com""
                         style=""color: #499fb6; text-decoration: none;"">offical.Tendez.website@hotmail.com</a>
                    or visit our
                    <a href="""" target=""_blank"" style=""color: #499fb6; text-decoration: none;"">Help Center</a>
               </p>
          </main>

          <footer style=""
          width: 100%;
          max-width: 490px;
          margin: 20px auto 0;
          text-align: center;
          border-top: 1px solid #e6ebf1;
        "">
<div style=""margin: 0; margin-top: 16px;"">
    <a  href=""https://wa.me/97336221722"" target=""_blank"" style=""display: inline-block;"">
        <img width=""36px"" alt=""WhatsApp"" src=""https://upload.wikimedia.org/wikipedia/commons/6/6b/WhatsApp.svg"" />
    </a>
    <a href=""https://www.instagram.com/Eshopbh/profilecard/?igsh=MTR2dnF1c2VwdGRxNg=="" target=""_blank"" style=""display: inline-block; margin-left: 8px;"">
        <img width=""36px"" alt=""Instagram"" src=""https://upload.wikimedia.org/wikipedia/commons/a/a5/Instagram_icon.png"" />
    </a>
</div>

               <p style=""margin: 0; margin-top: 16px; color: #434343;"">
                    Copyright © 2024 Tendez. All rights reserved.
               </p>
          </footer>
     </div>
</body>

</html>
";
            return HTML;
        }

    }
}
