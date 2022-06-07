# OzCodeReview

## Features
  - Review the code directly in VS 2022.
  - Multiusers
  - You can a review with differents types (Must correct, should correct, comment)
  - You can choose whom belongs the review
  - You can edit a review, change its status or close it
  - A notification is send by email when a review is created or updated
  - The list of project reviews is visible in the view / view code reviews panel

## Backend setup
This application needs you to install backend accessible here and configure it via appsettings :

  - Authentication:AuthenticationTokenSecretSigningKey : a random string to secure yours tokens
  - ConnectionStrings:DefaultConnection : a valid Sql Database connection string where data will be stored
  - Data:Emails:SmtpHost : STMP server
  - Data:Emails:SmtpPort : SMTP port
  - Data:Emails:SmtpUser : SMTP user
  - Data:Emails:SmtpPassword : SMTP user password
  - Data:Network:BaseUrl : the backend url. Must ends with a /

Log to backend as admin (default login/password are admin@ozcodereview.fr / ozCodeReview@* ) and create users

## VS Setup
Please go to tools / options / ozcodereview and fill the serverurl, your login /password (defined in previous step)
