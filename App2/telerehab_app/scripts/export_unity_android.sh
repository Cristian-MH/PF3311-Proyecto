#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
flutter_project="$(cd "$script_dir/.." && pwd)"
unity_project="$(cd "$flutter_project/../../TeleRehabAvatarUnity" && pwd)"
unity_editor="/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity"
android_player="/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/PlaybackEngines/AndroidPlayer"
export_path="$flutter_project/android/unityLibrary"

if [[ ! -x "$unity_editor" ]]; then
  echo "Unity 2022.3.62f3 no está instalado en la ruta esperada: $unity_editor" >&2
  exit 1
fi

if [[ ! -d "$android_player" ]]; then
  cat >&2 <<'EOF'
Falta Android Build Support para Unity 2022.3.62f3.
Abre Unity Hub > Installs > 2022.3.62f3 > Add modules e instala:
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
