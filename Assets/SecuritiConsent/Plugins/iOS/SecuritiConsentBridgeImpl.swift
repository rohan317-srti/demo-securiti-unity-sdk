// SecuritiConsentBridgeImpl.swift
// Real ConsentUI SDK bridge using @_cdecl for Swift ↔ C interop.
// Unity calls these functions via DllImport("__Internal").

import ConsentUI
import Foundation

// MARK: - Cached state (populated on SDK ready, before OnSDKReady fires)

private var _cachedPurposesJSON    = "[]"
private var _cachedPermissionsJSON = "[]"
private var _cachedPurposes: [ConsentUI.Purpose] = []
private var _cachedPermissions: [ConsentUI.AppPermission] = []
private var _sdkReadySent = false
private var _cachedAppUrl = ""

// MARK: - Unity message bridge
// UnitySendMessage is defined in UnityFramework; declare it here without a header.

@_silgen_name("UnitySendMessage")
func UnitySendMessage(_ obj: UnsafePointer<CChar>,
                      _ method: UnsafePointer<CChar>,
                      _ msg: UnsafePointer<CChar>)

private func sendToUnity(_ method: String, _ msg: String) {
    "SecuritiConsentSDK".withCString { obj in
        method.withCString { m in
            msg.withCString { s in
                UnitySendMessage(obj, m, s)
            }
        }
    }
}

// MARK: - ConsentStatus mapping
// C# ConsentStatus: NOT_DETERMINED=0, GRANTED=1, DECLINED=2, WITHDRAWN=3

private func toInt(_ status: ConsentUI.ConsentStatus?) -> Int32 {
    switch status {
    case .notDetermined:  return 0   // NOT_DETERMINED
    case .granted:        return 1   // GRANTED
    case .declined:       return 2   // DECLINED
    case .withDrawn:      return 3   // WITHDRAWN
    case .none:           return 0   // NOT_DETERMINED
    @unknown default:     return 0   // NOT_DETERMINED
    }
}

private func toConsentStatus(_ value: Int32) -> ConsentUI.ConsentStatus {
    switch value {
    case 0:  return .notDetermined
    case 1:  return .granted
    case 2:  return .declined
    case 3:  return .withDrawn
    default: return .notDetermined
    }
}

private func consentLabel(_ v: Int32) -> String {
    switch v {
    case 0:  return "NOT_DETERMINED"
    case 1:  return "GRANTED"
    case 2:  return "DECLINED"
    case 3:  return "WITHDRAWN"
    default: return "NOT_DETERMINED"
    }
}

// MARK: - LogLevel mapping
// C# LoggerLevel: DEBUG, INFO, WARNING, ERROR

private func mapLogLevel(_ s: String) -> ConsentUI.LogLevel {
    switch s.uppercased() {
    case "DEBUG":   return .debug
    case "WARNING": return .warning
    case "ERROR":   return .error
    default:        return .info
    }
}

// MARK: - JSON builders

private func buildPurposesJSON(_ list: [ConsentUI.Purpose]) -> String {
    var items: [[String: Any]] = []
    for p in list {
        let statusInt = toInt(p.consentStatus)
        let name = p.purposeName?["en"] ?? p.purposeName?.values.first ?? ""
        let desc = p.purposeDescription?["en"] ?? p.purposeDescription?.values.first ?? ""
        items.append([
            "id":            "\(p.purposeId ?? 0)",
            "name":          name,
            "description":   desc,
            "consent":       statusInt,
            "consentString": consentLabel(statusInt),
            "required":      !(p.disableOptOut ?? false)
        ])
    }
    guard let data = try? JSONSerialization.data(withJSONObject: items),
          let s = String(data: data, encoding: .utf8) else { return "[]" }
    return s
}

