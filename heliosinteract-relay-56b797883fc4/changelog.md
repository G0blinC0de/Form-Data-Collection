# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Changed
- Target framework updated to .NET Core 3.1
- Files are cached once per post, rather than per Relay
- More informative logs from various modules
- Dispatch module writes DispatchRequests.yml for fine debugging
- Updated various plugins
- Updated modules to follow a more standard format
### Removed
- Unused/inconsistent treatment of FileInfo pointing at a file on-disk (now consistently ignored)

## [3.5.2] - 2022-10-15
### Fixed
- Bug where Eshots answers values were not serialized at all

## [3.5.1] - 2022-09-28
### Fixed
- Bug where Eshots answers values were not serialized correctly

## [3.5.0] - 2022-09-19
### Added
- Eshots relay
- New exceptions classes

## [3.4.4] - 2022-09-02
### Fixed
- Bug which caused RequestLogger to throw a NullReferenceException for certain http responses

## [3.4.3] - 2022-09-02
### Added
- All logs are recorded in Logs/relay.log
- Patron requests are logged to Logs/patronRequests.yml
### Changed
- Revamped logs across the entire app to better show which Relay and Entry are creating the log
### Fixed
- Bug where nested data types were serialized as JToken types rather than object collections

## [3.4.2] - 2022-07-21
### Fixed
- Bug where nested data types within metadata would be rejected even when valid

## [3.4.1] - 2022-06-27
### Changed
- RFile uses property get/set methods to make sure that name and mimeType values are not stale
### Removed
- RFile.GetName(), RFile.GetPath(), and RFile.GetMimeType() -- use RFile.path, RFile.name, and RFile.mimeType instead
### Fixed
- Bug where files with the same name would overwrite each other in the same cache

## [3.4.0] - 2022-06-24
### Added
- Patron relay module
- Dummy relay module

## [3.3.1] - 2022-05-17
### Changed
- Reduced log clutter from Dispatch relay
- Dispatch relay handles and reports signficantly more exceptions
### Fixed
- Issue where Polygon relay did not correctly handle not receiving any file at all

## [3.3.0] - 2022-05-12
### Added
- Polygon relay module
- Polygon documentation in readme

## [3.2.0] - 2022-04-05
### Added
- Keen relay module
- Keen documentation in readme
### Changed
- Formatting in changelog

## [3.1.1] - 2022-04-01
### Added
- Added changelog
### Changed
- Reformatted code in several places, moved `RelayService` enum to its own file
- Cleaner logic for checking Relay Service flags
- Major revisions to readme

## [3.1.0] - 2022-03-19
### Added
- CSV Export relay module
### Changed
- Updated readme
- Moved Google UA module classes into namespace `Helios.Relay.GoogleUa`

## [3.0.0] - 2022-02-17
### Added
- Dispatch relay module
- Google Analytics (UA) relay module
### Changed
- Relay requests can now target multiple relay services
- Refactoring for relay module base classes/interfaces for easier module additions
- Additional case handling in Reach/Twilio modules
- Changed build settings so that Relay builds to a standalone application

## [Older Versions] - Changelog not recorded. See git history.