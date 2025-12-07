import SwiftUI

// MARK: - Settings Keys

private enum SettingsKey {
    static let enabled = "gonhanh.enabled"
    static let method = "gonhanh.method"
}

// MARK: - Input Mode

enum InputMode: Int {
    case telex = 0
    case vni = 1
}

// MARK: - Settings View

struct SettingsView: View {
    @State private var enabled = true
    @State private var mode: InputMode = .telex

    var body: some View {
        VStack(alignment: .leading, spacing: 20) {
            // Header
            Text("⚡ GoNhanh")
                .font(.system(size: 24, weight: .bold))

            Divider()

            // Enable toggle
            Toggle("Bật bộ gõ tiếng Việt", isOn: $enabled)
                .toggleStyle(.switch)

            // Mode selection
            VStack(alignment: .leading, spacing: 8) {
                Text("Chế độ gõ")
                    .font(.headline)

                Picker("", selection: $mode) {
                    Text("Telex (aw, ow, w, s, f, r, x, j)").tag(InputMode.telex)
                    Text("VNI (a8, o9, 1-5)").tag(InputMode.vni)
                }
                .pickerStyle(.radioGroup)
            }

            Spacer()

            // Buttons
            HStack {
                Spacer()

                Button("Hủy") {
                    NSApp.keyWindow?.close()
                }
                .keyboardShortcut(.cancelAction)

                Button("Lưu") {
                    saveSettings()
                }
                .keyboardShortcut(.defaultAction)
                .buttonStyle(.borderedProminent)
            }
        }
        .padding(20)
        .frame(width: 400, height: 300)
        .onAppear {
            loadSettings()
        }
    }

    func loadSettings() {
        let defaults = UserDefaults.standard

        // Load enabled state (default: true)
        if defaults.object(forKey: SettingsKey.enabled) != nil {
            enabled = defaults.bool(forKey: SettingsKey.enabled)
        }

        // Load input method (default: telex)
        let methodValue = defaults.integer(forKey: SettingsKey.method)
        mode = InputMode(rawValue: methodValue) ?? .telex
    }

    func saveSettings() {
        let defaults = UserDefaults.standard
        defaults.set(enabled, forKey: SettingsKey.enabled)
        defaults.set(mode.rawValue, forKey: SettingsKey.method)

        // Sync with Rust engine
        RustBridge.setEnabled(enabled)
        RustBridge.setMethod(mode.rawValue)

        NSApp.keyWindow?.close()
    }
}

struct SettingsView_Previews: PreviewProvider {
    static var previews: some View {
        SettingsView()
    }
}
