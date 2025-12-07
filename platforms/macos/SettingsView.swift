import SwiftUI

// MARK: - Settings View

struct SettingsView: View {
    @State private var enabled = true
    @State private var mode: InputMode = .telex

    var body: some View {
        VStack(alignment: .leading, spacing: 20) {
            // Header
            HStack {
                Image(systemName: "keyboard.fill")
                    .font(.title2)
                    .foregroundColor(.accentColor)
                Text(AppMetadata.name)
                    .font(.system(size: 22, weight: .bold))
            }

            Divider()

            // Enable toggle
            Toggle("Bật bộ gõ tiếng Việt", isOn: $enabled)
                .toggleStyle(.switch)

            // Mode selection
            VStack(alignment: .leading, spacing: 10) {
                Text("Kiểu gõ")
                    .font(.headline)

                ForEach(InputMode.allCases, id: \.rawValue) { inputMode in
                    ModeRadioButton(
                        mode: inputMode,
                        isSelected: mode == inputMode,
                        onSelect: { mode = inputMode }
                    )
                }
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
        .frame(width: 400, height: 320)
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

        // Notify menu bar to update
        NotificationCenter.default.post(name: .settingsChanged, object: nil)

        NSApp.keyWindow?.close()
    }
}

// MARK: - Mode Radio Button

struct ModeRadioButton: View {
    let mode: InputMode
    let isSelected: Bool
    let onSelect: () -> Void

    var body: some View {
        Button(action: onSelect) {
            HStack {
                Image(systemName: isSelected ? "largecircle.fill.circle" : "circle")
                    .foregroundColor(isSelected ? .accentColor : .secondary)

                VStack(alignment: .leading, spacing: 2) {
                    Text(mode.name)
                        .font(.body)
                        .foregroundColor(.primary)

                    Text(mode.description)
                        .font(.caption)
                        .foregroundColor(.secondary)
                }

                Spacer()
            }
            .padding(.vertical, 4)
        }
        .buttonStyle(.plain)
    }
}

// MARK: - Notifications

extension Notification.Name {
    static let settingsChanged = Notification.Name("settingsChanged")
}

// MARK: - Preview

struct SettingsView_Previews: PreviewProvider {
    static var previews: some View {
        SettingsView()
    }
}
