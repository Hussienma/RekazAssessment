# Simple Drive API - .NET

## Introduction
This software is an api that uploads files, sent as base64 blobs, into multiple storage options: S3, Local Storage, and Database. The storage option should be configured by changing `appsettings.json` "StorageSettings:StorageOption" to one of three values: "S3", "LOCAL", "DATABASE".

The API contains three routes:
- **POST** _/v1/auth?name=**name**_: Generates JWT Bearer token. This token should be included in the 'Authorization' header with each request to upload to retrieve a file.
- **POST** _/v1/blobs/: This request uploads a file to the configured storage option. The body must contain two fields:
  - id: this can be a file name, file path, or any other valid string. Trying to upload with an id that already exists returns a `Bad Request`.
  - data: this is the file content encoded as base64 blob. If the data cannot be decoded the service returns `Bad Request`.
- **GET** _/v1/blobs/<id>_: Returns the file using its id. The response contains some metadata as well. If the file is not found the service returns `Not Found`.

## Configuration
As mentioned in the previous section, the specific storage service is configured at **BUILD TIME**. If the **DATABASE** option is selected, the connection string for FileDatabase should be provided. All `StorageSettings` options for a specific service are required when that service is chosen.

## Application Bootstraping
The steps for running the application are:
### Applying database migrations:
  The databases are configured using Microsoft Entity Framework. There are two database contexts: 
  1. **AppDbContext**: contains the database for file metadata. It is required regardless of storage service.
  2. **FileDbContext**: contains the database used for storing file data, if the option is enabled.

  The command for creating the database:
  ```
  dotnet ef database update --context *Context*
  ```

### Running the application
The application is build and run by using:
```
dotnet run
```
By defualt it runs in **Development** Enviornment. 

## S3 HTTP Protocol
The file uploading to S3 buckets are done through HTTP requests, The files are uploaded in a single chunk for implementation simplicity's sake. The service has been thoroughly using MinIO installed on local machine, however the requests meets Amazon's S3 specifications and should work fine with any S3 compatible service.

## Tests
Both unit tests and integration tests are provided with the project. The tests are written using XUnit and AspNetCore.Testing.
