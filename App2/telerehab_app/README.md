# RehaBot

A Flutter-based telerehabilitation application for managing patients, therapies, and therapy logs.

## Project Overview

This project includes:
- `lib/models/` for data models like `patient`, `therapy`, and `therapy_log`
- `lib/screens/` and `lib/services/` for application UI and business logic
- platform-specific folders for Android, iOS, macOS, Linux, web, and Windows targets

## Setup

1. Install Flutter: https://flutter.dev/docs/get-started/install
2. Open the project in your IDE.
3. Fetch dependencies:
   ```bash
   flutter pub get
   ```

## Running the App

Run on a connected device or emulator:

```bash
flutter run
```

To target a specific platform, add a device ID or platform flag:

```bash
flutter run -d chrome
flutter run -d ios
flutter run -d macos
```

## Testing

Run widget tests with:

```bash
flutter test
```

## Notes

- Keep `pubspec.yaml` and `pubspec.lock` under version control for stable dependency resolution.
- Do not commit generated build artifacts from `build/`, `.dart_tool/`, or platform-specific temporary files.

## Resources

- [Flutter documentation](https://docs.flutter.dev/)
- [Flutter packages](https://pub.dev/)
