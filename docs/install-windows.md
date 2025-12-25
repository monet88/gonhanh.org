# Gõ Nhanh trên Windows

> ✅ **Sẵn sàng** - Phiên bản chính thức cho Windows 10/11

---

## Tính năng

- Gõ tiếng Việt Telex/VNI (Chuẩn Unicode)
- Gõ tắt tùy chỉnh
- Tự động khôi phục tiếng Anh thông minh
- System tray menu
- Tự khởi động cùng Windows
- < 1ms độ trễ gõ phím
- Hoàn toàn offline, không thu thập dữ liệu

---

## Cài đặt

1. Tải về file `.zip` từ [GitHub Releases](https://github.com/khaphanspace/gonhanh.org/releases/latest)
2. Giải nén và chạy file `GoNhanh.exe`
3. Cấp quyền nếu Windows SmartScreen yêu cầu

---

## Quy tắc gõ (Tham khảo)

### Telex

| Gõ | Kết quả |
|----|---------|
| `as`, `af`, `ar`, `ax`, `aj` | á, à, ả, ã, ạ |
| `aa`, `aw`, `ee`, `oo` | â, ă, ê, ô |
| `ow`, `uw`, `dd` | ơ, ư, đ |

### VNI

| Gõ | Kết quả |
|----|---------|
| `a1`, `a2`, `a3`, `a4`, `a5` | á, à, ả, ã, ạ |
| `a6`, `a8`, `o6`, `e6` | â, ă, ô, ê |
| `o7`, `u7`, `d9` | ơ, ư, đ |

---

## Theo dõi

- [Releases](https://github.com/khaphanspace/gonhanh.org/releases)
- [GitHub Issues](https://github.com/khaphanspace/gonhanh.org/issues)

---

## Dành cho Developer

<details>
<summary>Build từ source</summary>

**Yêu cầu:**
- Windows 10/11
- [Rust](https://rustup.rs/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (C++ & .NET workload)

```powershell
git clone https://github.com/khaphanspace/gonhanh.org.git
cd gonhanh.org/platforms/windows
cargo build --release
```
</details>
