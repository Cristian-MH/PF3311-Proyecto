# Local Flutter plugin overrides

`shared_preferences_android` and `flutter_embed_unity_2022_3_android` are
pinned locally until the published packages migrate to Built-in Kotlin. The
only Android build changes remove legacy `kotlin-android` plugin application
and use `kotlin.compilerOptions`, following the Flutter 3.44 migration guide.
Remove each override from `pubspec.yaml` after upgrading to an upstream release
with the same migration.
