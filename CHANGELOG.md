# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.144.0] - 2026-04-22

### Changed
- Aligned Unity package version with Securiti Consent native SDK version (1.144.0)
- Bumped Android native SDK dependency: `ai.securiti.cmpsdkcore:consent-sdk` 1.141.0 → 1.144.0
- Bumped iOS native SDK (ConsentUI CocoaPod) dependency: 1.143.0 → 1.144.0

### Removed
- Hardcoded QA credentials from `ConsentManager.cs` — callers must now supply their own `AppURL`, `CdnURL`, `TenantID`, and `AppID`
- Internal phase/status/completion tracking documents from the repository root

## [0.1.0] - 2026-01-19

### Added
- Initial release of Securiti Consent SDK for Unity
- Android bridge (Kotlin) with JNI interface
- iOS bridge (Objective-C++) with P/Invoke interface
- C# Unity API with platform conditionals
- Editor mock mode for in-editor testing
- EDM4U integration for automatic dependency resolution
- Core SDK functionality:
  - Initialize SDK with configuration options
  - Present consent banner
  - Present preference center
  - Get purposes and consent status
  - Set purpose consent
  - Set permission consent
  - Reset consents
- Unity events for SDK callbacks (OnReady, OnError)
- Basic integration example scene
- ProGuard rules embedded in Android AAR
- CocoaPods integration for iOS via EDM4U
- PostProcessBuild script for iOS Xcode configuration

### Platform Support
- Unity 2021.3 LTS and newer
- Android API 24+ (Android 7.0+)
- iOS 15.0+
- IL2CPP and Mono scripting backends

### Documentation
- README with installation and quick start guide
- API reference for core methods
- Troubleshooting guide
- Basic integration example

### Known Limitations
- Requires External Dependency Manager for Unity (EDM4U) as prerequisite
- iOS requires CocoaPods 1.14.2+
- Android requires Gradle 8.0+

[Unreleased]: https://github.com/rohan317-srti/demo-securiti-unity-sdk/compare/v1.144.0...HEAD
[1.144.0]: https://github.com/rohan317-srti/demo-securiti-unity-sdk/compare/v0.1.0...v1.144.0
[0.1.0]: https://github.com/rohan317-srti/demo-securiti-unity-sdk/releases/tag/v0.1.0
