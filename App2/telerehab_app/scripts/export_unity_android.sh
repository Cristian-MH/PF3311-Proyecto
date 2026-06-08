#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
flutter_project="$(cd "$script_dir/.." && pwd)"
unity_project="$(cd "$flutter_project/../../TeleRehabAvatarUnity" && pwd)"
unity_version="6000.4.0f1"
unity_editor="/Applications/Unity/Hub/Editor/$unity_version/Unity.app/Contents/MacOS/Unity"
android_player="/Applications/Unity/Hub/Editor/$unity_version/PlaybackEngines/AndroidPlayer"
export_path="$flutter_project/android/unityLibrary"

if [[ ! -x "$unity_editor" ]]; then
  echo "Unity $unity_version no está instalado en la ruta esperada: $unity_editor" >&2
  exit 1
fi

if [[ ! -d "$android_player" ]]; then
  cat >&2 <<'EOF'
Falta Android Build Support para Unity $unity_version.
Abre Unity Hub > Installs > $unity_version > Add modules e instala:
- Android Build Support
- Android SDK & NDK Tools
- OpenJDK
EOF
  exit 1
fi

mkdir -p "$export_path"

"$unity_editor" \
  -projectPath "$unity_project" \
  -batchmode \
  -nographics \
  -buildTarget Android \
  -executeMethod TeleRehabUnityExportSetup.ConfigureAndExportAndroid \
  -exportPath "$export_path" \
  -logFile - \
  -quit

for settings_file in "$flutter_project/settings.gradle" "$export_path/settings.gradle"; do
  if [[ -f "$settings_file" ]]; then
    perl -0pi -e "s/^include ':launcher'\\R//m" "$settings_file"
  fi
done

for manifest_file in "$export_path/src/main/AndroidManifest.xml" "$export_path/unityLibrary/src/main/AndroidManifest.xml"; do
  if [[ -f "$manifest_file" ]]; then
    perl -0pi -e 's/\n\s*<intent-filter>\s*<category android:name="android\.intent\.category\.LAUNCHER" \/> \s*<action android:name="android\.intent\.action\.MAIN" \/> \s*<\/intent-filter>//sx' "$manifest_file"
    perl -0pi -e 's/\n\s*<intent-filter>\s*<action android:name="android\.intent\.action\.MAIN" \/> \s*<category android:name="android\.intent\.category\.LAUNCHER" \/> \s*<\/intent-filter>//sx' "$manifest_file"
  fi
done

for proguard_file in "$export_path/proguard-unity.txt" "$export_path/unityLibrary/proguard-unity.txt"; do
  if [[ -f "$proguard_file" ]]; then
    perl -0pi -e 's/^-ignorewarnings\R\R?//m' "$proguard_file"
  fi
done
