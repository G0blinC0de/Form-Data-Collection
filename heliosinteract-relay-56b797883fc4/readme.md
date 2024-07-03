# Relay
##### Version 3.6.0
---

## Overview
Relay accepts requests that need to be sent to a relay service. Each request is cached in a database and queued for delivery. If an entry fails to send, it returns to the back of the queue to be reattempted later.

If an entry fails to send for a reason deemed terminal (such as missing data or missing a file required for sending) it will be temporarily moved to a terminal database. If the main database is empty, the service will cycle through entries in the terminal database to see if they have been fixed.

---
# Relay Services
## Supported Services
Relay currently supports the following services:

* Dispatch
* Google Analytics (UA)
* CSV write to disk
* Keen
* Polygon (Note: this service is exclusively for the LiveNation x Coinbase WAGMI Club project)
* Patron
* Eshots
* Dummy (Note: No remote endpoint, always returns success)

Legacy modules exist for the following services:

* Reach
* Twilio

***Note:** Legacy modules are not supported and are not guaranteed to work. If your project requires one of these legacy modules, please discuss reviving that module with the dev team.*

## Unity Module

For integrating Unity projects with Relay, use the [Relay Unity Plugin]("https://bitbucket.org/heliosinteract/relay-unity-plugin.git/src/master/).

## Selecting a Relay Service

Each request to Relay includes an integer value for the "RelayService" field. Relay evaluates this integer bitwise, as a set of flags.

**Quick Solution**: To determine which integer to send, add the values of your desired Relay Services from the table below.

| Relay Service           | Integer    |
| :---                    | :---       |
| Reach *(legacy)*        | 1          |
| Twilio *(legacy)*       | 2          |
| Dispatch                | 4          |
| Google Analytics (UA)   | 8          |
| CSV Export              | 16         |
| Keen                    | 32         |
| Polygon                 | 64         |
| Patron                  | 128        |
| Eshots                  | 256        |
| Dummy                   | 1073741824 |

Example:
```
Dispatch = 4
Google Analytics = 8

Dispatch + Google Analytics = 12

The number 12 as a 32-bit signed integer:
00000000_00000000_00000000_00001100

You can read the flags from right to left:
Reach            → 0
Twilio           → 0
Dispatch         → 1
Google Analytics → 1
CSV Export       → 0
Keen             → 0
Polygon          → 0
Patron           → 0
Eshots           → 0
(unused slots)   → 0 x21
Dummy            → 0
(always zero)    → 0
```

# Configuration

## Configuration Files
Relay configures on launch from a set of JSON files. These files are located in `Relay/Configuration`.

## Global Configuration Settings
**File:**  `relay.json`

These settings are shared among all Relay Services.

* DatabaseConnectionString: (string) Encrypted database connection string for LiteDb. See Encryption.
* EnableDummyRelay: (bool) Whether to enable Relay's Dummy module.
<!--
* WaitBeforeRetryMilliseconds: (int) The time in milliseconds to wait after a failed attempt has been made to send out information before retrying.
* TimeoutMilliseconds: (int) The amount of time to wait in milliseconds before timing-out when waiting for a response.
-->

## Raven Logging Service
**File:** `raven.json`

* Address: (string) The ip address for communicating with Woodpecker.
* Endpoint: (int) The ip end point for communicating with Woodpecker.

## Dispatch
**File:** `dispatch.json`

* Url: (string) The base url for the Dispatch API.
* SignedUrlTimeout: (int) The time, in seconds, after which a signed PUT url retrieved from the Dispatch service will expire (300 by default).

## Google Analytics (UA)
**File:** `google-ua.json`

* PropertyId: (string) The id of the Google Analytics UA property.
* Events: (Event[]) A list of events which will be sent to the Google Analytics property. See below.

### Events
Event objects contain four fields. Google Analytics requires the `category` and `action` fields. The `label` and `value` fields are optional.

* category: (DataSource&lt;string>) The name of the metric (such as "GuestName" or "PointsScored").
* action: (DataSource&lt;string>) The action type performedin this event. This is usually something like "selected".
* label: (DataSource&lt;string>) If the collected metric is a string, this field stores the actual data of the metric.
* value: (DataSource&lt;int>) If the collected metric is a number, this field stores the actual data of the metric.

Using a DataSource object (see below), each of the four fields in an event can be set to a direct value, or they can pull a value from a metadata field in the Relay entry. Additionally, the `label` and `value` fields can be configured to be ignored entirely.

Example Event:
```json
{
  "category": { "value": "PointsScored" },
  "action": { "value": "selected" },
  "label": { "ignore": true },
  "value": {
    "source": "Experience",
    "key": "pointsScored"
    }
}
```

### DataSource
There are three configurations for a DataSource: `value`, `source/key`, and `ignore`. See below for examples of how to use each configuration.

#### Value Configuration
Represents a literal value. This is typically used for the `category` and `action` fields of an Event.

Example:
```json
"category": { "value": "PointsScored" }
```

#### Source/Key Configuration
Indicates that the data should be pulled from the metadata of a Relay entry. This is typically used for the actual metric data collected.

* Valid values for `source` are: `Experience`, `Guest`, and `File`
* The `key` field must match a key in the source's metadata

This example will retrieve the value from `Experience.meta["pointsScored"]` in the Relay entry:
```json
"value": {
  "source": "Experience",
  "key": "pointsScored"
}
```

#### Ignore Configuration
Indicates that a field should be ignored entirely.

Example:
```json
"label": { "ignore": true }
```
***NOTE:** The ignore configuration is invalid for the `category` and `action` fields of an Event. If configured this way, Relay will throw an error on startup.*

## CSV Export
**File:** `csv-export.json`

* Path: (string) The path where the CSV file will be created/appended to
* Fields: (Field[]) A list of fields that the CSV exporter will write. See below.

### Fields
* name: (string) The name of the field (written in the header line of the CSV file)
* source: (string) Which metadata to access for the value of the field (valid options are `Experience`, `Guest`, and `File`)
* key: (string) The key for the metadata.

Example:
```json
{
  "name": "Guest Name",
  "source": "Guest",
  "key": "name"
}
```

## Keen
**File:** `keen.json`

* ProjectId: (string) The Keen project ID. 
* EventCollection: (string) A name for the collection of events that will be created in Keen.
* ApiKey: (string) The API key for the Keen project (must have write permissions).

## Polygon
**File:** `polygon.json`

* BaseUrl: (string) The base url where requests are made (include the trailing "api/")
* User: (string) The username that the Token endpoint will authenticate
* Password: (string) The password that the Token endpoint will authenticate. This string is encrypted (see StringEncryption.cs for more information).
* MaxGasPrice: (string) The maximum gas price at which the NFT should be minted.

## Patron
**File:** `patron.json`

* DeveloperEmail: (string) The email that the authorization endpoint will use authenticate.
* DeveloperPassword: (string) The password that the authorization endpoint will use authenticate. This string is encrypted (see StringEncryption.cs for more information).
* BaseUrl: (string) The base url where requests are made (DO NOT include the trailing "api/")
* BrandId: (string) The Patron brandId for the activation.
* EventId: (string) The Patron eventId for the activation.
* PhotoStreamId: (string) The Patron photoStreamId for the activation.
* VideoStreamId: (string) The Patron videoStreamId for the activation.

## Eshots
**File:** `eshots.json`

* BaseUrl: (string) The base url where requests will be sent (https://eshots.io)
* ClientLicenseId: (string) The numeric string representing the Eshots clientLicenseId value 
* Username: (string) The username used for API access
* Password: (string) The password used for API access. This string is encrypted (see StringEncryption.cs for more information).
* DataTransform
  * uId: (string) A JSON path indicating from where the Eshots uId value should be collected (ex. `Guest.metadata.userId`)
  * rElatId: (string) A JSON path indicating from where the Eshots rElatId value should be collected (ex. `Experience.metadata.rElatId`)
  * answers: (key-value map) A map for the fields submitted to Eshots as "answers". The keys will be used as the Eshots keyword, and the value is a JSON path indicating from where to collect the value for that keyword. Example:
    ```json
    "data": {
      "score": "Experience.metadata.score",
      "email": "Guest.email"
    }
    ```

## Reach (legacy)
**File:** `reach.json`

* BaseUrl: (string) The base url for the reach API.
* WriteFileToDisk: (boolean) If the relay service should write the file to disk.
* ActivationId: (string) Absolute file path. This is the file path that files will be written to if they have a matching activationId. Can have more than one ActivationId : Path, seperate entries by comma (Standard Json).

## Twilio (legacy)
**File** `twilio.json`
* AccountSid: (string) The SID of the Twilio account.
* AuthToken: (string) The auth token for the Twilio account.
* PhoneNumber: (string) The phone number that texts will be sent from.
* Message: (string) The message sent to guests.

# Database
The database is located in `Relay/Database`. The file `RelayCache.db` is the main cache where objects are stored until they are sent to their appropriate destinations.

**Password:** Contact Ops (Is this Ops? Contact Dev.)

**Database Viewer:** [LiteDBViewer](https://github.com/falahati/LiteDBViewer/releases)

# Encryption
To keep data secure, Relay decrypts encrypted strings from the config files were we store sensitive information (Passwords, Usernames, Connection Strings, etc).
Because the information is encrypted in the config file, there is a helper method in the app to encrypt (Not decrypt) a string in the event
that one of the encrypted sensitive entries has to be changed out on site. Type /encrypt and follow the prompts in the console to use.
To copy the string out, right click on the console window, select "Properties" and enable QuickEditMode (This will allow you to copy from the terminal window).

# Logging
Red logs and what-to-do.

Log: `[Program] Failed to disable "Quick Edit Mode"`

* **What to Do:** Notify Ops that this was logged. To disable manually right click top bar of console, select properties, toggle off "Quick Edit Mode" (Box should not be checked).

Log: `[ReachRelay] No file was found for cache entry: {cacheEntry.cacheId} at path: {filePath}`

* **What to Do:** Notify Ops that a file is not being found and that an entry is being marked as terminal.

Log: `[RelayController] Returning Message: No relays online.`

* **What to Do:** Notify ops that the relay services are not starting. Make sure the config files are present and have the correct values (If this is logging, they do not).

