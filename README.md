# ColeSoft.Extensions.Logging.Splunk

[![Build status](https://ci.appveyor.com/api/projects/status/e4s6reeh69qf580p/branch/master?svg=true)](https://ci.appveyor.com/project/iMobile3/loggr-extensions-logging/branch/master) [![NuGet Version](http://img.shields.io/nuget/v/Loggr.Extensions.Logging.svg?style=flat)](https://www.nuget.org/packages/Loggr.Extensions.Logging/)

Log to [Splunk][0] directly from [Microsoft.Extensions.Logging][1] using the [Splunk HTTP Event Collector (HEC)][5].

## Installation

ColeSoft.Extensions.Logging.Splunk installs through [NuGet][3] and requires [.NET Core][4] >= 2.0.

```
PS> Install-Package ColeSoft.Extensions.Logging.Splunk
```

Configure the Splunk provider through code:

```c#
WebHost.CreateDefaultBuilder(args)
    .ConfigureLogging((hostingContext, logging) => {
            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            // Add other providers
            logging.AddSplunk();
        })
    }
```
Or:
```c#
IServiceCollection services;  // from somewhere

services.AddLogging(builder => builder.AddSplunk());
```
As a minimum the following configuration must be supplied to that the provider know where 
to send the data and with what credential:
```json
"Logging": {
    "Splunk": {
        "SplunkCollectorUrl": "https://splunk-server-name:8088/services/collector/",
        "AuthenticationToken": "92C168CF-C097-45F3-A3A8-128C3C509E9F"
    }
},
```

In the above examples we configure the splunk event collector with the libraries default 
settings.  Settings can also be described in code as well as configuration by setting the 
properties on an instance of a SplunkLoggerOptions object in a delegate supplied to the 
AddSplunk call.

## Usage

Log messages to Splunk, just as with every other provider:

```c#
logger.LogInformation("This is information");
```

## Advanced Topics

### Splunk Endpoint (Json vs Raw)
TODO

### Available Configuration Options

Other configuration options are availble to be set on the SplunkLoggerOptions, either in the delegate supplied
to the AddSplunk call or via the application settings json file.

| Option                    	| Description                                                                                                                                                                                                                                                                                 	| Default Value 	|
|---------------------------	|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------	|---------------	|
| IncludeScopes             	| Whether to include scope information for log events.                                                                                                                                                                                                                                        	| `false`         	|
| SplunkCollectorUrl        	| The Url for the splunk collector                                                                                                                                                                                                                                                            	|               	|
| AuthenticationToken       	| The HEC authentication token.                                                                                                                                                                                                                                                               	|               	|
| UseAuthTokenAsQueryString 	| Whether to use or not hec token authentication at query string                                                                                                                                                                                                                              	| `false`         	|
| Timeout                   	| The timeout (in milliseconds) used for http HEC requests when sending data to the Splunk instance.                                                                                                                                                                                          	| `1500`          	|
| CustomHeaders             	| Any custom header to be applied at HEC calls.                                                                                                                                                                                                                                               	|               	|
| ChannelIdType             	| Where, if at all, to include the channel identifier in the query to the endpoint.                                                                                                                                                                                                           	| `ChannelIdOption.None`         	|
| Host                      	| The host value to assign to the event data. This is typically the hostname of the client from which you're sending data.                                                                                                                                                                    	|               	|
| Index                     	| The name of the index by which the event data is to be indexed.  The index you specify here must within the list of allowed indexes if the token has the indexes parameter set.                                                                                                             	|               	|
| Source                    	| The source value to assign to the event data.  For example, if you're sending data from an app you're developing, you could set this key to the name of the app.                                                                                                                            	|               	|
| SourceType                	| The sourcetype value to assign to the event data.                                                                                                                                                                                                                                           	|               	|
| TimestampFormat           	| The format string used to format the timestamp within the LogData.  The default value of null will result in the number of seconds to 3 decimal places since the unix epoch time being used.  Any other format string here will be passed to the `System.DateTime.ToString(string)` method. 	| `null`          	|
| BatchInterval             	| The frequency, in  milliseconds, with which to try and send events to the HEC endpoint.  A value of 0 will only result in sends when greater than <see cref="BatchSize"/> events have been collected.                                                                                       	| `1000`          	|
| BatchSize                 	| Once BatchSize items are collected they will be sent regardless of time till the next BatchInterval.  Also, The maximum number of items to send in a single batch.                                                                                                                          	| `50`            	|

### Payload Customisation
TODO

[0]: https://www.splunk.com/
[1]: https://github.com/aspnet/Logging
[2]: https://github.com/loggr/loggr-dotnet
[3]: https://www.nuget.org/packages/Loggr.Extensions.Logging
[4]: https://github.com/dotnet/core
[5]: https://docs.splunk.com/Documentation/Splunk/latest/Data/UsetheHTTPEventCollector