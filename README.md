# README #

## Purpose ##

Fenrir is a service testing framework that compares results of micro-services. Requests are generated from data sources (web-service, database, flat file, etc.) or predefined in json following comparable request format. It supports image, json, xml, and text comparison sources. It is meant to be flexible and to be integrated into development pipelines.

## Usage ##

### Comparison Request Json ###

```json
[
    {
        "pre": [
            {
                "url":"http://www.example.com/preThings",
                "verb":"post",
                "payload": {
                    "content-type":"application/json",
                    "body": {
                        "what":"thing2"
                    }
                },
                "expectedResult": {
                    "code": 201,
                    "content-type":"application/json",
                    "body": {
                        "id":"*",
                        "what":"thing2"
                    }
                }
            }
        ],
        "url":"http://www.example.com/things/123",
        "verb":"get",
        "comparisonRequest":{
            "url":"http://test.example.com/things/123",
            "verb":"get"
        },
        "expectedResult": {
            "code": 200,
            "content-type":"application/json",
            "body": {
                "id":"123",
                "what":"thing"
            }
        }
    }
]
```

### Request Plugins ###

```
.
+--bin
|  +--plugins
|     +-- fenrir.plugin.{plugin name}
|         +-- fenrir.plugin.{plugin name}.json
|         +-- fenrir.plugin.{plugin name}.dll
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

* [DocFX](https://dotnet.github.io/docfx/)
* [Fluent Assertions](http://www.fluentassertions.com/)
* [XUnit](https://xunit.github.io/)
* [StyleCopAnalyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
* [Cake](https://github.com/cake-build/cake)
* [PowerArgs](https://github.com/adamabdelhamed/PowerArgs)