private func buildPermissionsJSON(_ list: [ConsentUI.AppPermission]) -> String {
    var items: [[String: Any]] = []
    for perm in list {
        let statusInt = toInt(perm.consentStatus)
        let desc = perm.description?["en"] ?? perm.description?.values.first ?? ""
        items.append([
            "id":            "\(perm.id ?? 0)",
            "name":          perm.name ?? "",
            "description":   desc,
            "consent":       statusInt,
            "consentString": consentLabel(statusInt)
        ])
    }
    guard let data = try? JSONSerialization.data(withJSONObject: items),
          let s = String(data: data, encoding: .utf8) else { return "[]" }
    return s
}

// MARK: - C bridge functions

@_cdecl("_InitializeConsentSDK")
public func _InitializeConsentSDK(
    _ appURL:              UnsafePointer<CChar>?,
    _ cdnURL:              UnsafePointer<CChar>?,
    _ tenantID:            UnsafePointer<CChar>?,
    _ appID:               UnsafePointer<CChar>?,
    _ testingMode:         Bool,
    _ loggerLevel:         UnsafePointer<CChar>?,
    _ consentsCheckInterval: Int32,
    _ subjectId:           UnsafePointer<CChar>?,
    _ languageCode:        UnsafePointer<CChar>?,
    _ locationCode:        UnsafePointer<CChar>?
) {
    let appUrl  = appURL  .map { String(cString: $0) } ?? ""
    let cdnUrl  = cdnURL  .map { String(cString: $0) } ?? ""
    let tenant  = tenantID.map { String(cString: $0) } ?? ""
    let appId   = appID   .map { String(cString: $0) } ?? ""
    let level   = loggerLevel.map { String(cString: $0) } ?? "INFO"

    func optStr(_ p: UnsafePointer<CChar>?) -> String? {
        guard let p = p else { return nil }
        let s = String(cString: p)
        return s.isEmpty ? nil : s
    }
    let subject  = optStr(subjectId)
    let language = optStr(languageCode)
    let location = optStr(locationCode)

    let opts = ConsentSDKOptions(
        appUrl:                appUrl,
        cdnUrl:                cdnUrl,
        tenantId:              tenant,
        appId:                 appId,
        testingMode:           testingMode,
        logLevel:              mapLogLevel(level),
        consentsCheckInterval: Int(consentsCheckInterval),
        subjectId:             subject,
        languageCode:          language,
        locationCode:          location
    )

    _sdkReadySent = false
    _cachedAppUrl = appUrl
    ConsentSDK.shared.setupSDK(options: opts)

    ConsentSDK.shared.isReady { status in
        // .notAvailable is the SDK's initial state before it finishes loading;
        // .inProgress means it is still loading — both are transient, so ignore them.
        // Only act when the SDK explicitly reports .available.
        guard status == .available, !_sdkReadySent else { return }
        _sdkReadySent = true

        // Pre-fetch purposes and permissions BEFORE firing OnSDKReady so that
        // C# callers can immediately use _GetPurposes() / _GetPermissions().
        Task.detached {
            async let pFetch    = ConsentSDK.shared.getPurposeConsents()
            async let permFetch = ConsentSDK.shared.getPermissionConsents()
            let (purposes, perms) = await (pFetch, permFetch)
            _cachedPurposes        = purposes
            _cachedPermissions     = perms
            _cachedPurposesJSON    = buildPurposesJSON(purposes)
            _cachedPermissionsJSON = buildPermissionsJSON(perms)
            sendToUnity("OnSDKReady", "{\"status\":\"ready\"}")
        }
    }
}

@_cdecl("_PresentConsentBanner")
public func _PresentConsentBanner() {
    DispatchQueue.main.async {
        ConsentSDK.shared.presentConsentBanner()
    }
}

@_cdecl("_PresentPreferenceCenter")
public func _PresentPreferenceCenter() {
    DispatchQueue.main.async {
        ConsentSDK.shared.presentPreferenceCenter()
    }
}

@_cdecl("_GetSDKStatus")
public func _GetSDKStatus() -> Int32 {
    return Int32(ConsentSDK.shared.getStatus().rawValue)
}

/// Returns a malloc-allocated C string (Unity copies it via PtrToStringAnsi; leaked intentionally).
@_cdecl("_GetPurposes")
public func _GetPurposes() -> UnsafePointer<CChar>? {
    return UnsafePointer(strdup(_cachedPurposesJSON))
}

