# Integration sample 

This is a .NET integration sample for the <a href="https://bulksign.com">Bulksign platform</a> . It allows the integrator to :

- receive all the callbacks triggered from Bulksign

- maintain, in a local SQL Server database, a list of all bundles send for signing

- download the completed documents and stored them locally


# How to use  this  ?

- clone this repository

- create a empty SQL Server database  

- run the \content\db-query.sql  query to create the DB schema.

- extend the code to handle the callbacks if needed (now the information is logged)

- build and deploy the integration code

- update your integration code to point to this endpoint when sending bundles for signing

IF you are using the Bulksign SDK, this is done by specifying the endpoint in the BulksignApi ctor

```
  BulksignApi api = new BulksignApi ("http://myendpoint/bulksignintegration/restapi")
```

*assuming you have deployed this integration to "http://myendpoint/bulksignintegration"

