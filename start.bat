SET MSIClientId=00000000-0000-0000-0000-000000000000

IF "%ENVIRONMENT%"=="TODO_YOUR_ENVIRONMENT_NAME" (
  SET MSIClientId=TODO_YOUR_MANAGED_IDENTITY_GUID
  SET ForwardScheme=TODO_SCHEME_HTTP_OR_HTTPS
  SET ForwardHost=TODO_YOUR_INTERNAL_IP:TODO_YOUR_INTERNAL_PORT
  SET RequestForwardTimeOutInSeconds=TODO_YOUR_TIMEOUT_SETTING
)

REM Microsoft Tenant ID == 72f988bf-86f1-41af-91ab-2d7cd011db47
SET AZURE_TENANT_ID=TODO_YOUR_TENANT_ID
SET AZURE_CLIENT_ID=TODO_YOUR_CLIENT_ID
SET ValidAudienceList=https://YOURURL.YOURDOMAIN.COM
SET ValidIssuerList=https://login.microsoftonline.com/%AZURE_TENANT_ID%,https://sts.windows.net/%AZURE_TENANT_ID%/
SET OpenIdAuthority=https://login.microsoftonline.com/%AZURE_TENANT_ID%
SET OpenIdSignOutUrl=/signin-oidc

SET DataProtectionKeyIdentifier=https://TODO_YOUR_DATAPROTECTION_KEYVAULT_ACCOUNT_NAME.vault.azure.net/keys/dataprotection
SET DataProtectionStorageUrl=https://TODO_YOUR_STORAGE_ACCOUNT_NAME_FOR_DATA_PROTECTION_KEYS.blob.core.windows.net/dataprotection/keys.xml

SET SSLCertificateSecretIdentifier=TODO_SECRET_NAME_FOR_SSL_CERT
SET SSLCertificateSecretUrl=https://TODO_YOUR_SSL_CERTIFICATE_KEYVAULT_ACCOUNT_NAME.vault.azure.net

REM Used by Azure LB to probe your individual VM (should not have authentication or ssl)
SET LBPORT=6000

REM Used for https redirection (sort of like HSTS)
SET HTTPS_HOST_NAME=https://YOURURL.YOURDOMAIN.COM
SET ASPNETCORE_URLS=http://+:80;http://+:%LBPORT%;https://+:443

AzureADProxy.exe