/// Returns a malloc-allocated C string (same memory model as _GetPurposes).
@_cdecl("_GetPermissions")
public func _GetPermissions() -> UnsafePointer<CChar>? {
    return UnsafePointer(strdup(_cachedPermissionsJSON))
}

@_cdecl("_GetConsentByPurposeId")
public func _GetConsentByPurposeId(_ purposeId: UnsafePointer<CChar>?) -> Int32 {
    guard let p = purposeId, let idInt = Int(String(cString: p)) else { return 0 }
    let status = ConsentSDK.shared.getConsent(purposeId: idInt)
    return toInt(status)
}

@_cdecl("_SetPurposeConsent")
public func _SetPurposeConsent(_ purposeId: UnsafePointer<CChar>?, _ consentStatus: Int32) -> Bool {
    guard let p = purposeId else { return false }
    let idStr      = String(cString: p)
    let newConsent = toConsentStatus(consentStatus)
    guard var purpose = _cachedPurposes.first(where: { "\($0.purposeId ?? -1)" == idStr }) else {
        return false
    }
    purpose.consentStatus = newConsent
    let result = ConsentSDK.shared.setConsent(purpose: purpose, consent: newConsent)
    if let idx = _cachedPurposes.firstIndex(where: { "\($0.purposeId ?? -1)" == idStr }) {
        _cachedPurposes[idx].consentStatus = newConsent
        _cachedPurposesJSON = buildPurposesJSON(_cachedPurposes)
    }
    return result
}

@_cdecl("_SetPermissionConsent")
public func _SetPermissionConsent(_ permissionId: UnsafePointer<CChar>?, _ consentStatus: Int32) -> Bool {
    guard let p = permissionId else { return false }
    let idStr      = String(cString: p)
    let newConsent = toConsentStatus(consentStatus)
    guard var permission = _cachedPermissions.first(where: { "\($0.id ?? -1)" == idStr }) else {
        return false
    }
    permission.consentStatus = newConsent
    let result = ConsentSDK.shared.setConsent(permission: permission, consent: newConsent)
    if let idx = _cachedPermissions.firstIndex(where: { "\($0.id ?? -1)" == idStr }) {
        _cachedPermissions[idx].consentStatus = newConsent
        _cachedPermissionsJSON = buildPermissionsJSON(_cachedPermissions)
    }
    return result
}

@_cdecl("_GetConsentByPermissionId")
public func _GetConsentByPermissionId(_ permissionId: UnsafePointer<CChar>?) -> Int32 {
    guard let p = permissionId else { return 0 }
    let idStr = String(cString: p)
    if let perm = _cachedPermissions.first(where: { "\($0.id ?? -1)" == idStr }) {
        return toInt(perm.consentStatus)
    }
    return 0
}

// NOTE: GCM (Google Consent Mode) getter APIs were added in ConsentUI > 1.140.
// CocoaPods trunk currently pins to 1.140.0, which doesn't expose
// getGCMConsents() / getGCMConfig(). Stubbed to return empty JSON so the
// iOS build compiles. Restore the full implementation once the pod is
// pinned to 1.144+ via a custom podspec repo or direct git source.

@_cdecl("_GetGCMConsents")
public func _GetGCMConsents() -> UnsafePointer<CChar>? {
    return UnsafePointer(strdup("[]"))
}

@_cdecl("_GetGCMConfig")
public func _GetGCMConfig() -> UnsafePointer<CChar>? {
    return UnsafePointer(strdup("{}"))
}

@_cdecl("_GetBannerConfig")
public func _GetBannerConfig() -> UnsafePointer<CChar>? {
    var result = "{}"
    let semaphore = DispatchSemaphore(value: 0)
    Task.detached {
        if let config = await ConsentSDK.shared.getBannerConfig() {
            // BannerConfig conforms to Codable with snake_case CodingKeys
            if let data = try? JSONEncoder().encode(config),
               let s = String(data: data, encoding: .utf8) {
                result = s
            }
        }
        semaphore.signal()
    }
    semaphore.wait()
    return UnsafePointer(strdup(result))
}

