# Local Flutter plugin overrides

`shared_preferences_android` is pinned locally until the published package
migrates to Built-in Kotlin. The only Android build change removes the legacy
`kotlin-android` plugin application, following the Flutter 3.44 migration
guide. Remove the override from `pubspec.yaml` after upgrading to an upstream
release with the same migration.
