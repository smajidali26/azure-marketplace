# Azure Marketplace Integration - Views

This is where the application views live. This is where you'll find the main landing page template, as well as pages for success and error states. Any UI updates or changes to copy will be updated here.

# Views Directory Structure
```
📂Views
 ┣ 📂LandingPage
 ┃ ┣ 📜 Error.cshtml
 ┃ ┣ 📜 Index.cshtml
 ┃ ┣ 📜 NoTokenError.cshtml
 ┃ ┗ 📜 Success.cshtml
 ┣ 📂Shared
 ┃ ┣ 📜 _Layout.cshtml
 ┃ ┣ 📜 _ValidationScriptsPartial.cshtml
 ┃ ┗ 📜 Error.cshtml
 ┣ 📜 _ViewImports.cshtml
 ┣ 📜 _ViewStart.cshtml
 ┗ 📜 README.md
```

# Sub-Folders

## LandingPage

`Error.cshtml` - The page that displays if any errors occur during application runtime.

`Index.cshtml` - The main landing page. This is where users will review their information, as well as confirming their subscription.

`NoTokenError.cshtml` - The page that displays if no token has been included with the request parameters.

`Success.cshtml` - The page that displays upon successful confirmation of the subsciption update.

## Shared

`_Layout.cshtml` - A wrapper around application views. This includes meta tags, as well as linking to any required stylesheets or scripts. Any styling applied to this file will be applied to every view in the app.

`_ValidationScriptsPartial.cshtml` - Adds in jQuery validation scripts for validating user input data.

`Error.cshtml` - A generic error page layout.

# Styling

Any updates to CSS/JS/assets should be added to the `LandingPage/wwwroot` folder inside their respective folder. A `wwwroot/css/variables.css` file has been added in order to quickly update layout styles (such as adding primary brand colors and assets).