@_cdecl("_GetSettingsPrompt")
public func _GetSettingsPrompt() -> UnsafePointer<CChar>? {
    var result = "{}"
    let semaphore = DispatchSemaphore(value: 0)
    Task.detached {
        if let prompt = await ConsentSDK.shared.getSettingsPrompt() {
            var dict: [String: Any] = [:]
            if let v = prompt.promptHeading     { dict["prompt_heading"] = v }
            if let v = prompt.promptMessage      { dict["prompt_message"] = v }
            if let v = prompt.settingsButtonText { dict["settings_button_text"] = v }
            if let v = prompt.notNowButtonText   { dict["not_now_button_text"] = v }
            if let v = prompt.permissions        { dict["permissions"] = v }
            if let data = try? JSONSerialization.data(withJSONObject: dict),
               let s = String(data: data, encoding: .utf8) {
                result = s
            }
        }
        semaphore.signal()
    }
    semaphore.wait()
    return UnsafePointer(strdup(result))
}

@_cdecl("_UploadConsents")
public func _UploadConsents(_ jsonStr: UnsafePointer<CChar>?) {
    guard let p = jsonStr else { return }
    let json = String(cString: p)
    Task.detached {
        guard let data = json.data(using: .utf8),
              let parsed = try? JSONSerialization.jsonObject(with: data) as? [String: Any] else {
            sendToUnity("OnError", "{\"error\":\"Failed to parse upload request\"}")
            return
        }

        // Build PostConsentsRequest from parsed JSON
        let purposeConsents: [ConsentUI.PurposeConsent] = (parsed["purpose_consents"] as? [[String: Any]] ?? []).compactMap { item in
            guard let pid = item["purpose_id"] as? Int,
                  let statusStr = item["consent_status"] as? String,
                  let ts = item["timestamp"] as? Int64 else { return nil }
            let status = ConsentUI.ConsentStatus(rawValue: statusStr) ?? .notDetermined
            let essential = item["is_essential"] as? Bool ?? false
            return ConsentUI.PurposeConsent(purposeID: pid, consentStatus: status, timestamp: ts, isEssential: essential)
        }

        let permConsents: [ConsentUI.PermissionConsent] = (parsed["permissions"] as? [[String: Any]] ?? []).compactMap { item in
            guard let perm = item["permission"] as? String,
                  let statusStr = item["consent_status"] as? String,
                  let ts = item["timestamp"] as? Int64 else { return nil }
            let status = ConsentUI.ConsentStatus(rawValue: statusStr) ?? .notDetermined
            guard let key = ConsentUI.PrivaciKey(rawValue: perm) else { return nil }
            return ConsentUI.PermissionConsent(permission: key, consentStatus: status, timestamp: ts)
        }

        let request = ConsentUI.PostConsentsRequest(
            uuid: parsed["uuid"] as? String ?? "",
            appUUID: parsed["app_uuid"] as? String ?? "",
            device: parsed["device"] as? String ?? "",
            implicitConsent: parsed["implicit_consent"] as? Bool ?? false,
            version: parsed["version"] as? Int ?? 0,
            purposeConsents: purposeConsents,
            permissions: permConsents,
            testingMode: parsed["is_test_mode"] as? Bool ?? false,
            adId: parsed["ad_id"] as? String ?? "",
            bannerInfo: parsed["banner_info"] as? String ?? ""
        )

        await ConsentSDK.shared.uploadConsents(url: _cachedAppUrl, request: request)
        sendToUnity("OnConsentsUploaded", "{\"success\":true}")
    }
}

@_cdecl("_RemoveListeners")
public func _RemoveListeners() {
    ConsentSDK.shared.removeListeners()
}

@_cdecl("_ResetConsents")
public func _ResetConsents() {
    ConsentSDK.shared.removeConsents()
    _cachedPurposes          = []
    _cachedPermissions       = []
    _cachedPurposesJSON      = "[]"
    _cachedPermissionsJSON   = "[]"
    sendToUnity("OnConsentsReset", "{}")
}
