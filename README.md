# Fenrir #
[![NuGet](https://img.shields.io/nuget/v/fenrir.svg?style=flat-square&label=nuget+fenrir)](https://www.nuget.org/packages/fenrir/)
[![NuGet](https://img.shields.io/nuget/v/fenrir.core.svg?style=flat-square&label=nuget+core)](https://www.nuget.org/packages/fenrir.core/)

## Purpose ##

Fenrir is a service testing framework that load tests, stresses, and compares results of micro-services. Requests are generated from data sources (web-service, database, flat file, etc.) or predefined in json following comparable request format. It supports image, json, xml, and text comparison targets. It is meant to be flexible and to be integrated into development pipelines.

![Request Cli](https://raw.githubusercontent.com/jorelius/Fenrir/master/documentation/fenrir_request.JPG "Fenrir request cli")

## Console ##

Fenrir is packaged as a dotnet console tool. It exposes many of the features the core framework provides. 

### Prerequisites ###

dotnet core sdk 2.0.3 or greater

### Install ###

```console
$ dotnet tool install fenrir -g
```

### Usage ###

Pre-generated Json httpRequest Tree file

```console
$ fenrir request -f ./jsonrequest.json -t 20
```

Generator Plugin

```console
$ fenrir generator "Plugin Name" -t 20
```

Simple Load Test

```console
$ fenrir simple "http://your.service.io/some/get" -t 20 -c 200
```

Console Help

![help](https://raw.githubusercontent.com/jorelius/Fenrir/master/documentation/fenrir_help.JPG "Fenrir Help")

### Plugins ###

The console supports plugins that same way the framework does with one caveat, the plugin director is located under the current users home directory in the ".fenrir" sub directory.

```
+-- ~/.fenrir
|   +--plugins
|      +-- {plugin project dir}
|          +-- {plugin name}.dll
```

## Framework ##

RequestTreeAgent

At the center of this framework is RequestTreeAgent. Its main function is to take a HttpRequestTree and flattening the tree into sets of parallelizable requests that are then requested at some level of concurrency.  

IRequestGenerator

Interface for integrating with Fenrir extension system for 3rd party development of request generators.

## Comparison Request Json ##

Fenrir supports input via the Json httpRequest Tree Schema. This format enables describing http requests in a consumable format.

```json
[
    {
        "pre": [
            {
                "url":"http://www.example.com/preThings",
                "method":"post",
                "payload": {
                    "headers": [
                        {
                            "name": "Content-Type",
                            "value": "application/json"
                        }
                    ],
                    "body": {
                        "what":"thing2"
                    }
                },
                "expectedResult": {
                    "code": 201,
                    "payload": {
                        "headers": [
                            {
                                "name": "Content-Type",
                                "value": "application/json"
                            }
                        ],
                        "body": {
                            "what":"thing2"
                        }
                    }
                }
            }
        ],
        "url":"http://www.example.com/things/123",
        "method":"get",
        "expectedResult": {
            "code": 200,
            "payload": {
                "headers": [
                    {
                        "name": "Content-Type",
                        "value": "application/json"
                    }
                ],
                "body": {
                    "id":"123",
                    "what":"thing"
                }
            }
        }
    }
]
```

## Request Plugins ##

Fenrir supports a plugin model for generating requests programmatically. Plugins implement the "IRequestGenerator" interface. This interface allows you to extend the functionality of fenrir and dynamically generate request for various scenarios (load testing, service environment comparison, etc.) and control how they are compared to expected values.

```csharp
    public class TestPlugin : IRequestGenerator
    {
        public Guid Id => Guid.Parse("pre-generated guid");

        public string Name => "Plugin Name";

        public string Description => "Provides a detailed description for console";
        
        public List<Option> Options { get; set; } = new List<Option>
        {
            new Option(new OptionDescription("FirstOption", "1", "option description"))
        };

        ResultComparerFactory ComparerFactoryOverride { get; set; } = null;

        public IEnumerable<Request> Run()
        {
            var count = int.Parse(Options[0].Value);
            for(int i = 0; i < count; i++)
            {
                yield return new Request();
            }
        }
    }
```

## Development ##

    dotnet restore
    dotnet build

### Dependencies ###

* dot net core v2.0.3

### How to run tests ###

    dotnet test

## Contribution guidelines ##

* Writing tests
* Code review
* Other guidelines

## Acknowledgements ##
* [JsonDiffPatch.Net](https://github.com/wbish/jsondiffpatch.net)
* [XUnit](https://xunit.github.io/)
* [Netling](https://github.com/hallatore/Netling)
* [Json httpRequest Tree Schema](https://github.com/jorelius/json-httprequest-tree)

