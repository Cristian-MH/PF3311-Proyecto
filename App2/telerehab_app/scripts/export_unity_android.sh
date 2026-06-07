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
