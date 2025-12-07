import Cocoa
import SwiftUI

// MARK: - Menu Item Tags

private enum MenuTag: Int {
    case enabled = 100
    case telex = 200
    case vni = 201
}

// Share settings keys with SettingsView
private enum SettingsKeys {
    static let enabled = "gonhanh.enabled"
    static let method = "gonhanh.method"
}

class MenuBarController {
    private var statusItem: NSStatusItem!
    private var settingsWindow: NSWindow?
    private var isEnabled = true
    private var currentMethod = 0  // 0=Telex, 1=VNI

    init() {
        // Load saved settings before UI setup
        loadSettings()

        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)

        if let button = statusItem.button {
            let icon = isEnabled ? "keyboard" : "keyboard.badge.ellipsis"
            button.image = NSImage(systemSymbolName: icon, accessibilityDescription: "GoNhanh")
        }

        setupMenu()

        // Apply settings to Rust engine
        RustBridge.setEnabled(isEnabled)
        RustBridge.setMethod(currentMethod)

        // Start keyboard hook
        KeyboardHookManager.shared.start()
    }

    private func loadSettings() {
        let defaults = UserDefaults.standard

        // Load enabled (default: true)
        if defaults.object(forKey: SettingsKeys.enabled) != nil {
            isEnabled = defaults.bool(forKey: SettingsKeys.enabled)
        }

        // Load method (default: 0=Telex)
        currentMethod = defaults.integer(forKey: SettingsKeys.method)
    }

    private func setupMenu() {
        let menu = NSMenu()

        // Enable/Disable
        let enabledItem = NSMenuItem(
            title: "Bật GoNhanh",
            action: #selector(toggleEnabled),
            keyEquivalent: ""
        )
        enabledItem.target = self
        enabledItem.tag = MenuTag.enabled.rawValue
        enabledItem.state = isEnabled ? .on : .off
        menu.addItem(enabledItem)

        menu.addItem(NSMenuItem.separator())

        // Input method submenu
        let methodMenu = NSMenu()

        let telexItem = NSMenuItem(title: "Telex", action: #selector(setTelex), keyEquivalent: "")
        telexItem.target = self
        telexItem.tag = MenuTag.telex.rawValue
        telexItem.state = currentMethod == 0 ? .on : .off
        methodMenu.addItem(telexItem)

        let vniItem = NSMenuItem(title: "VNI", action: #selector(setVNI), keyEquivalent: "")
        vniItem.target = self
        vniItem.tag = MenuTag.vni.rawValue
        vniItem.state = currentMethod == 1 ? .on : .off
        methodMenu.addItem(vniItem)

        let methodItem = NSMenuItem(title: "Kiểu gõ", action: nil, keyEquivalent: "")
        methodItem.submenu = methodMenu
        menu.addItem(methodItem)

        menu.addItem(NSMenuItem.separator())

        let settingsItem = NSMenuItem(
            title: "Cài đặt...",
            action: #selector(openSettings),
            keyEquivalent: ","
        )
        settingsItem.target = self
        menu.addItem(settingsItem)

        let aboutItem = NSMenuItem(
            title: "Về GoNhanh",
            action: #selector(showAbout),
            keyEquivalent: ""
        )
        aboutItem.target = self
        menu.addItem(aboutItem)

        menu.addItem(NSMenuItem.separator())

        let quitItem = NSMenuItem(
            title: "Thoát",
            action: #selector(quit),
            keyEquivalent: "q"
        )
        quitItem.target = self
        menu.addItem(quitItem)

        statusItem.menu = menu
    }

    @objc func toggleEnabled() {
        isEnabled.toggle()
        RustBridge.setEnabled(isEnabled)
        UserDefaults.standard.set(isEnabled, forKey: SettingsKeys.enabled)

        if let item = statusItem.menu?.item(withTag: MenuTag.enabled.rawValue) {
            item.state = isEnabled ? .on : .off
        }

        // Update icon
        if let button = statusItem.button {
            button.image = NSImage(
                systemSymbolName: isEnabled ? "keyboard" : "keyboard.badge.ellipsis",
                accessibilityDescription: "GoNhanh"
            )
        }
    }

    @objc func setTelex() {
        currentMethod = 0
        RustBridge.setMethod(0)
        UserDefaults.standard.set(0, forKey: SettingsKeys.method)
        updateMethodMenu()
    }

    @objc func setVNI() {
        currentMethod = 1
        RustBridge.setMethod(1)
        UserDefaults.standard.set(1, forKey: SettingsKeys.method)
        updateMethodMenu()
    }

    private func updateMethodMenu() {
        guard let methodItem = statusItem.menu?.item(withTitle: "Kiểu gõ"),
              let methodMenu = methodItem.submenu else { return }

        methodMenu.item(withTag: MenuTag.telex.rawValue)?.state = currentMethod == 0 ? .on : .off
        methodMenu.item(withTag: MenuTag.vni.rawValue)?.state = currentMethod == 1 ? .on : .off
    }

    @objc func openSettings() {
        if settingsWindow == nil {
            let contentView = SettingsView()
            let hostingController = NSHostingController(rootView: contentView)

            settingsWindow = NSWindow(contentViewController: hostingController)
            settingsWindow?.title = "GoNhanh - Cài đặt"
            settingsWindow?.styleMask = [.titled, .closable]
            settingsWindow?.setContentSize(NSSize(width: 400, height: 300))
            settingsWindow?.center()
        }

        settingsWindow?.makeKeyAndOrderFront(nil)
        NSApp.activate(ignoringOtherApps: true)
    }

    @objc func showAbout() {
        let version = Bundle.main.infoDictionary?["CFBundleShortVersionString"] as? String ?? "0.1.0"
        let options: [NSApplication.AboutPanelOptionKey: Any] = [
            .applicationName: "GoNhanh",
            .applicationVersion: version,
            .credits: NSAttributedString(string: "Bộ gõ tiếng Việt hiệu suất cao\n\n🦀 Made with Rust + SwiftUI")
        ]
        NSApp.orderFrontStandardAboutPanel(options: options)
    }

    @objc func quit() {
        NSApp.terminate(nil)
    }
}
