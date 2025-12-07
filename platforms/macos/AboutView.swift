import SwiftUI
import AppKit

// MARK: - About View

struct AboutView: View {
    @State private var isHoveringGithub = false
    @State private var isHoveringWebsite = false

    var body: some View {
        VStack(spacing: 16) {
            // App Icon
            Image(systemName: "keyboard.fill")
                .font(.system(size: 50))
                .foregroundColor(.accentColor)
                .padding(.top, 20)

            // App Name & Version
            VStack(spacing: 4) {
                Text(AppMetadata.name)
                    .font(.system(size: 24, weight: .bold))

                Text("Phiên bản \(AppMetadata.version)")
                    .font(.subheadline)
                    .foregroundColor(.secondary)
            }

            // Tagline
            Text(AppMetadata.tagline)
                .font(.body)
                .foregroundColor(.secondary)
                .multilineTextAlignment(.center)

            Divider()
                .padding(.horizontal, 30)

            // Author
            VStack(spacing: 6) {
                Text("Tác giả")
                    .font(.caption)
                    .foregroundColor(.secondary)

                Text(AppMetadata.author)
                    .font(.body)
            }

            // Links
            HStack(spacing: 20) {
                LinkButton(
                    title: "GitHub",
                    icon: "chevron.left.forwardslash.chevron.right",
                    url: AppMetadata.repository
                )

                LinkButton(
                    title: "Website",
                    icon: "globe",
                    url: AppMetadata.website
                )
            }
            .padding(.vertical, 8)

            // Tech Stack
            HStack(spacing: 6) {
                TechBadge(icon: "gearshape.2.fill", text: "Rust")
                TechBadge(icon: "swift", text: "SwiftUI")
            }

            Spacer()

            // Copyright
            VStack(spacing: 4) {
                Text(AppMetadata.copyright)
                    .font(.caption2)
                    .foregroundColor(.tertiaryLabel)

                Text("License: \(AppMetadata.license)")
                    .font(.caption2)
                    .foregroundColor(.tertiaryLabel)
            }
            .padding(.bottom, 16)
        }
        .frame(width: 340, height: 380)
    }
}

// MARK: - Link Button

struct LinkButton: View {
    let title: String
    let icon: String
    let url: String

    @State private var isHovering = false

    var body: some View {
        Button(action: {
            if let url = URL(string: url) {
                NSWorkspace.shared.open(url)
            }
        }) {
            HStack(spacing: 6) {
                Image(systemName: icon)
                    .font(.system(size: 12))
                Text(title)
                    .font(.system(size: 12, weight: .medium))
            }
            .padding(.horizontal, 12)
            .padding(.vertical, 6)
            .background(
                RoundedRectangle(cornerRadius: 6)
                    .fill(isHovering ? Color.accentColor.opacity(0.15) : Color.gray.opacity(0.1))
            )
            .foregroundColor(isHovering ? .accentColor : .primary)
        }
        .buttonStyle(.plain)
        .onHover { hovering in
            isHovering = hovering
        }
    }
}

// MARK: - Tech Badge

struct TechBadge: View {
    let icon: String
    let text: String

    var body: some View {
        HStack(spacing: 4) {
            Image(systemName: icon)
                .font(.system(size: 10))
            Text(text)
                .font(.system(size: 10, weight: .medium))
        }
        .padding(.horizontal, 8)
        .padding(.vertical, 4)
        .background(
            Capsule()
                .fill(Color.gray.opacity(0.15))
        )
        .foregroundColor(.secondary)
    }
}

// MARK: - Preview

struct AboutView_Previews: PreviewProvider {
    static var previews: some View {
        AboutView()
    }
}
