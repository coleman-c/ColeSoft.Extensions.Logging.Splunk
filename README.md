# <img src="NugetIcon.jpg" alt="drawing" width="30"/> ColeSoft.Extensions.Logging.Splunk 



[![Build Status](https://karumbo.visualstudio.com/ColeSoft.Extensions.Logging.Splunk/_apis/build/status/ColeSoft.Extensions.Logging.Splunk-CI?branchName=master)](https://karumbo.visualstudio.com/ColeSoft.Extensions.Logging.Splunk/_build/latest?definitionId=6&branchName=master) 
[![NuGet Version](http://img.shields.io/nuget/v/ColeSoft.Extensions.Logging.Splunk.svg?style=flat)](https://www.nuget.org/packages/ColeSoft.Extensions.Logging.Splunk/)
[![NuGet Version](http://img.shields.io/nuget/vpre/ColeSoft.Extensions.Logging.Splunk.svg?style=flat)](https://www.nuget.org/packages/ColeSoft.Extensions.Logging.Splunk/)

Log to [Splunk][0] directly from [Microsoft.Extensions.Logging][1] using the [Splunk HTTP Event Collector (HEC)][5].

The Splunk HEC will need to be configured on the Splunk server as [detailed here][5].
## Installation

ColeSoft.Extensions.Logging.Splunk installs through [NuGet][3] and requires [.NET Standard][4] >= [2.0][6].

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
As a minimum the following configuration must be supplied so that the provider knows where 
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
properties on an instance of a `SplunkLoggerOptions` object in a delegate supplied to the 
`AddSplunk` call as show below:
```c#
logging.AddSplunk(options => {
        options.SplunkCollectorUrl = "https://splunk-server-name:8088/services/collector/";
        options.AuthenticationToken = "92C168CF-C097-45F3-A3A8-128C3C509E9F";        
    });
```

## Usage

Log messages to Splunk, just as with every other provider:

```c#
logger.LogInformation("This is some information");
```

## Advanced Topics

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

### Splunk Endpoint (Json vs Raw)
Starting with Splunk 6.4 then it is possible to transmit data to a Raw event collector as well as the default Json event collector.  

Overloads of the `AddSplunk` call will accept a parameter controling the endpoint targeted. 
In the case of Raw then the data is formatted in a default manner (not as a Json string) and passed to the 
`services/collector/raw` endpoint.  The format of this payload can be customised as detailed in the section [below][7].
```c#
logging.AddSplunk(SplunkEndpoint.Raw)
```

### Payload Customisation
The format of the data that is sent to Splunk can be customised via a delegate supplied to the `AddSplunk` call.
This customised payload is treated slightly differently defending upon the endpoint being used, as detailed below.
### Json endpoint
For the Json endpoint the returned object is passed to the [Newtonsoft.Json][8] library for serialisation.  A simple example 
with an anonymous type is shown below, but a concrete type could also be used making use of more advanced serialzation 
features from the library.

```c#
logging.AddSplunk(
    data => 
        new
        {
            time = data.Timestamp,
            level = data.Level,
            message = data.Message
        });
```
### Raw endpoint
For the Raw endpoint the returned object has `ToString` called upon it.  In the example below just simple a string is returned
but this could be a more complex object which overrides ToString.

```c#
logging.AddSplunk(
    SplunkEndpoint.Raw,
    data => 
    {
        var sb = new StringBuilder();
        sb.Append($"{data.Timestamp}:{data.CategoryName}:{data.Level}:{data.Message}");
        if (data.Exception != null)
        {
            sb.Append(data.Exception);
        }
        return sb.ToString();
    });
```

## Also...
Check out Andrew Horth's [Event Flow collector for the Splunk HEC][9].

[0]: https://www.splunk.com/
[1]: https://github.com/aspnet/Logging
[2]: https://github.com/loggr/loggr-dotnet
[3]: https://www.nuget.org/packages/ColeSoft.Extensions.Logging.Splunk
[4]: https://docs.microsoft.com/en-us/dotnet/standard/net-standard
[5]: https://docs.splunk.com/Documentation/Splunk/latest/Data/UsetheHTTPEventCollector
[6]: https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md
[7]: https://github.com/coleman-c/ColeSoft.Extensions.Logging.Splunk#payload-customisation
[8]: https://github.com/JamesNK/Newtonsoft.Json
[9]: https://github.com/hortha/diagnostics-eventflow-splunk