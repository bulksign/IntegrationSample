## What is this ? 
<br/>
This is a .NET integration sample code for the <a href="https://bulksign.com">Bulksign platform</a>. It allows the integrator to :
<br/>
<br/>

- receive all the callbacks triggered from Bulksign platform

- handle envelope status updates with periodic configurable polling  (just in case your callback handler goes down)

- maintain, in a local SQL Server database, a list of all envelopes send for signing

- optionally store the configuration of each sent envelope (could be used for troubleshooting)

- downloads the completed documents and stores them locally

- works with any version of Bulksign (SAAS and OnPremise)

- it's shipped as source code so any integrator can change it and adapt it to their needs.

<br/>
<br/>

## How to deploy and use ?
<br/>

- clone this repository and build the solution

- create a empty SQL Server database  

- run the \dbschema\db-query.sql  query to create the DB schema.

- extend the code to handle callbacks \ envelope completed action.

- configure the integration : 

```

<add key="IntervalCompletedDocumentsInMinutes" value="3" />

<add key="IntervalBundleStatusInMinutes" value="24" />
		
<add key="DatabaseConnectionString" value="" />
		
<add key="CompletedBundlePath" value="c:\CompleteDocuments\" />
		
<add key="BulksignRootApiUrl" value="http://bulksign.com/webapi/" /> 
		
<add key="StoreBundleConfiguration" value="false" />

```


- deploy the integration code : 

a) the CallbackReceiver website should be deployed in IIS 
b) WindowService project should be deployed as a Windows service 

```
cd c:\Windows\Microsoft.NET\Framework64\v4.0.30319\

installutil -i c:\build\BulksignIntegration.Service.exe

```

- your integration code to point to this endpoint when sending envelopes for signing

```
  BulksignApi api = new BulksignApi ("http://myendpoint/bulksignintegration/restapi")
```


