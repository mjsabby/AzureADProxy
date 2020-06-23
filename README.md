# AzureADProxy

Azure AD Proxy with Data Protection APIs

## Code

* ASP.NET Core application that operates like an Azure AD Proxy sending the username of the user or (app) as *X-AAD-Username*.
* If you need to implement an allow list of AppIDs or usernames, that is up to you to implement.

## Prerequisites
* Azure Storage Account for Data Protection
* Azure Key Vault for Protecting Keys
* Azure Key Vault for your SSL Certificate

## Guides

* [Azure Key Vault for Data Protection Keys](https://github.com/Azure/azure-sdk-for-net/blob/cd0ffd399220e33893bc12ead70360e4da93cd62/sdk/extensions/Azure.Extensions.AspNetCore.DataProtection.Keys/README.md)
* [Azure Storage Account for Data Protection Keys](https://github.com/Azure/azure-sdk-for-net/tree/Azure.Extensions.AspNetCore.DataProtection.Keys_1.0.0/sdk/extensions/Azure.Extensions.AspNetCore.DataProtection.Blobs/README.md)
* You need to acquire, upload and auto-rotate your SSL Certicate for the domain
